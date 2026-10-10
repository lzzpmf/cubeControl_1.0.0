using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;

namespace Cube.Desktop;

public partial class MainWindow
{
    private void InitializeZoom()
    {
        ApplyZoom();
        ZoomService.Instance.Changed += ApplyZoom;
        Closed += (_, _) => ZoomService.Instance.Changed -= ApplyZoom;
        PreviewKeyDown += ZoomKeyDown;
    }
    private void ApplyZoom()
    {
        foreach (var scroll in new[] { WorkspaceScroll, EditorScroll, IrScroll, HelpScroll }) Controls.SmoothScroll.Stop(scroll);
        ClientArea.LayoutTransform = new ScaleTransform(UiDensity.Factor, UiDensity.Factor);
        if (MoreOptionsButton.ContextMenu is { IsOpen: true } menu)
            menu.LayoutTransform = new ScaleTransform(UiDensity.Factor, UiDensity.Factor);
        UpdateWorkspaceLayout();
    }
    internal Action<int> SelectZoom { get; set; } = ZoomService.Instance.Select;
    private async void ChangeZoom(int percent)
    {
        try { SelectZoom(percent); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { await ShowNoticeAsync(LanguageService.T("Não foi possível salvar as preferências locais: ") + ex.Message); }
    }
    private void ZoomKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        int? value = e.Key switch
        {
            Key.Add or Key.OemPlus => ZoomService.Instance.Percent + 10,
            Key.Subtract or Key.OemMinus => ZoomService.Instance.Percent - 10,
            Key.D0 or Key.NumPad0 => 100,
            _ => null
        };
        if (value is null) return;
        e.Handled = true;
        ChangeZoom(value.Value);
    }
    private MenuItem CreateZoomMenu(Func<Task> reposition)
    {
        var row = new Grid { MinWidth = 240 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (int i = 0; i < 4; i++) row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock { Text = LanguageService.T("Zoom"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) });
        Wpf.Ui.Controls.Button Button(string name, string hint, Wpf.Ui.Controls.SymbolRegular symbol, int column)
        {
            var button = new Wpf.Ui.Controls.Button
            {
                Name = name, Width = 40, Height = 40, Padding = new Thickness(0), Margin = new Thickness(0),
                Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent,
                Icon = Cube.Desktop.Appearance.AppIcons.Create(symbol, navigation: true), ToolTip = LanguageService.T(hint)
            };
            AutomationProperties.SetName(button, LanguageService.T(hint));
            Grid.SetColumn(button, column); row.Children.Add(button); return button;
        }
        var less = Button("ZoomOutButton", "Diminuir zoom", Wpf.Ui.Controls.SymbolRegular.Subtract24, 1);
        var label = new TextBlock { Name = "ZoomPercent", MinWidth = 56, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        AutomationProperties.SetLiveSetting(label, AutomationLiveSetting.Polite);
        Grid.SetColumn(label, 2); row.Children.Add(label);
        var more = Button("ZoomInButton", "Aumentar zoom", Wpf.Ui.Controls.SymbolRegular.Add24, 3);
        var reset = Button("ZoomResetButton", "Restaurar zoom para 100%", Wpf.Ui.Controls.SymbolRegular.ArrowReset24, 4);
        void Refresh()
        {
            label.Text = $"{ZoomService.Instance.Percent}%";
            less.IsEnabled = ZoomService.Instance.Percent > 50;
            more.IsEnabled = ZoomService.Instance.Percent < 200;
            reset.IsEnabled = ZoomService.Instance.Percent != 100;
        }
        async void Set(int percent) { ChangeZoom(percent); Refresh(); await reposition(); }
        less.Click += (_, e) => { e.Handled = true; Set(ZoomService.Instance.Percent - 10); };
        more.Click += (_, e) => { e.Handled = true; Set(ZoomService.Instance.Percent + 10); };
        reset.Click += (_, e) => { e.Handled = true; Set(100); };
        Refresh();
        var item = new MenuItem { Name = "ZoomMenuRow", Header = row, StaysOpenOnClick = true, Focusable = true, Padding = new Thickness(12, 4, 12, 4) };
        // A content-only row avoids the native submenu arrow and keeps three real keyboard-accessible buttons.
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding("Header") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        presenter.SetValue(FrameworkElement.MarginProperty, new Thickness(12, 4, 12, 4));
        item.Template = new ControlTemplate(typeof(MenuItem)) { VisualTree = presenter };
        KeyboardNavigation.SetTabNavigation(item, KeyboardNavigationMode.Cycle);
        item.GotKeyboardFocus += (_, e) =>
        {
            if (ReferenceEquals(e.NewFocus, item)) (less.IsEnabled ? less : more).Focus();
        };
        item.Loaded += (_, _) => ZoomService.Instance.Changed += Refresh;
        item.Unloaded += (_, _) => ZoomService.Instance.Changed -= Refresh;
        return item;
    }
}
