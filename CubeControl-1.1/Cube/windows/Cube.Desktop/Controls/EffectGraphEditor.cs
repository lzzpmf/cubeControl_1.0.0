using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
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
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(nameof(Accent), typeof(Brush),
        typeof(EffectGraphEditor), new PropertyMetadata(Brushes.Gray, (d, _) => ((EffectGraphEditor)d).RefreshGraph()));
    public Brush Accent { get => (Brush)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }
    private Brush Signal => Cube.Desktop.Appearance.ThemeService.Instance.IsHighContrast ? Brush("CubeText") : Accent;
    private readonly Grid surface = new();
    private readonly Canvas overlay = new();
    private readonly GraphDrawing drawing;
    private readonly Dictionary<string, GraphNode> nodes = [];
    private readonly StackPanel inspector = new() { Margin = new Thickness(24, 12, 24, 0), MinHeight = 148 };
    private readonly WrapPanel choices = new() { MinHeight = 36 };
    private readonly TextBlock footer = new() { Margin = new Thickness(24, 8, 24, 12), TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock caption = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Wpf.Ui.Controls.Button number = new() { HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBlock numberText = new() { TextWrapping = TextWrapping.Wrap, MinHeight = 34, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBox entry = new() { Visibility = Visibility.Collapsed, MinWidth = 100 };
    private readonly TextBlock help = new() { TextWrapping = TextWrapping.Wrap, MinHeight = 44 };
    private GraphNode? selected;
    private readonly ComboBox parameterPicker = new() { MinHeight = 32, Margin = new Thickness(0, 0, 0, 8) };
    private readonly Slider adjustment = new() { MinHeight = 32, Margin = new Thickness(0, 4, 0, 4), IsMoveToPointEnabled = true };
    private bool updatingInspector;
    private double numericStart, resumeRate = 100;
    private bool editing;
    private double W => Math.Max(260, surface.ActualWidth - 48);
    private double DesignHeight => EffectKind == "Chorus" ? 550 : EffectKind == "Reverb" ? 500 : 520;
    private double GraphScaleY => surface.ActualHeight > 0 ? surface.ActualHeight / DesignHeight : 1;
    private Point GraphPosition(Point point) => new(point.X, point.Y / GraphScaleY);
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
        footer.SetResourceReference(StyleProperty, "CaptionText");
        caption.SetResourceReference(StyleProperty, "BodyStrongText");
        help.SetResourceReference(StyleProperty, "CaptionText");
        ClipToBounds = false;
        MinWidth = 308;
        SetResourceReference(BackgroundProperty, "CubeInset");
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        drawing = new GraphDrawing(this);
        surface.Children.Add(drawing);
        surface.Children.Add(overlay);
        surface.SizeChanged += (_, _) => RefreshGraph();
        SetRow(surface, 1); SetRow(footer, 2); SetColumnSpan(footer, 2);
        Children.Add(inspector); Children.Add(surface); Children.Add(footer);
        var heading = new WrapPanel();
        caption.Margin = new Thickness(0, 6, 12, 0);
        heading.Children.Add(caption);
        var valueRow = new Grid { MinHeight = 34 };
        valueRow.Children.Add(number); valueRow.Children.Add(entry);
        number.Content = numberText;
        heading.Children.Add(valueRow);
        inspector.Children.Add(parameterPicker); inspector.Children.Add(heading); inspector.Children.Add(adjustment); inspector.Children.Add(choices); inspector.Children.Add(help);
        parameterPicker.SelectionChanged += (_, _) =>
        {
            if (!updatingInspector && parameterPicker.SelectedItem is ComboBoxItem { Tag: GraphNode node }) Select(node);
        };
        adjustment.ValueChanged += (_, _) =>
        {
            if (!updatingInspector && IsEnabled) selected?.Change(adjustment.Value);
        };
        help.SetResourceReference(TextBlock.ForegroundProperty, "Muted");
        footer.SetResourceReference(TextBlock.ForegroundProperty, "Muted");
        choices.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && selected != null) { selected.Focus(); e.Handled = true; }
        };
        number.Click += (_, _) => BeginNumber();
        entry.PreviewKeyDown += NumberKey;
        entry.LostKeyboardFocus += (_, _) => { if (editing) EndNumber(true, false); };
        SizeChanged += (_, _) =>
        {
            bool wide = ActualWidth >= 520;
            ColumnDefinitions[0].Width = wide ? new GridLength(ActualWidth >= 720 ? 240 : 180) : new GridLength(1, GridUnitType.Star);
            ColumnDefinitions[1].Width = wide ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            RowDefinitions[0].Height = wide ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;
            RowDefinitions[1].Height = wide ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            SetRow(surface, wide ? 0 : 1); SetColumn(surface, wide ? 1 : 0);
            RefreshGraph();
        };
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
        DataContextChanged += (_, _) =>
        {
            CancelInteraction(); selected = null; choices.Children.Clear();
            if (nodes.Count > 0) Select(nodes.Values.First());
            else RefreshGraph();
        };
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
            FallbackValue = min
        });
        overlay.Children.Add(node);
        parameterPicker.Items.Add(new ComboBoxItem { Content = T(name), Tag = node });
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
        parameterPicker.Items.Clear();
        choices.Children.Clear();
        switch (EffectKind)
        {
            case "Delay":
                Add("TimeSamples", "Tempo entre ecos", "amostras internas", 2400, 16350, 1,
                    () => new Point(X(.12 + .08 + .16 * V("TimeSamples") / 16350), 96),
                    (start, d) => start + d.X * 16350 / (W * .16), "Arraste a primeira repetição. As demais mantêm o mesmo intervalo.");
                Add("Feedback", "Realimentação", "%", 0, 99.21875, .1,
                    () => new Point(X(.12 + 2 * (.08 + .16 * V("TimeSamples") / 16350)), 156 + 60 * V("Feedback") / 100),
                    (start, d) => start + d.Y * 100 / 60, "Mais feedback prolonga as repetições, sem mudar o tempo entre ecos.");
                Amplitude("Depth", "Profundidade", "% do original", 0, 100, 1, .23, 310,
                    "Profundidade da modulação do atraso; não altera amplitude de áudio.");
                Cycle("Rate", "Velocidade da modulação", "% do original", 0, 100, 310,
                    "Ciclo mais longo reduz a velocidade. Zero congela a modulação; use Retomar para ouvir o movimento.");
                Add("Mix", "Mistura", "%", 0, 92.1875, .1,
                    () => new Point(X(.72), 466 - 60 * V("Mix") / 92.1875),
                    (start, d) => start - d.Y * 92.1875 / 60, "Abaixo de 50, cresce o eco; acima, diminui o direto. Em 50, ambos ficam integrais.");
                break;
            case "Chorus":
                Amplitude("Depth", "Profundidade", "% do original", 0, 100, 1, .23, 150, "Expanda a crista para aumentar a profundidade, sem alterar ganhos.");
                Cycle("Speed", "Velocidade", "Hz estimados", .384, 3.024, 150, "Alongue o ciclo para reduzir a velocidade estimada.");
                Add("Waveform", "Forma de onda", "", 0, 1, 1,
                    () => new Point(X(.06), 150 - 48 * V("Depth") / 100 * ModulationWave((X(.06) - X(.23)) / CycleWidth("Speed", .384, 3.024))), (s, _) => s,
                    "Escolha Original ou Triangular. A forma Original é ilustrativa, não uma medição da ROM.");
                Amplitude("WetGain", "Ramo atrasado", "% do original", 50, 100, 1, .24, 274, "Amplitude do atraso antes da combinação com a entrada.");
                Amplitude("CombinedGain", "Ramo combinado", "% do original", 50, 100, 1, .67, 332, "Amplitude após combinar entrada e atraso; independente dos demais ganhos.");
                Amplitude("Mix", "Mistura", "% do original", 0, 100, 1, .8, 426,
                    "100% mantém a mistura original. O ramo processado contém som direto; não é wet puro.");
                Amplitude("Feedback", "Retorno", "%", 0, 10, .1, .35, 498,
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
                    "Escolha 1, 2 ou 3 seções. Os vales ilustram a varredura; não representam uma contagem de estágios.");
                Amplitude("Level", "Nível de saída", "% do original", 0, 100, 1, .78, 460, "Amplitude de saída independente dos vales. 100% conserva o nível original.");
                break;
            case "Reverb":
                Amplitude("Dry", "Direto", "%", 0, 100, 1, .3, 160, "Ganho direto independente; não reduz o reverberado.");
                Amplitude("Wet", "Reverberado", "%", 0, 90, 1, .82, 260, "Envelope global na saída, independente do ganho direto e do retorno das linhas.");
                Amplitude("ReturnLevel", "Retorno", "% do original", 0, 100, 1, .35, 340,
                    "Um ganho comum antes da saída. Zerar o retorno não elimina toda a contribuição processada.");
                Add("Decay", "Decaimento", "escala interna 1–15", 1, 15, 1,
                    () => new Point(X(.65), PersistenceY(V("Decay"), .65)),
                    (start, d) => 15 * (5 + Math.Log(Math.Clamp((465 - PersistenceY(start, .65) - d.Y) / 78, .001, 1)) / .65) / 3.8,
                    "Seleciona conjuntos de coeficientes. Persistência ilustrativa, sem segundos ou RT60.");
                Add("Damping", "Amortecimento", "escala interna 0–10; 10 = original", 0, 10, 1,
                    () => new Point(X(.58), 210 + 32 * V("Damping") / 10), (start, d) => start + d.Y * 10 / 32,
                    "Suaviza as reflexões. 10 mantém os coeficientes originais; a escala não representa frequência de corte.");
                break;
        }
        if (nodes.Count > 0) Select(nodes.Values.First());
        else RefreshGraph();
    }

    internal void Select(GraphNode node)
    {
        bool changed = selected != node;
        if (changed && editing)
        {
            EndNumber(true, false);
            if (editing) EndNumber(false, false);
        }
        selected = node;
        if (changed) CreateChoices(node);
        RefreshGraph();
    }

    internal void RefreshGraph()
    {
        if (drawing == null) return;
        double inspectorWidth = ActualWidth >= 520 ? ActualWidth >= 720 ? 192 : 132 : Math.Max(260, ActualWidth - 48);
        caption.MaxWidth = inspectorWidth;
        entry.MaxWidth = inspectorWidth;
        number.Width = Math.Min(260, inspectorWidth);
        inspector.MinHeight = selected == null ? 44 : W < 400 ? 172 : 148;
        choices.Visibility = choices.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        help.Visibility = selected == null ? Visibility.Collapsed : Visibility.Visible;
        foreach (var node in nodes.Values)
        {
            Point p = node.Anchor();
            Canvas.SetLeft(node, p.X - 18); Canvas.SetTop(node, p.Y * GraphScaleY - 18);
            AutomationProperties.SetName(node, $"{T(node.Name)} ({T(node.Unit)})");
            AutomationProperties.SetHelpText(node, T(node.Hint));
            node.InvalidateVisual();
        }
        if (selected != null)
        {
            updatingInspector = true;
            try
            {
                foreach (ComboBoxItem item in parameterPicker.Items)
                {
                    var parameter = (GraphNode)item.Tag;
                    item.Content = T(parameter.Name);
                    if (parameter == selected) parameterPicker.SelectedItem = item;
                }
                adjustment.Minimum = EffectKind == "Phaser" && selected.Key == "Upper" ? V("Lower") : selected.Minimum;
                adjustment.Maximum = EffectKind == "Phaser" && selected.Key == "Lower" ? V("Upper") : selected.Maximum;
                adjustment.SmallChange = selected.Step;
                adjustment.LargeChange = selected.Step * 10;
                adjustment.TickFrequency = selected.Step;
                adjustment.IsSnapToTickEnabled = selected.IsDiscrete;
                adjustment.Value = selected.Value;
                adjustment.Visibility = selected.Key is "Waveform" or "Sections" ? Visibility.Collapsed : Visibility.Visible;
                AutomationProperties.SetName(adjustment, T(selected.Name));
                AutomationProperties.SetName(parameterPicker, T("Selecione um ponto para ajustar"));
            }
            finally { updatingInspector = false; }
            caption.Text = T(selected.Name);
            string valueText = selected.Key == "Waveform" ? T(selected.Value == 0 ? "Original" : "Triangular")
                : $"{selected.Value.ToString("0.#####", CultureInfo.CurrentCulture)} {T(selected.Unit)}";
            numberText.Text = valueText;
            numberText.MaxWidth = number.Width - 16;
            AutomationProperties.SetName(number, $"{T(selected.Name)}: {valueText}. {T("Editar valor")}");
            if (!editing) help.Text = T(selected.Hint);
            number.Visibility = editing || selected.Key is "Waveform" or "Sections" ? Visibility.Collapsed : Visibility.Visible;
            foreach (Wpf.Ui.Controls.Button option in choices.Children)
            {
                bool active = option.Tag is int value && selected.Value == value;
                string label = option.Tag is int choice ? selected.Key == "Waveform" ? choice == 0 ? "Original" : "Triangular" : choice.ToString()
                    : selected.Value == 0 ? "Retomar modulação" : "Congelar fase";
                var choiceLabel = new TextBlock { Text = T(label), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
                var choiceMarker = new Border { Width = 3, Height = 16, CornerRadius = new CornerRadius(2), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Visibility = active ? Visibility.Visible : Visibility.Collapsed, IsHitTestVisible = false };
                choiceMarker.SetResourceReference(Border.BackgroundProperty, active ? "CubeSelectionIndicator" : "CubeFocus");
                // Keep a single cue when the selected choice also has keyboard focus.
                var markerStyle = new Style(typeof(Border));
                markerStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 16.0));
                markerStyle.Setters.Add(new Setter(UIElement.VisibilityProperty, active ? Visibility.Visible : Visibility.Collapsed));
                var markerFocus = new DataTrigger { Binding = new Binding(nameof(IsKeyboardFocused)) { Source = option }, Value = true };
                markerFocus.Setters.Add(new Setter(FrameworkElement.HeightProperty, 24.0));
                markerFocus.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible));
                markerStyle.Triggers.Add(markerFocus);
                choiceMarker.ClearValue(FrameworkElement.HeightProperty);
                choiceMarker.ClearValue(UIElement.VisibilityProperty);
                choiceMarker.Style = markerStyle;
                option.FocusVisualStyle = null;
                option.SetResourceReference(Control.ForegroundProperty, active ? "CubeSelectedText" : "CubeText");
                var choiceContent = new Grid();
                choiceContent.Children.Add(choiceLabel); choiceContent.Children.Add(choiceMarker);
                option.Content = choiceContent;
                option.FontWeight = active ? FontWeights.Bold : FontWeights.Normal;
                option.Background = active ? Brush("CubeSelected") : Brushes.Transparent;
                option.BorderThickness = new Thickness(0);
                AutomationProperties.SetName(option, $"{T(selected.Name)}: {T(label)}");
                AutomationProperties.SetItemStatus(option, active ? T("Selecionado") : "");
            }
        }
        else
        {
            adjustment.Visibility = Visibility.Collapsed;
            caption.Text = T("Selecione um ponto para ajustar"); help.Text = "";
            number.Visibility = Visibility.Collapsed;
        }
        footer.Text = T("Tab seleciona · setas ajustam · Enter digita") + "\n" + T("Visualização ilustrativa");
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
        // Enter, Tab and clicking away commit; Escape explicitly cancels.
        editing = false; entry.Visibility = Visibility.Collapsed; number.Visibility = Visibility.Visible;
        if (restoreFocus) selected.Focus();
        RefreshGraph();
    }

    private void CancelInteraction()
    {
        foreach (var node in nodes.Values) node.Cancel();
        if (editing) EndNumber(false, false);
    }

    private void CreateChoices(GraphNode node)
    {
        choices.Children.Clear();
        void Option(string label, int? value, Action action)
        {
            var button = new Wpf.Ui.Controls.Button
            {
                Content = T(label), Tag = value,
                Margin = new Thickness(0, 2, 8, 2)
            };
            button.Click += (_, _) => { if (IsEnabled) action(); };
            choices.Children.Add(button);
        }
        if (node.Key == "Waveform")
        {
            Option("Original", 0, () => node.Change(0));
            Option("Triangular", 1, () => node.Change(1));
        }
        else if (node.Key == "Sections")
        {
            for (int i = 1; i <= 3; i++)
            {
                int value = i;
                Option(i.ToString(), value, () => node.Change(value));
            }
        }
        else if (EffectKind == "Delay" && node.Key == "Rate")
            Option("Congelar fase", null, () =>
            {
                if (node.Value > 0) { resumeRate = node.Value; node.Change(0); }
                else node.Change(resumeRate);
            });
    }

    internal void OpenChoices(GraphNode node)
    {
        Select(node);
        if (choices.Children.Count > 0) ((Control)choices.Children[0]).Focus();
        else BeginNumber();
    }

    internal void NodeKey(GraphNode node, KeyEventArgs e)
    {
        Select(node);
        if (e.Key == Key.Escape) { node.Cancel(); e.Handled = true; }
        else if (e.Key == Key.Enter) { BeginNumber(); e.Handled = true; }
        else if (e.Key == Key.Space || e.Key == Key.Apps) { OpenChoices(node); e.Handled = true; }
        else if (e.Key is Key.Up or Key.Right or Key.Down or Key.Left)
        {
            double direction = e.Key is Key.Up or Key.Right ? 1 : -1;
            double step = node.Step * (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !node.IsDiscrete ? .1 : 1);
            node.Change(node.Value + direction * step); e.Handled = true;
        }
    }

    private void Draw(DrawingContext dc)
    {
        if (nodes.Count == 0) return;
        dc.PushOpacity(IsEnabled ? 1 : .55);
        dc.PushTransform(new ScaleTransform(1, GraphScaleY));
        switch (EffectKind)
        {
            case "Delay": DrawDelay(dc); break;
            case "Chorus": DrawChorus(dc); break;
            case "Phaser": DrawPhaser(dc); break;
            case "Reverb": DrawReverb(dc); break;
        }
        dc.Pop();
        dc.Pop();
    }

    private Pen Pen(string key = "", bool faint = false)
    {
        bool direct = key is "Original" or "Dry";
        return new Pen(faint ? Brush("CubeBorder") : direct || key.Length == 0 ? Brush("Muted") : Signal,
            selected?.Key == key ? 2.5 : faint ? 1 : 1.5) { DashStyle = direct ? DashStyles.Dash : DashStyles.Solid };
    }
    private void Line(DrawingContext dc, double x1, double y1, double x2, double y2, string key = "", bool faint = false)
        => dc.DrawLine(Pen(key, faint), new Point(X(x1), y1), new Point(X(x2), y2));
    private void Label(DrawingContext dc, string text, double x, double y, double width = 1)
    {
        var formatted = new FormattedText(T(text), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            12, Brush("Muted"), VisualTreeHelper.GetDpi(this).PixelsPerDip)
        { MaxTextWidth = Math.Max(10, W * Math.Min(width, 1 - x)), Trimming = TextTrimming.CharacterEllipsis };
        dc.PushTransform(new ScaleTransform(1, 1 / GraphScaleY));
        dc.DrawText(formatted, new Point(X(x), y * GraphScaleY));
        dc.Pop();
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
        if (fill) { dc.PushOpacity(.1); dc.DrawGeometry(Signal, null, geometry); dc.Pop(); }
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
        Label(dc, frozen ? "Modulação congelada" : "Ciclo mais longo, mais lento", .05, baseline + 52);
    }
    private double ModulationWave(double phase) => EffectKind == "Chorus" && V("Waveform") == 1
        ? 1 - 4 * Math.Abs(phase - Math.Floor(phase + .5)) : Math.Cos(phase * Math.PI * 2);

    private void DrawDelay(DrawingContext dc)
    {
        Label(dc, "Original e ecos", 0, 12);
        double interval = .08 + .16 * V("TimeSamples") / 16350;
        double mix = V("Mix") / 100, dry = mix < .5 ? 1 : 2 - 2 * mix, wet = mix < .5 ? 2 * mix : 1;
        Line(dc, 0, 156, 1, 156, faint: true);
        Line(dc, .12, 156, .12, 156 - 68 * dry, "Original");
        Label(dc, "Direto", .01, 52);
        Line(dc, .12, 76, .12 + interval, 76, "TimeSamples");
        Line(dc, .12, 71, .12, 81, "TimeSamples");
        Line(dc, .12 + interval, 71, .12 + interval, 81, "TimeSamples");
        var peaks = new List<Point>();
        for (int i = 1; i <= 3; i++)
        {
            double x = .12 + i * interval;
            double top = 156 - 60 * wet * Math.Pow(V("Feedback") / 100, i - 1);
            Curve(dc, [new Point(X(x - .025), 156), new Point(X(x), top), new Point(X(x + .025), 156)], "Mix");
            Label(dc, W < 420 ? i.ToString() : $"{T("Eco")} {i}", x - .015, 52);
            peaks.Add(new Point(X(x), top));
            double excursion = Math.Min(interval * .3, .025) * V("Depth") / 100;
            Line(dc, x - excursion, 170, x + excursion, 170, "Depth");
            Line(dc, x - excursion, 167, x - excursion, 173, "Depth");
            Line(dc, x + excursion, 167, x + excursion, 173, "Depth");
        }
        Curve(dc, peaks, "Feedback");
        Point first = nodes["TimeSamples"].Anchor();
        dc.DrawLine(Pen("TimeSamples"), first, new Point(first.X, 156 - 60 * wet));
        Point feedback = nodes["Feedback"].Anchor();
        Curve(dc, [new Point(first.X, 190), feedback, new Point(X(.12 + 3 * interval), 190)], "Feedback");
        Label(dc, "Profundidade da modulação", 0, 236);
        Modulation(dc, "Rate", 0, 100, 310, "Depth");
        Label(dc, "Mistura", 0, 390);
        Line(dc, .08, 444, .08 + .28 * dry, 444, "Original");
        Line(dc, .08, 480, .08 + .28 * wet, 480, "Mix");
        Label(dc, "Direto", .08, 424); Label(dc, "Ecos", .08, 486);
        Envelope(dc, "Mix", .72, 466, .16);
    }

    private void DrawChorus(DrawingContext dc)
    {
        Label(dc, "Original", 0, 8, .43);
        Label(dc, "Cópia modulada", .45, 8);
        double length = CycleWidth("Speed", .384, 3.024);
        Curve(dc, Enumerable.Range(0, 121).Select(i =>
        {
            double x = i / 120d;
            return new Point(X(x), 54 - 18 * Math.Sin(x * Math.PI * 8));
        }), "Original");
        Curve(dc, Enumerable.Range(0, 121).Select(i =>
        {
            double x = i / 120d;
            double delay = .035 + .02 * V("Depth") / 100 * ModulationWave(W * x / length);
            return new Point(X(x), 54 - 18 * V("WetGain") / 100 * Math.Sin((x - delay) * Math.PI * 8));
        }), "WetGain");
        Label(dc, "Modulação do atraso", 0, 108);
        Modulation(dc, "Speed", .384, 3.024, 150, "Depth");
        Envelope(dc, "WetGain", .24, 274);
        Label(dc, "Cópia", .12, 298, .28);
        Line(dc, .41, 274, .5, 332, "WetGain");
        Line(dc, .02, 336, .5, 332, "Original");
        Label(dc, "Original", .02, 316, .35);
        Envelope(dc, "CombinedGain", .67, 332);
        Label(dc, "Combinação", .5, 364);
        Line(dc, .84, 332, .8, 378, "CombinedGain");
        Envelope(dc, "Mix", .8, 426, .16);
        Line(dc, .02, 410, .64, 426, "Original");
        Line(dc, .96, 426, 1, 426, "Mix");
        Label(dc, "Mistura", .65, 456);
        double mix = V("Mix") / 100 * .85, combined = V("CombinedGain") / 100;
        double direct = 1 - mix + mix * combined / 2, delayed = mix * combined * V("WetGain") / 200;
        Label(dc, $"{T("Direto")} ×{direct:0.###}   {T("Cópia")} ×{delayed:0.###}", 0, 438);
        Curve(dc, [new Point(X(.86), 332), new Point(X(.98), 332), new Point(X(.98), 528),
            new Point(X(.04), 528), new Point(X(.04), 274), new Point(X(.07), 274)], "Feedback");
        Envelope(dc, "Feedback", .35, 498, .12);
        Line(dc, .23, 498, .12, 528, "Feedback");
        Line(dc, .47, 498, .7, 528, "Feedback");
        Label(dc, "Retorno", .5, 474);
    }

    private void DrawPhaser(DrawingContext dc)
    {
        Label(dc, "Região de varredura", 0, 12);
        double left = nodes["Lower"].Anchor().X, right = nodes["Upper"].Anchor().X;
        dc.PushOpacity(.12); dc.DrawRectangle(Signal, null, new Rect(left, 80, Math.Max(1, right - left), 98)); dc.Pop();
        Line(dc, .08, 105, .92, 105, "Original");
        dc.DrawLine(Pen("Lower"), new Point(left, 80), new Point(left, 178));
        dc.DrawLine(Pen("Upper"), new Point(right, 80), new Point(right, 178));
        Curve(dc, Enumerable.Range(0, 101).Select(i =>
        {
            double x = X(.08 + .84 * i / 100d);
            double phase = (x - left) / Math.Max(1, right - left);
            double dip = x >= left && x <= right ? 50 * Math.Pow(Math.Sin(phase * Math.PI * 3), 8) : 0;
            return new Point(x, 105 + dip);
        }), "Sweep");
        if (right - left >= 24)
        {
            dc.DrawLine(Pen("Speed"), new Point(left, 60), new Point(right, 60));
            foreach (var (tip, sign) in new[] { (left, 1), (right, -1) })
            {
                dc.DrawLine(Pen("Speed"), new Point(tip, 60), new Point(tip + 6 * sign, 55));
                dc.DrawLine(Pen("Speed"), new Point(tip, 60), new Point(tip + 6 * sign, 65));
            }
        }
        Label(dc, "300 Hz", .02, 188); Label(dc, "3.000 Hz", .77, 188);
        Label(dc, "Velocidade da varredura", 0, 244);
        Modulation(dc, "Speed", .48, 6.24, 305);
        Label(dc, "Seções", .06, 390); Label(dc, "Saída", .63, 390);
        for (int i = 0; i < (int)V("Sections"); i++)
        {
            double x = X(.18) + i * 30;
            dc.DrawRoundedRectangle(null, Pen("Sections"), new Rect(x - 12, 424, 24, 32), 3, 3);
        }
        Envelope(dc, "Level", .78, 460, .17);
    }

    private static double PersistenceY(double value, double position) => 465 - 78 * Math.Exp(-position * (5 - 3.8 * value / 15));

    private void DrawReverb(DrawingContext dc)
    {
        Label(dc, "Fonte e espaço", 0, 12);
        dc.DrawRoundedRectangle(null, Pen("", true), new Rect(X(.02), 70, W * .96, 288), 12, 12);
        Point source = new(X(.08), 180), output = new(X(.94), 180);
        dc.DrawEllipse(Brush("CubeText"), null, source, 4, 4);
        dc.DrawEllipse(null, Pen("Wet"), output, 6, 6);
        Label(dc, "Fonte", .03, 190); Label(dc, "Saída", .8, 92);
        Envelope(dc, "Dry", .3, 160, .13);
        dc.DrawLine(Pen("Dry"), source, new Point(X(.17), 160));
        dc.DrawLine(Pen("Dry"), new Point(X(.43), 160), output);
        Label(dc, "Direto", .2, 90);
        for (int i = 0; i < 3; i++)
            Curve(dc, [source, new Point(X(.18 + .075 * i), 224 + i * 32),
                new Point(X(.5), 260)], "Reflections");
        Label(dc, "Reflexões", .1, 264, .32);
        Point damping = nodes["Damping"].Anchor();
        var field = new Rect(X(.48), damping.Y, W * .25, 310 - damping.Y);
        dc.PushOpacity(.12); dc.DrawRoundedRectangle(Signal, null, field, 12, 12); dc.Pop();
        Curve(dc, Enumerable.Range(0, 41).Select(i =>
        {
            double x = .48 + .25 * i / 40;
            return new Point(X(x), damping.Y - (i == 16 ? 0 : 3 * Math.Sin(i * Math.PI / 8)));
        }), "Damping");
        Envelope(dc, "Wet", .82, 260, .08);
        Line(dc, .73, 260, .74, 260, "Wet");
        dc.DrawLine(Pen("Wet"), new Point(X(.9), 260), output);
        Label(dc, "Reverberado", .55, 314);
        Envelope(dc, "ReturnLevel", .35, 340, .12);
        Line(dc, .47, 340, .55, 310, "ReturnLevel");
        Label(dc, "Retorno", .18, 364);
        Curve(dc, Enumerable.Range(0, 61).Select(i =>
        {
            double x = i / 60d;
            // Qualitative envelope only; the scale selects firmware coefficient vectors.
            return new Point(X(x), PersistenceY(V("Decay"), x));
        }), "Decay");
        Line(dc, 0, 465, 1, 465, faint: true);
        Label(dc, "Decaimento", 0, 476);
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
                if (!node.changing) node.owner.RefreshGraph();
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
        internal bool IsDiscrete => Key is "TimeSamples" or "Sections" or "Waveform" or "Decay" or "Damping";
        internal Func<Point> Anchor { get; }
        private readonly Func<double, Vector, double> drag;
        private readonly EffectGraphEditor owner;
        private Point origin;
        private double original;
        private bool dragging;
        private bool changing;

        internal GraphNode(EffectGraphEditor owner, string key, string name, string unit, double min, double max, double step,
            Func<Point> anchor, Func<double, Vector, double> drag, string hint)
        {
            this.owner = owner; Key = key; Name = name; Unit = unit; Minimum = min; Maximum = max; Step = step;
            Anchor = anchor; this.drag = drag; Hint = hint;
            Width = Height = 36; Focusable = true; Cursor = Cursors.Hand;
            AutomationProperties.SetName(this, $"{T(name)} ({T(unit)})");
            AutomationProperties.SetHelpText(this, T(hint));
            GotKeyboardFocus += (_, _) => owner.Select(this);
            MouseEnter += (_, _) => InvalidateVisual();
            MouseLeave += (_, _) => InvalidateVisual();
            LostKeyboardFocus += (_, _) => InvalidateVisual();
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
            changing = true;
            try
            {
                SetCurrentValue(ValueProperty, value);
                GetBindingExpression(ValueProperty)?.UpdateSource();
                // Finish the source update before synchronizing the inspector controls.
                GetBindingExpression(ValueProperty)?.UpdateTarget();
            }
            finally { changing = false; }
            owner.RefreshGraph();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e); Focus(); owner.Select(this); e.Handled = true;
            if (Key is "Waveform" or "Sections") { owner.OpenChoices(this); return; }
            original = Value; origin = owner.GraphPosition(e.GetPosition(owner.surface)); dragging = CaptureMouse();
            if (e.ClickCount == 2) { Finish(); owner.BeginNumber(); }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging) Change(drag(original, owner.GraphPosition(e.GetPosition(owner.surface)) - origin));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (dragging) Change(drag(original, owner.GraphPosition(e.GetPosition(owner.surface)) - origin));
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
            dc.DrawEllipse(active ? owner.Signal : owner.Brush("CubeInset"), new Pen(owner.Signal, active || IsMouseOver ? 2.5 : 1.5), new Point(18, 18), IsMouseOver ? 7 : 5, IsMouseOver ? 7 : 5);
            if (active) dc.DrawEllipse(null, new Pen(owner.Signal, 1), new Point(18, 18), 9, 9);
            if (IsKeyboardFocused) dc.DrawRoundedRectangle(owner.Brush("CubeFocus"), null, new Rect(0, 6, 3, 24), 2, 2);
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
