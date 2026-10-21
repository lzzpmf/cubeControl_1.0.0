using Microsoft.UI.Xaml;
using System.IO;
using System.Text.Json.Nodes;

namespace Cube.WinUI.Platform;
internal static class Preferences
{
    private static JsonObject ReadForSave(string path)
    {
        try { if (File.Exists(path) && JsonNode.Parse(File.ReadAllText(path)) is JsonObject saved) return saved; }
        catch (System.Text.Json.JsonException) { }
        return new JsonObject { ["Version"] = 1, ["Mode"] = "System", ["Finish"] = "Official" };
    }
    public static Cube.Desktop.Appearance.ControlFinish LoadFinish(string directory)
    {
        try
        {
            var saved = JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "appearance-v1.json")));
            if (Enum.TryParse<Cube.Desktop.Appearance.ControlFinish>(saved?["Finish"]?.GetValue<string>(), out var finish) && Enum.IsDefined(finish)) return finish;
        }
        catch (Exception ex) when (ex is IOException or System.Text.Json.JsonException or UnauthorizedAccessException or InvalidOperationException) { }
        return Cube.Desktop.Appearance.ControlFinish.Official;
    }
    public static void SaveFinish(string directory, Cube.Desktop.Appearance.ControlFinish finish)
    {
        string path = Path.Combine(directory, "appearance-v1.json");
        var saved = ReadForSave(path);
        saved["Finish"] = finish.ToString();
        Directory.CreateDirectory(directory); File.WriteAllText(path + ".tmp", saved.ToJsonString()); File.Move(path + ".tmp", path, true);
    }
    public static ElementTheme LoadTheme(string directory)
    {
        try
        {
            var saved = JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "appearance-v1.json")));
            return saved?["Mode"]?.GetValue<string>() switch { "Light" => ElementTheme.Light, "Dark" => ElementTheme.Dark, _ => ElementTheme.Default };
        }
        catch (Exception ex) when (ex is IOException or System.Text.Json.JsonException or UnauthorizedAccessException or InvalidOperationException) { return ElementTheme.Default; }
    }
    public static void SaveTheme(string directory, ElementTheme theme)
    {
        string path = Path.Combine(directory, "appearance-v1.json");
        var saved = ReadForSave(path);
        saved["Mode"] = theme == ElementTheme.Default ? "System" : theme.ToString();
        Directory.CreateDirectory(directory); File.WriteAllText(path + ".tmp", saved.ToJsonString()); File.Move(path + ".tmp", path, true);
    }
}
