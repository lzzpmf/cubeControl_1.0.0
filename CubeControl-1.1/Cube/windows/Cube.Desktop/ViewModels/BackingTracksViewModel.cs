using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace Cube.Desktop.ViewModels;

public sealed class BackingTrack(string source) : Observable
{
    public string Source { get; } = source;
    public string Title => Path.GetFileNameWithoutExtension(Source);
    private double length;
    private string state = File.Exists(source) ? "Pronta" : "Arquivo ausente";
    public double Length { get => length; set { if (Set(ref length, value)) { Notify(nameof(LengthLabel)); Notify(nameof(MetadataLabel)); } } }
    public string LengthLabel => Length > 0 ? BackingTracksViewModel.FormatTime(Length) : "—";
    public string State { get => state; set => Set(ref state, value); }
    private string tuning = "E Standard";
    public string Tuning { get => tuning; set => Set(ref tuning, value); }
    private double bpm;
    internal bool ManualBpm, ManualKey, MusicPending;
    internal double TempoConfidence, KeyConfidence;
    internal void RefreshMusic() => Notify(nameof(MetadataLabel));
    private string originalKey = "Não definido";
    public double Bpm { get => bpm; set { if (Set(ref bpm, value)) Notify(nameof(MetadataLabel)); } }
    public string OriginalKey { get => originalKey; set { if (Set(ref originalKey, value)) Notify(nameof(MetadataLabel)); } }
    public string MetadataLabel => (MusicPending && Bpm <= 0 && !ManualKey ? LanguageService.T("Analisando…") : (Bpm > 0 ? $"{(ManualBpm ? "" : "≈")}{Bpm:0.#} BPM" : LanguageService.T("BPM não identificado")) + " · " + (OriginalKey == "Não definido" ? LanguageService.T("Tom não identificado") : OriginalKey)) + " · " + LengthLabel;
    public int Preset { get; set; }
    public ObservableCollection<BackingSection> Sections { get; } = [];
    public double? Start { get; set; }
    public double? End { get; set; }
    public double Speed { get; set; } = 100;
    public double Pitch { get; set; }
    public long FileSize { get; set; }
    public long LastWriteTicks { get; set; }
    public float[] Peaks { get; set; } = [];
    public long LastUsed { get; set; }
}
public sealed record BackingOutput(string Id, string Name);

public sealed partial class BackingTracksViewModel : Observable, IDisposable
{
    private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
    private readonly BackingAudioEngine engine = new();
    private readonly DispatcherTimer positionTimer = new() { Interval = TimeSpan.FromMilliseconds(40) };
    private readonly DispatcherTimer parameterTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private readonly CancellationTokenSource lifetime = new();
    private readonly HashSet<Task> backgroundWork = [];
    private DeviceWatcher? watcher;
    private CancellationTokenSource? waveformCancellation, exportCancellation;
    private BackingTrack? selectedTrack;
    private BackingOutput? selectedOutput;
    private string? preferredOutputId;
    private string status = "", analysisStatus = "";
    private double volume = 30, position, duration;
    private bool muted, looping, exporting, refreshing, restoring, disposed, continueSequence, hasError, pageVisible, opening, parameterGesture;
    private int selectionGeneration;
    private long cacheUse;
    private BackingPlaybackState state = BackingPlaybackState.Empty;
    public ObservableCollection<BackingTrack> Tracks { get; } = [];
    public ObservableCollection<BackingOutput> Outputs { get; } = [];
    public Func<Task<bool?>>? ChooseExportMode { get; set; }
    public ICommand AddFiles { get; }
    public ICommand RemoveTrack { get; }
    public Func<Task>? BeforePlayback { get; set; }
    public async Task StopForIrAudition()
    {
        selectionGeneration++; opening = false;
        await StopYouTubePreviewAndWait();
        UpdatePosition(); await engine.ReleaseOutput();
        positionTimer.Stop(); StopMonitor();
        State = SelectedTrack == null ? BackingPlaybackState.Empty : BackingPlaybackState.Paused;
        if (SelectedTrack != null) SelectedTrack.State = "Pausada";
    }
    public ICommand PlayPause { get; }
    public ICommand Stop { get; }
    public ICommand RefreshOutputs { get; }
    public ICommand BluetoothSettings { get; }
    public ICommand MoveUp { get; }
    public ICommand MoveDown { get; }
    public ICommand Previous { get; }
    public ICommand Next { get; }
    public ICommand SkipBack { get; }
    public ICommand SkipForward { get; }
    public ICommand MarkStart { get; }
    public ICommand MarkEnd { get; }
    public ICommand ResetRange { get; }
    public ICommand ClearStart { get; }
    public ICommand ClearEnd { get; }
    public ICommand ResetSpeed { get; }
    public ICommand ResetPitch { get; }
    public ICommand ExportRange { get; }
    public ICommand CancelExport { get; }
    public ICommand ShowFile { get; }
    public ICommand LocateFile { get; }
    public BackingPlaybackState State { get => state; private set { Set(ref state, value); RefreshPresentation(); } }
    public bool IsPlaying => State == BackingPlaybackState.Playing;
    public bool IsLoading => opening || State == BackingPlaybackState.Loading;
    public bool HasActivePlayback => engine.Snapshot.Source == SelectedTrack?.Source && State is BackingPlaybackState.Playing or BackingPlaybackState.Paused or BackingPlaybackState.Ended;
    public bool PageVisible { get => pageVisible; set { if (Set(ref pageVisible, value)) { if (!value) StopYouTubePreview(); Notify(nameof(ShowMiniPlayer)); } } }
    public bool ShowMiniPlayer => !PageVisible && (HasActivePlayback || HasError && SelectedTrack != null);
    public bool HasTracks => Tracks.Count > 0;
    public bool HasRange => SelectedTrack?.Start is double a && SelectedTrack.End is double b && b - a >= .1 - 1e-9;
    public bool CanSeek => SelectedTrack != null && Duration > 0;
    public bool CanLoop => HasRange && Duration >= .1;
    public bool IsExporting { get => exporting; private set => Set(ref exporting, value); }
    public bool HasError { get => hasError; private set { Set(ref hasError, value); Notify(nameof(ShowMiniPlayer)); Notify(nameof(StatusSeverity)); } }
    public Wpf.Ui.Controls.InfoBarSeverity StatusSeverity => HasError ? Wpf.Ui.Controls.InfoBarSeverity.Error : Wpf.Ui.Controls.InfoBarSeverity.Informational;
    public bool HasModifiedSpeed => Speed != 100;
    public bool HasModifiedPitch => Pitch != 0;
    public bool HasStatus => Status.Length > 0;
    public string Status { get => status; private set { Set(ref status, value); Notify(nameof(HasStatus)); } }
    public string AnalysisStatus { get => analysisStatus; private set => Set(ref analysisStatus, value); }
    public string OutputName => SelectedOutput?.Name ?? "Escolha a saída de áudio";
    public string OutputState => SelectedOutput == null ? "Indisponível" : engine.Snapshot.OutputId == SelectedOutput.Id && HasActivePlayback ? "Em uso" : "Selecionada";
    public string PlayLabel => IsPlaying ? "Pausar" : "Reproduzir";
    public SymbolRegular PlayIcon => IsPlaying ? SymbolRegular.Pause24 : SymbolRegular.Play24;
    public SymbolRegular MuteIcon => IsMuted ? SymbolRegular.SpeakerMute24 : SymbolRegular.Speaker224;
    public string MuteLabel => IsMuted ? "Restaurar som" : "Silenciar";
    public float[] Peaks => SelectedTrack?.Peaks ?? [];
    public string ExportLabel => HasRange ? "Exportar A → B" : "Exportar faixa WAV";
    public double Duration { get => duration; private set { Set(ref duration, value); Notify(nameof(CanSeek)); Notify(nameof(CanLoop)); Notify(nameof(TimeLabel)); } }
    public string TimeLabel => $"{FormatTime(Position, true)} / {FormatTime(Duration)}";
    public string RangeLabel => $"A {(double.IsNaN(RangeStart) ? "—" : FormatTime(RangeStart, true))}   B {(double.IsNaN(RangeEnd) ? "—" : FormatTime(RangeEnd, true))}" + (HasRange ? $"   {RangeEnd - RangeStart:0.0} s" : "");
    public static string FormatTime(double seconds, bool precise = false)
    {
        var time = TimeSpan.FromSeconds(double.IsFinite(seconds) ? Math.Max(0, precise ? Math.Round(seconds, 1) : seconds) : 0);
        return time.ToString(time.TotalHours >= 1 ? (precise ? @"hh\:mm\:ss\.f" : @"hh\:mm\:ss") : (precise ? @"mm\:ss\.f" : @"mm\:ss"));
    }
    public BackingTracksViewModel(string dataDirectory)
    {
        InitializePractice();
        InitializeYouTubeDownloads(dataDirectory);
        AddFiles = new UiCommand(() => Run(() => { var dialog = new OpenFileDialog { Multiselect = true, Filter = LanguageService.T("Áudio MP3 ou WAV") + "|*.mp3;*.wav" }; if (dialog.ShowDialog() == true) AddPaths(dialog.FileNames); }));
        RemoveTrack = new UiCommand(RemoveSelected, () => SelectedTrack != null);
        PlayPause = new UiCommand(() => Start(TogglePlayback()), () => !IsLoading && SelectedTrack != null && SelectedOutput != null);
        Stop = new UiCommand(() => Start(StopPlayback()), () => SelectedTrack != null);
        RefreshOutputs = new UiCommand(() => Start(RefreshDevices()), () => !refreshing);
        BluetoothSettings = new UiCommand(() => Run(() => Process.Start(new ProcessStartInfo("ms-settings:bluetooth") { UseShellExecute = true })));
        MoveUp = new UiCommand(() => MoveSelected(-1), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) > 0);
        MoveDown = new UiCommand(() => MoveSelected(1), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) < Tracks.Count - 1);
        Previous = new UiCommand(() => Start(SelectAdjacent(-1)), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) > 0);
        Next = new UiCommand(() => Start(SelectAdjacent(1)), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) < Tracks.Count - 1);
        SkipBack = new UiCommand(() => Position -= 5, () => CanSeek); SkipForward = new UiCommand(() => Position += 5, () => CanSeek);
        MarkStart = new UiCommand(() => RangeStart = Position, () => CanSeek && Duration >= .1);
        MarkEnd = new UiCommand(() => RangeEnd = Position, () => CanSeek && Duration >= .1);
        ResetRange = new UiCommand(() => ClearRange(true, true), () => SelectedTrack?.Start != null || SelectedTrack?.End != null);
        ClearStart = new UiCommand(() => ClearRange(true, false)); ClearEnd = new UiCommand(() => ClearRange(false, true));
        ResetSpeed = new UiCommand(() => Speed = 100, () => SelectedTrack != null); ResetPitch = new UiCommand(() => Pitch = 0, () => SelectedTrack != null);
        ExportRange = new UiCommand(() => Start(ExportSelection()), () => SelectedTrack != null && Duration > 0 && !exporting);
        CancelExport = new UiCommand(() => exportCancellation?.Cancel(), () => IsExporting);
        ShowFile = new UiCommand(() => Run(() => { if (SelectedTrack != null) Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + SelectedTrack.Source + "\"") { UseShellExecute = true }); }), () => SelectedTrack != null);
        LocateFile = new UiCommand(LocateSelected, () => SelectedTrack != null);
        positionTimer.Tick += (_, _) => UpdatePosition();
        parameterTimer.Tick += (_, _) => { parameterTimer.Stop(); if (!parameterGesture) Start(ApplyParameters()); };
        LanguageService.Instance.PropertyChanged += MusicLanguageChanged;
        engine.Changed += EngineChanged; Tracks.CollectionChanged += (_, _) => { Notify(nameof(HasTracks)); ScheduleMusic(); };

    }
    public BackingTrack? SelectedTrack
    {
        get => selectedTrack;
        set
        {
            if (selectedTrack == value) return;
            if (selectedTrack != null) selectedTrack.State = File.Exists(selectedTrack.Source) ? "Pronta" : "Arquivo ausente";
            selectedTrack = value; selectionGeneration++; opening = false; looping = false;
            parameterTimer.Stop(); positionTimer.Stop(); Start(engine.Clear()); position = 0;
            Duration = value?.Length ?? 0; State = value == null ? BackingPlaybackState.Empty : BackingPlaybackState.Ready;
            Notify(); Notify(nameof(Peaks)); Notify(nameof(Position)); Notify(nameof(Speed)); Notify(nameof(Pitch)); Notify(nameof(HasModifiedSpeed)); Notify(nameof(HasModifiedPitch)); RangeChanged(false);
            RefreshPractice();
            Status = ""; HasError = false; Changed(); Start(LoadWaveform()); ScheduleMusic();
        }
    }
    public BackingOutput? SelectedOutput
    {
        get => selectedOutput;
        set
        {
            if (!Set(ref selectedOutput, value)) return;
            StopYouTubePreview(); StopMonitor(); UpdatePosition(); Start(engine.ReleaseOutput()); positionTimer.Stop();
            if (HasActivePlayback) State = BackingPlaybackState.Paused;
            if (!restoring) { preferredOutputId = value?.Id; Changed(); }
            Notify(nameof(OutputName)); Notify(nameof(OutputState)); CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool ContinueSequence { get => continueSequence; set { if (Set(ref continueSequence, value)) Changed(); } }
    public bool IsLooping { get => looping; set { if (value && !CanLoop) return; if (Set(ref looping, value)) { if (value && (Position < RangeStart || Position >= RangeEnd)) position = RangeStart; Start(ApplyParameters()); Notify(nameof(Position)); Notify(nameof(TimeLabel)); } } }
    public double Speed { get => SelectedTrack?.Speed ?? 100; set { if (SelectedTrack == null || !double.IsFinite(value)) return; SelectedTrack.Speed = Math.Clamp(Math.Round(value), 50, 150); Notify(); Notify(nameof(HasModifiedSpeed)); Notify(nameof(CurrentBpm)); Notify(nameof(BpmBadge)); ScheduleParameters(); Changed(); } }
    public double Pitch { get => SelectedTrack?.Pitch ?? 0; set { if (SelectedTrack == null || !double.IsFinite(value)) return; SelectedTrack.Pitch = Math.Clamp(Math.Round(value), -6, 6); Notify(); Notify(nameof(HasModifiedPitch)); Notify(nameof(CurrentKey)); Notify(nameof(KeyBadge)); ScheduleParameters(); Changed(); } }
    public double Volume { get => volume; set { if (double.IsFinite(value) && Set(ref volume, Math.Clamp(value, 0, 100))) { UpdateYouTubePreviewVolume(); Start(engine.SetVolume(volume / 100, IsMuted)); Changed(); } } }
    public bool IsMuted { get => muted; set { if (Set(ref muted, value)) { UpdateYouTubePreviewVolume(); Start(engine.SetVolume(Volume / 100, value)); Notify(nameof(MuteIcon)); Notify(nameof(MuteLabel)); Changed(); } } }
    public double RangeStart
    {
        get => SelectedTrack?.Start ?? double.NaN;
        set
        {
            if (SelectedTrack == null || !double.IsFinite(value) || Duration < .1) return;
            value = Math.Clamp(value, 0, Duration - .1);
            if (SelectedTrack.End is double b && b - value < .1 - 1e-9) { Status = "A deve ficar antes de B, com pelo menos 0,1 s."; return; }
            SelectedTrack.Start = value; RangeChanged();
        }
    }
    public double RangeEnd
    {
        get => SelectedTrack?.End ?? double.NaN;
        set
        {
            if (SelectedTrack == null || !double.IsFinite(value) || Duration < .1) return;
            value = Math.Clamp(value, .1, Duration);
            if (SelectedTrack.Start is double a && value - a < .1 - 1e-9) { Status = "B deve ficar depois de A, com pelo menos 0,1 s."; return; }
            SelectedTrack.End = value; RangeChanged();
        }
    }
    private void ClearRange(bool a, bool b) { if (SelectedTrack == null) return; if (a) SelectedTrack.Start = null; if (b) SelectedTrack.End = null; looping = false; RangeChanged(); }
    private void RangeChanged(bool persist = true)
    {
        foreach (string name in new[] { nameof(RangeStart), nameof(RangeEnd), nameof(RangeLabel), nameof(HasRange), nameof(CanLoop), nameof(IsLooping), nameof(ExportLabel) }) Notify(name);
        if (persist) { Changed(); Start(ApplyParameters()); } CommandManager.InvalidateRequerySuggested();
    }
    public double Position
    {
        get => position;
        set
        {
            if (!CanSeek || !double.IsFinite(value)) return;
            position = IsLooping && HasRange ? Math.Clamp(value, RangeStart, RangeEnd) : Math.Clamp(value, 0, Duration);
            if (engine.Snapshot.Source == SelectedTrack?.Source && engine.Snapshot.OutputId != null) Start(engine.Seek(position));
            Notify(); Notify(nameof(TimeLabel));
        }
    }
    public void BeginParameterGesture() { parameterGesture = true; parameterTimer.Stop(); }
    public void EndParameterGesture() { parameterGesture = false; parameterTimer.Stop(); Start(ApplyParameters()); }
    private void ScheduleParameters() { parameterTimer.Stop(); if (!parameterGesture) parameterTimer.Start(); }
    private Task ApplyParameters() => engine.Snapshot.Source == SelectedTrack?.Source ? engine.Configure(Speed / 100, Pitch, IsLooping && HasRange, HasRange ? RangeStart : 0, HasRange ? RangeEnd : Duration) : Task.CompletedTask;
    private void RefreshPresentation()
    {
        foreach (string name in new[] { nameof(IsPlaying), nameof(IsLoading), nameof(HasActivePlayback), nameof(ShowMiniPlayer), nameof(PlayLabel), nameof(PlayIcon), nameof(OutputState) }) Notify(name);
        CommandManager.InvalidateRequerySuggested();
    }
    private void Dispatch(Action action) { if (!disposed && !dispatcher.HasShutdownStarted) _ = dispatcher.BeginInvoke(new Action(() => { if (!disposed) Run(action); })); }
    private void EngineChanged(BackingPlaybackSnapshot snapshot) => Dispatch(() =>
    {
        if (snapshot.Source != SelectedTrack?.Source || snapshot.Source == null || snapshot.Generation != engine.Snapshot.Generation) return;
        State = snapshot.State;
        if (snapshot.Duration > 0) { Duration = snapshot.Duration; SelectedTrack!.Length = Duration; }
        position = snapshot.Position; Notify(nameof(Position)); Notify(nameof(TimeLabel));
        SelectedTrack!.State = snapshot.State switch { BackingPlaybackState.Playing => "Tocando", BackingPlaybackState.Paused => "Pausada", BackingPlaybackState.Loading => "Carregando", BackingPlaybackState.Error => "Erro", BackingPlaybackState.Ended => "Finalizada", _ => "Pronta" };
        if (snapshot.State == BackingPlaybackState.Playing) { positionTimer.Start(); Status = ""; HasError = false; } else positionTimer.Stop();
        if (snapshot.Error != null) Error("Não foi possível reproduzir. Verifique o arquivo e a saída de áudio.", snapshot.Error);
        if (snapshot.State == BackingPlaybackState.Ended && ContinueSequence && !IsLooping) Start(AdvanceAtEnd());
    });
    private void UpdatePosition()
    {
        var snapshot = engine.Snapshot;
        if (snapshot.Source != SelectedTrack?.Source || snapshot.State is not (BackingPlaybackState.Playing or BackingPlaybackState.Paused or BackingPlaybackState.Ended)) return;
        position = snapshot.Position; Notify(nameof(Position)); Notify(nameof(TimeLabel));
    }
    private async Task TogglePlayback()
    {
        if (disposed || opening || SelectedTrack is not { } track || SelectedOutput is not { } output) return;
        StopYouTubePreview();
        if (IsPlaying) { await engine.Pause(); return; }
        int version = selectionGeneration; opening = true; RefreshPresentation();
        try
        {
            if (BeforePlayback != null) await BeforePlayback();
            await StopYouTubePreviewAndWait();
            if (disposed || version != selectionGeneration || output != SelectedOutput) return;
            if (engine.Snapshot.Source != track.Source || engine.Snapshot.OutputId != output.Id || State == BackingPlaybackState.Error)
                await engine.Open(track.Source, output.Id, Position, Speed / 100, Pitch, IsLooping && HasRange, HasRange ? RangeStart : 0, HasRange ? RangeEnd : Duration, Volume / 100, IsMuted);
            if (!disposed && version == selectionGeneration && output == SelectedOutput) await engine.Play();
        }
        finally { if (version == selectionGeneration) { opening = false; RefreshPresentation(); } }
    }
    public void PlaySelected() { StopYouTubePreview(); if (!IsPlaying) Start(TogglePlayback()); }
    private async Task StopPlayback() { await StopYouTubePreviewAndWait(); await engine.Stop(); position = IsLooping && HasRange ? RangeStart : 0; Notify(nameof(Position)); Notify(nameof(TimeLabel)); }
    private async Task SelectAdjacent(int step)
    {
        int index = SelectedTrack == null ? -1 : Tracks.IndexOf(SelectedTrack) + step;
        if (index < 0 || index >= Tracks.Count) return;
        bool resume = IsPlaying; SelectedTrack = Tracks[index]; if (resume) await TogglePlayback();
    }
    private async Task AdvanceAtEnd() { int index = SelectedTrack == null ? -1 : Tracks.IndexOf(SelectedTrack) + 1; if (index <= 0 || index >= Tracks.Count) return; SelectedTrack = Tracks[index]; await TogglePlayback(); }
    public async Task Initialize()
    {
        try { LoadInputs(); watcher = DeviceInformation.CreateWatcher(MediaDevice.GetAudioRenderSelector()); watcher.Added += DeviceChanged; watcher.Removed += DeviceChanged; watcher.Updated += DeviceChanged; watcher.Start(); await RefreshDevices(); }
        catch (Exception ex) { Error("Não foi possível listar as saídas de áudio. Tente atualizar saídas.", ex); }
    }
    private void DeviceChanged(DeviceWatcher sender, DeviceInformation args) => Dispatch(() => Start(RefreshDevices()));
    private void DeviceChanged(DeviceWatcher sender, DeviceInformationUpdate args) => Dispatch(() => Start(RefreshDevices()));
    private async Task RefreshDevices()
    {
        if (disposed || refreshing) return; refreshing = true;
        try
        {
            var devices = await Task.Run(() => { using var enumerator = new MMDeviceEnumerator(); return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(device => { using (device) return new BackingOutput(device.ID, device.FriendlyName); }).ToArray(); });
            if (disposed) return;
            foreach (var old in Outputs.ToArray()) if (!devices.Any(d => d.Id == old.Id))
            {
                if (SelectedOutput == old) { restoring = true; SelectedOutput = null; restoring = false; Status = "Saída desconectada. Reconecte o dispositivo e pressione Reproduzir."; HasError = true; } Outputs.Remove(old);
            }
            foreach (var device in devices) if (!Outputs.Any(d => d.Id == device.Id)) Outputs.Add(device);
            if (SelectedOutput == null && preferredOutputId != null) { restoring = true; SelectedOutput = Outputs.FirstOrDefault(d => string.Equals(d.Id, preferredOutputId, StringComparison.OrdinalIgnoreCase)); restoring = false; }
            if (SelectedOutput == null && Outputs.Count > 0)
            {
                string? defaultId = await Task.Run(() =>
                {
                    try
                    {
                        using var enumerator = new MMDeviceEnumerator();
                        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                        return device.ID;
                    }
                    catch { return null; }
                });
                if (disposed) return;
                restoring = true;
                try { SelectedOutput = Outputs.FirstOrDefault(d => d.Id == defaultId) ?? Outputs.FirstOrDefault(); }
                finally { restoring = false; }
            }
            if (Outputs.Count == 0) Status = "Nenhuma saída disponível. Conecte uma saída de áudio e atualize a lista.";
        }
        catch (Exception ex) { Error("Não foi possível listar as saídas de áudio. Tente atualizar saídas.", ex); }
        finally { refreshing = false; CommandManager.InvalidateRequerySuggested(); }
    }
    public void AddPaths(IEnumerable<string> paths)
    {
        int rejected = 0;
        foreach (string path in paths)
        {
            if (!Path.IsPathFullyQualified(path) || !File.Exists(path) || Path.GetExtension(path).ToLowerInvariant() is not (".mp3" or ".wav")) { rejected++; continue; }
            string source = Path.GetFullPath(path); if (Tracks.Any(t => string.Equals(t.Source, source, StringComparison.OrdinalIgnoreCase))) continue; Tracks.Add(new(source));
        }
        SelectedTrack ??= Tracks.FirstOrDefault(); Changed();
        if (rejected > 0) Status = string.Format(LanguageService.T("{0} arquivo(s) não adicionado(s). Use arquivos MP3/WAV locais e acessíveis."), rejected);
    }
    public void MoveTrack(BackingTrack track, int index) { int old = Tracks.IndexOf(track); if (old < 0 || index < 0 || index >= Tracks.Count) return; Tracks.Move(old, index); Changed(); }
    private void MoveSelected(int step) { if (SelectedTrack != null) MoveTrack(SelectedTrack, Tracks.IndexOf(SelectedTrack) + step); }
    private void RemoveSelected() { if (SelectedTrack is { } track) RemoveTrackItem(track); }
    internal void RemoveTrackItem(BackingTrack track)
    {
        int index = Tracks.IndexOf(track); if (index < 0) return;
        bool current = track == SelectedTrack;
        if (current) SelectedTrack = null;
        Tracks.Remove(track);
        if (current) SelectedTrack = Tracks.Count > 0 ? Tracks[Math.Min(index, Tracks.Count - 1)] : null;
        Changed();
    }
    internal void ShowTrackFile(BackingTrack track) => Run(() =>
    {
        if (Tracks.Contains(track)) Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + track.Source + "\"") { UseShellExecute = true });
    });
    private void LocateSelected() { if (SelectedTrack is { } track) LocateTrackFile(track); }
    internal void LocateTrackFile(BackingTrack track) => Run(() =>
    {
        if (!Tracks.Contains(track)) return;
        var dialog = new OpenFileDialog { Filter = "MP3/WAV|*.mp3;*.wav" }; if (dialog.ShowDialog() != true) return;
        if (Tracks.Any(t => t != track && string.Equals(t.Source, dialog.FileName, StringComparison.OrdinalIgnoreCase))) { Status = "Este arquivo já está na playlist."; return; }
        int index = Tracks.IndexOf(track); if (index < 0) return;
        bool current = SelectedTrack == track;
        var replacement = new BackingTrack(Path.GetFullPath(dialog.FileName));
        Tracks[index] = replacement;
        if (current) SelectedTrack = replacement;
        Changed();
    });
    internal void UpdateTrackMetadata(BackingTrack track, double bpm, string key, string tuning, int preset)
    {
        if (!Tracks.Contains(track) || !double.IsFinite(bpm) || !Keys.Contains(key)) return;
        if (bpm > 0 && bpm != track.Bpm) track.ManualBpm = true;
        if (key != track.OriginalKey) track.ManualKey = true;
        track.Bpm = bpm > 0 ? Math.Clamp(bpm, 20, 300) : track.Bpm; track.OriginalKey = key; track.Tuning = tuning.Trim(); track.Preset = Math.Clamp(preset, 0, 3);
        if (track == SelectedTrack)
            foreach (string name in new[] { nameof(Bpm), nameof(CurrentBpm), nameof(BpmBadge), nameof(OriginalKey), nameof(CurrentKey), nameof(KeyBadge), nameof(LinkedPreset), nameof(PresetBadge), nameof(PresetHint) }) Notify(name);
        FilteredTracks.Refresh(); Notify(nameof(SelectedTrack)); Changed();
    }
    internal void AddSectionAt(BackingTrack track, double time, string name)
    {
        name = name.Trim();
        if (!Tracks.Contains(track) || !double.IsFinite(time) || track.Length <= 0 || name.Length is 0 or > 40) return;
        time = Math.Clamp(time, 0, track.Length);
        int index = 0; while (index < track.Sections.Count && track.Sections[index].Position <= time) index++;
        track.Sections.Insert(index, new BackingSection(time, name)); Changed();
    }
    private async Task LoadWaveform()
    {
        waveformCancellation?.Cancel();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); waveformCancellation = cancellation;
        var track = SelectedTrack; if (track == null) { AnalysisStatus = ""; waveformCancellation = null; return; }
        try
        {
            var file = new FileInfo(track.Source);
            if (!file.Exists) { track.State = "Arquivo ausente"; AnalysisStatus = "Arquivo ausente. Use Localizar novamente no menu da faixa."; return; }
            if (track.FileSize != file.Length || track.LastWriteTicks != file.LastWriteTimeUtc.Ticks)
            {
                if (track.LastWriteTicks != 0) { track.Start = track.End = null; track.Sections.Clear(); looping = false; RangeChanged(); }
                track.Peaks = []; track.FileSize = file.Length; track.LastWriteTicks = file.LastWriteTimeUtc.Ticks; Changed();
            }
            if (track.Peaks.Length == 0)
            {
                AnalysisStatus = "Analisando waveform…";
                var work = BackingAudioProcessing.Analyze(track.Source, cancellation.Token); backgroundWork.Add(work);
                (double Duration, float[] Peaks) result;
                try { result = await work; } finally { backgroundWork.Remove(work); }
                if (disposed || cancellation.IsCancellationRequested || SelectedTrack != track) return;
                track.Length = result.Duration; track.Peaks = result.Peaks;
            }
            track.LastUsed = ++cacheUse; long total = Tracks.Sum(t => (long)t.Peaks.Length * sizeof(float));
            foreach (var old in Tracks.Where(t => t != track).OrderBy(t => t.LastUsed)) { if (total <= 32 * 1024 * 1024) break; total -= (long)old.Peaks.Length * sizeof(float); old.Peaks = []; }
            Duration = track.Length;
            if (track.Start is double a && (a < 0 || a >= Duration)) track.Start = null;
            if (track.End is double b && (b > Duration || b <= 0)) track.End = null;
            Notify(nameof(Peaks)); RangeChanged(false); AnalysisStatus = "";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Log(ex); if (!disposed && SelectedTrack == track && !cancellation.IsCancellationRequested) AnalysisStatus = "Waveform indisponível. Você ainda pode tentar reproduzir o arquivo."; }
        finally { if (waveformCancellation == cancellation) waveformCancellation = null; }
    }
    private async Task ExportSelection()
    {
        if (SelectedTrack is not { } track || exporting) return;
        double a = HasRange ? RangeStart : 0, b = HasRange ? RangeEnd : Duration, rate = Speed / 100, pitch = Pitch;
        bool? mode = ChooseExportMode == null ? false : await ChooseExportMode(); if (mode == null || disposed) return;
        string suffix = mode.Value ? $"-trecho-{rate * 100:0}pct-{pitch:+0;-0;0}st" : "-trecho";
        var dialog = new SaveFileDialog { Filter = "WAV PCM 16-bit|*.wav", FileName = track.Title + suffix + ".wav" }; if (dialog.ShowDialog() != true) return;
        if (Tracks.Any(t => string.Equals(Path.GetFullPath(dialog.FileName), t.Source, StringComparison.OrdinalIgnoreCase)) || string.Equals(Path.GetFullPath(dialog.FileName), track.Source, StringComparison.OrdinalIgnoreCase)) { Status = "Escolha outro nome para preservar o áudio original."; return; }
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); exportCancellation = cancellation;
        IsExporting = true; Status = "Exportando WAV…"; CommandManager.InvalidateRequerySuggested();
        try
        {
            var work = BackingAudioProcessing.Export(track.Source, dialog.FileName, a, b, rate, pitch, mode.Value, cancellation.Token); backgroundWork.Add(work);
            try { await work; } finally { backgroundWork.Remove(work); }
            if (!disposed) Status = "WAV exportado. O áudio original foi preservado.";
        }
        catch (OperationCanceledException) { if (!disposed) Status = "Exportação cancelada."; }
        catch (Exception ex) { if (!disposed) Error("Não foi possível exportar. Verifique o destino e tente novamente.", ex); }
        finally { exportCancellation = null; IsExporting = false; CommandManager.InvalidateRequerySuggested(); }
    }
    private void Changed() => CommandManager.InvalidateRequerySuggested();
    private void Log(Exception ex) => System.Diagnostics.Debug.WriteLine(ex);
    private void Error(string message, Exception ex) { Log(ex); Status = message; HasError = true; }
    private void Run(Action action) { try { action(); } catch (Exception ex) { Error("Operação não concluída. Tente novamente.", ex); } }
    private async void Start(Task operation) { try { await operation; } catch (OperationCanceledException) { } catch (Exception ex) { if (!disposed) Error("Operação de áudio não concluída. Verifique o arquivo e a saída.", ex); } }
    public void Dispose()
    {
        if (disposed) return;
        StopYouTubePreview(); StopMonitor(); meterTimer.Stop(); disposed = true; lifetime.Cancel(); waveformCancellation?.Cancel(); exportCancellation?.Cancel(); positionTimer.Stop(); parameterTimer.Stop();
        LanguageService.Instance.PropertyChanged -= MusicLanguageChanged;
        engine.Changed -= EngineChanged; engine.Dispose();
        try { Task.WhenAll(backgroundWork.ToArray()).GetAwaiter().GetResult(); } catch (Exception ex) { if (ex is not OperationCanceledException) Log(ex); }
        if (watcher != null) { watcher.Added -= DeviceChanged; watcher.Removed -= DeviceChanged; watcher.Updated -= DeviceChanged; if (watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted) watcher.Stop(); }
        lifetime.Dispose();
    }
}
