using Cube.Desktop;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;

internal static class GearArtworkTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    public static Task Run()
    {
        var pixels = new byte[200 * 100 * 4];
        for (int y = 20; y < 80; y++) for (int x = 50; x < 150; x++) pixels[(y * 200 + x) * 4 + 3] = 255;
        var padded = BitmapSource.Create(200, 100, 96, 96, PixelFormats.Bgra32, null, pixels, 800); padded.Freeze();
        var bounds = GearArtworkLayout.VisibleBounds(padded);
        Check(bounds == new Rect(50, 20, 100, 60), "Transparent padding must not determine apparent size");
        var viewport = new Size(360, 240);
        var a = GearArtworkLayout.Place(new Size(200, 100), bounds, viewport, 1);
        var b = GearArtworkLayout.Place(new Size(100, 60), new Rect(0, 0, 100, 60), viewport, 1);
        Check(Math.Abs(a.Width / 200 - b.Width / 100) < .0001, "Transparent margins must not change equipment scale");
        Check(Math.Abs(a.X + 100 * a.Width / 200 - 180) < .0001, "Equipment must be centered by visible bounds");
        var empty = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);
        Check(!GearArtworkLayout.VisibleBounds(empty).IsEmpty, "Transparent images must have a safe fallback");
        Check(GearArtworkLayout.Place(new Size(100, 60), bounds, new Size(0, 0), 1).IsEmpty, "Collapsed layout must be safe");

        string folder = Path.GetFullPath("C:/CubeControl-Build/reports/gear-artwork"); Directory.CreateDirectory(folder);
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-artwork-" + Guid.NewGuid().ToString("N")));
        var amps = model.Amps;
        var library = model.IrLibrary.Entries.ToArray();
        Check(library.Length > 0, "IR library is available for artwork coverage");
        foreach (var entry in library)
        {
            string? asset = CabinetArtwork.ForIr(entry.RelativePath, entry.Name);
            Check(asset != null && File.Exists(AssetPaths.Image(asset)), "Library IR has an illustration: " + entry.RelativePath);
        }
        Check(CabinetArtwork.ForIr("unknown.wav", "Unknown custom IR") == null, "Unknown names keep the generic fallback");
        Check(CabinetArtwork.ForIr(@"FENDER\IRs WAV\1x12\Deluxe Reverb Oxford\Big.wav", null) == "fender-deluxe-oxford-1x12.png", "Artwork matching tolerates path separators and casing");
        var inferred = library.GroupBy(e => CabinetArtwork.ForIr(e.RelativePath, e.Name)).Select(g =>
            new GearItem("illustration-" + g.Key, 0, IrViewModel.DisplayName(g.First().FullPath, g.First().Collection), "", "cab", "#C5B77F")).ToArray();
        Check(inferred.Length == 10, "Ten named cabinet families share art across microphone variants");
        var vertical = new[] {
            new GearItem("vertical-neutral", 0, "Gabinete 2×12 Vertical", "", "cab", "#C5B77F"),
            new GearItem("vertical-orange", 0, "Orange 2x12 Vertical Vintage 30", "", "cab", "#C5B77F") };
        Check(CabinetArtwork.ForIr(null, vertical[0].Name) == "cabinet-vertical-2x12.png", "Vertical geometry follows the explicit name and multiplication sign");
        Check(CabinetArtwork.ForIr(null, vertical[1].Name) == "orange-v30-vertical-2x12.png", "Orange vertical name chooses the matching enclosure");
        var cabinets = Enumerable.Range(0, 8).Select(i => new GearItem("cab-" + i, i + 1, Cube.Protocol.EquipmentCatalog.Cabinets[i].Name, "", "cab", "#C5B77F", cabinetId: "pack-bfi-cab-" + i)).ToArray();
        foreach (var group in new[] { amps, cabinets, inferred, vertical })
        {
            var sheet = new WrapPanel { Width = 720, Background = new SolidColorBrush(Color.FromRgb(37, 37, 37)) };
            foreach (var item in group)
            {
                var source = item.DefaultImage ?? throw new Exception("Missing asset " + item.Name);
                var visible = GearArtworkLayout.VisibleBounds(source);
                foreach (var size in new[] { new Size(360, 240), new Size(178, 135), new Size(270, 160) })
                {
                    var placed = GearArtworkLayout.Place(new Size(source.Width, source.Height), visible, size, 1);
                    double scale = placed.Width / source.Width;
                    var occupied = new Rect(placed.X + visible.X * scale, placed.Y + visible.Y * scale, visible.Width * scale, visible.Height * scale);
                    var frame = GearArtworkLayout.Frame(size); frame.Inflate(.01, .01);
                    Check(frame.Contains(occupied), "Equipment exceeds limiter: " + item.Name);
                    Check(Math.Abs(scale - placed.Height / source.Height) < .0001, "Aspect ratio distorted: " + item.Name);
                    Check(Math.Abs(occupied.X + occupied.Width / 2 - size.Width / 2) < .001, "Horizontal centering failed");
                    double area = occupied.Width * occupied.Height;
                    Check(area / (GearArtworkLayout.Frame(size).Width * GearArtworkLayout.Frame(size).Height) > (group == vertical ? .28 : .43), "Equipment looks disproportionately small: " + item.Name);
                }
                var tile = new StackPanel { Width = 240, Height = 190 };
                tile.Children.Add(new GearArt { Kind = item.Kind, Image = source, Height = 135 });
                tile.Children.Add(new TextBlock { Text = item.Name, Foreground = Brushes.White, Margin = new Thickness(12, 8, 12, 0), TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });
                sheet.Children.Add(tile);
            }
            sheet.Measure(new Size(720, double.PositiveInfinity)); sheet.Arrange(new Rect(sheet.DesiredSize)); sheet.UpdateLayout();
            var bitmap = new RenderTargetBitmap(720, (int)sheet.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(sheet);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var output = File.Create(Path.Combine(folder, group == amps ? "amps.png" : group == cabinets ? "cabinets.png" : group == vertical ? "vertical-2x12.png" : "ir-illustrations.png")); png.Save(output);
        }
        Console.WriteLine("PASS artwork: transparent margins, centering, empty images, collapsed layout, factory amp/cabinet assets, all library families and vertical 2x12 variants, proportional scaling and common visual occupancy at editor/gallery/compact sizes.");
        return Task.CompletedTask;
    }
}
