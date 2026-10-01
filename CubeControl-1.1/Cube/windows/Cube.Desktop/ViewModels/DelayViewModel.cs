using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class DelayViewModel : Observable
{
    private bool loading, loaded;
    private double timeSamples = 9600, feedback = 25, depth = 100, rate = 100, mix = 25;
    private string identity = "", message = "Experimental · validação sonora pendente. Ative a seção delay/modulação e desligue chorus/phaser antes de ler.";
    public event Action? Edited;
    public bool Loaded { get => loaded; private set => Set(ref loaded, value); }
    public string Identity => identity;
    public string Message { get => message; set => Set(ref message, value); }
    public double TimeSamples { get => timeSamples; set { if (Set(ref timeSamples, Math.Clamp(Math.Round(value),2400,16350))) Changed(); } }
    public double Feedback { get => feedback; set { if (Set(ref feedback, Math.Clamp(value,0,99.21875))) Changed(); } }
    public double Depth { get => depth; set { if (Set(ref depth, Math.Clamp(value,0,100))) Changed(); } }
    public double Rate { get => rate; set { if (Set(ref rate, Math.Clamp(value,0,100))) Changed(); } }
    public double Mix { get => mix; set { if (Set(ref mix, Math.Clamp(value,0,92.1875))) Changed(); } }
    public DelayParameters Parameters => new((int)TimeSamples,(float)(Feedback/100),(float)(Depth/1000),(float)(Rate/10000),(float)(Mix/100));
    private void Changed() { if (!loading) Edited?.Invoke(); }
    public void Load(string deviceIdentity, DelayParameters p)
    {
        p.Validate(); loading = true;
        try
        {
            identity = deviceIdentity; timeSamples = p.TimeSamples; feedback = p.Feedback*100d;
            depth = p.Depth*1000d; rate = p.Rate*10000d; mix = p.Mix*100d;
            foreach (string property in new[] { nameof(TimeSamples),nameof(Feedback),nameof(Depth),nameof(Rate),nameof(Mix),nameof(Identity) }) Notify(property);
            Loaded = true;
        }
        finally { loading = false; }
    }
}
