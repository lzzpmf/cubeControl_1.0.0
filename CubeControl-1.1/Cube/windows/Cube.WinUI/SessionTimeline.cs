using Cube.Desktop.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace Cube.WinUI;

// Native vector overview. The companion slider, clip list and numeric editor provide
// the same operations for keyboard and assistive-technology users.
internal sealed class SessionTimeline : UserControl
{
    private readonly BackingTracksViewModel model;
    private readonly Canvas canvas = new() { Background = new SolidColorBrush(Colors.Transparent) };
    private readonly Line cursor = new() { StrokeThickness = 2 };
    private readonly Grid grid = new();
    private bool subscribed;
    private bool drawPending;
    private readonly Polyline wave = new() { StrokeThickness = 1, IsHitTestVisible = false };
    private object? lastPeaks;
    private double lastWidth = -1, lastStart = -1, lastSpan = -1, lastDuration = -1;
    private double? rangeAnchor;
    public SessionTimeline(BackingTracksViewModel model)
    {
        this.model = model; MinHeight = 160; HorizontalContentAlignment = HorizontalAlignment.Stretch;
        Content = grid; grid.Children.Add(canvas); grid.Children.Add(cursor);
        cursor.IsHitTestVisible = false;
        Loaded += (_, _) => { if (!subscribed) { model.PropertyChanged += Changed; subscribed = true; } RequestDraw(); };
        Unloaded += (_, _) => { if (subscribed) model.PropertyChanged -= Changed; subscribed = false; };
        SizeChanged += (_, _) => RequestDraw(); ActualThemeChanged += (_, _) => RequestDraw();
        canvas.PointerPressed += (_, e) =>
        {
            if (!model.CanSeek) return;
            double time = Time(e.GetCurrentPoint(canvas).Position.X);
            bool shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            if (shift) { rangeAnchor = time; canvas.CapturePointer(e.Pointer); }
            else model.Position = time;
        };
        canvas.PointerReleased += (_, e) =>
        {
            if (rangeAnchor is double start) { double end = Time(e.GetCurrentPoint(canvas).Position.X); if (Math.Abs(end - start) >= .1) model.SelectRange(Math.Min(start, end), Math.Max(start, end)); }
            rangeAnchor = null; canvas.ReleasePointerCapture(e.Pointer);
        };
        canvas.PointerCaptureLost += (_, _) => rangeAnchor = null;
    }
    private double Span => Math.Max(.1, model.StudioViewportDuration);
    private double X(double time) => (time - model.ViewportStart) / Span * Math.Max(1, ActualWidth);
    private double Time(double x) => Math.Clamp(model.ViewportStart + x / Math.Max(1, ActualWidth) * Span, 0, model.Duration);
    private Brush Accent => (Brush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
    private Brush ForegroundBrush => (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
    private void Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Position") MoveCursor();
        else if (e.PropertyName is "Peaks" or "ClipRevision" or "Duration" or "Zoom" or "ViewportStart" or "SelectedStem" or "RangeStart" or "RangeEnd" or "SessionStems") RequestDraw();
    }
    private void MoveCursor() { cursor.Stroke = ForegroundBrush; cursor.X1 = cursor.X2 = Math.Clamp(X(model.Position), 0, Math.Max(0, ActualWidth)); cursor.Y1 = 0; cursor.Y2 = Height; }
    private void RequestDraw()
    {
        // Several properties change together on seek, zoom and clip edits.
        // Keep one pending refresh and render the latest state, never a stale snapshot.
        if (drawPending || !IsLoaded) return;
        drawPending = true;
        if (!DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            drawPending = false;
            if (IsLoaded) Draw();
        })) drawPending = false;
    }
    private void Draw()
    {
        if (ActualWidth < 1) return;
        var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(XamlRoot) as Button;
        var focusedClip = focused != null && canvas.Children.Contains(focused) ? focused.Tag : null;
        canvas.Children.Clear();
        var stems = (model.SessionStems ?? []).ToArray();
        bool empty = model.Peaks.Length == 0 && stems.All(stem => stem.Clips.Count == 0);
        Height = empty ? 160 : Math.Max(160, 100 + stems.Length * 38);
        if (empty)
        {
            var hint = new TextBlock { Text = Cube.Desktop.Localization.LanguageService.T("Adicione áudio à sessão para visualizar a linha do tempo."), TextWrapping = TextWrapping.Wrap, MaxWidth = Math.Max(1, ActualWidth - 24), Foreground = ForegroundBrush };
            Canvas.SetLeft(hint, 12); Canvas.SetTop(hint, 48); canvas.Children.Add(hint);
        }
        if (model.HasRange)
        {
            var range = new Rectangle { Width = Math.Max(0, Math.Clamp(X(model.RangeEnd), 0, ActualWidth) - Math.Clamp(X(model.RangeStart), 0, ActualWidth)), Height = Height, Fill = Accent, Opacity = .12 };
            Canvas.SetLeft(range, Math.Clamp(X(model.RangeStart), 0, ActualWidth)); canvas.Children.Add(range);
        }
        wave.Stroke = Accent;
        var peaks = model.Peaks;
        if (!ReferenceEquals(lastPeaks, peaks) || lastWidth != ActualWidth || lastStart != model.ViewportStart || lastSpan != Span || lastDuration != model.Duration)
        {
            wave.Points.Clear();
            int samples = (int)Math.Min(2000, Math.Ceiling(ActualWidth));
            for (int i = 0; i < samples; i++)
            {
                double x = samples <= 1 ? 0 : i * ActualWidth / (samples - 1);
                int index = (int)(Time(x) / Math.Max(.1, model.Duration) * peaks.Length);
                double peak = peaks.Length == 0 ? 0 : Math.Clamp(Math.Abs(peaks[Math.Clamp(index, 0, peaks.Length - 1)]), 0, 1);
                wave.Points.Add(new Point(x, 44 - peak * 32)); wave.Points.Add(new Point(x, 44 + peak * 32));
            }
            lastPeaks = peaks; lastWidth = ActualWidth; lastStart = model.ViewportStart; lastSpan = Span; lastDuration = model.Duration;
        }
        if (!empty) canvas.Children.Add(wave);
        for (int row = 0; row < stems.Length; row++)
        {
            var stem = stems[row];
            if (empty) break;
            var lane = new Rectangle { Width = ActualWidth, Height = 34, Fill = Accent, Opacity = row % 2 == 0 ? .08 : .03, IsHitTestVisible = false };
            Canvas.SetTop(lane, 90 + row * 38); canvas.Children.Add(lane);
            var label = new TextBlock { Text = stem.Name, Foreground = ForegroundBrush, Opacity = .65, IsHitTestVisible = false };
            Canvas.SetTop(label, 96 + row * 38); Canvas.SetLeft(label, 6); canvas.Children.Add(label);
            foreach (var clip in stem.Clips.Where(c => c.End >= model.ViewportStart && c.Position <= model.ViewportStart + Span))
            {
                double start = Math.Clamp(X(clip.Position), 0, ActualWidth), end = Math.Clamp(X(clip.End), 0, ActualWidth);
                var item = new Button { Content = (stem.IsMuted ? "[M] " : "") + clip.Name, Width = Math.Max(1, end - start), Height = 32, Padding = new Thickness(4, 0, 4, 0), Tag = clip.Id };
                Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(item, $"{stem.Name}, {clip.Name}, {clip.Position:0.00} {Cube.Desktop.Localization.LanguageService.T("segundos")}" + (stem.IsMuted ? ", " + Cube.Desktop.Localization.LanguageService.T("Pista silenciada") : ""));
                item.Click += (_, _) => model.SelectClip(stem, clip, false);
                Canvas.SetLeft(item, start); Canvas.SetTop(item, 90 + row * 38); canvas.Children.Add(item);
                if (Equals(focusedClip, clip.Id)) item.Loaded += (_, _) => item.Focus(FocusState.Keyboard);
            }
        }
        MoveCursor();
    }
}
