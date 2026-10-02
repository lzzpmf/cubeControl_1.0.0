using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Localization;

namespace Cube.Desktop.Controls;

/// <summary>Illustrative, directly editable diagrams. Values remain TwoWay-bound to the existing models.
/// No audio, transport, persistence or clock lives in this control.</summary>
public sealed class EffectGraphEditor : Grid
{
    public static readonly DependencyProperty EffectKindProperty = DependencyProperty.Register(nameof(EffectKind), typeof(string),
        typeof(EffectGraphEditor), new PropertyMetadata("", (d, _) => ((EffectGraphEditor)d).CreateGraph()));
    public string EffectKind { get => (string)GetValue(EffectKindProperty); set => SetValue(EffectKindProperty, value); }
    private readonly Canvas overlay = new();
    private readonly GraphDrawing drawing;
    private readonly Dictionary<string, GraphNode> nodes = [];
    private readonly Border inspector = new() { Padding = new Thickness(8), CornerRadius = new CornerRadius(5), Visibility = Visibility.Collapsed };
    private readonly TextBlock caption = new() { TextWrapping = TextWrapping.Wrap, FontSize = 12 };
    private readonly Wpf.Ui.Controls.Button number = new() { Padding = new Thickness(6, 2, 6, 2), HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBox entry = new() { Visibility = Visibility.Collapsed, MinWidth = 100 };
    private readonly TextBlock help = new() { FontSize = 11, TextWrapping = TextWrapping.Wrap };
    private GraphNode? selected;
    private double numericStart, resumeRate = 100;
    private bool editing;
    private double W => Math.Max(260, ActualWidth - 48);
    private double X(double fraction) => 24 + W * fraction;
    private static string T(string text) => LanguageService.T(text);
    private double V(string key)
    {
        if (!nodes.TryGetValue(key, out var node)) return 0;
        // Presentation only: a binding can temporarily revert to its DP default while
        // the XAML DataContext is being resolved. Never feed zero into a log axis.
        return double.IsFinite(node.Value) ? Math.Clamp(node.Value, node.Minimum, node.Maximum) : node.Minimum;
    }
    private Brush Brush(string key) => TryFindResource(key) as Brush ?? SystemColors.ControlTextBrush;

    public EffectGraphEditor()
    {
        ClipToBounds = false;
        MinWidth = 308;
        SetResourceReference(BackgroundProperty, "CubeInset");
        drawing = new GraphDrawing(this);
        Children.Add(drawing);
        Children.Add(overlay);
        var content = new StackPanel();
        content.Children.Add(caption); content.Children.Add(number); content.Children.Add(entry); content.Children.Add(help);
        inspector.Child = content;
        inspector.SetResourceReference(Border.BackgroundProperty, "CubeCard");
        inspector.SetResourceReference(Border.BorderBrushProperty, "CubeFocus");
        inspector.BorderThickness = new Thickness(1);
        number.Click += (_, _) => BeginNumber();
        entry.PreviewKeyDown += NumberKey;
        entry.LostKeyboardFocus += (_, _) => { if (editing) EndNumber(false, false); };
        SizeChanged += (_, _) => RefreshGraph();
        IsEnabledChanged += (_, _) => { if (!IsEnabled) CancelInteraction(); RefreshGraph(); };
        IsVisibleChanged += (_, _) => { if (!IsVisible) CancelInteraction(); };
        Loaded += (_, _) =>
        {
            PropertyChangedEventManager.AddHandler(LanguageService.Instance, LanguageChanged, "");
            RefreshGraph();
        };
        Unloaded += (_, _) =>
        {
            CancelInteraction();
            PropertyChangedEventManager.RemoveHandler(LanguageService.Instance, LanguageChanged, "");
        };
        DataContextChanged += (_, _) => { CancelInteraction(); selected = null; inspector.Visibility = Visibility.Collapsed; };
        SetResourceReference(TextElement.ForegroundProperty, "CubeText");
        // Resource changes invalidate the drawing as well as the WPF children.
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
    }

    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int),
        typeof(EffectGraphEditor), new PropertyMetadata(0, (d, _) => ((EffectGraphEditor)d).RefreshGraph()));
    private void LanguageChanged(object? sender, PropertyChangedEventArgs e) => RefreshGraph();

    private void Add(string key, string name, string unit, double min, double max, double step,
        Func<Point> anchor, Func<double, Vector, double> drag, string hint)
    {
        var node = new GraphNode(this, key, name, unit, min, max, step, anchor, drag, hint);
        nodes.Add(key, node);
        node.SetBinding(GraphNode.ValueProperty, new Binding(key)
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            FallbackValue = min,
            TargetNullValue = min
        });
        overlay.Children.Add(node);
    }

    private void Amplitude(string key, string name, string unit, double min, double max, double step,
        double x, double baseline, string hint)
    {
        Add(key, name, unit, min, max, step, () => new Point(X(x), baseline - 48 * V(key) / max),
            (start, delta) => start - delta.Y * max / 48, hint);
    }

    private static double CycleFraction(double value, double min, double max) => min > 0
        ? (1 / Math.Clamp(value, min, max) - 1 / max) / (1 / min - 1 / max)
        : 1 - Math.Clamp(value / max, 0, 1);
    private double CycleWidth(string key, double min, double max) => W * (.2 + .65 * CycleFraction(V(key), min, max));
    private void Cycle(string key, string name, string unit, double min, double max, double y, string hint)
    {
        Add(key, name, unit, min, max, .01, () => new Point(X(0) + CycleWidth(key, min, max), y + 36),
            (start, delta) =>
            {
                double fraction = Math.Clamp(CycleFraction(start, min, max) + delta.X / (W * .65), 0, 1);
                return min > 0 ? 1 / (1 / max + fraction * (1 / min - 1 / max)) : max * (1 - fraction);
            }, hint);
    }

    private void CreateGraph()
    {
        CancelInteraction(); nodes.Clear(); overlay.Children.Clear(); selected = null;
        inspector.Visibility = Visibility.Collapsed;
        Height = EffectKind == "Chorus" ? 650 : EffectKind == "Reverb" ? 620 : 560;
        switch (EffectKind)
        {
            case "Delay":
                Add("TimeSamples", "Tempo", "amostras internas", 2400, 16350, 1,
                    () => new Point(X(.12 + .08 + .16 * V("TimeSamples") / 16350), 96),
                    (start, d) => start + d.X * 16350 / (W * .16), "Arraste a primeira repetição. As demais mantêm o mesmo intervalo.");
                Add("Feedback", "Realimentação", "%", 0, 99.21875, .1,
                    () => new Point(X(.12 + 2 * (.08 + .16 * V("TimeSamples") / 16350)), 156 + 60 * V("Feedback") / 100),
                    (start, d) => start + d.Y * 100 / 60, "Expanda o contorno inferior do segundo eco; o tempo permanece intacto.");
                Amplitude("Depth", "Excursão temporal", "% do original", 0, 100, 1, .23, 310,
                    "Profundidade da modulação do atraso; não altera amplitude de áudio.");
                Cycle("Rate", "Velocidade da modulação", "% do original", 0, 100, 310,
                    "Alongar reduz a velocidade. Depende do tempo; zero congela a fase. Menu: congelar/retomar.");
                Add("Mix", "Mistura interna", "50 = direto e eco integrais", 0, 92.1875, .1,
                    () => new Point(X(.72), 486 - 60 * V("Mix") / 92.1875),
                    (start, d) => start - d.Y * 92.1875 / 60, "Envelope da combinação. Abaixo de 50, cresce o eco; acima, diminui o direto.");
                break;
            case "Chorus":
                Amplitude("Depth", "Excursão temporal", "% do original", 0, 100, 1, .23, 130, "Expanda a crista para aumentar a profundidade, sem alterar ganhos.");
                Cycle("Speed", "Velocidade", "Hz estimados", .384, 3.024, 130, "Alongue o ciclo para reduzir a velocidade estimada.");
                Add("Waveform", "Forma de onda", "", 0, 1, 1,
                    () => new Point(X(.06), 130 - 48 * V("Depth") / 100 * ModulationWave((X(.06) - X(.23)) / CycleWidth("Speed", .384, 3.024))), (s, _) => s,
                    "Escolha Original ou Triangular no menu da curva. Original preserva o seletor ROM.");
                Amplitude("WetGain", "Ramo atrasado", "% do original", 50, 100, 1, .24, 294, "Amplitude do atraso antes da combinação com a entrada.");
                Amplitude("CombinedGain", "Ramo combinado", "% do original", 50, 100, 1, .67, 372, "Amplitude após combinar entrada e atraso; independente dos demais ganhos.");
                Amplitude("Mix", "Contribuição processada", "% do original", 0, 100, 1, .8, 466,
                    "100% mantém a mistura original. O ramo processado contém som direto; não é wet puro.");
                Amplitude("Feedback", "Retorno de realimentação", "%", 0, 10, .1, .35, 558,
                    "Amplitude no caminho de retorno à entrada do buffer, não duração do caminho.");
                break;
            case "Phaser":
                Add("Lower", "Limite inferior", "Hz estimados", 300, 3000, 10,
                    () => new Point(X(.08 + .84 * Math.Log(V("Lower") / 300) / Math.Log(10)), 98),
                    (start, d) => Math.Min(V("Upper"), start * Math.Pow(10, d.X / (W * .84))), "Borda esquerda da faixa logarítmica. Não ultrapassa o limite superior.");
                Add("Upper", "Limite superior", "Hz estimados", 300, 3000, 10,
                    () => new Point(X(.08 + .84 * Math.Log(V("Upper") / 300) / Math.Log(10)), 154),
                    (start, d) => Math.Max(V("Lower"), start * Math.Pow(10, d.X / (W * .84))), "Borda direita da faixa logarítmica. Não ultrapassa o limite inferior.");
                Cycle("Speed", "Velocidade da varredura", "Hz estimados", .48, 6.24, 305, "Ciclo temporal separado do eixo espectral; alongar reduz a velocidade.");
                Add("Sections", "Seções de filtro", "seções", 1, 3, 1, () => new Point(X(.18), 440), (s, _) => s,
                    "Menu da cadeia: 1, 2 ou 3 seções. Os vales são ilustrativos, não uma contagem de estágios.");
                Amplitude("Level", "Envelope de saída", "% do original", 0, 100, 1, .78, 460, "Amplitude de saída independente dos vales. 100% conserva o nível original.");
                break;
            case "Reverb":
                Amplitude("Dry", "Caminho direto", "%", 0, 100, 1, .3, 160, "Ganho direto independente; não reduz o reverberado.");
                Amplitude("Wet", "Saída reverberada", "%", 0, 90, 1, .82, 260, "Envelope global na saída, independente do ganho direto e do retorno das linhas.");
                Amplitude("ReturnLevel", "Retorno das seis linhas", "% do original", 0, 100, 1, .4, 360,
                    "Um ganho comum antes da saída. Zerar o retorno não elimina toda a contribuição processada.");
                Add("Decay", "Persistência / realimentação", "escala interna 1–15", 1, 15, 1,
                    () => new Point(X(.65), PersistenceY(V("Decay"), .65)),
                    (start, d) => 15 * (5 + Math.Log(Math.Clamp((465 - PersistenceY(start, .65) - d.Y) / 78, .001, 1)) / .65) / 3.8,
                    "Seleciona conjuntos de coeficientes. Persistência ilustrativa, sem segundos ou RT60.");
                Add("Damping", "Amortecimento", "escala interna 0–10; 10 = original", 0, 10, 1,
                    () => new Point(X(.7), 512 + 32 * V("Damping") / 10), (start, d) => start + d.Y * 10 / 32,
                    "Maior valor aumenta o peso do estado anterior. Superfície ilustrativa, sem material ou frequência de corte.");
                break;
        }
        overlay.Children.Add(inspector);
        Panel.SetZIndex(inspector, 20);
        RefreshGraph();
    }

    internal void Select(GraphNode node)
    {
        if (selected != node && editing) EndNumber(false);
        selected = node;
        inspector.Visibility = Visibility.Visible;
        RefreshGraph();
    }

    internal void RefreshGraph()
    {
        if (drawing == null) return;
        foreach (var node in nodes.Values)
        {
            Point p = node.Anchor();
            Canvas.SetLeft(node, p.X - 18); Canvas.SetTop(node, p.Y - 18);
            AutomationProperties.SetName(node, $"{T(node.Name)} ({T(node.Unit)})");
            node.ToolTip = $"{T(node.Name)} · {T(node.Hint)}";
            node.InvalidateVisual();
        }
        if (selected != null)
        {
            caption.Text = T(selected.Name);
            number.Content = selected.Key == "Waveform" ? T(selected.Value == 0 ? "Original" : "Triangular")
                : $"{selected.Value.ToString("0.###", CultureInfo.CurrentCulture)} {T(selected.Unit)}";
            AutomationProperties.SetName(number, $"{T(selected.Name)}: {number.Content}. {T("Editar valor")}");
            help.Text = T(selected.Hint);
            inspector.Width = Math.Min(300, W);
            Point p = selected.Anchor();
            Canvas.SetLeft(inspector, Math.Clamp(p.X - 120, 4, Math.Max(4, ActualWidth - inspector.Width - 4)));
            Canvas.SetTop(inspector, Math.Min(Height - 116, p.Y + 24));
        }
        drawing.InvalidateVisual();
    }

    private void BeginNumber()
    {
        if (selected == null || !IsEnabled) return;
        if (selected.Key is "Waveform" or "Sections") { OpenChoices(selected); return; }
        numericStart = selected.Value; editing = true;
        entry.Text = numericStart.ToString("0.########", CultureInfo.CurrentCulture);
        AutomationProperties.SetName(entry, $"{T(selected.Name)} ({T(selected.Unit)})");
        entry.Visibility = Visibility.Visible; number.Visibility = Visibility.Collapsed;
        entry.Focus(); entry.SelectAll();
    }

    private void NumberKey(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape)) return;
        e.Handled = true; EndNumber(e.Key == Key.Enter);
    }

    private void EndNumber(bool commit, bool restoreFocus = true)
    {
        if (!editing || selected == null) return;
        if (commit)
        {
            if (!double.TryParse(entry.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out double value) || !double.IsFinite(value)
                || value < selected.Minimum || value > selected.Maximum
                || (EffectKind == "Phaser" && ((selected.Key == "Lower" && value > V("Upper")) || (selected.Key == "Upper" && value < V("Lower")))))
            { help.Text = T("Valor inválido ou fora dos limites.") + $" {selected.Minimum:0.###}–{selected.Maximum:0.###}"; return; }
            selected.Change(value);
        }
        // Text is only committed on Enter; Escape or focus loss leaves the original value intact.
        editing = false; entry.Visibility = Visibility.Collapsed; number.Visibility = Visibility.Visible;
        if (restoreFocus) selected.Focus();
        RefreshGraph();
    }

    private void CancelInteraction()
    {
        foreach (var node in nodes.Values) node.Cancel();
        if (editing) EndNumber(false, false);
    }

    internal void OpenChoices(GraphNode node)
    {
        Select(node);
        var menu = new ContextMenu { PlacementTarget = node };
        if (node.Key == "Waveform")
        {
            for (int i = 0; i < 2; i++)
            {
                int value = i;
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                var preview = new System.Windows.Shapes.Path { Width = 44, Height = 18, Margin = new Thickness(0, 0, 8, 0), StrokeThickness = 1.5,
                    Data = Geometry.Parse(i == 0 ? "M0,9 C7,-3 15,-3 22,9 C29,21 37,21 44,9" : "M0,9 L11,0 33,18 44,9") };
                preview.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, "CubeText");
                row.Children.Add(preview); row.Children.Add(new TextBlock { Text = T(i == 0 ? "Original" : "Triangular") });
                var item = new MenuItem { Header = row, IsCheckable = true, IsChecked = node.Value == i };
                AutomationProperties.SetName(item, T(i == 0 ? "Original" : "Triangular"));
                item.Click += (_, _) => node.Change(value); menu.Items.Add(item);
            }
        }
        else if (node.Key == "Sections")
        {
            for (int i = 1; i <= 3; i++)
            {
                int value = i;
                var item = new MenuItem { Header = $"{i} {T("seções")}", IsCheckable = true, IsChecked = node.Value == i };
                item.Click += (_, _) => node.Change(value); menu.Items.Add(item);
            }
        }
        else if (EffectKind == "Delay" && node.Key == "Rate")
        {
            var item = new MenuItem { Header = T(node.Value == 0 ? "Retomar modulação" : "Congelar fase") };
            item.Click += (_, _) => { if (node.Value > 0) { resumeRate = node.Value; node.Change(0); } else node.Change(resumeRate); };
            menu.Items.Add(item);
        }
        var edit = new MenuItem { Header = T("Editar valor") };
        if (node.Key is not ("Waveform" or "Sections")) { edit.Click += (_, _) => BeginNumber(); menu.Items.Add(edit); }
        menu.IsOpen = true;
    }

    internal void NodeKey(GraphNode node, KeyEventArgs e)
    {
        Select(node);
        if (e.Key == Key.Escape) { node.Cancel(); inspector.Visibility = Visibility.Collapsed; e.Handled = true; }
        else if (e.Key == Key.Enter) { BeginNumber(); e.Handled = true; }
        else if (e.Key == Key.Space || e.Key == Key.Apps) { OpenChoices(node); e.Handled = true; }
        else if (e.Key is Key.Up or Key.Right or Key.Down or Key.Left)
        {
            double direction = e.Key is Key.Up or Key.Right ? 1 : -1;
            double step = node.Step * (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? .1 : 1);
            node.Change(node.Value + direction * step); e.Handled = true;
        }
    }

    private void Draw(DrawingContext dc)
    {
        if (nodes.Count == 0) return;
        dc.PushOpacity(IsEnabled ? 1 : .55);
        Label(dc, "Arraste os contornos · Tab seleciona · setas ajustam · Enter digita", 0, 8);
        switch (EffectKind)
        {
            case "Delay": DrawDelay(dc); break;
            case "Chorus": DrawChorus(dc); break;
            case "Phaser": DrawPhaser(dc); break;
            case "Reverb": DrawReverb(dc); break;
        }
        Label(dc, "Representação ilustrativa · não é medição de áudio", 0, Height - 24);
        dc.Pop();
    }

    private Pen Pen(string key = "", bool faint = false) => new(Brush(faint ? "Muted" : selected?.Key == key ? "CubeFocus" : "CubeText"), selected?.Key == key ? 2.5 : 1.25);
    private void Line(DrawingContext dc, double x1, double y1, double x2, double y2, string key = "", bool faint = false)
        => dc.DrawLine(Pen(key, faint), new Point(X(x1), y1), new Point(X(x2), y2));
    private void Label(DrawingContext dc, string text, double x, double y)
    {
        var formatted = new FormattedText(T(text), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            12, Brush("Muted"), VisualTreeHelper.GetDpi(this).PixelsPerDip)
        { MaxTextWidth = Math.Max(40, W * (1 - x)), Trimming = TextTrimming.CharacterEllipsis };
        dc.DrawText(formatted, new Point(X(x), y));
    }
    private void Curve(DrawingContext dc, IEnumerable<Point> points, string key, bool fill = false)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            bool first = true;
            foreach (Point p in points) { if (first) { context.BeginFigure(p, fill, fill); first = false; } else context.LineTo(p, true, false); }
        }
        geometry.Freeze();
        if (fill) { dc.PushOpacity(.1); dc.DrawGeometry(Brush("CubeFocus"), null, geometry); dc.Pop(); }
        dc.DrawGeometry(null, Pen(key), geometry);
    }
    private void Envelope(DrawingContext dc, string key, double x, double baseline, double width = .17)
    {
        Point peak = nodes[key].Anchor();
        Curve(dc, [new Point(X(x - width), baseline), peak, new Point(X(x + width), baseline),
            new Point(peak.X, baseline + (baseline - peak.Y) * .6)], key, true);
    }
    private void Modulation(DrawingContext dc, string speed, double min, double max, double baseline, string? depth = null)
    {
        double length = CycleWidth(speed, min, max), amplitude = depth == null ? 26 : 48 * V(depth) / 100;
        bool frozen = EffectKind == "Delay" && V(speed) == 0;
        double start = X(0), crest = X(.23);
        // A crest belongs to the excursion contour; the cycle boundary belongs to the time contour.
        Curve(dc, Enumerable.Range(0, 81).Select(i =>
        {
            double x = start + W * i / 80;
            double phase = (x - crest) / length;
            double wave = ModulationWave(phase);
            return new Point(x, baseline - (frozen ? 0 : amplitude * wave));
        }), depth ?? speed);
        Line(dc, 0, baseline, 1, baseline, faint: true);
        Point edge = nodes[speed].Anchor();
        dc.DrawLine(Pen(speed), new Point(edge.X, baseline - 34), edge);
        if (depth != null)
        {
            Point peak = nodes[depth].Anchor();
            dc.DrawLine(Pen(depth), new Point(X(.16), peak.Y), new Point(X(.3), peak.Y));
        }
        Label(dc, frozen ? "Fase congelada · menu no ciclo para retomar" : "← ciclo mais longo / mais lento", .05, baseline + 42);
    }
    private double ModulationWave(double phase) => EffectKind == "Chorus" && V("Waveform") == 1
        ? 1 - 4 * Math.Abs(phase - Math.Floor(phase + .5)) : Math.Cos(phase * Math.PI * 2);

    private void DrawDelay(DrawingContext dc)
    {
        Label(dc, "Direto e ecos · tempo em amostras internas", 0, 44);
        double interval = .08 + .16 * V("TimeSamples") / 16350;
        Line(dc, 0, 156, 1, 156, faint: true);
        Line(dc, .12, 156, .12, 88);
        var peaks = new List<Point>();
        for (int i = 1; i <= 3; i++)
        {
            double x = .12 + i * interval;
            double top = 156 - 60 * Math.Pow(V("Feedback") / 100, i - 1);
            Curve(dc, [new Point(X(x - .025), 156), new Point(X(x), top), new Point(X(x + .025), 156),
                new Point(X(x), 312 - top), new Point(X(x - .025), 156)], i == 1 ? "TimeSamples" : "Feedback");
            peaks.Add(new Point(X(x), top));
        }
        Curve(dc, peaks, "Feedback");
        Curve(dc, peaks.Select(p => new Point(p.X, 312 - p.Y)), "Feedback");
        Label(dc, "Modulação temporal · excursão ≠ volume", 0, 224);
        Modulation(dc, "Rate", 0, 100, 310, "Depth");
        Label(dc, $"{T("Avanço nominal por bloco (tempo solicitado)")}: {V("Rate") / 10000 / Math.Max(2400, V("TimeSamples")):0.########}", 0, 374);
        double mix = V("Mix") / 100, dry = mix < .5 ? 1 : 2 - 2 * mix, wet = mix < .5 ? 2 * mix : 1;
        Label(dc, "Combinação · referência 50: ambos integrais", 0, 405);
        Curve(dc, [new Point(X(0), 475), new Point(X(.12), 475 - 26 * dry), new Point(X(.3), 475), new Point(X(.5), 480)], "Mix");
        Curve(dc, [new Point(X(0), 520), new Point(X(.12), 520 - 26 * wet), new Point(X(.3), 520), new Point(X(.5), 480)], "Mix");
        Envelope(dc, "Mix", .72, 486, .2);
        Line(dc, .5, 480, .52, 486, "Mix");
        Line(dc, .92, 486, 1, 486, "Mix");
        Label(dc, $"{T("Direto")} ×{dry:0.###}   {T("Eco")} ×{wet:0.###}", 0, 525);
    }

    private void DrawChorus(DrawingContext dc)
    {
        Label(dc, "Trajetória modulada · excursão temporal", 0, 44);
        Modulation(dc, "Speed", .384, 3.024, 130, "Depth");
        Label(dc, V("Waveform") == 0 ? "Original · forma ROM ilustrativa" : "Triangular", 0, 190);
        Label(dc, "Amplitude dos ramos de áudio", 0, 218);
        Envelope(dc, "WetGain", .24, 294);
        Label(dc, "Atrasado → ganho", .02, 300);
        Line(dc, .41, 294, .48, 372, "WetGain");
        Line(dc, .02, 344, .48, 372, "CombinedGain");
        Label(dc, "Entrada", .02, 326);
        Envelope(dc, "CombinedGain", .67, 372);
        Label(dc, "(entrada + atraso) / 2 → ganho", .42, 380);
        Line(dc, .84, 372, .8, 418, "Mix");
        Envelope(dc, "Mix", .8, 466, .16);
        double mix = V("Mix") / 100 * .85;
        Line(dc, .02, 440, .64, 466, "Mix");
        Label(dc, $"{T("Entrada")} ×{1 - mix:0.###}", .02, 416);
        Label(dc, $"{T("Processado")} ×{mix:0.###}", .57, 478);
        Curve(dc, [new Point(X(.5), 372), new Point(X(.5), 558), new Point(X(.08), 558), new Point(X(.08), 240)], "Feedback");
        Envelope(dc, "Feedback", .35, 558, .12);
        Label(dc, "↖ retorno à entrada do buffer", .05, 578);
        Label(dc, "100% = contribuição original; processado contém direto", 0, 606);
    }

    private void DrawPhaser(DrawingContext dc)
    {
        Label(dc, "Região de varredura · Hz estimados · escala logarítmica", 0, 44);
        double left = nodes["Lower"].Anchor().X, right = nodes["Upper"].Anchor().X;
        dc.PushOpacity(.1); dc.DrawRectangle(Brush("CubeFocus"), null, new Rect(left, 80, Math.Max(1, right - left), 98)); dc.Pop();
        dc.DrawLine(Pen("Lower"), new Point(left, 80), new Point(left, 178));
        dc.DrawLine(Pen("Upper"), new Point(right, 80), new Point(right, 178));
        Curve(dc, Enumerable.Range(0, 101).Select(i => new Point(X(i / 100d),
            105 + 50 * Math.Pow(Math.Sin(i / 100d * Math.PI * 3), 8))), "Lower");
        Label(dc, "300", .02, 188); Label(dc, "3.000", .86, 188);
        Label(dc, "Ciclo temporal da varredura", 0, 244);
        Modulation(dc, "Speed", .48, 6.24, 305);
        Label(dc, "Cadeia de seções", 0, 390); Label(dc, "Saída", .63, 390);
        for (int i = 0; i < (int)V("Sections"); i++)
        {
            double x = X(.18) + i * 30;
            dc.DrawRoundedRectangle(null, Pen("Sections"), new Rect(x - 12, 424, 24, 32), 3, 3);
        }
        Envelope(dc, "Level", .78, 460, .17);
        Label(dc, "Menu na cadeia: 1–3 seções · vales ilustrativos", 0, 493);
    }

    private static double PersistenceY(double value, double position) => 465 - 78 * Math.Exp(-position * (5 - 3.8 * value / 15));

    private void DrawReverb(DrawingContext dc)
    {
        Label(dc, "Espaço ilustrativo · sem dimensões acústicas", 0, 44);
        Curve(dc, [new Point(X(.05), 104), new Point(X(.68), 104), new Point(X(.94), 180), new Point(X(.94), 290),
            new Point(X(.27), 290), new Point(X(.05), 210), new Point(X(.05), 104)], "");
        Line(dc, .05, 210, .68, 210, faint: true); Line(dc, .68, 104, .68, 210, faint: true); Line(dc, .68, 210, .94, 290, faint: true);
        Envelope(dc, "Dry", .3, 160, .2);
        Label(dc, "Direto", .08, 168);
        for (int i = 0; i < 6; i++)
            Curve(dc, [new Point(X(.12), 200), new Point(X(.25 + .075 * i), 220 + i * 9), new Point(X(.4), 360)], "ReturnLevel");
        Envelope(dc, "Wet", .82, 260, .12);
        Line(dc, .5, 160, 1, 180, "Dry");
        Line(dc, .94, 260, 1, 180, "Wet");
        Label(dc, "Saída", .87, 160);
        Label(dc, "Reverberado", .7, 282);
        Envelope(dc, "ReturnLevel", .4, 360, .17);
        Line(dc, .57, 360, .7, 260, "ReturnLevel");
        Label(dc, "Retorno comum → difusão / filtros → saída", .02, 366);
        Curve(dc, Enumerable.Range(0, 61).Select(i =>
        {
            double x = i / 60d;
            // Qualitative envelope only; the scale selects firmware coefficient vectors.
            return new Point(X(x), PersistenceY(V("Decay"), x));
        }), "Decay");
        Label(dc, "Persistência · escala 1–15", 0, 470);
        Point damping = nodes["Damping"].Anchor();
        Curve(dc, [new Point(X(.08), 508), new Point(X(.5), 508), damping, new Point(X(.94), damping.Y + 10),
            new Point(X(.94), 570), new Point(X(.08), 570)], "Damping", true);
        Label(dc, "Amortecimento · 10 = coeficientes originais", 0, 575);
    }

    private sealed class GraphDrawing(EffectGraphEditor owner) : FrameworkElement
    {
        protected override void OnRender(DrawingContext drawingContext) => owner.Draw(drawingContext);
    }

    internal sealed class GraphNode : Control
    {
        internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(GraphNode),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) =>
            {
                var node = (GraphNode)d;
                node.owner.RefreshGraph();
                if (UIElementAutomationPeer.FromElement(node) is GraphNodePeer peer)
                    peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
            }));
        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        internal string Key { get; }
        internal new string Name { get; }
        internal string Unit { get; }
        internal string Hint { get; }
        internal double Minimum { get; }
        internal double Maximum { get; }
        internal double Step { get; }
        internal Func<Point> Anchor { get; }
        private readonly Func<double, Vector, double> drag;
        private readonly EffectGraphEditor owner;
        private Point origin;
        private double original;
        private bool dragging;

        internal GraphNode(EffectGraphEditor owner, string key, string name, string unit, double min, double max, double step,
            Func<Point> anchor, Func<double, Vector, double> drag, string hint)
        {
            this.owner = owner; Key = key; Name = name; Unit = unit; Minimum = min; Maximum = max; Step = step;
            Anchor = anchor; this.drag = drag; Hint = hint;
            Width = Height = 36; Focusable = true; Cursor = Cursors.Hand;
            ToolTip = $"{T(name)} · {T(hint)}";
            AutomationProperties.SetName(this, $"{T(name)} ({T(unit)})");
            AutomationProperties.SetHelpText(this, T(hint));
            GotKeyboardFocus += (_, _) => owner.Select(this);
            // XAML can assign EffectKind before DataContext. Keep logarithmic geometry finite
            // while inherited bindings are still resolving; this never writes to a model.
            SetCurrentValue(ValueProperty, min);
        }
        internal void Change(double value)
        {
            if (!double.IsFinite(value)) return;
            value = Math.Clamp(value, Minimum, Maximum);
            if (Key is "TimeSamples" or "Sections" or "Waveform" or "Decay" or "Damping") value = Math.Round(value);
            if (owner.EffectKind == "Phaser")
            {
                if (Key == "Lower") value = Math.Min(value, owner.V("Upper"));
                if (Key == "Upper") value = Math.Max(value, owner.V("Lower"));
            }
            SetCurrentValue(ValueProperty, value);
            GetBindingExpression(ValueProperty)?.UpdateSource();
            // Read back rounding/clamping performed by the authoritative ViewModel.
            GetBindingExpression(ValueProperty)?.UpdateTarget();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e); Focus(); owner.Select(this); e.Handled = true;
            if (Key is "Waveform" or "Sections") { owner.OpenChoices(this); return; }
            original = Value; origin = e.GetPosition(owner); dragging = CaptureMouse();
            if (e.ClickCount == 2) { Finish(); owner.BeginNumber(); }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging) Change(drag(original, e.GetPosition(owner) - origin));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (dragging) Change(drag(original, e.GetPosition(owner) - origin));
            Finish(); e.Handled = true;
        }
        protected override void OnLostMouseCapture(MouseEventArgs e) { base.OnLostMouseCapture(e); if (dragging) Cancel(); }
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e) { base.OnMouseRightButtonUp(e); owner.OpenChoices(this); e.Handled = true; }
        protected override void OnKeyDown(KeyEventArgs e) { base.OnKeyDown(e); owner.NodeKey(this, e); }
        private void Finish() { dragging = false; if (IsMouseCaptured) ReleaseMouseCapture(); }
        internal void Cancel() { if (!dragging) return; dragging = false; Change(original); if (IsMouseCaptured) ReleaseMouseCapture(); }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 36, 36));
            bool active = owner.selected == this;
            dc.DrawEllipse(owner.Brush("CubeInset"), new Pen(owner.Brush(active ? "CubeFocus" : "CubeText"), active ? 2 : 1.5), new Point(18, 18), 5, 5);
            if (IsKeyboardFocused) dc.DrawRoundedRectangle(null, new Pen(owner.Brush("CubeFocus"), 1.5) { DashStyle = DashStyles.Dot }, new Rect(2, 2, 32, 32), 5, 5);
        }
        protected override AutomationPeer OnCreateAutomationPeer() => new GraphNodePeer(this);

        private sealed class GraphNodePeer(GraphNode node) : FrameworkElementAutomationPeer(node), IRangeValueProvider
        {
            protected override string GetClassNameCore() => "EffectGraphElement";
            protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;
            public override object? GetPattern(PatternInterface patternInterface)
                => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);
            public bool IsReadOnly => !node.IsEnabled;
            public double LargeChange => node.Step * 10;
            public double SmallChange => node.Step;
            public double Maximum => node.owner.EffectKind == "Phaser" && node.Key == "Lower" ? node.owner.V("Upper") : node.Maximum;
            public double Minimum => node.owner.EffectKind == "Phaser" && node.Key == "Upper" ? node.owner.V("Lower") : node.Minimum;
            public double Value => node.Value;
            public void SetValue(double value)
            {
                if (IsReadOnly) throw new ElementNotEnabledException();
                if (!double.IsFinite(value) || value < Minimum || value > Maximum) throw new ArgumentOutOfRangeException(nameof(value));
                node.Change(value);
            }
        }
    }
}
