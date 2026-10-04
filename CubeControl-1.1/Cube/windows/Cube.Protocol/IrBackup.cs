using System.Security.Cryptography;
using System.Text.Json;

namespace Cube.Protocol;

public sealed record IrBackup(string Format, string Identity, string IdentityHex, int Slot, DateTimeOffset CapturedAt, string Sha256, string BinaryFile)
{
    public static async Task<string> Save(string directory, string identity, string identityHex, int slot, byte[] data)
    {
        if (data.Length != 4096 || slot is < 0 or > 7) throw new ArgumentException("Backup IR inválido.");
        Directory.CreateDirectory(directory);
        string stem = $"IR-{slot + 1}-{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}";
        string binary = stem + ".bin", json = Path.Combine(directory, stem + ".json");
        await File.WriteAllBytesAsync(Path.Combine(directory, binary), data);
        var record = new IrBackup("cubecontrol-ir-v1", identity, identityHex, slot, DateTimeOffset.Now, Convert.ToHexString(SHA256.HashData(data)), binary);
        await File.WriteAllTextAsync(json, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
        _ = await Load(json); // Verify on disk before authorizing erase.
        return json;
    }

    public static async Task Export(string path, string identity, string identityHex, int slot, byte[] data)
    {
        if (data.Length != 4096 || slot is < 0 or > 7) throw new ArgumentException("Backup IR inválido.");
        string binary = Path.GetFileNameWithoutExtension(path) + "-" + Guid.NewGuid().ToString("N") + ".bin";
        await File.WriteAllBytesAsync(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, binary), data);
        var record = new IrBackup("cubecontrol-ir-v1", identity, identityHex, slot, DateTimeOffset.Now,
            Convert.ToHexString(SHA256.HashData(data)), binary);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
        _ = await Load(path);
    }

    public static async Task<(IrBackup Info, byte[] Data)> Load(string path)
    {
        var info = JsonSerializer.Deserialize<IrBackup>(await File.ReadAllTextAsync(path)) ?? throw new FormatException("Backup inválido.");
        if (info.Format != "cubecontrol-ir-v1" || info.Identity is not ("SINCO-CubeBaby" or "CubeBaby") || info.Slot is < 0 or > 7 ||
            string.IsNullOrWhiteSpace(info.BinaryFile) || Path.GetFileName(info.BinaryFile) != info.BinaryFile)
            throw new FormatException("Backup incompatível.");
        byte[] data = await File.ReadAllBytesAsync(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, info.BinaryFile));
        if (data.Length != 4096 || !Convert.ToHexString(SHA256.HashData(data)).Equals(info.Sha256, StringComparison.OrdinalIgnoreCase)) throw new FormatException("Checksum do backup não confere.");
        return (info, data);
    }
}

public sealed record IrRamBackup(string Format, string IdentityHex, byte Flag, string Sha256, string PayloadHex)
{
    public static async Task Save(string path, string identityHex, IrRamSnapshot snapshot)
    {
        if (snapshot.Payload.Length != 2052) throw new ArgumentException("Snapshot RAM inválido.");
        var record = new IrRamBackup("cubecontrol-ir-ram-v1", identityHex, snapshot.Flag,
            Convert.ToHexString(SHA256.HashData(snapshot.Payload)), Convert.ToHexString(snapshot.Payload));
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
        _ = await Load(path);
    }
    public static async Task<(string IdentityHex, IrRamSnapshot Snapshot)> Load(string path)
    {
        var record = JsonSerializer.Deserialize<IrRamBackup>(await File.ReadAllTextAsync(path)) ?? throw new FormatException("Snapshot RAM inválido.");
        if (record.Format != "cubecontrol-ir-ram-v1") throw new FormatException("Formato de snapshot RAM desconhecido.");
        byte[] payload = Convert.FromHexString(record.PayloadHex);
        if (payload.Length != 2052 || Convert.ToHexString(SHA256.HashData(payload)) != record.Sha256) throw new FormatException("Checksum do snapshot RAM não confere.");
        return (record.IdentityHex, new(record.Flag, payload));
    }
}
