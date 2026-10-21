using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Cube.Desktop.Localization;

namespace Cube.WinUI.Controls;

/// <summary>Shared knob/fader, label and exact input. The number binding owns conversion.</summary>
public sealed class InstrumentField : UserControl
{
    public InstrumentControl Instrument { get; }
    public NumberBox Editor { get; }
    public InstrumentField(string label, double min, double max, double step, bool fader = false)
    {
        IsTabStop = false; MinWidth = 120; MaxWidth = 200;
        var panel = new StackPanel { Spacing = 6, Padding = new Thickness(6), HorizontalAlignment = HorizontalAlignment.Stretch };
        var title = new TextBlock { Text = LanguageService.T(label), TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
        AutomationProperties.SetAccessibilityView(title, Microsoft.UI.Xaml.Automation.Peers.AccessibilityView.Raw);
        Instrument = new InstrumentControl { Minimum = min, Maximum = max, Increment = step, FineAdjustment = step < 1, IsFader = fader, HorizontalAlignment = HorizontalAlignment.Center };
        Editor = new NumberBox { Minimum = min, Maximum = max, SmallChange = step, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact, HorizontalAlignment = HorizontalAlignment.Stretch };
        AutomationProperties.SetName(Instrument, LanguageService.T(label));
        AutomationProperties.SetName(Editor, LanguageService.T("Valor exato de ") + LanguageService.T(label));
        string help = LanguageService.T("Arraste ou role para ajustar. Shift permite ajuste fino. Duplo clique restaura o ajuste inicial; Backspace também.");
        AutomationProperties.SetHelpText(Instrument, help); ToolTipService.SetToolTip(Instrument, help);
        Instrument.SetBinding(InstrumentControl.ValueProperty, new Binding { Source = Editor, Path = new PropertyPath("Value"), Mode = BindingMode.OneWay });
        Instrument.ValueEdited += (_, value) => Editor.Value = value;
        panel.Children.Add(title); panel.Children.Add(Instrument); panel.Children.Add(Editor); Content = panel;
    }
}
