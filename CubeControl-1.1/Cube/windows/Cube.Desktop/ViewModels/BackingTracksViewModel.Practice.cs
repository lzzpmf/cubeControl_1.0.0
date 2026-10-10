using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
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
    private readonly DispatcherTimer meterTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private BackingLevelMeter? inputMeter;
    private int trackLevel, inputLevel;
    public int TrackLevel { get => trackLevel; private set => Set(ref trackLevel, value); }
    public int InputLevel { get => inputLevel; private set => Set(ref inputLevel, value); }
    private void RefreshMeters()
    {
        bool active = IsPlaying || IsMonitoring;
        if (active) meterTimer.Start(); else meterTimer.Stop();
        if (!IsPlaying) { TrackLevel = 0; engine.ConsumeOutputPeak(); }
        if (!IsMonitoring) InputLevel = 0;
    }
    private string search = "", sectionName = "Intro";
    private double zoom = 1, viewportStart, monitorVolume = 0;
    private BackingOutput? selectedInput;
    private WasapiRecorder? monitorCapture;
    private WasapiPlayer? monitorOutput;
    private MMDevice? monitorInputDevice, monitorOutputDevice;
    private VolumeSampleProvider? monitorGain;
    public ICollectionView FilteredTracks { get; private set; } = null!;
    public ObservableCollection<BackingOutput> Inputs { get; } = [];
    public string[] Keys { get; } = ["Não definido", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B", "Cm", "C#m", "Dm", "D#m", "Em", "Fm", "F#m", "Gm", "G#m", "Am", "A#m", "Bm"];
    public string[] PresetOptions { get; } = ["Sem vínculo", "Preset A", "Preset B", "Preset C"];
    public string Search { get => search; set { if (Set(ref search, value)) { FilteredTracks.Refresh(); Notify(nameof(SelectedTrack)); } } }
    public double Bpm { get => SelectedTrack?.Bpm ?? 0; set { if (SelectedTrack == null || !double.IsFinite(value)) return; SelectedTrack.ManualBpm = true; SelectedTrack.Bpm = Math.Clamp(value, 20, 300); Notify(); Notify(nameof(CurrentBpm)); Notify(nameof(BpmBadge)); UpdateMetronome(); } }
    public string CurrentBpm => $"{Bpm * Speed / 100:0.#} BPM atual · grade 4/4";
    public string OriginalKey { get => SelectedTrack?.OriginalKey ?? Keys[0]; set { if (SelectedTrack == null || !Keys.Contains(value)) return; SelectedTrack.ManualKey = true; SelectedTrack.OriginalKey = value; Notify(); Notify(nameof(CurrentKey)); Notify(nameof(KeyBadge)); } }
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
    public int LinkedPreset { get => SelectedTrack?.Preset ?? 0; set { if (SelectedTrack == null) return; SelectedTrack.Preset = Math.Clamp(value, 0, 3); Notify(); Notify(nameof(PresetHint)); Notify(nameof(PresetBadge)); } }
    public double Zoom { get => zoom; set { if (!double.IsFinite(value)) return; if (Set(ref zoom, Math.Clamp(value, 1, 16))) { Notify(nameof(ViewportDuration)); ViewportStart = viewportStart; } } }
    public double ViewportDuration => Duration / Zoom;
    public double ViewportStart { get => viewportStart; set { if (double.IsFinite(value)) Set(ref viewportStart, Math.Clamp(value, 0, Math.Max(0, Duration - ViewportDuration))); } }
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
    public BackingOutput? SelectedInput { get => selectedInput; set { if (Set(ref selectedInput, value)) StopMonitor(); } }
    public bool IsMonitoring => monitorCapture != null;
    public double MonitorVolume { get => monitorVolume; set { if (!double.IsFinite(value)) return; Set(ref monitorVolume, Math.Clamp(value, 0, 100)); if (monitorGain != null) monitorGain.Volume = (float)(monitorVolume / 100); } }
    public void SelectRange(double start, double end)
    {
        if (SelectedTrack == null || !double.IsFinite(start) || !double.IsFinite(end) || start < 0 || end > Duration || end - start < .1 - 1e-9) return;
        SelectedTrack.Start = start; SelectedTrack.End = end;
        if (IsLooping && (position < start || position >= end)) { position = start; Notify(nameof(Position)); Notify(nameof(TimeLabel)); }
        RangeChanged();
    }
    private void InitializePractice()
    {
        meterTimer.Tick += (_, _) =>
        {
            TrackLevel = IsPlaying ? Math.Max((int)(TrackLevel * .65), (int)(Math.Clamp(engine.ConsumeOutputPeak(), 0, 1) * 100)) : 0;
            InputLevel = IsMonitoring ? Math.Max((int)(InputLevel * .65), (int)(Math.Clamp(inputMeter?.ConsumePeak() ?? 0, 0, 1) * 100)) : 0;
        };
        PropertyChanged += (_, e) => { if (e.PropertyName is nameof(IsPlaying) or nameof(IsMonitoring)) RefreshMeters(); };
        LowerPitch = new UiCommand(() => Pitch--, () => SelectedTrack != null && Pitch > -6);
        RaisePitch = new UiCommand(() => Pitch++, () => SelectedTrack != null && Pitch < 6);
        ZoomIn = new UiCommand(() => Zoom *= 2, () => CanSeek && Zoom < 16);
        ZoomOut = new UiCommand(() => Zoom /= 2, () => CanSeek && Zoom > 1);
        FitWaveform = new UiCommand(() => { Zoom = 1; ViewportStart = 0; }, () => CanSeek);
        FilteredTracks = new ListCollectionView(Tracks);
        FilteredTracks.Filter = item => item is BackingTrack track && (track.Title.Contains(Search, StringComparison.CurrentCultureIgnoreCase) || track.Tuning.Contains(Search, StringComparison.CurrentCultureIgnoreCase));
        GoToA = new UiCommand(() => Position = RangeStart, () => double.IsFinite(RangeStart));
        AddSection = new UiCommand(() => { var marker = new BackingSection(Position, SectionName.Trim()); int index = 0; while (index < Sections!.Count && Sections[index].Position <= Position) index++; Sections.Insert(index, marker); }, () => CanSeek && !string.IsNullOrWhiteSpace(SectionName) && SectionName.Length <= 40);
        RemoveSection = new UiCommand(() => { if (SelectedSection is { } section) Sections?.Remove(section); SelectedSection = null; }, () => SelectedSection != null);
        GoToSection = new UiCommand(() => { if (SelectedSection != null) Position = SelectedSection.Position; }, () => SelectedSection != null);
        ToggleMonitor = new UiCommand(() => { if (IsMonitoring) StopMonitor(); else StartMonitor(); }, () => !IsRecordingMix && !recordingFinalizing && (IsMonitoring || SelectedInput != null && SelectedOutput != null));
        RefreshInputs = new UiCommand(LoadInputs);
        PropertyChanged += (_, e) => { if (e.PropertyName == nameof(Duration)) { Notify(nameof(ViewportDuration)); ViewportStart = viewportStart; } };
    }
    private void RefreshPractice()
    {
        RefreshMetronome(); UpdateMetronome(false);
        Zoom = 1; ViewportStart = 0; SelectedSection = null;
        foreach (var name in new[] { nameof(BpmBadge), nameof(KeyBadge), nameof(PresetBadge), nameof(Bpm), nameof(CurrentBpm), nameof(OriginalKey), nameof(CurrentKey), nameof(LinkedPreset), nameof(PresetHint), nameof(Sections), nameof(SelectedSection) }) Notify(name);
    }
    private void LoadInputs()
    {
        try
        {
            string? id = SelectedInput?.Id;
            Inputs.Clear();
            using var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                using (device) Inputs.Add(new(device.ID, device.FriendlyName));
            SelectedInput = Inputs.FirstOrDefault(x => x.Id == id);
        }
        catch (Exception ex) { Error("Não foi possível listar as entradas de áudio.", ex); }
    }
    private void StartMonitor()
    {
        if (SelectedInput == null || SelectedOutput == null) return;
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            monitorInputDevice = enumerator.GetDevice(SelectedInput.Id);
            monitorOutputDevice = enumerator.GetDevice(SelectedOutput.Id);
            monitorCapture = new WasapiRecorderBuilder().WithDevice(monitorInputDevice).Build();
            var capture = monitorCapture;
            var buffer = new BufferedWaveProvider(capture.WaveFormat, TimeSpan.FromMilliseconds(200)) { DiscardOnBufferOverflow = true };
            monitorGain = new VolumeSampleProvider(buffer.ToSampleProvider()) { Volume = (float)(MonitorVolume / 100) };
            monitorOutput = new WasapiPlayerBuilder().WithDevice(monitorOutputDevice).WithSharedMode().WithLatency(40).Build();
            inputMeter = new BackingLevelMeter(monitorGain);
            monitorOutput.Init(new SampleToWaveProvider(inputMeter));
            byte[] silence = [];
            capture.DataAvailable += (data, flags, devicePosition, qpcPosition) =>
            {
                if ((flags & AudioClientBufferFlags.Silent) != 0)
                { if (silence.Length < data.Length) silence = new byte[data.Length]; buffer.AddSamples(silence.AsSpan(0, data.Length)); }
                else buffer.AddSamples(data);
            };
            capture.RecordingStopped += (_, e) => Dispatch(() => { if (monitorCapture != capture) return; StopMonitor(); if (e.Exception != null) Error("Entrada de áudio desconectada.", e.Exception); });
            monitorOutput.PlaybackStopped += (_, e) => Dispatch(() => { if (monitorCapture != capture) return; StopMonitor(); if (e.Exception != null) Error("Monitor de entrada interrompido.", e.Exception); });
            capture.StartRecording(); monitorOutput.Play(); Notify(nameof(IsMonitoring));
        }
        catch (Exception ex) { StopMonitor(); Error("Não foi possível iniciar o monitor de entrada.", ex); }
    }
    private void StopMonitor()
    {
        if (IsRecordingMix && recording is { } session) Start(session.Stop(new InvalidOperationException("Guitar monitor interrupted during recording.")));
        var capture = monitorCapture; monitorCapture = null;
        // Release every endpoint even when a disconnected device throws during teardown.
        foreach (IDisposable? resource in new IDisposable?[] { capture, monitorOutput, monitorInputDevice, monitorOutputDevice })
            try { resource?.Dispose(); } catch (Exception ex) { Log(ex); }
        monitorOutput = null; monitorGain = null; inputMeter = null; monitorInputDevice = monitorOutputDevice = null;
        Notify(nameof(IsMonitoring));
    }
}
