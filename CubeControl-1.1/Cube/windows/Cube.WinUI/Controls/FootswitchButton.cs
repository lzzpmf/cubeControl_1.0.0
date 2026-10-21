using Cube.Desktop.Localization;
using Cube.WinUI.Appearance;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.UI.ViewManagement;

namespace Cube.WinUI.Controls;

public sealed class FootswitchButton : ToggleButton
{
    private readonly string label;
    private readonly Image image = new() { Width = 42, Height = 42, Stretch = Stretch.Uniform };
    private readonly TextBlock state = new() { TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center };
    public FootswitchButton(string label)
    {
        this.label = label; HorizontalAlignment = HorizontalAlignment.Stretch;
        var panel = new StackPanel { Spacing = 6 }; panel.Children.Add(image); panel.Children.Add(state); Content = panel;
        AutomationProperties.SetAccessibilityView(image, AccessibilityView.Raw); AutomationProperties.SetAccessibilityView(state, AccessibilityView.Raw);
        Loaded += (_, _) => { InstrumentAppearance.Changed += AppearanceChanged; Refresh(); };
        Unloaded += (_, _) => InstrumentAppearance.Changed -= AppearanceChanged;
        ActualThemeChanged += (_, _) => Refresh(); Checked += (_, _) => Refresh(); Unchecked += (_, _) => Refresh();
        StudioMotion.Attach(this); Refresh();
    }
    private void AppearanceChanged(object? sender, EventArgs args) => Refresh();
    private void Refresh()
    {
        image.Visibility = new AccessibilitySettings().HighContrast ? Visibility.Collapsed : Visibility.Visible;
        image.Source = InstrumentAppearance.Face("footswitch", ActualTheme);
        state.Text = LanguageService.T(label) + " · " + LanguageService.T(IsChecked == true ? "Ligado" : "Desligado");
        AutomationProperties.SetName(this, state.Text);
    }
}
