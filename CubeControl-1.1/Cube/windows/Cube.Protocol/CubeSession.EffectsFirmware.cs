using System.Security.Cryptography;

namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private bool effectsFirmwareB;
    public string? EffectsFirmware { get; private set; }
    private uint EffectsPhysicalStateOffset => effectsFirmwareB ? 0xFFFD81F6u : 0xFFFD8202u;
    private uint EffectsBankAliasOffset => effectsFirmwareB ? 0xFFFD8270u : 0xFFFD827Cu;

    // Extracted from cubebaby_B_20220706.up (SHA-256
    // 9184852bdebfa1d939ad21a250a579104ec1e3de17f072edbd8311015daad216).
    // Each full range is pinned independently; no wildcard/hash bypass or uniform delta.
    private static readonly (uint Legacy, uint Address, int Size, string Hash)[] FirmwareBSignatures =
    [
        (0x832108, 0x832150, 0xC, "98fc02a2cabd77eebec1a962bbbf4c2f774ae6cb47452e5ba69a6519b31cebe8"),
        (0x83239C, 0x83239C, 0x104, "ec47ca4bd39e6b067e230f9229284ac7de2144284bb701b307578f0ab29fa876"),
        (0x831D84, 0x831DCC, 0x46, "0a4a8557dfbaa001fcc7f2c275560d0aea87d4ca5c9d494c6314885722854f97"),
        (0x831F5C, 0x831FA4, 0x1B8, "4556fb1e97957fa8f1c4632b129c175f103962d6a7a72d60471cccef2a898edd"),
        (0x8327C4, 0x8327C4, 0x88, "1456a3ca957bdfaf603e50156a2d95e5e32fb78e2d153ff65af4cb22f498e4c7"),
        (0x8326B4, 0x8326B4, 0x110, "9459bbe1e5d208ad5861b78a303706c3bb052d789ea1b311da3c4d94027c89a4"),
        (0x832188, 0x832188, 0xCC, "324522c9c470193f702e49921eb87f6b4183b279e114376cf1a29106ea4aeddf"),
        (0x833540, 0x833540, 0x54, "78adc5ec3bc6558618428322d2d5d2394a0e9d3d091a32e297bc01572b0697bb"),
        (0x831AC0, 0x831B08, 0x49C, "f736c0f94cce148bbd6bdbd226821cace31e9bdd46dfe7737f18ff9950081d14"),
        (0xC13B04, 0xC13AF4, 0x30, "034b4ecbdd49b0b7320eb2c7868efe0218c1e1f596f2d349a40212269dbace51"),
        (0x80A91E, 0x80A92A, 0x2A, "465961c91038ea88c1490bc3e4cf0dbf9acc3b550e6f096398d37e25bf685b83"),
        (0x831AC0, 0x831B08, 0x484, "3200ac91fe2c974144fa5def0de417ff24c8ec36e5d1b669aacf5554f464e6e3"),
        (0x833724, 0x833724, 0x150, "4b1049723ee8c84e1b343128b753ba8c45a9d61f41aea22c0c44c829f1823335"),
        (0x832DA8, 0x832DA8, 0x338, "4905e920f96fe55663e6b79468cca245188671a2ce2ce2165b2658a0e273b460"),
        (0xC13C3C, 0xC13C2C, 0xC4, "92af4372f8e352cdd41ffaec5c91bf0adb45589eb5a5873e06b210eec15f9a73"),
        (0x833562, 0x833562, 0x32, "68bb0fe53c47af78c5da900254ec78601c680d2a2e60eee0f7a919498341ea51"),
        (0x80A8BC, 0x80A8C8, 0x62, "2b66bf16d3d6b3fd1407e1c51bc893c1f3ce28e3451410c97647c66d05b201b0"),
        (0xC488E0, 0xC4883C, 0x38, "2a25b41e50d042188203cc21dd4f3834beb1824934114fb5ab9c79c8d710085a"),
        (0x8359A0, 0x8359A0, 0x6C, "1b7f8ab5a9f431c89e1fec2060c3b71993b416dc20e8d5526b2169e91b59ea79"),
        (0x833978, 0x833978, 0x284, "08eb26cc26a4cd54fb96a74571ef512e33ddf8c03e70c75a20eaba5cabb1b469"),
        (0x833904, 0x833904, 0x74, "2350a62fd74025c471c39dfd49a7975d7f7980a4b022e91144f33a53658ec802"),
        (0x8338E0, 0x8338E0, 0x24, "063117fcd1176d4d25249908ee50718ca040709c26cf021207ecc39dfaf00147"),
        (0xC13D00, 0xC13CF0, 0x200, "de0493bb57ba14d93318a24eaba6330df7d2e44603239cd02446217038b76d70"),
        (0xC48C98, 0xC48BF4, 0x84, "caee96fe76d1f0a2a3d2bc732290ba8836e9537475aaf5037b1acb0d78191284"),
        (0xC4881C, 0xC48778, 0xB4, "f875a55d71d4c82934f456afa1c681a7f68a33deaa49a272bdf9a8a4abb19502"),
        (0xC488D0, 0xC4882C, 0x10, "bb0f1d792c9c2475436b133de1c381ab37f3d8fd243caca4b66656456fd3b0ef"),
        (0xC48D38, 0xC48C94, 0x78, "c4e784896d3fc6bf501bbf71537cacbbff8ad4f0a6060df5d9995bf7da523fc2"),
    ];

    private uint EffectsAddress(uint legacy)
    {
        if (effectsFirmwareB)
        {
            foreach (var range in FirmwareBSignatures)
                if (range.Legacy == legacy) return unchecked(range.Address - 0x8347F8u);
            legacy = legacy switch
            {
                0x80C374 => 0x80C378, 0x80C37C => 0x80C380, 0x80C388 => 0x80C38C,
                0x80C3DC => 0x80C3E0, 0xC487AC => 0xC48708, 0xC487E4 => 0xC48740,
                0x817F86 => 0x817F7E, 0x80FF84 => 0x80FF7C, 0x81823C => 0x818234,
                0x8181BC => 0x8181B4, 0x818228 => 0x818220,
                _ => legacy
            };
        }
        return unchecked(legacy - 0x8347F8u);
    }

    private async Task VerifyEffectsRange(uint legacy, int size, string legacyHash, string effect, CancellationToken ct)
    {
        string expected = effectsFirmwareB
            ? FirmwareBSignatures.Single(r => r.Legacy == legacy && r.Size == size).Hash : legacyHash;
        var bytes = await ReadChunks(4, EffectsAddress(legacy), size, ct);
        if (!HashMatches(bytes, expected))
            throw new NotSupportedException($"Firmware incompatível com o criador de {effect}: a assinatura de código não corresponde ao perfil verificado. A edição normal de presets continua disponível.");
    }

    private static bool HashMatches(byte[] bytes, string hash) =>
        Convert.ToHexString(SHA256.HashData(bytes)).Equals(hash, StringComparison.OrdinalIgnoreCase);

    // Bounded research capture only, without approving firmware compatibility or writing.
    public async Task<Dictionary<string, byte[]>> ReadEffectsFirmwareBEvidence(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            var result = new Dictionary<string, byte[]>();
            foreach (var range in FirmwareBSignatures)
                result[$"{range.Address:X8}-{range.Size:X}"] = await ReadChunks(4,
                    unchecked(range.Address - 0x8347F8u), range.Size, ct);
            foreach (var (address, size) in new (uint, int)[] { (0x80C380, 4), (0x80C38C, 4), (0x80C3E0, 12) })
                result[$"{address:X8}-{size:X}"] = await ReadChunks(4, unchecked(address - 0x8347F8u), size, ct);
            result["bank"] = await ReadInternal(0, 48, ct);
            result["bank-alias"] = await ReadInternal(0xFFFD8270, 48, ct, 4);
            result["physical"] = await ReadInternal(0xFFFD81F6, 2, ct, 4);
            return result;
        }
        finally { gate.Release(); }
    }

    public async Task<string> VerifyEffectsFirmware(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            await VerifyPhaserFirmware(ct);
            await VerifyChorusFirmware(ct);
            await VerifyDelayFirmware(ct);
            await VerifyReverbFirmware(ct);
            await ReadPhaserCore(ct); // Bank alias and physical context also use the selected profile.
            return EffectsFirmware!;
        }
        finally { gate.Release(); }
    }
}
