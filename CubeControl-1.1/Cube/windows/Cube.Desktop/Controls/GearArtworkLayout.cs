using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cube.Desktop.Controls;

internal static class GearArtworkLayout
{
    private sealed record Bounds(Rect Value);
    private static readonly ConditionalWeakTable<BitmapSource, Bounds> cache = new();

    // Ignore only nearly transparent fringe when measuring the equipment, not when drawing it.
    internal static Rect VisibleBounds(ImageSource source)
    {
        if (source is not BitmapSource bitmap) return new Rect(0, 0, source.Width, source.Height);
        return bitmap.IsFrozen ? cache.GetValue(bitmap, b => new Bounds(Measure(b))).Value : Measure(bitmap);
    }

    private static Rect Measure(BitmapSource source)
    {
        BitmapSource pixels = source.Format == PixelFormats.Bgra32 ? source : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int width = pixels.PixelWidth, height = pixels.PixelHeight, stride = checked(width * 4);
        byte[] row = new byte[stride];
        int left = width, right = -1, top = height, bottom = -1;
        for (int y = 0; y < height; y++)
        {
            pixels.CopyPixels(new Int32Rect(0, y, width, 1), row, stride, 0);
            for (int x = 0; x < width; x++)
                if (row[x * 4 + 3] >= 16)
                {
                    left = Math.Min(left, x); right = Math.Max(right, x);
                    top = Math.Min(top, y); bottom = y;
                }
        }
        if (right < left) return new Rect(0, 0, source.Width, source.Height);
        double dx = source.Width / width, dy = source.Height / height;
        return new Rect(left * dx, top * dy, (right - left + 1) * dx, (bottom - top + 1) * dy);
    }

    internal static Rect Frame(Size size)
    {
        double height = Math.Max(0, size.Height - 24);
        double width = Math.Min(Math.Max(0, size.Width - 24), height * 1.62);
        return new Rect((size.Width - width) / 2, (size.Height - height) / 2, width, height);
    }

    internal static Rect Place(Size source, Rect visible, Size viewport, double artworkScale)
    {
        var frame = Frame(viewport);
        if (frame.Width <= 0 || frame.Height <= 0 || visible.Width <= 0 || visible.Height <= 0) return Rect.Empty;
        // Comparable visual weight for heads, combos and cabinets, bounded without stretching/cropping.
        double scale = Math.Min(Math.Sqrt(frame.Width * frame.Height * .48 / (visible.Width * visible.Height)),
            Math.Min(frame.Width / visible.Width, frame.Height / visible.Height)) * artworkScale;
        return new Rect(viewport.Width / 2 - (visible.X + visible.Width / 2) * scale,
            viewport.Height / 2 - (visible.Y + visible.Height / 2) * scale, source.Width * scale, source.Height * scale);
    }
}
