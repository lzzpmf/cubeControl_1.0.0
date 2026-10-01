using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Protocol;

/// <summary>Controls consumed by the verified live phaser worker. No memory addresses in preset files.</summary>
public sealed record PhaserParameters(float Step, float Lower, float Upper, float Level, int Sections)
{
    public const string Mapping = "cube-baby-live-20261001-8347f8-v1";
    public static PhaserParameters Default { get; } = new(0.00001f, 300, 3000, 0.64f, 3);
    public void Validate()
    {
        if (!float.IsFinite(Step) || Step is < 0.00001f or > 0.00013f ||
            !float.IsFinite(Lower) || !float.IsFinite(Upper) || Lower < 300 || Upper > 3000 || Lower > Upper ||
            !float.IsFinite(Level) || Level is < 0 or > 0.64f || Sections is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(PhaserParameters), "Parâmetros fora dos limites experimentais do phaser.");
    }
    public static PhaserParameters Read(byte[] dsp)
    {
        if (dsp.Length != 0xF70) throw new FormatException("Snapshot DSP inválido.");
        float F(int offset) => BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(offset, 4));
        var result = new PhaserParameters(F(0x644), F(0x64C), F(0x650), F(0x654), dsp[0x641] + 1);
        result.Validate();
        return result;
    }
    internal byte[] ApplyTo(byte[] dsp)
    {
        Validate();
        var result = dsp.ToArray();
        result[0x641] = (byte)(Sections - 1);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x644), Step);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x64C), Lower);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x650), Upper);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x654), Level);
        return result;
    }
    public static bool IsDynamicOffset(int offset) => offset is >= 0x658 and < 0x65C or >= 0xF6C and < 0xF70;
    public static bool IsControlOffset(int offset) => offset == 0x641 || offset is >= 0x644 and < 0x648 or >= 0x64C and < 0x658;
    public static bool StableEquals(byte[] a, byte[] b, bool ignoreControls = false) =>
        a.Length == 0xF70 && b.Length == a.Length && Enumerable.Range(0, a.Length)
            .All(i => IsDynamicOffset(i) || (ignoreControls && IsControlOffset(i)) || a[i] == b[i]);
}

public sealed record PhaserPreset(string Format, string Mapping, string Identity, PhaserParameters Parameters)
{
    private static readonly JsonSerializerOptions Options = new()
    { WriteIndented = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true };
    public static string Export(string identity, PhaserParameters parameters)
    {
        parameters.Validate();
        ValidateIdentity(identity);
        return JsonSerializer.Serialize(new PhaserPreset("cubecontrol-phaser-v1", PhaserParameters.Mapping, identity, parameters), Options);
    }
    public static PhaserPreset Import(string json)
    {
        if (json.Length > 16384) throw new FormatException("Arquivo de phaser excessivamente grande.");
        var preset = JsonSerializer.Deserialize<PhaserPreset>(json, Options) ?? throw new FormatException("Arquivo vazio.");
        if (preset.Format != "cubecontrol-phaser-v1" || preset.Mapping != PhaserParameters.Mapping || preset.Parameters == null)
            throw new FormatException("Formato ou mapeamento de phaser incompatível.");
        ValidateIdentity(preset.Identity);
        preset.Parameters.Validate();
        return preset;
    }
    internal static void ValidateIdentity(string identity)
    {
        if (string.IsNullOrEmpty(identity) || identity.Length > 256) throw new FormatException("Identidade inválida.");
        var bytes = Convert.FromHexString(identity);
        if (!bytes.AsSpan().StartsWith("SINCO-CubeBaby"u8)) throw new FormatException("Identidade Cube Baby incompatível.");
    }
}

public sealed record PhaserRecovery(string Format, string Mapping, string Identity, byte[] Bank,
    byte[] PhysicalState, byte[] OriginalDsp, PhaserParameters Previous, PhaserParameters Target)
{
    public void Validate()
    {
        if (Format != "cubecontrol-phaser-recovery-v1" || Mapping != PhaserParameters.Mapping ||
            Bank is not { Length: 48 } || PhysicalState is not { Length: 2 } || PhysicalState[0] > 2 || PhysicalState[1] != 1 ||
            OriginalDsp is not { Length: 0xF70 } || Previous == null || Target == null)
            throw new FormatException("Backup de phaser inválido.");
        PhaserPreset.ValidateIdentity(Identity);
        CubeBabyProfile.ValidateBank(Bank);
        Previous.Validate(); Target.Validate(); PhaserParameters.Read(OriginalDsp);
        int slot = PhysicalState[0];
        if (Bank[slot * 16 + 8] is < 9 or > 15 || Bank[slot * 16 + 11] != 1 ||
            OriginalDsp[0x63C] != 3 || OriginalDsp[0x63D] != 2 || OriginalDsp[0x640] != 1 ||
            BinaryPrimitives.ReadSingleLittleEndian(OriginalDsp.AsSpan(0x648)) != 5000)
            throw new FormatException("Backup não representa um phaser ativo compatível.");
    }
}
