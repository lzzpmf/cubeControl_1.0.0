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
    public Cube.Desktop.Studio.ViewModels.ShellViewModel Studio { get; }
    private Panel? footerHome;
    private bool studioCompactNavigation;
    private bool closing;
    private bool closeInProgress;
    private readonly bool automaticConnectionEnabled;
    private readonly Wpf.Ui.SnackbarService snackbarService = new();
    private IInputElement? operationFocus;
    private bool restoringOperationFocus;
    private readonly System.Windows.Threading.DispatcherTimer busyMessageTimer = new() { Interval = TimeSpan.FromSeconds(4) };
    private static readonly string[] BusyMessages = ["Preparando o palco…", "Nos bastidores do seu som…", "Dando passagem ao seu timbre…", "Afinando os detalhes…", "Seu som vem aí…"];
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
        Studio = new Cube.Desktop.Studio.ViewModels.ShellViewModel(Model.DataDirectory, BackingTracks);
        Model.IrAudition.BeforePlayback = Studio.BeforeIrAuditionAsync;
        Model.IrAudition.PlaybackAllowed = () => Studio.CanAuditionIr;
        Studio.BeforePlayback = Model.IrAudition.StopAsync;
        BackingTracks.BeforePlayback = async () =>
        {
            if (!Studio.CanPreviewPlayback) throw new InvalidOperationException(LanguageService.T("Finalize a gravação antes de iniciar uma prévia."));
            await Model.IrAudition.StopAsync();
            await Studio.BeforeYouTubePreviewAsync();
        };
        PreviewKeyDown += SuppressStandaloneAltDown;
        PreviewKeyUp += SuppressStandaloneAltUp;
        Deactivated += (_, _) => standaloneAltPressed = false;
        PreviewKeyDown += BackingKeyDown;
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.F6) return;
            if (Navigation.IsKeyboardFocusWithin && Tabs.SelectedContent is UIElement page)
                page.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            else if (Navigation.SelectedItem is ListBoxItem item) item.Focus();
            e.Handled = true;
        };
        Deactivated += (_, _) => Studio.StopPreview();
        Appearance.UiDensity.Initialize();
        InitializeComponent();
        Studio.DialogHost = DialogHost; Studio.Owner = this;
        Studio.NavigateToStudio = () => { Tabs.SelectedIndex = 2; Navigation.SelectedIndex = 2; BackingView.Focus(); };
        Studio.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(Studio.CanAuditionIr)) CommandManager.InvalidateRequerySuggested(); };
        footerHome = FooterActions.Parent as Panel;
        ConfigureDragDrop();
        RegisterName("IrLibraryTree", SingleIrLibrary.Tree);
        RegisterName("IrLibrarySearch", SingleIrLibrary.SearchBox); DataContext = Model;
        Model.ConfirmIrWrite = ConfirmIrWriteAsync;
        InitializeZoom();
        MoveFooterToPage();
        IrWorkspaceTabs.PreviewMouseLeftButtonDown += (_, e) =>
        {
            if (e.OriginalSource is not DependencyObject source ||
                ItemsControl.ContainerFromElement(IrWorkspaceTabs, source) is not TabItem) return;

            // Tab selection can restore focus inside the new panel. Keep that
            // deferred BringIntoView from moving the page after a header click.
            suppressIrTabBringIntoView = true;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(() => suppressIrTabBringIntoView = false));
        };
        IrWorkspaceTabs.RequestBringIntoView += (_, e) =>
        {
            if (suppressIrTabBringIntoView) e.Handled = true;
        };
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
        Closed += (_, _) =>
        {
            try { Studio.Dispose(); BackingTracks.Dispose(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            busyMessageTimer.Stop(); Model.PropertyChanged -= BusyStateChanged;
        };
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
            if (initializeDevices)
            {
                await Model.Initialize();
                await BackingTracks.Initialize();
                await Studio.InitializeAsync();
                await Model.IrAudition.RefreshDevices();
            }
            if (automaticConnectionEnabled && !Environment.GetCommandLineArgs().Contains("--offline")) Model.StartAutomaticConnection();
        };
        Closing += async (_, e) =>
        {
            if (closing) return;
            e.Cancel = true;
            if (closeInProgress) return;
            closeInProgress = true;
            try
            {
                BackingView.FinishEdits();
                Studio.ClosePending = true;
                if (!await Studio.CanCloseAsync()) return;
                if (!await Model.Ask("Fechar o CubeControl? Alterações não salvas serão descartadas.")) return;
                IsEnabled = false;
                // Closing must not wait indefinitely for a disconnected or busy pedal.
                using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                async Task RestoreForExit()
                {
                    await Model.IrAudition.StopAsync();
                    if (Model.Busy) return;
                    Func<Task>[] steps = [Model.Flush, Model.EndDelayPreviewAsync, Model.EndReverbPreviewAsync,
                        Model.EndChorusPreviewAsync, Model.EndPhaserPreviewAsync,
                        async () => { await Model.RestoreAmpAsync(); }, Model.EndPreviewAsync];
                    foreach (var step in steps)
                    {
                        stop.Token.ThrowIfCancellationRequested();
                        await step();
                    }
                }
                try { await RestoreForExit().WaitAsync(stop.Token); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
                closing = true;
                try { Appearance.ThemeService.Instance.Detach(); Model.Dispose(); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    Close();
                    Application.Current.Shutdown();
                }));
            }
            catch (Exception ex)
            {
                IsEnabled = true;
                System.Diagnostics.Debug.WriteLine(ex);
                ShowNotification(new UserNotification("Não foi possível finalizar o áudio", ex.Message, Wpf.Ui.Controls.ControlAppearance.Danger));
            }
            finally { closeInProgress = false; if (!closing) Studio.ClosePending = false; }
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
    private void NavigationLeave(object sender, MouseEventArgs e) => navigationWheelDelta = 0;
    private void NavigationWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        if (Navigation.Items.Count == 0 || !Navigation.IsEnabled)
        { navigationWheelDelta = 0; return; }
        navigationWheelDelta += e.Delta;
        int steps = navigationWheelDelta / 120;
        navigationWheelDelta %= 120;
        if (steps == 0) return;
        Navigation.SelectedIndex = Math.Clamp(Navigation.SelectedIndex - steps, 0, Navigation.Items.Count - 1);
        Navigation.ScrollIntoView(Navigation.SelectedItem);
    }
    private void NavigationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Tabs == null || e.Source != Navigation || Navigation.SelectedIndex < 0) return;
        int previous = Tabs.SelectedIndex;
        if (previous == Navigation.SelectedIndex) return;
        Tabs.SelectedIndex = Navigation.SelectedIndex;
        if (Tabs.SelectedContent is FrameworkElement page)
        {
            Controls.SurfaceMotion.SetHorizontal(page, true);
            Controls.SurfaceMotion.SetOffset(page, Navigation.SelectedIndex >= previous ? 12 : -12);
            Controls.SurfaceMotion.Replay(page);
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
        Studio.PageVisible = Tabs.SelectedIndex == 2;
        Model.IrAudition.PanelVisible = Tabs.SelectedIndex == 1;
        UpdateWorkspaceLayout();
        MoveFooterToPage();
    }
    private void MoveFooterToPage()
    {
        if (FooterActions == null || Tabs == null) return;
        if (Tabs.SelectedIndex == 2)
        {
            if (footerHome != null && FooterActions.Parent != footerHome)
            { if (FooterActions.Parent is Panel old) old.Children.Remove(FooterActions); footerHome.Children.Add(FooterActions); }
            return;
        }
        var page = Tabs.SelectedIndex switch { 1 => IrScroll, 3 => null, 4 => HelpScroll, _ => EditorScroll };
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
        if (Tabs.SelectedIndex == 2) return;
        // Preserve controls whose wheel has a musical/navigation meaning.
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null && node != EditorScroll;)
        {
            if (node is Controls.AmpEqGraph eq && eq.FilterAt(e.GetPosition(eq)) >= 0) return;
            if (node is Controls.BackingFader { IsEnabled: true } or Controls.BackingWaveform { IsOverview: false }
                || node is Controls.Knob { IsEnabled: true }
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
        Grid.SetRow(NavigationHeader, studioCompactNavigation ? 0 : compact ? 1 : 0);
        Grid.SetColumnSpan(NavigationHeader, studioCompactNavigation ? 1 : compact ? 3 : 1);
        NavigationHeader.Margin = new Thickness(0, !studioCompactNavigation && compact ? 12 : 0, 0, 0);
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
        bool backingCompact = Tabs.SelectedIndex == 2 && (ClientArea.ActualHeight < 620 || Workspace.ActualWidth < 900);
        if (studioCompactNavigation != backingCompact)
        {
            studioCompactNavigation = backingCompact;
            Navigation.ItemsPanel = (ItemsPanelTemplate)FindResource(backingCompact ? "StudioCompactNavigationPanel" : "CubeNavigationWrapPanel");
            ScrollViewer.SetHorizontalScrollBarVisibility(Navigation, backingCompact ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Disabled);
        }
        Grid.SetRow(NavigationHeader, backingCompact ? 0 : HeaderGrid.ActualWidth < 1000 ? 1 : 0);
        Grid.SetColumnSpan(NavigationHeader, backingCompact ? 1 : HeaderGrid.ActualWidth < 1000 ? 3 : 1);
        NavigationHeader.Margin = new Thickness(0, !backingCompact && HeaderGrid.ActualWidth < 1000 ? 12 : 0, 0, 0);
        bool shortViewport = ClientArea.ActualHeight < 400 || backingCompact;
        ConnectionGroup.Visibility = backingCompact ? Visibility.Collapsed : Visibility.Visible;
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
        foreach (var scroll in new[] { EditorScroll, IrScroll, HelpScroll })
            scroll.VerticalScrollBarVisibility = compact ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Hidden;
        FocusedArt.MaxHeight = ClientArea.ActualHeight < 764 ? 160 : 240;
        double margin = ClientArea.ActualWidth < 1000 ? 16 : 24;
        Workspace.Margin = new Thickness(margin, shortViewport ? 12 : 20, margin, 12);
        pageScrollOwner = Tabs.SelectedIndex == 2 ? null : compact ? WorkspaceScroll
            : Tabs.SelectedIndex switch { 1 => IrScroll, 4 => HelpScroll, _ => EditorScroll };
        SyncWindowPageScroll();
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(SyncWindowPageScroll));
    }
    private void HelpSectionExpanded(object sender, RoutedEventArgs e)
    {
        // Ignore expansion events bubbling up from controls inside a section.
        if (!ReferenceEquals(sender, e.OriginalSource) ||
            sender is not Wpf.Ui.Controls.CardExpander { Parent: Panel panel } selected) return;
        foreach (var section in panel.Children.OfType<Wpf.Ui.Controls.CardExpander>())
            if (!ReferenceEquals(section, selected))
                section.SetCurrentValue(Expander.IsExpandedProperty, false);
    }

    private bool suppressIrTabBringIntoView;
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
        if (closing || notification.Appearance == Wpf.Ui.Controls.ControlAppearance.Danger && Model.HasError) return;
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
    private Appearance.DialogService Dialogs => new(DialogHost);
    private async void ConnectionClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem)
        {
            MoreOptionsButton.ContextMenu.IsOpen = false;
            MoreOptionsButton.Focus();
        }
        await Dialogs.ShowAsync("Conexão USB", new Views.ConnectionDetails { DataContext = Model });
    }
    private async void UserManualClick(object sender, RoutedEventArgs e)
    {
        if (Documentation.ManualLauncher.TryOpen(AssetPaths.Documents, LanguageService.Instance.Language,
            Appearance.ThemeService.Instance.IsLight, out string error)) return;
        await Dialogs.ShowAsync("Manual do cubeControl", error);
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
        int ampDestination = Model.SelectedAmp.Value;
        var gallery = new Views.GearGallery(Model.IsAmp ? Model.AmpGalleryItems : Model.Cabinets, Model.FocusedGear, Model.IsAmp);
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
            {
                if (gallery.Selected.UploadPreset is { } preset)
                {
                    if (!Model.CanUseAmp) return;
                    Model.Amp.Load(preset.Build(), preset.Name);
                    Model.Amp.Slot = ampDestination;
                    await Model.WriteAmpAsync();
                }
                else Model.ChooseGear(gallery.Selected);
            }
        }
        finally { dismissal.Dispose(); galleryOpen = false; }
    }
    private void ConfigureDragDrop()
    {
        IrPacksCard.ConfigureLibraryDrag(SingleIrLibrary.Tree, () => Model.IrWorkspaceTab == 1);
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
}
