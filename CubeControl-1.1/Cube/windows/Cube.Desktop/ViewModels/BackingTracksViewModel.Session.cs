using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Input;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed class BackingStem : Observable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    private string source = "", state = "Pronta";
    private bool muted, solo;
    private double volume = 100, offset, length;
    private float[] peaks = [];
    public string Source { get => source; set { if (Set(ref source, value)) Notify(nameof(Title)); } }
    public string Title => Path.GetFileNameWithoutExtension(Source);
    public string State { get => state; set => Set(ref state, value); }
    public bool IsMuted { get => muted; set { if (Set(ref muted, value)) Notify(nameof(MuteLabel)); } }
    public bool IsSolo { get => solo; set { if (Set(ref solo, value)) Notify(nameof(SoloLabel)); } }
    public string MuteLabel => IsMuted ? "Restaurar som da pista" : "Silenciar pista";
    public string SoloLabel => IsSolo ? "Desativar solo da pista" : "Ouvir pista em solo";
    public double Volume { get => volume; set { if (double.IsFinite(value)) Set(ref volume, Math.Clamp(value, 0, 100)); } }
    public double Offset { get => offset; set { if (double.IsFinite(value)) Set(ref offset, value); } }
    public double RecordedSpeed { get; set; } = 1;
    public double RecordedPitch { get; set; }
    public double Length { get => length; set { if (Set(ref length, value)) Notify(nameof(TimelineLength)); } }
    public double TimelineLength => Length * RecordedSpeed;
    public float[] Peaks { get => peaks; set => Set(ref peaks, value); }
    public bool IsProvisional { get; set; }
    internal BackingStemSource AudioSource => new(Id, Source, Offset, RecordedSpeed, RecordedPitch, IsMuted, IsSolo, Volume / 100);
}

public sealed partial class BackingTracksViewModel
{
    public ObservableCollection<BackingStem>? SessionStems => SelectedTrack?.Stems;
    private BackingStem? selectedStem;
    public BackingStem? SelectedStem { get => selectedStem; set { if (Set(ref selectedStem, value)) { Notify(nameof(Peaks)); CommandManager.InvalidateRequerySuggested(); } } }
    public bool CanEditSession => !IsRecording && !recordingFinalizing;
    private double latencyCompensationMs;
    public double LatencyCompensationMs
    {
        get => latencyCompensationMs;
        set { if (CanEditSession && double.IsFinite(value) && Set(ref latencyCompensationMs, Math.Clamp(value, -1000, 1000))) PersistSession(); }
    }
    private bool followRecording = true;
    public bool FollowRecording { get => followRecording; set => Set(ref followRecording, value); }
    public bool ExportMix { get; set; } = true;
    public ICommand ImportStem { get; private set; } = null!;
    public ICommand RemoveStem { get; private set; } = null!;
    public ICommand StemUp { get; private set; } = null!;
    public ICommand StemDown { get; private set; } = null!;
    public ICommand LocateStem { get; private set; } = null!;
    private readonly HashSet<BackingStem> attachedStems = [];
    private readonly DispatcherTimer sessionSaveTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };

    private void InitializeSession()
    {
        sessionSaveTimer.Tick += (_, _) =>
        {
            sessionSaveTimer.Stop();
            try { SaveRecordingPlaylist(); } catch (Exception ex) { Error("Não foi possível salvar a playlist.", ex); }
        };
        ImportStem = new UiCommand(() =>
        {
            var dialog = new OpenFileDialog { Multiselect = true, Filter = "MP3/WAV|*.mp3;*.wav" };
            if (dialog.ShowDialog() == true) Start(AddSessionFiles(dialog.FileNames));
        }, () => SelectedTrack != null && CanEditSession);
        RemoveStem = new UiCommand(() =>
        {
            if (SelectedStem is not { } stem || SessionStems == null) return;
            SessionStems.Remove(stem); DetachStem(stem); SelectedStem = SessionStems.FirstOrDefault();
            PersistSession(); Start(engine.RefreshSession(SessionSources())); RefreshSessionDuration();
        }, () => CanEditSession && SelectedStem != null && SessionStems?.Count > 1);
        StemUp = new UiCommand(() => MoveStem(SelectedStem!, SessionStems!.IndexOf(SelectedStem!) - 1), () => SelectedStem != null && SessionStems?.IndexOf(SelectedStem) > 0);
        StemDown = new UiCommand(() => MoveStem(SelectedStem!, SessionStems!.IndexOf(SelectedStem!) + 1), () => SelectedStem != null && SessionStems?.IndexOf(SelectedStem) < SessionStems?.Count - 1);
        LocateStem = new UiCommand(() =>
        {
            if (SelectedStem is not { } stem) return;
            var dialog = new OpenFileDialog { Filter = "MP3/WAV|*.mp3;*.wav" };
            if (dialog.ShowDialog() != true) return;
            stem.Source = dialog.FileName; stem.Peaks = []; stem.State = "Pronta";
            PersistSession(); Start(LoadSessionWaves()); Start(engine.RefreshSession(SessionSources()));
        }, () => CanEditSession && SelectedStem != null);
    }
    private void ActivateSession()
    {
        if (SelectedTrack is { } track)
        {
            if (track.Stems.Count == 0) track.Stems.Add(new() { Source = track.Source });
            foreach (var stem in track.Stems) AttachStem(stem);
        }
        SelectedStem = SessionStems?.FirstOrDefault(); Notify(nameof(SessionStems));
        Start(LoadSessionWaves());
    }
    private void AttachStem(BackingStem stem)
    {
        if (!attachedStems.Add(stem)) return;
        stem.PropertyChanged += StemChanged;
    }
    private void DetachStem(BackingStem stem)
    {
        if (attachedStems.Remove(stem)) stem.PropertyChanged -= StemChanged;
    }
    private void StemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not BackingStem stem) return;
        if (stem == SelectedStem && e.PropertyName == nameof(BackingStem.Peaks)) Notify(nameof(Peaks));
        if (e.PropertyName is not (nameof(BackingStem.IsMuted) or nameof(BackingStem.IsSolo) or nameof(BackingStem.Volume))) return;
        if (SessionStems?.Contains(stem) == true) Start(engine.SetStemMix(SessionSources()));
        PersistSession();
    }
    private BackingStemSource[] SessionSources() => SessionStems?.Where(s => !s.IsProvisional).Select(s => s.AudioSource).ToArray() ?? [];
    public void MoveStem(BackingStem stem, int index)
    {
        if (SessionStems is not { } stems || !stems.Contains(stem) || index < 0 || index >= stems.Count) return;
        stems.Move(stems.IndexOf(stem), index); PersistSession();
    }
    public async Task AddSessionFiles(IEnumerable<string> files)
    {
        try { await AddSessionFilesCore(files); }
        catch (Exception ex) { Error("Não foi possível adicionar pistas à sessão.", ex); }
    }
    private async Task AddSessionFilesCore(IEnumerable<string> files)
    {
        if (!CanEditSession || SelectedTrack is not { } track) return;
        foreach (string file in files)
        {
            if (!File.Exists(file) || Path.GetExtension(file).ToLowerInvariant() is not (".mp3" or ".wav")) continue;
            var stem = new BackingStem { Source = Path.GetFullPath(file) }; track.Stems.Add(stem); AttachStem(stem);
        }
        PersistSession(); await LoadSessionWaves();
        if (SelectedTrack == track) await engine.RefreshSession(SessionSources());
    }
    private async Task LoadSessionWaves()
    {
        if (SelectedTrack is not { } track) return;
        foreach (var stem in track.Stems.ToArray())
        {
            if (disposed || stem.IsProvisional || stem.Peaks.Length > 0) continue;
            if (!File.Exists(stem.Source)) { stem.State = "Arquivo ausente"; continue; }
            try
            {
                string analyzedSource = stem.Source;
                var work = BackingAudioProcessing.Analyze(analyzedSource, lifetime.Token, 32768); backgroundWork.Add(work);
                (double Duration, float[] Peaks) result;
                try { result = await work; } finally { backgroundWork.Remove(work); }
                if (disposed || !track.Stems.Contains(stem) || stem.Source != analyzedSource) continue;
                stem.Length = result.Duration; stem.Peaks = result.Peaks;
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { Log(ex); stem.State = "Waveform indisponível"; }
        }
        if (SelectedTrack == track) RefreshSessionDuration();
    }
    private void RefreshSessionDuration()
    {
        Duration = SessionStems?.Select(s => Math.Max(0, s.Offset + s.TimelineLength)).DefaultIfEmpty(0).Max() ?? 0;
    }
    private void PersistSession()
    {
        if (!playlistLoaded || disposed) return;
        sessionSaveTimer.Stop(); sessionSaveTimer.Start();
    }
}
