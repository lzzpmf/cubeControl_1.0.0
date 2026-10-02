using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Protocol;

public sealed record DelaySnapshot(byte[] Bank, byte[] Dsp, byte[] PhysicalState);
public sealed record DelayRuntimeSample(DateTimeOffset Started, DateTimeOffset Finished, byte[] CurrentTime, byte[] Runtime);

public sealed record DelayParameters(int TimeSamples, float Feedback, float Depth, float Rate, float Mix)
{
    public const string Mapping = "cube-baby-delay-live-8347f8-v1";
    internal static readonly byte[] Filters = Convert.FromHexString("0000803fdd60c0be3a57303fdd60c0be3a57303f00000000000000007f16533fe678b5bf44331f3f478fdfbf9676463f0000000000000000");
    public void Validate()
    {
        if (TimeSamples is < 2400 or > 16350 || !float.IsFinite(Feedback) || Feedback is < 0 or > 0.9921875f ||
            !float.IsFinite(Depth) || Depth is < 0 or > 0.1f || !float.IsFinite(Rate) || Rate is < 0 or > 0.01f ||
            !float.IsFinite(Mix) || Mix is < 0 or > 0.921875f)
            throw new ArgumentOutOfRangeException(nameof(DelayParameters), "Parâmetros fora dos limites do delay.");
    }
    public static DelayParameters Read(byte[] dsp)
    {
        if (dsp.Length != 0xF70) throw new FormatException("Snapshot DSP inválido.");
        var p = new DelayParameters(BinaryPrimitives.ReadInt16LittleEndian(dsp.AsSpan(0x696)),
            BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(0x698)), BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(0x69C)),
            BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(0x6A0)), BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(0x6DC)));
        p.Validate(); return p;
    }
    internal byte[] ApplyTo(byte[] dsp)
    {
        Validate(); var result = dsp.ToArray();
        BinaryPrimitives.WriteInt16LittleEndian(result.AsSpan(0x696), (short)TimeSamples);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x698), Feedback);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x69C), Depth);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x6A0), Rate);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x6DC), Mix);
        return result;
    }
    internal static readonly (int Offset, int Length)[] Fields = [(0x696, 2), (0x698, 4), (0x69C, 4), (0x6A0, 4), (0x6DC, 4)];
    public static bool IsControlOffset(int offset) => Fields.Any(f => offset >= f.Offset && offset < f.Offset + f.Length);
    // Delay phase, moving time, ring and filter states are outside the DSP snapshot.
    public static bool StableEquals(byte[] a, byte[] b, bool ignoreControls = false) =>
        a.Length == 0xF70 && b.Length == a.Length && Enumerable.Range(0, a.Length)
        .All(i => i is >= 0xF6C and < 0xF70 || (ignoreControls && IsControlOffset(i)) || a[i] == b[i]);
    internal static void ValidateActive(byte[] bank, byte[] dsp, byte[] physical)
    {
        if (bank.Length != 48 || dsp.Length != 0xF70 || physical.Length != 2 || physical[0] > 2 || physical[1] != 1)
            throw new FormatException("Contexto de delay inválido.");
        CubeBabyProfile.ValidateBank(bank);
        if (bank[physical[0] * 16 + 11] != 1 || dsp[0x694] != 3)
            throw new FormatException("Ative a seção delay/modulação no preset físico antes de ler.");
        if (dsp[0x63C] != 0) throw new FormatException("Desligue chorus/phaser para esta edição em RAM de delay.");
        if (dsp[0x695] != 0 || !dsp.AsSpan(0x6A4, 56).SequenceEqual(Filters))
            throw new FormatException("Estrutura de delay desconhecida; edição em RAM bloqueada.");
        Read(dsp);
    }
}

public sealed record DelayPreset(string Format, string Mapping, string Identity, DelayParameters Parameters)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true };
    public static string Export(string identity, DelayParameters parameters)
    {
        parameters.Validate(); PhaserPreset.ValidateIdentity(identity);
        return JsonSerializer.Serialize(new DelayPreset("cubecontrol-delay-v1", DelayParameters.Mapping, identity, parameters), Options);
    }
    public static DelayPreset Import(string json)
    {
        if (json.Length > 16384) throw new FormatException("Arquivo de delay excessivamente grande.");
        var p = JsonSerializer.Deserialize<DelayPreset>(json, Options) ?? throw new FormatException("Arquivo vazio.");
        if (p.Format != "cubecontrol-delay-v1" || p.Mapping != DelayParameters.Mapping || p.Parameters == null)
            throw new FormatException("Formato ou mapeamento de delay incompatível.");
        PhaserPreset.ValidateIdentity(p.Identity); p.Parameters.Validate(); return p;
    }
}

public sealed record DelayRecovery(string Format, string Mapping, string Identity, byte[] Bank,
    byte[] PhysicalState, byte[] OriginalDsp, DelayParameters Previous, DelayParameters Target, string Integrity)
{
    private string Digest() => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        JsonSerializer.SerializeToUtf8Bytes(new { Format, Mapping, Identity, Bank, PhysicalState, OriginalDsp, Previous, Target })));
    internal DelayRecovery Seal() => this with { Integrity = Digest() };
    public void Validate()
    {
        if (Format != "cubecontrol-delay-recovery-v1" || Mapping != DelayParameters.Mapping ||
            Bank is not { Length: 48 } || PhysicalState is not { Length: 2 } || OriginalDsp is not { Length: 0xF70 } || Previous == null || Target == null)
            throw new FormatException("Backup de delay inválido.");
        PhaserPreset.ValidateIdentity(Identity); Previous.Validate(); Target.Validate();
        DelayParameters.ValidateActive(Bank, OriginalDsp, PhysicalState);
        if (Integrity != Digest()) throw new FormatException("Integridade do backup de delay divergiu.");
    }
}
