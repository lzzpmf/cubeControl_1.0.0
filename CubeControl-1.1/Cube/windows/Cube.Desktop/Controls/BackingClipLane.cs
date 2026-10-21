using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Controls;

public sealed class BackingClipLane : FrameworkElement
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(BackingTracksViewModel), typeof(BackingClipLane), new PropertyMetadata(null, Changed));
    public static readonly DependencyProperty StemProperty = DependencyProperty.Register(nameof(Stem), typeof(BackingStem), typeof(BackingClipLane), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, StemChanged));
    public BackingTracksViewModel? Model { get => (BackingTracksViewModel?)GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    public BackingStem? Stem { get => (BackingStem?)GetValue(StemProperty); set => SetValue(StemProperty, value); }
    private static void StemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var lane = (BackingClipLane)d;
        if (e.OldValue is BackingStem old) PropertyChangedEventManager.RemoveHandler(old, lane.StemUpdated, "");
        if (e.NewValue is BackingStem stem) PropertyChangedEventManager.AddHandler(stem, lane.StemUpdated, "");
    }
    private void StemUpdated(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BackingStem.Color) or nameof(BackingStem.Peaks)) InvalidateVisual();
    }
    private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var lane = (BackingClipLane)d;
        if (e.OldValue is BackingTracksViewModel old) PropertyChangedEventManager.RemoveHandler(old, lane.Update, "");
        if (e.NewValue is BackingTracksViewModel model) PropertyChangedEventManager.AddHandler(model, lane.Update, "");
        lane.InvalidateVisual();
    }
    private void Update(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "ClipRevision" or "Position" or "ViewportStart" or "ViewportDuration" or "TrackHeight" or "IsRecording") InvalidateVisual();
        if (e.PropertyName == "ClipRevision" && UIElementAutomationPeer.FromElement(this) is LanePeer peer)
            peer.RefreshClips();
    }
    public BackingClipLane()
    {
        Focusable = true; ClipToBounds = true; AllowDrop = true;
        System.Windows.Automation.AutomationProperties.SetName(this, LanguageService.T("Editor de clipes"));
        ToolTip = LanguageService.T("Arraste o corpo para mover e as bordas para aparar. Alt ignora o snap.");
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new LanePeer(this);
    private Brush Brush(string key) => TryFindResource(key) as Brush ?? Brushes.Gray;
    private double X(double time) => 12 + (time - (Model?.ViewportStart ?? 0)) / Math.Max(.001, Model?.StudioViewportDuration ?? 1) * Math.Max(1, ActualWidth - 24);
    private double Time(double x) => (Model?.ViewportStart ?? 0) + (x - 12) / Math.Max(1, ActualWidth - 24) * Math.Max(.001, Model?.StudioViewportDuration ?? 1);
    private readonly List<(BackingClip Clip, Rect Bounds)> areas = [];
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc); dc.DrawRectangle(Brush("CubeWell"), null, new Rect(RenderSize)); areas.Clear();
        if (Model == null || Stem == null || Model.StudioViewportDuration <= 0) return;
        double bar = 60 / Math.Max(20, Model.TransportBpm) * 4;
        var meter = Model.SessionMeter.Split('/');
        if (meter.Length == 2 && int.TryParse(meter[0], out int n) && int.TryParse(meter[1], out int d) && d > 0) bar = 60 / Math.Max(20, Model.TransportBpm) * n * 4 / d;
        var gridPen = new Pen(Brush("CubeBorder"), .5);
        for (double t = Math.Ceiling((Model.ViewportStart - Model.FirstBeatOffset) / bar) * bar + Model.FirstBeatOffset; t < Model.ViewportStart + Model.StudioViewportDuration; t += bar)
            dc.DrawLine(gridPen, new Point(X(t), 0), new Point(X(t), ActualHeight));
        var rows = new List<double>(); var placements = new List<(BackingClip Clip, int Row)>();
        var visibleClips = Stem.Clips.ToList();
        if (Model.IsRecording && Stem.IsArmed && Stem.TimelineLength > 0)
        {
            var take = new BackingClipTake(Guid.Empty, Model.RecordingState, [new(Guid.Empty, "capture", 0, 0, Stem.TimelineLength, Stem.RecordedSpeed)]);
            visibleClips.Add(new(Guid.Empty, Model.RecordingState, Stem.Offset, Stem.TimelineLength, [take], Guid.Empty));
        }
        foreach (var clip in visibleClips.OrderBy(c => c.Position))
        {
            int row = rows.FindIndex(end => end <= clip.Position); if (row < 0) { row = rows.Count; rows.Add(0); }
            rows[row] = clip.End; placements.Add((clip, row));
        }
        if (visibleClips.Count == 0)
        {
            var empty = new Rect(12, 10, Math.Max(0, ActualWidth - 24), Math.Max(0, ActualHeight - 20));
            dc.DrawRoundedRectangle(null, new Pen(Brush("CubeBorder"), 1) { DashStyle = DashStyles.Dash }, empty, 3, 3);
            var hint = new FormattedText(LanguageService.T("Arraste áudio aqui"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), TryFindResource("CubeFontSizeCaption") is double size ? size : 12, Brush("Muted"), VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(hint, new Point(Math.Max(12, (ActualWidth - hint.Width) / 2), Math.Max(10, (ActualHeight - hint.Height) / 2)));
        }
        double height = ActualHeight / Math.Max(1, rows.Count);
        foreach (var (clip, row) in placements)
        {
            double left = X(clip.Position), right = X(clip.End);
            if (right < 0 || left > ActualWidth) continue;
            var bounds = new Rect(left, row * height + 2, Math.Max(2, right - left), Math.Max(2, height - 4)); areas.Add((clip, bounds));
            bool selected = Model.SelectedClips.Contains(clip.Id);
            var trackBrush = SystemParameters.HighContrast ? Brush("CubeAccent") : new SolidColorBrush((Color)ColorConverter.ConvertFromString(Stem.Color));
            var tint = trackBrush.Clone(); tint.Opacity = selected ? .38 : .22;
            dc.DrawRoundedRectangle(Brush("CubeInset"), null, bounds, 4, 4);
            dc.DrawRoundedRectangle(tint, new Pen(Brush(selected ? "CubeAccent" : "CubeBorder"), selected ? 2 : 1), bounds, 4, 4);
            dc.PushClip(new RectangleGeometry(bounds));
            var text = new FormattedText(clip.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), TryFindResource("CubeFontSizeCaption") is double size ? size : 12, Brush("CubeText"), VisualTreeHelper.GetDpi(this).PixelsPerDip) { MaxTextWidth = Math.Max(1, Math.Min(ActualWidth, bounds.Width) - 16), MaxTextHeight = 20, Trimming = TextTrimming.CharacterEllipsis };
            dc.DrawText(text, new Point(left + 8, bounds.Top + 2));
            double center = bounds.Top + 22 + Math.Max(0, bounds.Height - 24) / 2, amplitude = Math.Max(0, bounds.Height - 26) / 2;
            var pen = new Pen(trackBrush, 1);
            foreach (var layer in clip.ActiveTake?.Layers ?? [])
            {
                (double Duration, float[] Peaks) wave;
                if (layer.Source == "capture") wave = (Stem.Length, Stem.Peaks);
                else if (!Model.ClipWaves.TryGetValue(layer.Source, out wave)) continue;
                if (wave.Peaks.Length == 0) continue;
                for (double x = Math.Max(0, left); x < Math.Min(ActualWidth, right); x += 2)
                {
                    double local = Time(x) - clip.Position - layer.Position;
                    if (local < 0 || local >= layer.Duration) continue;
                    double source = layer.SourceStart + local / layer.RecordedSpeed;
                    int index = (int)(source / Math.Max(.001, wave.Duration) * wave.Peaks.Length);
                    if (index < 0 || index >= wave.Peaks.Length) continue;
                    double peak = wave.Peaks[index] * amplitude;
                    dc.DrawLine(pen, new Point(x, center - peak), new Point(x, center + peak));
                }
            }
            dc.Pop();
        }
        double playhead = X(Model.Position); dc.DrawLine(new Pen(Brush("CubeText"), 2), new(playhead, 0), new(playhead, ActualHeight));
        if (IsKeyboardFocused) dc.DrawRectangle(null, new Pen(Brush("CubeFocus"), 1), new Rect(RenderSize));
    }
    private BackingClip? dragClip;
    private Point origin;
    private string gesture = "";
    private bool moved;
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e); Focus();
        if (Model == null || Stem == null || !Model.CanEditSession) return;
        Model.SelectedStem = Stem;
        var point = e.GetPosition(this); var hit = areas.LastOrDefault(a => a.Bounds.Contains(point));
        if (hit.Clip == null) { Model.Position = Math.Max(0, Time(point.X)); return; }
        if (!Model.SelectedClips.Contains(hit.Clip.Id) || Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) Model.SelectClip(Stem, hit.Clip, Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
        if (hit.Clip.Duration < .001) { e.Handled = true; return; }
        dragClip = hit.Clip; origin = point; moved = false;
        gesture = point.X - hit.Bounds.Left < 8 ? "left" : hit.Bounds.Right - point.X < 8 ? "right" : "move";
        Model.BeginClipGesture(); CaptureMouse(); e.Handled = true;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (dragClip == null || !IsMouseCaptured || Model == null || Stem == null) return;
        Point point = e.GetPosition(this);
        if (!moved && Math.Abs(point.X - origin.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(point.Y - origin.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        moved = true;
        double time = Model.SnapTime(Time(point.X), Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));
        if (gesture == "move")
        {
            DragFeedback.Show(this, dragClip.Name + " · " + LanguageService.T("Mover clipes") + " · " + Model.SnapTime(dragClip.Position + Time(point.X) - Time(origin.X), Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)).ToString("0.000") + " s", point, true);
            return;
        }
        var clip = dragClip;
        if (gesture == "left") { time = Math.Clamp(time, Math.Max(0, clip.Position + (clip.ActiveTake?.Layers.Select(l => l.Position).DefaultIfEmpty(0).Min() ?? 0)), clip.End - .001); clip = clip.Trim(time, clip.End); }
        else if (gesture == "right") { time = Math.Clamp(time, clip.Position + .001, clip.Position + (clip.ActiveTake?.Layers.Select(l => l.Position + l.Duration).DefaultIfEmpty(clip.Duration).Max() ?? clip.Duration)); clip = clip.Trim(clip.Position, time); }
        Model.ReplaceClip(Stem, clip);
    }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (dragClip != null && Model != null && Stem != null)
        {
            if (gesture == "move" && moved)
            {
                double target = Model.SnapTime(dragClip.Position + Time(e.GetPosition(this).X) - Time(origin.X), Keyboard.Modifiers.HasFlag(ModifierKeys.Alt));
                var window = Window.GetWindow(this); var hit = window?.InputHitTest(e.GetPosition(window)) as DependencyObject;
                BackingClipLane? lane = null;
                for (var node = hit; node != null; node = VisualTreeHelper.GetParent(node)) if (node is BackingClipLane found) { lane = found; break; }
                if (lane?.Stem == null) { dragClip = null; DragFeedback.Clear(); ReleaseMouseCapture(); Model.CancelClipGesture(); e.Handled = true; return; }
                Model.MoveClips(target - dragClip.Position, lane.Stem == Stem ? null : lane.Stem);
            }
            dragClip = null; DragFeedback.Clear(); ReleaseMouseCapture(); Model.EndClipGesture(); e.Handled = true;
        }
        base.OnMouseLeftButtonUp(e);
    }
    protected override void OnLostMouseCapture(MouseEventArgs e) { if (dragClip != null) { dragClip = null; DragFeedback.Clear(); Model?.CancelClipGesture(); } base.OnLostMouseCapture(e); }
    protected override void OnDragOver(DragEventArgs e)
    {
        if (Model?.CanEditSession == true && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Any(f => System.IO.Path.GetExtension(f).ToLowerInvariant() is ".mp3" or ".wav"))
        {
            e.Effects = DragDropEffects.Copy; e.Handled = true;
            DragFeedback.Show(this, LanguageService.T("Importar áudio"), e.GetPosition(this), true);
        }
        else { e.Effects = DragDropEffects.None; e.Handled = true; }
    }
    protected override void OnDragLeave(DragEventArgs e) { DragFeedback.Clear(); base.OnDragLeave(e); }
    protected override async void OnDrop(DragEventArgs e)
    {
        DragFeedback.Clear();
        if (Model == null || Stem == null || e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        e.Handled = true;
        double at = Model.SnapTime(Time(e.GetPosition(this).X));
        try { foreach (string file in files) await Model.InsertLocalAudio(file, Stem, at); }
        catch (Exception ex) { MessageBox.Show(ex.Message, LanguageService.T("Importar áudio")); }
    }
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (Model == null || Stem == null) return;
        var clip = areas.LastOrDefault(a => a.Bounds.Contains(e.GetPosition(this))).Clip;
        if (clip != null && !Model.SelectedClips.Contains(clip.Id)) Model.SelectClip(Stem, clip, false);
        ShowMenu(); e.Handled = true;
    }
    private void ShowMenu()
    {
        if (Model == null || Stem == null) return;
        var menu = new ContextMenu();
        void Add(string title, Action action) { var item = new MenuItem { Header = LanguageService.T(title), IsEnabled = Model.CanEditSession }; item.Click += (_, _) => action(); menu.Items.Add(item); }
        Add("Dividir no cursor", Model.SplitClips); Add("Copiar", Model.CopyClips); Add("Colar", () => Model.PasteClips()); Add("Duplicar clipes", () => Model.PasteClips(true)); Add("Excluir clipes", Model.DeleteClips);
        if (Stem.Clips.FirstOrDefault(c => Model.SelectedClips.Contains(c.Id)) is { } clip)
        {
            Add("Localizar áudio do clipe", () => { var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "MP3/WAV|*.mp3;*.wav" }; if (dialog.ShowDialog() == true && clip.ActiveTake?.Layers.FirstOrDefault() is { } layer) _ = Model.RelinkClipSource(Stem, layer.Source, dialog.FileName); });
            var destinations = new MenuItem { Header = LanguageService.T("Mover para pista"), IsEnabled = Model.CanEditSession };
            foreach (var destination in Model.SessionStems ?? []) { var item = new MenuItem { Header = destination.Name }; item.Click += (_, _) => { Model.BeginClipGesture(); Model.MoveClips(0, destination); Model.EndClipGesture(); }; destinations.Items.Add(item); }
            menu.Items.Add(destinations);

        }
        ContextMenu = menu; menu.PlacementTarget = this; menu.IsOpen = true;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && dragClip != null) { dragClip = null; DragFeedback.Clear(); ReleaseMouseCapture(); Model?.CancelClipGesture(); e.Handled = true; return; }
        if (Model?.CanEditSession == true && e.Key is Key.Left or Key.Right
            && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) && Model.SelectedClips.Count > 0)
        {
            double step = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? .1 : .01;
            Model.BeginClipGesture();
            Model.MoveClips(e.Key == Key.Left ? -step : step);
            Model.EndClipGesture(); e.Handled = true; return;
        }
        if (Model != null && Stem != null && e.Key is Key.Left or Key.Right && Stem.Clips.Count > 0)
        {
            var clips = Stem.Clips.OrderBy(c => c.Position).ToArray(); int index = Array.FindIndex(clips, c => Model.SelectedClips.Contains(c.Id));
            var selected = clips[Math.Clamp(index + (e.Key == Key.Right ? 1 : -1), 0, clips.Length - 1)];
            Model.SelectClip(Stem, selected, Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
            System.Windows.Automation.AutomationProperties.SetName(this, selected.Name + " · " + selected.Position.ToString("0.000") + " s"); e.Handled = true; return;
        }
        if (e.Key == Key.Apps || e.Key == Key.F10 && Keyboard.Modifiers == ModifierKeys.Shift) { ShowMenu(); e.Handled = true; }
        base.OnKeyDown(e);
    }
    private sealed class LanePeer(BackingClipLane lane) : FrameworkElementAutomationPeer(lane), ISelectionProvider
    {
        private readonly Dictionary<Guid, ClipPeer> children = [];
        protected override string GetClassNameCore() => nameof(BackingClipLane);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.List;
        public override object? GetPattern(PatternInterface pattern) => pattern == PatternInterface.Selection ? this : base.GetPattern(pattern);
        internal void RefreshClips()
        {
            foreach (var id in children.Keys.Where(id => lane.Stem?.Clips.All(c => c.Id != id) != false).ToArray()) children.Remove(id);
            ResetChildrenCache();
            RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
        }
        protected override List<AutomationPeer> GetChildrenCore() => (lane.Stem?.Clips ?? []).OrderBy(c => c.Position)
            .Select(c => (AutomationPeer)(children.TryGetValue(c.Id, out var peer) ? peer : children[c.Id] = new ClipPeer(lane, this, c.Id))).ToList();
        internal IRawElementProviderSimple Provider => ProviderFromPeer(this);
        internal IRawElementProviderSimple ClipProvider(AutomationPeer peer) => ProviderFromPeer(peer);
        public bool CanSelectMultiple => true;
        public bool IsSelectionRequired => false;
        public IRawElementProviderSimple[] GetSelection() => GetChildrenCore().OfType<ClipPeer>().Where(c => c.IsSelected).Select(ClipProvider).ToArray();
    }
    private sealed class ClipPeer(BackingClipLane lane, LanePeer parent, Guid id) : AutomationPeer, ISelectionItemProvider, IInvokeProvider
    {
        private BackingClip? Clip => lane.Stem?.Clips.FirstOrDefault(c => c.Id == id);
        protected override string GetNameCore() => Clip?.Name ?? "";
        protected override string GetAutomationIdCore() => id.ToString();
        protected override string GetClassNameCore() => nameof(BackingClip);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;
        protected override string GetHelpTextCore() => Clip is { } clip
            ? $"{LanguageService.T("Posição")}: {clip.Position:0.###} s; {LanguageService.T("Duração")}: {clip.Duration:0.###} s" : "";
        protected override Rect GetBoundingRectangleCore()
        {
            var bounds = lane.areas.FirstOrDefault(a => a.Clip.Id == id).Bounds;
            if (bounds.IsEmpty || bounds.Width == 0 || !lane.IsVisible || PresentationSource.FromVisual(lane) == null) return Rect.Empty;
            bounds.Intersect(new Rect(lane.RenderSize));
            return bounds.IsEmpty ? Rect.Empty : new Rect(lane.PointToScreen(bounds.TopLeft), lane.PointToScreen(bounds.BottomRight));
        }
        protected override Point GetClickablePointCore() { var r = GetBoundingRectangleCore(); return r.IsEmpty ? new(double.NaN, double.NaN) : new(r.X + r.Width / 2, r.Y + r.Height / 2); }
        protected override List<AutomationPeer>? GetChildrenCore() => null;
        protected override string GetAccessKeyCore() => "";
        protected override string GetAcceleratorKeyCore() => "";
        protected override string GetItemStatusCore() => "";
        protected override string GetItemTypeCore() => "";
        protected override AutomationPeer? GetLabeledByCore() => null;
        protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.Horizontal;
        protected override bool HasKeyboardFocusCore() => lane.IsKeyboardFocused && IsSelected;
        protected override bool IsKeyboardFocusableCore() => true;
        protected override bool IsEnabledCore() => lane.IsEnabled && Clip != null;
        protected override bool IsOffscreenCore() => GetBoundingRectangleCore().IsEmpty;
        protected override bool IsPasswordCore() => false;
        protected override bool IsRequiredForFormCore() => false;
        protected override bool IsContentElementCore() => true;
        protected override bool IsControlElementCore() => true;
        protected override void SetFocusCore() => Select();
        public override object? GetPattern(PatternInterface pattern) => pattern is PatternInterface.SelectionItem or PatternInterface.Invoke ? this : null;
        public bool IsSelected => lane.Model?.SelectedClips.Contains(id) == true;
        public IRawElementProviderSimple SelectionContainer => parent.Provider;
        public void Select() => ChangeSelection(false, false);
        public void AddToSelection() { if (!IsSelected) ChangeSelection(true, false); }
        public void RemoveFromSelection() { if (IsSelected) ChangeSelection(true, true); }
        private void ChangeSelection(bool additive, bool removing)
        {
            if (!lane.IsEnabled) throw new ElementNotEnabledException();
            if (Clip is not { } clip || lane.Model == null || lane.Stem == null) return;
            lane.Model.SelectClip(lane.Stem, clip, additive);
            lane.Focus();
            RaiseAutomationEvent(removing ? AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection
                : additive ? AutomationEvents.SelectionItemPatternOnElementAddedToSelection : AutomationEvents.SelectionItemPatternOnElementSelected);
        }
        public void Invoke() { Select(); lane.ShowMenu(); }
    }

}
