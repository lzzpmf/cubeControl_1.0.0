using System.Diagnostics;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private double masterVolume = 1;
    public double MasterDb { get => BackingMixMath.ToDb(masterVolume); set { if (!CanAdjustMix || !double.IsFinite(value)) return; masterVolume = BackingMixMath.FromDb(value); if (SelectedTrack != null) SelectedTrack.MasterGain = masterVolume; Notify(); Start(engine.SetMasterVolume(masterVolume)); PersistSession(); } }
    public double AccompanimentDb { get => BackingMixMath.ToDb(Volume / 100); set => Volume = BackingMixMath.FromDb(value) * 100; }
    public double ClickDb { get => BackingMixMath.ToDb(MetronomeVolume / 100); set => MetronomeVolume = BackingMixMath.FromDb(value) * 100; }
    private BackingMeterReading masterReading = BackingMeterReading.Silent;
    public BackingMeterReading MasterMeter { get => masterReading; private set => Set(ref masterReading, value); }
    private double clickRmsDb = -60;
    public double ClickRmsDb { get => clickRmsDb; private set => Set(ref clickRmsDb, value); }
    private double masterRmsDb = -60;
    public double MasterRmsDb { get => masterRmsDb; private set => Set(ref masterRmsDb, value); }
    private readonly BackingMeterEnvelope masterEnvelope = new();
    private readonly Dictionary<Guid, BackingMeterEnvelope> stemEnvelopes = [], inputEnvelopes = [];
    private bool monitorMuted, metronomeMuted;
    private readonly BackingMeterEnvelope trackEnvelope = new(), guitarEnvelope = new(), clickEnvelope = new();
    private BackingMeterReading trackMeter = BackingMeterReading.Silent, guitarMeter = BackingMeterReading.Silent, clickMeter = BackingMeterReading.Silent;
    public BackingMeterReading TrackMeter { get => trackMeter; private set => Set(ref trackMeter, value); }
    public BackingMeterReading GuitarMeter { get => guitarMeter; private set => Set(ref guitarMeter, value); }
    public BackingMeterReading ClickMeter { get => clickMeter; private set => Set(ref clickMeter, value); }
    private void UpdateMixerMeters()
    {
        double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
        var master = engine.ConsumeMasterMeter();
        MasterMeter = masterEnvelope.Update(master.Peak, IsPlaying || IsMonitoring || metronomeEnabled, now);
        MasterRmsDb = IsPlaying || IsMonitoring || metronomeEnabled ? (master.Rms > 0 ? Math.Max(-60, 20 * Math.Log10(master.Rms)) : -60) : -60;
        foreach (var stem in SessionStems ?? [])
        {
            if (!stemEnvelopes.TryGetValue(stem.Id, out var envelope)) stemEnvelopes[stem.Id] = envelope = new();
            var signal = engine.StemMeters.GetValueOrDefault(stem.Id)?.Consume() ?? (Peak: 0f, Rms: 0f);
            var captured = (recording ?? liveCapture)?.ConsumeMeters(stem.Id) ?? default;
            if (!inputEnvelopes.TryGetValue(stem.Id, out var inputEnvelope)) inputEnvelopes[stem.Id] = inputEnvelope = new();
            stem.InputMeter = inputEnvelope.Update(captured.Input.Peak, IsMonitoring, now);
            stem.InputRmsDb = IsMonitoring && captured.Input.Rms > 0 ? Math.Max(-60, 20 * Math.Log10(captured.Input.Rms)) : -60;
            if (stem.IsMonitoring || stem.IsArmed) signal = captured.Input;
            stem.Meter = envelope.Update(signal.Peak, IsPlaying || stem.IsMonitoring || stem.IsArmed, now);
            stem.RmsDb = IsPlaying || stem.IsMonitoring ? (signal.Rms > 0 ? Math.Max(-60, 20 * Math.Log10(signal.Rms)) : -60) : -60;
        }
        TrackMeter = trackEnvelope.Update(engine.ConsumeTrackPeak(), IsPlaying, now);
        GuitarMeter = guitarEnvelope.Update(0, IsMonitoring, now);
        var clickSignal = engine.ConsumeClickSignal();
        ClickRmsDb = metronomeEnabled && clickSignal.Rms > 0 ? Math.Max(-60, 20 * Math.Log10(clickSignal.Rms)) : -60;
        float synced = engine.ConsumeClickPeak(), independent = synced;
        ClickMeter = clickEnvelope.Update(FollowTrack ? synced : independent,
            metronomeEnabled && (FollowTrack ? IsPlaying : metronomeProvider != null), now);
    }
    public bool MonitorMuted
    {
        get => monitorMuted;
        set { Set(ref monitorMuted, value); }
    }
    public bool MetronomeMuted
    {
        get => metronomeMuted;
        set { if (Set(ref metronomeMuted, value)) UpdateMetronome(false); }
    }
    private bool inputClipping;
    private double lastInputClip = double.NegativeInfinity;
    public bool InputClipping { get => inputClipping; private set => Set(ref inputClipping, value); }
    private void UpdateMixerInputPeak()
    {
        float peak = (recording ?? liveCapture)?.InputPeak.Consume() ?? 0;
        double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
        if (IsMonitoring && peak >= 1) lastInputClip = now;
        InputClipping = IsMonitoring && now - lastInputClip < 1;
        InputLevel = IsMonitoring ? (int)(Math.Clamp(peak, 0, 1) * 100) : 0;
    }
    public void ResetMixerVolumes()
    {
        Volume = 30; MonitorVolume = 0; MetronomeVolume = 100;
    }
}
