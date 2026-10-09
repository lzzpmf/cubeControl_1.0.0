using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Views;
using Cube.Protocol;
using Cube.Desktop.Controls;
using System.Security.Cryptography;

internal static class GearImagePickerTests
{
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        if (root is T match) yield return match;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            foreach (var child in Find<T>(VisualTreeHelper.GetChild(root, i))) yield return child;
    }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-image-picker-" + Guid.NewGuid().ToString("N"));
        using var fake = new FakeConnection();
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(folder);
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model, initializeDevices: false) { ShowInTaskbar = false, Width = 1280, Height = 900 };
        window.Show(); await Task.Delay(200);
        try
        {
            int writes = fake.Writes;
            model.ChangeImage.Execute(null); await Task.Delay(250);
            var picker = Find<GearImagePicker>(window).Single(p => p.IsVisible);
            var category = (ComboBox)picker.FindName("Category");
            var list = (ListBox)picker.FindName("Images");
            Check(category.SelectedIndex == 1 && list.Items.Count > 0, "Amp opens directly on the bundled amp images");
            var grid = Find<EquipmentImageGridPanel>(list).Single();
            Check(grid.Columns >= 3, "Desktop collection has at least three columns");
            string equipmentRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Equipment");
            string alternativesRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Catalog");
            var collection = EquipmentImageEntry.Discover(equipmentRoot, alternativesRoot);
            Check(collection.Count(i => i.Id.StartsWith("Catalog/Cabeçotes/", StringComparison.Ordinal)) == 64, "All 64 archived heads are packaged and discoverable");
            Check(collection.Count(i => i.Id.StartsWith("Catalog/Racks/", StringComparison.Ordinal)) == 32, "All 32 archived racks are packaged and discoverable");
            Check(collection.Count(i => i.Id.StartsWith("Equipment/Cabinets/", StringComparison.Ordinal)) == 35, "All 35 cabinet images including subfolders are discoverable");
            Check(collection.Select(i => i.Id).Distinct().Count() == collection.Length, "Relative identities are unique across sources");
            Check(collection.Any(i => i.Name == "CubeBaby" && i.Category == "Dispositivos") && !collection.Any(i => i.Name.StartsWith("footswitch", StringComparison.OrdinalIgnoreCase)), "CubeBaby is a device and footswitch is excluded");
            string sourceAssets = Path.GetFullPath("CubeControl-1.1/Cube/windows/Cube.Desktop/Assets");
            foreach (string source in new[] { Path.Combine(sourceAssets, "Images", "Equipment") }.Concat(new[] { "Cabeçotes", "Caixas", "Pedais", "Guitarras", "Dispositivos", "Equipment", "Racks" }.Select(c => Path.Combine(sourceAssets, "Archive", "Images", c))))
                foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Where(EquipmentImageEntry.IsEquipmentImage))
                {
                    bool archived = file.StartsWith(Path.Combine(sourceAssets, "Archive"), StringComparison.OrdinalIgnoreCase);
                    string sourceRoot = archived ? Path.Combine(sourceAssets, "Archive", "Images") : Path.Combine(sourceAssets, "Images", "Equipment");
                    string copy = Path.Combine(archived ? alternativesRoot : equipmentRoot, Path.GetRelativePath(sourceRoot, file));
                    Check(File.Exists(copy) && SHA256.HashData(File.ReadAllBytes(file)).SequenceEqual(SHA256.HashData(File.ReadAllBytes(copy))), "Distributed image matches source: " + file);
                }
            var first = (EquipmentImageEntry)list.Items[0];
            string fixtureEquipment = Path.Combine(folder, "fixture", "Equipment", "Cabinets");
            string fixtureCatalog = Path.Combine(folder, "fixture", "Catalog", "Racks");
            Directory.CreateDirectory(fixtureEquipment); Directory.CreateDirectory(fixtureCatalog);
            File.Copy(first.Path, Path.Combine(fixtureEquipment, "same-name.png"));
            File.Copy(collection[^1].Path, Path.Combine(fixtureCatalog, "same-name.png"));
            var sameNames = EquipmentImageEntry.Discover(Path.GetDirectoryName(fixtureEquipment)!, Path.GetDirectoryName(fixtureCatalog)!);
            Check(sameNames.Length == 2 && sameNames[0].Id != sameNames[1].Id, "Same filename in different sources keeps both images");
            list.SelectedItem = first;
            list.Focus();
            void Arrow(System.Windows.Input.Key key) => list.RaiseEvent(new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent });
            Arrow(System.Windows.Input.Key.Right); Check(list.SelectedIndex == 1, "Right moves to the adjacent card");
            Arrow(System.Windows.Input.Key.Down); Check(list.SelectedIndex == 1 + grid.Columns, "Down moves one grid row");
            Arrow(System.Windows.Input.Key.Left); Arrow(System.Windows.Input.Key.Up); Check(list.SelectedIndex == 0, "Left and Up return to the first card");
            int resizeZoom = Cube.Desktop.Appearance.ZoomService.Instance.Percent;
            Cube.Desktop.Appearance.ZoomService.Instance.Preview(140);
            window.Width = 640; await Task.Delay(150);
            Check(picker.SelectedPath == first.Path && grid.Columns < 3, "Resize preserves selection and reduces columns");
            Cube.Desktop.Appearance.ZoomService.Instance.Preview(resizeZoom);
            window.Width = 1280; await Task.Delay(150);
            Check(picker.SelectedPath == first.Path && grid.Columns >= 3, "Returning to desktop preserves selection and restores columns");
            Check(!File.Exists(Path.Combine(folder, "gear-images-v1.json")) || !GearImagePreferences.Load(Path.Combine(folder, "gear-images-v1.json"), "unused").ContainsKey("amp-0"), "Selection is provisional");
            category.SelectedIndex = 0; await Task.Delay(100);
            Check(list.Items.Count > 20, "All equipment subfolders are discoverable");
            Check(picker.SelectedPath == first.Path && ReferenceEquals(list.SelectedItem, first), "Filter changes preserve the selected image");
            int guitars = Array.IndexOf(new[] { "Todos", "Amplificadores", "Gabinetes", "Efeitos", "Dispositivos", "Microfones", "Guitarras", "Racks" }, "Guitarras");
            category.SelectedIndex = guitars; Check(list.Items.Cast<EquipmentImageEntry>().All(i => i.Category == "Guitarras") && list.Items.Count == 13, "Guitar picklist filter includes archived guitars");
            category.SelectedIndex = 7; Check(list.Items.Count == 32, "Rack picklist filter includes archived racks");
            category.SelectedIndex = 0; await Task.Delay(100);
            list.ScrollIntoView(list.Items[list.Items.Count - 1]); await Task.Delay(150);
            grid = Find<EquipmentImageGridPanel>(list).Single();
            var lastCard = list.ItemContainerGenerator.ContainerFromIndex(list.Items.Count - 1) as ListBoxItem;
            Check(lastCard != null && VisualTreeHelper.GetParent(lastCard) == grid && lastCard.TranslatePoint(new Point(), list).Y < list.ActualHeight, "Scrolling to the end realizes and displays the last card");
            var visibleEntries = Find<ListBoxItem>(list).Where(c => c.TranslatePoint(new Point(), list).Y >= 0).Select(c => (EquipmentImageEntry)c.DataContext).ToArray();
            for (int attempt = 0; attempt < 50 && visibleEntries.Any(i => i.Thumbnail == null); attempt++) await Task.Delay(40);
            Check(visibleEntries.Length > 0 && visibleEntries.All(i => i.Thumbnail != null), "Visible recycled cards have loaded images");
            var apply = (Wpf.Ui.Controls.Button)picker.FindName("Apply");
            var computer = Find<Wpf.Ui.Controls.Button>(picker).First(b => b.Content as string == "Escolher do computador");
            Check(computer.IsVisible && computer.TranslatePoint(new Point(), picker).Y + computer.ActualHeight <= picker.ActualHeight, "PC action remains inside the picker after scrolling");
            Check(Find<ListBoxItem>(list).Count() < list.Items.Count, "Thumbnails use virtualized rows");
            string reports = "C:/CubeControl-Build/reports/gear-image-picker"; Directory.CreateDirectory(reports);
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap)); using (var output = File.Create(Path.Combine(reports, "picker.png"))) png.Save(output);
            Find<Wpf.Ui.Controls.Button>(picker).First(b => b.Content as string == "Cancelar").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            await Task.Delay(350);
            Check(!Find<GearImagePicker>(window).Any(p => p.IsVisible), "Cancel closes the selector without applying");
            model.ChangeImage.Execute(null); await Task.Delay(250);
            picker = Find<GearImagePicker>(window).Single(p => p.IsVisible);
            // A PC file is provisional until the same Apply action confirms it.
            string pcImage = Path.Combine(folder, "pc-image.png"); File.Copy(first.Path, pcImage);
            picker.SelectPath(pcImage);
            ((Wpf.Ui.Controls.Button)picker.FindName("Apply")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            await Task.Delay(350);
            Check(!Find<GearImagePicker>(window).Any(p => p.IsVisible) && GearImagePreferences.Load(Path.Combine(folder, "gear-images-v1.json"), "unused").ContainsKey("amp-0"), "Apply confirms the provisional PC image and closes");
            model.ChangeImage.Execute(null); await Task.Delay(250);
            picker = Find<GearImagePicker>(window).Single(p => p.IsVisible);
            list = (ListBox)picker.FindName("Images");
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
            list.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent, Source = row });
            await Task.Delay(350);
            Check(!Find<GearImagePicker>(window).Any(p => p.IsVisible), "Double click applies and closes");
            model.ChangeImage.Execute(null); await Task.Delay(250);
            picker = Find<GearImagePicker>(window).Single(p => p.IsVisible);
            var dialog = Find<Wpf.Ui.Controls.ContentDialog>(window).Single(d => d.IsVisible);
            dialog.RaiseEvent(new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, System.Windows.Input.Key.Escape) { RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent });
            await Task.Delay(350);
            Check(!Find<GearImagePicker>(window).Any(p => p.IsVisible), "Escape cancels the selector");
            foreach (var target in model.Amps.Take(1).Concat(model.Effects).Concat(model.Cabinets.Take(1)))
                Check(model.ApplyGearImage(target, first.Path), "Bundled image applies to " + target.Id);
            using var reloaded = new EditorViewModel(folder);
            var stored = GearImagePreferences.Load(Path.Combine(folder, "gear-images-v1.json"), "unused");
            foreach (string key in new[] { "amp-0", "chorus", "phaser", "delay", "reverb", "cab-off" })
                Check(stored.ContainsKey(key) && File.Exists(Path.Combine(folder, "Images", stored[key])), "Image persists for " + key);
            Check(reloaded.Effects.All(e => e.Image != null), "Effect images reload");
            var previous = model.Amps[0].Image;
            string invalid = Path.Combine(folder, "invalid.png"); File.WriteAllText(invalid, "invalid image");
            Check(!model.ApplyGearImage(model.Amps[0], invalid) && ReferenceEquals(previous, model.Amps[0].Image), "Invalid image preserves current artwork");
            string large = Path.Combine(folder, "large.png"); using (var file = File.Create(large)) file.SetLength(20 * 1024 * 1024 + 1);
            Check(!model.ApplyGearImage(model.Amps[0], large), "Size limit is enforced");
            model.SelectedStage = 1; model.ChangeImage.Execute(null); await Task.Delay(150);
            picker = Find<GearImagePicker>(window).Single(p => p.IsVisible);
            Check(((ComboBox)picker.FindName("Category")).SelectedIndex == 3, "Effects start in the effects category");
            Find<Wpf.Ui.Controls.Button>(picker).First(b => b.Content as string == "Cancelar").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); await Task.Delay(350);
            model.ResetImage.Execute(null);
            Check(!GearImagePreferences.Load(Path.Combine(folder, "gear-images-v1.json"), "unused").ContainsKey("chorus"), "Restore removes effect customization");
            var zoom = Cube.Desktop.Appearance.ZoomService.Instance;
            var theme = Cube.Desktop.Appearance.ThemeService.Instance;
            var language = Cube.Desktop.Localization.LanguageService.Instance;
            int originalZoom = zoom.Percent; var originalTheme = theme.Mode; string originalLanguage = language.Language;
            try
            {
                foreach (var mode in new[] { Cube.Desktop.Appearance.ThemeMode.Dark, Cube.Desktop.Appearance.ThemeMode.Light, Cube.Desktop.Appearance.ThemeMode.System })
                foreach (int percent in new[] { 50, 100, 200 })
                {
                    theme.Preview(mode);
                    if (mode == Cube.Desktop.Appearance.ThemeMode.System) Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                    zoom.Preview(percent); language.Preview(mode == Cube.Desktop.Appearance.ThemeMode.Light ? "en-US" : "pt-BR");
                    window.Width = 900; window.Height = 700; await Task.Delay(150);
                    model.ChangeImage.Execute(null); await Task.Delay(250);
                    picker = Find<GearImagePicker>(window).Single(p => p.IsVisible);
                    list = (ListBox)picker.FindName("Images");
                    grid = Find<EquipmentImageGridPanel>(list).Single();
                    Check(percent != 200 || grid.Columns <= 2, "High zoom adapts the catalog to one or two columns");
                    var body = Find<ScrollViewer>(window).First(s => s.Name == "PART_ContentScroll" && s.IsVisible);
                    Check(body.ScrollableHeight < 1 && body.ScrollableWidth < 1, $"Only the collection scrolls at {mode}, zoom {percent}: outer {body.ScrollableHeight}x{body.ScrollableWidth}");
                    computer = Find<Wpf.Ui.Controls.Button>(picker).First(b => b.Content as string == Cube.Desktop.Localization.LanguageService.T("Escolher do computador"));
                    Check(computer.Focus() && computer.IsKeyboardFocused, "PC action accepts keyboard focus");
                    var host = (FrameworkElement)window.FindName("DialogHost");
                    var bounds = computer.TransformToAncestor(host).TransformBounds(new Rect(computer.RenderSize));
                    Check(bounds.Bottom <= host.ActualHeight + 1 && bounds.Right <= host.ActualWidth + 1, $"PC button fits host at {percent}");
                    var capture = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); capture.Render(window);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(capture)); using (var output = File.Create(Path.Combine(reports, $"{mode}-{percent}.png"))) encoder.Save(output);
                    Find<Wpf.Ui.Controls.Button>(picker).First(b => b.Content as string == Cube.Desktop.Localization.LanguageService.T("Cancelar")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); await Task.Delay(350);
                }
            }
            finally { zoom.Preview(originalZoom); theme.Preview(originalTheme); language.Preview(originalLanguage); }
            Check(fake.Writes == writes, "Image selection and persistence never send MIDI commands");
            Console.WriteLine("PASS image picker: complete source/hash coverage, 64 heads, 32 racks, 35 cabinets, adaptive grid, arrows, filter/resize preservation, recycled thumbnails, fixed PC action, confirmation, cancellation, persistence, validation, themes/zoom and no MIDI writes.");
        }
        finally { model.Ask = _ => Task.FromResult(true); window.Close(); }
    }
}
