using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cube.Desktop.Appearance;

/// <summary>Shared, cached faces for musical controls. Geometry and input stay in their controls.</summary>
internal static class ControlArtwork
{
    internal static ImageSource KnobFace => Application.Current.TryFindResource("CubeControlKnobFace") as ImageSource ?? Load("Knobs/knob_oficial_01.png");
    internal static ImageSource FaderFace => Application.Current.TryFindResource("CubeControlFaderFace") as ImageSource ?? Load("Faders/fader_oficial_01.png");
    private static readonly Dictionary<string, ImageSource> Cache = new();
    private static ImageSource Load(string path)
    {
        if (Cache.TryGetValue(path, out var cached)) return cached;
        var bitmap = new BitmapImage(new Uri($"pack://application:,,,/CubeControl;component/Assets/Images/Controls/{path}"));
        var pixels = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
        int stride = pixels.PixelWidth * 4;
        var data = new byte[stride * pixels.PixelHeight];
        pixels.CopyPixels(data, stride, 0);
        int left = pixels.PixelWidth, top = pixels.PixelHeight, right = -1, bottom = -1;
        for (int y = 0; y < pixels.PixelHeight; y++)
            for (int x = 0; x < pixels.PixelWidth; x++)
                if (data[y * stride + x * 4 + 3] > 16)
                {
                    left = Math.Min(left, x); top = Math.Min(top, y);
                    right = Math.Max(right, x); bottom = Math.Max(bottom, y);
                }
        if (right < left || bottom < top) throw new InvalidOperationException($"Empty control artwork: {path}");
        var face = new CroppedBitmap(bitmap, new Int32Rect(left, top, right - left + 1, bottom - top + 1));
        face.Freeze();
        Cache[path] = face;
        return face;
    }

    private static ImageBrush FaceBrush(ImageSource face)
    {
        var brush = new ImageBrush(face) { Stretch = Stretch.Uniform };
        brush.Freeze();
        return brush;
    }

    internal static void Apply(ResourceDictionary resources, ControlFinish finish, bool light, bool highContrast)
    {
        bool original = finish == ControlFinish.Official && !light;
        var definition = ControlFinishCatalog.Get(finish);
        string folder = definition.Folder;
        ImageSource Face(string name, string official) => Load(original ? official : $"Finishes/{folder}/{name}.png");
        resources["CubeControlKnobFace"] = Face("knob", "Knobs/knob_oficial_01.png");
        resources["CubeControlExternalKnobMarker"] = definition.ExternalKnobMarker;
        resources["CubeControlKnobPivotY"] = definition.KnobPivotY;
        resources["CubeControlFaderFace"] = Face("fader-vertical", "Faders/fader_oficial_01.png");
        resources["CubeControlHorizontalFader"] = highContrast ? AccessibleFace("fader") : FaceBrush(Face("fader-horizontal", "Faders/fader_horizontal_oficial_01.png"));
        resources["OfficialSwitchOn"] = highContrast ? AccessibleFace("on") : FaceBrush(Face("switch-on", "Switches/chave_seletora_oficial_01_on.png"));
        resources["OfficialSwitchOff"] = highContrast ? AccessibleFace("off") : FaceBrush(Face("switch-off", "Switches/chave_seletora_oficial_01_off.png"));
        resources["CubeControlFootswitch"] = highContrast ? AccessibleFace("foot") : FaceBrush(Face("footswitch", "Switches/footswitch_oficial_01.png"));
        resources["CubePlayerUsesOfficialArtwork"] = original;
        resources["CubePlayerButtonFace"] = Load($"Finishes/{folder}/player-face.png");
        resources["CubePlayerRecordFace"] = Face("record", "BackingTrack/record.png");
        resources["CubeRecordingIndicator"] = highContrast ? SystemColors.WindowTextBrush : Brushes.Red;
        string playerGlyph = definition.Glyph;
        resources["CubePlayerGlyph"] = highContrast ? SystemColors.WindowTextBrush : new SolidColorBrush((Color)ColorConverter.ConvertFromString(playerGlyph));
        if (!highContrast)
        {
            string track = light ? definition.LightTrack : definition.DarkTrack;
            resources["CubeKnobTrack"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(track));
        }
    }

    private static DrawingBrush AccessibleFace(string kind)
    {
        var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            var border = new Pen(SystemColors.WindowTextBrush, 2);
            if (kind == "foot")
            {
                dc.DrawEllipse(SystemColors.WindowBrush, border, new Point(28, 28), 26, 26);
                dc.DrawEllipse(SystemColors.WindowBrush, border, new Point(28, 28), 18, 18);
            }
            else if (kind == "fader")
            {
                dc.DrawRoundedRectangle(SystemColors.WindowBrush, border, new Rect(1, 1, 18, 30), 2, 2);
                dc.DrawLine(border, new Point(5, 16), new Point(15, 16));
            }
            else
            {
                bool on = kind == "on";
                dc.DrawRoundedRectangle(SystemColors.WindowBrush, border, new Rect(1, 1, 54, 24), 12, 12);
                dc.DrawEllipse(on ? SystemColors.HighlightBrush : SystemColors.WindowTextBrush, null, new Point(on ? 43 : 13, 13), 9, 9);
                dc.DrawLine(new Pen(SystemColors.WindowTextBrush, 2), new Point(on ? 10 : 36, 13), new Point(on ? 20 : 46, 13));
            }
        }
        return new DrawingBrush(group) { Stretch = Stretch.Uniform };
    }
}
