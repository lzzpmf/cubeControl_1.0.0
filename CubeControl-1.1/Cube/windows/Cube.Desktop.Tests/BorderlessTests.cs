using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Cube.Desktop.Appearance;
using Wpf.Ui.Appearance;

internal static class BorderlessTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Console.WriteLine("PASS " + message);
    }

    public static async Task Run()
    {
        ThemeService.Instance.Initialize();
        var panel = new StackPanel { Margin = new Thickness(16) };
        var list = new ListBox { Height = 100 };
        list.Items.Add("First"); list.Items.Add("Second"); panel.Children.Add(list);
        Control[] controls = [new TextBox { Text = "Editable" }, new Wpf.Ui.Controls.TextBox { Text = "Search" },
            new Wpf.Ui.Controls.NumberBox { Value = 12 }, new ComboBox { ItemsSource = new[] { "A", "B" }, SelectedIndex = 0 },
            new Wpf.Ui.Controls.CardExpander { Header = "Expand", Content = new TextBlock { Text = "Details" } },
            new Wpf.Ui.Controls.Button { Content = "Action" },
            new Expander { Header = "Capture details", Content = new TextBlock { Text = "Metadata" } }];
        foreach (var control in controls) panel.Children.Add(control);
        var window = new Window { Content = panel, Width = 480, Height = 600, ShowInTaskbar = false };
        try
        {
            window.Show();
            foreach (var theme in new[] { ApplicationTheme.Light, ApplicationTheme.Dark, ApplicationTheme.HighContrast })
            {
                ApplicationThemeManager.Apply(theme);
                window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                foreach (var control in controls)
                {
                    control.ApplyTemplate();
                    Check(control.Template != null, $"{theme}: {control.GetType().Name} template loads");
                }
                list.SelectedIndex = 0; window.UpdateLayout();
                var item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
                var marker = Descendants(item).OfType<Border>().Single(b => b.Width == 3);
                Check(marker.Visibility == Visibility.Visible, $"{theme}: selected item has a marker");
                Check(Descendants(item).OfType<Border>().All(b => b.BorderThickness == new Thickness(0)), $"{theme}: selection has no perimeter stroke");
                item.Focus(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                Check(item.IsKeyboardFocused && marker.Height == 24, $"{theme}: focus extends the existing marker");
                if (theme == ApplicationTheme.HighContrast)
                    Check(((SolidColorBrush)marker.Background).Color == SystemColors.HighlightTextColor, "selected marker contrasts with system highlight");
                item.IsEnabled = false; window.UpdateLayout();
                Check(marker.Visibility == Visibility.Collapsed, $"{theme}: disabled item hides selection/focus cue");
                item.IsEnabled = true; list.SelectedIndex = 1; controls[0].Focus(); window.UpdateLayout();
                Check(marker.Visibility == Visibility.Collapsed, $"{theme}: previous item loses its marker");
                var expander = (Wpf.Ui.Controls.CardExpander)controls[4];
                expander.IsExpanded = true; await Task.Delay(250); window.UpdateLayout();
                Check(Descendants(expander).OfType<TextBlock>().Any(t => t.Text == "Details" && t.IsVisible), "expander reveals content after animation");
                expander.IsExpanded = false; await Task.Delay(250);
                var standardExpander = (Expander)controls[6];
                foreach (bool expanded in new[] { false, true })
                {
                    standardExpander.IsExpanded = expanded; await Task.Delay(250); window.UpdateLayout();
                    Check(Descendants(standardExpander).OfType<Border>().All(b => b.BorderThickness == new Thickness(0)), $"{theme}: standard expander has no outline (expanded={expanded})");
                    Check(((TextBlock)standardExpander.Content).IsVisible == expanded, "standard expander preserves content visibility");
                }
            }
            var field = (TextBox)controls[0];
            field.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("Tag") { Source = window, Mode = System.Windows.Data.BindingMode.TwoWay });
            var binding = field.GetBindingExpression(TextBox.TextProperty)!;
            Validation.MarkInvalid(binding, new ValidationError(new ExceptionValidationRule(), binding, "Invalid value", null));
            window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(Validation.GetHasError(field) && Validation.GetErrors(field)[0].ErrorContent.Equals("Invalid value"), "validation retains its error message");
            var adorners = System.Windows.Documents.AdornerLayer.GetAdornerLayer(field)?.GetAdorners(field);
            Check(adorners != null && adorners.SelectMany(Descendants).OfType<TextBlock>().Any(t => t.Text == "Invalid value"), "validation renders its message in the real adorner layer");
            Check(!adorners!.SelectMany(Descendants).OfType<Border>().Any(b => b.BorderThickness != new Thickness(0)), "validation template does not draw an error outline");
            Validation.ClearInvalid(binding);
            Check(!Validation.GetHasError(field), "correcting validation clears the error state");
            var combo = (ComboBox)controls[3]; combo.IsDropDownOpen = true;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            combo.SelectedIndex = 1; combo.IsDropDownOpen = false;
            Check((string)combo.SelectedItem == "B", "combo popup preserves selection behavior");
            var menu = new ContextMenu { PlacementTarget = controls[0] };
            menu.Items.Add(new MenuItem { Header = "Checked", IsCheckable = true, IsChecked = true });
            menu.IsOpen = true; await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(menu.Template != null, "context menu opens with its resource template");
            menu.IsOpen = false;
            var tooltip = new ToolTip { Content = "Help", PlacementTarget = controls[0] };
            tooltip.IsOpen = true; await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(tooltip.Template != null, "tooltip opens with its resource template"); tooltip.IsOpen = false;
            var host = new Wpf.Ui.Controls.ContentDialogHost(); panel.Children.Add(host);
            var dialog = new Wpf.Ui.Controls.ContentDialog(host) { Title = "Template", Content = "Details", PrimaryButtonText = "Accept", CloseButtonText = "Cancel" };
            var pending = dialog.ShowAsync(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(dialog.Template != null, "dialog opens with its resource template");
            dialog.Hide(); await pending;
            var messageBox = new Wpf.Ui.Controls.MessageBox { Owner = window, Title = "Template", Content = "Message", CloseButtonText = "Close" };
            var messageLoaded = new TaskCompletionSource();
            messageBox.Loaded += async (_, _) =>
            {
                try
                {
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    Check(messageBox.IsVisible && messageBox.Template != null, "message box opens with its resource template");
                    var closeMessage = Descendants(messageBox).OfType<Button>().Single(b => Equals(b.Content, "Close"));
                    closeMessage.Command.Execute(closeMessage.CommandParameter);
                    messageLoaded.SetResult();
                }
                catch (Exception error) { messageLoaded.SetException(error); ((Window)messageBox).Close(); }
            };
            await messageBox.ShowDialogAsync(); await messageLoaded.Task;
            var offset = new ExpansionOffsetConverter();
            Check((double)offset.Convert([80.0, .25], typeof(double), "negative", System.Globalization.CultureInfo.InvariantCulture) == -20, "expansion animation preserves signed progress");
            var match = new DialogButtonMatchConverter();
            Check((bool)match.Convert(Wpf.Ui.Controls.ContentDialogButton.Primary, typeof(bool), Wpf.Ui.Controls.ContentDialogButton.Primary, System.Globalization.CultureInfo.InvariantCulture), "default dialog button matches");
            Check(!(bool)match.Convert(Wpf.Ui.Controls.ContentDialogButton.Primary, typeof(bool), Wpf.Ui.Controls.ContentDialogButton.Close, System.Globalization.CultureInfo.InvariantCulture), "other dialog buttons are not default");
        }
        finally { window.Close(); ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.System); }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i); yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}

