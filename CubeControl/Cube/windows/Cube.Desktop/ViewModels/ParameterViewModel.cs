using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class ParameterViewModel(Parameter parameter, int slot, Action<ParameterKey, int> changed) : Observable
{
    private int current;
    public Parameter Definition => parameter;
    public string Name => parameter.Name;
    public string Description => parameter.Description;
    public int Maximum => parameter.Maximum;
    public bool IsChoice => parameter.Options != null;
    public bool IsToggle => parameter.Maximum == 1;
    public bool IsKnob => !IsChoice && !IsToggle;
    public bool IsModulation => parameter.Offset == 8;
    public string[] Options => parameter.Options ?? [];
    public string SelectedOption => IsChoice ? Options[current] : "";
    public int Choice { get => current; set => Value = value; }
    public bool Enabled { get => current != 0; set => Value = value ? 1 : 0; }
    public string Display => parameter.Offset == 8 || (parameter.Offset is 3 or 7 && current == 0) ? CubeBabyProfile.Display(parameter.Offset, current) : "";
    public string Detail => parameter.Offset switch { 8 => "CHORUS  ←  OFF  →  PHASER", 9 => "Gabinetes reconhecidos pelo conteúdo · packs e IRs próprios", _ => Description };
    public UiCommand TurnModOff => new(() => Value = 8);
    public double Value
    {
        get => current;
        set
        {
            if (!double.IsFinite(value)) return;
            int next = Math.Clamp((int)Math.Round(value), 0, Maximum);
            if (next == current) return;
            current = next; Changed(); changed(new(slot, parameter.Offset), current);
        }
    }
    public void Load(int value) { current = value; Changed(); }
    private void Changed() { Notify(nameof(Value)); Notify(nameof(Choice)); Notify(nameof(Enabled)); Notify(nameof(Display)); Notify(nameof(SelectedOption)); }
}

public sealed class ModuleViewModel(string name, string number, string color, string description, IEnumerable<ParameterViewModel> parameters)
{
    public string Name => name;
    public string Number => number;
    public string Color => color;
    public string Description => description;
    public ParameterViewModel[] Parameters { get; } = parameters.ToArray();
}

public sealed class PresetViewModel : Observable
{
    private string state = "Não carregado";
    public int Index { get; }
    public string Name => ((char)('A' + Index)).ToString();
    public string State { get => state; set => Set(ref state, value); }
    public ParameterViewModel[] Parameters { get; }
    public MusicalPresetViewModel Musical { get; }
    public DialViewModel[] Dials { get; }
    public ModuleViewModel[] Modules { get; }
    public ParameterViewModel Volume => Parameters[5];
    public PresetViewModel(int slot, Action<ParameterKey, int> changed)
    {
        Index = slot;
        Parameters = CubeBabyProfile.Parameters.Select(p => new ParameterViewModel(p, slot, changed)).ToArray();
        Musical = new(Parameters);
        Dials = Parameters.Select(DialViewModel.From).ToArray();
        foreach (var p in Parameters) p.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ParameterViewModel.Value)) Dials[p.Definition.Offset].Refresh(); };
        Modules =
        [
            Make("Amp", "01", "#DAA66F", "Cor, ganho e brilho do seu timbre", [0, 1, 2, 12]),
            Make("Modulação", "02", "#8FBAA2", "Chorus, centro desligado, Phaser", [8]),
            Make("Delay", "03", "#7FA7CE", "Espaço e repetição", [6, 4, 7, 11]),
            Make("Reverb", "04", "#BD9DCB", "Ambiência", [3]),
            Make("IR Cab", "05", "#C3B889", "Resposta do gabinete", [9, 10])
        ];
    }
    private ModuleViewModel Make(string name, string number, string color, string description, int[] offsets)
        => new(name, number, color, description, offsets.Select(i => Parameters[i]));
    public void Load(byte[] bank) { foreach (var p in Parameters) p.Load(bank[Index * 16 + p.Definition.Offset]); }
}
