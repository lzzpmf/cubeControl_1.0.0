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
    private void IrLibrarySearchLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
        {
        if (IrLibrarySearch.Template?.FindName("PART_TextBox", IrLibrarySearch) is Wpf.Ui.Controls.TextBox search)
        {
            search.IconPlacement = Wpf.Ui.Controls.ElementPlacement.Left;
            search.SetValue(Border.CornerRadiusProperty, new CornerRadius(22));
            search.MinHeight = 40;
            search.Padding = new Thickness(12, 8, 12, 8);
        }
        }));
    }

    private void IrLibraryFolderClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IrLibraryNode { IsFolder: true, IsBrand: false } node })
        {
            node.IsSelected = true;
            node.IsExpanded = !node.IsExpanded;
            e.Handled = true;
        }
    }
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        if (IrLibraryTree == null) return;
        bool clickedChevron = false;
        TreeViewItem? clickedItem = null;
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null;)
        {
            if (ReferenceEquals(node, IrLibraryTree))
            {
                if (e.ChangedButton == MouseButton.Left && clickedItem != null)
                {
                    if (clickedItem.DataContext is IrLibraryNode { IsBrand: true })
                    {
                        // Handle the whole brand row before the native chevron or selection handler.
                        // The nearest item owns the click, so descendants keep their own behavior.
                        clickedItem.Focus();
                        clickedItem.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
                        clickedItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, !clickedItem.IsExpanded);
                        e.Handled = true;
                    }
                    else if (clickedChevron)
                        clickedItem.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
                }
                return;
            }
            if (node is System.Windows.Controls.Primitives.ToggleButton) clickedChevron = true;
            if (node is TreeViewItem item) clickedItem ??= item;
            node = node is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
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
    internal MainWindow(EditorViewModel model)
    {
        Model = model;
        BackingTracks = new BackingTracksViewModel(Model.DataDirectory);
        Appearance.UiDensity.Initialize();
        InitializeComponent(); DataContext = Model;
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
            if (e.Key == Key.Enter && e.OriginalSource is TextBox box && box is not Wpf.Ui.Controls.NumberBox && !box.IsReadOnly)
            { box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource(); e.Handled = true; }
        }));
        Loaded += async (_, _) =>
        {
            Appearance.ThemeService.Instance.Attach(this);
            Appearance.ThemeService.Instance.Changed += RefreshThemeColors;
            await Model.Initialize();
            await BackingTracks.Initialize();
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
            if (closing) return;
            e.Cancel = true;
            if (Model.Busy) { await ShowNoticeAsync("Aguarde a operação terminar antes de fechar."); return; }
            if (Model.Dirty && !await Model.Ask("Há ajustes aplicados que não foram salvos. Fechar o editor?")) return;
            try { await Model.Flush(); await Model.EndPreviewAsync(); }
            catch (Exception ex)
            {
                if (!await Model.Ask(ex.Message + "\nFechar mesmo assim? Os backups permanecem no computador.")) return;
            }
            closing = true; Appearance.ThemeService.Instance.Detach(); Model.Dispose(); _ = Dispatcher.BeginInvoke(new Action(Close));
        };
    }
    private bool noticeOpen;
    internal async Task ShowNoticeAsync(string message)
    {
        if (noticeOpen) return;
        noticeOpen = true;
        var previousFocus = Keyboard.FocusedElement;
        try
        {
            var dialog = new Wpf.Ui.Controls.MessageBox { PrimaryButtonIcon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Checkmark24, FontSize = 16 }, CloseButtonIcon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Dismiss24, FontSize = 16 },
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

    private void NavigationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Tabs != null && e.Source == Navigation && Navigation.SelectedIndex >= 0)
        {
            Tabs.SelectedIndex = Navigation.SelectedIndex;
        }
    }
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
            Controls.SurfaceMotion.SetOffset(surface, 4);
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
            Icon = new Wpf.Ui.Controls.SymbolIcon { Symbol = symbol, FontSize = 20 }
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
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateZoomMenu(AlignMenu));
        // WPF UI 4.3 names its submenu popup "Popup". Open toward the workspace,
        // since this menu is anchored at the window's right edge.
        foreach (var submenu in new[] { themeMenu, languageMenu })
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
        MoveFooterToPage();
    }
    private void MoveFooterToPage()
    {
        if (FooterActions == null || Tabs == null) return;
        var page = Tabs.SelectedIndex switch { 1 => IrScroll, 2 => HelpScroll, 3 => BackingTracksScroll, 4 => null, _ => EditorScroll };
        if (page?.Content is not StackPanel content || FooterActions.Parent == content) return;
        if (FooterActions.Parent is Panel parent) parent.Children.Remove(FooterActions);
        content.Children.Add(FooterActions);
    }
    private int chainWheelDelta;
    private void EditorPageWheel(object sender, MouseWheelEventArgs e)
    {
        // Preserve controls whose wheel has a musical/navigation meaning.
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null && node != EditorScroll;)
        {
            if (node is Controls.Knob { IsEnabled: true }
                || (node == EffectChain && Model.HasBank)
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
        Grid.SetRow(PresetStateText, 2);
        Grid.SetColumnSpan(OutputGroup, 1);
        bool narrow = EditHeaderGrid.ActualWidth < 440;
        if (narrow)
        {
            Grid.SetColumnSpan(OutputGroup, 3);
            OutputGroup.HorizontalAlignment = HorizontalAlignment.Center;
            OutputGroup.Margin = new Thickness(0, 16, 0, 0);
            Grid.SetRow(SaveGroup, 2); Grid.SetColumn(SaveGroup, 0); Grid.SetColumnSpan(SaveGroup, 3);
            SaveGroup.Margin = new Thickness(0, 16, 0, 0);
            Grid.SetRow(PresetStateText, 3); Grid.SetRow(FootswitchHint, 4);
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
            if (shortViewport) item.Padding = new Thickness(10, 6, 10, 6);
            else item.ClearValue(Control.PaddingProperty);
        }
        HeaderGrid.Margin = new Thickness(0, 0, 0, shortViewport ? 8 : 20);
        bool compact = ClientArea.ActualHeight < 764 || Workspace.ActualWidth < 1000;
        WorkspaceScroll.VerticalScrollBarVisibility = compact ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        // A single scrolling owner: natural content height in compact mode, a
        // constrained page viewport in desktop mode. Never force a workspace height.
        Workspace.Height = double.NaN;
        if (AmpEditorHost != null) AmpEditorHost.Height = compact ? Math.Max(420, ClientArea.ActualHeight - HeaderGrid.ActualHeight - 80) : double.NaN;
        foreach (var scroll in new[] { EditorScroll, IrScroll, HelpScroll, BackingTracksScroll })
            scroll.VerticalScrollBarVisibility = compact ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
        FocusedArt.MaxHeight = ClientArea.ActualHeight < 764 ? 160 : 240;
        double margin = ClientArea.ActualWidth < 1000 ? 16 : 24;
        Workspace.Margin = new Thickness(margin, shortViewport ? 12 : 20, margin, 12);
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
            notification.Appearance, new Wpf.Ui.Controls.SymbolIcon(symbol), notification.Duration);
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
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "Cube-Baby-M-Vave-Guitar-Pedal-User-Manual.pdf");
            if (!File.Exists(path)) throw new FileNotFoundException();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception error) when (error is IOException or System.ComponentModel.Win32Exception or UnauthorizedAccessException)
        {
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
            {
                Title = "Manual do usuário",
                Content = "Não foi possível abrir o manual. Verifique se o PDF está na pasta Assets do aplicativo e se há um leitor de PDF instalado.",
                CloseButtonText = "Fechar"
            };
            await dialog.ShowLocalizedAsync();
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
    private bool galleryOpen;
    private async void GalleryClick(object sender, RoutedEventArgs e)
    {
        if (galleryOpen || !Model.HasGallery || Model.Busy || (!Model.IsAmp && !Model.CanEdit)) return;
        galleryOpen = true;
        var gallery = new Views.GearGallery(Model.IsAmp ? Model.Amps : Model.Cabinets, Model.FocusedGear, Model.IsAmp);
        var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Content = gallery, PrimaryButtonText = "Aplicar seleção", CloseButtonText = "Cancelar", DialogMaxWidth = 900 };
        var title = new DockPanel { LastChildFill = true };
        var close = new Wpf.Ui.Controls.Button { Icon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Dismiss24, FontSize = 16 }, Width = 40, Height = 40, MinHeight = 40, BorderThickness = new Thickness(0), Padding = new Thickness(0), Margin = new Thickness(16, 0, 0, 0), Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent, ToolTip = LanguageService.T("Fechar") };
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
    private Point backingDragStart;
    private BackingTrack? backingDragTrack;
    private void BackingTrackDragStart(object sender, MouseButtonEventArgs e)
    {
        backingDragStart = e.GetPosition(BackingTrackList);
        backingDragTrack = e.OriginalSource is DependencyObject source && ItemsControl.ContainerFromElement(BackingTrackList, source) is ListBoxItem item ? item.DataContext as BackingTrack : null;
    }
    private void BackingTrackDragMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || backingDragTrack == null) return;
        var delta = e.GetPosition(BackingTrackList) - backingDragStart;
        if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        var track = backingDragTrack; backingDragTrack = null;
        DragDrop.DoDragDrop(BackingTrackList, new DataObject(typeof(BackingTrack), track), DragDropEffects.Move);
    }
    private void BackingTracksDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : e.Data.GetDataPresent(typeof(BackingTrack)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }
    private void BackingTracksDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files) BackingTracks.AddPaths(files);
        else if (e.Data.GetData(typeof(BackingTrack)) is BackingTrack track)
        {
            int index = BackingTracks.Tracks.Count - 1;
            if (e.OriginalSource is DependencyObject source && ItemsControl.ContainerFromElement(BackingTrackList, source) is ListBoxItem item && item.DataContext is BackingTrack target)
                index = BackingTracks.Tracks.IndexOf(target);
            BackingTracks.MoveTrack(track, index);
        }
        e.Handled = true;
    }
    private void CreateAmpClick(object sender, RoutedEventArgs e)
    {
        if (Model.Busy) return;
        if (AmpEditorHost.Content is not Views.AmpEditor)
            AmpEditorHost.Content = new Views.AmpEditor(Model);
        Navigation.SelectedIndex = 4;
        Dispatcher.BeginInvoke(new Action(() => ((Views.AmpEditor)AmpEditorHost.Content).ChooseBase()));
    }
    private void PedalboardActionsSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.Primitives.UniformGrid grid)
            grid.Columns = grid.ActualWidth >= 1000 ? 4 : grid.ActualWidth >= 520 ? 2 : 1;
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
