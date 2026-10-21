using Cube.WinUI.Controls;
using Cube.WinUI.Appearance;
using Cube.Desktop.Appearance;
using Cube.Desktop.ViewModels;
using Cube.WinUI.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using Cube.Desktop.Localization;

namespace Cube.WinUI;
public sealed partial class WorkspacePage : Page
{
    private Workspace workspace = null!;
    private EditorViewModel E => workspace.Editor;
    private BackingTracksViewModel B => workspace.Backing;
    private static string T(string text) => LanguageService.T(text);
    private readonly List<Action> detach = [];
    public WorkspacePage() { InitializeComponent(); }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Trace("Page navigated");
        var (area, state) = ((string, Workspace))e.Parameter;
        workspace = state; B.PageVisible = area == "backing";
        Heading.Text = area switch { "ir" => "IRs", "backing" => "Backing tracks", "amps" => "Criador de amps", "device" => "Dispositivo e ajuda", _ => "Pedaleira" };
        Heading.Text = T(Heading.Text);
        if (area == "backing")
        {
            var status = Text(B, "Status");
            WorkbenchPanel.SetFullWidth(status, true); Sections.Children.Add(status);
        }
        switch (area) { case "backing": BuildBacking(); break; case "ir": BuildIrs(); break; case "amps": BuildAmps(); break; case "device": BuildDevice(); break; default: BuildPedal(); break; }
        if (area == "backing")
        {
            string[] order = ["Linha do tempo", "Pistas e gravação", "Dispositivos e mixagem", "Sessões", "Transporte", "Trecho e estudo", "Metrônomo", "Clipes", "Biblioteca local", "YouTube", "Exportação WAV"];
            var children = Sections.Children.OrderBy(child => child is FrameworkElement { Tag: string title } ? Array.IndexOf(order, title) : -1).ToArray();
            Sections.Children.Clear(); foreach (var child in children) Sections.Children.Add(child);
        }
    }
    protected override void OnNavigatedFrom(NavigationEventArgs e) => CancelPending();
    internal void CancelPending()
    {
        void EndGestures(DependencyObject parent)
        {
            if (parent is InstrumentControl instrument) { instrument.CancelGesture(); return; }
            for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
                EndGestures(Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i));
        }
        EndGestures(this);
        foreach (var action in detach) action(); detach.Clear();
    }
    private void Observe(INotifyPropertyChanged model, string property, Action update)
    {
        PropertyChangedEventHandler handler = (_, e) => { if (e.PropertyName == property || string.IsNullOrEmpty(e.PropertyName)) update(); };
        model.PropertyChanged += handler; detach.Add(() => model.PropertyChanged -= handler);
    }
    private static void Bind(FrameworkElement control, DependencyProperty property, object source, string path, bool edit = false)
    {
        App.Trace("Bind " + source.GetType().Name + "." + path);
        // Fail visibly during route tests rather than silently shipping an inert control.
        var type = source.GetType();
        foreach (string segment in path.Split('.'))
            type = (type.GetProperty(segment) ?? throw new InvalidOperationException($"Binding inexistente: {type.Name}.{segment}")).PropertyType;
        control.SetBinding(property, new Binding { Source = source, Path = new PropertyPath(path), Mode = edit ? BindingMode.TwoWay : BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Converter = property == TextBlock.TextProperty ? new LocalizedText() : property == NumberBox.ValueProperty ? new FiniteNumber() : null });
    }
    private TextBlock Text(object source, string path)
    {
        var text = new TextBlock { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
        Bind(text, TextBlock.TextProperty, source, path);
        if (path is "Status" or "ErrorMessage" or "SaveState" or "RecordingState" or "MetronomeStatus")
        {
            void ShowMessage() => text.Visibility = string.IsNullOrWhiteSpace(text.Text) ? Visibility.Collapsed : Visibility.Visible;
            text.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => ShowMessage()); ShowMessage();
            AutomationProperties.SetLiveSetting(text, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
            if (source is INotifyPropertyChanged model) Observe(model, path, () => text.DispatcherQueue.TryEnqueue(() =>
            {
                if (text.IsLoaded) Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer.FromElement(text)?.RaiseAutomationEvent(Microsoft.UI.Xaml.Automation.Peers.AutomationEvents.LiveRegionChanged);
            }));
        }
        return text;
    }
    private static TextBox Edit(object source, string path, string label)
    { var box = new TextBox { Header = T(label) }; Bind(box, TextBox.TextProperty, source, path, true); AutomationProperties.SetName(box, T(label)); return box; }
    private static NumberBox Number(object source, string path, string label, double min, double max, double step = 1)
    { var box = new NumberBox { Header = T(label), Minimum = min, Maximum = max, SmallChange = step, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact }; Bind(box, NumberBox.ValueProperty, source, path, true); AutomationProperties.SetName(box, T(label)); return box; }
    private InstrumentField Instrument(object source, string path, string label, double min, double max, double step = 1, bool fader = false, Action? restore = null)
    {
        var field = new InstrumentField(label, min, max, step, fader);
        Bind(field.Editor, NumberBox.ValueProperty, source, path, true);
        bool backing = source == B || source is BackingStem;
        if (restore == null) _ = workspace.Reference(source, path);
        field.Instrument.RestoreRequested += (_, _) =>
        {
            if (restore != null) restore();
            else field.Editor.Value = backing ? path switch { "MasterDb" or "ClickDb" or "GainDb" or "Pan" or "Pitch" => 0, "Volume" or "Speed" => 100, _ => workspace.Reference(source, path) } : workspace.Reference(source, path);
        };
        field.Instrument.GestureStarted += (_, _) => { if (backing) B.BeginParameterGesture(); else E.BeginGesture(field); };
        field.Instrument.GestureEnded += (_, _) => { if (backing) B.EndParameterGesture(); else E.EndGesture(field); };
        return field;
    }
    private static WorkbenchPanel Rack(params UIElement[] fields)
    {
        var rack = new WorkbenchPanel { CellWidth = 145, MaximumColumns = 8, Gap = 8 };
        foreach (var field in fields) rack.Children.Add(field);
        return rack;
    }
    private static ToggleButton Bypass(object source, string path, string label)
    {
        var toggle = new FootswitchButton(label);
        Bind(toggle, ToggleButton.IsCheckedProperty, source, path, true);
        return toggle;
    }
    private static CheckBox Check(object source, string path, string label)
    { var box = new CheckBox { Content = T(label) }; Bind(box, ToggleButton.IsCheckedProperty, source, path, true); return box; }
    private static Views.LibraryBrowser Library(object source, string items, string selected, string label, string display)
    {
        var browser = new Views.LibraryBrowser(label); browser.List.DisplayMemberPath = display;
        Bind(browser.List, ItemsControl.ItemsSourceProperty, source, items); Bind(browser.List, Selector.SelectedItemProperty, source, selected, true);
        return browser;
    }
    private static ComboBox Choice(object source, string items, string selected, string label, string display = "", bool index = false)
    {
        var box = new ComboBox { Header = T(label), HorizontalAlignment = HorizontalAlignment.Stretch, DisplayMemberPath = display };
        Bind(box, ItemsControl.ItemsSourceProperty, source, items); Bind(box, index ? Selector.SelectedIndexProperty : Selector.SelectedItemProperty, source, selected, true);
        AutomationProperties.SetName(box, T(label)); return box;
    }
    private static Button Command(object source, string path, string label)
    { var button = new Button { Content = new TextBlock { Text = T(label), TextWrapping = TextWrapping.Wrap } }; StudioMotion.Attach(button); AutomationProperties.SetName(button, T(label)); Bind(button, ButtonBase.CommandProperty, source, path); AutomationProperties.SetAutomationId(button, path); return button; }
    private static Button Action(string label, Func<Task> action, object? source = null, string? enabled = null)
    {
        var button = new Button { Content = new TextBlock { Text = T(label), TextWrapping = TextWrapping.Wrap } }; StudioMotion.Attach(button); AutomationProperties.SetName(button, T(label));
        if (source != null && enabled != null) Bind(button, IsEnabledProperty, source, enabled);
        button.Click += async (_, _) => { try { await action(); } catch (Exception ex) { await Dialogs.Show(new ContentDialog { Title = "Não foi possível concluir", Content = ex.Message, CloseButtonText = "Fechar" }); } };
        return button;
    }
    private static Button Action(string label, Action action) => Action(label, () => { action(); return Task.CompletedTask; });
    private sealed class SectionContent : ContentControl
    {
        private readonly StackPanel panel = new() { Spacing = 12 };
        public UIElementCollection Children => panel.Children;
        public SectionContent() { Content = panel; HorizontalContentAlignment = HorizontalAlignment.Stretch; IsTabStop = false; }
    }
    private SectionContent Section(string title, params UIElement[] children)
    {
        var panel = new SectionContent(); panel.Loaded += (_, _) => DescribeControls(panel, T(title)); foreach (var child in children) panel.Children.Add(child);
        var content = new StackPanel { Spacing = 14 };
        var heading = new TextBlock { Text = T(title), FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
        AutomationProperties.SetHeadingLevel(heading, Microsoft.UI.Xaml.Automation.Peers.AutomationHeadingLevel.Level2);
        content.Children.Add(heading); content.Children.Add(panel);
        var frame = new Border { Style = (Style)Application.Current.Resources["StudioPanelStyle"], Child = content, VerticalAlignment = VerticalAlignment.Top, Tag = title };
        if (title is "Cadeia de efeitos" or "Linha do tempo" or "Controles principais" or "Equalização A" or "Equalização B" or "Amplificador" or "Biblioteca de IRs") WorkbenchPanel.SetFullWidth(frame, true);
        Sections.Children.Add(frame); return panel;
    }
    private static void DescribeControls(DependencyObject root, string context)
    {
        if (root is Control control)
        {
            if (control is ListView && string.IsNullOrEmpty(AutomationProperties.GetName(control))) AutomationProperties.SetName(control, context);
            if (control is ComboBox combo && string.IsNullOrEmpty(AutomationProperties.GetName(control))) AutomationProperties.SetName(control, combo.Header?.ToString() ?? context);
            if (control is NumberBox number && string.IsNullOrEmpty(AutomationProperties.GetName(control))) AutomationProperties.SetName(control, number.Header?.ToString() ?? context);
            if (control is Button or ToggleButton or Slider or TextBox or ComboBox or NumberBox)
                if (string.IsNullOrEmpty(AutomationProperties.GetHelpText(control))) AutomationProperties.SetHelpText(control, context);
        }
        for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root); i++) DescribeControls(Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i), context);
    }
    private static WorkbenchPanel Buttons(params FrameworkElement[] buttons)
    {
        var rack = new WorkbenchPanel { CellWidth = 155, MaximumColumns = 4, Gap = 8 };
        foreach (var button in buttons) rack.Children.Add(button);
        return rack;
    }
    private void BuildDevice()
    {
        var language = LanguageService.Instance;
        var languages = new ComboBox { Header = T("Idioma"), ItemsSource = language.Languages, DisplayMemberPath = "Name", SelectedItem = language.Languages.FirstOrDefault(l => l.Code == language.Language) };
        Section(T("Idioma"), languages, Action(T("Aplicar idioma"), () => { if (languages.SelectedItem is LanguageOption selected) language.Select(selected.Code); }));
        var root = (FrameworkElement)Dialogs.Window!.Content;
        var theme = new ComboBox { Header = T("Tema"), ItemsSource = new[] { "Sistema", "Claro", "Escuro" }.Select(T).ToArray(), SelectedIndex = (int)root.RequestedTheme };
        var finishes = new ComboBox { Header = T("Acabamento"), ItemsSource = ControlFinishCatalog.All, DisplayMemberPath = "Label", SelectedItem = ControlFinishCatalog.Get(InstrumentAppearance.Finish) };
        bool applyingFinish = false;
        finishes.SelectionChanged += async (_, _) =>
        {
            if (applyingFinish || finishes.SelectedItem is not ControlFinishDefinition selected) return;
            applyingFinish = true; var previous = InstrumentAppearance.Finish;
            try { Preferences.SaveFinish(E.DataDirectory, selected.Finish); InstrumentAppearance.Select(selected.Finish); }
            catch (Exception ex)
            {
                finishes.SelectedItem = ControlFinishCatalog.Get(previous);
                await Dialogs.Show(new ContentDialog { Title = "Não foi possível concluir", Content = ex.Message, CloseButtonText = "Fechar" });
            }
            finally { applyingFinish = false; }
        };
        Section("Aparência", theme, finishes, Action("Aplicar tema", () => { var choice = (ElementTheme)theme.SelectedIndex; Preferences.SaveTheme(E.DataDirectory, choice); root.RequestedTheme = choice; }));
        var showcase = new WorkbenchPanel { CellWidth = 150, MaximumColumns = 4 };
        foreach (bool fader in new[] { false, true })
        {
            var field = new InstrumentField(fader ? "Fader" : "Knob", 0, 100, 1, fader); field.Editor.Value = 50;
            field.Instrument.RestoreRequested += (_, _) => field.Editor.Value = 50;
            showcase.Children.Add(field);
        }
        showcase.Children.Add(new FootswitchButton("Bypass"));
        Section("Prévia do acabamento", showcase);
        var refresh = Command(E, "RefreshPorts", "Atualizar portas MIDI"); AutomationProperties.SetAutomationId(refresh, "RefreshButton");
        Section("Conexão USB / MIDI", Choice(E.Connection, "Inputs", "Input", "Entrada MIDI", "Name"), Choice(E.Connection, "Outputs", "Output", "Saída MIDI", "Name"), Buttons(refresh, Command(E, "Connect", "Conectar"), Command(E, "Disconnect", "Desconectar")), Text(E.Connection, "Identity"));
        Section("Firmware", Choice(E, "Firmwares", "SelectedFirmware", "Arquivo de firmware", "Name"), Buttons(Command(E, "ChooseFirmware", "Escolher firmware"), Command(E, "RefreshFirmwareList", "Atualizar lista"), Command(E, "InstallFirmware", "Instalar firmware")));
        Section("Restauração", Buttons(Command(E, "ResetPresets", "Restaurar presets de fábrica"), Command(E, "RestoreOriginalIrs", "Restaurar IRs originais")));
        Section("Diagnóstico", Text(E, "ErrorMessage"), Text(E, "Log"));
        Section("Ajuda", Action("Abrir manuais e guias", () => { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Documents")) { UseShellExecute = true }); }));
    }
}
