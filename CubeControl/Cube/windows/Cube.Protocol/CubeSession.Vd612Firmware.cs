namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private bool effectsFirmwareVd612;

    // CubeBaby_VD612.up SHA-256:
    // 1311c2f02c36ae632716c47a72d26cebd95a06042f19d5301199a7c92b44df03.
    // Matched against read-only captures from the connected SINCO-CubeBaby.
    // The RAM image at CPU 0x830C00 maps to file offset 0xAA00; DSP is 0x834724.
    // Flash CPU addresses map to file offset address - 0xC00000 + 0x100.
    private const uint Vd612DspBase = 0x834724;
    private const uint Vd612ReaderOffset = unchecked(0x832510u - Vd612DspBase);
    private const string Vd612ReaderHash = "5ae3d652403d374cd15795e022930cfaae08be935ca7b70a05e48b5d1345cfde";

    private static readonly (uint Address, int Size, string Hash)[] Vd612Signatures =
    [
        // Copied RAM code: preset application, modulation/delay/reverb workers and oscillators.
        (0x830C00, 0x3200, "983121d32bea04960a75ba05d34322e48c126e0db3b134b9fb170a0859f5fb5b"),
        // SysEx read/write dispatcher and effect initialization.
        (0xC49F00, 0x600, "9153c49c51c40e1d4d3fe393d47faf5922e11e39683e1ca50f83c3a4560b9ec6"),
        (0xC13800, 0x800, "888ae8c5723d92b981b570e5da606459470c449261251e7e51873283045d63f1"),
        // Immutable filter coefficients, reverb decay table and effect dispatch tables.
        (0xC48900, 0xA00, "ec34bb1569d178a1f1db0408ea35ca63c6dfa8433e2cdbaabea4350da29a8f33"),
    ];

    private async Task VerifyVd612Firmware(CancellationToken ct)
    {
        foreach (var range in Vd612Signatures)
            if (!HashMatches(await ReadChunks(4, unchecked(range.Address - Vd612DspBase), range.Size, ct), range.Hash))
                throw new NotSupportedException("Firmware VD612 divergente do perfil de efeitos verificado. A edição normal de presets continua disponível.");

        var bank = await ReadInternal(0, 48, ct);
        var alias = await ReadInternal(unchecked(0x80CBBCu - Vd612DspBase), 48, ct, 4);
        if (!bank.SequenceEqual(alias)) throw new IOException("Alias do banco VD612 diverge; releia antes de preparar o efeito.");
        var state = await ReadInternal(unchecked(0x80CB16u - Vd612DspBase), 2, ct, 4);
        if (state[0] > 2 || state[1] > 1) throw new IOException("Estado físico VD612 não reconhecido; releia o pedal.");

        effectsFirmwareVd612 = true;
        effectsFirmwareB = false;
        phaserFirmwareVerified = chorusFirmwareVerified = delayFirmwareVerified = reverbFirmwareVerified = true;
        EffectsFirmware = "CubeBaby_VD612";
    }
}
