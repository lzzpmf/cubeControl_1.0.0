using Cube.Desktop.Appearance;
using Cube.WinUI.Appearance;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.ViewManagement;

namespace Cube.WinUI.Controls;

public sealed class TransportButton : Button
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(TransportButton), new PropertyMetadata("", Changed));
    public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(TransportButton), new PropertyMetadata(false, Changed));
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public bool IsPlaying { get => (bool)GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }
    public bool IsStop { get; set; }
    private readonly Border face = new() { Width = 30, Height = 30, CornerRadius = new CornerRadius(15) };
    private readonly FontIcon glyph = new() { FontSize = 14 };
    private readonly TextBlock caption = new() { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
    public TransportButton()
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        face.Child = glyph; panel.Children.Add(face); panel.Children.Add(caption); Content = panel;
        AutomationProperties.SetAccessibilityView(panel, Microsoft.UI.Xaml.Automation.Peers.AccessibilityView.Raw);
        Loaded += (_, _) => { InstrumentAppearance.Changed += AppearanceChanged; Refresh(); };
        Unloaded += (_, _) => InstrumentAppearance.Changed -= AppearanceChanged;
        ActualThemeChanged += (_, _) => Refresh();
    }
    private static void Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((TransportButton)sender).Refresh();
    private void AppearanceChanged(object? sender, EventArgs args) => Refresh();
    private void Refresh()
    {
        caption.Text = Label; AutomationProperties.SetName(this, Label);
        glyph.Glyph = IsStop ? "\uE71A" : IsPlaying ? "\uE769" : "\uE768";
        bool contrast = new AccessibilitySettings().HighContrast;
        face.Background = contrast ? null : new ImageBrush { ImageSource = InstrumentAppearance.Face("player-face", ActualTheme), Stretch = Stretch.UniformToFill };
        if (contrast) glyph.Foreground = Foreground;
        else
        {
            string color = ControlFinishCatalog.Get(InstrumentAppearance.Finish).Glyph.TrimStart('#');
            glyph.Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, System.Convert.ToByte(color[..2], 16), System.Convert.ToByte(color[2..4], 16), System.Convert.ToByte(color[4..6], 16)));
        }
    }
}
