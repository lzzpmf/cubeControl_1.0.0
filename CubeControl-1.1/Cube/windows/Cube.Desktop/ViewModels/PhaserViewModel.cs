using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class PhaserViewModel : Observable
{
    private bool loading, loaded;
    private double speed = 0.48, lower = 300, upper = 3000, level = 100, sections = 3;
    private string identity = "", message = "Experimental · validação sonora pendente. Ative o phaser no preset físico antes de ler.";
    public event Action? Edited;
    public bool Loaded { get => loaded; private set => Set(ref loaded, value); }
    public string Identity => identity;
    public string Message { get => message; set => Set(ref message, value); }
    public double Speed { get => speed; set { if (Set(ref speed, Math.Clamp(value, 0.48, 6.24))) Changed(); } }
    public double Lower { get => lower; set { if (Set(ref lower, Math.Clamp(value, 300, upper))) Changed(); } }
    public double Upper { get => upper; set { if (Set(ref upper, Math.Clamp(value, lower, 3000))) Changed(); } }
    public double Level { get => level; set { if (Set(ref level, Math.Clamp(value, 0, 100))) Changed(); } }
    public double Sections { get => sections; set { if (Set(ref sections, Math.Clamp(Math.Round(value), 1, 3))) Changed(); } }
    public PhaserParameters Parameters => new(Math.Clamp((float)(Speed / 48000), 0.00001f, 0.00013f),
        (float)Lower, (float)Upper, Math.Clamp((float)(Level / 100 * 0.64), 0, 0.64f), (int)Sections);
    private void Changed() { if (!loading) Edited?.Invoke(); }
    public void Load(string deviceIdentity, PhaserParameters parameters)
    {
        parameters.Validate();
        loading = true;
        try
        {
            identity = deviceIdentity;
            speed = parameters.Step * 48000d; lower = parameters.Lower; upper = parameters.Upper;
            level = parameters.Level / 0.64d * 100; sections = parameters.Sections;
            foreach (string property in new[] { nameof(Speed), nameof(Lower), nameof(Upper), nameof(Level), nameof(Sections), nameof(Identity) }) Notify(property);
            Loaded = true;
        }
        finally { loading = false; }
    }
}
