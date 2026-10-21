using Cube.Desktop.Localization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cube.Desktop;
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private void BackingPlaylistItemSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not Grid row || row.Children[0] is not StackPanel details) return;
        // The content grid excludes the shared 12-DIP padding and one-DIP border.
        bool compact = e.NewSize.Width + 26 < 480;
        if (details.Children[0] is TextBlock title)
            title.TextWrapping = compact ? TextWrapping.Wrap : TextWrapping.NoWrap;
        if (details.Children[1] is TextBlock metadata)
        {
            metadata.TextWrapping = compact ? TextWrapping.Wrap : TextWrapping.NoWrap;
            metadata.TextTrimming = TextTrimming.CharacterEllipsis;
        }
    }
    // Compatibility names also keep automation clients pointed at the shared browser.
    private TreeView IrLibraryTree => SingleIrLibrary.Tree;
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        DependencyObject? node = e.OriginalSource as DependencyObject;
        while (node != null)
        {
            if (ReferenceEquals(node, IrLibraryTree) || node is Controls.IrPackEditor or Controls.IrPackReview
                || node is Expander { Name: "IrCaptureDetails" }) return;
            node = node is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
        Model.IrLibrary.ClearSelection();
    }

    public EditorViewModel Model { get; }
    public BackingTracksViewModel BackingTracks { get; }
    private bool closing;
    private readonly bool automaticConnectionEnabled;
    private readonly Wpf.Ui.SnackbarService snackbarService = new();
    private IInputElement? operationFocus;
    private bool restoringOperationFocus;
    private readonly System.Windows.Threading.DispatcherTimer busyMessageTimer = new() { Interval = TimeSpan.FromSeconds(4) };
    private static readonly string[] BusyMessages = ["Preparando o palco…", "Nos bastidores do seu som…", "Dando passagem ao seu timbre…"];
    private int busyMessageIndex;
    private void UpdateBusyMessage() => BusyMessage.Text = LanguageService.T(BusyMessages[busyMessageIndex]);
    private void BusyStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EditorViewModel.Busy)) return;
        if (Model.Busy) operationFocus = Keyboard.FocusedElement;
        else if (operationFocus is UIElement target)
        {
            operationFocus = null;
            restoringOperationFocus = true;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
            {
                try { if (target.IsVisible && target.IsEnabled) target.Focus(); }
                finally { restoringOperationFocus = false; }
            }));
        }
        busyMessageTimer.Stop();
        busyMessageIndex = 0;
        UpdateBusyMessage();
        if (Model.Busy) busyMessageTimer.Start();
    }

    public MainWindow() : this(new EditorViewModel()) { automaticConnectionEnabled = true; }
    internal MainWindow(EditorViewModel model, bool initializeDevices = true)
    {
        Model = model;
        Model.ImageSelectionRequested += ShowImageSelection;
        Closed += (_, _) => Model.ImageSelectionRequested -= ShowImageSelection;
        BackingTracks = new BackingTracksViewModel(Model.DataDirectory);
        Model.IrAudition.BeforePlayback = BackingTracks.StopForIrAudition;
        BackingTracks.BeforePlayback = Model.IrAudition.StopAsync;
        BackingTracks.ChooseExportMode = ChooseBackingExportMode;
        PreviewKeyDown += SuppressStandaloneAltDown;
        PreviewKeyUp += SuppressStandaloneAltUp;
        Deactivated += (_, _) => standaloneAltPressed = false;
        PreviewKeyDown += BackingKeyDown;
        Deactivated += (_, _) => { BackingTracks.StopYouTubePreview(); _ = BackingTracks.StopLocalPreview(); };
        Appearance.UiDensity.Initialize();
        InitializeComponent();
        ConfigureDragDrop();
        BackingPage.AddHandler(System.Windows.Controls.Primitives.Thumb.DragStartedEvent, new System.Windows.Controls.Primitives.DragStartedEventHandler((_, _) => BackingTracks.BeginClipGesture()));
        BackingPage.AddHandler(System.Windows.Controls.Primitives.Thumb.DragCompletedEvent, new System.Windows.Controls.Primitives.DragCompletedEventHandler((_, _) => BackingTracks.EndClipGesture()));
        RegisterName("IrLibraryTree", SingleIrLibrary.Tree);
        RegisterName("IrLibrarySearch", SingleIrLibrary.SearchBox); DataContext = Model;
        BackingPage.Loaded += (_, _) =>
        {
            var host = VisualTreeHelper.GetParent(BackingPage) as FrameworkElement;
            if (host != backingViewportHost)
            {
                if (backingViewportHost != null) backingViewportHost.SizeChanged -= BackingViewportSizeChanged;
                backingViewportHost = host;
                if (host != null) host.SizeChanged += BackingViewportSizeChanged;
            }
            ApplyBackingLayout(BackingPage.ActualWidth);
        };
        BackingMainWave.MenuRequested = (wave, time) => OpenBackingMenu(wave, "wave", time: time);
        BackingMainWave.ZoomRequested = (steps, fraction) =>
        {
            if (BackingTracks.Duration <= 0) return;
            if (BackingTracks.IsRecording) BackingTracks.FollowRecording = false;
            double anchor = BackingTracks.ViewportStart + fraction * BackingTracks.ViewportDuration;
            BackingTracks.Zoom *= Math.Pow(2, steps);
            BackingTracks.ViewportStart = anchor - fraction * BackingTracks.ViewportDuration;
        };

        Model.ConfirmIrWrite = ConfirmIrWriteAsync;
        InitializeZoom();
        MoveFooterToPage();
        Workspace.RequestBringIntoView += (_, e) =>
        {
            if (Model.Busy || restoringOperationFocus) e.Handled = true;
        };
#if CUBE_SIMULATION
        Title = "CubeControl — SIMULAÇÃO TEMPORÁRIA · sem pedal físico";
#endif
        SourceInitialized += (_, _) => Appearance.WindowWorkArea.ClampInitialSize(this);
        busyMessageTimer.Tick += (_, _) => { busyMessageIndex = (busyMessageIndex + 1) % BusyMessages.Length; UpdateBusyMessage(); };
        Model.PropertyChanged += BusyStateChanged;
        Closed += (_, _) => { BackingTracks.Dispose(); busyMessageTimer.Stop(); Model.PropertyChanged -= BusyStateChanged; };
        snackbarService.SetSnackbarPresenter(Notifications);
        Model.NotificationRequested += ShowNotification;
        Closed += (_, _) => Model.NotificationRequested -= ShowNotification;
        SizeChanged += (_, _) => UpdateWorkspaceLayout();
        Workspace.SizeChanged += (_, _) => UpdateWorkspaceLayout();
        ClientArea.SizeChanged += (_, _) => UpdateWorkspaceLayout();
        Tabs.SelectionChanged += (_, e) => { if (e.Source == Tabs) UpdateSaveAppearance(); };
        Closed += (_, _) => Appearance.ThemeService.Instance.Changed -= RefreshThemeColors;
        Navigation.SelectedIndex = Tabs.SelectedIndex;
        AddHandler(Controls.Knob.GestureStartedEvent, new RoutedEventHandler((_, e) => Model.BeginGesture(e.OriginalSource)));
        AddHandler(Controls.Knob.GestureEndedEvent, new RoutedEventHandler((_, e) => Model.EndGesture(e.OriginalSource)));
        AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler((_, e) =>
        {
            if (e.Key == Key.Enter && e.OriginalSource is TextBox box && box is not Wpf.Ui.Controls.NumberBox && !box.IsReadOnly && !box.AcceptsReturn)
            { box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource(); e.Handled = true; }
        }));
        Loaded += async (_, _) =>
        {
            Appearance.ThemeService.Instance.Attach(this);
            Appearance.ThemeService.Instance.Changed += RefreshThemeColors;
            // Visual test hosts exercise the real window with isolated models and simulated MIDI.
            if (initializeDevices)
            {
                await Model.Initialize();
                await BackingTracks.Initialize();
                await Model.IrAudition.RefreshDevices();
            }
            string[] args = Environment.GetCommandLineArgs(); int smoke = Array.IndexOf(args, "--smoke");
            if (automaticConnectionEnabled && smoke < 0 && !args.Contains("--offline")) Model.StartAutomaticConnection();
            if (smoke >= 0 && args.Length > smoke + 1)
            {
                if (args.Contains("--english")) LanguageService.Instance.Preview("en-US");
                if (args.Contains("--light")) Appearance.ThemeService.Instance.Preview(Appearance.ThemeMode.Light);
                if (args.Contains("--dark")) Appearance.ThemeService.Instance.Preview(Appearance.ThemeMode.Dark);
                if (args.Contains("--highcontrast")) Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                if (!args.Contains("--offline")) await Model.Execute(Model.ConnectAsync);
                if (args.Contains("--compact")) { Width = 920; Height = 740; }
                if (args.Contains("--small")) { Width = 680; Height = 600; }
                if (args.Contains("--scale125")) { Width = 1400; Height = 940; Workspace.LayoutTransform = new ScaleTransform(1.25, 1.25); DialogHost.LayoutTransform = new ScaleTransform(1.25, 1.25); }
                if (args.Contains("--scale150")) { Width = 1440; Height = 1080; Workspace.LayoutTransform = new ScaleTransform(1.5, 1.5); DialogHost.LayoutTransform = new ScaleTransform(1.5, 1.5); }
                if (args.Contains("--scale200")) { Width = 1840; Height = 1400; Workspace.LayoutTransform = new ScaleTransform(2, 2); DialogHost.LayoutTransform = new ScaleTransform(2, 2); }
                int wav = Array.IndexOf(args, "--wav");
                if (wav >= 0) Model.Ir.Load(args[wav + 1], IrAudio.Decode(await File.ReadAllBytesAsync(args[wav + 1])));
                await Task.Delay(250);
                for (int tab = 0; tab < Tabs.Items.Count; tab++)
                {
                    Tabs.SelectedIndex = tab; UpdateLayout(); await Task.Delay(80);
                    Capture(args[smoke + 1].Replace(".png", $"-{tab}.png"));
                    var scroll = Descendants<ScrollViewer>(Tabs).OrderByDescending(s => s.ScrollableHeight).FirstOrDefault();
                    if (scroll?.ScrollableHeight > 0)
                    {
                        scroll.ScrollToEnd(); UpdateLayout();
                        Capture(args[smoke + 1].Replace(".png", $"-{tab}-end.png"));
                        if (scroll.VerticalOffset == 0) throw new Exception("Rolagem indisponível.");
                    }
                }
                Tabs.SelectedIndex = 0;
                for (int effect = 0; effect < Model.Stages.Length; effect++)
                {
                    Model.SelectedStage = effect; UpdateLayout(); await Task.Delay(80);
                    foreach (var scroll in Descendants<ScrollViewer>(Tabs)) scroll.ScrollToTop();
                    UpdateLayout();
                    Capture(args[smoke + 1].Replace(".png", $"-effect-{effect}.png"));
                }
                if (WorkspaceScroll.ScrollableHeight > 0)
                {
                    WorkspaceScroll.ScrollToEnd(); UpdateLayout();
                    Capture(args[smoke + 1].Replace(".png", "-workspace-end.png"));
                    WorkspaceScroll.ScrollToTop();
                }
                if (args.Contains("--gallery"))
                {
                    var gallery = new Views.GearGallery(Model.Amps, Model.SelectedAmp, true);
                    var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = "Escolha seu amplificador", Content = gallery, PrimaryButtonText = "Aplicar seleção", CloseButtonText = "Cancelar", DialogMaxWidth = 900 };
                    var shown = dialog.ShowLocalizedAsync(); await Task.Delay(350); UpdateLayout(); Capture(args[smoke + 1].Replace(".png", "-gallery.png")); dialog.Hide(); await shown;
                    var mapping = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = "Match Cabinet · associações", Content = new Views.MatchSettings(Model.CreateMatchRows()), PrimaryButtonText = "Salvar associações", CloseButtonText = "Cancelar", DialogMaxWidth = 680, DialogMaxHeight = 900 };
                    shown = mapping.ShowLocalizedAsync(); await Task.Delay(350); UpdateLayout(); Capture(args[smoke + 1].Replace(".png", "-associations.png")); mapping.Hide(); await shown;
                }
                await File.WriteAllTextAsync(args[smoke + 1] + ".log", Model.Log);
                if (Model.WriteCommandCount != 0) throw new Exception("Smoke must never write to the device.");
                foreach (string line in Model.Log.Split('\n').Where(l => l.Contains(" TX ")))
                {
                    var command = UsbSysEx.Decode(Convert.FromHexString(line[(line.IndexOf(" TX ") + 4)..].Trim())).Command;
                    if (command is not (0x11 or 0x23)) throw new InvalidOperationException("Renderização provocou escrita: " + line);
                }
                CheckPresentation();
                closing = true; Appearance.ThemeService.Instance.Detach(); Model.Dispose(); Close();
            }
        };
        Closing += async (_, e) =>
        {
            _ = Model.IrAudition.StopAsync();
            if (closing) return;
            e.Cancel = true;
            if (Model.Busy) { await ShowNoticeAsync("Aguarde a operação terminar antes de fechar."); return; }
            if (!await Model.PackEditor.ConfirmDiscard()) return;
            if (Model.Dirty && !await Model.Ask("Há ajustes aplicados que não foram salvos. Fechar o editor?")) return;
            if (Enumerable.Range(1, 4).Any(Model.AdvancedPreviewHasChanges) && !await Model.Ask("Os ajustes dos efeitos estão apenas na RAM. Encerrar as prévias e restaurar os parâmetros anteriores antes de fechar?")) return;
            try
            {
                await Model.Flush(); await Model.EndDelayPreviewAsync(); await Model.EndReverbPreviewAsync(); await Model.EndChorusPreviewAsync(); await Model.EndPhaserPreviewAsync();
                if (await Model.RestoreAmpAsync() == false) throw new IOException(Model.AmpPreviewMessage);
                await Model.EndPreviewAsync();
            }
            catch (Exception ex)
            {
                if (!await Model.Ask(ex.Message + "\nFechar mesmo assim? Os snapshots em memória serão descartados.")) return;
            }
            closing = true; Appearance.ThemeService.Instance.Detach(); Model.Dispose(); _ = Dispatcher.BeginInvoke(new Action(Close));
        };
    }
    private bool noticeOpen;
    private async void OperationDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || sender is not FrameworkElement { DataContext: EditorViewModel.OperationHistoryItem item }) return;
        e.Handled = true;
        await Model.RestoreOperationAsync(item);
    }
    private async Task<(bool Confirmed, string? ImagePath)> ConfirmIrWriteAsync(string message)
    {
        var content = new StackPanel { MaxWidth = 560 };
        content.Children.Add(new TextBlock { Text = LanguageService.T(message), TextWrapping = TextWrapping.Wrap });
        content.Children.Add(new TextBlock { Text = LanguageService.T("Imagem do gabinete (opcional)"), Margin = new Thickness(0, 16, 0, 8) });
        var images = new ComboBox { HorizontalContentAlignment = HorizontalAlignment.Stretch, MaxDropDownHeight = 320 };
        images.Items.Add(new ComboBoxItem { Content = LanguageService.T("Manter imagem atual ou padrão"), Tag = null });
        string directory = AssetPaths.CabinetSelection;
        if (Directory.Exists(directory))
            foreach (string path in Directory.EnumerateFiles(directory, "*.png").OrderBy(Path.GetFileName))
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                var bitmap = new BitmapImage();
                bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.DecodePixelWidth = 120;
                bitmap.UriSource = new Uri(path); bitmap.EndInit(); bitmap.Freeze();
                row.Children.Add(new Image { Source = bitmap, Width = 72, Height = 48, Stretch = Stretch.Uniform, Margin = new Thickness(0, 0, 12, 0) });
                row.Children.Add(new TextBlock { Text = Path.GetFileNameWithoutExtension(path), MaxWidth = 360, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center });
                images.Items.Add(new ComboBoxItem { Content = row, Tag = path });
            }
        images.SelectedIndex = 0;
        content.Children.Add(images);
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
        {
            Title = LanguageService.T("Gravar IR"), Content = content, DialogMaxWidth = 640,
            PrimaryButtonText = LanguageService.T("Gravar IR"), CloseButtonText = LanguageService.T("Cancelar")
        };
        bool confirmed = await dialog.ShowLocalizedAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary;
        return (confirmed, confirmed ? (images.SelectedItem as ComboBoxItem)?.Tag as string : null);
    }
    internal async Task ShowNoticeAsync(string message)
    {
        if (noticeOpen) return;
        noticeOpen = true;
        var previousFocus = Keyboard.FocusedElement;
        try
        {
            var dialog = new Wpf.Ui.Controls.MessageBox { PrimaryButtonIcon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Checkmark24), CloseButtonIcon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Dismiss24),
                Owner = this, Title = "CubeControl", Content = LanguageService.T(message),
                CloseButtonText = LanguageService.T("Fechar")
            };
            await dialog.ShowDialogAsync();
        }
        finally
        {
            noticeOpen = false;
            if (previousFocus is UIElement { IsVisible: true, IsEnabled: true } element) element.Focus();
        }
    }



    private bool standaloneAltPressed;
    private void SuppressStandaloneAltDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is not (Key.LeftAlt or Key.RightAlt))
        {
            standaloneAltPressed = false;
            return;
        }
        // Handle only the modifier event, never the following shortcut key.
        // Ctrl+Alt (including AltGr) and other modifier combinations pass through.
        if ((Keyboard.Modifiers & ~ModifierKeys.Alt) != ModifierKeys.None)
        {
            standaloneAltPressed = false;
            return;
        }
        if (!e.IsRepeat) standaloneAltPressed = true;
        e.Handled = true;
    }
    private void SuppressStandaloneAltUp(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is not (Key.LeftAlt or Key.RightAlt)) return;
        if (standaloneAltPressed && (Keyboard.Modifiers & ~ModifierKeys.Alt) == ModifierKeys.None)
            e.Handled = true;
        standaloneAltPressed = false;
    }

    private int navigationWheelDelta;
    private bool navigatingByWheel;
    private void NavigationLeave(object sender, MouseEventArgs e) => navigationWheelDelta = 0;
    private void NavigationWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        if (!Navigation.IsEnabled || Navigation.SelectedIndex < 0 || Navigation.Items.Count == 0)
        { navigationWheelDelta = 0; return; }
        navigationWheelDelta += e.Delta;
        int steps = navigationWheelDelta / 120;
        navigationWheelDelta %= 120;
        if (steps == 0) return;
        int next = Math.Clamp(Navigation.SelectedIndex - steps, 0, Navigation.Items.Count - 1);
        if (Navigation.Items[next] is ListBoxItem item && (!item.IsEnabled || item.Visibility != Visibility.Visible))
        { navigationWheelDelta = 0; return; }
        navigatingByWheel = true;
        try { Navigation.SelectedIndex = next; }
        finally { navigatingByWheel = false; }
    }
    private void NavigationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source == Navigation && !navigatingByWheel) navigationWheelDelta = 0;
        if (Tabs != null && e.Source == Navigation && Navigation.SelectedIndex >= 0)
        {
            Tabs.SelectedIndex = Navigation.SelectedIndex;
        }
    }
    internal Action<Appearance.ControlFinish> SelectControlFinish { get; set; } = Appearance.ThemeService.Instance.SelectFinish;
    private void MoreOptionsClick(object sender, RoutedEventArgs e)
    {
        if (MoreOptionsButton.ContextMenu is { IsOpen: true } opened) { opened.IsOpen = false; return; }
        // Rebuild on opening so translations and persisted selections are always current.
        var menu = new ContextMenu
        {
            PlacementTarget = MoreOptionsButton,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
            MinWidth = 260,
            UseLayoutRounding = true,
            SnapsToDevicePixels = true
        };
        // WPF UI disables the native popup animation; animate its surface after anchoring.
        async Task AlignMenu()
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            if (!menu.IsOpen || menu.Template.FindName("Border", menu) is not FrameworkElement surface) return;
            var root = (FrameworkElement)PresentationSource.FromVisual(menu).RootVisual;
            var anchor = MoreOptionsButton.PointToScreen(new Point(MoreOptionsButton.ActualWidth, MoreOptionsButton.ActualHeight));
            var rootOrigin = root.PointToScreen(new Point());
            var edge = surface.PointToScreen(new Point(surface.ActualWidth, 0));
            var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(MoreOptionsButton);
            // Absolute placement avoids applying the client's 90% transform a second time.
            menu.PlacementTarget = null;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
            menu.HorizontalOffset = (anchor.X - (edge.X - rootOrigin.X)) / dpi.DpiScaleX;
            menu.VerticalOffset = anchor.Y / dpi.DpiScaleY + 4;
            // Changing placement also reanchors the template margin; settle it before final alignment.
            for (int pass = 0; pass < 3; pass++)
            {
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                if (!menu.IsOpen) return;
                edge = surface.PointToScreen(new Point(surface.ActualWidth, 0));
                menu.HorizontalOffset += (anchor.X - edge.X) / dpi.DpiScaleX;
            }
            Controls.SurfaceMotion.SetEnabled(surface, true);
        }
        menu.Opened += async (_, _) =>
        {
            menu.ApplyTemplate();
            if (menu.Template.FindName("Border", menu) is Border surface)
                Appearance.PopupRendering.SeparateShadow(surface);
            await AlignMenu();
        };
        MenuItem Entry(string label, Wpf.Ui.Controls.SymbolRegular symbol) => new()
        {
            Header = LanguageService.T(label),
            Icon = Cube.Desktop.Appearance.AppIcons.Create(symbol, navigation: true)
        };
        var themeMenu = Entry("Tema", Wpf.Ui.Controls.SymbolRegular.Color24);
        foreach (var (mode, label) in new[] { (Appearance.ThemeMode.System, "Sistema"), (Appearance.ThemeMode.Light, "Claro"), (Appearance.ThemeMode.Dark, "Escuro") })
        {
            var item = new MenuItem { Header = LanguageService.T(label), Style = (Style)FindResource("PreferenceMenuItem"), Template = (ControlTemplate)FindResource("PreferenceMenuItemTemplate"), Tag = Appearance.ThemeService.Instance.Mode == mode };
            item.Click += async (_, args) =>
            {
                args.Handled = true;
                try { Appearance.ThemeService.Instance.Select(mode); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { await ShowNoticeAsync(LanguageService.T("Não foi possível salvar as preferências locais: ") + ex.Message); }
            };
            themeMenu.Items.Add(item);
        }
        var finishMenu = Entry("Acabamento dos controles", Wpf.Ui.Controls.SymbolRegular.Color24);
        foreach (var definition in Appearance.ControlFinishCatalog.All)
        {
            var finish = definition.Finish;
            var label = definition.Label;
            var item = new MenuItem { Header = LanguageService.T(label), Style = (Style)FindResource("PreferenceMenuItem"), Template = (ControlTemplate)FindResource("PreferenceMenuItemTemplate"), Tag = Appearance.ThemeService.Instance.Finish == finish };
            item.Click += async (_, args) =>
            {
                args.Handled = true;
                try { SelectControlFinish(finish); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { await ShowNoticeAsync(LanguageService.T("Não foi possível salvar as preferências locais: ") + ex.Message); }
            };
            finishMenu.Items.Add(item);
        }
        var languageMenu = Entry("Idioma", Wpf.Ui.Controls.SymbolRegular.LocalLanguage24);
        foreach (var language in LanguageService.Instance.Languages)
        {
            var option = new MenuItem { Header = language.Name, Style = (Style)FindResource("PreferenceMenuItem"), Template = (ControlTemplate)FindResource("PreferenceMenuItemTemplate"), Tag = LanguageService.Instance.Language == language.Code };
            option.Click += async (_, args) =>
            {
                args.Handled = true;
                try { LanguageService.Instance.Select(language.Code); }
                catch (IOException ex) { await ShowNoticeAsync(LanguageService.T("Não foi possível salvar as preferências locais: ") + ex.Message); }
                catch (UnauthorizedAccessException ex) { await ShowNoticeAsync(LanguageService.T("Não foi possível salvar as preferências locais: ") + ex.Message); }
            };
            languageMenu.Items.Add(option);
        }
        menu.Items.Add(themeMenu);
        menu.Items.Add(languageMenu);
        menu.Items.Add(finishMenu);
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateZoomMenu(AlignMenu));
        // WPF UI 4.3 names its submenu popup "Popup". Open toward the workspace,
        // since this menu is anchored at the window's right edge.
        foreach (var submenu in new[] { themeMenu, languageMenu, finishMenu })
            submenu.Loaded += (_, _) =>
            {
                submenu.ApplyTemplate();
                if (submenu.Template.FindName("Popup", submenu) is System.Windows.Controls.Primitives.Popup popup)
                {
                    popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
                    popup.Opened += (_, _) =>
                    {
                        if (submenu.Template.FindName("SubmenuBorder", submenu) is Border surface)
                            Appearance.PopupRendering.SeparateShadow(surface);
                    };
                }
            };
        menu.Closed += (_, _) => { if (MoreOptionsButton.IsEnabled) MoreOptionsButton.Focus(); };
        MoreOptionsButton.ContextMenu = menu;
        menu.IsOpen = true;
    }
    private void ManageIrsClick(object sender, RoutedEventArgs e)
    {
        Tabs.SelectedIndex = 1;
        Navigation.SelectedIndex = 1;
        Navigation.Focus();
    }
    private void UpdateSaveAppearance()
    {
        Navigation.SelectedIndex = Tabs.SelectedIndex;
        BackingTracks.PageVisible = Tabs.SelectedIndex == 2;
        Model.IrAudition.PanelVisible = Tabs.SelectedIndex == 1;
        UpdateWorkspaceLayout();
        MoveFooterToPage();
    }
    private void MoveFooterToPage()
    {
        if (FooterActions == null || Tabs == null) return;
        var page = Tabs.SelectedIndex switch { 1 => IrScroll, 2 => BackingTracksScroll, 3 => null, 4 => HelpScroll, _ => EditorScroll };
        if (page?.Content is not StackPanel content || FooterActions.Parent == content) return;
        if (FooterActions.Parent is Panel parent) parent.Children.Remove(FooterActions);
        content.Children.Add(FooterActions);
    }
    // Prevent ListBoxItem from focusing/selecting on right-click in navigation controls.
    private void IgnoreSelectorRightClick(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private int chainWheelDelta;
    private int presetWheelDelta;
    private void EditorPageWheel(object sender, MouseWheelEventArgs e)
    {
        // Preserve controls whose wheel has a musical/navigation meaning.
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null && node != EditorScroll;)
        {
            if (node is Controls.AmpEqGraph eq && eq.FilterAt(e.GetPosition(eq)) >= 0) return;
            if (node is Controls.BackingFader { IsEnabled: true } or Controls.BackingWaveform { IsOverview: false }
                || node is Controls.Knob { IsEnabled: true }
                || node == BackingBpmButton
                || node == BackingSpeedOptions || node == BackingPitchOptions
                || ((node == EffectChain || node == PresetSelector) && Model.HasBank)
                || (node == FocusedArt && Model.HasGallery)) return;
            node = node is Visual || node is System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
        PageMouseWheel(sender, e);
    }
    private void PageMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Disabled inner viewers still consume the bubbling wheel event in compact
        // mode. Route it to the nearest viewer that can move in this direction,
        // then fall through from the library boundaries to the page/workspace.
        if (e.Delta == 0 || SystemParameters.WheelScrollLines == 0) return;
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null;)
        {
            if (node is ScrollViewer scroll && scroll.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled
                && scroll.ScrollableHeight > 0
                && (e.Delta > 0 ? Controls.SmoothScroll.Target(scroll) > 0 : Controls.SmoothScroll.Target(scroll) < scroll.ScrollableHeight))
            {
                if (Controls.SmoothScroll.Wheel(scroll, e.Delta)) { e.Handled = true; return; }
            }
            node = node is Visual || node is System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
    }
    private void PresetSelectorLeave(object sender, MouseEventArgs e) => presetWheelDelta = 0;
    private void PresetSelectorWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Model.HasBank) { presetWheelDelta = 0; return; }
        e.Handled = true;
        presetWheelDelta += e.Delta;
        int steps = presetWheelDelta / 120;
        presetWheelDelta %= 120;
        if (steps == 0) return;
        Model.SelectedIndex = Math.Clamp(Model.SelectedIndex - steps, 0, Model.Presets.Length - 1);
        PresetSelector.ScrollIntoView(Model.Current);
    }

    private void EffectChainLeave(object sender, MouseEventArgs e) => chainWheelDelta = 0;
    private void EffectChainWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Model.HasBank) { chainWheelDelta = 0; return; }
        e.Handled = true;
        chainWheelDelta += e.Delta;
        int steps = chainWheelDelta / 120;
        chainWheelDelta %= 120;
        if (steps == 0) return;
        Model.SelectedStage = Math.Clamp(Model.SelectedStage - steps, 0, Model.Stages.Length - 1);
        EffectChain.ScrollIntoView(Model.Stages[Model.SelectedStage]);
    }

    private void HeaderSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (HeaderGrid == null || ConnectionGroup == null || EditHeaderGrid == null || SaveGroup == null) return;
        bool compact = HeaderGrid.ActualWidth < 1000;
        Grid.SetRow(ConnectionGroup, 0);
        Grid.SetColumn(ConnectionGroup, 1);
        Grid.SetColumnSpan(ConnectionGroup, 1);
        ConnectionGroup.Margin = new Thickness(16, 0, 0, 0);
        Grid.SetRow(NavigationHeader, compact ? 1 : 0);
        Grid.SetColumnSpan(NavigationHeader, compact ? 3 : 1);
        NavigationHeader.Margin = new Thickness(0, compact ? 12 : 0, 0, 0);
        // Equal side columns keep A/B/C centered independently of action widths.
        bool stackActions = EditHeaderGrid.ActualWidth < 1040;
        Grid.SetRow(PresetGroup, 0); Grid.SetColumn(PresetGroup, stackActions ? 0 : 1);
        Grid.SetColumnSpan(PresetGroup, stackActions ? 3 : 1);
        PresetGroup.Margin = new Thickness(0);
        Grid.SetRowSpan(OutputGroup, stackActions ? 1 : 4);
        Grid.SetRowSpan(SaveGroup, stackActions ? 1 : 4);
        Grid.SetRow(OutputGroup, stackActions ? 1 : 0); Grid.SetColumn(OutputGroup, 0);
        OutputGroup.HorizontalAlignment = HorizontalAlignment.Left;
        OutputGroup.Margin = new Thickness(0, stackActions ? 16 : 0, 16, 0);
        Grid.SetRow(SaveGroup, stackActions ? 1 : 0); Grid.SetColumn(SaveGroup, stackActions ? 1 : 2);
        Grid.SetColumnSpan(SaveGroup, stackActions ? 2 : 1);
        SaveGroup.Margin = new Thickness(stackActions ? 0 : 16, stackActions ? 16 : 0, 0, 0);
        SaveGroup.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetRow(FootswitchHint, 3);
        Grid.SetColumnSpan(OutputGroup, 1);
        bool narrow = EditHeaderGrid.ActualWidth < 440;
        if (narrow)
        {
            Grid.SetColumnSpan(OutputGroup, 3);
            OutputGroup.HorizontalAlignment = HorizontalAlignment.Center;
            OutputGroup.Margin = new Thickness(0, 16, 0, 0);
            Grid.SetRow(SaveGroup, 2); Grid.SetColumn(SaveGroup, 0); Grid.SetColumnSpan(SaveGroup, 3);
            SaveGroup.Margin = new Thickness(0, 16, 0, 0);
            Grid.SetRow(FootswitchHint, 4);
        }
        if (HeaderGrid.ActualWidth < 600)
        {
            Grid.SetColumn(ConnectionGroup, 0); Grid.SetColumnSpan(ConnectionGroup, 2);
            ConnectionGroup.Margin = new Thickness(0);
        }
    }
    private (FrameworkElement Cell, int Row, int Column)[]? footswitchCells;
    private void FootswitchTableSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not Grid table) return;
        footswitchCells ??= table.Children.OfType<FrameworkElement>().Select(cell => (cell, Grid.GetRow(cell), Grid.GetColumn(cell))).ToArray();
        bool narrow = table.ActualWidth < 520;
        while (table.RowDefinitions.Count < 11) table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        table.ColumnDefinitions[0].Width = narrow ? new GridLength(1, GridUnitType.Star) : new GridLength(100);
        table.ColumnDefinitions[1].Width = narrow ? new GridLength(1, GridUnitType.Star) : new GridLength(150);
        table.ColumnDefinitions[2].Width = narrow ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        foreach (var (cell, row, column) in footswitchCells)
        {
            Grid.SetRow(cell, narrow && row > 0 ? 2 * row - (column == 2 ? 0 : 1) : row);
            Grid.SetColumn(cell, narrow && column == 2 ? 0 : column);
            Grid.SetColumnSpan(cell, narrow && column == 2 ? 3 : 1);
            cell.Visibility = narrow && row == 0 && column == 2 ? Visibility.Collapsed : Visibility.Visible;
            if (cell is TextBlock text) text.TextWrapping = TextWrapping.Wrap;
        }
    }
    private void IrIndividualSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // The section includes 32 DIPs of card padding: switch at 1800 DIPs.
        bool wide = IrSectionPanel.ActualWidth >= 1768;
        IrIndividualLayout.ColumnDefinitions[1].Width = wide ? new GridLength(440) : new GridLength(0);
        Grid.SetRowSpan(IrAnalysisPanel, wide ? 3 : 1);
        Grid.SetColumn(IrStatusNotice, wide ? 1 : 0);
        Grid.SetRow(IrStatusNotice, wide ? 0 : 1);
        Grid.SetColumn(IrPreparationPanel, wide ? 1 : 0);
        Grid.SetRow(IrPreparationPanel, wide ? 1 : 2);
        IrAnalysisPanel.Margin = new Thickness(0, 0, wide ? 16 : 0, 12);
    }

    private void UpdateWorkspaceLayout()
    {
        if (WorkspaceScroll == null) return;
        // At high DPI / very short work areas, keep every command reachable by scrolling
        // the content and footer beneath the permanently visible navigation.
        bool shortViewport = ClientArea.ActualHeight < 400;
        foreach (ListBoxItem item in Navigation.Items)
        {
            if (item.Content is not StackPanel row) continue;
            row.Children[0].Visibility = shortViewport ? Visibility.Collapsed : Visibility.Visible;
            if (row.Children[1] is StackPanel labels)
                labels.Children[1].Visibility = shortViewport ? Visibility.Collapsed : Visibility.Visible;
            if (shortViewport) item.MinHeight = 32; else item.ClearValue(FrameworkElement.MinHeightProperty);
            if (shortViewport) item.Padding = new Thickness(10, 6, 10, 6);
            else item.ClearValue(Control.PaddingProperty);
        }
        HeaderGrid.Margin = new Thickness(0, 0, 0, shortViewport ? 8 : 20);
        bool compact = Tabs.SelectedIndex == 3 || ClientArea.ActualHeight < 764 || Workspace.ActualWidth < 1000;
        WorkspaceScroll.VerticalScrollBarVisibility = Tabs.SelectedIndex == 2 ? ScrollBarVisibility.Disabled : compact ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Disabled;
        // A single scrolling owner: natural content height in compact mode, a
        // constrained page viewport in desktop mode. Never force a workspace height.
        Workspace.Height = double.NaN;
        if (AmpEditorHost?.Content is Views.AmpEditor ampEditor) ampEditor.IsCompactLayout = compact;
        foreach (var scroll in new[] { EditorScroll, IrScroll, HelpScroll, BackingTracksScroll })
            scroll.VerticalScrollBarVisibility = scroll == BackingTracksScroll ? ScrollBarVisibility.Hidden : compact ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Hidden;
        FocusedArt.MaxHeight = ClientArea.ActualHeight < 764 ? 160 : 240;
        double margin = ClientArea.ActualWidth < 1000 ? 16 : 24;
        Workspace.Margin = new Thickness(margin, shortViewport ? 12 : 20, margin, 12);
        pageScrollOwner = Tabs.SelectedIndex == 2 ? BackingTracksScroll : compact ? WorkspaceScroll
            : Tabs.SelectedIndex switch { 1 => IrScroll, 4 => HelpScroll, _ => EditorScroll };
        SyncWindowPageScroll();
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(SyncWindowPageScroll));
    }
    private ScrollViewer? pageScrollOwner;
    private void PageScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, pageScrollOwner)) SyncWindowPageScroll();
    }
    private void SyncWindowPageScroll()
    {
        if (WindowPageScrollBar == null) return;
        if (pageScrollOwner == null || !pageScrollOwner.IsVisible)
        {
            WindowPageScrollBar.Visibility = Visibility.Collapsed;
            return;
        }
        WindowPageScrollBar.Margin = new Thickness(0,
            Workspace.Margin.Top + HeaderGrid.ActualHeight + HeaderGrid.Margin.Bottom, 0, Workspace.Margin.Bottom);
        WindowPageScrollBar.Maximum = pageScrollOwner.ScrollableHeight;
        WindowPageScrollBar.ViewportSize = pageScrollOwner.ViewportHeight;
        WindowPageScrollBar.LargeChange = pageScrollOwner.ViewportHeight;
        WindowPageScrollBar.SmallChange = 48;
        WindowPageScrollBar.Value = pageScrollOwner.VerticalOffset;
        WindowPageScrollBar.Visibility = pageScrollOwner.ScrollableHeight > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    private void WindowPageScroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
    {
        if (pageScrollOwner == null) return;
        Controls.SmoothScroll.Stop(pageScrollOwner);
        pageScrollOwner.ScrollToVerticalOffset(e.NewValue);
    }

    internal void ShowNotification(UserNotification notification)
    {
        if (!Dispatcher.CheckAccess()) { Dispatcher.BeginInvoke(() => ShowNotification(notification)); return; }
        if (closing) return;
        var symbol = notification.Appearance switch
        {
            Wpf.Ui.Controls.ControlAppearance.Danger => Wpf.Ui.Controls.SymbolRegular.ErrorCircle24,
            Wpf.Ui.Controls.ControlAppearance.Caution => Wpf.Ui.Controls.SymbolRegular.Warning24,
            _ => Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24
        };
        snackbarService.Show(LanguageService.T(notification.Title), LanguageService.T(notification.Message),
            notification.Appearance, Cube.Desktop.Appearance.AppIcons.Create(symbol), notification.Duration);
        if (Notifications.Content is { } snackbar)
        {
            System.Windows.Automation.AutomationProperties.SetName(snackbar,
                LanguageService.T(notification.Title) + ". " + LanguageService.T(notification.Message));
            System.Windows.Automation.AutomationProperties.SetLiveSetting(snackbar,
                notification.Appearance == Wpf.Ui.Controls.ControlAppearance.Danger
                    ? System.Windows.Automation.AutomationLiveSetting.Assertive
                    : System.Windows.Automation.AutomationLiveSetting.Polite);
            System.Windows.Automation.Peers.UIElementAutomationPeer.FromElement(snackbar)?
                .RaiseAutomationEvent(System.Windows.Automation.Peers.AutomationEvents.LiveRegionChanged);
        }
    }
    private void RefreshThemeColors() => Model.RefreshThemeColors();
    private async void ConnectionClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem)
        {
            MoreOptionsButton.ContextMenu.IsOpen = false;
            MoreOptionsButton.Focus();
        }
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = "Conexão USB", Content = new Views.ConnectionDetails { DataContext = Model }, CloseButtonText = "Fechar" };
        await dialog.ShowLocalizedAsync();
    }
    private async void UserManualClick(object sender, RoutedEventArgs e)
    {
        if (Documentation.ManualLauncher.TryOpen(AssetPaths.Documents, LanguageService.Instance.Language,
            Appearance.ThemeService.Instance.IsLight, out string error)) return;
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
        {
            Title = "Manual do cubeControl", Content = error, CloseButtonText = "Fechar"
        };
        await dialog.ShowLocalizedAsync();
    }
    private Views.DocumentationWindow? documentationWindow;
    private void DocumentationClick(object sender, RoutedEventArgs e)
    {
        if (documentationWindow == null)
        {
            documentationWindow = new Views.DocumentationWindow { Owner = this };
            documentationWindow.Closed += (_, _) => documentationWindow = null;
            documentationWindow.Show();
        }
        else
        {
            if (documentationWindow.WindowState == WindowState.Minimized) documentationWindow.WindowState = WindowState.Normal;
            documentationWindow.Activate();
        }
    }
    private int gearWheelDelta;
    private int gearWheelPreset = -1, gearWheelStage = -1;
    private void GearImageLeave(object sender, MouseEventArgs e) => gearWheelDelta = 0;
    private void GearImageWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Model.HasGallery) return;
        e.Handled = true;
        if (!Model.CanEdit) { gearWheelDelta = 0; return; }
        if (gearWheelPreset != Model.SelectedIndex || gearWheelStage != Model.SelectedStage)
        { gearWheelDelta = 0; gearWheelPreset = Model.SelectedIndex; gearWheelStage = Model.SelectedStage; }
        gearWheelDelta += e.Delta;
        int steps = gearWheelDelta / 120;
        gearWheelDelta %= 120;
        if (steps == 0) return;
        var choices = Model.IsAmp ? Model.Amps : Model.Cabinets;
        int index = Array.IndexOf(choices, Model.FocusedGear);
        int next = Math.Clamp(index - steps, 0, choices.Length - 1);
        if (next != index)
        {
            EquipmentImageButton.AnimateScrollChevron(Math.Sign(next - index));
            FocusedArt.AnimateScroll(Math.Sign(next - index));
            Model.ChooseGear(choices[next]);
        }
    }
    private void AmpCatalogClick(object sender, RoutedEventArgs e)
    {
        if (Model.Busy) return;
        Model.SelectedStage = 0;
        GalleryClick(sender, e);
    }
    private async void ShowImageSelection(GearItem target)
    {
        if (galleryOpen || Model.Busy) return;
        galleryOpen = true;
        var picker = new Views.GearImagePicker(target);
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
        {
            Title = LanguageService.T("Personalizar imagem"), Content = picker,
            CloseButtonText = "", DialogMaxWidth = 960, DialogMaxHeight = 900
        };
        var title = new DockPanel { LastChildFill = true };
        var close = new Wpf.Ui.Controls.Button
        {
            Icon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Dismiss24),
            Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent, Width = 40, Height = 40,
            Padding = new Thickness(0), ToolTip = LanguageService.T("Fechar")
        };
        System.Windows.Automation.AutomationProperties.SetName(close, LanguageService.T("Fechar"));
        DockPanel.SetDock(close, Dock.Right); title.Children.Add(close);
        title.Children.Add(new TextBlock { Text = LanguageService.T("Personalizar imagem"), VerticalAlignment = VerticalAlignment.Center });
        dialog.Title = title;
        close.Click += (_, _) => dialog.Hide(Wpf.Ui.Controls.ContentDialogResult.None);
        picker.ApplyRequested += () =>
        {
            if (picker.SelectedPath is { } path)
            {
                if (Model.ApplyGearImage(target, path)) dialog.Hide(Wpf.Ui.Controls.ContentDialogResult.Primary);
                else picker.ShowError(Model.ErrorMessage);
            }
        };
        picker.CancelRequested += () => dialog.Hide(Wpf.Ui.Controls.ContentDialogResult.None);
        System.Windows.Input.KeyEventHandler escape = (_, args) =>
        {
            if (args.Key == Key.Escape) { args.Handled = true; dialog.Hide(Wpf.Ui.Controls.ContentDialogResult.None); }
        };
        dialog.PreviewKeyDown += escape;
        void SizeSurface()
        {
            if (Controls.GalleryDialogDismissal.Surface(dialog) is not Border surface) return;
            var margin = dialog.DialogMargin;
            surface.Width = Math.Max(0, Math.Min(960, DialogHost.ActualWidth - margin.Left - margin.Right));
            surface.Height = Math.Max(0, Math.Min(750, DialogHost.ActualHeight - margin.Top - margin.Bottom));
        }
        void ResizePicker(object? sender, SizeChangedEventArgs args) => SizeSurface();
        DialogHost.SizeChanged += ResizePicker;
        dialog.Loaded += (_, _) =>
        {
            SizeSurface();
            // This picker owns its fixed actions; hide WPF UI's otherwise empty footer.
            if (dialog.Template.FindName("CloseButton", dialog) is DependencyObject nativeClose &&
                System.Windows.Media.VisualTreeHelper.GetParent(nativeClose) is Grid nativeActions &&
                System.Windows.Media.VisualTreeHelper.GetParent(nativeActions) is Border nativeFooter)
                nativeFooter.Visibility = Visibility.Collapsed;
            if (dialog.Template.FindName("PART_ContentScroll", dialog) is ScrollViewer body)
            {
                body.Padding = new Thickness(0);
                body.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                body.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                picker.SetBinding(FrameworkElement.HeightProperty, new System.Windows.Data.Binding(nameof(FrameworkElement.ActualHeight)) { Source = body });
                picker.SetBinding(FrameworkElement.WidthProperty, new System.Windows.Data.Binding(nameof(FrameworkElement.ActualWidth)) { Source = body });
            }
        };
        using var dismissal = new Controls.GalleryDialogDismissal(this, dialog);
        var previousFocus = Keyboard.FocusedElement;
        using var adaptive = Appearance.AdaptiveDialogs.Attach(dialog);
        dialog.PreviewMouseWheel += Controls.SmoothScroll.RouteWheel;
        try { await dialog.ShowAsync(); }
        finally
        {
            DialogHost.SizeChanged -= ResizePicker; dialog.PreviewKeyDown -= escape;
            dialog.PreviewMouseWheel -= Controls.SmoothScroll.RouteWheel;
            galleryOpen = false;
            if (previousFocus is UIElement { IsVisible: true, IsEnabled: true } element) element.Focus();
        }
    }
    private bool galleryOpen;
    private void EquipmentImageClick(object sender, RoutedEventArgs e)
    {
        if (Model.HasGallery) GalleryClick(sender, e);
        else if (Model.ChangeImage.CanExecute(null)) Model.ChangeImage.Execute(null);
    }

    private async void GalleryClick(object sender, RoutedEventArgs e)
    {
        if (galleryOpen || !Model.HasGallery || Model.Busy || (!Model.IsAmp && !Model.CanEdit)) return;
        galleryOpen = true;
        var gallery = new Views.GearGallery(Model.IsAmp ? Model.Amps : Model.Cabinets, Model.FocusedGear, Model.IsAmp);
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Content = gallery, PrimaryButtonText = "Aplicar seleção", CloseButtonText = "Cancelar", DialogMaxWidth = 900 };
        var title = new DockPanel { LastChildFill = true };
        var close = new Wpf.Ui.Controls.Button { Icon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Dismiss24), Width = 40, Height = 40, MinHeight = 40, BorderThickness = new Thickness(0), Padding = new Thickness(0), Margin = new Thickness(16, 0, 0, 0), Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent, ToolTip = LanguageService.T("Fechar") };
        System.Windows.Automation.AutomationProperties.SetName(close, LanguageService.T("Fechar"));
        DockPanel.SetDock(close, Dock.Right); title.Children.Add(close);
        title.Children.Add(new TextBlock { Text = LanguageService.T(Model.IsAmp ? "Escolha seu amplificador" : "Escolha seu cabinet"), VerticalAlignment = VerticalAlignment.Center });
        dialog.Title = title;
        close.Click += (_, _) => dialog.Hide(Wpf.Ui.Controls.ContentDialogResult.None);
        dialog.IsPrimaryButtonEnabled = Model.CanEdit;
        gallery.ApplyRequested += () => { if (gallery.Selected != null && Model.CanEdit) dialog.Hide(Wpf.Ui.Controls.ContentDialogResult.Primary); };
        var dismissal = new Controls.GalleryDialogDismissal(this, dialog);
        try
        {
            if (await dialog.ShowLocalizedAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary && gallery.Selected != null && Model.CanEdit)
                Model.ChooseGear(gallery.Selected);
        }
        finally { dismissal.Dispose(); galleryOpen = false; }
    }
    private void ConfigureDragDrop()
    {
        Controls.CubeDragDrop.Attach(BackingTrackList,
            info => info.SourceItem is BackingTrack track ? (track, DragDropEffects.Move) : (null, DragDropEffects.None));
        Controls.CubeDragDrop.Attach(BackingPage, over: BackingTracksDragOver, drop: BackingTracksDrop);
        Controls.CubeDragDrop.Attach(BackingStemList,
            info => BackingTracks.CanEditSession && info.SourceItem is BackingStem stem ? (stem, DragDropEffects.Move) : (null, DragDropEffects.None),
            BackingStemsDragOver, BackingStemsDrop);
        IrPacksCard.ConfigureLibraryDrag(SingleIrLibrary.Tree, () => Model.IrWorkspaceTab == 1);
    }
    private (DragDropEffects Effect, int Index, ListBoxItem? Target, bool After) EvaluatePlaylistDrop(Controls.CubeDropContext e)
    {
        if (BackingTracks.CanEditSession && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Any(IsBackingFile))
            return (DragDropEffects.Copy, -1, null, false);
        if (e.Data.GetData(typeof(BackingTrack)) is not BackingTrack track || !BackingTracks.Tracks.Contains(track))
            return (DragDropEffects.None, -1, null, false);
        Point point = e.GetPosition(BackingTrackList);
        if (point.X < 0 || point.Y < 0 || point.X > BackingTrackList.ActualWidth || point.Y > BackingTrackList.ActualHeight)
            return (DragDropEffects.None, -1, null, false);
        var target = e.OriginalSource is DependencyObject source ? ItemsControl.ContainerFromElement(BackingTrackList, source) as ListBoxItem : null;
        bool after = target != null && e.GetPosition(target).Y >= target.ActualHeight / 2;
        int index = Controls.DropInsertion.Index(BackingTracks.Tracks.IndexOf(track), target?.DataContext is BackingTrack item ? BackingTracks.Tracks.IndexOf(item) : -1, after, BackingTracks.Tracks.Count);
        return (index < 0 ? DragDropEffects.None : DragDropEffects.Move, index, target, after);
    }
    private static bool IsBackingFile(string path) => System.IO.File.Exists(path) && System.IO.Path.GetExtension(path).ToLowerInvariant() is ".mp3" or ".wav";
    private void BackingTracksDragOver(object sender, Controls.CubeDropContext e)
    {
        var result = EvaluatePlaylistDrop(e); e.Effects = result.Effect;
        BackingDropOverlay.Visibility = Visibility.Collapsed;
        UIElement target = result.Target ?? (UIElement)BackingTrackList;
        string source = e.Data.GetData(typeof(BackingTrack)) is BackingTrack track ? track.Title : LanguageService.T("Arquivos de áudio");
        string action = result.Effect == DragDropEffects.Copy ? LanguageService.T("Adicionar à playlist")
            : result.Effect == DragDropEffects.None ? LanguageService.T("Destino inválido")
            : result.Target?.DataContext is BackingTrack item ? LanguageService.T(result.After ? "Mover depois de" : "Mover antes de") + " “" + item.Title + "”"
            : LanguageService.T("Mover para o fim");
        Controls.DragFeedback.Show(target, source + " → " + action, e.GetPosition(target), result.Effect != DragDropEffects.None, result.Target == null ? null : result.After);
        if (e.OriginalSource is DependencyObject origin) Controls.DragFeedback.AutoScroll(origin, e);
        e.Handled = true;
    }
    private void BackingTracksDrop(object sender, Controls.CubeDropContext e)
    {
        var result = EvaluatePlaylistDrop(e); e.Effects = result.Effect;
        Controls.DragFeedback.Clear(); BackingDropOverlay.Visibility = Visibility.Collapsed;
        if (result.Effect == DragDropEffects.Copy && e.Data.GetData(DataFormats.FileDrop) is string[] files) { var animate = Controls.DragMotion.CaptureList(BackingTrackList); BackingTracks.AddPaths(files); animate(); }
        else if (result.Effect == DragDropEffects.Move && e.Data.GetData(typeof(BackingTrack)) is BackingTrack track) { var animate = Controls.DragMotion.CaptureList(BackingTrackList); BackingTracks.MoveTrack(track, result.Index); animate(); }
        e.Handled = true;
    }
    private (DragDropEffects Effect, int Index, ListBoxItem? Target, bool After) EvaluateStemDrop(Controls.CubeDropContext e)
    {
        if (BackingTracks.SessionStems is not { } stems) return (DragDropEffects.None, -1, null, false);
        if (BackingTracks.CanEditSession && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Any(IsBackingFile)) return (DragDropEffects.Copy, -1, null, false);
        if (!BackingTracks.CanEditSession || e.Data.GetData(typeof(BackingStem)) is not BackingStem stem || !stems.Contains(stem)) return (DragDropEffects.None, -1, null, false);
        var target = e.OriginalSource is DependencyObject source ? ItemsControl.ContainerFromElement(BackingStemList, source) as ListBoxItem : null;
        bool after = target != null && e.GetPosition(target).Y >= target.ActualHeight / 2;
        int index = Controls.DropInsertion.Index(stems.IndexOf(stem), target?.DataContext is BackingStem item ? stems.IndexOf(item) : -1, after, stems.Count);
        return (index < 0 ? DragDropEffects.None : DragDropEffects.Move, index, target, after);
    }
    private void BackingStemsDragOver(object sender, Controls.CubeDropContext e)
    {
        var result = EvaluateStemDrop(e); e.Effects = result.Effect;
        UIElement target = result.Target ?? (UIElement)BackingStemList;
        string source = e.Data.GetData(typeof(BackingStem)) is BackingStem stem ? stem.Title : LanguageService.T("Arquivos de áudio");
        string action = result.Effect == DragDropEffects.Copy ? LanguageService.T("Adicionar pistas à sessão")
            : result.Effect == DragDropEffects.None ? LanguageService.T("Destino inválido")
            : result.Target?.DataContext is BackingStem item ? LanguageService.T(result.After ? "Mover depois de" : "Mover antes de") + " “" + item.Title + "”" : LanguageService.T("Mover para o fim");
        Controls.DragFeedback.Show(target, source + " → " + action, e.GetPosition(target), result.Effect != DragDropEffects.None, result.Target == null ? null : result.After);
        if (e.OriginalSource is DependencyObject origin) Controls.DragFeedback.AutoScroll(origin, e);
        e.Handled = true;
    }
    private async void BackingStemsDrop(object sender, Controls.CubeDropContext e)
    {
        var result = EvaluateStemDrop(e); e.Effects = result.Effect; e.Handled = true; Controls.DragFeedback.Clear();
        if (result.Effect == DragDropEffects.Copy && e.Data.GetData(DataFormats.FileDrop) is string[] files) { var animate = Controls.DragMotion.CaptureList(BackingStemList); await BackingTracks.AddSessionFiles(files); animate(); }
        else if (result.Effect == DragDropEffects.Move && e.Data.GetData(typeof(BackingStem)) is BackingStem stem) { var animate = Controls.DragMotion.CaptureList(BackingStemList); BackingTracks.MoveStem(stem, result.Index); animate(); }
    }

    private FrameworkElement? backingViewportHost;
    private void BackingViewportSizeChanged(object sender, SizeChangedEventArgs e) => ApplyBackingLayout(BackingPage.ActualWidth);
    private void BackingPageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyBackingLayout(e.NewSize.Width);
        // Keep the arrangement reachable even at maximum UI zoom on a short window.
        // The transport reflows without scaling its interaction targets.
        ConfigureBackingTransport(e.NewSize.Width, e.NewSize.Height);
        ApplyBackingLayout(e.NewSize.Width);
    }
    private void BackingLibrarySizeChanged(object sender, SizeChangedEventArgs e) => ApplyBackingLayout(e.NewSize.Width);
    private void ApplyBackingLayout(double width)
    {
        if (BackingLibraryLayout == null || BackingSideColumn == null) return;
        bool visible = BackingLibraryLayout.Visibility == Visibility.Visible;
        bool wide = BackingPage.ActualWidth >= 1200;
        BackingSideColumn.Width = new GridLength(visible && wide ? 310 : 0);
        Grid.SetColumn(BackingLibraryLayout, wide ? 1 : 0);
        Grid.SetRow(BackingLibraryLayout, wide ? 1 : 0);
        Grid.SetRowSpan(BackingLibraryLayout, wide ? 1 : 2);
        BackingLibraryLayout.Width = wide ? double.NaN : Math.Min(310, Math.Max(180, BackingPage.ActualWidth));
        BackingLibraryLayout.HorizontalAlignment = wide ? HorizontalAlignment.Stretch : HorizontalAlignment.Right;
    }
    private void BackingSectionClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: BackingSection section })
        {
            BackingTracks.SelectedSection = section;
            if (BackingTracks.GoToSection.CanExecute(null)) BackingTracks.GoToSection.Execute(null);
        }
    }
    private void BackingDisableLoopClick(object sender, RoutedEventArgs e) => BackingTracks.IsLooping = false;
    private void IrAuditionLoopClick(object sender, RoutedEventArgs e) => Model.IrAudition.Loop = !Model.IrAudition.Loop;
    private void BackingLoopClick(object sender, RoutedEventArgs e) => BackingTracks.IsLooping = !BackingTracks.IsLooping;
    private void BackingReturnClick(object sender, MouseButtonEventArgs e) { Navigation.SelectedIndex = Tabs.SelectedIndex = 2; BackingTracks.PageVisible = true; e.Handled = true; }
    private void BackingTrackDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (BackingButtonAncestor(e.OriginalSource as DependencyObject) == null && e.OriginalSource is DependencyObject source && ItemsControl.ContainerFromElement(BackingTrackList, source) is ListBoxItem) { BackingTracks.PlaySelected(); e.Handled = true; }
    }
    private void BackingParameterStart(object sender, MouseButtonEventArgs e) => BackingTracks.BeginParameterGesture();
    private void BackingParameterEnd(object sender, MouseEventArgs e) => BackingTracks.EndParameterGesture();
    private async Task<bool?> ChooseBackingExportMode()
    {
        if (backingDialogOpen) return null;
        backingDialogOpen = true;
        try
        {
            var choice = new CheckBox { Content = LanguageService.T("Aplicar velocidade e tom da sessão"), IsChecked = false, Margin = new Thickness(0, 8, 0, 8) };
            var mix = new RadioButton { Content = LanguageService.T("Mixagem da sessão"), IsChecked = BackingTracks.ExportMix, GroupName = "ExportTarget", Margin = new Thickness(0, 8, 0, 0) };
            var isolated = new RadioButton { Content = LanguageService.T("Pista selecionada"), IsChecked = !BackingTracks.ExportMix, IsEnabled = BackingTracks.SelectedStem != null, GroupName = "ExportTarget" };
            var panel = new StackPanel(); panel.Children.Add(mix); panel.Children.Add(isolated); panel.Children.Add(choice);
            var whole = new RadioButton { Content = LanguageService.T("Sessão inteira"), GroupName = "ExportInterval", IsChecked = !BackingTracks.ExportInterval };
            var interval = new RadioButton { Content = LanguageService.T("Intervalo A/B"), GroupName = "ExportInterval", IsChecked = BackingTracks.ExportInterval && BackingTracks.HasRange, IsEnabled = BackingTracks.HasRange };
            panel.Children.Add(whole); panel.Children.Add(interval);
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = LanguageService.T(BackingTracks.ExportLabel), Content = panel, PrimaryButtonText = LanguageService.T("Exportar"), CloseButtonText = LanguageService.T("Cancelar") };
            if (await dialog.ShowLocalizedAsync() != Wpf.Ui.Controls.ContentDialogResult.Primary) return null;
            BackingTracks.ExportMix = mix.IsChecked == true; BackingTracks.ExportInterval = interval.IsChecked == true;
            return choice.IsChecked == true;
        }
        finally { backingDialogOpen = false; }
    }
    private void BackingKnobStart(object sender, RoutedEventArgs e) { BackingTracks.BeginParameterGesture(); e.Handled = true; }
    private void BackingKnobEnd(object sender, RoutedEventArgs e) { BackingTracks.EndParameterGesture(); e.Handled = true; }
    private void BackingSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is BackingTrack track) BackingTracks.SelectedTrack = track;
        if (BackingTrackList.SelectedItem != BackingTracks.SelectedTrack) BackingTrackList.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, BackingTracks.SelectedTrack);
    }
    private void BackingKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Handled || backingDialogOpen) return;
        if (e.Key == Key.Escape && BackingTracks.YouTubePreviewId != null)
        { BackingTracks.StopYouTubePreview(); e.Handled = true; return; }
        if (Tabs.SelectedIndex == 1) return;
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null; node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
            if (node is Controls.YouTubeResultCard || node is Controls.BackingClipLane && e.Key is Key.Left or Key.Right or Key.Escape or Key.Apps or Key.F10) return;
        bool backingPage = Tabs.SelectedIndex == 2;
        if (!backingPage && (BackingTracks.SelectedTrack == null || e.Key is not (Key.Space or Key.L or Key.Left or Key.Right or Key.Up or Key.Down))) return;
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null; node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
        {
            if (ReferenceEquals(node, BackingMetronomePanel))
            {
                if ((e.Key == Key.Apps || e.Key == Key.F10 && Keyboard.Modifiers == ModifierKeys.Shift) && OpenBackingKeyboardMenu(e.OriginalSource as DependencyObject)) e.Handled = true;
                return;
            }
            if (node is System.Windows.Controls.Primitives.TextBoxBase or ComboBox or MenuItem or Wpf.Ui.Controls.ContentDialog) return;
            if (node is Slider && e.Key is Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End or Key.PageUp or Key.PageDown) return;
            if (!backingPage && node is Controls.Knob or Slider) return;
        }
        var modifiers = Keyboard.Modifiers;
        if (backingPage && BackingStemList.IsKeyboardFocusWithin && BackingTracks.CanEditSession)
        {
            Action? edit = modifiers == ModifierKeys.Control ? e.Key switch
            {
                Key.Z => () => BackingTracks.UndoEdit(), Key.Y => () => BackingTracks.UndoEdit(true),
                Key.C => BackingTracks.CopyClips, Key.V => () => BackingTracks.PasteClips(), Key.D => () => BackingTracks.PasteClips(true), _ => null
            } : modifiers == ModifierKeys.None ? e.Key switch { Key.Delete => BackingTracks.DeleteClips, Key.S => BackingTracks.CutStudioClips, _ => null } : null;
            if (edit != null) { edit(); e.Handled = true; return; }
        }
        if (backingPage && (e.Key == Key.Apps || e.Key == Key.F10 && modifiers == ModifierKeys.Shift)
            && OpenBackingKeyboardMenu(e.OriginalSource as DependencyObject)) { e.Handled = true; return; }
        if (BackingButtonAncestor(e.OriginalSource as DependencyObject) is FrameworkElement { Tag: string menuKind } && menuKind is "track" or "wave" or "section" or "speed" or "pitch" or "player" or "metronome" && e.Key is Key.Space or Key.Enter) return;
        ICommand? command = null;
        if (BackingTrackList.IsKeyboardFocusWithin)
        {
            if (e.Key == Key.Delete && modifiers == ModifierKeys.None) command = BackingTracks.RemoveTrack;
            else if (e.Key == Key.Enter && modifiers == ModifierKeys.None) { BackingTracks.PlaySelected(); e.Handled = true; return; }
            else if (e.Key == Key.Up && modifiers == ModifierKeys.Alt) command = BackingTracks.MoveUp;
            else if (e.Key == Key.Down && modifiers == ModifierKeys.Alt) command = BackingTracks.MoveDown;
        }
        if (command == null && modifiers is ModifierKeys.None or ModifierKeys.Shift)
        {
            if (e.Key is Key.Left or Key.Right) { BackingTracks.Position += (e.Key == Key.Left ? -1 : 1) * (modifiers == ModifierKeys.Shift ? 10 : 5); e.Handled = true; return; }
            if (modifiers != ModifierKeys.None) return;
            if (e.Key is Key.Up or Key.Down) { BackingTracks.Speed += e.Key == Key.Up ? 5 : -5; e.Handled = true; return; }
            if (e.IsRepeat && e.Key is Key.Space or Key.L) { e.Handled = true; return; }
            command = e.Key switch { Key.Space => BackingTracks.PlayPause, _ => null };
            if (e.Key == Key.Home) { BackingTracks.Position = BackingTracks.IsLooping ? BackingTracks.RangeStart : 0; e.Handled = true; }
            if (e.Key == Key.L) { BackingTracks.IsLooping = !BackingTracks.IsLooping; e.Handled = true; }
        }
        if (command?.CanExecute(null) == true) { command.Execute(null); e.Handled = true; }
    }
    private async void AdvancedEffectClick(object sender, RoutedEventArgs e)
        => await Model.ToggleEffectCardAsync();

    private void CreateAmpClick(object sender, RoutedEventArgs e)
    {
        if (Model.Busy) return;
        if (AmpEditorHost.Content is not Views.AmpEditor)
            AmpEditorHost.Content = new Views.AmpEditor(Model);
        UpdateWorkspaceLayout();
        Navigation.SelectedIndex = 3;
        Dispatcher.BeginInvoke(new Action(() => ((Views.AmpEditor)AmpEditorHost.Content).ChooseBase()));
    }

    private async void MatchSettingsClick(object sender, RoutedEventArgs e)
    {
        var view = new Views.MatchSettings(Model.CreateMatchRows());
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = "Match Cabinet · associações", Content = view, PrimaryButtonText = "Salvar associações", CloseButtonText = "Cancelar", DialogMaxWidth = 680, DialogMaxHeight = 900 };
        var result = await dialog.ShowLocalizedAsync();
        if (result == Wpf.Ui.Controls.ContentDialogResult.Primary) Model.SaveMatches(view.Rows);
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T item) yield return item;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }
    private static void CheckPresentation()
    {
        var events = new List<(ParameterKey Key, int Value)>();
        var preset = new PresetViewModel(2, (key, value) => events.Add((key, value)));
        var bank = new byte[48]; bank[40] = 7; preset.Load(bank);
        if (events.Count != 0 || preset.Parameters[8].Value != 7) throw new Exception("Load must preserve value 7 without writing.");
        preset.Parameters[8].TurnModOff.Execute(null); preset.Parameters[10].Enabled = true;
        if (events.Count != 2 || events[0] != (new ParameterKey(2, 8), 8) || events[1] != (new ParameterKey(2, 10), 1)) throw new Exception("Presentation mappings failed.");
        var knob = new Controls.Knob { Maximum = 7, Value = 3 };
        knob.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(Application.Current.MainWindow), 0, Key.Right) { RoutedEvent = Keyboard.KeyDownEvent });
        if (knob.Value != 4) throw new Exception("Knob keyboard increment failed.");
    }
    private void Capture(string path)
    {
        var bitmap = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(this);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var output = File.Create(path); encoder.Save(output);
    }
}
