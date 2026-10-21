#if WINUI
using ICollectionView = Cube.WinUI.Platform.ListCollectionView;
#endif
using System.Collections.ObjectModel;
using System.ComponentModel;
#if WINUI
using Cube.WinUI.Platform;
#else
using System.Windows.Data;
#endif
using System.Windows.Input;
#if WINUI

#else
using System.Windows.Threading;
#endif
using Cube.Desktop.Services;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.ViewModels;

public sealed record BackingSection(double Position, string Name)
{
    public string Label => $"{BackingTracksViewModel.FormatTime(Position, true)} · {Name}";
}

public sealed partial class BackingTracksViewModel
{
    private readonly DispatcherTimer meterTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };
    private int trackLevel, inputLevel;
    public int TrackLevel { get => trackLevel; private set => Set(ref trackLevel, value); }
    public int InputLevel { get => inputLevel; private set => Set(ref inputLevel, value); }
    private void RefreshMeters()
    {
        bool active = IsPlaying || IsMonitoring || metronomeEnabled;
        if (active) meterTimer.Start(); else meterTimer.Stop();
        if (!IsPlaying) { TrackLevel = 0; engine.ConsumeOutputPeak(); }
        if (!IsMonitoring) { InputLevel = 0; InputClipping = false; lastInputClip = double.NegativeInfinity; }
        UpdateMixerMeters();
    }
    private string search = "", sectionName = "Intro";
    private double zoom = 1, viewportStart, monitorVolume = 0;
    private BackingOutput? selectedInput;
    public ICollectionView FilteredTracks { get; private set; } = null!;
    public ObservableCollection<BackingOutput> Inputs { get; } = [];
    public string[] Keys { get; } = ["Não definido", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B", "Cm", "C#m", "Dm", "D#m", "Em", "Fm", "F#m", "Gm", "G#m", "Am", "A#m", "Bm"];
    public string[] PresetOptions { get; } = ["Sem vínculo", "Preset A", "Preset B", "Preset C"];
    public string Search { get => search; set { if (Set(ref search, value)) { FilteredTracks.Refresh(); Notify(nameof(SelectedTrack)); } } }
    public string Tuning
    {
        get => SelectedTrack?.Tuning ?? "";
        set { if (CanEditSession && SelectedTrack is { } track) { UpdateTrackMetadata(track, track.Bpm, track.OriginalKey, value, track.Preset); Notify(); } }
    }
    public double Bpm { get => SelectedTrack?.Bpm ?? 0; set { if (!CanEditSession || SelectedTrack == null || !double.IsFinite(value)) return; SelectedTrack.ManualBpm = true; SelectedTrack.Bpm = Math.Clamp(value, 20, 300); Notify(); Notify(nameof(CurrentBpm)); Notify(nameof(TransportBpm)); Notify(nameof(BpmBadge)); UpdateMetronome(); PersistSession(); } }
    public string CurrentBpm => $"{Bpm * Speed / 100:0.#} BPM atual · {SessionMeter}";
    public string OriginalKey { get => SelectedTrack?.OriginalKey ?? Keys[0]; set { if (SelectedTrack == null || !Keys.Contains(value)) return; SelectedTrack.ManualKey = true; SelectedTrack.OriginalKey = value; Notify(); Notify(nameof(CurrentKey)); Notify(nameof(KeyBadge)); PersistSession(); } }
    public string BpmBadge => SelectedTrack?.MusicPending == true && Bpm <= 0 ? "Analisando…" : Bpm > 0 ? $"BPM: {(SelectedTrack?.ManualBpm == true ? "" : "≈")}{Bpm * Speed / 100:0.#}" : "BPM não identificado";
    public string KeyBadge => OriginalKey == Keys[0] ? "Tom não identificado" : $"Tom: {(SelectedTrack?.ManualKey == true ? "" : "≈")}{OriginalKey}" + (Pitch != 0 ? $" → {CurrentKey}" : "");
    public string PresetBadge => LinkedPreset == 0 ? "Preset: —" : $"Preset: {(char)('A' + LinkedPreset - 1)}";
    public ICommand LowerPitch { get; private set; } = null!;
    public ICommand RaisePitch { get; private set; } = null!;
    public ICommand ZoomIn { get; private set; } = null!;
    public ICommand ZoomOut { get; private set; } = null!;
    public ICommand FitWaveform { get; private set; } = null!;
    public string CurrentKey => TransposeKey(OriginalKey, (int)Pitch);
    public static string TransposeKey(string key, int semitones)
    {
        string[] notes = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
        bool minor = key.EndsWith('m'); int note = Array.IndexOf(notes, minor ? key[..^1] : key);
        return note < 0 ? "Não definido" : notes[((note + semitones) % 12 + 12) % 12] + (minor ? "m" : "");
    }
    public string PresetHint => LinkedPreset == 0 ? "Sem preset associado" : $"Preset {(char)('A' + LinkedPreset - 1)} · selecione no footswitch";
    public int LinkedPreset { get => SelectedTrack?.Preset ?? 0; set { if (SelectedTrack == null) return; SelectedTrack.Preset = Math.Clamp(value, 0, 3); Notify(); Notify(nameof(PresetHint)); Notify(nameof(PresetBadge)); PersistSession(); } }
    public double Zoom { get => zoom; set { if (!double.IsFinite(value)) return; if (Set(ref zoom, Math.Clamp(value, 1, 16))) { Notify(nameof(ViewportDuration)); ViewportStart = viewportStart; } } }
    public double ViewportDuration => Duration / Zoom;
    public double ViewportStart { get => viewportStart; set { if (double.IsFinite(value)) Set(ref viewportStart, Math.Clamp(value, 0, Math.Max(0, Duration - ViewportDuration))); } }
    private void FollowWaveformPlayback()
    {
        if (IsRecording && !FollowRecording || !IsPlaying || Zoom <= 1 || ViewportDuration <= 0) return;
        // Advance in overlapping pages before the playhead reaches the edge.
        // Keeping the viewport still within a page avoids rebuilding wave geometry every tick.
        if (Position < ViewportStart || Position >= ViewportStart + ViewportDuration * .8)
            ViewportStart = Position - ViewportDuration * .2;
    }
    public ObservableCollection<BackingSection>? Sections => SelectedTrack?.Sections;
    private BackingSection? selectedSection;
    public BackingSection? SelectedSection { get => selectedSection; set { if (Set(ref selectedSection, value)) CommandManager.InvalidateRequerySuggested(); } }
    public string SectionName { get => sectionName; set => Set(ref sectionName, value); }
    public ICommand GoToA { get; private set; } = null!;
    public ICommand AddSection { get; private set; } = null!;
    public ICommand RemoveSection { get; private set; } = null!;
    public ICommand GoToSection { get; private set; } = null!;
    public ICommand ToggleMonitor { get; private set; } = null!;
    public ICommand RefreshInputs { get; private set; } = null!;
    private string? savedInputId;
    public ObservableCollection<BackingInputChoice> InputChoices { get; } = [];
    private void RefreshInputChoices()
    {
        InputChoices.Clear();
        if (SelectedInput == null) return;
        try
        {
            using var enumerator = new MMDeviceEnumerator(); using var device = enumerator.GetDevice(SelectedInput.Id);
            using var client = device.CreateAudioClient();
            int channels = client.MixFormat.Channels;
            for (int i = 0; i < channels; i++) { InputChoices.Add(new(i, 1)); if (i + 1 < channels) InputChoices.Add(new(i, 2)); }
        }
        catch (Exception ex) { Log(ex); }
    }
    public BackingOutput? SelectedInput { get => selectedInput; set { if (CanChangeRecordingInput && Set(ref selectedInput, value)) { savedInputId = value?.Id; if (SelectedTrack != null) SelectedTrack.CaptureEndpoint = savedInputId; RefreshInputChoices(); RestartMonitorForDeviceChange(); } } }
    private BackingMultitrackCapture? liveCapture;
    private bool suppressMonitorChanges;
    public bool IsMonitoring => liveCapture != null || recording != null;
    public double MonitorVolume { get => monitorVolume; set { if (!double.IsFinite(value)) return; Set(ref monitorVolume, Math.Clamp(value, 0, 100));  } }
    public void SelectRange(double start, double end)
    {
        if (!CanEditSession || SelectedTrack == null || !double.IsFinite(start) || !double.IsFinite(end) || start < 0 || end > Duration || end - start < .1 - 1e-9) return;
        SelectedTrack.Start = start; SelectedTrack.End = end;
        if (IsLooping && (position < start || position >= end)) { position = start; Notify(nameof(Position)); Notify(nameof(TimeLabel)); }
        RangeChanged();
    }
    private void InitializePractice()
    {
        meterTimer.Tick += (_, _) =>
        {
            TrackLevel = IsPlaying ? Math.Max((int)(TrackLevel * .65), (int)(Math.Clamp(engine.ConsumeOutputPeak(), 0, 1) * 100)) : 0;
            UpdateMixerInputPeak();
            UpdateMixerMeters();
        };
        PropertyChanged += (_, e) => { if (e.PropertyName is nameof(IsPlaying) or nameof(IsMonitoring)) RefreshMeters(); };
        LowerPitch = new UiCommand(() => Pitch--, () => SelectedTrack != null && Pitch > -6);
        RaisePitch = new UiCommand(() => Pitch++, () => SelectedTrack != null && Pitch < 6);
        ZoomIn = new UiCommand(() => Zoom *= 2, () => Duration > 0 && Zoom < 16);
        ZoomOut = new UiCommand(() => Zoom /= 2, () => Duration > 0 && Zoom > 1);
        FitWaveform = new UiCommand(() => { Zoom = 1; ViewportStart = 0; }, () => Duration > 0);
        FilteredTracks = new ListCollectionView(Tracks);
        FilteredTracks.Filter = item => item is BackingTrack track && (track.Title.Contains(Search, StringComparison.CurrentCultureIgnoreCase) || track.Tuning.Contains(Search, StringComparison.CurrentCultureIgnoreCase));
        GoToA = new UiCommand(() => Position = RangeStart, () => double.IsFinite(RangeStart));
        AddSection = new UiCommand(() => { var marker = new BackingSection(Position, SectionName.Trim()); int index = 0; while (index < Sections!.Count && Sections[index].Position <= Position) index++; Sections.Insert(index, marker); }, () => CanSeek && !string.IsNullOrWhiteSpace(SectionName) && SectionName.Length <= 40);
        RemoveSection = new UiCommand(() => { if (SelectedSection is { } section) Sections?.Remove(section); SelectedSection = null; }, () => SelectedSection != null);
        GoToSection = new UiCommand(() => { if (SelectedSection != null) Position = SelectedSection.Position; }, () => SelectedSection != null);
        ToggleMonitor = new UiCommand(() => { if (IsMonitoring) StopMonitor(true); else StartMonitor(); }, () => CanEditSession && (IsMonitoring || SelectedInput != null && SelectedOutput != null));
        RefreshInputs = new UiCommand(LoadInputs, () => CanChangeRecordingInput);
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Duration)) { Notify(nameof(ViewportDuration)); ViewportStart = viewportStart; }
            if (e.PropertyName is nameof(Position) or nameof(Zoom) or nameof(IsPlaying)) FollowWaveformPlayback();
        };
    }
    private void RefreshPractice()
    {
        Notify(nameof(Tuning));
        RefreshMetronome(); UpdateMetronome(false);
        Zoom = 1; ViewportStart = 0; SelectedSection = null;
        foreach (var name in new[] { nameof(BpmBadge), nameof(KeyBadge), nameof(PresetBadge), nameof(Bpm), nameof(CurrentBpm), nameof(OriginalKey), nameof(CurrentKey), nameof(LinkedPreset), nameof(PresetHint), nameof(Sections), nameof(SelectedSection) }) Notify(name);
    }
    private void LoadInputs()
    {
        if (!CanChangeRecordingInput) return;
        try
        {
            string? id = SelectedInput?.Id ?? savedInputId;
            Inputs.Clear();
            using var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                using (device) Inputs.Add(new(device.ID, device.FriendlyName));
            if (id != null && !Inputs.Any(x => x.Id == id)) Inputs.Add(new(id, Cube.Desktop.Localization.LanguageService.T("Dispositivo indisponível"), false));
            SelectedInput = Inputs.FirstOrDefault(x => x.Id == id);
        }
        catch (Exception ex) { Error("Não foi possível listar as entradas de áudio.", ex); }
    }
    private void RestartMonitorForDeviceChange()
    {
        if (IsRecording) return;
        StopMonitor();
        if (SessionStems?.Any(s => s.IsMonitoring) != true) return;
        if (SelectedInput?.Available == true && SelectedOutput != null) StartMonitor();
        else
        {
            StopMonitor(clearRoutes: true);
            Status = "Escolha uma entrada e saída disponíveis para monitorar.";
            HasError = true;
        }
    }
    private void StartMonitor()
    {
        if (SelectedInput == null || SelectedOutput == null || IsRecording) return;
        try
        {
            if (SessionStems?.Any(s => s.IsMonitoring) != true && SelectedStem != null) SelectedStem.IsMonitoring = true;
            if (liveCapture != null) return;
            liveCapture = new(SelectedInput.Id, CaptureRoutes(false));
            liveCapture.Start(); Start(engine.SetMonitor(SelectedOutput.Id, liveCapture.MonitorProvider));
            var current = liveCapture;
            _ = current.Completion.ContinueWith(task => Dispatch(() =>
            {
                if (liveCapture != current) return; StopMonitor(clearRoutes: true);
                if (task.Result != null) Error("Entrada de áudio desconectada.", task.Result);
            }));
            Notify(nameof(IsMonitoring));
        }
        catch (Exception ex) { StopMonitor(clearRoutes: true); Error("Não foi possível iniciar o monitor de entrada.", ex); }
    }
    private void StopMonitor(bool clearRoutes = false)
    {
        if (clearRoutes)
        {
            suppressMonitorChanges = true;
            try { foreach (var stem in SessionStems ?? []) stem.IsMonitoring = false; }
            finally { suppressMonitorChanges = false; Notify(nameof(MonitoredInputs)); }
        }
        var previous = liveCapture; liveCapture = null;
        Start(engine.SetMonitor(SelectedOutput?.Id ?? "", null));
        try { previous?.Dispose(); } catch (Exception ex) { Log(ex); }
        Notify(nameof(IsMonitoring));
    }
}
