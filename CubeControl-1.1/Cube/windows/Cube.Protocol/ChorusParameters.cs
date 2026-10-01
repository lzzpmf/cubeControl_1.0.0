using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Protocol;

/// <summary>Bounded controls demonstrated by the live chorus investigation; no arbitrary coefficients or addresses.</summary>
public sealed record ChorusParameters(float Depth, float Step, float Mix, float Feedback, int Waveform, float WetGain, float CombinedGain)
{
    public const string Mapping = "cube-baby-chorus-live-20261001-8347f8-v1";
    public static ChorusParameters Default { get; } = new(0.4f, 0.000031f, 0.85f, 0, 0, 1, 1);
    internal static readonly int[] Offsets = [0x640, 0x644, 0x648, 0x64C, 0x650, 0x66C, 0x688];
    public void Validate()
    {
        static bool Within(float value, float min, float max) => float.IsFinite(value) && value >= min && value <= max;
        if (!Within(Depth, 0, 0.4f) || !Within(Step, 0.000008f, 0.000063f) || !Within(Mix, 0, 0.85f) ||
            !Within(Feedback, 0, 0.1f) || Waveform is < 0 or > 1 || !Within(WetGain, 0.5f, 1) || !Within(CombinedGain, 0.5f, 1))
            throw new ArgumentOutOfRangeException(nameof(ChorusParameters), "Parâmetros fora dos limites experimentais do chorus.");
    }
    public static ChorusParameters Read(byte[] dsp)
    {
        if (dsp.Length != 0xF70) throw new FormatException("Snapshot DSP inválido.");
        float F(int offset) => BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(offset, 4));
        var parameters = new ChorusParameters(F(0x644), F(0x648), F(0x688), F(0x64C), dsp[0x640], F(0x650), F(0x66C));
        parameters.Validate();
        return parameters;
    }
    internal byte[] ApplyTo(byte[] dsp)
    {
        Validate();
        if (dsp.Length != 0xF70) throw new FormatException("Snapshot DSP inválido.");
        var result = dsp.ToArray(); result[0x640] = (byte)Waveform;
        foreach (var (offset, value) in new[] { (0x644, Depth), (0x648, Step), (0x688, Mix), (0x64C, Feedback), (0x650, WetGain), (0x66C, CombinedGain) })
            BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(offset, 4), value);
        return result;
    }
    public static bool IsControlOffset(int offset) => offset == 0x640 ||
        Offsets.Skip(1).Any(start => offset >= start && offset < start + 4);
    public static bool IsDynamicOffset(int offset) => offset is >= 0x68C and < 0x694 or >= 0xF6C and < 0xF70;
    public static bool StableEquals(byte[] a, byte[] b, bool ignoreControls = false) =>
        a.Length == 0xF70 && b.Length == a.Length && Enumerable.Range(0, a.Length)
            .All(i => IsDynamicOffset(i) || (ignoreControls && IsControlOffset(i)) || a[i] == b[i]);
    internal static void ValidateStructure(byte[] dsp)
    {
        Read(dsp);
        if (dsp[0x63C] != 3 || dsp[0x63D] != 5)
            throw new FormatException("Snapshot não representa um chorus ativo compatível.");
        // Only b0 is editable. Nonzero recursive coefficients would invalidate the feedback bound.
        foreach (int start in new[] { 0x654, 0x670 })
            if (Enumerable.Range(start, 16).Any(i => dsp[i] != 0))
                throw new FormatException("Filtros do chorus incompatíveis com a prévia experimental.");
    }
}

public sealed record ChorusPreset(string Format, string Mapping, string Identity, ChorusParameters Parameters)
{
    private static readonly JsonSerializerOptions Options = new()
    { WriteIndented = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true };
    public static string Export(string identity, ChorusParameters parameters)
    {
        parameters.Validate(); PhaserPreset.ValidateIdentity(identity);
        return JsonSerializer.Serialize(new ChorusPreset("cubecontrol-chorus-v1", ChorusParameters.Mapping, identity, parameters), Options);
    }
    public static ChorusPreset Import(string json)
    {
        if (json.Length > 16384) throw new FormatException("Arquivo de chorus excessivamente grande.");
        var preset = JsonSerializer.Deserialize<ChorusPreset>(json, Options) ?? throw new FormatException("Arquivo vazio.");
        if (preset.Format != "cubecontrol-chorus-v1" || preset.Mapping != ChorusParameters.Mapping || preset.Parameters == null)
            throw new FormatException("Formato ou mapeamento de chorus incompatível.");
        PhaserPreset.ValidateIdentity(preset.Identity); preset.Parameters.Validate();
        return preset;
    }
}

public sealed record ChorusRecovery(string Format, string Mapping, string Identity, byte[] Bank,
    byte[] PhysicalState, byte[] OriginalDsp, ChorusParameters Previous, ChorusParameters Target)
{
    public void Validate()
    {
        if (Format != "cubecontrol-chorus-recovery-v1" || Mapping != ChorusParameters.Mapping ||
            Bank is not { Length: 48 } || PhysicalState is not { Length: 2 } || PhysicalState[0] > 2 || PhysicalState[1] != 1 ||
            OriginalDsp is not { Length: 0xF70 } || Previous == null || Target == null)
            throw new FormatException("Backup de chorus inválido.");
        PhaserPreset.ValidateIdentity(Identity); CubeBabyProfile.ValidateBank(Bank);
        Previous.Validate(); Target.Validate(); ChorusParameters.ValidateStructure(OriginalDsp);
        int slot = PhysicalState[0];
        if (Bank[slot * 16 + 8] > 6 || Bank[slot * 16 + 11] != 1)
            throw new FormatException("Backup não representa um chorus ativo compatível.");
    }
}
