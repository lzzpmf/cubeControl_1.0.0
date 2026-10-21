using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
namespace Cube.WinUI;
public sealed partial class MainWindow : Window
{
    private readonly Workspace workspace = new();
    private bool closing, checkingClose;
    private Task initialization = Task.CompletedTask;
    public MainWindow()
    {
        App.Trace("MainWindow InitializeComponent");
        InitializeComponent();
        AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Icons", "desktopIcon.ico"));
        Platform.Dialogs.Window = this;
        Root.AddHandler(UIElement.KeyDownEvent, new Microsoft.UI.Xaml.Input.KeyEventHandler((_, e) =>
        {
            if (e.Key != Windows.System.VirtualKey.F6) return;
            // A modal dialog owns keyboard navigation while it is open.
            if (Platform.Dialogs.IsOpen) return;
            var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(Root.XamlRoot) as DependencyObject;
            bool Within(DependencyObject? child, DependencyObject parent)
            {
                while (child != null) { if (child == parent) return true; child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(child); }
                return false;
            }
            int current = Within(focused, PlaybackBar) ? 2 : Within(focused, ContentFrame) ? 1 : 0;
            bool reverse = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            for (int step = 1; step <= 3; step++)
            {
                int next = (current + (reverse ? -step : step) + 3) % 3;
                var target = next == 0 ? Navigation.SelectedItem as Control : Microsoft.UI.Xaml.Input.FocusManager.FindFirstFocusableElement(next == 1 ? ContentFrame : PlaybackBar) as Control;
                if (target?.Focus(FocusState.Keyboard) == true) break;
                if (next == 0 && Microsoft.UI.Xaml.Input.FocusManager.FindFirstFocusableElement(Navigation) is Control first && first.Focus(FocusState.Keyboard)) break;
            }
            e.Handled = true;
        }), true);
        SessionBar.DataContext = workspace.Editor;
        SessionStatus.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Source = workspace.Editor, Path = new PropertyPath("Status"), Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay, Converter = new Platform.LocalizedText() });
        workspace.Editor.PropertyChanged += EditorStatusChanged;
        Appearance.StudioMotion.Attach(PlaybackPlay); Appearance.StudioMotion.Attach(PlaybackStop);
        Appearance.StudioMotion.Attach(SessionConnect); Appearance.StudioMotion.Attach(SessionDisconnect);
        Appearance.InstrumentAppearance.Select(Platform.Preferences.LoadFinish(workspace.Editor.DataDirectory));
        PlaybackBar.DataContext = workspace.Backing;
        PlaybackPlay.Command = workspace.Backing.PlayPause; PlaybackStop.Command = workspace.Backing.Stop;
        Root.RequestedTheme = Platform.Preferences.LoadTheme(workspace.Editor.DataDirectory);
        Root.SizeChanged += (_, _) => AdaptChrome();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 800));
        App.Trace("MainWindow Select");
        Navigation.SelectedItem = Navigation.MenuItems[0];
        RefreshLanguage();
        Cube.Desktop.Localization.LanguageService.Instance.PropertyChanged += LanguageChanged;
        bool initialized = false;
        ((FrameworkElement)Content).Loaded += async (_, _) =>
        {
            if (initialized) return; initialized = true;
            initialization = workspace.Initialize();
            try { await initialization; }
            catch (Exception ex) { await Platform.Dialogs.Show(new ContentDialog { Title = "Falha ao inicializar", Content = ex.Message, CloseButtonText = "Fechar" }); }
        };
        AppWindow.Closing += async (_, args) =>
        {
            if (closing) return;
            args.Cancel = true;
            if (checkingClose) return;
            checkingClose = true;
            try
            {
                try { await initialization; } catch { /* The initialization error was presented above. */ }
                if (await workspace.PrepareClose()) { closing = true; Close(); }
            }
            finally { checkingClose = false; }
        };
        Closed += (_, _) => { Cube.Desktop.Localization.LanguageService.Instance.PropertyChanged -= LanguageChanged; workspace.Editor.PropertyChanged -= EditorStatusChanged; (ContentFrame.Content as WorkspacePage)?.CancelPending(); workspace.Dispose(); };
    }
    private void EditorStatusChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName != "Status") return;
        SessionStatus.DispatcherQueue.TryEnqueue(() =>
        {
            if (SessionStatus.IsLoaded) Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer.FromElement(SessionStatus)?.RaiseAutomationEvent(Microsoft.UI.Xaml.Automation.Peers.AutomationEvents.LiveRegionChanged);
        });
    }
    private bool? compactChrome;
    private void AdaptChrome()
    {
        bool compact = Root.ActualWidth < 780;
        if (compactChrome == compact) return;
        compactChrome = compact;
        SessionBar.ColumnDefinitions.Clear(); SessionBar.RowDefinitions.Clear();
        SessionBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        SessionBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        SessionBar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        if (compact)
        {
            SessionBar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); SessionBar.RowSpacing = 6;
            Grid.SetRow(ActivePreset, 1); Grid.SetColumn(ActivePreset, 0);
            Grid.SetRow(SessionCommands, 1); Grid.SetColumn(SessionCommands, 1);
            PlaybackBar.RowDefinitions.Clear();
            PlaybackBar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            PlaybackBar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumnSpan(PlaybackTime, 3); Grid.SetRow(PlaybackPlay, 1); Grid.SetRow(PlaybackStop, 1);
        }
        else
        {
            SessionBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            SessionBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); SessionBar.RowSpacing = 0;
            Grid.SetRow(ActivePreset, 0); Grid.SetColumn(ActivePreset, 2);
            Grid.SetRow(SessionCommands, 0); Grid.SetColumn(SessionCommands, 3);
            Grid.SetColumnSpan(PlaybackTime, 1); Grid.SetRow(PlaybackPlay, 0); Grid.SetRow(PlaybackStop, 0);
            PlaybackBar.RowDefinitions.Clear();
        }
    }
    private void LanguageChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "Language") return;
        RefreshLanguage();
        if (Navigation.SelectedItem is NavigationViewItem item)
        { ContentFrame.Navigate(typeof(WorkspacePage), ((string)item.Tag, workspace), new SuppressNavigationTransitionInfo()); ContentFrame.BackStack.Clear(); }
    }
    private void RefreshLanguage()
    {
        static string T(string value) => Cube.Desktop.Localization.LanguageService.T(value);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(Navigation, T("Áreas do CubeControl"));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetHelpText(Navigation, T("F6 alterna entre navegação, conteúdo e reprodução. Shift+F6 retorna à região anterior."));
        foreach (NavigationViewItem item in Navigation.MenuItems)
            item.Content = Cube.Desktop.Localization.LanguageService.T((string)item.Tag switch { "ir" => "IRs", "backing" => "Backing tracks", "amps" => "Criador de amps", "device" => "Dispositivo e ajuda", _ => "Pedaleira" });
        PlaybackStop.Label = Cube.Desktop.Localization.LanguageService.T("Parar áudio");
        PlaybackPlay.SetBinding(Controls.TransportButton.LabelProperty, new Microsoft.UI.Xaml.Data.Binding { Source = workspace.Backing, Path = new PropertyPath("PlayLabel"), Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay, Converter = new Platform.LocalizedText() });
        SessionConnect.Content = T("Conectar"); SessionDisconnect.Content = T("Desconectar");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(SessionConnect, T("Conectar pedal"));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(SessionDisconnect, T("Desconectar pedal"));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(ActivePreset, T("Preset ativo"));
    }
    private void SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        App.Trace("Navigation selection");
        if (args.SelectedItem is not NavigationViewItem item) return;
        ContentFrame.Navigate(typeof(WorkspacePage), ((string)item.Tag, workspace), new SuppressNavigationTransitionInfo());
        if (ContentFrame.Content is FrameworkElement page) Appearance.StudioMotion.Enter(page);
        App.Trace("Navigation done");
        ContentFrame.BackStack.Clear();
    }
}
