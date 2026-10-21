using Cube.Desktop.Appearance;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Cube.WinUI.Appearance;

internal static class InstrumentAppearance
{
    private static readonly Dictionary<string, ImageSource> cache = [];
    public static ControlFinish Finish { get; private set; } = ControlFinish.Official;
    public static double Pivot => ControlFinishCatalog.Get(Finish).KnobPivotY;
    public static event EventHandler? Changed;
    public static void Select(ControlFinish finish) { Finish = finish; Changed?.Invoke(null, EventArgs.Empty); }
    public static ImageSource Face(string kind, ElementTheme theme)
    {
        var definition = ControlFinishCatalog.Get(Finish);
        bool official = Finish == ControlFinish.Official && theme == ElementTheme.Dark && kind != "player-face";
        string file = official ? kind switch { "knob" => "Knobs/knob_oficial_01.png", "fader" => "Faders/fader_oficial_01.png", _ => "Switches/footswitch_oficial_01.png" }
            : $"Finishes/{definition.Folder}/{(kind == "fader" ? "fader-vertical" : kind)}.png";
        if (!cache.TryGetValue(file, out var image))
        {
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Controls", file));
            cache[file] = image = new BitmapImage(new Uri(path));
            _ = CropTransparentMargin(file, path);
        }
        return image;
    }
    // Match the WPF artwork normalization once per asset, not once per rendered frame.
    private static async Task CropTransparentMargin(string key, string path)
    {
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            using var stream = await file.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var pixels = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, new BitmapTransform(), ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
            byte[] bytes = pixels.DetachPixelData();
            int width = (int)decoder.PixelWidth, height = (int)decoder.PixelHeight;
            int left = width, top = height, right = -1, bottom = -1;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (bytes[(y * width + x) * 4 + 3] > 16)
                    { left = Math.Min(left, x); right = Math.Max(right, x); top = Math.Min(top, y); bottom = Math.Max(bottom, y); }
            if (right < left || bottom < top) return;
            var cropped = new WriteableBitmap(right - left + 1, bottom - top + 1);
            using var output = cropped.PixelBuffer.AsStream();
            for (int y = top; y <= bottom; y++) output.Write(bytes, (y * width + left) * 4, cropped.PixelWidth * 4);
            cropped.Invalidate(); cache[key] = cropped;
            Changed?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException)
        { App.Trace("Artwork normalization: " + ex.Message); }
    }
}
