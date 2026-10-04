using System.Text.Json;

namespace Cube.Protocol;

// This store contains artwork associations only, never device or session state.
public static class GearImagePreferences
{
    private sealed record Store(int Version, Dictionary<string, string> Images);
    private static bool Allowed(string key) => key.StartsWith("amp-", StringComparison.Ordinal)
        || key == "cab-off" || (key.Length == 64 && key.All(Uri.IsHexDigit));
    public static Dictionary<string, string> Load(string path, string legacyPath)
    {
        bool migrate = !File.Exists(path);
        string source = migrate ? legacyPath : path;
        var images = new Dictionary<string, string>();
        try
        {
            if (File.Exists(source))
            {
                using var json = JsonDocument.Parse(File.ReadAllText(source));
                if (json.RootElement.TryGetProperty("Images", out var entries) && entries.ValueKind == JsonValueKind.Object)
                    foreach (var entry in entries.EnumerateObject())
                        if (Allowed(entry.Name) && entry.Value.ValueKind == JsonValueKind.String && entry.Value.GetString() is { Length: > 0 } file)
                            images[entry.Name] = Path.GetFileName(file);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        // Write a migration marker even when the legacy document is invalid.
        if (migrate) Save(path, images);
        return images;
    }
    public static void Save(string path, Dictionary<string, string> images)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var allowed = images.Where(p => Allowed(p.Key)).ToDictionary(p => p.Key, p => Path.GetFileName(p.Value));
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(new Store(1, allowed)));
        File.Move(path + ".tmp", path, true);
    }
}
