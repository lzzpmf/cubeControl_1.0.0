using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using Cube.Studio.Core;
using Cube.Desktop.Localization;
using Cube.Desktop.Appearance;

namespace Cube.Desktop.Studio.Controls;

/// <summary>
/// Native timeline surface backed by decoded peak caches; painting never reads audio files.
/// Edits are previewed locally and emitted once on mouse-up; the editor owns undo/history.
/// </summary>
public sealed class TimelineControl : FrameworkElement
{
    public const double RulerHeight = 26;
    private double EffectiveRulerHeight => RulerHeight;
    public static readonly DependencyProperty VisibleTrackIndexProperty = DependencyProperty.Register(
        nameof(VisibleTrackIndex), typeof(int), typeof(TimelineControl), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender));
    public int VisibleTrackIndex { get => (int)GetValue(VisibleTrackIndexProperty); set => SetValue(VisibleTrackIndexProperty, value); }
    public void MatchViewport(TimelineControl other)
    {
        _zoom = other._zoom; _viewStart = other._viewStart;
        InvalidateContent();
    }
    private readonly DrawingVisual _cursorVisual = new();
    private readonly TranslateTransform _cursorTransform = new();
    private DrawingGroup? _contentDrawing;
    private readonly Dictionary<string, Brush> _drawingBrushes = new();
    private bool _contentDirty = true, _drawingContent;
    private double _drawingDuration, _drawingScale;
    private int _contentBuildCount;
    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => index == 0 ? _cursorVisual : throw new ArgumentOutOfRangeException(nameof(index));
    private void InvalidateContent()
    {
        _contentDirty = true;
        base.InvalidateVisual();
    }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ThemeRevisionProperty || e.Property == VisibleTrackIndexProperty || e.Property == PracticeModeProperty || e.Property == PeakProviderProperty)
            InvalidateContent();
    }
    private void UpdateCursor()
    {
        double x = Project == null ? -100 : TimeToX(Project.Position);
        _cursorVisual.Opacity = IsVisible && x >= 0 && x <= ActualWidth ? 1 : 0;
        _cursorTransform.X = x;
    }
    private void RebuildCursor()
    {
        using var dc = _cursorVisual.RenderOpen();
        if (Project == null || ActualHeight < 1) return;
        var accent = ResourceBrush(VisibleTrackIndex >= 0 ? "BackingPlayhead" : "CubeAccent", "#86B8FF");
        double top = VisibleTrackIndex >= 0 ? EffectiveRulerHeight - 10 : 0;
        dc.DrawLine(new Pen(accent, 1.5), new Point(0, top), new Point(0, ActualHeight));
        var marker = new StreamGeometry();
        using (var path = marker.Open())
        {
            path.BeginFigure(new Point(-7, top), true, true);
            path.LineTo(new Point(7, top), true, false);
            path.LineTo(new Point(0, top + 9), true, false);
        }
        marker.Freeze(); dc.DrawGeometry(accent, null, marker);
    }
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo); InvalidateContent();
    }
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi); InvalidateContent();
    }
    private const double MinimumClipDuration = .15;
    private readonly List<INotifyPropertyChanged> _propertySubscriptions = [];
    private readonly List<INotifyCollectionChanged> _collectionSubscriptions = [];
    private double _zoom = 1;
    private double _viewStart;
    private Gesture? _gesture;
    private Guid? _dropTrack;

    public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(
        nameof(Project), typeof(ProjectModel), typeof(TimelineControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, ProjectChanged));
    public static readonly DependencyProperty CanEditProperty = DependencyProperty.Register(
        nameof(CanEdit), typeof(bool), typeof(TimelineControl), new PropertyMetadata(true, CanEditChanged));
    public static readonly DependencyProperty CanSeekProperty = DependencyProperty.Register(
        nameof(CanSeek), typeof(bool), typeof(TimelineControl), new PropertyMetadata(true));
    public static readonly DependencyProperty SnapEnabledProperty = DependencyProperty.Register(
        nameof(SnapEnabled), typeof(bool), typeof(TimelineControl), new PropertyMetadata(true));
    public static readonly DependencyProperty PracticeModeProperty = DependencyProperty.Register(
        nameof(PracticeMode), typeof(bool), typeof(TimelineControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(TimelineControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty PeakProviderProperty = DependencyProperty.Register(nameof(PeakProvider), typeof(IStudioPeakProvider), typeof(TimelineControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, PeaksProviderChanged));
    public IStudioPeakProvider? PeakProvider { get => (IStudioPeakProvider?)GetValue(PeakProviderProperty); set => SetValue(PeakProviderProperty, value); }

    public ProjectModel? Project { get => (ProjectModel?)GetValue(ProjectProperty); set => SetValue(ProjectProperty, value); }
    public bool CanEdit { get => (bool)GetValue(CanEditProperty); set => SetValue(CanEditProperty, value); }
    public bool CanSeek { get => (bool)GetValue(CanSeekProperty); set => SetValue(CanSeekProperty, value); }
    public bool SnapEnabled { get => (bool)GetValue(SnapEnabledProperty); set => SetValue(SnapEnabledProperty, value); }
    public bool PracticeMode { get => (bool)GetValue(PracticeModeProperty); set => SetValue(PracticeModeProperty, value); }
    public double Zoom => _zoom;
    public double ViewStart => _viewStart;

    public event EventHandler<TimelineSeekEventArgs>? SeekRequested;
    public event EventHandler<TimelineSelectionEventArgs>? SelectionRequested;
    public event EventHandler<TimelineEditEventArgs>? EditRequested;
    public event EventHandler<TimelineTrackHeightEventArgs>? TrackHeightRequested;
    public event EventHandler<TimelineFilesDroppedEventArgs>? FilesDropped;
    public event EventHandler? ViewChanged;

    public TimelineControl()
    {
        AddVisualChild(_cursorVisual);
        _cursorVisual.Transform = _cursorTransform;
        IsVisibleChanged += (_, _) => { if (IsVisible) InvalidateContent(); };
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
        Focusable = true;
        ClipToBounds = true;
        AllowDrop = true;
        ToolTip = LanguageService.T("Clique: posicionar cursor · S: dividir · Ctrl+scroll: altura da track · Shift+scroll: zoom");
        Loaded += (_, _) => { RewireSubscriptions(); if (PeakProvider is { } peaks) { peaks.PeaksChanged -= PeaksChanged; peaks.PeaksChanged += PeaksChanged; } LanguageService.Instance.PropertyChanged += LanguageChanged; };
        Unloaded += (_, _) => { _contentDrawing = null; CancelGesture(); DetachSubscriptions(); if (PeakProvider is { } peaks) peaks.PeaksChanged -= PeaksChanged; LanguageService.Instance.PropertyChanged -= LanguageChanged; };
    }

    /// <summary>Use this same calculation for the adjacent native track headers.</summary>
    public static double[] CalculateTrackHeights(ProjectModel? project, double availableHeight)
    {
        var count = project?.Tracks.Count ?? 0;
        if (count == 0) return [];
        var height = Math.Max(0, double.IsFinite(availableHeight) ? availableHeight : 0);
        var minimum = Math.Min(72, height / count);
        var remaining = Math.Max(0, height - minimum * count);
        var total = project!.Tracks.Sum(track => Math.Max(.05, track.HeightWeight));
        return project.Tracks.Select(track => minimum + remaining * Math.Max(.05, track.HeightWeight) / total).ToArray();
    }

    public void FitProject() => ResetView();
    protected override AutomationPeer OnCreateAutomationPeer() => new TimelineAutomationPeer(this);
    public void ResetView()
    {
        _zoom = 1;
        _viewStart = 0;
        ViewChanged?.Invoke(this, EventArgs.Empty);
        InvalidateContent();
    }

    protected override Size MeasureOverride(Size availableSize) => new(
        double.IsFinite(availableSize.Width) ? availableSize.Width : 600,
        double.IsFinite(availableSize.Height) ? availableSize.Height : 350);

    private double TotalDuration => _drawingContent ? _drawingDuration : CalculateDuration();
    private double CalculateDuration() => Math.Max(1, Math.Max(Project?.Duration ?? 80,
        Project?.Tracks.SelectMany(track => track.Clips).Select(clip => clip.Start + clip.Duration).DefaultIfEmpty(0).Max() ?? 0));
    private double VisibleDuration => TotalDuration / Math.Max(1, _zoom);
    private double PixelsPerSecond => _drawingContent ? _drawingScale : ActualWidth / Math.Max(.001, VisibleDuration);
    private double TimeToX(double time) => (time - _viewStart) * PixelsPerSecond;
    private double XToTime(double x) => Math.Clamp(_viewStart + x / Math.Max(.001, PixelsPerSecond), 0, TotalDuration);

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (!IsVisible) return;
        if (_contentDirty || _contentDrawing == null)
        {
            _drawingBrushes.Clear();
            _drawingDuration = CalculateDuration();
            _drawingScale = ActualWidth / Math.Max(.001, _drawingDuration / Math.Max(1, _zoom));
            var drawing = new DrawingGroup();
            _drawingContent = true;
            try { using var content = drawing.Open(); DrawContent(content); }
            finally { _drawingContent = false; }
            if (drawing.CanFreeze) drawing.Freeze();
            _contentDrawing = drawing; _contentDirty = false; _contentBuildCount++;
            RebuildCursor();
        }
        dc.DrawDrawing(_contentDrawing);
        UpdateCursor();
    }
    private void DrawContent(DrawingContext dc)
    {
        var background = ResourceBrush("CubeCanvas", "#191C22");
        var surface = ResourceBrush("CubeCard", "#232730");
        var line = ResourceBrush("CubeBorder", "#3B4351");
        dc.DrawRectangle(background, null, new Rect(RenderSize));
        if (ActualWidth < 1 || ActualHeight < 1 || Project is null) return;
        var bounds = TrackBounds();
        DrawLoopShade(dc);
        DrawGrid(dc);
        var anySolo = Project.Tracks.Any(track => track.IsSolo);
        for (var index = 0; index < bounds.Count; index++)
        {
            var (track, lane) = bounds[index];
            dc.PushClip(new RectangleGeometry(lane));
            if (_dropTrack == track.Id)
                dc.DrawRectangle(OpacityBrush(ResourceBrush("CubeAccent", "#86B8FF"), .1), null, lane);
            dc.DrawLine(new Pen(line, 1), new Point(0, lane.Top + .5), new Point(ActualWidth, lane.Top + .5));
            var clips = DisplayClips(track).ToList();
            var muted = track.IsMuted || (anySolo && !track.IsSolo);
            if (muted) dc.PushOpacity(.45);
            foreach (var clip in clips) DrawClip(dc, clip, lane, Project.Tracks.IndexOf(track));
            DrawCrossfades(dc, clips, lane);
            if (muted) dc.Pop();
            if (clips.Count == 0)
                DrawText(dc, LanguageService.T(PracticeMode ? Project.Tracks.IndexOf(track) == 0 ? "Importe um áudio ou escolha uma backing" : "Grave sua guitarra" : "Arraste um áudio"), new Point(14, lane.Top + lane.Height / 2 - 7), ResourceBrush("Muted", "#B5BFCE"), ActualWidth - 28);
            dc.Pop();
        }
        dc.DrawRectangle(surface, null, new Rect(0, 0, ActualWidth, Math.Min(EffectiveRulerHeight, ActualHeight)));
        DrawRuler(dc);
        dc.DrawLine(new Pen(line, 1), new Point(.5, 0), new Point(.5, ActualHeight));
    }

    private void DrawGrid(DrawingContext dc)
    {
        if (Project is null) return;
        var meter = Project.Meter.Split('/');
        var beats = meter.Length > 0 && int.TryParse(meter[0], out var numerator) ? numerator : 4;
        var denominator = meter.Length > 1 && int.TryParse(meter[1], out var parsed) ? parsed : 4;
        var seconds = PracticeMode ? 1 : 60 / Math.Max(30, Project.Bpm) * beats * 4 / Math.Max(1, denominator);
        while (seconds * PixelsPerSecond < 24) seconds *= 2;
        var first = Math.Floor(_viewStart / seconds) * seconds;
        var pen = new Pen(OpacityBrush(ResourceBrush("CubeBorder", "#3B4351"), .5), .5);
        for (var time = first; time <= _viewStart + VisibleDuration + seconds; time += seconds)
        {
            var x = TimeToX(time);
            if (x >= 0 && x <= ActualWidth) dc.DrawLine(pen, new Point(x, EffectiveRulerHeight), new Point(x, ActualHeight));
        }
    }

    private void DrawLoopShade(DrawingContext dc)
    {
        if (Project is null || (!Project.LoopEnabled && _gesture?.Kind != DragKind.LoopSelection)) return;
        var (start, end) = LoopBounds();
        var left = Math.Clamp(TimeToX(start), 0, ActualWidth);
        var right = Math.Clamp(TimeToX(end), 0, ActualWidth);
        if (right > left && ActualHeight > EffectiveRulerHeight)
            dc.DrawRectangle(OpacityBrush(ResourceBrush("CubeAccent", "#86B8FF"), .055), null,
                new Rect(left, EffectiveRulerHeight, right - left, ActualHeight - EffectiveRulerHeight));
    }

    private void DrawRuler(DrawingContext dc)
    {
        var secondary = ResourceBrush("Muted", "#B5BFCE");
        double[] intervals = [.25, .5, 1, 2, 5, 10, 15, 20, 30, 60, 120, 300, 600, 1800];
        var desired = VisibleDuration * 80 / Math.Max(80, ActualWidth);
        var step = intervals.FirstOrDefault(value => value >= desired, 3600);
        var first = Math.Ceiling(_viewStart / step) * step;
        for (var time = first; time <= _viewStart + VisibleDuration; time += step)
        {
            var x = TimeToX(time);
            var label = VisibleTrackIndex >= 0 && step >= 1 ? $"{(int)(time / 60)}:{(int)time % 60:00}" : FormatTime(time, step);
            var width = TextWidth(label);
            DrawText(dc, label, new Point(Math.Clamp(x + 4, 2, Math.Max(2, ActualWidth - width - 2)), VisibleTrackIndex >= 0 ? 4 : 1), secondary, width + 1);
        }
        if (Project is null || (Project.LoopEnd <= Project.LoopStart && _gesture?.Kind != DragKind.LoopSelection)) return;
        var (start, end) = LoopBounds();
        var left = Math.Clamp(TimeToX(start), 0, ActualWidth);
        var right = Math.Clamp(TimeToX(end), 0, ActualWidth);
        if (right <= left) return;
        var accent = ResourceBrush("CubeAccent", "#86B8FF");
        var bar = new Rect(left, EffectiveRulerHeight - 10, right - left, 10);
        dc.DrawRoundedRectangle(ResourceBrush("CubeSelected", "#243D5F"), null, bar, 2, 2);
        dc.DrawLine(new Pen(accent, 2), new Point(left + 1, bar.Top), new Point(left + 1, bar.Bottom));
        dc.DrawLine(new Pen(accent, 2), new Point(right - 1, bar.Top), new Point(right - 1, bar.Bottom));
        if (right - left > 36)
        {
            DrawText(dc, "A", new Point(left + 4, bar.Top - 2), ResourceBrush("CubeSelectedText", "#FFFFFF"), 12);
            DrawText(dc, "B", new Point(right - 12, bar.Top - 2), ResourceBrush("CubeSelectedText", "#FFFFFF"), 12);
        }
    }

    private void DrawPracticeWaveform(DrawingContext dc, DisplayClip clip, Rect lane, int index)
    {
        var rect = ClipRect(clip, lane);
        if (rect.Right < 0 || rect.Left > ActualWidth) return;
        var brush = ThemeService.Instance.IsHighContrast ? SystemColors.WindowTextBrush
            : ResourceBrush(index == 0 ? "BackingPlayed" : "BackingFuture", "#82ACD2");
        dc.DrawRectangle(null, new Pen(ResourceBrush("CubeBorder", "#3B4351"), .5), new Rect(0, lane.Top, ActualWidth, lane.Height));
        var centre = lane.Top + lane.Height / 2;
        var amplitude = Math.Max(2, lane.Height / 2 - 5);
        var sourceStart = clip.Model.SourceTimeAt(clip.Model.Start);
        var ratio = clip.Model.SourceTimeAt(clip.Model.Start + 1) - sourceStart;
        var geometry = new StreamGeometry();
        using (var path = geometry.Open())
        {
            for (double x = Math.Max(0, rect.Left); x < Math.Min(rect.Right, ActualWidth); x += 1.5)
            {
                var time = sourceStart + (clip.SourceOffset - clip.Model.SourceOffset + (x - rect.Left) / Math.Max(.001, PixelsPerSecond)) * ratio;
                var peak = PeakProvider?.GetPeak(clip.Model, time, 1.5 / Math.Max(.001, PixelsPerSecond) * ratio) ?? 0;
                var height = Math.Max(.5, (float.IsFinite(peak) ? Math.Clamp(peak, 0, 1) : 0) * amplitude);
                path.BeginFigure(new Point(x, centre - height), false, false);
                path.LineTo(new Point(x, centre + height), true, false);
            }
        }
        geometry.Freeze();
        dc.DrawGeometry(null, new Pen(brush, 1.3), geometry);
    }

    private void DrawClip(DrawingContext dc, DisplayClip clip, Rect lane, int trackIndex)
    {
        if (lane.Height < 20) return;
        if (PracticeMode && VisibleTrackIndex >= 0)
        {
            DrawPracticeWaveform(dc, clip, lane, trackIndex);
            return;
        }
        var rect = ClipRect(clip, lane);
        if (rect.Right < 0 || rect.Left > ActualWidth) return;
        var guitar = trackIndex is 1 or 2;
        var stroke = PracticeMode
            ? ResourceBrush(guitar ? "BackingFuture" : "BackingPlayed", guitar ? "#82ACD2" : "#80B8A8")
            : ResourceBrush(guitar ? "BackingPlayed" : "CubeAccent", guitar ? "#8AD4B1" : "#86B8FF");
        var fill = ResourceBrush(guitar ? "CubeInset" : "CubeSelected", guitar ? "#254237" : "#243D5F");
        var selected = Project?.SelectedClipId == clip.Model.Id;
        if (ThemeService.Instance.IsHighContrast)
        {
            fill = selected ? SystemColors.HighlightBrush : SystemColors.WindowBrush;
            stroke = selected ? SystemColors.HighlightTextBrush : SystemColors.WindowTextBrush;
        }
        dc.DrawRoundedRectangle(fill, new Pen(selected ? ResourceBrush("CubeAccent", "#86B8FF") : stroke, selected ? 1.8 : .8), rect, 4, 4);
        dc.PushClip(new RectangleGeometry(rect));
        var take = clip.Model.Takes.Count > 1 ? $" · T{clip.Model.ActiveTakeIndex + 1}/{clip.Model.Takes.Count}" : "";
        var label = clip.Model.Name + take;
        if (PeakProvider is { } media && !media.HasMedia(clip.Model)) label = "⚠ " + LanguageService.T("Mídia ausente") + " · " + label;
        DrawText(dc, label, new Point(rect.Left + 7, rect.Top + 3), stroke, Math.Max(0, rect.Width - 14));
        if (rect.Height >= 32)
        {
            var middle = rect.Top + 18 + (rect.Height - 22) / 2;
            var amplitude = Math.Max(2, (rect.Height - 26) / 2);
            var geometry = new StreamGeometry();
            var sourceAtClipStart = clip.Model.SourceTimeAt(clip.Model.Start);
            var sourceSecondsPerTimelineSecond = clip.Model.SourceTimeAt(clip.Model.Start + 1) - sourceAtClipStart;
            using (var context = geometry.Open())
            {
                for (var x = Math.Max(rect.Left + 3, 2); x < Math.Min(rect.Right - 3, ActualWidth); x += 3)
                {
                    var sourceTime = sourceAtClipStart + (clip.SourceOffset - clip.Model.SourceOffset + (x - rect.Left) / Math.Max(.001, PixelsPerSecond)) * sourceSecondsPerTimelineSecond;
                    var measured = PeakProvider?.GetPeak(clip.Model, sourceTime, 3 / Math.Max(.001, PixelsPerSecond) * sourceSecondsPerTimelineSecond) ?? 0;
                    var peak = float.IsFinite(measured) ? Math.Clamp(measured, 0, 1) : 0;
                    context.BeginFigure(new Point(x, middle - peak * amplitude), false, false);
                    context.LineTo(new Point(x, middle + peak * amplitude), true, false);
                }
            }
            geometry.Freeze();
            dc.DrawGeometry(null, new Pen(stroke, 1), geometry);
        }
        if (rect.Width > 12 && (!PracticeMode || trackIndex == 1))
        {
            var handlePen = new Pen(stroke, 2);
            dc.DrawLine(handlePen, new Point(rect.Left + 3, rect.Bottom - 9), new Point(rect.Left + 3, rect.Bottom - 3));
            dc.DrawLine(handlePen, new Point(rect.Right - 3, rect.Bottom - 9), new Point(rect.Right - 3, rect.Bottom - 3));
        }
        dc.Pop();
    }

    private void DrawCrossfades(DrawingContext dc, List<DisplayClip> clips, Rect lane)
    {
        var ordered = clips.OrderBy(clip => clip.Start).ToList();
        var accent = ResourceBrush("CubeAccent", "#86B8FF");
        for (var i = 0; i < ordered.Count; i++)
        for (var j = i + 1; j < ordered.Count; j++)
        {
            var start = Math.Max(ordered[i].Start, ordered[j].Start);
            var end = Math.Min(ordered[i].Start + ordered[i].Duration, ordered[j].Start + ordered[j].Duration);
            if (end <= start) continue;
            var left = Math.Clamp(TimeToX(start), 0, ActualWidth);
            var right = Math.Clamp(TimeToX(end), 0, ActualWidth);
            if (right - left < 2 || lane.Height < 24) continue;
            var top = lane.Top + 7;
            var bottom = lane.Bottom - 13;
            dc.DrawRectangle(OpacityBrush(accent, .11), null, new Rect(left, top, right - left, Math.Max(0, bottom - top)));
            dc.DrawLine(new Pen(accent, 1.2), new Point(left, top), new Point(right, bottom));
            dc.DrawLine(new Pen(accent, 1.2), new Point(left, bottom), new Point(right, top));
            if (right - left > 72)
                DrawText(dc, "Crossfade", new Point(left + 4, lane.Bottom - 13), ResourceBrush("Muted", "#B5BFCE"), right - left - 8);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (Project is null) return;
        Focus();
        var point = e.GetPosition(this);
        if (point.Y < EffectiveRulerHeight)
        {
            if (PracticeMode && CanEdit && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                _gesture = new Gesture(DragKind.LoopSelection, point, Guid.Empty, null, XToTime(point.X), MinimumClipDuration, 0);
                CaptureMouse();
                e.Handled = true;
                return;
            }
            if (CanEdit && Project.LoopEnd > Project.LoopStart && point.Y >= EffectiveRulerHeight - 13)
            {
                var startDistance = Math.Abs(point.X - TimeToX(Project.LoopStart));
                var endDistance = Math.Abs(point.X - TimeToX(Project.LoopEnd));
                if (Math.Min(startDistance, endDistance) <= 7)
                {
                    _gesture = new Gesture(startDistance < endDistance ? DragKind.LoopStart : DragKind.LoopEnd, point,
                        Guid.Empty, null, Project.LoopStart, Project.LoopEnd - Project.LoopStart, 0);
                    CaptureMouse();
                    e.Handled = true;
                    return;
                }
            }
            if (CanSeek) SeekRequested?.Invoke(this, new TimelineSeekEventArgs(XToTime(point.X)));
            e.Handled = true;
            return;
        }
        var hit = HitClip(point);
        var track = TrackAt(point.Y);
        if (track is null) return;
        SelectionRequested?.Invoke(this, new TimelineSelectionEventArgs(track.Id, hit?.Clip.Id));
        if (hit is null)
        {
            if (CanSeek) SeekRequested?.Invoke(this, new TimelineSeekEventArgs(XToTime(point.X)));
        }
        else if (e.ClickCount == 2)
        {
            if (CanSeek) SeekRequested?.Invoke(this, new TimelineSeekEventArgs(hit.Clip.Start));
        }
        else if (CanEdit)
        {
            var kind = point.X - hit.Rect.Left < 6 ? DragKind.TrimLeft : hit.Rect.Right - point.X < 6 ? DragKind.TrimRight : DragKind.Move;
            if (PracticeMode && (Project.Tracks.IndexOf(track) != 1 || kind == DragKind.Move))
            {
                if (CanSeek) SeekRequested?.Invoke(this, new TimelineSeekEventArgs(XToTime(point.X)));
                e.Handled = true;
                return;
            }
            _gesture = new Gesture(kind, point, track.Id, hit.Clip, hit.Clip.Start, hit.Clip.Duration, hit.Clip.SourceOffset);
            CaptureMouse();
        }
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var point = e.GetPosition(this);
        if (_gesture is not { } gesture)
        {
            UpdateHover(point);
            return;
        }
        if (!CanEdit) { CancelGesture(); return; }
        if (!gesture.HasMoved && Math.Abs(point.X - gesture.Origin.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(point.Y - gesture.Origin.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        gesture.HasMoved = true;
        var delta = (point.X - gesture.Origin.X) / Math.Max(.001, PixelsPerSecond);
        switch (gesture.Kind)
        {
            case DragKind.Move:
                gesture.Start = Math.Clamp(Snap(gesture.OriginalStart + delta), 0, Math.Max(0, TotalDuration - gesture.OriginalDuration));
                gesture.TargetTrackId = TrackAt(Math.Clamp(point.Y, EffectiveRulerHeight + 1, Math.Max(EffectiveRulerHeight + 1, ActualHeight - 1)))?.Id ?? gesture.TrackId;
                break;
            case DragKind.TrimLeft:
                var minimumStart = Math.Max(0, gesture.OriginalStart - gesture.OriginalSourceOffset);
                var maximumStart = gesture.OriginalStart + gesture.OriginalDuration - MinimumClipDuration;
                gesture.Start = Math.Clamp(Snap(gesture.OriginalStart + delta), minimumStart, Math.Max(minimumStart, maximumStart));
                gesture.Duration = gesture.OriginalStart + gesture.OriginalDuration - gesture.Start;
                gesture.SourceOffset = gesture.OriginalSourceOffset + gesture.Start - gesture.OriginalStart;
                break;
            case DragKind.TrimRight:
                var maximumDuration = Math.Max(MinimumClipDuration, Math.Min(gesture.Clip!.SourceDuration - gesture.OriginalSourceOffset, TotalDuration - gesture.OriginalStart));
                gesture.Duration = Math.Clamp(Snap(gesture.OriginalStart + gesture.OriginalDuration + delta) - gesture.OriginalStart, MinimumClipDuration, maximumDuration);
                break;
            case DragKind.LoopStart:
                var end = gesture.OriginalStart + gesture.OriginalDuration;
                gesture.Start = Math.Clamp(Snap(XToTime(point.X)), 0, Math.Max(0, end - MinimumClipDuration));
                gesture.Duration = end - gesture.Start;
                break;
            case DragKind.LoopEnd:
                gesture.Duration = Math.Clamp(Snap(XToTime(point.X)) - gesture.OriginalStart, MinimumClipDuration, Math.Max(MinimumClipDuration, TotalDuration - gesture.OriginalStart));
                break;
            case DragKind.LoopSelection:
                var current = XToTime(point.X);
                gesture.Start = Math.Min(current, gesture.OriginalStart);
                gesture.Duration = Math.Max(MinimumClipDuration, Math.Abs(current - gesture.OriginalStart));
                break;
        }
        Cursor = gesture.Kind == DragKind.Move ? Cursors.SizeAll : Cursors.SizeWE;
        InvalidateContent();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_gesture is not { } gesture) return;
        _gesture = null;
        if (IsMouseCaptured) ReleaseMouseCapture();
        if (gesture.HasMoved && CanEdit)
        {
            var kind = gesture.Kind switch
            {
                DragKind.Move => TimelineEditKind.Move,
                DragKind.TrimLeft or DragKind.TrimRight => TimelineEditKind.Trim,
                _ => TimelineEditKind.Loop
            };
            if (Math.Abs(gesture.Start - gesture.OriginalStart) > .00001 || Math.Abs(gesture.Duration - gesture.OriginalDuration) > .00001 || gesture.TargetTrackId != gesture.TrackId)
                EditRequested?.Invoke(this, new TimelineEditEventArgs
                {
                    Kind = kind, TrackId = gesture.TrackId, ClipId = gesture.Clip?.Id ?? Guid.Empty,
                    TargetTrackId = gesture.TargetTrackId, Start = gesture.Start, Duration = gesture.Duration,
                    SourceOffset = gesture.SourceOffset, Time = XToTime(e.GetPosition(this).X)
                });
        }
        Cursor = Cursors.Arrow;
        InvalidateContent();
        e.Handled = true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e) { base.OnLostMouseCapture(e); CancelGesture(); }
    protected override void OnMouseLeave(MouseEventArgs e) { base.OnMouseLeave(e); if (_gesture is null) Cursor = Cursors.Arrow; }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);
        Focus();
        var hit = HitClip(e.GetPosition(this));
        if (hit is null) return;
        SelectionRequested?.Invoke(this, new TimelineSelectionEventArgs(hit.Track.Id, hit.Clip.Id));
        var menu = new ContextMenu { PlacementTarget = this, Placement = PlacementMode.MousePoint };
        menu.Opened += (_, _) => StudioPopupScale.Apply(menu);
        void AddItem(string label, string shortcut, TimelineEditKind kind, bool enabled)
        {
            var item = new MenuItem { Header = LanguageService.T(label), InputGestureText = shortcut, IsEnabled = enabled };
            item.Click += (_, _) => EmitClipAction(kind, hit.Track, hit.Clip);
            menu.Items.Add(item);
        }
        var position = Project?.Position ?? 0;
        if (PracticeMode)
        {
            if (Project?.Tracks.IndexOf(hit.Track) == 1)
            {
                void TrimAtCursor(string label, bool fromStart)
                {
                    var item = new MenuItem { Header = LanguageService.T(label), IsEnabled = CanEdit && position > hit.Clip.Start + MinimumClipDuration && position < hit.Clip.Start + hit.Clip.Duration - MinimumClipDuration };
                    item.Click += (_, _) => EditRequested?.Invoke(this, new TimelineEditEventArgs
                    {
                        Kind = TimelineEditKind.Trim, TrackId = hit.Track.Id, ClipId = hit.Clip.Id,
                        Start = fromStart ? position : hit.Clip.Start,
                        Duration = fromStart ? hit.Clip.Start + hit.Clip.Duration - position : position - hit.Clip.Start
                    });
                    menu.Items.Add(item);
                }
                TrimAtCursor("Aparar início até o cursor", true);
                TrimAtCursor("Aparar fim até o cursor", false);
                AddItem("Excluir tentativa", "Delete", TimelineEditKind.Delete, CanEdit);
                menu.Items.Add(new Separator());
            }
            AddItem("Repetir este trecho", "", TimelineEditKind.Loop, CanEdit);
            menu.IsOpen = true;
            e.Handled = true;
            return;
        }
        AddItem("Dividir no cursor", "S", TimelineEditKind.Split, CanEdit && position > hit.Clip.Start + MinimumClipDuration && position < hit.Clip.Start + hit.Clip.Duration - MinimumClipDuration);
        AddItem("Duplicar clipe", "Ctrl+D", TimelineEditKind.Duplicate, CanEdit);
        AddItem("Excluir clipe", "Delete", TimelineEditKind.Delete, CanEdit);
        menu.Items.Add(new Separator());
        AddItem("Loop neste clipe", "", TimelineEditKind.Loop, CanEdit);
        menu.IsOpen = true;
        e.Handled = true;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        var point = e.GetPosition(this);
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (PracticeMode) return;
            var track = TrackAt(point.Y);
            if (track is not null) TrackHeightRequested?.Invoke(this, new TimelineTrackHeightEventArgs(track.Id, e.Delta > 0 ? 1.25 : .8));
            e.Handled = true;
        }
        else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            var anchor = XToTime(point.X);
            _zoom = Math.Clamp(_zoom * (e.Delta > 0 ? 1.25 : .8), 1, 32);
            _viewStart = Math.Clamp(anchor - point.X / Math.Max(.001, ActualWidth) * VisibleDuration, 0, Math.Max(0, TotalDuration - VisibleDuration));
            ViewChanged?.Invoke(this, EventArgs.Empty);
            InvalidateContent();
            e.Handled = true;
        }
        else if (_zoom > 1)
        {
            _viewStart = Math.Clamp(_viewStart - Math.Sign(e.Delta) * VisibleDuration * .12, 0, Math.Max(0, TotalDuration - VisibleDuration));
            ViewChanged?.Invoke(this, EventArgs.Empty);
            InvalidateContent();
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && _gesture is not null) { CancelGesture(); e.Handled = true; return; }
        if (e.Key == Key.D0 && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) { FitProject(); e.Handled = true; return; }
        if (CanSeek && (e.Key is Key.Left or Key.Right) && Keyboard.Modifiers is ModifierKeys.None or ModifierKeys.Shift)
        {
            var amount = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 5 : 1;
            SeekRequested?.Invoke(this, new TimelineSeekEventArgs(Math.Clamp((Project?.Position ?? 0) + (e.Key == Key.Left ? -amount : amount), 0, TotalDuration)));
            e.Handled = true;
            return;
        }
        if (!CanEdit || Project is null || PracticeMode) return;
        var selected = Project.Tracks.SelectMany(track => track.Clips.Select(clip => (track, clip))).FirstOrDefault(pair => pair.clip.Id == Project.SelectedClipId);
        if (selected.clip is null) return;
        TimelineEditKind? kind = e.Key switch
        {
            Key.S when Keyboard.Modifiers == ModifierKeys.None => TimelineEditKind.Split,
            Key.Delete when Keyboard.Modifiers == ModifierKeys.None => TimelineEditKind.Delete,
            Key.D when Keyboard.Modifiers == ModifierKeys.Control => TimelineEditKind.Duplicate,
            _ => null
        };
        if (kind is not null) { EmitClipAction(kind.Value, selected.track, selected.clip); e.Handled = true; }
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);
        if (CanEdit && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            _dropTrack = TrackAt(e.GetPosition(this).Y)?.Id;
            e.Effects = _dropTrack is null ? DragDropEffects.None : DragDropEffects.Copy;
        }
        else { _dropTrack = null; e.Effects = DragDropEffects.None; }
        InvalidateContent();
        e.Handled = true;
    }
    protected override void OnDragLeave(DragEventArgs e) { base.OnDragLeave(e); _dropTrack = null; InvalidateContent(); }
    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);
        _dropTrack = null;
        var point = e.GetPosition(this);
        var track = TrackAt(point.Y);
        if (CanEdit && track is not null && e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Length > 0)
            FilesDropped?.Invoke(this, new TimelineFilesDroppedEventArgs(paths, track.Id, XToTime(point.X)));
        InvalidateContent();
        e.Handled = true;
    }

    private void EmitClipAction(TimelineEditKind kind, TrackModel track, ClipModel clip) => EditRequested?.Invoke(this,
        new TimelineEditEventArgs { Kind = kind, TrackId = track.Id, ClipId = clip.Id, TargetTrackId = track.Id,
            Start = clip.Start, Duration = clip.Duration, SourceOffset = clip.SourceOffset, Time = Project?.Position ?? 0 });

    private void UpdateHover(Point point)
    {
        var hit = HitClip(point);
        if (hit is not null)
        {
            if (PracticeMode)
            {
                var canTrim = CanEdit && Project?.Tracks.IndexOf(hit.Track) == 1;
                Cursor = canTrim && (point.X - hit.Rect.Left < 6 || hit.Rect.Right - point.X < 6) ? Cursors.SizeWE : Cursors.IBeam;
                ToolTip = hit.Clip.Name + " · " + hit.Clip.Duration.ToString("0.##", CultureInfo.CurrentCulture) + " s\n" + LanguageService.T(canTrim ? "Arraste as bordas para aparar · botão direito: opções" : "Clique para posicionar o cursor");
                return;
            }
            Cursor = CanEdit ? (point.X - hit.Rect.Left < 6 || hit.Rect.Right - point.X < 6 ? Cursors.SizeWE : Cursors.SizeAll) : Cursors.Arrow;
            ToolTip = $"{hit.Clip.Name} · {hit.Clip.Duration:0.##} s\n" + LanguageService.T("Arraste: mover · bordas: trim · Alt: ignorar grade · botão direito: opções");
        }
        else
        {
            Cursor = point.Y < EffectiveRulerHeight ? Cursors.IBeam : Cursors.Arrow;
            ToolTip = LanguageService.T(PracticeMode ? "Clique: posicionar cursor · Shift+arraste na régua: A/B · Shift+rolagem: zoom · Ctrl+0: enquadrar" : "Clique: posicionar cursor · Ctrl+scroll: altura da track · Shift+scroll: zoom · Ctrl+0: enquadrar projeto");
        }
    }

    private double Snap(double value)
    {
        if (PracticeMode || !SnapEnabled || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Project is null) return value;
        var division = 60 / Math.Max(30, Project.Bpm) / 4;
        return Math.Round(value / division) * division;
    }
    private (double Start, double End) LoopBounds() => _gesture is { Kind: DragKind.LoopStart or DragKind.LoopEnd or DragKind.LoopSelection } gesture
        ? (gesture.Start, gesture.Start + gesture.Duration) : (Project?.LoopStart ?? 0, Project?.LoopEnd ?? 0);

    private List<(TrackModel Track, Rect Bounds)> TrackBounds()
    {
        var result = new List<(TrackModel Track, Rect Bounds)>();
        if (Project is null) return result;
        if (VisibleTrackIndex >= 0)
        {
            if (VisibleTrackIndex < Project.Tracks.Count)
                result.Add((Project.Tracks[VisibleTrackIndex], new Rect(0, EffectiveRulerHeight, Math.Max(0, ActualWidth), Math.Max(0, ActualHeight - EffectiveRulerHeight))));
            return result;
        }
        var heights = CalculateTrackHeights(Project, ActualHeight - EffectiveRulerHeight);
        var y = EffectiveRulerHeight;
        for (var i = 0; i < Project.Tracks.Count; i++)
        {
            result.Add((Project.Tracks[i], new Rect(0, y, Math.Max(0, ActualWidth), heights[i])));
            y += heights[i];
        }
        return result;
    }
    private TrackModel? TrackAt(double y) => TrackBounds().FirstOrDefault(pair => y >= pair.Bounds.Top && y < pair.Bounds.Bottom).Track;
    private ClipHit? HitClip(Point point)
    {
        foreach (var (track, lane) in TrackBounds())
        {
            if (!lane.Contains(point)) continue;
            foreach (var clip in track.Clips.Reverse())
            {
                var rect = ClipRect(new DisplayClip(clip, clip.Start, clip.Duration, clip.SourceOffset), lane);
                if (rect.Contains(point)) return new ClipHit(track, clip, rect);
            }
        }
        return null;
    }
    private Rect ClipRect(DisplayClip clip, Rect lane) => new(TimeToX(clip.Start), lane.Top + 6,
        Math.Max(1, clip.Duration * PixelsPerSecond), Math.Max(10, lane.Height - 18));
    private IEnumerable<DisplayClip> DisplayClips(TrackModel track)
    {
        foreach (var clip in track.Clips)
        {
            if (_gesture is { HasMoved: true, Clip: not null } drag && drag.Clip.Id == clip.Id) continue;
            yield return new DisplayClip(clip, clip.Start, clip.Duration, clip.SourceOffset);
        }
        if (_gesture is { HasMoved: true, Clip: not null } gesture && gesture.TargetTrackId == track.Id)
            yield return new DisplayClip(gesture.Clip, gesture.Start, gesture.Duration, gesture.SourceOffset);
    }

    private void CancelGesture()
    {
        _gesture = null;
        if (IsMouseCaptured) ReleaseMouseCapture();
        Cursor = Cursors.Arrow;
        InvalidateContent();
    }
    private void RewireSubscriptions()
    {
        DetachSubscriptions();
        if (Project is null) return;
        if (_gesture?.Clip is { } draggedClip && !Project.Tracks.Any(track => track.Clips.Contains(draggedClip)))
            CancelGesture();
        Subscribe(Project);
        Subscribe(Project.Tracks);
        foreach (var track in Project.Tracks)
        {
            Subscribe(track);
            Subscribe(track.Clips);
            foreach (var clip in track.Clips)
            {
                Subscribe(clip);
                Subscribe(clip.Takes);
                foreach (var take in clip.Takes) Subscribe(take);
            }
        }
        InvalidateContent();
    }
    private void Subscribe(object item)
    {
        if (item is INotifyPropertyChanged property)
        {
            property.PropertyChanged += ModelPropertyChanged;
            _propertySubscriptions.Add(property);
        }
        if (item is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += ModelCollectionChanged;
            _collectionSubscriptions.Add(collection);
        }
    }
    private void DetachSubscriptions()
    {
        foreach (var subscription in _propertySubscriptions) subscription.PropertyChanged -= ModelPropertyChanged;
        foreach (var subscription in _collectionSubscriptions) subscription.CollectionChanged -= ModelCollectionChanged;
        _propertySubscriptions.Clear();
        _collectionSubscriptions.Clear();
    }
    private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Volume and pan are rendered by the adjacent native knobs, not the waveform.
        if (sender is TrackModel && e.PropertyName is nameof(TrackModel.VolumeDb) or nameof(TrackModel.Pan)) return;
        if (ReferenceEquals(sender, Project) && e.PropertyName == nameof(ProjectModel.Position))
        {
            if (IsVisible) UpdateCursor();
            return;
        }
        _viewStart = Math.Clamp(_viewStart, 0, Math.Max(0, TotalDuration - VisibleDuration));
        InvalidateContent();
    }
    private void ModelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RewireSubscriptions();
    private static void ProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timeline = (TimelineControl)d;
        timeline.CancelGesture();
        timeline.ResetView();
        timeline.RewireSubscriptions();
    }
    private void LanguageChanged(object? sender, PropertyChangedEventArgs e) => InvalidateContent();
    private void PeaksChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.CheckAccess()) InvalidateContent();
        else Dispatcher.BeginInvoke(InvalidateContent);
    }
    private static void PeaksProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timeline = (TimelineControl)d;
        if (e.OldValue is IStudioPeakProvider previous) previous.PeaksChanged -= timeline.PeaksChanged;
        if (timeline.IsLoaded && e.NewValue is IStudioPeakProvider current) current.PeaksChanged += timeline.PeaksChanged;
    }
    private static void CanEditChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue) ((TimelineControl)d).CancelGesture();
    }

    private Brush ResourceBrush(string key, string fallback)
    {
        if (_drawingBrushes.TryGetValue(key, out var cached)) return cached;
        var brush = (TryFindResource(key) as Brush)?.CloneCurrentValue() ?? new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback));
        if (brush.CanFreeze) brush.Freeze();
        _drawingBrushes[key] = brush;
        return brush;
    }
    private static Brush OpacityBrush(Brush brush, double opacity)
    {
        var clone = brush.Clone();
        clone.Opacity *= opacity;
        clone.Freeze();
        return clone;
    }
    private void DrawText(DrawingContext dc, string text, Point origin, Brush brush, double maxWidth)
    {
        if (maxWidth <= 1) return;
        var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), TryFindResource("CubeFontSizeCaption") is double fontSize ? fontSize : 12, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip)
        { MaxTextWidth = maxWidth, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis };
        dc.DrawText(formatted, origin);
    }
    private double TextWidth(string text) => new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
        new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), TryFindResource("CubeFontSizeCaption") is double fontSize ? fontSize : 12, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip).Width;
    private static string FormatTime(double seconds, double interval)
    {
        var time = TimeSpan.FromSeconds(Math.Max(0, seconds));
        if (seconds < 1) return seconds.ToString("0.##", CultureInfo.CurrentCulture) + "s";
        return interval < 1
            ? $"{(int)time.TotalMinutes}:" + (time.Seconds + time.Milliseconds / 1000d).ToString("00.##", CultureInfo.CurrentCulture)
            : $"{(int)time.TotalMinutes}:{time.Seconds:00}";
    }

    private enum DragKind { Move, TrimLeft, TrimRight, LoopStart, LoopEnd, LoopSelection }
    private sealed class TimelineAutomationPeer(TimelineControl owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(TimelineControl);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;
        protected override string GetHelpTextCore() => LanguageService.T(owner.PracticeMode ? "Clique: posicionar cursor · Shift+arraste na régua: A/B · Shift+rolagem: zoom · Ctrl+0: enquadrar" : "Clique: posicionar cursor · S: dividir · Ctrl+scroll: altura da track · Shift+scroll: zoom");
    }
    private sealed class Gesture(DragKind kind, Point origin, Guid trackId, ClipModel? clip, double start, double duration, double sourceOffset)
    {
        public DragKind Kind { get; } = kind;
        public Point Origin { get; } = origin;
        public Guid TrackId { get; } = trackId;
        public Guid TargetTrackId { get; set; } = trackId;
        public ClipModel? Clip { get; } = clip;
        public double OriginalStart { get; } = start;
        public double OriginalDuration { get; } = duration;
        public double OriginalSourceOffset { get; } = sourceOffset;
        public double Start { get; set; } = start;
        public double Duration { get; set; } = duration;
        public double SourceOffset { get; set; } = sourceOffset;
        public bool HasMoved { get; set; }
    }
    private sealed record DisplayClip(ClipModel Model, double Start, double Duration, double SourceOffset);
    private sealed record ClipHit(TrackModel Track, ClipModel Clip, Rect Rect);
}
