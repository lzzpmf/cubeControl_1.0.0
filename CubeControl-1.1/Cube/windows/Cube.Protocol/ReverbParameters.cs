using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Protocol;

public sealed record ReverbSnapshot(byte[] Bank, byte[] Dsp, byte[] PhysicalState);

// Rates/delay lengths and the table-loaded unused byte are deliberately not editable.
public sealed record ReverbParameters(float Dry, float Wet, int Decay, int ReturnLevel, int Damping)
{
    public const string Mapping = "cube-baby-reverb-live-8347f8-v1";
    internal static readonly byte[] DefaultBlock = Convert.FromHexString("005000282a2c2e303200000000000000000000000000a904eb04270565059f05e705c1018d02ee000000803f000000006dc8813feca4d2bf7270313f0133d3bf21e5333f00000000000000008c85793f0ea1d8bf3a92533f0ea1d8bfd7174d3f00000000000000007479573f470463be9e781e3e1ef9dbbea94c513e0000000000000000");
    internal static readonly byte[] FeedbackTable = Convert.FromHexString("7f0b740a9609c508110846071f1bdd19c618b117b91693151c24fc22fe21ff201720001faa29b028d127f02622262925662d8b2cc82b022b4b2a6d291230512fa42ef42d512d8b2c14326731cc302e309c2fe92ea33307337b32eb316731c430e2345434d4335133d8324332e7356435ef34763407347e33c0364736db356b350435853477370737a2363b36db3564351538ac374e37ed36933624369d383b38e23787373337ca361439b73864380e38bf375c37");
    public void Validate()
    {
        if (!float.IsFinite(Dry) || Dry is < 0 or > 1 || !float.IsFinite(Wet) || Wet is < 0 or > 0.9f ||
            Decay is < 1 or > 15 || ReturnLevel is < 0 or > 80 || Damping is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(ReverbParameters), "Parâmetros fora dos limites do reverb.");
    }
    internal static byte[] DampingBytes(int step) => Enumerable.Range(0, 6).Select(i => (byte)((40 + 2 * i) * step / 10)).ToArray();
    public static ReverbParameters Read(byte[] dsp)
    {
        if (dsp.Length != 0xF70) throw new FormatException("Snapshot DSP inválido.");
        int decay = Enumerable.Range(1, 15).FirstOrDefault(i => dsp.AsSpan(0x6EA, 12).SequenceEqual(FeedbackTable.AsSpan((i - 1) * 12, 12)));
        int damping = Enumerable.Range(0, 11).FirstOrDefault(i => dsp.AsSpan(0x6E3, 6).SequenceEqual(DampingBytes(i)), -1);
        var p = new ReverbParameters(BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(0x708)),
            BinaryPrimitives.ReadSingleLittleEndian(dsp.AsSpan(0x70C)), decay, dsp[0x6E1], damping);
        p.Validate(); return p;
    }
    internal byte[] ApplyTo(byte[] dsp)
    {
        Validate(); var result = dsp.ToArray();
        result[0x6E1] = (byte)ReturnLevel;
        DampingBytes(Damping).CopyTo(result, 0x6E3);
        FeedbackTable.AsSpan((Decay - 1) * 12, 12).CopyTo(result.AsSpan(0x6EA));
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x708), Dry);
        BinaryPrimitives.WriteSingleLittleEndian(result.AsSpan(0x70C), Wet);
        return result;
    }
    internal static readonly (int Offset, int Length)[] Fields = [(0x6E1, 1), (0x6E3, 6), (0x6EA, 12), (0x708, 4), (0x70C, 4)];
    public static bool IsControlOffset(int offset) => Fields.Any(f => offset >= f.Offset && offset < f.Offset + f.Length);
    // Only the separately observed derived global-volume float is excluded. Reverb audio state lives outside this snapshot.
    public static bool StableEquals(byte[] a, byte[] b, bool ignoreControls = false) =>
        a.Length == 0xF70 && b.Length == a.Length && Enumerable.Range(0, a.Length)
        .All(i => i is >= 0xF6C and < 0xF70 || (ignoreControls && IsControlOffset(i)) || a[i] == b[i]);
    internal static void ValidateActive(byte[] bank, byte[] dsp, byte[] physical)
    {
        if (bank.Length != 48 || dsp.Length != 0xF70 || physical.Length != 2 || physical[0] > 2 || physical[1] != 1)
            throw new FormatException("Contexto de reverb inválido.");
        CubeBabyProfile.ValidateBank(bank);
        int slot = physical[0];
        if (bank[slot * 16 + 3] == 0 || bank[slot * 16 + 10] != 1 || dsp[0x6E0] != 3)
            throw new FormatException("Ative o reverb e a seção reverb/cabinet no preset físico antes de ler.");
        // Modulation must be off: its dynamic masks are algorithm-dependent and not part of this profile.
        if (dsp[0x63C] != 0) throw new FormatException("Desligue a modulação para esta edição em RAM de reverb.");
        for (int i = 1; i < 0x84; i++)
            if (!IsControlOffset(0x6E0 + i) && i != 2 && dsp[0x6E0 + i] != DefaultBlock[i])
                throw new FormatException("Estrutura de reverb desconhecida; edição em RAM bloqueada.");
        if (dsp[0x6E2] != bank[slot * 16 + 3] * 8) throw new FormatException("Tabela de reverb divergiu.");
        Read(dsp);
    }
}

public sealed record ReverbPreset(string Format, string Mapping, string Identity, ReverbParameters Parameters)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true };
    public static string Export(string identity, ReverbParameters parameters)
    {
        parameters.Validate(); PhaserPreset.ValidateIdentity(identity);
        return JsonSerializer.Serialize(new ReverbPreset("cubecontrol-reverb-v1", ReverbParameters.Mapping, identity, parameters), Options);
    }
    public static ReverbPreset Import(string json)
    {
        if (json.Length > 16384) throw new FormatException("Arquivo de reverb excessivamente grande.");
        var p = JsonSerializer.Deserialize<ReverbPreset>(json, Options) ?? throw new FormatException("Arquivo vazio.");
        if (p.Format != "cubecontrol-reverb-v1" || p.Mapping != ReverbParameters.Mapping || p.Parameters == null)
            throw new FormatException("Formato ou mapeamento de reverb incompatível.");
        PhaserPreset.ValidateIdentity(p.Identity); p.Parameters.Validate(); return p;
    }
}

public sealed record ReverbRecovery(string Format, string Mapping, string Identity, byte[] Bank,
    byte[] PhysicalState, byte[] OriginalDsp, ReverbParameters Previous, ReverbParameters Target)
{
    public void Validate()
    {
        if (Format != "cubecontrol-reverb-recovery-v1" || Mapping != ReverbParameters.Mapping ||
            Bank is not { Length: 48 } || PhysicalState is not { Length: 2 } || OriginalDsp is not { Length: 0xF70 } || Previous == null || Target == null)
            throw new FormatException("Backup de reverb inválido.");
        PhaserPreset.ValidateIdentity(Identity); Previous.Validate(); Target.Validate();
        ReverbParameters.ValidateActive(Bank, OriginalDsp, PhysicalState);
    }
}
