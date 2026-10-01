using System.Security.Cryptography;
using System.Text.Json;

namespace Cube.Protocol;

public sealed record CabinetIdentity(int Slot, string Hash, string Name, string Category, string? FactoryId);
public sealed class CabinetInventory
{
    private readonly Dictionary<int, CabinetIdentity> slots = [];
    private readonly Dictionary<string, int> originals = [];
    public IReadOnlyList<CabinetIdentity> Slots => slots.Values.OrderBy(x => x.Slot).ToArray();
    public bool Complete => slots.Count == 8;
    public void SetFactory(FactoryResources? resources)
    {
        originals.Clear(); if (resources == null) return;
        for (int i = 0; i < 8; i++) originals[Hash(resources.IrSectors[i])] = i;
    }
    public static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    public CabinetIdentity Update(int slot, byte[] bytes, string? localName = null)
    {
        if (slot is < 0 or > 7 || bytes.Length != 4096) throw new ArgumentException("Setor de cabinet inválido.");
        string hash = Hash(bytes); bool known = originals.TryGetValue(hash, out int factorySlot);
        string name = known ? EquipmentCatalog.Cabinets[factorySlot].Name : localName ?? $"IR personalizado — slot {slot + 1}";
        return slots[slot] = new(slot, hash, name, known ? "Original identificado" : "IR personalizado", known ? $"factory-cab-{factorySlot}" : null);
    }
    public CabinetIdentity? Resolve(string? reference)
    {
        if (!Complete || reference == null) return null;
        return Slots.FirstOrDefault(x => x.Hash == reference || x.FactoryId == reference);
    }
    public void Clear() => slots.Clear();
}

public sealed class VisualPreferences
{
    public int Version { get; set; } = 1;
    public bool AutomaticCabinetMatching { get; set; }
    public Dictionary<int, string> Matches { get; set; } = [];
    public Dictionary<string, string> Images { get; set; } = [];
    public Dictionary<string, string> IrNames { get; set; } = [];
    public static VisualPreferences Load(string path)
    {
        if (!File.Exists(path)) return new();
        var data = JsonSerializer.Deserialize<VisualPreferences>(File.ReadAllText(path)) ?? throw new FormatException("Preferências inválidas.");
        if (data.Version != 1 || data.Matches == null || data.Images == null || data.IrNames == null || data.Matches.Keys.Any(k => k is < 0 or > 8)) throw new FormatException("Versão de preferências não suportada.");
        // Legacy suggestions used factory IDs; manually saved associations use content hashes.
        foreach (var key in data.Matches.Where(pair => pair.Value?.StartsWith("factory-cab-", StringComparison.Ordinal) == true).Select(pair => pair.Key).ToArray())
            data.Matches.Remove(key);
        return data;
    }
    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, path, true);
    }
}
