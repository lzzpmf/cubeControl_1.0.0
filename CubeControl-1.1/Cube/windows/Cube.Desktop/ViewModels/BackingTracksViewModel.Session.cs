using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Input;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed record BackingInputChoice(int First, int Count)
{
    public string Label => Count == 1 ? $"Mono {First + 1}" : $"Stereo {First + 1}/{First + 2}";
}

public sealed record BackingTake(Guid Id, string Source, double Offset, double RecordedSpeed = 1, double RecordedPitch = 0, bool Incomplete = false, bool LegacyMono = true)
{
    public string Title => Path.GetFileNameWithoutExtension(Source) + (Incomplete ? " !" : "");
}

public sealed class BackingStem : Observable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public List<BackingClip> Clips { get; } = [];
    public bool ClipsInitialized { get; set; }
    private string source = "", state = "Pronta";
    private bool muted, solo, armed, monitoring;
    private string name = "", color = "#6495ED";
    private double pan;
    private int inputChannel, inputChannels = 1;
    private BackingTake? activeTake;
    private bool canEditRouting = true;
    private bool canAdjustMix = true;
    public bool CanAdjustMix { get => canAdjustMix; internal set => Set(ref canAdjustMix, value); }
    public bool CanEditRouting { get => canEditRouting; internal set => Set(ref canEditRouting, value); }
    public bool LegacyMono { get; set; }
    public ObservableCollection<BackingTake> Takes { get; } = [];
    public BackingTake? ActiveTake
    {
        get => activeTake;
        set
        {
            if (!CanEditRouting || value == null || value.Incomplete || !Takes.Contains(value) || activeTake == value) return;
            activeTake = value;
            Source = value.Source; Offset = value.Offset; RecordedSpeed = value.RecordedSpeed; RecordedPitch = value.RecordedPitch; LegacyMono = value.LegacyMono;
            Peaks = []; Length = 0; Notify(nameof(TimelineLength)); Notify(nameof(ActiveTake));
        }
    }
    internal BackingTake[] SnapshotTakes() => Takes.Select(t => t.Id == activeTake?.Id && CanEditRouting
        ? t with { Source = Source, Offset = Offset, RecordedSpeed = RecordedSpeed, RecordedPitch = RecordedPitch, LegacyMono = LegacyMono } : t).ToArray();
    public void EnsureTake()
    {
        if (activeTake != null || ClipsInitialized || string.IsNullOrEmpty(Source)) return;
        var take = new BackingTake(Guid.NewGuid(), Source, Offset, RecordedSpeed, RecordedPitch, LegacyMono: LegacyMono);
        Takes.Add(take); activeTake = take; Notify(nameof(ActiveTake));
    }
    public string Name { get => string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(Source) : name; set { if (CanEditRouting && !string.IsNullOrWhiteSpace(value) && Set(ref name, value.Trim())) Notify(nameof(Title)); } }
    public string Color { get => color; set { if (value.Length == 7 && value[0] == '#' && int.TryParse(value[1..], System.Globalization.NumberStyles.HexNumber, null, out _)) Set(ref color, value); } }
    public bool IsArmed { get => armed; set { if (CanEditRouting) Set(ref armed, value); } }
    public bool IsMonitoring { get => monitoring; set { if (CanEditRouting) Set(ref monitoring, value); } }
    public BackingInputChoice InputChoice { get => new(InputChannel, InputChannels); set { if (value != null && CanEditRouting) { InputChannel = value.First; InputChannels = value.Count; Notify(); } } }
    public int InputChannel { get => inputChannel; set { if (CanEditRouting) Set(ref inputChannel, Math.Max(0, value)); } }
    public int InputChannels { get => inputChannels; set { if (CanEditRouting) Set(ref inputChannels, Math.Clamp(value, 1, 2)); } }
    public double Pan { get => pan; set { if (CanAdjustMix && double.IsFinite(value)) Set(ref pan, Math.Clamp(value, -1, 1)); } }
    public double GainDb { get => BackingMixMath.ToDb(Volume / 100); set { if (double.IsFinite(value)) Volume = BackingMixMath.FromDb(value) * 100; } }
    private BackingMeterReading meter = BackingMeterReading.Silent;
    public BackingMeterReading Meter { get => meter; internal set => Set(ref meter, value); }
    private BackingMeterReading inputReading = BackingMeterReading.Silent;
    public BackingMeterReading InputMeter { get => inputReading; internal set => Set(ref inputReading, value); }
    private double inputRmsDb = -60;
    public double InputRmsDb { get => inputRmsDb; internal set => Set(ref inputRmsDb, value); }
    private double rmsDb = -60;
    public double RmsDb { get => rmsDb; internal set => Set(ref rmsDb, value); }
    private double volume = 100, offset, length;
    private float[] peaks = [];
    public string Source { get => source; set { if (Set(ref source, value)) { Notify(nameof(Title)); Notify(nameof(Name)); } } }
    public string Title => Name;
    public string State { get => state; set => Set(ref state, value); }
    public bool IsMuted { get => muted; set { if (CanAdjustMix && Set(ref muted, value)) Notify(nameof(MuteLabel)); } }
    public bool IsSolo { get => solo; set { if (CanAdjustMix && Set(ref solo, value)) Notify(nameof(SoloLabel)); } }
    public string MuteLabel => IsMuted ? "Restaurar som da pista" : "Silenciar pista";
    public string SoloLabel => IsSolo ? "Desativar solo da pista" : "Ouvir pista em solo";
    public double Volume { get => volume; set { if (CanAdjustMix && double.IsFinite(value)) { if (Set(ref volume, Math.Clamp(value, 0, BackingMixMath.MaximumGain * 100))) Notify(nameof(GainDb)); } } }
    public double Offset { get => offset; set { if (double.IsFinite(value)) Set(ref offset, value); } }
    public double RecordedSpeed { get; set; } = 1;
    public double RecordedPitch { get; set; }
    public double Length { get => length; set { if (Set(ref length, value)) Notify(nameof(TimelineLength)); } }
    public double TimelineLength => Length * RecordedSpeed;
    public float[] Peaks { get => peaks; set => Set(ref peaks, value); }
    public bool IsProvisional { get; set; }
    internal BackingStemSource AudioSource => new(Id, Source, Offset, RecordedSpeed, RecordedPitch, IsMuted, IsSolo, Volume / 100, Pan, LegacyMono);
    internal void RebindMedia(BackingProjectStem saved)
    {
        source = saved.Source;
        Takes.Clear(); foreach (var take in saved.Takes) Takes.Add(take);
        activeTake = Takes.FirstOrDefault(t => t.Id == saved.ActiveTake);
        Clips.Clear(); Clips.AddRange(saved.Clips);
        Notify(nameof(Source));
    }
}

public sealed partial class BackingTracksViewModel
{
    public IEnumerable<BackingStem> MonitoredInputs => SessionStems?.Where(s => s.IsMonitoring || IsRecording && s.IsArmed) ?? Enumerable.Empty<BackingStem>();
    public ObservableCollection<BackingStem>? SessionStems => SelectedTrack?.Stems;
    private double trackHeaderWidth = 280;
    public double TrackHeaderWidth { get => trackHeaderWidth; set => Set(ref trackHeaderWidth, value); }
    private double trackHeight = 120;
    public double TrackHeight { get => trackHeight; set => Set(ref trackHeight, Math.Clamp(value, 80, 320)); }
    private BackingStem? selectedStem;
    public BackingStem? SelectedStem { get => selectedStem; set { if (Set(ref selectedStem, value)) { Notify(nameof(Peaks)); CommandManager.InvalidateRequerySuggested(); } } }
    public bool CanEditSession => !IsRecording && !recordingFinalizing && !projectBusy && !IsExporting;
    private double latencyCompensationMs;
    public double LatencyCompensationMs
    {
        get => latencyCompensationMs;
        set { if (CanEditSession && double.IsFinite(value) && Set(ref latencyCompensationMs, Math.Clamp(value, -1000, 1000))) PersistSession(); }
    }
    private bool followRecording = true;
    public bool FollowRecording { get => followRecording; set => Set(ref followRecording, value); }
    public bool ExportMix { get; set; } = true;
    public ICommand DuplicateStem { get; private set; } = null!;
    public ICommand AddEmptyStem { get; private set; } = null!;
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
            CommitEditHistory();
            try { SaveRecordingPlaylist(); SaveState = "Salvo automaticamente"; } catch (Exception ex) { SaveState = "Falha ao salvar"; Error("Não foi possível salvar a playlist.", ex); }
        };
        AddEmptyStem = new UiCommand(() =>
        {
            if (SelectedTrack == null) { var track = new BackingTrack(Path.Combine(recordingDirectory, "New.session")) { Name = LanguageService.T("Novo projeto"), Bpm = 120, ManualBpm = true, AccompanimentGain = 1 }; track.Stems.Clear(); Tracks.Add(track); SelectedTrack = track; }
            var stem = new BackingStem { Source = Path.Combine(recordingDirectory, Guid.NewGuid() + ".wav"), Name = LanguageService.T(SessionStems!.Count == 0 ? "Guitarra" : "Nova pista"), Color = "#58BFA6", LegacyMono = false, ClipsInitialized = true };
            SessionStems!.Add(stem); AttachStem(stem); SelectedStem = stem; PersistSession();
        }, () => CanEditSession);
        DuplicateStem = new UiCommand(() =>
        {
            if (SelectedStem is not { } original) return;
            original.EnsureTake();
            var copy = new BackingStem { Source = original.Source, Name = original.Name + " (2)", Color = original.Color, Volume = original.Volume, Pan = original.Pan, IsMuted = original.IsMuted, IsSolo = original.IsSolo, LegacyMono = original.LegacyMono, InputChannel = original.InputChannel, InputChannels = original.InputChannels };
            EnsureClips(original); copy.Clips.AddRange(original.Clips.Select(c => c with { Id = Guid.NewGuid() })); copy.ClipsInitialized = true;
            foreach (var take in original.SnapshotTakes()) copy.Takes.Add(take with { Id = Guid.NewGuid() });
            copy.ActiveTake = copy.Takes.ElementAtOrDefault(original.ActiveTake == null ? 0 : original.Takes.IndexOf(original.ActiveTake));
            SessionStems!.Add(copy); AttachStem(copy); SelectedStem = copy; PersistSession(); Start(LoadSessionWaves()); Start(engine.RefreshSession(SessionSources()));
        }, () => CanEditSession && SelectedStem != null);
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
        }, () => CanEditSession && SelectedStem != null);
        StemUp = new UiCommand(() => MoveStem(SelectedStem!, SessionStems!.IndexOf(SelectedStem!) - 1), () => CanEditSession && SelectedStem != null && SessionStems?.IndexOf(SelectedStem) > 0);
        StemDown = new UiCommand(() => MoveStem(SelectedStem!, SessionStems!.IndexOf(SelectedStem!) + 1), () => CanEditSession && SelectedStem != null && SessionStems?.IndexOf(SelectedStem) < SessionStems?.Count - 1);
        LocateStem = new UiCommand(() =>
        {
            if (SelectedStem is not { } stem) return;
            var dialog = new OpenFileDialog { Filter = "MP3/WAV|*.mp3;*.wav" };
            if (dialog.ShowDialog() != true) return;
            Start(RelinkClipSource(stem, stem.Source, dialog.FileName));
            if (stem.ActiveTake is { } active)
            {
                var replacement = active with { Source = dialog.FileName };
                stem.Takes[stem.Takes.IndexOf(active)] = replacement; stem.ActiveTake = replacement;
            }
            else stem.Source = dialog.FileName;
            stem.Peaks = []; stem.State = "Pronta";
            PersistSession(); Start(LoadSessionWaves()); Start(engine.RefreshSession(SessionSources()));
        }, () => CanEditSession && SelectedStem != null);
    }
    private void ActivateSession()
    {
        if (SelectedTrack is { } track)
        {
            foreach (var stem in track.Stems) AttachStem(stem);
        }
        StopMonitor(true);
        if (SelectedTrack is { } selected)
        {
            masterVolume = selected.MasterGain; volume = selected.AccompanimentGain * 100;
            if (!devicePreferencesLoaded && savedInputId == null) savedInputId = selected.CaptureEndpoint;
            selectedInput = Inputs.FirstOrDefault(x => x.Id == savedInputId);
            if (selectedInput == null && savedInputId != null) { selectedInput = new(savedInputId, LanguageService.T("Dispositivo indisponível"), false); Inputs.Add(selectedInput); }
            RefreshInputChoices(); Notify(nameof(SelectedInput)); Notify(nameof(Volume)); Notify(nameof(AccompanimentDb)); Notify(nameof(MasterDb));
            Start(engine.SetMasterVolume(masterVolume));
        }
        SelectedClips.Clear(); InitializeHistory(); Notify(nameof(SessionMeter));
        SelectedStem = SessionStems?.FirstOrDefault(); Notify(nameof(SessionStems)); Notify(nameof(MonitoredInputs));
        Start(LoadSessionWaves());
    }
    private void AttachStem(BackingStem stem)
    {
        if (!attachedStems.Add(stem)) return;
        stem.CanEditRouting = CanEditSession; stem.CanAdjustMix = CanAdjustMix;
        stem.EnsureTake();
        EnsureClips(stem);
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
        if (e.PropertyName == nameof(BackingStem.ActiveTake)) { if (recordingFinalizing) { PersistSession(); return; } Start(LoadSessionWaves()); Start(engine.RefreshSession(SessionSources())); PersistSession(); return; }
        if (e.PropertyName is nameof(BackingStem.Name) or nameof(BackingStem.Color) or nameof(BackingStem.InputChannel) or nameof(BackingStem.InputChannels)) { PersistSession(); if (e.PropertyName is nameof(BackingStem.InputChannel) or nameof(BackingStem.InputChannels)) RestartMonitorForDeviceChange(); return; }
        if (e.PropertyName == nameof(BackingStem.IsMonitoring)) { if (suppressMonitorChanges) return; Notify(nameof(MonitoredInputs)); if (recording != null) recording.SetRoutes(CaptureRoutes(true)); else RestartMonitorForDeviceChange(); return; }
        if (e.PropertyName == nameof(BackingStem.IsArmed)) { RestartMonitorForDeviceChange(); Changed(); return; }
        if (e.PropertyName is not (nameof(BackingStem.Pan) or nameof(BackingStem.IsMuted) or nameof(BackingStem.IsSolo) or nameof(BackingStem.Volume))) return;
        if (SessionStems?.Contains(stem) == true) { Start(engine.SetStemMix(SessionSources())); (recording ?? liveCapture)?.SetRoutes(CaptureRoutes(IsRecording)); }
        PersistSession();
    }
    private BackingStemSource[] SessionSources() => SessionStems?.Where(s => !s.IsProvisional).SelectMany(s => s.Clips.SelectMany(c => c.Sources(s)).DefaultIfEmpty(s.AudioSource with { Offset = 0, ClipLength = 0 }).Select(source => source with { Muted = s.IsMuted || IsRecording && s.IsArmed })).ToArray() ?? [];
    public void MoveStem(BackingStem stem, int index)
    {
        if (!CanEditSession || SessionStems is not { } stems || !stems.Contains(stem) || index < 0 || index >= stems.Count) return;
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
        foreach (string candidate in files)
        {
            string file = candidate;
            if (!File.Exists(file) || Path.GetExtension(file).ToLowerInvariant() is not (".mp3" or ".wav")) continue;
            file = await OwnProjectAudioAsync(file);
            if (SelectedTrack != track || !CanEditSession) return;
            if (string.IsNullOrEmpty(track.AnalysisSource)) track.AnalysisSource = File.Exists(track.Source) ? track.Source : file;
            var stem = new BackingStem { Source = Path.GetFullPath(file), Name = Path.GetFileNameWithoutExtension(candidate) }; track.Stems.Add(stem); AttachStem(stem); PersistSession();
        }
        PersistSession(); await LoadSessionWaves();
        if (SelectedTrack == track) await engine.RefreshSession(SessionSources());
        ScheduleMusic();
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
        foreach (var source in track.Stems.SelectMany(s => s.Clips).SelectMany(c => c.Takes).SelectMany(t => t.Layers).Select(l => l.Source).Distinct().ToArray())
        {
            if (ClipWaves.ContainsKey(source) || !File.Exists(source)) continue;
            try { var result = await BackingAudioProcessing.Analyze(source, lifetime.Token, 32768); ClipWaves[source] = result; }
            catch (OperationCanceledException) { return; } catch (Exception ex) { Log(ex); }
        }
        long cacheBytes = ClipWaves.Values.Sum(w => (long)w.Peaks.Length * sizeof(float));
        foreach (string key in ClipWaves.Keys.ToArray()) { if (cacheBytes <= 32 * 1024 * 1024) break; cacheBytes -= (long)ClipWaves[key].Peaks.Length * sizeof(float); ClipWaves.Remove(key); }
        if (SelectedTrack == track) { RefreshSessionDuration(); InitializeHistory(); NotifyClips(); }
    }
    private void RefreshSessionDuration()
    {
        Duration = SessionStems?.SelectMany(s => s.Clips).Select(c => Math.Max(0, c.End)).DefaultIfEmpty(0).Max() ?? 0;
    }
    private void PersistSession()
    {
        MarkProjectChanged();
        // Legacy playlists are import sources. Portable projects have their own recovery/save lifecycle.
        if (!string.IsNullOrEmpty(projectDirectory)) return;
        if (!playlistLoaded || disposed || editingGesture) return;
        SaveState = "Salvando…"; sessionSaveTimer.Stop(); sessionSaveTimer.Start();
    }
}
