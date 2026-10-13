using System.Diagnostics;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private bool monitorMuted, metronomeMuted;
    private readonly BackingMeterEnvelope trackEnvelope = new(), guitarEnvelope = new(), clickEnvelope = new();
    private BackingMeterReading trackMeter = BackingMeterReading.Silent, guitarMeter = BackingMeterReading.Silent, clickMeter = BackingMeterReading.Silent;
    public BackingMeterReading TrackMeter { get => trackMeter; private set => Set(ref trackMeter, value); }
    public BackingMeterReading GuitarMeter { get => guitarMeter; private set => Set(ref guitarMeter, value); }
    public BackingMeterReading ClickMeter { get => clickMeter; private set => Set(ref clickMeter, value); }
    private void UpdateMixerMeters()
    {
        double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
        TrackMeter = trackEnvelope.Update(engine.ConsumeTrackPeak(), IsPlaying, now);
        GuitarMeter = guitarEnvelope.Update(monitorMeter?.ConsumePeak() ?? 0, IsMonitoring, now);
        float synced = engine.ConsumeClickPeak(), independent = metronomeLevelMeter?.ConsumePeak() ?? 0;
        ClickMeter = clickEnvelope.Update(FollowTrack ? synced : independent,
            metronomeEnabled && (FollowTrack ? IsPlaying : metronomeOutput != null), now);
    }
    public bool MonitorMuted
    {
        get => monitorMuted;
        set { if (Set(ref monitorMuted, value) && monitorGain != null) monitorGain.Volume = value ? 0 : (float)(MonitorVolume / 100); }
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
        float peak = inputMeter?.ConsumePeak() ?? 0;
        double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
        if (IsMonitoring && peak >= 1) lastInputClip = now;
        InputClipping = IsMonitoring && now - lastInputClip < 1;
        InputLevel = IsMonitoring ? (int)(Math.Clamp(peak, 0, 1) * 100) : 0;
    }
    public void ResetMixerVolumes()
    {
        Volume = 30; MonitorVolume = 0; MetronomeVolume = 30;
    }
}
