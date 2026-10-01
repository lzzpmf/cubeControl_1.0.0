using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;
public partial class GearGallery : UserControl
{
    public event Action? ApplyRequested;
    private void ApplyDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton != System.Windows.Input.MouseButton.Left || e.OriginalSource is not DependencyObject source) return;
        if (ItemsControl.ContainerFromElement(Items, source) is ListBoxItem { DataContext: GearItem item })
        {
            Items.SelectedItem = item;
            e.Handled = true;
            ApplyRequested?.Invoke();
        }
    }
    private readonly GearItem[] all;
    public GearItem? Selected => Items.SelectedItem as GearItem;
    public GearGallery(GearItem[] items, GearItem current, bool amps)
    {
        all = items; InitializeComponent();
        Loaded += (_, _) => CompactDialogActions();
        Filter.ItemsSource = amps ? new[] { Localization.LanguageService.T("Todos"), "Clean", "Overdrive", "Distortion" } : new[] { Localization.LanguageService.T("Todos") };
        Filter.Visibility = amps ? Visibility.Visible : Visibility.Collapsed;
        Filter.SelectedIndex = 0; Items.SelectedItem = current;
    }
    private void CompactDialogActions()
    {
        DependencyObject? parent = this;
        while (parent != null && parent is not Wpf.Ui.Controls.ContentDialog)
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        if (parent is not Wpf.Ui.Controls.ContentDialog dialog) return;
        StabilizeDialogSize(dialog);
        // WPF UI 4.3 uses star-sized footer columns; keep its buttons and commands,
        // but make the gallery's action group size to its localized content.
        if (dialog.Template.FindName("PrimaryButton", dialog) is not Wpf.Ui.Controls.Button apply ||
            dialog.Template.FindName("CloseButton", dialog) is not Wpf.Ui.Controls.Button cancel ||
            System.Windows.Media.VisualTreeHelper.GetParent(apply) is not Grid actions) return;
        actions.HorizontalAlignment = HorizontalAlignment.Right;
        actions.ColumnDefinitions[0].Width = GridLength.Auto;
        actions.ColumnDefinitions[4].Width = GridLength.Auto;
        foreach (var button in new[] { apply, cancel })
        {
            button.MinWidth = 120;
            button.Margin = new Thickness(0);
        }
    }
    private void StabilizeDialogSize(Wpf.Ui.Controls.ContentDialog dialog)
    {
        // Size the surface from its host, never from the filtered item count.
        // Keep WPF UI's content scroller and footer inside that stable surface.
        if (System.Windows.Media.VisualTreeHelper.GetParent(dialog) is not FrameworkElement host ||
            dialog.Template.FindName("DialogRootGrid", dialog) is not Grid root ||
            root.Children.OfType<Border>().FirstOrDefault() is not Border surface) return;

        void UpdateSize()
        {
            var margin = dialog.DialogMargin;
            // Three 222-DIP tiles, dialog padding and room for the scrollbar.
            const double galleryWidth = 740;
            double width = Math.Max(0, Math.Min(Math.Min(galleryWidth, dialog.DialogMaxWidth), host.ActualWidth - margin.Left - margin.Right));
            double height = Math.Max(0, Math.Min(750, host.ActualHeight - margin.Top - margin.Bottom));
            surface.Width = surface.MaxWidth = width;
            surface.Height = surface.MaxHeight = height;
        }
        SizeChangedEventHandler resized = (_, _) => UpdateSize();
        RoutedEventHandler? unloaded = null;
        unloaded = (_, _) =>
        {
            host.SizeChanged -= resized;
            Unloaded -= unloaded;
        };
        host.SizeChanged += resized;
        Unloaded += unloaded;
        UpdateSize();
    }
    private void FilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Items == null) return;
        var selected = Selected; string? category = Filter.SelectedItem as string;
        Items.ItemsSource = Filter.SelectedIndex <= 0 ? all : all.Where(i => i.Category == category).ToArray();
        Items.SelectedItem = selected != null && Items.Items.Contains(selected) ? selected : Items.Items.Cast<GearItem>().FirstOrDefault();
    }
}
