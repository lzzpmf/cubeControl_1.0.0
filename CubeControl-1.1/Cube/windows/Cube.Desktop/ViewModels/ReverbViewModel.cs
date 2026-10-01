using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class ReverbViewModel : Observable
{
    private bool loading, loaded;
    private double dry = 100, wet = 55, decay = 8, returnLevel = 100, damping = 10;
    private string identity = "", message = "Experimental · validação sonora pendente. Ative o reverb e desligue a modulação antes de ler.";
    public event Action? Edited;
    public bool Loaded { get => loaded; private set => Set(ref loaded, value); }
    public string Identity => identity;
    public string Message { get => message; set => Set(ref message, value); }
    public double Dry { get => dry; set { if (Set(ref dry, Math.Clamp(value, 0, 100))) Changed(); } }
    public double Wet { get => wet; set { if (Set(ref wet, Math.Clamp(value, 0, 90))) Changed(); } }
    public double Decay { get => decay; set { if (Set(ref decay, Math.Clamp(Math.Round(value), 1, 15))) Changed(); } }
    public double ReturnLevel { get => returnLevel; set { if (Set(ref returnLevel, Math.Clamp(value, 0, 100))) Changed(); } }
    public double Damping { get => damping; set { if (Set(ref damping, Math.Clamp(Math.Round(value), 0, 10))) Changed(); } }
    public ReverbParameters Parameters => new((float)(Dry / 100), (float)(Wet / 100), (int)Decay,
        (int)Math.Round(ReturnLevel * 0.8), (int)Damping);
    private void Changed() { if (!loading) Edited?.Invoke(); }
    public void Load(string deviceIdentity, ReverbParameters p)
    {
        p.Validate(); loading = true;
        try
        {
            identity = deviceIdentity; dry = p.Dry * 100d; wet = p.Wet * 100d;
            decay = p.Decay; returnLevel = p.ReturnLevel / 0.8; damping = p.Damping;
            foreach (string property in new[] { nameof(Dry), nameof(Wet), nameof(Decay), nameof(ReturnLevel), nameof(Damping), nameof(Identity) }) Notify(property);
            Loaded = true;
        }
        finally { loading = false; }
    }
}
