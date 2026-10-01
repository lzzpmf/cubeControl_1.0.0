using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class DensityTests
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
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
        Console.WriteLine("PASS " + label);
    }
    private static double Scale(Visual visual, Visual root) =>
        (visual.TransformToAncestor(root).Transform(new Point(100, 0)) - visual.TransformToAncestor(root).Transform(new Point())).Length / 100;
    private static double PopupScale(Visual visual) => Scale(visual, PresentationSource.FromVisual(visual).RootVisual);
    public static async Task Run()
    {
        string temporary = Path.Combine(Path.GetTempPath(), "cube-density-" + Guid.NewGuid().ToString("N"));
        string folder = Path.GetFullPath("C:/CubeControl-Build/reports/density-3.3.22"); Directory.CreateDirectory(folder);
        using var fake = new FakeConnection();
        fake.Bank[0] = 2; fake.Bank[1] = 4; fake.Bank[2] = 8; fake.Bank[8] = 8;
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(temporary);
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model) { ShowInTaskbar = false };
        Check(window.Width == 1280 && window.Height == 720, "default requested window is 1280x720");
        Check(WindowWorkArea.Fit(new Size(1280,720), new Size(1200,672)) == new Size(1200,672), "initial window respects smaller usable monitor area");
        window.Show(); await Task.Delay(150); while (model.Busy) await Task.Delay(30);
        var tabs = (TabControl)window.FindName("Tabs");
        var client = (Grid)window.FindName("ClientArea");
        var outer = (ScrollViewer)window.FindName("WorkspaceScroll");
        var knob = (FrameworkElement)window.FindName("PresetVolumeKnob");
        var host = (Wpf.Ui.Controls.ContentDialogHost)window.FindName("DialogHost");
        async Task Settle() { window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
        void Capture(string name, Visual? source = null)
        {
            var root = source ?? window;
            var bounds = root is FrameworkElement element ? new Size(element.ActualWidth, element.ActualHeight) : VisualTreeHelper.GetDescendantBounds(root).Size;
            var bitmap = new RenderTargetBitmap(Math.Max(1,(int)Math.Ceiling(bounds.Width)), Math.Max(1,(int)Math.Ceiling(bounds.Height)), 96,96, PixelFormats.Pbgra32);
            bitmap.Render(root); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(folder,name+".png")); encoder.Save(file);
        }
        try
        {
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            foreach (string language in new[] { "pt-BR", "en-US" })
            {
                ThemeService.Instance.Preview(theme); LanguageService.Instance.Preview(language);
                window.Width = 1280; window.Height = 720;
                for (int tab=0; tab<3; tab++)
                {
                    tabs.SelectedIndex=tab; outer.ScrollToTop(); await Settle();
                    Check(Math.Abs(Scale(knob,window)-.9)<.001, $"single 90% client scale {theme}/{language}/{tab}");
                    Check(outer.ExtentWidth<=outer.ViewportWidth+1,"no horizontal overflow");
                    Capture($"{theme}-{language}-tab{tab}");
                }
            }
            tabs.SelectedIndex=0; outer.ScrollToTop(); await Settle();
            Check(Math.Abs(knob.ActualWidth * Scale(knob,window)-93.6)<.1,"main volume renders at 93.6 DIPs");
            var title = Find<Wpf.Ui.Controls.TitleBar>(window).First();
            Check(Math.Abs(Scale(title,window)-1)<.001,"title bar remains unscaled");
            var presets=(ListBox)window.FindName("PresetSelector");
            presets.SelectedIndex=1; await Settle();
            Check(model.SelectedIndex==1 && fake.Writes==0,"preset selection retains binding without MIDI edits");
            var pending = new TaskCompletionSource(); var operation=model.Execute(()=>pending.Task); await Settle();
            var busy=(Border)window.FindName("BusyOverlay");
            Check(busy.IsVisible && Math.Abs(Scale(busy,window)-.9)<.001,"busy overlay inherits exactly one scale");
            pending.SetResult(); await operation;

            var menu = new ContextMenu { PlacementTarget=knob };
            var parent = new MenuItem { Header="Options" }; parent.Items.Add(new MenuItem { Header="Nested option" }); menu.Items.Add(parent);
            for (int opening=0;opening<2;opening++)
            {
                menu.IsOpen=true; await Settle();
                Check(Math.Abs(PopupScale(parent)-.9)<.001,"context menu is 90% after open/reopen");
                parent.IsSubmenuOpen=true; await Settle();
                var nested=(MenuItem)parent.Items[0];
                Check(Math.Abs(PopupScale(nested)-.9)<.001,"submenu inherits 90%, not 81%");
                if(opening==0) Capture("context-menu",PresentationSource.FromVisual(parent).RootVisual);
                parent.IsSubmenuOpen=false; menu.IsOpen=false;
            }
            var tooltip=new ToolTip { Content="Volume", PlacementTarget=knob };
            for(int opening=0;opening<2;opening++)
            {
                tooltip.IsOpen=true; await Settle();
                Check(Math.Abs(PopupScale(tooltip)-.9)<.001,"tooltip is 90% after open/reopen");
                tooltip.IsOpen=false;
            }
            tabs.SelectedIndex=1; outer.ScrollToTop(); await Settle();
            var combo=Find<ComboBox>(tabs).Single(c => c.ItemsSource == model.Ir.Slots); combo.BringIntoView(); await Settle();
            combo.IsDropDownOpen=true; await Settle();
            var popup=Find<Popup>(combo).Single(p => p.IsOpen);
            Check(Math.Abs(PopupScale(popup.Child)-.9)<.001,$"ComboBox popup inherits exactly 90% (actual {PopupScale(popup.Child)})");
            combo.SelectedIndex=2; combo.IsDropDownOpen=false;
            Check(model.Ir.Slot==2,"dropdown selection retains binding");
            var gallery=new Cube.Desktop.Views.GearGallery(model.Amps,model.SelectedAmp,true);
            var dialog=new Wpf.Ui.Controls.ContentDialog(host) { Title="Gallery", Content=gallery, PrimaryButtonText="Apply", CloseButtonText="Cancel", DialogMaxWidth=900 };
            var shown=dialog.ShowAsync(); await Task.Delay(200); await Settle();
            Check(Math.Abs(Scale(gallery,window)-.9)<.001,"internal dialog inherits exactly 90%");
            var filter=(ListBox)gallery.FindName("Filter"); filter.SelectedIndex=1; await Settle();
            Check(Math.Abs(Scale(filter,window)-.9)<.001,"category filters inherit exactly one scale");
            var apply=(Wpf.Ui.Controls.Button)dialog.Template.FindName("PrimaryButton",dialog);
            var cancel=(Wpf.Ui.Controls.Button)dialog.Template.FindName("CloseButton",dialog);
            Check(apply.ActualWidth < 240 && cancel.ActualWidth < 240 && apply.ActualHeight >= 36, "gallery actions size to content instead of stretching across dialog");
            Capture("gallery"); dialog.Hide(); await shown;
            var message=new Wpf.Ui.Controls.MessageBox { Owner=window, Title="Confirmation", Content="Test message", PrimaryButtonText="Confirm", CloseButtonText="Cancel" };
            var messageShown = message.ShowDialogAsync(); await Task.Delay(100); await Settle();
            var content=(ContentPresenter)message.Template.FindName("ContentPresenter",message);
            Check(Math.Abs(Scale(content,message)-.9)<.001,"WPF UI confirmation content is 90%");
            Check(Find<Wpf.Ui.Controls.Button>(message).Where(b=>b.Content?.ToString()=="Confirm").All(b=>Math.Abs(Scale(b,message)-.9)<.001),"WPF UI confirmation buttons are 90%");
            Check(Find<Wpf.Ui.Controls.TitleBar>(message).All(t=>Math.Abs(Scale(t,message)-1)<.001),"confirmation title bar remains unscaled");
            Find<Wpf.Ui.Controls.Button>(message).Single(b => b.Content?.ToString() == "Cancel")
                .RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            await messageShown;
            var rootGrid=(Grid)window.Content;
            foreach(double simulatedDpi in new[]{1.25,1.5,2.0})
            {
                rootGrid.LayoutTransform=new ScaleTransform(simulatedDpi,simulatedDpi); await Settle();
                Check(Math.Abs(Scale(knob,window)-.9*simulatedDpi)<.001,"density composes once with simulated DPI "+simulatedDpi);
            }
            rootGrid.LayoutTransform=Transform.Identity;
            foreach(var (width,height) in new[]{(640,480),(1280,720),(1920,1080),(1280,720)})
            {
                window.Width=width; window.Height=height; await Settle();
                Check(Math.Abs(Scale(knob,window)-.9)<.001,"resize preserves scale "+width);
                Check(outer.ExtentWidth<=outer.ViewportWidth+1,"resize has no horizontal overflow");
            }
            Check(fake.Writes==0,"density, popups and navigation send no MIDI edits");
            var navigation = (ListBox)window.FindName("Navigation");
            window.Width = 640; window.Height = 720; outer.ScrollToTop(); await Settle();
            foreach (var item in navigation.Items.OfType<ListBoxItem>())
            {
                item.IsSelected = true; await Settle();
                Check(tabs.SelectedIndex == navigation.Items.IndexOf(item), "navigation card activates corresponding page");
                var bounds = item.TransformToAncestor(navigation).TransformBounds(new Rect(item.RenderSize));
                Check(bounds.Left >= -1 && bounds.Right <= navigation.ActualWidth + 1 && item.IsVisible, "navigation card fits minimum width");
            }
            Capture("navigation-compact");
        }
        finally { window.Close(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); if(Directory.Exists(temporary)) Directory.Delete(temporary,true); }
    }
}
