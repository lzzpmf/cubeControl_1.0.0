using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class ChorusViewModel : Observable
{
    private bool loading, loaded;
    private double depth = 100, speed = 1.488, mix = 100, feedback, wetGain = 100, combinedGain = 100;
    private int waveform;
    private string identity = "", message = "Experimental · validação sonora pendente. Ative o chorus no preset físico antes de ler.";
    public event Action? Edited;
    public bool Loaded { get => loaded; private set => Set(ref loaded, value); }
    public string Identity => identity;
    public string Message { get => message; set => Set(ref message, value); }
    public double Depth { get => depth; set { if (double.IsFinite(value) && Set(ref depth, Math.Clamp(value, 0, 100))) Changed(); } }
    public double Speed { get => speed; set { if (double.IsFinite(value) && Set(ref speed, Math.Clamp(value, 0.384, 3.024))) Changed(); } }
    public double Mix { get => mix; set { if (double.IsFinite(value) && Set(ref mix, Math.Clamp(value, 0, 100))) Changed(); } }
    public double Feedback { get => feedback; set { if (double.IsFinite(value) && Set(ref feedback, Math.Clamp(value, 0, 10))) Changed(); } }
    public int Waveform { get => waveform; set { if (value is >= 0 and <= 1 && Set(ref waveform, value)) Changed(); } }
    public double WetGain { get => wetGain; set { if (double.IsFinite(value) && Set(ref wetGain, Math.Clamp(value, 50, 100))) Changed(); } }
    public double CombinedGain { get => combinedGain; set { if (double.IsFinite(value) && Set(ref combinedGain, Math.Clamp(value, 50, 100))) Changed(); } }
    public ChorusParameters Parameters => new(Math.Clamp((float)(Depth / 100 * 0.4), 0, 0.4f),
        Math.Clamp((float)(Speed / 48000), 0.000008f, 0.000063f), Math.Clamp((float)(Mix / 100 * 0.85), 0, 0.85f),
        Math.Clamp((float)(Feedback / 100), 0, 0.1f), Waveform, Math.Clamp((float)(WetGain / 100), 0.5f, 1),
        Math.Clamp((float)(CombinedGain / 100), 0.5f, 1));
    private void Changed() { if (!loading) Edited?.Invoke(); }
    public void Load(string deviceIdentity, ChorusParameters parameters)
    {
        parameters.Validate(); loading = true;
        try
        {
            identity = deviceIdentity;
            depth = parameters.Depth / (double)0.4f * 100;
            speed = parameters.Step * 48000d; mix = parameters.Mix / (double)0.85f * 100;
            feedback = parameters.Feedback * 100d; waveform = parameters.Waveform;
            wetGain = parameters.WetGain * 100d; combinedGain = parameters.CombinedGain * 100d;
            foreach (string property in new[] { nameof(Depth), nameof(Speed), nameof(Mix), nameof(Feedback), nameof(Waveform), nameof(WetGain), nameof(CombinedGain), nameof(Identity) }) Notify(property);
            Loaded = true;
        }
        finally { loading = false; }
    }
}
