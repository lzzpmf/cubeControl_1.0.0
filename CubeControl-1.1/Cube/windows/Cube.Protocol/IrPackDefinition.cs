using System.Security.Cryptography;
using System.Text.Json;

namespace Cube.Protocol;

public sealed record IrPackEntry(int Slot, string RelativePath, string Sha256, string Name, int Distance = 50);
public sealed record IrPackDefinition
{
    public int Version { get; init; } = 1;
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Group { get; init; } = "Meus packs";
    public IrPackEntry[] Entries { get; init; } = [];
}

public sealed partial class IrPackStore(string libraryDirectory, string dataDirectory)
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public string DirectoryPath => Path.Combine(dataDirectory, "IrPacks");
    public string LibraryDirectory => libraryDirectory;
    public string ImportedDirectory => Path.Combine(dataDirectory, "ImportedIrs");
    public static void Validate(IrPackDefinition pack)
    {
        if (pack.Version != 1 || !Guid.TryParseExact(pack.Id, "N", out _) || string.IsNullOrWhiteSpace(pack.Name)
            || pack.Name.Length > 120 || pack.Description == null || pack.Description.Length > 4000
            || pack.Entries == null || pack.Entries.Length is < 1 or > 8)
            throw new FormatException("Pack inválido ou versão não suportada.");
        if (pack.Entries.Any(e => e == null || e.Slot is < 0 or > 7 || e.Distance is < 0 or > 99
            || string.IsNullOrWhiteSpace(e.Name) || e.Name.Length > 1000 || e.Sha256 == null
            || e.Sha256.Length != 64 || !e.Sha256.All(Uri.IsHexDigit)
            || string.IsNullOrWhiteSpace(e.RelativePath) || e.RelativePath.Length > 1000 || e.RelativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || Path.IsPathRooted(e.RelativePath)
            || e.RelativePath.Contains(':') || e.RelativePath.Split('/', '\\').Any(p => p is ".." or "." or "")
            || !e.RelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            || pack.Entries.Select(e => e.Slot).Distinct().Count() != pack.Entries.Length)
            throw new FormatException("Referência ou posição de IR inválida.");
    }
    public IrPackEntry Reference(int slot, string relativePath, string name, int distance = 50)
    {
        string path = ResolvePath(relativePath);
        if (new FileInfo(path).Length > 32 * 1024 * 1024) throw new FormatException("WAV excede 32 MB.");
        return new(slot, relativePath.Replace('\\', '/'), Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), name, distance);
    }
    private string ResolvePath(string relative)
    {
        bool imported = relative.StartsWith("Imported/", StringComparison.Ordinal);
        string root = Path.GetFullPath(imported ? ImportedDirectory : libraryDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string path = Path.GetFullPath(Path.Combine(root, (imported ? relative[9..] : relative).Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new FormatException("Referência fora da biblioteca.");
        for (var item = new FileInfo(path) as FileSystemInfo; item != null && item.FullName.Length >= root.TrimEnd(Path.DirectorySeparatorChar).Length; item = item is FileInfo f ? f.Directory : ((DirectoryInfo)item).Parent)
            if (item.Exists && (item.Attributes & FileAttributes.ReparsePoint) != 0) throw new FormatException("Referência fora da biblioteca.");
        return path;
    }
    public IrPack Resolve(IrPackDefinition definition)
    {
        Validate(definition);
        var sectors = Enumerable.Range(0, 8).Select(_ => Array.Empty<byte>()).ToArray();
        var problems = new List<string>();
        foreach (var entry in definition.Entries)
        {
            try
            {
                string path = ResolvePath(entry.RelativePath);
                if (!File.Exists(path)) throw new FormatException("IR ausente na biblioteca.");
                if (new FileInfo(path).Length > 32 * 1024 * 1024) throw new FormatException("WAV excede 32 MB.");
                byte[] bytes = File.ReadAllBytes(path);
                if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new FormatException("IR modificado na biblioteca.");
                sectors[entry.Slot] = IrAudio.Decode(bytes).Sector(entry.Distance);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
            { problems.Add($"Slot {entry.Slot + 1}: {entry.Name} — {ex.Message}"); }
        }
        return new(definition.Id, definition.Name, sectors) { Definition = definition, Description = definition.Description, Group = definition.Group, Problem = string.Join("\n", problems) };
    }
    public static IrPackDefinition Read(string path)
    {
        if (new FileInfo(path).Length > 128 * 1024) throw new FormatException("Arquivo de pack excede 128 KB.");
        var pack = JsonSerializer.Deserialize<IrPackDefinition>(File.ReadAllText(path)) ?? throw new FormatException("Pack inválido.");
        Validate(pack); return pack;
    }
    public static void Export(string path, IrPackDefinition pack)
    {
        Validate(pack);
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(temp, JsonSerializer.Serialize(pack, Json)); File.Move(temp, path, true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public void Save(IrPackDefinition pack)
    {
        Validate(pack); Directory.CreateDirectory(DirectoryPath);
        Export(Path.Combine(DirectoryPath, pack.Id + ".cubeirpack.json"), pack with { Group = "Meus packs" });
    }
    public IrPackDefinition Import(string path)
    {
        var pack = Read(path) with { Id = Guid.NewGuid().ToString("N"), Group = "Meus packs" };
        Save(pack); return pack;
    }
    public void Delete(IrPackDefinition pack)
    {
        Validate(pack);
        if (pack.Group != "Meus packs") throw new InvalidOperationException("Pack somente leitura.");
        File.Delete(Path.Combine(DirectoryPath, pack.Id + ".cubeirpack.json"));
    }
    public IReadOnlyList<IrPack> LoadPersonal(out string problems)
    {
        var result = new List<IrPack>(); var errors = new List<string>();
        if (Directory.Exists(DirectoryPath)) foreach (string path in Directory.EnumerateFiles(DirectoryPath, "*.cubeirpack.json"))
        {
            try { result.Add(Resolve(Read(path) with { Group = "Meus packs" })); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or JsonException)
            { errors.Add(Path.GetFileName(path) + ": " + ex.Message); }
        }
        problems = string.Join("\n", errors); return result;
    }
}
