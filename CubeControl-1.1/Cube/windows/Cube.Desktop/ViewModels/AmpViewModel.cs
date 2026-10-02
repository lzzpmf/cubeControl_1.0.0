using System.Globalization;
using System.IO;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class AmpValue(int index, float value) : Observable
{
    public int Index { get; } = index;
    private string reference = value.ToString("G9", CultureInfo.InvariantCulture);
    public string Reference { get => reference; set => Set(ref reference, value); }
    private string text = value.ToString("G9", CultureInfo.InvariantCulture);
    public event Action? Edited;
    public string Value { get => text; set { if (Set(ref text, value)) Edited?.Invoke(); } }
    private string effective = value.ToString("G9", CultureInfo.InvariantCulture);
    public string Effective { get => effective; set => Set(ref effective, value); }
    public float Number()
    {
        if (!float.TryParse(Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !float.IsFinite(value))
            throw new ArgumentException($"Valor inválido na linha {Index}.");
        return value;
    }
}

public sealed class AmpControl(string label, string detail, string asset, double minimum, double maximum, double initial, string unit, Action<double> changed, bool logarithmic = false) : Observable
{
    private double number = initial;
    private readonly double referenceNumber = initial;
    public string Code { get; init; } = "";
    public int Decimals { get; init; } = 2;
    public Action? RestoreAction { get; set; }
    public double Number => number;
    public string EditText => number.ToString("0." + new string('#', Math.Max(3, Decimals)), CultureInfo.CurrentCulture);
    public string Label { get; } = label;
    public string Detail { get; } = detail;
    public string Asset { get; } = Path.Combine(AppContext.BaseDirectory, "Assets", "Equipment", asset);
    public double Position
    {
        get => 1000 * (logarithmic ? Math.Log(number / minimum) / Math.Log(maximum / minimum) : (number - minimum) / (maximum - minimum));
        set
        {
            if (!double.IsFinite(value)) return;
            double ratio = Math.Clamp(value / 1000, 0, 1);
            double next = logarithmic ? minimum * Math.Pow(maximum / minimum, ratio) : minimum + ratio * (maximum - minimum);
            SetNumber(next);
        }
    }
    public string Display => number.ToString(Decimals == 3 ? "0.000" : "0.##", CultureInfo.CurrentCulture) + unit;
    public bool SetNumber(double value)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum) return false;
        if (Math.Abs(value - number) < 1e-10) return true;
        number = value; NotifyValues(); changed(number); return true;
    }
    public bool TryEdit(string text) => double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && SetNumber(value);
    private void NotifyValues() { Notify(nameof(Position)); Notify(nameof(Display)); Notify(nameof(EditText)); Notify(nameof(Number)); }
    public void Reset(double value) { number = Math.Clamp(value, minimum, maximum); NotifyValues(); }
    public void Restore() { if (RestoreAction != null) RestoreAction(); else SetNumber(referenceNumber); }
}

public sealed class AmpViewModel : Observable
{
    private bool updatingGroups;
    public AmpControl[] MainControls { get; private set; } = [];
    public AmpControl[] EqAControls { get; private set; } = [];
    public AmpControl[] EqBControls { get; private set; } = [];
    public AmpControl[] CurveControls { get; private set; } = [];
    public AmpControl[] CoefficientAControls { get; private set; } = [];
    public AmpControl[] CoefficientBControls { get; private set; } = [];
    public AmpControl[][] CoefficientASections => [CoefficientAControls.Take(5).ToArray(), CoefficientAControls.Skip(5).ToArray()];
    public AmpControl[][] CoefficientBSections => [CoefficientBControls.Take(3).ToArray(), CoefficientBControls.Skip(3).Take(3).ToArray(), CoefficientBControls.Skip(6).ToArray()];
    public event Action? Edited;
    private byte[]? image;
    private int slot;
    private string name = "Nenhum amp carregado";
    public string[] Slots { get; } = EquipmentCatalog.Amps.Select((amp, i) => $"{i + 1} · {amp.Name}").ToArray();
    public int Slot { get => slot; set => Set(ref slot, value); }
    public string Name { get => name; private set => Set(ref name, value); }
    public bool Loaded => image != null;
    public AmpValue[] Gain { get; private set; } = [];
    public AmpValue[] Compensation { get; private set; } = [];
    public AmpValue[] Curve { get; private set; } = [];
    public AmpValue[] CoefficientsA { get; private set; } = [];
    public AmpValue[] CoefficientsB { get; private set; } = [];
    public string GainFactor { get; set; } = "1";
    public string CompensationFactor { get; set; } = "1";
    private bool changeEqA, changeEqB;
    public bool ChangeEqA { get => changeEqA; set => Set(ref changeEqA, value); }
    public bool ChangeEqB { get => changeEqB; set => Set(ref changeEqB, value); }
    public string FrequencyA { get; set; } = "1050";
    public string LevelA { get; set; } = "5";
    public string QA { get; set; } = "2";
    public string FrequencyB { get; set; } = "200";
    public string LevelB { get; set; } = "2";
    public string QB { get; set; } = "1.2";
    private static double Parse(string value)
    {
        if (!double.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double number) || !double.IsFinite(number))
            throw new ArgumentException("Informe um número válido.");
        return number;
    }
    public void LoadSlot(byte[] bytes, int slot)
    {
        string title = $"Amp do slot {slot + 1}";
        using var stream = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps");
        if (stream != null)
        {
            var original = new byte[AmpImage.Size];
            for (int i = 0; i < EquipmentCatalog.Amps.Count; i++)
            {
                stream.ReadExactly(original);
                if (!bytes.AsSpan().SequenceEqual(original)) continue;
                title = EquipmentCatalog.Amps[i].Name;
                break;
            }
        }
        Load(bytes, title);
    }
    public void Load(byte[] bytes, string title)
    {
        AmpImage.Validate(bytes); image = bytes.ToArray(); Name = title;
        Gain = Rows(0x0C, 8); Compensation = Rows(0x2C, 8); Curve = Rows(0x11C, 256);
        CoefficientsA = Rows(0x50, 50); CoefficientsB = Rows(0x51C, 50);
        GainFactor = CompensationFactor = "1"; ChangeEqA = ChangeEqB = false;
        MainControls = [
            new("Ganho", "Ajuste a intensidade do ganho do seu amp.", "Botão Metálico de Controle Texturizado.png", 0, Math.Min(100, AmpImage.MaximumGain / Math.Max(.001, Gain.Max(r => r.Number()))), 1, "×", v => { GainFactor = Invariant(v); UpdateEffective(Gain, v); Edited?.Invoke(); }),
            new("Volume", "Equilibre o volume ao ajustar seu timbre.", "Botão Vintage de Metal Dourado.png", 0, Math.Min(8, AmpImage.MaximumCompensation / Math.Max(.001, Compensation.Max(r => r.Number()))), 1, "×", v => { CompensationFactor = Invariant(v); UpdateEffective(Compensation, v); Edited?.Invoke(); })];
        EqAControls = EqControls(bytes, 0x5E8, true);
        EqBControls = EqControls(bytes, 0x604, false);
        CurveControls = GroupControls(Curve, 16, true, "Fader Retrô preto com faixa creme.png", "Fader Vintage de Estilo Retrô.png");
        CoefficientAControls = GroupControls(CoefficientsA, 8, false, "Fader Preto com Indicador Prateado.png", prefix: "A");
        CoefficientBControls = GroupControls(CoefficientsB, 8, false, "Fader Retrô Charcoal com Faixa Creme.png", prefix: "B");
        foreach (var row in Gain) row.Edited += () => { UpdateEffective(Gain, Parse(GainFactor)); Edited?.Invoke(); };
        foreach (var row in Compensation) row.Edited += () => { UpdateEffective(Compensation, Parse(CompensationFactor)); Edited?.Invoke(); };
        Notify(nameof(CoefficientASections)); Notify(nameof(CoefficientBSections));
        foreach (string property in new[] { nameof(MainControls), nameof(EqAControls), nameof(EqBControls), nameof(CurveControls), nameof(CoefficientAControls), nameof(CoefficientBControls) }) Notify(property);
        foreach (string property in new[] { nameof(Loaded), nameof(Gain), nameof(Compensation), nameof(Curve), nameof(CoefficientsA), nameof(CoefficientsB), nameof(GainFactor), nameof(CompensationFactor), nameof(ChangeEqA), nameof(ChangeEqB) }) Notify(property);
    }
    private static string Invariant(double value) => value.ToString("G17", CultureInfo.InvariantCulture);
    private static void UpdateEffective(AmpValue[] rows, double factor)
    {
        foreach (var row in rows)
        {
            try { row.Effective = ((float)(row.Number() * factor)).ToString("G9", CultureInfo.InvariantCulture); }
            catch (ArgumentException) { row.Effective = "—"; }
        }
    }
    private AmpControl[] EqControls(byte[] bytes, int offset, bool first)
    {
        double b0 = AmpImage.Get(bytes, offset), b2 = AmpImage.Get(bytes, offset + 8), a1 = AmpImage.Get(bytes, offset + 12), a2 = AmpImage.Get(bytes, offset + 16);
        double w = Math.Acos(Math.Clamp(-a1 / (1 + a2), -1, 1));
        double amplitude = (b0 - b2) / (1 - a2);
        double hz = Math.Clamp(w * 48000 / (2 * Math.PI), 20, 20000);
        double db = Math.Clamp(20 * Math.Log10(Math.Max(1e-6, amplitude)), -12, 12);
        double q = Math.Clamp(Math.Sin(w) / (2 * (1 - a2) / (1 + a2) * Math.Sqrt(Math.Max(1e-6, amplitude))), .1, 10);
        if (!double.IsFinite(q)) q = 1;
        if (first) { FrequencyA = Invariant(hz); LevelA = Invariant(db); QA = Invariant(q); }
        else { FrequencyB = Invariant(hz); LevelB = Invariant(db); QB = Invariant(q); }
        string asset = first ? "Botão Rotativo Vintage Preto Marconi.png" : "Botão Giratório Vintage de Metal e Preto.png";
        void Changed(int field, double value)
        {
            if (first) { ChangeEqA = true; if (field == 0) FrequencyA = Invariant(value); else if (field == 1) LevelA = Invariant(value); else QA = Invariant(value); }
            else { ChangeEqB = true; if (field == 0) FrequencyB = Invariant(value); else if (field == 1) LevelB = Invariant(value); else QB = Invariant(value); }
            Edited?.Invoke();
        }
        AmpControl[] controls = [new("Frequência", "Centro do reforço ou corte.", asset, 20, 20000, hz, " Hz", v => Changed(0, v), true),
            new("Reforço/corte", "Positivo reforça; negativo corta.", asset, -12, 12, db, " dB", v => Changed(1, v)),
            new("Largura (Q)", "Q maior concentra o efeito em uma faixa menor.", asset, .1, 10, q, "", v => Changed(2, v), true)];
        foreach (var control in controls) control.RestoreAction = () =>
        {
            controls[0].Reset(hz); controls[1].Reset(db); controls[2].Reset(q);
            if (first) { FrequencyA = Invariant(hz); LevelA = Invariant(db); QA = Invariant(q); ChangeEqA = false; }
            else { FrequencyB = Invariant(hz); LevelB = Invariant(db); QB = Invariant(q); ChangeEqB = false; }
            Edited?.Invoke();
        };
        return controls;
    }
    public double[] EqCoefficients(bool first)
    {
        if (image == null) return [1, 0, 0, 0, 0];
        var data = image.ToArray(); int offset = first ? 0x5E8 : 0x604;
        if (first && ChangeEqA) AmpImage.SetEq(data, offset, Parse(FrequencyA), Parse(LevelA), Parse(QA));
        if (!first && ChangeEqB) AmpImage.SetEq(data, offset, Parse(FrequencyB), Parse(LevelB), Parse(QB));
        return Enumerable.Range(0, 5).Select(i => (double)AmpImage.Get(data, offset + i * 4)).ToArray();
    }
    private AmpControl[] GroupControls(AmpValue[] rows, int count, bool curve, string asset, string? alternate = null, string prefix = "")
    {
        var result = new AmpControl[count];
        for (int group = 0; group < count; group++)
        {
            int start = group * rows.Length / count, end = (group + 1) * rows.Length / count;
            var basis = rows[start..end].Select(r => r.Number()).ToArray();
            string[] names = prefix == "A" ? ["Ganho adicional", "Corpo", "Médio-graves", "Médios", "Punch", "Suavidade 1", "Suavidade 2", "Suavidade 3"]
                : ["Caráter de captação", "Ressonância de médios", "Ressonância de médio-agudos", "Corpo grave 1", "Corpo grave 2", "Corpo grave 3", "Definição 1", "Definição 2"];
            bool capped = (prefix == "A" && (group is 1 or 3)) || (prefix == "B" && group <= 2);
            double maximum = curve ? 200 : capped ? 100 : 110;
            string detail = curve ? "Molde a resposta à intensidade do sinal. 100% mantém o ajuste inicial."
                : "Explore a mudança no timbre. 100% mantém o ajuste inicial; use Shift para ajustar com precisão.";
            if (prefix == "B" && group == 0) detail += " Explore diferentes cores de captação.";
            if (prefix == "B" && (group is 1 or 2)) detail += " Ajuste aos poucos para encontrar a ressonância desejada.";
            AmpControl? control = null;
            control = new(curve ? $"{group + 1:00}" : names[group], detail, alternate != null && group % 2 != 0 ? alternate : asset, curve ? 0 : 90, maximum, 100, "%", percent =>
            {
                updatingGroups = true;
                try
                {
                    for (int i = start; i < end; i++)
                    {
                        double factor = curve ? percent / 100 : Math.Clamp(percent / 100, 0.9, maximum / 100);
                        float value = (float)(basis[i - start] * factor);
                        if (curve) value = Math.Clamp(value, 0, 1);
                        rows[i].Value = value.ToString("G9", CultureInfo.InvariantCulture);
                        rows[i].Effective = rows[i].Value;
                    }
                }
                finally { updatingGroups = false; }
                Edited?.Invoke();
            }) { Code = curve ? "" : $"{prefix}{group + 1:00}", Decimals = prefix == "B" && (group is 1 or 2) ? 3 : 2 };
            for (int i = start; i < end; i++)
                rows[i].Edited += () =>
                {
                    if (updatingGroups) return;
                    try
                    {
                        var edited = rows[start..end].Select(r => r.Number()).ToArray();
                        edited.CopyTo(basis, 0); control.Reset(100);
                        for (int j = start; j < end; j++) rows[j].Reference = rows[j].Value;
                        UpdateEffective(rows[start..end], 1);
                    }
                    catch (ArgumentException) { }
                    Edited?.Invoke();
                };
            result[group] = control;
        }
        return result;
    }
    private AmpValue[] Rows(int offset, int count) => Enumerable.Range(0, count).Select(i => new AmpValue(i, AmpImage.Get(image!, offset + i * 4))).ToArray();
    public byte[] Build()
    {
        if (image == null) throw new ArgumentException("Leia um slot ou importe um arquivo de amp.");
        var data = image.ToArray();
        double gainFactor = Parse(GainFactor), compensationFactor = Parse(CompensationFactor);
        if (gainFactor is < 0 or > 100 || compensationFactor is < 0 or > 100) throw new ArgumentException("Multiplicadores devem estar entre 0 e 100.");
        foreach (var (rows, offset, factor) in new[] { (Gain, 0x0C, gainFactor), (Compensation, 0x2C, compensationFactor), (Curve, 0x11C, 1d), (CoefficientsA, 0x50, 1d), (CoefficientsB, 0x51C, 1d) })
            for (int i = 0; i < rows.Length; i++) AmpImage.Set(data, offset + i * 4, (float)(rows[i].Number() * factor));
        if (ChangeEqA) AmpImage.SetEq(data, 0x5E8, Parse(FrequencyA), Parse(LevelA), Parse(QA));
        if (ChangeEqB) AmpImage.SetEq(data, 0x604, Parse(FrequencyB), Parse(LevelB), Parse(QB));
        AmpImage.Validate(data);
        return data;
    }
}
