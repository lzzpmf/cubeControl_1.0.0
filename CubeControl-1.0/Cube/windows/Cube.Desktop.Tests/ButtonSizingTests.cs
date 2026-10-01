using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.Appearance;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class ButtonSizingTests
{
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    public static async Task Run()
    {
        var previous = ThemeService.Instance.Mode;
        string folder = Path.GetFullPath("C:/CubeControl-Build/reports/button-sizing"); Directory.CreateDirectory(folder);
        var panel = new StackPanel { Margin = new Thickness(24) };
        var buttons = new List<Button>();
        foreach (var appearance in new[] { ControlAppearance.Primary, ControlAppearance.Secondary, ControlAppearance.Transparent })
        {
            var row = new WrapPanel(); panel.Children.Add(row);
            foreach (string label in new[] { "Adicionar dispositivo", "Saiba mais", "Save associations" })
            {
                var button = new Button { Content = label, Appearance = appearance };
                if (label.StartsWith("Save")) button.Icon = new SymbolIcon(SymbolRegular.Save24) { FontSize = 16 };
                row.Children.Add(button); buttons.Add(button);
            }
        }
        var disabled = new Button { Content = "Salvar presets", IsEnabled = false }; panel.Children.Add(disabled); buttons.Add(disabled);
        var window = new Window { Content = panel, Width = 680, Height = 320, ShowInTaskbar = false };
        window.SetResourceReference(Window.BackgroundProperty, "CubeCanvas"); window.Show();
        try
        {
            foreach (string theme in new[] { "Light", "Dark", "HighContrast" })
            {
                if (theme == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                else ThemeService.Instance.Preview(Enum.Parse<ThemeMode>(theme));
                await Task.Delay(80); window.UpdateLayout();
                foreach (var button in buttons)
                {
                    Check(Math.Abs(button.ActualHeight - 40) < 1, $"Unequal command height: {button.Content}/{theme}/{button.ActualHeight}");
                    Check(button.Padding.Top == 8 && button.Padding.Bottom == 8, "Unbalanced vertical padding");
                    Check(button.VerticalContentAlignment == VerticalAlignment.Center, "Text not centered");
                    Check(button.Template == ((Style)Application.Current.FindResource("DefaultUiButtonStyle")).Setters.OfType<Setter>().Single(s => s.Property == Control.TemplateProperty).Value, "Native WPF UI template must be retained");
                }
                var image = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); image.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
                using var stream = File.Create(Path.Combine(folder, theme + ".png")); encoder.Save(stream);
            }
            ThemeService.Instance.Preview(ThemeMode.Dark);
            var message = new Wpf.Ui.Controls.MessageBox { Owner = window, Title = "Confirmation", Content = "Button layout check", PrimaryButtonText = "Confirm", CloseButtonText = "Cancel" };
            var shown = message.ShowDialogAsync(); await Task.Delay(120); message.UpdateLayout();
            try
            {
                var actions = Find<Button>(message).Where(b => b.Content is string text && (text == "Confirm" || text == "Cancel")).ToArray();
                Check(actions.Length == 2 && actions.All(b => b.ActualHeight >= 40 && b.Padding.Top >= 8 && b.Padding.Bottom >= 8), "Confirmation actions must share command sizing");
                Check(Math.Abs(actions[0].ActualHeight - actions[1].ActualHeight) < 1, "Confirmation actions must have equal heights");
            }
            finally { Find<Button>(message).Single(b => b.Content?.ToString() == "Cancel").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await shown; }
            Console.WriteLine("PASS button sizing: native templates, primary/secondary/transparent/disabled, icon and text, light/dark/high contrast, equal 40-DIP height, balanced padding and confirmation actions.");
        }
        finally { window.Close(); ThemeService.Instance.Preview(previous); }
    }
}
