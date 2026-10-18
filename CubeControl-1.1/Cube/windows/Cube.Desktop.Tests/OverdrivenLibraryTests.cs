using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
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
using Cube.Protocol;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class OverdrivenLibraryTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }

    public static async Task Run()
    {
        var library = new IrLibraryViewModel(() => true);
        var entries = library.Entries.Where(e => e.Metadata != null).ToArray();
        Check(entries.Length == 682 && entries.Select(e => e.Metadata!.CollectionId).Distinct().Count() == 13, "13 Overdriven collections and 682 catalog entries");
        Check(Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses", "Overdriven.fr"), "readme.txt", SearchOption.AllDirectories).Count() == 13,
            "all original collection readmes copied to build output");
        Check(entries.Select(e => e.RelativePath).Distinct().Count() == 682 && library.Roots.All(n => n.Name != "Overdriven.fr") && Leaves(library.Roots).Count(n => n.Entry!.Metadata != null) == 682,
            "each catalog entry occurs once under cabinet brands");
        foreach (var entry in entries)
        {
            var m = entry.Metadata!;
            byte[] bytes = await File.ReadAllBytesAsync(entry.FullPath);
            var audio = IrAudio.Decode(bytes);
            CheckFile(Convert.ToHexString(SHA256.HashData(bytes)) == m.Sha256 && audio.Channels == m.Channels
                && audio.SampleRate == m.SampleRate && audio.Bits == m.Bits && audio.Format == m.Format
                && Math.Abs(audio.Duration * 1000 - m.DurationMs) < .001 && audio.Prepare().Length == 512, entry.RelativePath);
        }
        Check(entries.Count(e => e.Metadata is { Bits: 16, DurationMs: 50 }) == 82
            && entries.Count(e => e.Metadata is { Bits: 24, DurationMs: 100 }) == 600, "all 682 hashes and WAV formats verified, including GOV documentation discrepancy");
        Check(entries.Count(e => e.Name.Contains("T75-M3-")) == 6 && entries.Where(e => e.Name.Contains("T75-M3-")).All(e => e.Metadata!.Microphone == "CND-M3"), "M3 shorthand resolved through explicit T75 exception");
        Check(entries.Single(e => e.Name == "OD-FBVET30-DN-409U-09-40").Metadata!.Position == "P09"
            && entries.Single(e => e.Name == "OD-FBVET30-DN-421B-10-40-LC").Metadata!.Position == "P10", "two legacy position exceptions retain original names");
        Check(entries.Any(e => e.Metadata is { IsPreview: true, Microphone: "LD-103" })
            && entries.All(e => e.Metadata!.Microphone != "SP1-PREVIEW"), "preview files identify their own microphones");
        Check(entries.Any(e => e.Metadata!.Variants.SequenceEqual(new[] { "LC", "P0" }))
            && entries.Any(e => e.Metadata!.Variants.SequenceEqual(new[] { "LC", "L" })), "combined suffixes retained");
        Check(entries.Any(e => e.Metadata!.CaptureDistanceMm == 0) && entries.Any(e => e.Metadata!.CaptureDistanceMm == 150), "physical distances include zero and values over pedal parameter range");
        Check(entries.Where(e => e.Metadata is { Microphone: "LD-103", Brand: "Zilla" or "Mesa Boogie" }).All(e => e.Metadata!.MicrophoneModel == "Neumann TLM 103")
            && entries.Where(e => e.Metadata is { Microphone: "LD-103", Brand: "Marshall" }).All(e => e.Metadata!.MicrophoneModel == null), "modeled microphone identity is scoped to the documenting collection pages");
        Check(entries.Where(e => e.Metadata!.Microphone == "RBN-CN-2").All(e => e.Metadata!.MicrophoneModel == null), "undocumented microphone models are not inferred");
        var cv = entries.First(e => e.Metadata!.Collection == "E112 / CV").Metadata!;
        var vt = entries.First(e => e.Metadata!.Collection == "E112 / VT").Metadata!;
        Check(cv.CollectionId != vt.CollectionId && cv.Cabinet == "DE-112" && vt.Cabinet == "DE-112", "CV and VT keep separate identities and documented cabinet descriptions");

        var candidate = entries.First(e => e.Metadata is { Collection: "FB / GOV", Microphone: "DYN-57", Position: "P10" });
        var selected = candidate.Metadata!;
        library.Search = "Zilla 2x12 Governor Shure SM57 P10";
        Check(library.VisibleEntries.Count > 0 && library.VisibleEntries.All(e => e.Metadata is { Brand: "Zilla", Microphone: "DYN-57", Position: "P10" }), "search combines friendly brand, speaker and capture names");
        library.Selected = candidate;
        Check(library.SelectedDetails.Contains("mm") && library.SelectedDetails.Contains("512") && library.SelectedDetails.Contains(candidate.Name), "capture details preserve original name and conversion limit");
        library.Search = "no-such-overdriven-ir";
        Check(library.VisibleEntries.Count == 0 && library.Selected == null, "filtering clears selection with empty results");
        library.Search = "";
        Check(library.VisibleEntries.Count == 958, "empty search restores all legacy and Overdriven entries");
        Check(library.Roots.Select(n => n.Name).SequenceEqual(new[] { "Engl", "Fender", "Marshall", "Mesa Boogie", "Orange", "Vox", "Zilla" }), "existing brands merged; new cabinet brands sorted");
        foreach (var entry in entries)
        {
            var m = entry.Metadata!;
            var brand = library.Roots.Single(n => n.Name == m.Brand);
            var layout = brand.Children.Single(n => n.Name == m.SpeakerConfiguration);
            var model = layout.Children.Single(n => n.Name == m.ModelLabel);
            CheckFile(model.Children.Count(n => n.Entry == entry) == 1 && model.Children.All(n => !n.IsFolder), entry.RelativePath);
        }
        Check(Leaves(library.Roots).Select(n => n.Entry!.RelativePath).Distinct().Count() == 958, "unified hierarchy preserves every WAV identity");
        Check(entries.All(e => e.Metadata!.Sources.Length >= 3 && e.Metadata.IdentificationEvidence.Length > 0), "all 13 collections retain evidence and sources");
        Check(cv.ModelLabel != vt.ModelLabel && cv.SpeakerLabel == "CV", "conflicting CV readme does not merge CV and VT");
        Check(entries.Where(e => e.Metadata!.Identification == "probable").All(e => e.Metadata!.ModelLabel.Contains("identificação provável")), "inferred brands are marked at the model level");
        Check(library.Roots.SelectMany(r => r.Children).SelectMany(n => n.Children).Where(n => n.Children.Any(c => c.Entry?.Metadata != null))
            .All(n => n.Children.Select(c => c.Name).Distinct().Count() == n.Children.Count), "capture labels are unambiguous within each model");
        library.Search = candidate.Name;
        Check(library.VisibleEntries.Contains(candidate), "original filename search remains available");
        library.Search = "Overdriven";
        Check(library.VisibleEntries.Count == 682, "provider search finds all Overdriven captures without a provider root");
        library.Search = "";

        string temp = Path.Combine(Path.GetTempPath(), "cube-overdriven-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string root = Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses");
            var store = new IrPackStore(root, temp);
            var distant = entries.First(e => e.Metadata!.CaptureDistanceMm == 150);
            var reference = store.Reference(0, distant.RelativePath, distant.Name);
            var pack = new IrPackDefinition { Name = "Capture distance test", Entries = [reference] };
            var resolved = store.Resolve(pack);
            Check(reference.Distance == 80 && resolved.Problem == "" && resolved.Sectors[0].SequenceEqual(IrAudio.Decode(File.ReadAllBytes(distant.FullPath)).Sector(80)),
                "pack hashes and paths resolve; physical 150 mm never initializes pedal Distance");
            File.Copy(candidate.FullPath, Path.Combine(temp, "external.wav"));
            var imported = new IrLibraryViewModel(() => true, root, temp, packPicker: true);
            imported.Search = "external";
            Check(imported.VisibleEntries.Any(e => e.RelativePath.StartsWith("Imported/")), "imported WAV remains accessible through search");
            File.WriteAllText(Path.Combine(temp, "Overdriven.catalog.v1.json"), "{}");
            var invalid = new IrLibraryViewModel(() => true, temp);
            Check(invalid.Entries.Count == 1 && invalid.Entries[0].Metadata == null && invalid.Message.Contains("Metadados indisponíveis"), "malformed manifest falls back to physical WAV tree");
            File.WriteAllText(Path.Combine(temp, "Overdriven.catalog.v1.json"), JsonSerializer.Serialize(new { Version = 1, Provider = "Overdriven.fr", Entries = new[] { selected with { RelativePath = "Overdriven.fr/../outside.wav" } } }));
            Check(new IrLibraryViewModel(() => true, temp).Entries.All(e => e.Metadata == null), "unsafe manifest paths rejected without hiding WAVs");
            var duplicates = new[] { "Overdriven.fr/a/same.wav", "Overdriven.fr/b/same.wav" }
                .Select(path => selected with { RelativePath = path, OriginalName = "same.wav" }).ToArray();
            foreach (var duplicate in duplicates)
            {
                string path = Path.Combine(temp, duplicate.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Copy(candidate.FullPath, path);
            }
            File.WriteAllText(Path.Combine(temp, "Overdriven.catalog.v1.json"), JsonSerializer.Serialize(new { Version = 1, Provider = "Overdriven.fr", Entries = duplicates }));
            var collision = new IrLibraryViewModel(() => true, temp);
            var duplicateLeaves = Leaves(collision.Roots).Where(n => n.Entry!.Metadata != null).ToArray();
            Check(duplicateLeaves.Length == 2 && duplicateLeaves.Select(n => n.Name).Distinct().Count() == 2,
                "same capture and original filename in different folders remain distinguishable");
            var oldCatalog = duplicates.Select(m => m with { Brand = "Marca não identificada", SpeakerConfiguration = "Configuração não informada", CabinetModel = "", SpeakerLabel = "", Identification = "unknown", Sources = [] });
            File.WriteAllText(Path.Combine(temp, "Overdriven.catalog.v1.json"), JsonSerializer.Serialize(new { Version = 1, Provider = "Overdriven.fr", Entries = oldCatalog }));
            Check(new IrLibraryViewModel(() => true, temp).Roots.Any(n => n.Name == "Marca não identificada" && n.EntryCount == 2),
                "unidentified captures remain browsable without inventing a brand");
        }
        finally { Directory.Delete(temp, recursive: true); }
        await CheckUi(candidate);
    }

    private static IEnumerable<IrLibraryNode> Leaves(IEnumerable<IrLibraryNode> nodes)
    {
        foreach (var node in nodes)
            if (node.Entry != null) yield return node;
            else foreach (var leaf in Leaves(node.Children)) yield return leaf;
    }

    private static void CheckFile(bool condition, string path)
    {
        if (!condition) throw new Exception("Catalog/hash/decode mismatch: " + path);
    }

    private static void Capture(Window window, string path)
    {
        // RenderTargetBitmap omits the compositor's Mica backdrop. Composite onto
        // the current solid app canvas so translucent cards can be inspected.
        var visual = new DrawingVisual();
        using (var drawing = visual.RenderOpen())
        {
            var bounds = new Rect(0, 0, window.ActualWidth, window.ActualHeight);
            drawing.DrawRectangle(ThemeService.Brush("CubeCanvas"), null, bounds);
        }
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(path); encoder.Save(output);
    }

    private static async Task CheckUi(IrLibraryEntry candidate)
    {
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-overdriven-ui-" + Guid.NewGuid().ToString("N")));
        var window = new MainWindow(model, initializeDevices: false) { ShowInTaskbar = false, Width = 1280, Height = 900 };
        int originalZoom = ZoomService.Instance.Percent;
        string originalLanguage = LanguageService.Instance.Language;
        var originalTheme = ThemeService.Instance.Mode;
        window.Show();
        try
        {
            ((TabControl)window.FindName("Tabs")).SelectedIndex = 1;
            await Task.Delay(250);
            while (model.Busy) await Task.Delay(25);
            var tree = (TreeView)window.FindName("IrLibraryTree");
            DependencyObject? parent = tree;
            while (parent is not IrLibraryBrowser && parent != null) parent = VisualTreeHelper.GetParent(parent);
            var browser = (IrLibraryBrowser)parent!;
            foreach (string filter in new[] { "IrCollectionFilter", "IrCabinetFilter", "IrSpeakerFilter", "IrMicrophoneFilter", "IrPositionFilter", "IrDistanceFilter" })
                Check(browser.FindName(filter) == null, "removed picklist " + filter);
            model.IrLibrary.Search = "Zilla Governor DYN-57";
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(model.IrLibrary.VisibleEntries.All(e => e.Metadata is { Brand: "Zilla", Microphone: "DYN-57" }) && model.WriteCommandCount == 0, "unified search works without sending MIDI");
            model.IrLibrary.Selected = candidate;
            await model.PendingLibraryLoad;
            var details = (Expander)browser.FindName("IrCaptureDetails");
            details.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
            Check(model.IrLibrary.Selected == candidate && model.WriteCommandCount == 0, "opening capture details preserves selected IR without MIDI commands");
            details.IsExpanded = true;
            string report = TestPaths.Report("C:/CubeControl-Build/reports/overdriven-library");
            Directory.CreateDirectory(report);
            foreach (string language in new[] { "pt-BR", "en-US" })
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            foreach (var (width, zoom) in new[] { (640, 100), (1280, 100), (640, 200), (1280, 50) })
            {
                LanguageService.Instance.Preview(language); ThemeService.Instance.Preview(theme); ZoomService.Instance.Preview(zoom);
                window.Width = width; window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                browser.SearchBox.BringIntoView();
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                var scroll = (ScrollViewer)window.FindName("WorkspaceScroll");
                Check(scroll.ExtentWidth <= scroll.ViewportWidth + 1, $"Unified library responsive {language}/{theme}/{width}/{zoom}");
                Capture(window, Path.Combine(report, $"{language}-{theme}-{width}-{zoom}.png"));
            }
            ZoomService.Instance.Preview(100); window.Width = 1280;
            foreach (string language in new[] { "pt-BR", "en-US" })
            {
                LanguageService.Instance.Preview(language); window.UpdateLayout(); details.BringIntoView();
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                Capture(window, Path.Combine(report, $"{language}-capture-details.png"));
            }
            LanguageService.Instance.Preview("en-US");
            Check(LanguageService.T(model.IrLibrary.SelectedDetails).Contains("Approximate capture distance:") && LanguageService.T("identificação provável") == "probable identification", "capture details and identification localized");
        }
        finally
        {
            ZoomService.Instance.Preview(originalZoom); LanguageService.Instance.Preview(originalLanguage); ThemeService.Instance.Preview(originalTheme); window.Close();
        }
    }

}
