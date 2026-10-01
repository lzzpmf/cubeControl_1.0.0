using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// LiveSetting describes the region; raise a notification when its actual status changes.
public sealed class LiveStatusText : TextBlock
{
    static LiveStatusText() => TextProperty.OverrideMetadata(typeof(LiveStatusText),
        new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

    private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var control = (LiveStatusText)sender;
        if (!control.IsVisible || AutomationProperties.GetLiveSetting(control) == AutomationLiveSetting.Off
            || !AutomationPeer.ListenerExists(AutomationEvents.LiveRegionChanged)) return;
        UIElementAutomationPeer.CreatePeerForElement(control)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }
}
