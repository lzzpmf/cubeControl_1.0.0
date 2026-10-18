using System.IO;
using System.Text.Json;

namespace Cube.Desktop.Appearance;

internal sealed class ZoomSettings
{
    public int Version { get; set; } = 1;
    public int Percent { get; set; } = 100;
    public static int Load(string path)
    {
        try
        {
            var value = JsonSerializer.Deserialize<ZoomSettings>(File.ReadAllText(path));
            if (value?.Version == 1 && value.Percent is >= 50 and <= 200 && value.Percent % 10 == 0) return value.Percent;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        return 100;
    }
    public static void Save(string path, int percent)
    {
        if (percent is < 50 or > 200 || percent % 10 != 0) throw new ArgumentOutOfRangeException(nameof(percent));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(new ZoomSettings { Percent = percent }));
        File.Move(path + ".tmp", path, true);
    }
}

internal sealed class ZoomService
{
    public static ZoomService Instance { get; } = new();
    private readonly string path = Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "CubeControl", "zoom-v1.json");
    public int Percent { get; private set; }
    public event Action? Changed;
    private ZoomService() => Percent = ZoomSettings.Load(path);
    public void Select(int percent)
    {
        percent = Math.Clamp(percent, 50, 200);
        ZoomSettings.Save(path, percent);
        Preview(percent);
    }
    internal void Preview(int percent)
    {
        percent = Math.Clamp(percent, 50, 200);
        if (percent == Percent) return;
        Percent = percent;
        Changed?.Invoke();
    }
}
