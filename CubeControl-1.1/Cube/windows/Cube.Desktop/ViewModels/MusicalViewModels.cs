using Cube.Protocol;
using System.Windows.Input;

namespace Cube.Desktop.ViewModels;

public sealed class DialViewModel(string name, string description, int minimum, int maximum, Func<double> read, Action<double> write) : Observable
{
    public string Name => name;
    public string Description => description;
    public int Minimum => minimum;
    public int Maximum => maximum;
    public string RangeLabel => $"Faixa: {minimum}–{maximum}";
    public double Value
    {
        get => read();
        set
        {
            if (!double.IsFinite(value)) return;
            double next = Math.Clamp(Math.Round(value), Minimum, Maximum);
            if (next != read()) write(next);
        }
    }
    public void Refresh() => Notify(nameof(Value));
    public static DialViewModel From(ParameterViewModel p) => new(p.Name, p.Description, 0, p.Maximum, () => p.Value, v => p.Value = v);
}

public sealed class MusicalPresetViewModel : Observable
{
    private readonly ParameterViewModel[] parameters;
    public ModulationState Modulation { get; } = new();
    public ReverbState Reverb { get; } = new();
    public DialViewModel ModulationDial { get; }
    public DialViewModel ReverbDial { get; }
    private int chorusPosition = 4, phaserPosition = 4;
    public DialViewModel ChorusDial { get; }
    public DialViewModel PhaserDial { get; }
    public bool ChorusEnabled { get => ModulationEnabled && ChorusSelected; set => SetKindEnabled(ModulationKind.Chorus, value); }
    public bool PhaserEnabled { get => ModulationEnabled && PhaserSelected; set => SetKindEnabled(ModulationKind.Phaser, value); }
    public bool ModulationEnabled { get => parameters[11].Enabled && Modulation.Enabled; set { if (value) SetKindEnabled(Modulation.Kind, true); else { parameters[8].Value = Modulation.SetEnabled(false); Refresh(); } } }
    private int delayMix = 32;
    public DialViewModel DelayMixDial { get; }
    public bool DelayEnabled
    {
        get => parameters[11].Enabled && parameters[7].Value > 0;
        set
        {
            if (value == DelayEnabled) return;
            if (value)
            {
                // A bypassed section can retain a modulation value: do not reactivate it implicitly.
                if (!parameters[11].Enabled) parameters[8].Value = 8;
                parameters[7].Value = delayMix > 0 ? delayMix : 32;
                parameters[11].Enabled = true;
            }
            else
            {
                parameters[7].Value = 0;
                if (!ModulationEnabled) parameters[11].Enabled = false;
            }
            Refresh();
        }
    }
    public bool ReverbEnabled { get => Reverb.Enabled; set { parameters[3].Value = Reverb.SetEnabled(value); Refresh(); } }
    public bool ChorusSelected => Modulation.Kind == ModulationKind.Chorus;
    public bool PhaserSelected => !ChorusSelected;
    public string ModulationName => ChorusSelected ? "Chorus" : "Phaser";
    public ICommand ChooseChorus { get; }
    public ICommand ChoosePhaser { get; }
    public MusicalPresetViewModel(ParameterViewModel[] parameters)
    {
        this.parameters = parameters;
        DelayMixDial = new("Mistura", parameters[7].Description, 0, parameters[7].Maximum, () => delayMix, v =>
        {
            bool active = DelayEnabled;
            delayMix = (int)v;
            if (active) parameters[7].Value = delayMix;
            Refresh();
        });
        parameters[7].PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(ParameterViewModel.Value)) return;
            if (parameters[7].Value > 0) delayMix = (int)parameters[7].Value;
            Refresh();
        };
        parameters[11].PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ParameterViewModel.Value)) Refresh(); };
        ChorusDial = new("Ajuste", "Sete posições de Chorus. Ligar Chorus desliga Phaser.", 1, 7, () => chorusPosition, v => AdjustKind(ModulationKind.Chorus, (int)v));
        PhaserDial = new("Ajuste", "Sete posições de Phaser. Ligar Phaser desliga Chorus.", 1, 7, () => phaserPosition, v => AdjustKind(ModulationKind.Phaser, (int)v));
        ChooseChorus = new UiCommand(() => Select(ModulationKind.Chorus));
        ChoosePhaser = new UiCommand(() => Select(ModulationKind.Phaser));
        ModulationDial = new("Ajuste", "Sete posições do efeito selecionado.", 1, 7, () => Modulation.Position, v => { var raw = Modulation.Adjust((int)v); if (raw.HasValue) parameters[8].Value = raw.Value; Refresh(); });
        ReverbDial = new("Quantidade", "Enquanto desligado, prepara o ajuste para a próxima ativação.", 1, 15, () => Reverb.Amount, v => { var raw = Reverb.Adjust((int)v); if (raw.HasValue) parameters[3].Value = raw.Value; Refresh(); });
        parameters[8].PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ParameterViewModel.Value)) { Modulation.Load((int)parameters[8].Value); Refresh(); } };
        parameters[3].PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ParameterViewModel.Value)) { Reverb.Load((int)parameters[3].Value); Refresh(); } };
        Modulation.Load((int)parameters[8].Value); Reverb.Load((int)parameters[3].Value); Refresh();
    }
    private int PreparedPosition(ModulationKind kind) => kind == ModulationKind.Chorus ? chorusPosition : phaserPosition;
    private void AdjustKind(ModulationKind kind, int position)
    {
        if (kind == ModulationKind.Chorus) chorusPosition = position; else phaserPosition = position;
        // Editing the inactive card is local; never switch the shared MIDI block here.
        if (Modulation.Kind == kind)
        {
            var raw = Modulation.Adjust(position);
            if (ModulationEnabled && raw.HasValue) parameters[8].Value = raw.Value;
        }
        Refresh();
    }
    private void SetKindEnabled(ModulationKind kind, bool enabled)
    {
        if (!enabled)
        {
            if (Modulation.Enabled && Modulation.Kind == kind) ModulationEnabled = false;
            return;
        }
        if (ModulationEnabled && Modulation.Kind == kind) return;
        int position = PreparedPosition(kind);
        Modulation.Select(kind); Modulation.Adjust(position);
        // One raw parameter change, without an intermediate OFF or another effect's amount.
        parameters[8].Value = Modulation.SetEnabled(true);
        if (!parameters[11].Enabled)
        {
            // Enable the shared hardware gate without reviving a bypassed delay.
            parameters[7].Value = 0;
            parameters[11].Enabled = true;
        }
        Refresh();
    }
    private void Select(ModulationKind kind)
    {
        int position = PreparedPosition(kind);
        Modulation.Select(kind);
        var raw = Modulation.Adjust(position);
        if (raw.HasValue) parameters[8].Value = raw.Value;
        Refresh();
    }
    private void Refresh()
    {
        if (Modulation.Kind == ModulationKind.Chorus) chorusPosition = Modulation.Position; else phaserPosition = Modulation.Position;
        Notify(nameof(ModulationEnabled)); Notify(nameof(ReverbEnabled)); Notify(nameof(ChorusSelected)); Notify(nameof(PhaserSelected)); Notify(nameof(ModulationName));
        Notify(nameof(ChorusEnabled)); Notify(nameof(PhaserEnabled));
        ChorusDial.Refresh(); PhaserDial.Refresh();
        Notify(nameof(DelayEnabled)); DelayMixDial.Refresh();
        ModulationDial.Refresh(); ReverbDial.Refresh();
    }
}

public sealed class StageViewModel(int index, string title, string subtitle, string color) : Observable
{
    private Func<bool>? readToggle, canToggle;
    private Action<bool>? writeToggle;
    public bool CanToggle => canToggle?.Invoke() == true;
    public bool IsEffectEnabled
    {
        get => readToggle?.Invoke() == true;
        set { if (CanToggle && value != IsEffectEnabled) writeToggle?.Invoke(value); }
    }
    public string ToggleLabel => (index == 0 ? "Tone · " : "") + (IsEffectEnabled ? "Ligado" : "Desligado");
    public string ToggleCaption => "";
    public string ToggleName => index == 0 ? "Seção Tone" : title;
    public string ToggleHint => index == 0 ? "Interruptor da seção Tone; não é bypass do amplificador" : index is 1 or 2 ? "Chorus e Phaser compartilham o mesmo bloco. Ligar um desliga o outro." : title;
    public void BindToggle(Func<bool> read, Action<bool> write, Func<bool> can)
    { readToggle = read; writeToggle = write; canToggle = can; }
    public void RefreshToggle() { Notify(nameof(IsEffectEnabled)); Notify(nameof(CanToggle)); Notify(nameof(ToggleLabel)); }
    public int Index => index;
    public string Number => (index + 1).ToString("00");
    public string Title => title;
    public string Subtitle => subtitle;
    public string Color => Appearance.ThemeService.ModuleColor(color);
    public void RefreshColor() => Notify(nameof(Color));
    private string state = "";
    public string State { get => state; set => Set(ref state, value); }
}
