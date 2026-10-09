using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cube.Desktop.ViewModels;

// Entries live for the picker lifetime; realized rows request cached, small thumbnails.
public sealed class EquipmentImageEntry(string path, string category) : Observable
{
    private static readonly System.Threading.SemaphoreSlim decoders = new(2);
    public string Path => path;
    private string? relativeId;
    public string Id => relativeId ?? path;
    public string Category => category;
    public string Name => System.IO.Path.GetFileNameWithoutExtension(path);
    private ImageSource? thumbnail;
    private bool requested;
    public ImageSource? Thumbnail
    {
        get { if (!requested) { requested = true; _ = LoadThumbnail(); } return thumbnail; }
    }
    private async Task LoadThumbnail()
    {
        await decoders.WaitAsync();
        try
        {
            thumbnail = await Task.Run(() =>
            {
                try
                {
                    using var stream = File.OpenRead(path);
                    var bitmap = new BitmapImage(); bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 300; bitmap.StreamSource = stream; bitmap.EndInit(); bitmap.Freeze();
                    return (ImageSource?)bitmap;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or FormatException) { return null; }
            });
            Notify(nameof(Thumbnail));
        }
        finally { decoders.Release(); }
    }
    public static EquipmentImageEntry[] Discover(string root, string? alternatives = null)
    {
        return Scan(root, "Equipment").Concat(alternatives == null ? [] : Scan(alternatives, "Catalog"))
            .OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(i => i.Id, StringComparer.Ordinal).ToArray();
    }

    public static bool IsEquipmentImage(string path) =>
        System.IO.Path.GetExtension(path).ToLowerInvariant() is ".png" or ".jpg" or ".jpeg"
        && !System.Text.RegularExpressions.Regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(path),
            @"^(?:Botão|Botao|Fader|footswitch|knob|switch|interruptor)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private static IEnumerable<EquipmentImageEntry> Scan(string root, string source)
    {
        if (!Directory.Exists(root)) yield break;
        foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(IsEquipmentImage))
        {
            string relative = System.IO.Path.GetRelativePath(root, file).Replace('\\', '/');
            string category = relative.Split('/')[0] switch
            {
                "Amps" or "Cabeçotes" => "Amplificadores",
                "Cabinets" or "Caixas" => "Gabinetes",
                "Effects" or "Pedais" => "Efeitos",
                "Devices" or "Dispositivos" or "Equipment" => "Dispositivos",
                "Microphones" => "Microfones",
                "Guitarras" => "Guitarras", "Racks" => "Racks", _ => "Todos"
            };
            yield return new EquipmentImageEntry(file, category) { relativeId = source + "/" + relative };
        }
    }
}
