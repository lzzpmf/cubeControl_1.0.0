using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private sealed record MetronomePreferences(int Version = 1, bool Follow = false, double Bpm = 120,
        string Meter = "4/4", bool Accent = true, double Volume = 30, int CountIn = 0,
        Dictionary<string, double>? Offsets = null, Dictionary<string, double>? TrackBpms = null, string Sound = "Click", string[]? CustomFiles = null);
    public string[] BuiltInMetronomeSounds { get; } = ["Click", "Bell", "Ping", "Woodblock", "Hi-hat"];
    public System.Collections.ObjectModel.ObservableCollection<string> CustomMetronomeFiles { get; } = new();
    private string metronomeSound = "Click";
    private MetronomeSoundBuffer? customMetronomeSound;
    public string MetronomeSound => metronomeSound;
    public void SelectMetronomeSound(string sound)
    {
        try
        {
            var buffer = BuiltInMetronomeSounds.Contains(sound) ? null : MetronomeSoundBuffer.Load(sound);
            customMetronomeSound = buffer; metronomeSound = sound; Notify(nameof(MetronomeSound)); UpdateMetronome();
        }
        catch (Exception ex) { Error("Não foi possível carregar o som do metrônomo.", ex); }
    }
    public async Task ImportMetronomeSound(string path)
    {
        try
        {
            var buffer = await Task.Run(() => MetronomeSoundBuffer.Load(path));
            if (disposed) return;
            string directory = Path.Combine(Path.GetDirectoryName(metronomePath)!, "metronome-sounds");
            Directory.CreateDirectory(directory);
            string target = Path.Combine(directory, Path.GetFileNameWithoutExtension(path) + "-" + Guid.NewGuid().ToString("N")[..8] + ".wav");
            File.Copy(path, target);
            CustomMetronomeFiles.Add(target); customMetronomeSound = buffer; metronomeSound = target;
            Notify(nameof(MetronomeSound)); UpdateMetronome();
        }
        catch (Exception ex) { Error("Não foi possível importar o som. Use WAV mono ou estéreo de até 2 segundos.", ex); }
    }
    private string metronomePath = "";
    private readonly Dictionary<string, double> metronomeOffsets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> metronomeTrackBpms = new(StringComparer.OrdinalIgnoreCase);
    private readonly MetronomeTapTempo tapTempo = new();
    private readonly DispatcherTimer metronomeTimer = new() { Interval = TimeSpan.FromMilliseconds(40) };
    private MMDeviceEnumerator? metronomeEnumerator;
    private MMDevice? metronomeDevice;
    private WasapiPlayer? metronomeOutput;
    private MetronomeSampleProvider? metronomeProvider;
    private bool followTrack, metronomeEnabled, metronomeAccent = true;
    private double metronomeBpm = 120, metronomeVolume = 30;
    private string metronomeMeter = "4/4";
    private int countInBars, metronomeBeat;
    private int metronomeGeneration;
    private bool metronomeStarting;
    public ICommand ToggleMetronome { get; private set; } = null!;
    public ICommand TapMetronome { get; private set; } = null!;
    public ICommand MarkFirstBeat { get; private set; } = null!;
    public string[] MetronomeMeters { get; } = ["2/4", "3/4", "4/4", "6/8"];
    public int[] MetronomeCountIns { get; } = [0, 1, 2];
    public bool FollowTrack
    {
        get => followTrack;
        set { if (!Set(ref followTrack, value)) return; metronomeGeneration++; StopIndependentMetronome(); metronomeEnabled = false; RefreshMetronome(); UpdateMetronome(); }
    }
    public bool MetronomeEnabled => metronomeEnabled;
    public string MetronomeToggleLabel => LanguageService.T(metronomeEnabled ? "Desligar metrônomo" : "Ligar metrônomo");
    public double MetronomeBpm
    {
        get => FollowTrack ? Bpm : metronomeBpm;
        set { if (!double.IsFinite(value)) return; if (FollowTrack) Bpm = value; else metronomeBpm = Math.Clamp(value, 20, 300); RefreshMetronome(); UpdateMetronome(); }
    }
    public double MetronomeVolume { get => metronomeVolume; set { if (double.IsFinite(value) && Set(ref metronomeVolume, Math.Clamp(value, 0, 100))) UpdateMetronome(); } }
    public string MetronomeMeter { get => metronomeMeter; set { if (MetronomeMeters.Contains(value) && Set(ref metronomeMeter, value)) UpdateMetronome(); } }
    public bool MetronomeAccent { get => metronomeAccent; set { if (Set(ref metronomeAccent, value)) UpdateMetronome(); } }
    public int CountInBars { get => countInBars; set { if (value is >= 0 and <= 2 && Set(ref countInBars, value)) UpdateMetronome(); } }
    public double FirstBeatOffset
    {
        get => SelectedTrack != null && metronomeOffsets.TryGetValue(SelectedTrack.Source, out double value) ? value : 0;
        set { if (SelectedTrack == null || !double.IsFinite(value)) return; metronomeOffsets[SelectedTrack.Source] = Math.Clamp(value, 0, Math.Max(0, Duration)); Notify(); UpdateMetronome(); }
    }
    public bool[] MetronomeBeatMarkers => Enumerable.Range(1, int.Parse(MetronomeMeter[..1])).Select(x => x == MetronomeBeat).ToArray();
    public int MetronomeBeat { get => metronomeBeat; private set { if (Set(ref metronomeBeat, value)) Notify(nameof(MetronomeBeatMarkers)); } }
    public string MetronomeStatus => LanguageService.T(engine.IsCountingIn && FollowTrack ? "Contagem inicial" : metronomeEnabled && (FollowTrack ? IsPlaying : metronomeOutput != null) ? "Tocando" : metronomeEnabled ? "Pronto" : "Desligado");
    public string MetronomeTempoLabel => $"{(FollowTrack ? MetronomeBpm * Speed / 100 : MetronomeBpm):0.#} BPM";
    private MetronomeSettings MetronomeConfiguration(bool independent = false) => new(
        metronomeEnabled, MetronomeBpm > 0 ? MetronomeBpm : 120,
        int.Parse(MetronomeMeter[..1]), MetronomeMeter == "6/8", MetronomeAccent, MetronomeVolume / 100,
        independent ? 0 : FirstBeatOffset, CountInBars, MetronomeSound, customMetronomeSound);

    private void InitializeMetronome(string directory)
    {
        metronomePath = Path.Combine(directory, "backing-metronome-v1.json");
        try
        {
            if (File.Exists(metronomePath) && JsonSerializer.Deserialize<MetronomePreferences>(File.ReadAllText(metronomePath)) is { Version: 1 } saved)
            {
                foreach (string file in saved.CustomFiles ?? []) CustomMetronomeFiles.Add(file);
                try
                {
                    customMetronomeSound = BuiltInMetronomeSounds.Contains(saved.Sound) ? null : MetronomeSoundBuffer.Load(saved.Sound);
                    metronomeSound = saved.Sound;
                }
                catch (Exception ex) { Error("Som do metrônomo indisponível. Click selecionado.", ex); }
                followTrack = saved.Follow;
                metronomeBpm = double.IsFinite(saved.Bpm) ? Math.Clamp(saved.Bpm, 20, 300) : 120;
                metronomeMeter = MetronomeMeters.Contains(saved.Meter) ? saved.Meter : "4/4";
                metronomeAccent = saved.Accent; metronomeVolume = double.IsFinite(saved.Volume) ? Math.Clamp(saved.Volume, 0, 100) : 30;
                countInBars = Math.Clamp(saved.CountIn, 0, 2);
                foreach (var entry in saved.Offsets ?? []) if (double.IsFinite(entry.Value) && entry.Value >= 0) metronomeOffsets[entry.Key] = entry.Value;
                foreach (var entry in saved.TrackBpms ?? []) if (double.IsFinite(entry.Value) && entry.Value is >= 20 and <= 300) metronomeTrackBpms[entry.Key] = entry.Value;
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { Log(ex); }
        ToggleMetronome = new UiCommand(() => Start(ToggleMetronomePlayback()), () => !metronomeStarting && (metronomeEnabled || SelectedOutput != null));
        TapMetronome = new UiCommand(() => { if (tapTempo.Tap(Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency) is double bpm) MetronomeBpm = bpm; }, () => !FollowTrack || SelectedTrack != null);
        MarkFirstBeat = new UiCommand(() => FirstBeatOffset = Position, () => CanSeek);
        metronomeTimer.Tick += (_, _) =>
        {
            try { MetronomeBeat = !metronomeEnabled ? 0 : FollowTrack ? (IsPlaying ? engine.MetronomeBeat : 0)
                : metronomeOutput == null ? 0 : MetronomeConfiguration(true).BeatAt(metronomeOutput.GetPosition() / (double)metronomeOutput.OutputWaveFormat.AverageBytesPerSecond);
            }
            catch (Exception ex) { MetronomeOutputChanged(); Error("Metrônomo interrompido. Verifique a saída de áudio.", ex); }
            Notify(nameof(MetronomeStatus));
        };
    }
    private void SaveMetronome()
    {
        if (metronomePath.Length == 0) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(metronomePath)!);
            string temporary = metronomePath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(new MetronomePreferences(1, FollowTrack, metronomeBpm,
                MetronomeMeter, MetronomeAccent, MetronomeVolume, CountInBars, metronomeOffsets, metronomeTrackBpms, MetronomeSound, CustomMetronomeFiles.ToArray())));
            File.Move(temporary, metronomePath, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Error("Não foi possível salvar o metrônomo.", ex); }
    }
    private void RefreshMetronome()
    {
        if (metronomeEnabled) metronomeTimer.Start(); else { metronomeTimer.Stop(); MetronomeBeat = 0; }
        foreach (string name in new[] { nameof(MetronomeBeatMarkers), nameof(MetronomeEnabled), nameof(MetronomeToggleLabel), nameof(MetronomeBpm), nameof(MetronomeTempoLabel), nameof(FirstBeatOffset), nameof(MetronomeStatus) }) Notify(name);
        CommandManager.InvalidateRequerySuggested();
    }
    private void RestoreTrackMetronome()
    {
        if (SelectedTrack != null && metronomeTrackBpms.TryGetValue(SelectedTrack.Source, out double saved)) { SelectedTrack.Bpm = saved; SelectedTrack.ManualBpm = true; }
        RefreshMetronome(); UpdateMetronome(false);
    }
    private void UpdateMetronome(bool persist = true)
    {
        if (disposed) return;
        if (SelectedTrack is { ManualBpm: true } track) metronomeTrackBpms[track.Source] = track.Bpm;
        if (persist) SaveMetronome();
        Start(engine.ConfigureMetronome(MetronomeConfiguration() with { Enabled = FollowTrack && metronomeEnabled && Bpm > 0 }));
        metronomeProvider?.Configure(MetronomeConfiguration(true), IsMuted);
        RefreshMetronome();
    }
    private async Task ToggleMetronomePlayback()
    {
        if (metronomeEnabled) { metronomeEnabled = false; StopIndependentMetronome(); UpdateMetronome(); return; }
        if (FollowTrack && (SelectedTrack == null || Bpm <= 0)) { Status = "Defina o BPM da faixa para ligar o metrônomo."; return; }
        if (SelectedOutput == null) { Status = "Escolha a saída de áudio"; return; }
        if (metronomeStarting) return;
        int request = metronomeGeneration;
        var endpoint = SelectedOutput;
        metronomeStarting = true; RefreshMetronome();
        try { if (BeforePlayback != null) await BeforePlayback(); }
        finally { metronomeStarting = false; RefreshMetronome(); }
        if (disposed || SelectedOutput != endpoint || metronomeGeneration != request) return;
        metronomeEnabled = true;
        if (!FollowTrack)
        {
            try
            {
                metronomeEnumerator = new MMDeviceEnumerator(); metronomeDevice = metronomeEnumerator.GetDevice(SelectedOutput.Id);
                metronomeProvider = new(MetronomeConfiguration(true)); metronomeProvider.Configure(MetronomeConfiguration(true), IsMuted);
                metronomeOutput = new WasapiPlayerBuilder().WithDevice(metronomeDevice).WithSharedMode().WithEventSync().WithLatency(100).Build();
                var current = metronomeOutput;
                current.PlaybackStopped += (_, args) => Dispatch(() => { if (metronomeOutput != current) return; StopIndependentMetronome(); metronomeEnabled = false; RefreshMetronome(); if (args.Exception != null) Error("Metrônomo interrompido. Verifique a saída de áudio.", args.Exception); });
                metronomeOutput.Init(new SampleToWaveProvider(metronomeProvider)); metronomeOutput.Play();
            }
            catch { metronomeEnabled = false; StopIndependentMetronome(); RefreshMetronome(); throw; }
        }
        UpdateMetronome();
    }
    private void StopIndependentMetronome()
    {
        var previous = metronomeOutput; metronomeOutput = null;
        try { previous?.Stop(); } catch (Exception ex) { Log(ex); }
        foreach (IDisposable? resource in new IDisposable?[] { previous, metronomeDevice, metronomeEnumerator })
            try { resource?.Dispose(); } catch (Exception ex) { Log(ex); }
        metronomeProvider = null; metronomeDevice = null; metronomeEnumerator = null; MetronomeBeat = 0;
    }
    private void MetronomeOutputChanged()
    {
        metronomeGeneration++; StopIndependentMetronome(); metronomeEnabled = false; UpdateMetronome(false);
    }
}
