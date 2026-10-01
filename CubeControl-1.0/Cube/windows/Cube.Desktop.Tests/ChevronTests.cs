using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Wpf.Ui.Controls;

internal static class ChevronTests
{
    public static async Task Run()
    {
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-chevron-" + Guid.NewGuid().ToString("N")));
        var window = new MainWindow(model) { ShowInTaskbar = false };
        window.Show();
        try
        {
            int count = 0;
            foreach (int page in new[] { 1, 2 })
            {
                ((ListBox)window.FindName("Navigation")).SelectedIndex = page;
                await Task.Delay(100); window.UpdateLayout();
                var controls = Find<Control>(window).Where(ExpanderChevron.GetEnabled).ToArray();
                if (controls.Length != (page == 1 ? 1 : 4)) throw new Exception("Missing target expanders");
                foreach (var control in controls)
                {
                    var toggle = Find<ToggleButton>(control).Single(b => b.TemplatedParent == control);
                    var peer = new ToggleButtonAutomationPeer(toggle);
                    var provider = (IToggleProvider)peer.GetPattern(PatternInterface.Toggle)!;
                    foreach (bool expanded in new[] { false, true, false, true })
                    {
                        if (toggle.IsChecked != expanded) provider.Toggle();
                        await Task.Delay(200); window.UpdateLayout();
                        bool actual = control is Expander e ? e.IsExpanded : ((CardExpander)control).IsExpanded;
                        var glyph = Find<SymbolIcon>(toggle).Single(i => i.Name == "ControlChevronIcon");
                        if (actual != expanded || glyph.Symbol != (expanded ? SymbolRegular.ChevronUp24 : SymbolRegular.ChevronDown24))
                            throw new Exception("Chevron state does not match expansion");
                    }
                    count++;
                }
            }
            Console.WriteLine($"PASS {count} expanders: down when closed, up when open; repeated native toggling");
        }
        finally { window.Close(); }
    }
    static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T result) yield return result;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }
}
