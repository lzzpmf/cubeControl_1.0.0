using System.IO;
using System.Reflection;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cube.Desktop.Localization;
using Cube.Desktop.Appearance;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class CuratedPackTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private static void Reject(Action action, string message)
    {
        try { action(); } catch (FormatException) { Console.WriteLine("PASS " + message); return; }
        throw new Exception(message);
    }
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T found) yield return found;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }
    public static async Task Run()
    {
        string data = Path.Combine(Path.GetTempPath(), "cube-curated-packs-" + Guid.NewGuid().ToString("N"));
        using var model = new EditorViewModel(data);
        var curated = model.IrPacks.Where(p => p.Definition != null).ToArray();
        Check(curated.Length == 34 && model.PackCatalogProblems == "", "34 curated packs available without factory dependency");
        int[][] counts = [[2,1,2,1,2], [5,2,1,0,0], [0,0,8,0,0], [0,0,1,1,6], [0,1,1,6,0], [4,2,2,0,0], [5,2,0,1,0], [3,2,1,2,0], [0,0,4,2,2], [0,0,2,2,4]];
        string[] brands = ["Fender", "Vox", "Marshall", "Orange", "Mesa Boogie"];
        for (int i = 0; i < counts.Length; i++)
            Check(brands.Select(b => curated[i].Definition!.Entries.Count(e => e.RelativePath.StartsWith(b + "/"))).SequenceEqual(counts[i]), "brand composition: " + curated[i].Name);
        foreach (var pack in curated)
        {
            Check(pack.Problem == "" && pack.Definition!.Entries.Length == 8 && pack.Sectors.All(s => s.Length == 4096), "all references resolve: " + pack.Name);
            Check(pack.Definition!.Entries.All(e => e.Distance == 80), "default distance: " + pack.Name);
            Check(pack.Sectors.Select(CabinetInventory.Hash).Distinct().Count() == 8, "unique prepared sectors: " + pack.Name);
        }
        Check(curated.Select(p => p.Id).Distinct().Count() == curated.Length, "catalog IDs are unique");
        Check(curated.Select(p => string.Join("|", p.Definition!.Entries.Select(e => e.Sha256).OrderBy(h => h))).Distinct().Count() == curated.Length,
            "packs have distinct content selections, independent of order and distance");
        Check(curated.All(p => p.Group == p.Definition!.Group && p.Group != "Meus packs"), "catalog grouping is preserved");
        var first = curated[0].Definition!;
        var partial = first with { Id = Guid.NewGuid().ToString("N"), Name = "Personal", Group = "Meus packs", Entries = [first.Entries[0] with { Slot = 2, Distance = 23 }, first.Entries[1] with { Slot = 6 }] };
        model.PackStore.Save(partial);
        using (var restarted = new EditorViewModel(data))
            Check(restarted.IrPacks.Any(p => p.Id == partial.Id && p.HasSlot(2) && !p.HasSlot(0)), "personal partial pack survives restart");
        string exported = Path.Combine(data, "export.cubeirpack.json");
        IrPackStore.Export(exported, partial);
        var imported = model.PackStore.Import(exported);
        Check(imported.Id != partial.Id && imported.Entries.SequenceEqual(partial.Entries), "export/import preserves slots and distance with new identity");
        model.PackStore.Delete(imported);
        Check(model.PackStore.LoadPersonal(out _).Count == 1, "delete removes only selected personal pack");
        Reject(() => IrPackStore.Validate(partial with { Version = 2 }), "unknown version rejected");
        Reject(() => IrPackStore.Validate(partial with { Entries = [partial.Entries[0] with { RelativePath = "../outside.wav" }] }), "path traversal rejected");
        Reject(() => IrPackStore.Validate(partial with { Entries = [partial.Entries[0], partial.Entries[0]] }), "duplicate slots rejected");
        Reject(() => IrPackStore.Validate(partial with { Entries = [partial.Entries[0] with { Distance = 100 }] }), "invalid distance rejected");
        var missing = partial with { Entries = [partial.Entries[0] with { RelativePath = "missing.wav" }] };
        Check(model.PackStore.Resolve(missing).Problem.Length > 0, "missing references remain repairable");
        var altered = partial with { Entries = [partial.Entries[0] with { Sha256 = new string('0', 64) }] };
        Check(model.PackStore.Resolve(altered).Problem.Length > 0, "modified library content blocked");

        using var fake = new FakeConnection(); using var session = new CubeSession(fake); await session.Identify();
        await model.Execute(() => model.AttachSession(session));
        int writes = fake.Writes;
        model.ReloadIrPacks(partial.Id);
        var editor = model.PackEditor;
        var grid = new IrPackGridPanel();
        for (int i = 0; i < 8; i++) grid.Children.Add(new Border { Height = 100 });
        foreach (var (width, columns) in new[] { (1076d, 4), (800d, 2), (532d, 2), (400d, 1) })
        {
            grid.Measure(new Size(width, double.PositiveInfinity)); grid.Arrange(new Rect(0, 0, width, grid.DesiredSize.Height));
            Check(grid.Children.Cast<UIElement>().All(c => c.RenderSize.Width >= 260), "grid preserves usable card width at " + width);
            var nextRow = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot((FrameworkElement)grid.Children[columns]);
            Check(nextRow.Left == 0 && nextRow.Top == 112, "grid wraps in reading order at " + width);
        }
        // The standalone host must select the same workspace as MainWindow's pack tab.
        // Otherwise shared library selection starts an audition and locks draft editing.
        model.IrWorkspaceTab = 1;
        var control = new IrPackEditor { DataContext = editor };
        Window Host(IrPackEditor content, double width = 1100) {
            var host = new Window { Width = width, Height = 800, Content = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled } };
            host.SetResourceReference(Window.BackgroundProperty, "CubeCanvas");
            return host;
        }
        var window = Host(control, 1700);
        window.Show(); await System.Windows.Threading.Dispatcher.Yield();
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        string screenshot = Path.Combine(data, "pack-manager.png");
        using (var file = File.Create(screenshot)) encoder.Save(file);
        Console.WriteLine("SCREENSHOT " + screenshot);
        void Click(string label, int index = 0) { var button = Find<Button>(window).Where(b => Equals(b.Content, LanguageService.T(label))).ElementAt(index); Check(button.Command.CanExecute(null), "action available: " + label); button.Command.Execute(null); }
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
        await editor.SelectAsync(null);
        await System.Windows.Threading.Dispatcher.Yield();
        Check(control.FindName("PackDetails") == null && control.FindName("PackName") == null, "new pack starts with cabinet cards and metadata is reserved for save popup");
        editor.ChooseDetails = (name, description) => Task.FromResult<(string Name, string Description)?>((string.IsNullOrEmpty(name) ? "UI personal" : name, description));
        editor.PackLibrary.Selected = editor.PackLibrary.Entries[0];
        var previewFile = model.Ir.File;
        editor.Search = "no-results-unique-token";
        Check(editor.LibraryEmpty && !editor.Library.Any(), "empty library search is explicit");
        editor.Search = "";
        editor.PackLibrary.Selected = editor.PackLibrary.Entries[0];
        await model.PendingLibraryLoad;
        Check(model.Ir.File == previewFile && !model.Busy && fake.Writes == writes, "pack selection leaves the preview IR unchanged without starting audition writes");
        SlotMenu(2, "Usar neste slot"); SlotMenu(5, "Usar neste slot");
        await System.Windows.Threading.Dispatcher.Yield();
        Check(editor.Slots.Count(s => s.Entry != null) == 2, "keyboard context actions assign the selected library IR");
        Check(Find<TextBlock>(window).Any(t => t.Text == LanguageService.T("Este pack contém IRs repetidos.")), "editor warns about repeated IRs");
        void SlotMenu(int index, string label)
        {
            var card = Find<Border>(window).Where(b => b.Name == "SlotCard").ElementAt(index);
            var menu = card.ContextMenu; menu.PlacementTarget = card; menu.IsOpen = true; menu.UpdateLayout();
            var item = menu.Items.OfType<MenuItem>().Single(m => Equals(m.Header, LanguageService.T(label)));
            Check(item.Command.CanExecute(null), "slot menu command available: " + label);
            item.Command.Execute(null); menu.IsOpen = false;
        }
        editor.Slots[2].DistanceValue = 23;
        Check(editor.Slots[2].Distance == "23" && editor.Dirty, $"distance edits only the local slot draft (distance={editor.Slots[2].Distance}, dirty={editor.Dirty}, editable={editor.CanEdit}, busy={model.Busy})");
        SlotMenu(2, "Mover para o slot anterior"); SlotMenu(5, "Remover IR"); Click("Salvar pack");
        Check(model.IrPacks.Single(p => p.Name == "UI personal").Definition!.Entries.Single().Slot == 1, "editor assigns, moves, removes and saves a partial pack");
        Check(model.SelectedIrPack?.Name == "UI personal" && !editor.Dirty, "saving synchronizes the pack used for application");
        editor.Slots[1].Distance = "99";
        Check(editor.Dirty && model.ApplyIrPack.CanExecute(null), "unsaved draft can be reviewed and applied");
        editor.Slots[1].Distance = "100";
        Check(editor.Slots[1].Distance == "99" && editor.Message == "Use uma distância de 0 a 99.", "invalid distance is rejected without losing the valid value");
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Cancel);
        model.Ask = _ => Task.FromResult(false);
        Check(!await editor.SelectAsync(curated[0]) && editor.Dirty && editor.Name == "UI personal", "cancelled selection preserves the draft");
        Check(!await editor.ConfirmDiscard(), "closing confirmation can preserve the draft");
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
        model.Ask = _ => Task.FromResult(true);
        editor.Save();
        Check(model.SelectedIrPack!.Definition!.Entries.Single().Distance == 99, "distance persists when saved");
        await editor.SelectAsync(curated[0]);
        Check(editor.ReadOnly && !editor.SavePack.CanExecute(null) && editor.Slots[0].UseIr.CanExecute(null), "predefined source offers direct editing and only saves after a change");
        editor.Slots[0].Remove.Execute(null);
        Check(editor.Dirty && !editor.ReadOnly && editor.IsCreating && model.ApplyIrPack.CanExecute(null), "first removal automatically creates an independent editable draft");
        editor.Save();
        Check(model.SelectedIrPack!.Id != curated[0].Id && model.SelectedIrPack.Group == "Meus packs", "implicit draft saves as a separate personal pack");
        string sourceId = model.SelectedIrPack.Id;
        editor.Slots[0].UseIr.Execute(null); editor.Save();
        Check(model.SelectedIrPack!.Id == sourceId, "direct personal editing updates the existing personal pack");
        string uiExport = Path.Combine(data, "ui-export.cubeirpack.zip");
        editor.Export(uiExport); editor.Import(uiExport);
        Check(editor.Current == model.SelectedIrPack && editor.Dirty && editor.Slots.All(s => s.Entry != null), "import stages the portable pack without saving");
        editor.Save();
        string deletedId = editor.Current!.Id;
        editor.DeletePack.Execute(null);
        Check(model.IrPacks.All(p => p.Id != deletedId), "editor deletes the selected personal pack");
        window.Close();
        int originalZoom = ZoomService.Instance.Percent; string originalLanguage = LanguageService.Instance.Language;
        var originalTheme = ThemeService.Instance.Mode;
        try
        {
            foreach (var (lang, zoom, theme, width, highContrast) in new[] {
                ("en-US", 200, ThemeMode.Light, 640, false), ("pt-BR", 50, ThemeMode.Dark, 640, false),
                ("pt-BR", 100, ThemeMode.Dark, 1700, false), ("en-US", 100, ThemeMode.Light, 1700, false),
                ("pt-BR", 100, ThemeMode.Dark, 1700, true) })
            {
                LanguageService.Instance.Preview(lang); ZoomService.Instance.Preview(zoom); ThemeService.Instance.Preview(theme);
                var layoutControl = new IrPackEditor { DataContext = editor, LayoutTransform = new ScaleTransform(UiDensity.Factor, UiDensity.Factor) };
                if (highContrast) Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                var layout = Host(layoutControl, width);
                layout.Show(); await System.Windows.Threading.Dispatcher.Yield(); layout.UpdateLayout();
                Check(Find<Button>(layout).Any(b => Equals(b.Content, LanguageService.T("Salvar pack"))), $"save action present at {lang}/{zoom}/{theme}");
                Check(Find<ScrollViewer>(layout).First().ScrollableWidth == 0, "compact editor needs no horizontal page scrolling");
                foreach (var action in Find<Wpf.Ui.Controls.Button>(layoutControl).Where(b => b.IsVisible))
                {
                    var bounds = action.TransformToAncestor(layoutControl).TransformBounds(new Rect(action.RenderSize));
                    Check(bounds.Left >= 0 && bounds.Right <= layoutControl.ActualWidth + 1 && action.ActualHeight >= 39.5,
                        $"pack action fits compact card with standard height: {action.Content}/{lang}/{zoom}");
                }
                var selector = (ComboBox)layoutControl.FindName("PackSelector");
                selector.SelectedItem = model.IrPacks.First(p => p.Id == curated[1].Id);
                await System.Windows.Threading.Dispatcher.Yield();
                Check(editor.Current?.Id == curated[1].Id && model.SelectedIrPack?.Id == curated[1].Id, "inline selector synchronizes editor and apply target");
                var capture = new System.Windows.Media.Imaging.RenderTargetBitmap((int)layout.ActualWidth, (int)layout.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                capture.Render(layout); var png = new System.Windows.Media.Imaging.PngBitmapEncoder(); png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(capture));
                string target = Path.Combine(data, $"manager-{lang}-{zoom}-{theme}-{highContrast}.png"); using (var file = File.Create(target)) png.Save(file);
                Console.WriteLine("SCREENSHOT " + target); layout.Close();
            }
        }
        finally { LanguageService.Instance.Preview(originalLanguage); ZoomService.Instance.Preview(originalZoom); ThemeService.Instance.Preview(originalTheme); }
        using (var pageModel = new EditorViewModel(Path.Combine(data, "page")))
        {
            var page = new MainWindow(pageModel, initializeDevices: false) { Width = 1100, Height = 800, ShowInTaskbar = false };
            page.Show(); await System.Windows.Threading.Dispatcher.Yield();
            var tabs = (TabControl)page.FindName("Tabs"); tabs.SelectedIndex = 1;
            pageModel.IrWorkspaceTab = 1;
            page.UpdateLayout(); await System.Windows.Threading.Dispatcher.Yield();
            var card = (IrPackEditor)page.FindName("IrPacksCard");
            Check(ReferenceEquals(card.DataContext, pageModel.PackEditor), "IR page hosts the session pack editor");
            await pageModel.PackEditor.SelectAsync(null); pageModel.PackEditor.Name = "Navigation draft";
            tabs.SelectedIndex = 0; tabs.SelectedIndex = 1;
            page.UpdateLayout(); await System.Windows.Threading.Dispatcher.Yield();
            Check(pageModel.PackEditor.Dirty && pageModel.PackEditor.Name == "Navigation draft", "switching sections preserves the draft");
            var slotCard = Find<Border>(card).First(b => b.Name == "SlotCard");
            slotCard.BringIntoView(); await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            page.Activate(); slotCard.Focus();
            Check(slotCard.IsKeyboardFocusWithin, "compact slot card accepts keyboard focus");
            pageModel.PackEditor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Cancel);
            pageModel.Ask = _ => Task.FromResult(false); page.Close();
            Check(page.IsVisible && pageModel.PackEditor.Dirty, "closing the application respects cancelled discard");
            pageModel.PackEditor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
            pageModel.Ask = _ => Task.FromResult(true); pageModel.PackEditor.DiscardChanges.Execute(null);
            await pageModel.PackEditor.SelectAsync(pageModel.IrPacks.First());
            var scroll = (ScrollViewer)page.FindName("IrScroll");
            foreach (var theme in new[] { ThemeMode.Dark, ThemeMode.Light })
            {
                ThemeService.Instance.Preview(theme); ZoomService.Instance.Preview(100);
                scroll.ScrollToEnd(); page.UpdateLayout(); await System.Windows.Threading.Dispatcher.Yield();
                Check(Find<Button>(card).Any(b => Equals(b.Content, LanguageService.T("Aplicar")) && b.IsVisible), "apply remains reachable at page end");
                var capture = new System.Windows.Media.Imaging.RenderTargetBitmap((int)page.ActualWidth, (int)page.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                capture.Render(page); var png = new System.Windows.Media.Imaging.PngBitmapEncoder(); png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(capture));
                string target = Path.Combine(data, $"ir-page-{theme}.png"); using (var file = File.Create(target)) png.Save(file);
                Console.WriteLine("SCREENSHOT " + target);
            }
            page.Close(); await System.Windows.Threading.Dispatcher.Yield();
            ThemeService.Instance.Preview(originalTheme); ZoomService.Instance.Preview(originalZoom);
        }
        await PackWorkflowTests.Run();
        await PackReviewUiTests.Run();
        Check(fake.Writes == writes, "opening manager and browsing packs sends no device commands");
        byte[] original = fake.Flash.ToArray(), bank = fake.Bank.ToArray();
        async Task Apply() => await (Task)typeof(EditorViewModel).GetMethod("ApplyIrPackAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(model, null)!;
        model.SelectedIrPack = model.PackStore.Resolve(partial);
        model.ChoosePackIrSlots = different =>
        {
            Check(different.Select((v, i) => (v, i)).Where(x => x.v).Select(x => x.i).SequenceEqual(new[] { 2, 6 }), "empty slots excluded from comparison");
            return Task.FromResult<int[]?>([2]);
        };
        model.Ask = _ => Task.FromResult(false); await Apply(); Check(fake.Erases == 0, "cancel preserves flash");
        model.Ask = _ => Task.FromResult(true); await Apply();
        for (int i = 0; i < 8; i++) Check(fake.Flash.AsSpan(i * 4096, 4096).SequenceEqual(i == 2 ? model.SelectedIrPack.Sectors[i] : original.AsSpan(i * 4096, 4096)), "selective write slot " + i);
        Check(fake.Erases == 1 && fake.Bank.SequenceEqual(bank), "presets and unselected slots preserved");
        model.SelectedIrPack = model.PackStore.Resolve(partial with { Entries = [partial.Entries[0]] });
        model.ChoosePackIrSlots = _ => throw new Exception("identical pack prompted");
        await Apply(); Check(fake.Erases == 1, "identical partial pack performs no write");
        using var readback = new EditorViewModel(data);
        var names = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(data, "ir-pack-names-v1.json")))!;
        Check(names.ContainsKey(CabinetInventory.Hash(fake.Flash.AsSpan(2 * 4096, 4096).ToArray())), "IR name persisted by content hash");
        model.SelectedIrPack = model.PackStore.Resolve(missing); writes = fake.Writes;
        try { await Apply(); throw new Exception("missing reference accepted"); }
        catch (FormatException) { Check(fake.Writes == writes, "invalid reference rejected before device write"); }
        model.SelectedIrPack = curated[0];
        model.ChoosePackIrSlots = _ => Task.FromResult<int[]?>([0, 1]);
        fake.Observe = packet => { if (packet.Command == 0x21 && fake.Erases == 2) throw new IOException("Injected second-slot failure"); };
        try { await Apply(); throw new Exception("failure not propagated"); }
        catch (InvalidOperationException ex) { Check(ex.Message.Contains("Slots concluídos: 1."), "intermediate failure reports completed slot only"); }
    }
}
