using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cube.Desktop.Controls;

internal static class GrabCursors
{
    public static Cursor OpenHand { get; } = Create(false);
    public static Cursor ClosedHand { get; } = Create(true);

    private static Cursor Create(bool closed)
    {
        const int size = 32;
        var visual = new DrawingVisual();
        using (var drawing = visual.RenderOpen())
        {
            var outline = Geometry.Parse(closed
                ? "M 9,25 L 6,20 L 5,16 Q 5,13 8,14 L 10,16 L 10,11 Q 10,8 13,9 L 14,10 Q 15,7 18,9 Q 21,7 23,10 Q 27,9 27,13 L 27,20 Q 27,23 24,26 L 24,29 L 12,29 L 12,27 Z"
                : "M 11,28 L 7,22 L 3,17 Q 2,14 5,14 L 10,18 L 7,8 Q 7,5 10,6 L 14,15 L 13,4 Q 14,1 16,4 L 18,14 L 19,4 Q 21,2 22,5 L 22,15 L 25,8 Q 27,6 28,9 L 26,21 Q 26,25 23,27 L 23,30 L 12,30 Z");
            drawing.DrawGeometry(Brushes.White, new Pen(Brushes.Black, 1.5) { LineJoin = PenLineJoin.Round }, outline);
            var creases = Geometry.Parse(closed
                ? "M 14,11 L 14,16 M 18,10 L 18,16 M 22,11 L 22,16 M 10,18 Q 14,17 16,21"
                : "M 13,20 L 15,24 M 18,19 L 18,24 M 22,19 L 21,24");
            drawing.DrawGeometry(null, new Pen(Brushes.Black, 1), creases);
        }
        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var pixels = new byte[size * size * 4];
        new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0).CopyPixels(pixels, size * 4, 0);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        // CUR directory followed by a bottom-up 32-bit DIB and transparency mask.
        writer.Write((ushort)0); writer.Write((ushort)2); writer.Write((ushort)1);
        writer.Write((byte)size); writer.Write((byte)size); writer.Write((ushort)0);
        writer.Write((ushort)16); writer.Write((ushort)16);
        writer.Write(40 + pixels.Length + size * 4); writer.Write(22);
        writer.Write(40); writer.Write(size); writer.Write(size * 2);
        writer.Write((ushort)1); writer.Write((ushort)32);
        writer.Write(0); writer.Write(pixels.Length);
        writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
        for (int y = size - 1; y >= 0; y--) writer.Write(pixels, y * size * 4, size * 4);
        for (int y = size - 1; y >= 0; y--)
            for (int group = 0; group < 4; group++)
            {
                byte mask = 0;
                for (int bit = 0; bit < 8; bit++)
                    if (pixels[(y * size + group * 8 + bit) * 4 + 3] == 0) mask |= (byte)(128 >> bit);
                writer.Write(mask);
            }
        writer.Flush();
        stream.Position = 0;
        return new Cursor(stream, true);
    }
}
