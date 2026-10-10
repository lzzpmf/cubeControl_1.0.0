namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private bool monitorMuted, metronomeMuted, mixerExpanded;
    public bool MixerExpanded { get => mixerExpanded; set => Set(ref mixerExpanded, value); }
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
    private DateTime lastInputClip;
    public bool InputClipping { get => inputClipping; private set => Set(ref inputClipping, value); }
    private void UpdateMixerInputPeak()
    {
        float peak = inputMeter?.ConsumePeak() ?? 0;
        if (IsMonitoring && peak >= 1) lastInputClip = DateTime.UtcNow;
        InputClipping = IsMonitoring && DateTime.UtcNow - lastInputClip < TimeSpan.FromSeconds(1);
        InputLevel = IsMonitoring ? Math.Max((int)(InputLevel * .65), (int)(Math.Clamp(peak, 0, 1) * 100)) : 0;
    }
    public void ResetMixerVolumes()
    {
        Volume = 30; MonitorVolume = 0; MetronomeVolume = 30;
    }
}
