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
        Check(entries.Select(e => e.RelativePath).Distinct().Count() == 682 && library.Roots.Single(n => n.Name == "Overdriven.fr").EntryCount == 682,
            "each catalog entry occurs once in collection/microphone tree");
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
        Check(entries.Where(e => e.Metadata!.Microphone == "LD-103").All(e => e.Metadata!.MicrophoneModel == null), "undocumented microphone models are not inferred");
        var cv = entries.First(e => e.Metadata!.Collection == "E112 / CV").Metadata!;
        var vt = entries.First(e => e.Metadata!.Collection == "E112 / VT").Metadata!;
        Check(cv.CollectionId != vt.CollectionId && cv.Cabinet == "DE-112" && vt.Cabinet == "DE-112", "CV and VT keep separate identities and documented cabinet descriptions");

        var candidate = entries.First(e => e.Metadata is { Collection: "FB / GOV", Microphone: "DYN-57", Position: "P10" });
        var selected = candidate.Metadata!;
        library.CollectionFilter = selected.CollectionId;
        library.CabinetFilter = selected.Cabinet;
        library.SpeakerFilter = selected.Speaker;
        library.MicrophoneFilter = selected.Microphone;
        library.PositionFilter = selected.Position;
        library.DistanceFilter = selected.CaptureDistanceMm.ToString();
        library.Search = "Shure SM57";
        Check(library.VisibleEntries.Count > 0 && library.VisibleEntries.All(e => e.Metadata is { Microphone: "DYN-57", Position: "P10" } m
            && m.CollectionId == selected.CollectionId && m.Cabinet == selected.Cabinet && m.Speaker == selected.Speaker && m.CaptureDistanceMm == selected.CaptureDistanceMm), "all six filters intersect with model-name search");
        library.Selected = candidate;
        Check(library.SelectedDetails.Contains("mm") && library.SelectedDetails.Contains("512") && library.SelectedDetails.Contains(candidate.Name), "capture details preserve original name and conversion limit");
        library.Search = "no-such-overdriven-ir";
        Check(library.VisibleEntries.Count == 0 && library.Selected == null, "filtering clears selection with empty results");
        library.ClearFilters.Execute(null);
        Check(library.VisibleEntries.Count == 958 && library.CollectionFilter == "" && library.Search == "", "clear restores all legacy and Overdriven entries");
        library.MicrophoneFilter = "?";
        Check(library.VisibleEntries.Count == 276 && library.VisibleEntries.All(e => e.Metadata == null), "not-provided option preserves legacy library");
        library.ClearFilters.Execute(null);

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
            Check(reference.Distance == 50 && resolved.Problem == "" && resolved.Sectors[0].SequenceEqual(IrAudio.Decode(File.ReadAllBytes(distant.FullPath)).Sector(50)),
                "pack hashes and paths resolve; physical 150 mm never initializes pedal Distance");
            File.Copy(candidate.FullPath, Path.Combine(temp, "external.wav"));
            var imported = new IrLibraryViewModel(() => true, root, temp, packPicker: true);
            imported.PositionFilter = "?";
            Check(imported.VisibleEntries.Any(e => e.RelativePath.StartsWith("Imported/")), "imported WAV remains accessible through unknown metadata filter");
            File.WriteAllText(Path.Combine(temp, "Overdriven.catalog.v1.json"), "{}");
            var invalid = new IrLibraryViewModel(() => true, temp);
            Check(invalid.Entries.Count == 1 && invalid.Entries[0].Metadata == null && invalid.Message.Contains("Metadados indisponíveis"), "malformed manifest falls back to physical WAV tree");
            File.WriteAllText(Path.Combine(temp, "Overdriven.catalog.v1.json"), JsonSerializer.Serialize(new { Version = 1, Provider = "Overdriven.fr", Entries = new[] { selected with { RelativePath = "Overdriven.fr/../outside.wav" } } }));
            Check(new IrLibraryViewModel(() => true, temp).Entries.All(e => e.Metadata == null), "unsafe manifest paths rejected without hiding WAVs");
        }
        finally { Directory.Delete(temp, recursive: true); }
        await CheckUi(candidate);
    }

    private static void CheckFile(bool condition, string path)
    {
        if (!condition) throw new Exception("Catalog/hash/decode mismatch: " + path);
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
            var collection = (ComboBox)browser.FindName("IrCollectionFilter");
            collection.SelectedValue = candidate.Metadata!.CollectionId;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(model.IrLibrary.CollectionFilter == candidate.Metadata.CollectionId && model.IrLibrary.VisibleEntries.Count == 82, "collection ComboBox updates shared viewmodel");
            ((ComboBox)browser.FindName("IrMicrophoneFilter")).SelectedValue = "DYN-57";
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(model.IrLibrary.VisibleEntries.All(e => e.Metadata!.Microphone == "DYN-57") && model.WriteCommandCount == 0, "microphone ComboBox filters without sending MIDI");
            model.IrLibrary.Selected = candidate;
            await model.PendingLibraryLoad;
            var details = (Expander)browser.FindName("IrCaptureDetails");
            details.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
            Check(model.IrLibrary.Selected == candidate && model.WriteCommandCount == 0, "opening capture details preserves selected IR without MIDI commands");
            details.IsExpanded = true;
            string report = "C:/CubeControl-Build/reports/overdriven-library";
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
                Check(scroll.ExtentWidth <= scroll.ViewportWidth + 1, $"Overdriven filters responsive {language}/{theme}/{width}/{zoom}");
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var output = File.Create(Path.Combine(report, $"{language}-{theme}-{width}-{zoom}.png")); encoder.Save(output);
            }
            ZoomService.Instance.Preview(100); window.Width = 1280;
            foreach (string language in new[] { "pt-BR", "en-US" })
            {
                LanguageService.Instance.Preview(language); window.UpdateLayout(); details.BringIntoView();
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var output = File.Create(Path.Combine(report, $"{language}-capture-details.png")); encoder.Save(output);
            }
            LanguageService.Instance.Preview("en-US");
            Check(LanguageService.T(model.IrLibrary.SelectedDetails).Contains("Approximate capture distance:") && LanguageService.T("Limpar filtros") == "Clear filters", "capture details and filters localized");
        }
        finally
        {
            ZoomService.Instance.Preview(originalZoom); LanguageService.Instance.Preview(originalLanguage); ThemeService.Instance.Preview(originalTheme); window.Close();
        }
    }

}
