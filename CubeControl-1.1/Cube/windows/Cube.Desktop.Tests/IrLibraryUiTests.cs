using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class IrLibraryUiTests
{
    public static async Task Run()
    {
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-library-ui-" + Guid.NewGuid().ToString("N")));
        var window = new MainWindow(model, initializeDevices: false) { ShowInTaskbar = false };
        window.Show(); await Task.Delay(250); while (model.Busy) await Task.Delay(25);
        ((TabControl)window.FindName("Tabs")).SelectedIndex = 1;
        var tree = (TreeView)window.FindName("IrLibraryTree");
        string folder = Path.GetFullPath("C:/CubeControl-Build/reports/ir-library-ui"); Directory.CreateDirectory(folder);
        async Task Settle() { window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
        try
        {
            await Settle();
            if (tree.SelectedItem != null || tree.Items.Count != 5) throw new Exception("Library must open with five collapsed groups and no selection.");
            var collection = (TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(0);
            collection.IsExpanded = true; collection.IsSelected = true; await Settle();
            if (model.Ir.Audio != null) throw new Exception("Opening or selecting a collection must not load audio.");
            async Task<TreeViewItem> FirstLeaf(TreeViewItem parent)
            {
                while (((IrLibraryNode)parent.DataContext).IsFolder)
                {
                    parent.IsExpanded = true; await Settle();
                    parent = (TreeViewItem)parent.ItemContainerGenerator.ContainerFromIndex(0);
                }
                return parent;
            }
            var item = await FirstLeaf(collection);
            item.IsSelected = true; await model.PendingLibraryLoad; await Settle();
            item.Focus();
            item.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.Down) { RoutedEvent = Keyboard.KeyDownEvent });
            await model.PendingLibraryLoad;
            if (model.Ir.SourcePath != model.IrLibrary.Entries[1].FullPath) throw new Exception("Keyboard selection must load the next IR.");
            Console.WriteLine("PASS keyboard navigation loads library IR");
            var audio = model.Ir.Audio;
            var individualSelection = model.IrLibrary.Selected;
            var packCard = (Cube.Desktop.Controls.IrPackEditor)window.FindName("IrPacksCard");
            var packBrowser = (Cube.Desktop.Controls.IrLibraryBrowser)packCard.FindName("PackLibrary");
            packBrowser.Tree.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
            model.PackEditor.PackLibrary.Selected = model.PackEditor.PackLibrary.Entries[0];
            model.PackEditor.Search = "Vox";
            await Settle();
            if (model.IrLibrary.Selected != individualSelection || model.IrLibrary.Search != "" || model.Ir.Audio != audio)
                throw new Exception("Pack library search and pointer selection must preserve the individual library selection and audio.");
            model.PackEditor.Search = "";
            Console.WriteLine("PASS shared libraries keep independent selection, search and preview state");
            foreach (string outside in new[] { "IrLibrarySearch", "HeaderGrid" })
            {
                collection.IsSelected = true; await Settle();
                ((UIElement)window.FindName(outside)).RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
                await Settle();
                if (tree.SelectedItem != null || model.IrLibrary.Selected != null || model.Ir.Audio != audio || !collection.IsExpanded)
                    throw new Exception("Click outside must clear selection without unloading audio or collapsing the brand: " + outside);
            }
            Console.WriteLine("PASS filter and outside clicks clear selection while preserving audio and expansion");
            System.Windows.Controls.Primitives.ToggleButton? Chevron(DependencyObject parent)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is System.Windows.Controls.Primitives.ToggleButton toggle) return toggle;
                    if (Chevron(child) is { } found) return found;
                }
                return null;
            }
            collection.IsSelected = true;
            var otherBrand = (TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(1);
            var chevron = Chevron(otherBrand) ?? throw new Exception("Missing brand chevron");
            chevron.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
            await Settle();
            if (!otherBrand.IsSelected || collection.IsSelected || tree.SelectedItem != otherBrand.DataContext || model.Ir.Audio != audio)
                throw new Exception("Chevron click must select its own brand without replacing audio");
            Console.WriteLine("PASS chevron selects the matching brand and clears the previous highlight");


            var brandHeader = (FrameworkElement)otherBrand.Template.FindName("Border", otherBrand);
            foreach (var point in new[] { new Point(1, 1), new Point(20, 10),
                new Point(65, 15), new Point(120, 20),
                new Point(brandHeader.ActualWidth - 2, brandHeader.ActualHeight - 2) })
            {
                collection.IsExpanded = true; collection.IsSelected = true; await Settle();
                var hit = otherBrand.InputHitTest(brandHeader.TranslatePoint(point, otherBrand)) as UIElement
                    ?? throw new Exception("Brand row has a non-clickable area: " + point);
                void Click() => hit.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    { RoutedEvent = Mouse.PreviewMouseDownEvent });
                Click(); await Settle();
                if (!otherBrand.IsExpanded || !otherBrand.IsSelected || collection.IsExpanded || model.Ir.Audio != audio)
                    throw new Exception("Whole brand row must select and expand exactly once: " + point);
                Click(); await Settle();
                if (otherBrand.IsExpanded || !otherBrand.IsSelected)
                    throw new Exception("Second brand row click must collapse exactly once: " + point);
            }
            Console.WriteLine("PASS full brand row hit targets: corners, chevron, icon, text and right edge");

            model.IrLibrary.Search = "Orange"; await Settle();
            if (tree.Items.Count != 1 || !model.IrLibrary.Roots[0].IsExpanded || model.IrLibrary.Roots[0].EntryCount != 20 || model.IrLibrary.Selected != null || model.Ir.Audio != audio)
                throw new Exception("Search must show expanded matching folders without changing audio.");
            model.IrLibrary.Search = ""; await Settle();
            collection = (TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(0); collection.IsExpanded = true; await Settle();
            item = await FirstLeaf(collection); item.IsSelected = true; await model.PendingLibraryLoad;
            await model.Execute(() => model.LoadWavAndPreviewAsync("external.wav", audio!)); await Settle();
            if (tree.SelectedItem != null) throw new Exception("External import must clear visual tree selection.");
            item.IsSelected = true; await model.PendingLibraryLoad;
            Console.WriteLine("PASS tree search, folder navigation and external selection synchronization");
            var page = (ScrollViewer)window.FindName("IrScroll");
            var workspace = (ScrollViewer)window.FindName("WorkspaceScroll");
            var import = (FrameworkElement)window.FindName("ChooseWavCard");
            var selectedBeforeScroll = model.IrLibrary.Selected;
            var audioBeforeScroll = model.Ir.Audio;
            async Task Wheel(UIElement source, int delta)
            {
                source.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta) { RoutedEvent = Mouse.PreviewMouseWheelEvent });
                await Task.Delay(250); await Settle();
            }
            foreach (var size in new[] { (640, 480), (1280, 720), (1400, 940) })
            {
                window.Width = size.Item1; window.Height = size.Item2; await Settle();
                var owner = workspace.VerticalScrollBarVisibility == ScrollBarVisibility.Auto ? workspace : page;
                owner.ScrollToTop(); await Settle();
                await Wheel(import, -120);
                if (owner.VerticalOffset <= 0) throw new Exception("Wheel down must scroll IR page at " + size);
                double down = owner.VerticalOffset;
                await Wheel(import, 120);
                if (owner.VerticalOffset >= down) throw new Exception("Wheel up must scroll IR page at " + size);
                await FirstLeaf((TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(0));
                await Settle();
                var libraryScroll = FindScroll(tree)!;
                var leaf = (TreeViewItem)((TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(0)).ItemContainerGenerator.ContainerFromIndex(0);
                libraryScroll.ScrollToTop(); owner.ScrollToTop(); await Settle();
                await Wheel(leaf, -120);
                if (libraryScroll.VerticalOffset <= 0 || owner.VerticalOffset != 0) throw new Exception("Wheel must first scroll expanded library");
                libraryScroll.ScrollToEnd(); await Settle();
                await Wheel(leaf, -120);
                if (owner.VerticalOffset <= 0) throw new Exception("Library bottom must pass wheel down to page");
                libraryScroll.ScrollToTop(); await Settle(); down = owner.VerticalOffset;
                await Wheel(leaf, 120);
                if (owner.VerticalOffset >= down) throw new Exception("Library top must pass wheel up to page");
                if (model.Ir.PreviewActive || model.IrLibrary.Selected != selectedBeforeScroll || model.Ir.Audio != audioBeforeScroll)
                    throw new Exception("Scrolling must not select or load an IR");
                Console.WriteLine("PASS IR wheel down/up and tree boundary routing " + size);
            }
            foreach (var root in model.IrLibrary.Roots) root.IsExpanded = false;
            model.IrLibrary.Roots[0].IsExpanded = true;
            foreach (string language in new[] { "pt-BR", "en-US" })
            foreach (string theme in new[] { "Light", "Dark", "HighContrast" })
            {
                LanguageService.Instance.Preview(language);
                ThemeService.Instance.Preview(theme == "Light" ? ThemeMode.Light : ThemeMode.Dark);
                if (theme == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                foreach (int width in new[] { 640, 1280, 1920 })
                {
                    window.Width = width; window.Height = 900; await Settle();
                    var outer = (ScrollViewer)window.FindName("WorkspaceScroll"); outer.ScrollToTop();
                    ((ScrollViewer)window.FindName("IrScroll")).ScrollToTop(); await Settle();
                    if (outer.ExtentWidth > outer.ViewportWidth + 1) throw new Exception("Library horizontal overflow");
                    var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var output = File.Create(Path.Combine(folder, $"{language}-{theme}-{width}.png")); encoder.Save(output);
                }
                Console.WriteLine($"PASS library layout {language}/{theme} 640/1280/1920");
            }
        }
        finally { ThemeService.Instance.Preview(ThemeMode.Dark); LanguageService.Instance.Preview("pt-BR"); window.Close(); }
    }
    private static ScrollViewer? FindScroll(DependencyObject node)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
        {
            var child = VisualTreeHelper.GetChild(node, i);
            if (child is ScrollViewer scroll) return scroll;
            if (FindScroll(child) is { } nested) return nested;
        }
        return null;
    }
}
