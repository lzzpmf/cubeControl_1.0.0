using System.Windows;
using System.Windows.Media;
using Cube.Desktop.Appearance;

namespace Cube.Desktop.Studio.Controls;

/// <summary>A recording glyph on the same selectable control finish used by Cube's player.</summary>
public sealed class RecordButtonFace : FrameworkElement
{
    public static readonly DependencyProperty IsRecordingProperty = DependencyProperty.Register(
        nameof(IsRecording), typeof(bool), typeof(RecordButtonFace), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public bool IsRecording { get => (bool)GetValue(IsRecordingProperty); set => SetValue(IsRecordingProperty, value); }
    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(RecordButtonFace), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public RecordButtonFace()
    {
        IsHitTestVisible = false;
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
    }
    protected override void OnRender(DrawingContext dc)
    {
        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) return;
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var glyph = TryFindResource("CubePlayerGlyph") as Brush ?? SystemColors.WindowTextBrush;
        if (!ThemeService.Instance.IsHighContrast && TryFindResource("CubePlayerButtonFace") is ImageSource face)
        {
            var scale = size / Math.Max(face.Width, face.Height);
            dc.DrawImage(face, new Rect(center.X - face.Width * scale / 2, center.Y - face.Height * scale / 2, face.Width * scale, face.Height * scale));
        }
        else dc.DrawEllipse(SystemColors.WindowBrush, new Pen(glyph, 1), center, size * .44, size * .44);
        var recordingBrush = ThemeService.Instance.IsHighContrast ? glyph : IsRecording ? Brushes.Red : Brushes.IndianRed;
        dc.DrawEllipse(recordingBrush, null, center, size * .14, size * .14);
    }
}
