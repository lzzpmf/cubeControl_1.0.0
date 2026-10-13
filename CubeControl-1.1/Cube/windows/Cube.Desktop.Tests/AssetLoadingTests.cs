using System.IO;
using System.Resources;
using System.Collections;
using System.Windows;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using System.Windows.Media;

internal static class AssetLoadingTests
{
    public static Task Run()
    {
        foreach (var entry in AssetPaths.Images)
            if (GearItem.ReadImage(AssetPaths.Image(entry.Key)) == null)
                throw new Exception("External asset missing or invalid: " + entry.Value);
        var selection = Directory.GetFiles(AssetPaths.CabinetSelection, "*.png");
        if (selection.Length != 14 || selection.Any(path => GearItem.ReadImage(path) == null))
            throw new Exception("Cabinet selection must retain all 14 decodable options");
        if (GearItem.ReadImage(AssetPaths.Device("SINCO-CubeBaby.png")) == null)
            throw new Exception("Device image unavailable");
        var catalog = Cube.Desktop.Documentation.DocumentCatalog.Load(AssetPaths.Documents);
        foreach (var entry in catalog.Documents)
            foreach (var version in entry.Versions)
                if (!File.Exists(catalog.FilePath(version))) throw new Exception("Document unavailable: " + version.Path);
        using var resources = typeof(App).Assembly.GetManifestResourceStream("CubeControl.g.resources")
            ?? throw new Exception("WPF resources missing");
        using var reader = new ResourceReader(resources);
        var embeddedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry entry in reader)
        {
            string key = (string)entry.Key;
            if (!key.StartsWith("assets/images/", StringComparison.Ordinal) || !key.EndsWith(".png", StringComparison.Ordinal)) continue;
            var uri = new Uri("pack://application:,,,/CubeControl;component/" + key);
            var image = new BitmapImage(uri);
            if (image.PixelWidth == 0) throw new Exception("Invalid WPF image: " + key);
            embeddedImages.Add(key);
        }
        var recordImages = Cube.Desktop.Appearance.ControlFinishCatalog.All
            .Select(f => "assets/images/controls/finishes/" + f.Folder + "/record.png")
            .Append("assets/images/controls/backingtrack/record.png");
        foreach (string key in recordImages)
            if (!embeddedImages.Contains(key)) throw new Exception("REC artwork missing from embedded resources: " + key);
        if (embeddedImages.Count < 20) throw new Exception("Base embedded artwork is incomplete");
        if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Assets", "Archive")))
            throw new Exception("Archive must not be distributed");
        string data = Path.Combine(Path.GetTempPath(), "cube-asset-override-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(data, "Images"));
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 255, 255 }, 4)));
        using (var file = File.Create(Path.Combine(data, "Images", "custom.png"))) encoder.Save(file);
        string preferences = Path.Combine(data, "gear-images-v1.json");
        GearImagePreferences.Save(preferences, new Dictionary<string, string> { ["amp-0"] = "custom.png" });
        using (var model = new EditorViewModel(data))
        {
            if (model.Amps[0].Image is not BitmapSource { PixelHeight: 1200 })
                throw new Exception("Saved custom image must override relocated default");
            model.ResetImage.Execute(null);
            if (model.Amps[0].Image is not BitmapSource restored
                || restored.PixelHeight != ((BitmapSource)model.Amps[0].DefaultImage!).PixelHeight
                || GearImagePreferences.Load(preferences, "unused").ContainsKey("amp-0"))
                throw new Exception("Reset must restore relocated default and persist removal");
        }
        Console.WriteLine("PASS custom images: saved override loads; reset restores default and persists.");
        Console.WriteLine($"PASS assets: {AssetPaths.Images.Count} external mappings, 14 cabinet options, device, PDF, {embeddedImages.Count} WPF images; archive excluded.");
        return Task.CompletedTask;
    }
}
