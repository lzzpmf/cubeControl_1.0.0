using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Cube.Desktop.Localization;
using Cube.Desktop.Studio.Controls;
using Cube.Desktop.ViewModels;
using Cube.Studio.Audio;
using Cube.Studio.Core;

namespace Cube.Desktop.Studio.ViewModels;

/// <summary>An application-owned, deliberately non-persistent practice session.</summary>
public sealed partial class ShellViewModel : ViewModelBase, IDisposable, IStudioPeakProvider
{
    private readonly List<RelayCommand> _commands = [];
    private readonly DispatcherTimer _timer;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly IStudioAudioEngine _audio;
    private readonly string _dataDirectory, _runDirectory;
    private bool _disposed, _pageVisible, _operationBusy, _transportBusy, _closePending, _renderDirty = true, _projecting, _capturePending;
    private bool _metronomeOptionsOpen, _audioOptionsOpen, _preserveRecordings, _discardApproved;
    private string _statusMessage = "Pronto", _tapStatus = "TAP · definir andamento";
    private readonly Stopwatch _tapClock = Stopwatch.StartNew();
    private readonly List<double> _taps = [];
    private double _operationProgress, _snapshotRetryAfter;
    private CancellationTokenSource? _operation;
    private Task _snapshotWorker = Task.CompletedTask;
    private readonly SemaphoreSlim _snapshotGate = new(1, 1);
    private long _sessionGeneration, _backingIntent;
    private StudioTransportSnapshot _state = new(false, false, 0, 0);
    private bool _metronomeEnabled;
    private double _metronomeBpm = 100, _metronomeVolumeDb = -12;
    private string _meter = "4/4", _metronomeSound = "Click";

    public ShellViewModel(string dataDirectory, BackingTracksViewModel downloader, IStudioAudioEngine? engine = null)
    {
        _dataDirectory = Path.GetFullPath(dataDirectory);
        _runDirectory = Path.Combine(_dataDirectory, "PracticeRecordings", Guid.NewGuid().ToString("N"));
        Downloader = downloader;
        _audio = engine ?? new StudioAudioEngine(dataDirectory);
        Editor = new PracticeEditor();
        Project = new ProjectModel { Duration = 30, SnapEnabled = false };
        Project.Tracks.Add(new TrackModel { Name = T("Acompanhamento") });
        Project.Tracks.Add(new TrackModel { Name = T("Minha guitarra") });
        foreach (var track in Project.Tracks) track.PropertyChanged += TrackChanged;
        Session.PropertyChanged += PracticeChanged;
        Editor.PropertyChanged += PracticeChanged;
        Downloader.PropertyChanged += DownloaderChanged;
        Downloader.PresentedYouTubeResults.CollectionChanged += ResultsChanged;
        LanguageService.Instance.PropertyChanged += LanguageChanged;

        BackCommand = Command(() => Seek(Project.Position - 5), () => CanEditTimeline);
        ForwardCommand = Command(() => Seek(Project.Position + 5), () => CanEditTimeline);
        PlayPauseCommand = Command(() => Run(PlayPauseAsync), () => CanTransport && (IsRecording || HasAudio));
        StopCommand = Command(() => Run(StopAsync), () => !_transportBusy);
        RecordCommand = Command(() => Run(ToggleRecordingAsync), () => CanTransport);
        ToggleLoopCommand = Command(() => { if (!Session.LoopEnabled && Session.LoopEnd <= Session.LoopStart) SetLoop(0, Project.Duration); else { Session.LoopEnabled = !Session.LoopEnabled; SyncPresentation(); } }, () => CanEditTimeline);
        SetLoopStartCommand = Command(() => SetLoop(Project.Position, Math.Max(Project.Position + .15, Session.LoopEnd)), () => CanEditTimeline);
        SetLoopEndCommand = Command(() => SetLoop(Math.Min(Session.LoopStart, Math.Max(0, Project.Position - .15)), Project.Position), () => CanEditTimeline);
        ToggleMetronomeCommand = Command(() => MetronomeEnabled = !MetronomeEnabled, () => !_closePending && !IsBusy);
        TapTempoCommand = Command(TapTempo, () => !_closePending && !IsBusy);
        ClearStudyCommand = Command(() => Run(ClearStudyAsync), () => CanEditTimeline);
        ImportCommand = Command(ImportFiles, () => CanReplaceBacking);
        SaveAudioCommand = Command(() => Run(ExportAsync), () => CanEditTimeline && SelectedAttempt != null);
        UndoCommand = Command(() => { Editor.Undo(); SyncPresentation(); }, () => CanUndo);
        RedoCommand = Command(() => { Editor.Redo(); SyncPresentation(); }, () => CanRedo);
        DeleteAttemptCommand = Command(() => { if (SelectedAttempt is { } a) Editor.DeleteAttempt(a.Id); SyncPresentation(); }, () => CanEditTimeline && SelectedAttempt != null);
        ListenAttemptCommand = Command(() => Run(ListenAttemptAsync), () => CanEditTimeline && SelectedAttempt != null);
        UseBackingCommand = Command(p => { if (p is PlaybackResultViewModel result) Run(() => AddPlaybackAsync(result)); }, p => CanReplaceBacking && p is PlaybackResultViewModel);
        DownloadPlaybackCommand = Command(p => { if (p is PlaybackResultViewModel result) Run(() => Downloader.DownloadYouTube(result.Video)); }, p => p is PlaybackResultViewModel && !Downloader.YouTubeDownloading);
        TogglePreviewCommand = Command(p => { if (p is PlaybackResultViewModel result) TogglePreview(result); }, p => CanPreviewPlayback && p is PlaybackResultViewModel);
        PitchDownCommand = Command(() => { Session.Semitones--; }, () => IsTempoEditable && Session.Semitones > -12);
        PitchUpCommand = Command(() => { Session.Semitones++; }, () => IsTempoEditable && Session.Semitones < 12);
        ResetPitchCommand = Command(() => Session.Semitones = 0, () => IsTempoEditable);
        ResetSpeedCommand = Command(() => SpeedPercent = 100, () => IsTempoEditable);
        SearchCommand = Command(() => Downloader.RefreshSearch());
        CancelOperationCommand = Command(() => _operation?.Cancel(), () => _operation != null);
        ApplyAudioSettingsCommand = Command(() => Run(() => ExecuteTransportAsync(ApplyAudioSettingsAsync)), () => CanChangeAudioSettings);
        RefreshAudioDevicesCommand = Command(() => Run(RefreshAudioDevicesAsync), () => CanChangeAudioSettings);
        AsioControlPanelCommand = Command(() => Run(() => ExecuteTransportAsync(OpenAsioPanelAsync)), () => IsAsio && CanChangeAudioSettings);
        OpenStudioCommand = Command(() => NavigateToStudio?.Invoke());
        RecoverRecordingsCommand = Command(() => Run(RecoverRecordingsAsync), () => CanEditTimeline);
        HelpCommand = Command(() => Run(() => ShowMessageAsync("Atalhos do Studio", "Espaço: reproduzir/pausar · R: gravar · M: metrônomo · L: repetir A/B · Delete: excluir tentativa · Ctrl+Z/Y: desfazer/refazer · Ctrl+I: importar · Ctrl+0 na timeline: enquadrar.")));
        SyncPresentation();
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += Tick; _timer.Start();
    }

    public PracticeEditor Editor { get; }
    public PracticeSession Session => Editor.Session;
    // Rendering projection only. No project store or project commands participate in this flow.
    public ProjectModel Project { get; }
    public ObservableCollection<PracticeAttempt> Attempts => Session.Attempts;
    public PracticeAttempt? SelectedAttempt { get => Session.SelectedAttempt; set { if (CanEditTimeline) { Editor.SelectAttempt(value?.Id); SyncPresentation(); } } }
    public BackingTracksViewModel Downloader { get; }
    public IStudioPeakProvider PeakProvider => this;
    public Func<Task>? BeforePlayback { get; set; }
    public Action? NavigateToStudio { get; set; }
    public Wpf.Ui.Controls.ContentDialogHost? DialogHost { get; set; }
    public Window? Owner { get; set; }
    public bool IsPlaying => _state.IsPlaying;
    public bool IsRecording => _state.IsRecording || _capturePending;
    public bool IsStopped => !IsPlaying;
    public bool IsBusy => _operationBusy;
    private bool CanTransport => !_transportBusy && !IsBusy && !_closePending;
    public bool CanEditTimeline => !IsRecording && CanTransport;
    public bool CanReplaceBacking => !IsRecording && !IsBusy && !_transportBusy && !_closePending;
    public bool CanChangeAudioSettings => CanEditTimeline;
    internal bool ClosePending { get => _closePending; set { if (Set(ref _closePending, value)) RefreshTransport(); } }
    public bool IsTempoEditable => CanEditTimeline;
    public bool HasAudio => Session.Backing != null || SelectedAttempt != null;
    public bool CanUndo => Editor.CanUndo && CanEditTimeline;
    public bool CanRedo => Editor.CanRedo && CanEditTimeline;
    public bool CanPreviewPlayback => CanEditTimeline;
    public bool CanAuditionIr => CanEditTimeline;
    public bool PageVisible { get => _pageVisible; set { if (Set(ref _pageVisible, value)) { Downloader.PageVisible = value; if (!value) StopPreview(); OnPropertyChanged(nameof(ShowMiniPlayer)); } } }
    public bool ShowMiniPlayer => !PageVisible && (IsPlaying || IsRecording || _state.IsMetronomeRunning);
    public string ProjectTitle => Session.Backing?.Name ?? T("Cube Studio");
    public string TimeDisplay => FormatPosition(Project.Position);
    public string TransportStatus => T(IsRecording ? "Gravando" : IsPlaying ? "Reproduzindo" : _state.IsMetronomeRunning ? "Metrônomo" : "Parado");
    public string PlayToolTip => T(IsPlaying ? "Pausar · Espaço" : "Reproduzir · Espaço");
    public string TransposeText => (Session.Semitones > 0 ? "+" : "") + Session.Semitones + " st";
    public double SpeedPercent { get => Session.SpeedPercent; set { if (IsTempoEditable) Session.SpeedPercent = value; } }
    public string SpeedText => SpeedPercent.ToString("0.#", CultureInfo.CurrentCulture) + "%";
    public double MinSpeedPercent => 25;
    public double MaxSpeedPercent => 200;
    public bool MetronomeEnabled { get => _metronomeEnabled; set { if (Set(ref _metronomeEnabled, value)) Run(() => UpdateMetronomeAsync()); } }
    public double MetronomeBpm { get => _metronomeBpm; set { if (Set(ref _metronomeBpm, Math.Clamp(double.IsFinite(value) ? value : 100, 30, 300))) Run(() => UpdateMetronomeAsync()); } }
    public double MetronomeVolumeDb { get => _metronomeVolumeDb; set { if (Set(ref _metronomeVolumeDb, Math.Clamp(double.IsFinite(value) ? value : -12, -61, 6))) Run(() => UpdateMetronomeAsync()); } }
    public string Meter { get => _meter; set { if (Meters.Contains(value) && Set(ref _meter, value)) Run(() => UpdateMetronomeAsync()); } }
    public string MetronomeSound { get => _metronomeSound; set { if (MetronomeSounds.Contains(value) && Set(ref _metronomeSound, value)) Run(() => UpdateMetronomeAsync()); } }
    public string MetronomeToolTip => T("Metrônomo independente da velocidade da backing");
    public bool MetronomePulse => _state.IsMetronomePulse;
    public string StatusMessage { get => T(_statusMessage); private set { _statusMessage = value; OnPropertyChanged(); } }
    public string TapStatus { get => T(_tapStatus); private set { _tapStatus = value; OnPropertyChanged(); } }
    public double OperationProgress { get => _operationProgress; private set => Set(ref _operationProgress, value); }
    public IReadOnlyList<string> Meters => ProjectModel.AvailableMeters;
    public IReadOnlyList<string> MetronomeSounds => ProjectModel.AvailableMetronomeSounds;
    public bool IsMetronomeOptionsOpen { get => _metronomeOptionsOpen; set { if (value) IsAudioOptionsOpen = false; Set(ref _metronomeOptionsOpen, value); } }
    public bool IsAudioOptionsOpen { get => _audioOptionsOpen; set { if (value) IsMetronomeOptionsOpen = false; Set(ref _audioOptionsOpen, value); } }

    public RelayCommand BackCommand { get; }
    public RelayCommand ForwardCommand { get; }
    public RelayCommand PlayPauseCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand RecordCommand { get; }
    public RelayCommand ToggleLoopCommand { get; }
    public RelayCommand SetLoopStartCommand { get; }
    public RelayCommand SetLoopEndCommand { get; }
    public RelayCommand ToggleMetronomeCommand { get; }
    public RelayCommand TapTempoCommand { get; }
    public RelayCommand ClearStudyCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand SaveAudioCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand RedoCommand { get; }
    public RelayCommand DeleteAttemptCommand { get; }
    public RelayCommand ListenAttemptCommand { get; }
    public RelayCommand UseBackingCommand { get; }
    public RelayCommand AddPlaybackCommand => UseBackingCommand;
    public RelayCommand DownloadPlaybackCommand { get; }
    public RelayCommand TogglePreviewCommand { get; }
    public RelayCommand PitchDownCommand { get; }
    public RelayCommand PitchUpCommand { get; }
    public RelayCommand ResetPitchCommand { get; }
    public RelayCommand ResetSpeedCommand { get; }
    public RelayCommand SearchCommand { get; }
    public RelayCommand CancelOperationCommand { get; }
    public RelayCommand ApplyAudioSettingsCommand { get; }
    public RelayCommand RefreshAudioDevicesCommand { get; }
    public RelayCommand AsioControlPanelCommand { get; }
    public RelayCommand HelpCommand { get; }
    public RelayCommand OpenStudioCommand { get; }
    public RelayCommand RecoverRecordingsCommand { get; }

    private RelayCommand Command(Action action, Func<bool>? can = null) { var command = new RelayCommand(action, can); _commands.Add(command); return command; }
    private RelayCommand Command(Action<object?> action, Predicate<object?>? can = null) { var command = new RelayCommand(action, can); _commands.Add(command); return command; }
    private async void Run(Func<Task> action)
    {
        try { await action(); }
        catch (OperationCanceledException) { if (!_disposed) StatusMessage = "Operação cancelada"; }
        catch (Exception ex) { if (!_disposed) StatusMessage = ex.Message; Debug.WriteLine(ex); }
        finally { if (!_disposed) RefreshTransport(); }
    }
    public void SetTempoFromUi(double bpm) { _taps.Clear(); MetronomeBpm = bpm; }
    public void SetSpeedFromUi(double percent) => SpeedPercent = percent;
    public void SetLoop(double start, double end)
    {
        if (!CanEditTimeline) return;
        Session.LoopStart = Math.Clamp(start, 0, Math.Max(0, Project.Duration - .15));
        Session.LoopEnd = Math.Clamp(end, Session.LoopStart + .15, Project.Duration);
        Session.LoopEnabled = true; SyncPresentation();
    }
    public void TrimSelectedAttempt(double start, double duration)
    {
        if (!CanEditTimeline || SelectedAttempt is not { } attempt) return;
        double framesPerSecond = attempt.SourceSampleRate / (attempt.RecordedSpeed * attempt.ClockRatio);
        long first = Math.Clamp((long)Math.Round((start - attempt.Start) * framesPerSecond), 0, attempt.SourceFrameCount - 1);
        long last = Math.Clamp((long)Math.Round((start + duration - attempt.Start) * framesPerSecond), first + 1, attempt.SourceFrameCount);
        Editor.TrimAttempt(attempt.Id, first, last); SyncPresentation();
    }
    private void TapTempo()
    {
        double now = _tapClock.Elapsed.TotalMilliseconds;
        if (_taps.Count > 0 && now - _taps[^1] > 2500) _taps.Clear();
        _taps.Add(now); if (_taps.Count > 9) _taps.RemoveAt(0);
        if (_taps.Count >= 2)
        {
            var intervals = _taps.Skip(1).Select((t, i) => t - _taps[i]).Order().ToArray();
            double median = intervals.Length % 2 == 1 ? intervals[intervals.Length / 2] : (intervals[intervals.Length / 2 - 1] + intervals[intervals.Length / 2]) / 2;
            _metronomeBpm = Math.Clamp(60000 / Math.Max(1, median), 30, 300); OnPropertyChanged(nameof(MetronomeBpm));
        }
        Run(() => UpdateMetronomeAsync(true));
        TapStatus = _taps.Count < 2 ? "TAP · toque novamente" : $"TAP · {_taps.Count} · {MetronomeBpm:0.#} BPM";
    }
    public void ClosePopups() { IsMetronomeOptionsOpen = IsAudioOptionsOpen = false; }
    private void PracticeChanged(object? sender, PropertyChangedEventArgs e) { if (!_projecting) SyncPresentation(); }
    private void TrackChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_projecting || e.PropertyName is not (nameof(TrackModel.VolumeDb) or nameof(TrackModel.IsMuted))) return;
        var track = (TrackModel)sender!;
        var mix = track == Project.Tracks[0] ? Session.BackingMix : Session.AttemptMix;
        mix.VolumeDb = track.VolumeDb; mix.Muted = track.IsMuted; QueueSnapshot();
    }
    private void SyncPresentation()
    {
        _projecting = true;
        try
        {
            Project.Duration = Math.Max(Math.Max(30, Session.Duration), IsRecording ? _state.Duration : 0);
            Project.Position = Session.Position;
            Project.Semitones = Session.Semitones;
            Project.LoopStart = Session.LoopStart; Project.LoopEnd = Session.LoopEnd; Project.LoopEnabled = Session.LoopEnabled;
            var sources = new PracticeSource?[] { Session.Backing, IsRecording ? null : SelectedAttempt };
            for (int i = 0; i < 2; i++)
            {
                var track = Project.Tracks[i]; var source = sources[i];
                track.Clips.Clear();
                if (source != null) track.Clips.Add(ProjectClip(source));
                var mix = i == 0 ? Session.BackingMix : Session.AttemptMix;
                track.VolumeDb = mix.VolumeDb; track.IsMuted = mix.Muted;
            }
        }
        finally { _projecting = false; }
        QueueSnapshot(); PeaksChanged?.Invoke(this, EventArgs.Empty);
        foreach (var name in new[] { nameof(ProjectTitle), nameof(TimeDisplay), nameof(SelectedAttempt), nameof(Attempts), nameof(TransposeText), nameof(SpeedPercent), nameof(SpeedText), nameof(HasAudio) }) OnPropertyChanged(name);
        RefreshTransport();
    }
    private static ClipModel ProjectClip(PracticeSource source)
    {
        var clip = new ClipModel { Id = source.Id, Name = source.Name, Start = source.AudibleStart, Duration = source.Duration,
            SourceOffset = source.TrimStartFrame / (double)source.SourceSampleRate * source.RecordedSpeed * source.ClockRatio,
            SourceDuration = source.AudioDuration * source.RecordedSpeed * source.ClockRatio, IsSelected = source is PracticeAttempt };
        source.CopySourceTo(clip); return clip;
    }
    private void RefreshTransport()
    {
        foreach (var name in new[] { nameof(IsPlaying), nameof(IsRecording), nameof(IsStopped), nameof(CanEditTimeline), nameof(CanReplaceBacking), nameof(CanChangeAudioSettings), nameof(IsTempoEditable), nameof(TransportStatus), nameof(PlayToolTip), nameof(CanUndo), nameof(CanRedo), nameof(CanPreviewPlayback), nameof(CanAuditionIr), nameof(ShowMiniPlayer), nameof(IsBusy) }) OnPropertyChanged(name);
        foreach (var command in _commands) command.RaiseCanExecuteChanged();
    }
    private void LanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        Project.Tracks[0].Name = T("Acompanhamento"); Project.Tracks[1].Name = T("Minha guitarra");
        SyncPresentation(); OnPropertyChanged(nameof(StatusMessage)); OnPropertyChanged(nameof(TapStatus)); OnPropertyChanged(nameof(AudioSummary)); RefreshYouTubeResults();
    }
    private static string T(string text) => LanguageService.T(text);
    private static string FormatPosition(double seconds) { long total = Math.Max(0, (long)Math.Round(seconds * 1000)); return $"{total / 60000:00}:{total / 1000 % 60:00}.{total % 1000:000}"; }
    public void Dispose()
    {
        if (_disposed) return; _disposed = true;
        _timer.Stop(); _timer.Tick -= Tick; _lifetime.Cancel(); _operation?.Cancel(); _backingPreparation?.Cancel(); StopPreview();
        Session.PropertyChanged -= PracticeChanged; Editor.PropertyChanged -= PracticeChanged;
        foreach (var track in Project.Tracks) track.PropertyChanged -= TrackChanged;
        Downloader.PropertyChanged -= DownloaderChanged; Downloader.PresentedYouTubeResults.CollectionChanged -= ResultsChanged;
        LanguageService.Instance.PropertyChanged -= LanguageChanged;
        if (IsRecording) _preserveRecordings = true;
        _audio.Dispose();
        if (_discardApproved && !_preserveRecordings) CleanupOwnedRecordings();
        _lifetime.Dispose();
    }
}
