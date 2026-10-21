using Cube.Desktop.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Cube.WinUI.Views;

public sealed class LibraryBrowser : UserControl
{
    public ListView List { get; } = new() { Height = 250, SelectionMode = ListViewSelectionMode.Single };
    public LibraryBrowser(string name)
    {
        IsTabStop = false;
        var empty = new TextBlock { Text = LanguageService.T("Nenhum item encontrado."), Margin = new Thickness(12), TextWrapping = TextWrapping.Wrap, IsHitTestVisible = false };
        AutomationProperties.SetName(List, LanguageService.T(name));
        var panel = new Grid(); panel.Children.Add(List); panel.Children.Add(empty); Content = panel;
        void Refresh() => empty.Visibility = List.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        List.Items.VectorChanged += (_, _) => Refresh(); Loaded += (_, _) => Refresh();
    }
}
