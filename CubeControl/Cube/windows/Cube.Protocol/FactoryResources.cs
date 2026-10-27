using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Cube.Protocol;

// Resources are read from the user's original CubeSuite installation, not redistributed.
public sealed record FactoryResources(byte[] Presets, byte[][] IrSectors, string SourceDirectory)
{
    public const string ExeHash = "0E0183FD6DC1B347534C9CE1E3677ECDA2A6AE77FE3DA8D6314D43C7C2CFD2E6";
    public const string BfiHash = "2023EE73672E43A0381A37AE5ECB7B1E298836E84B052DF609D4EF9509EE37E5";
    public static FactoryResources Load(string exePath)
    {
        byte[] exe = File.ReadAllBytes(exePath);
        if (Convert.ToHexString(SHA256.HashData(exe)) != ExeHash) throw new FormatException("Esta versão do CubeSuite ainda não foi analisada. Recursos de fábrica não liberados.");
        string directory = Path.GetDirectoryName(Path.GetFullPath(exePath))!;
        byte[] bfi = File.ReadAllBytes(Path.Combine(directory, "bin", "BFI.bin"));
        if (bfi.Length != 69632 || Convert.ToHexString(SHA256.HashData(bfi)) != BfiHash) throw new FormatException("BFI.bin não corresponde à imagem de fábrica analisada.");
        // VA 0x584B94, ImageBase 0x400000. Resolve RVA through the PE section table.
        int pe = BinaryPrimitives.ReadInt32LittleEndian(exe.AsSpan(0x3c));
        int sections = BinaryPrimitives.ReadUInt16LittleEndian(exe.AsSpan(pe + 6));
        int table = pe + 24 + BinaryPrimitives.ReadUInt16LittleEndian(exe.AsSpan(pe + 20));
        int raw = -1;
        for (int i = 0; i < sections; i++)
        {
            var section = exe.AsSpan(table + 40 * i, 40);
            uint address = BinaryPrimitives.ReadUInt32LittleEndian(section[12..]);
            uint size = BinaryPrimitives.ReadUInt32LittleEndian(section[16..]);
            if (0x184b94 >= address && 0x184b94 + 48 <= address + size)
                raw = checked((int)(BinaryPrimitives.ReadUInt32LittleEndian(section[20..]) + 0x184b94 - address));
        }
        if (raw < 0) throw new FormatException("Recurso de presets não localizado.");
        byte[] presets = exe.AsSpan(raw, 48).ToArray(); CubeBabyProfile.ValidateBank(presets);
        return new(presets, Enumerable.Range(0, 8).Select(i => bfi.AsSpan(0x9000 + i * 4096, 4096).ToArray()).ToArray(), directory);
    }
}
