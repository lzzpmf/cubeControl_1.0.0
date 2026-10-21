using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class PackCompactUiTests
{
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T item) yield return item;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-pack-compact-" + Guid.NewGuid().ToString("N")));
        var editor = model.PackEditor;
        await editor.SelectAsync(model.IrPacks.First(p => p.Definition != null && p.Group != "Meus packs"));
        editor.SelectedIr = editor.PackLibrary.Entries.First();
        var fixtureIr = editor.SelectedIr;
        for (int i = 0; i < 8; i++) editor.AssignIr(i, editor.SelectedIr);
        var old = Application.Current.MainWindow;
        var theme = ThemeService.Instance.Mode; var language = LanguageService.Instance.Language; var zoom = ZoomService.Instance.Percent;
        var window = new MainWindow(model, initializeDevices: false) { Width = 1600, Height = 1000, ShowInTaskbar = false };
        Application.Current.MainWindow = window; window.Show();
        ((TabControl)window.FindName("Tabs")).SelectedIndex = 1;
        ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 1;
        var view = (IrPackEditor)window.FindName("IrPacksCard");
        string folder = TestPaths.Report("C:/CubeControl-Build/reports/ir-pack-compact"); Directory.CreateDirectory(folder);
        try
        {
            foreach (var (code, percent, mode, width, contrast) in new[] {
                ("pt-BR", 100, ThemeMode.Dark, 1600, false), ("en-US", 100, ThemeMode.Light, 1600, false),
                ("pt-BR", 50, ThemeMode.Dark, 640, false), ("en-US", 200, ThemeMode.Light, 640, false),
                ("pt-BR", 100, ThemeMode.Dark, 1600, true) })
            {
                LanguageService.Instance.Preview(code); ZoomService.Instance.Preview(percent); ThemeService.Instance.Preview(mode);
                if (contrast) Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 1;
                window.Width = width; window.Height = width == 640 ? 480 : 1000;
                await Task.Delay(250); window.UpdateLayout();
                if (code == "en-US") Check(Find<TextBlock>(view).Any(t => t.Text == "Drag an IR onto a slot. Drag cabinet cards to swap their positions."), "mounted drag instructions switch to English");
                var grid = Find<IrPackGridPanel>(view).Single();
                Check(((ComboBox)view.FindName("PackSelector")).SelectedItem == null, "implicit draft selector clears catalog text before showing current draft caption");
                var notice = (Wpf.Ui.Controls.InfoBar)view.FindName("PackCreationNotice");
                Check(notice.IsOpen && notice.IsVisible && notice.Severity.ToString() == "Warning" && Equals(notice.Title, LanguageService.T("Criando pack")), "implicit draft has the standard localized warning notice");
                var reviewButton = (FrameworkElement)view.FindName("ReviewPackButton");
                var restoreButton = (FrameworkElement)view.FindName("RestorePackButton");
                var reviewBounds = reviewButton.TransformToAncestor(view).TransformBounds(new Rect(reviewButton.RenderSize));
                var restoreBounds = restoreButton.TransformToAncestor(view).TransformBounds(new Rect(restoreButton.RenderSize));
                var gridBounds = grid.TransformToAncestor(view).TransformBounds(new Rect(grid.RenderSize));
                Check(reviewBounds.Top >= gridBounds.Bottom && restoreBounds.Top >= gridBounds.Bottom && !reviewBounds.IntersectsWith(restoreBounds) && restoreButton.IsVisible,
                    "apply and restoration are visible, distinct actions below the cabinet grid");
                if (width == 1600) Check(Math.Abs(reviewBounds.Top - restoreBounds.Top) < 1, "main actions align side by side in wide layouts");

                var cards = Find<Border>(view).Where(b => b.Name == "SlotCard").OrderBy(b => ((IrPackSlotViewModel)b.DataContext).Index).ToArray();
                Check(cards.Length == 8 && cards.All(c => c.ActualWidth > 0), $"all eight destinations rendered at {code}/{percent}/{contrast}");
                if (grid.ActualWidth >= 756)
                {
                    var a = cards[0].TransformToAncestor(grid).Transform(new Point());
                    var b = cards[3].TransformToAncestor(grid).Transform(new Point());
                    var c = cards[4].TransformToAncestor(grid).Transform(new Point());
                    Check(Math.Abs(a.Y - b.Y) < 1 && c.Y > a.Y && cards.All(card => card.TransformToAncestor(grid).Transform(new Point()).Y + card.ActualHeight <= grid.ActualHeight + 1), "mounting grid fits eight volume-editable cards in two rows without clipping");
                }
                ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 0;
                await Task.Delay(100); window.UpdateLayout();
                var audition = (Border)window.FindName("IrAuditionCard");
                var fader = Find<BackingFader>(audition).Single();
                Check(fader.Orientation == Orientation.Horizontal && fader.ActualHeight >= 32 && fader.Template.FindName("PART_Track", fader) is System.Windows.Controls.Primitives.Track { Orientation: Orientation.Horizontal }, "audition uses a real horizontal themed fader");
                Check(Find<BackingButtonFace>(audition).Any(f => f.Art == "play") && Find<BackingButtonFace>(audition).Any(f => f.Art == "stop"), "audition shares the player transport artwork");
                ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 1;
                await Task.Delay(100); window.UpdateLayout();
                cards[0].BringIntoView(); window.Activate(); cards[0].Focus(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                var trash = Find<Wpf.Ui.Controls.Button>(cards[0]).Single();
                Check(trash.IsVisible && trash.IsEnabled, $"focused card exposes accessible trash action (focus={cards[0].IsKeyboardFocusWithin}, visibility={trash.Visibility}, enabled={trash.IsEnabled}, editable={editor.CanEdit})");
                cards[0].RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.Delete) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
                Check(!editor.Slots[0].HasEntry, "Delete removes only the focused slot");
                Check(editor.AssignIr(0, fixtureIr), "refill removed card with the original fixture independently of library selection");
                view.BringIntoView(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(window); var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                string target = Path.Combine(folder, $"mount-{code}-{percent}-{mode}-{contrast}.png");
                using (var output = File.Create(target)) png.Save(output);
                Console.WriteLine("SCREENSHOT " + target);
                ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 0; window.UpdateLayout();
                audition.BringIntoView(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
                png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                target = Path.Combine(folder, $"audition-{code}-{percent}-{mode}-{contrast}.png");
                using (var output = File.Create(target)) png.Save(output);
                Console.WriteLine("SCREENSHOT " + target);
            }
            LanguageService.Instance.Preview("pt-BR"); ZoomService.Instance.Preview(100); ThemeService.Instance.Preview(ThemeMode.Dark);
            window.Width = 1600; window.Height = 1000;
            ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 1; window.UpdateLayout();
            var destination = Find<Border>(view).Single(b => b.Name == "SlotCard" && ((IrPackSlotViewModel)b.DataContext).Index == 2);
            var sourceType = typeof(IrPackEditor).GetNestedType("SlotDrag", BindingFlags.NonPublic)!;
            var libraryType = typeof(IrPackEditor).GetNestedType("LibraryDrag", BindingFlags.NonPublic)!;
            DragEventArgs Args(IDataObject data)
            {
                var constructor = typeof(DragEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
                var args = (DragEventArgs)constructor.Invoke(constructor.GetParameters().Select(p => p.ParameterType == typeof(IDataObject) ? (object)data : p.ParameterType == typeof(int) ? 0 : p.ParameterType == typeof(DragDropKeyStates) ? DragDropKeyStates.LeftMouseButton : p.ParameterType == typeof(Point) ? new Point() : p.ParameterType == typeof(DragDropEffects) ? DragDropEffects.Copy | DragDropEffects.Move : (object)destination).ToArray());
                args.RoutedEvent = DragDrop.DropEvent; return args;
            }
            void Drag(string handler, DragEventArgs args)
            {
                var adapter = GongSolutions.Wpf.DragDrop.DragDrop.GetDropHandler(destination);
                var info = new GongSolutions.Wpf.DragDrop.DropInfo(destination, args, null, GongSolutions.Wpf.DragDrop.EventType.Bubbled);
                if (handler == "SlotDragOver") adapter.DragOver(info);
                else if (handler == "SlotDragLeave") adapter.DragLeave(info);
                else adapter.Drop(info);
                args.Effects = info.Effects;
            }
            editor.Slots[0].DistanceValue = 17; editor.Slots[2].DistanceValue = 81;
            var slotData = new DataObject(sourceType, Activator.CreateInstance(sourceType, editor, 0)!);
            var over = Args(slotData); Drag("SlotDragOver", over);
            Check(over.Effects == DragDropEffects.Move && DragFeedback.IsVisible, "typed slot drag highlights an accepted destination");
            Drag("SlotDragLeave", over); Check(!DragFeedback.IsVisible, "leaving a destination clears drag feedback without changing content");
            Drag("SlotDrop", Args(slotData));
            Check(editor.Slots[2].Entry!.Distance == 17 && editor.Slots[0].Entry!.Distance == 81, "slot drop handler swaps IRs and distances");
            var entry = editor.PackLibrary.Entries.Last();
            var libraryData = new DataObject(libraryType, Activator.CreateInstance(libraryType, editor, entry)!);
            Drag("SlotDrop", Args(libraryData));
            Check(editor.Slots[2].Entry!.RelativePath.Replace('\\', '/') == entry.RelativePath.Replace('\\', '/') && editor.Slots[2].Entry!.Distance == 80, "library drop handler assigns selected source with default distance");
            var beforeDrop = editor.Draft().Entries.ToArray();
            var external = new DataObject(DataFormats.FileDrop, new[] { "outside.wav" });
            var invalid = Args(external); Drag("SlotDrop", invalid);
            Check(invalid.Effects == DragDropEffects.None && editor.Draft().Entries.SequenceEqual(beforeDrop), "external file drops are rejected without draft changes");
            await model.Execute(() => {
                var locked = Args(libraryData); Drag("SlotDrop", locked);
                Check(locked.Effects == DragDropEffects.None && !editor.AssignIr(0, entry) && !editor.SwapSlots(0, 1), "processing lock rejects assignment and reordering");
                return Task.CompletedTask;
            });
            var volumeFader = (BackingFader)window.FindName("IrAuditionVolume");
            double beforeVolume = model.IrAudition.Volume;
            volumeFader.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120) { RoutedEvent = Mouse.MouseWheelEvent });
            Check(model.IrAudition.Volume == beforeVolume + 1, "horizontal fader wheel updates audition volume through its existing binding");
            editor.Name = "Existing name"; editor.Description = "Existing notes";
            var pending = editor.EnsureDetailsAsync(true);
            var host = (Wpf.Ui.Controls.ContentDialogHost)window.FindName("DialogHost");
            await Task.Delay(400); window.UpdateLayout();
            var dialog = Find<Wpf.Ui.Controls.ContentDialog>(host).Single();
            var fields = Find<TextBox>(dialog).Where(t => t.MaxLength is 120 or 4000).ToArray();
            Check(fields.Length == 2 && fields.Any(t => t.Text == "Existing name") && fields.Any(t => t.Text == "Existing notes"), "save popup pre-fills existing metadata");
            fields[0].Text = "Canceled metadata"; dialog.Hide();
            Check(!await pending && editor.Name == "Existing name" && editor.Description == "Existing notes", "canceling real metadata popup preserves draft fields");
            Check(model.WriteCommandCount == 0, "mounting, focus, removal, theme changes and metadata send no device commands");
        }
        finally
        {
            editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
            window.Close(); Application.Current.MainWindow = old;
            ThemeService.Instance.Preview(theme); LanguageService.Instance.Preview(language); ZoomService.Instance.Preview(zoom);
        }
    }
}
