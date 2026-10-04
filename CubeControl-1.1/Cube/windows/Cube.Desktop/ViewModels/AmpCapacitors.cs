using System.IO;

namespace Cube.Desktop.ViewModels;

public sealed record CapacitorProfile(string Id, string Label, string Capacitance, string Description, IReadOnlyList<double> Deviations);
public sealed record CapacitorState(int ProfileIndex, double Intensity);

public sealed class CapacitorBank : Observable
{
    private readonly Action changed;
    private int profileIndex;
    private double intensity = 50;
    public string Title { get; }
    public string Subtitle { get; }
    public IReadOnlyList<CapacitorProfile> Profiles { get; }
    public string SelectorFace => Path.Combine(AppContext.BaseDirectory, "Assets", "Equipment", "Botão Rotativo Vintage Preto Marconi.png");
    public string IntensityFace => Path.Combine(AppContext.BaseDirectory, "Assets", "Equipment", "Botão Vintage de Metal Dourado.png");
    public UiCommand RestoreOriginal { get; }
    public CapacitorBank(bool coupling, Action changed)
    {
        this.changed = changed;
        Title = coupling ? "Capacitores de Acoplamento" : "Capacitor de Derivação de Cátodo";
        Subtitle = coupling ? "Coupling Capacitors" : "Cathode Bypass Capacitor";
        CapacitorProfile original = new("original", "Original", "", "Preserva a referência carregada.", Array.AsReadOnly(new double[8]));
        Profiles = Array.AsReadOnly(coupling ? new[] {
            original,
            Profile("coupling-2n2", "2,2 nF · Enxuto", "2,2 nF", "Graves contidos, com foco e ataque.", [-33,-30,-26,-22,-17,-12,-7,-3]),
            Profile("coupling-4n7", "4,7 nF · Focado", "4,7 nF", "Corpo enxuto com presença definida.", [-24,-23,-21,-18,-14,-10,-6,-2]),
            Profile("coupling-10n", "10 nF · Contido", "10 nF", "Uma redução suave do corpo.", [-12,-13,-12,-10,-8,-6,-4,-1]),
            Profile("coupling-22n", "22 nF · Encorpado", "22 nF", "Mais preenchimento e peso no timbre.", [12,16,18,18,15,11,7,3]),
            Profile("coupling-47n", "47 nF · Amplo", "47 nF", "Corpo amplo e graves mais presentes.", [22,28,33,32,27,20,12,5])
        } : new[] {
            original,
            Profile("cathode-0u68", "0,68 µF · Cortante", "0,68 µF", "Médios e agudos em destaque, inspirado no Plexi.", [-3,-8,-15,-23,-30,-33,-29,-20]),
            Profile("cathode-2u2", "2,2 µF · Presente", "2,2 µF", "Presença com corpo mais contido.", [-2,-5,-10,-16,-22,-24,-20,-13]),
            Profile("cathode-4u7", "4,7 µF · Articulado", "4,7 µF", "Articulação com uma mudança suave de corpo.", [-1,-3,-6,-10,-13,-14,-11,-7]),
            Profile("cathode-10u", "10 µF · Cheio", "10 µF", "Resposta mais cheia e arredondada.", [2,5,9,13,17,19,17,12]),
            Profile("cathode-25u", "25 µF · Pleno", "25 µF", "Corpo pleno, inspirado nos circuitos Fender.", [4,10,17,24,30,33,30,22])
        });
        RestoreOriginal = new(() => { RestoreState(new(0, 50)); changed(); });
    }
    private static CapacitorProfile Profile(string id, string label, string capacitance, string description, double[] deviations)
        => new(id, label, capacitance, description, Array.AsReadOnly(deviations));
    public int ProfileIndex
    {
        get => profileIndex;
        set
        {
            if (value < 0 || value >= Profiles.Count || !Set(ref profileIndex, value)) return;
            Notify(nameof(SelectedProfile)); Notify(nameof(IsActive)); changed();
        }
    }
    public double Intensity
    {
        get => intensity;
        set
        {
            if (!double.IsFinite(value) || value < 0 || value > 100 || !Set(ref intensity, value)) return;
            Notify(nameof(IntensityDisplay)); changed();
        }
    }
    public string IntensityDisplay => $"{Intensity:0.#}%";
    public bool IsActive => profileIndex != 0;
    public CapacitorProfile SelectedProfile => Profiles[profileIndex];
    public double Percentage(int segment) => 100 + SelectedProfile.Deviations[segment] * intensity / 100;
    public CapacitorState CaptureState() => new(profileIndex, intensity);
    internal void RestoreState(CapacitorState state)
    {
        profileIndex = state.ProfileIndex; intensity = state.Intensity;
        Notify(nameof(ProfileIndex)); Notify(nameof(SelectedProfile)); Notify(nameof(IsActive));
        Notify(nameof(Intensity)); Notify(nameof(IntensityDisplay));
    }
}

public sealed record AmpEditSnapshot(byte[] Reference, string Name, int Slot, double[][] Controls,
    CapacitorState Coupling, CapacitorState Cathode);

public sealed partial class AmpViewModel
{
    private bool suppressEdits;
    private void RaiseEdited() { if (!suppressEdits) Edited?.Invoke(); }
    public CapacitorBank Coupling { get; private set; } = null!;
    public CapacitorBank Cathode { get; private set; } = null!;
    public CapacitorBank[] CapacitorBanks => Loaded ? [Coupling, Cathode] : [];
    private void InitializeCapacitors()
    {
        Coupling = new(true, () => ApplyCapacitor(Coupling, 0));
        Cathode = new(false, () => ApplyCapacitor(Cathode, 8));
        Notify(nameof(Coupling)); Notify(nameof(Cathode)); Notify(nameof(CapacitorBanks));
    }
    private void ApplyCapacitor(CapacitorBank bank, int start)
    {
        bool previous = suppressEdits;
        suppressEdits = true;
        try
        {
            for (int i = 0; i < 8; i++) CurveControls[start + i].SetNumber(bank.Percentage(i));
        }
        finally { suppressEdits = previous; }
        RaiseEdited();
    }
    private AmpControl[][] AllControls => [MainControls, EqAControls, EqBControls, CurveControls, CoefficientAControls, CoefficientBControls];
    public AmpEditSnapshot CaptureEditSnapshot() => new(image!.ToArray(), Name, Slot,
        AllControls.Select(group => group.Select(control => control.Number).ToArray()).ToArray(), Coupling.CaptureState(), Cathode.CaptureState());
    public void RestoreEditSnapshot(AmpEditSnapshot snapshot)
    {
        suppressEdits = true;
        try
        {
            Load(snapshot.Reference, snapshot.Name); Slot = snapshot.Slot;
            var groups = AllControls;
            for (int g = 0; g < groups.Length; g++)
                for (int i = 0; i < groups[g].Length; i++) groups[g][i].SetNumber(snapshot.Controls[g][i]);
            Coupling.RestoreState(snapshot.Coupling); Cathode.RestoreState(snapshot.Cathode);
        }
        finally { suppressEdits = false; }
        RefreshPresentation();
    }
}
