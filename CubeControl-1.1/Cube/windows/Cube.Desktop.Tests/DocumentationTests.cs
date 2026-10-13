using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Documentation;
using Cube.Desktop.Localization;
using Cube.Desktop.Views;
using Wpf.Ui.Appearance;

internal static class DocumentationTests
{
    public static async Task Run()
    {
        int count = 0;
        void Check(bool condition, string message)
        { if (!condition) throw new Exception(message); count++; Console.WriteLine("PASS " + message); }
        var rootArgument = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--documentation-root=", StringComparison.Ordinal));
        string documentRoot = rootArgument == null ? AssetPaths.Documents : rootArgument["--documentation-root=".Length..];
        var catalog = DocumentCatalog.Load(documentRoot);
        Check(catalog.Documents.Length >= 18, "published catalog includes guides and all three PDFs");
        foreach (var entry in catalog.Documents)
        {
            foreach (var version in entry.Versions)
            {
                Check(File.Exists(catalog.FilePath(version)), "packaged file exists: " + version.Path);
                if (version.Format == "markdown")
                {
                    var result = MarkdownDocument.Render(File.ReadAllText(catalog.FilePath(version)), _ => { });
                    Check(result.Document.Blocks.Count > 0 && result.Headings.Count > 1, "guide renders with table of contents: " + version.Path);
                }
            }
            if (entry.Category != "manuais") Check(entry.Select("en-US").Language == "en-US" && entry.Select("pt-BR").Language == "pt-BR", "both guide languages selected: " + entry.Id);
        }
        Check(catalog.Search("pt-BR", "", "conexao").Any(c => c.Entry.Id == "primeiros-passos"), "accent-insensitive keyword search");
        Check(catalog.Search("en-US", "", "installation usb").Any(c => c.Entry.Id == "primeiros-passos"), "search matches every query word");
        Check(!catalog.Search("en-US", "criadores", "USB installation").Any(), "category filter constrains search");
        Check(catalog.Search("en-US", "manuais", "").Count() == 3, "all manual languages remain visible");
        var first = catalog.Documents.First(d => d.Id == "primeiros-passos").Select("pt-BR");
        Check(catalog.FindLink(first, "presets.md#editar-e-ouvir")?.Entry.Id == "presets", "relative guide links resolve through published catalog");
        Check(catalog.FindLink(first, "#conectar-o-cube-baby")?.Version == first, "same-document anchor preserves version");
        Check(catalog.FindLink(first, "missing.md") == null, "unpublished file links are rejected");
        try { catalog.FindLink(first, "../../../../secret.md"); throw new Exception("Path escaped root"); }
        catch (InvalidDataException) { Check(true, "links cannot escape document root"); }
        string? target = null;
        var rendered = MarkdownDocument.Render("# Same\n\n# Same\n\n**Bold** and *italic* with `code` and [link](presets.md).\n\n1. First\n2. Second\n\n| Name | Value |\n| --- | --- |\n| Item | 2 |\n\n```text\nraw **text**\n```", value => target = value);
        Check(rendered.Headings.Select(h => h.Anchor).SequenceEqual(new[] { "same", "same-1" }), "duplicate headings use distinct anchors");
        Check(rendered.Document.Blocks.OfType<System.Windows.Documents.List>().Single().ListItems.Count == 2, "ordered list contains both items");
        Check(rendered.Document.Blocks.OfType<Table>().Single().RowGroups[0].Rows.Count == 2, "Markdown table renders header and body");
        var inline = rendered.Document.Blocks.OfType<Paragraph>().First(p => p.Inlines.OfType<Bold>().Any());
        Check(inline.Inlines.OfType<Italic>().Any() && inline.Inlines.OfType<Run>().Any(r => r.Text == "code"), "emphasis and code remain selectable native text");
        inline.Inlines.OfType<Hyperlink>().Single().RaiseEvent(new RoutedEventArgs(Hyperlink.ClickEvent));
        Check(target == "presets.md", "hyperlink dispatches navigation");
        var pdf = catalog.FilePath(catalog.Documents.First(d => d.Category == "manuais").Versions[0]);
        Check(DocumentLauncher.TryOpen(pdf, out _, value => target = value) && target == pdf, "PDF opens with the selected packaged path");
        Check(!DocumentLauncher.TryOpen(pdf, out var error, _ => throw new Win32Exception()) && error.Length > 0, "missing PDF association gives an actionable failure");
        Check(!DocumentLauncher.TryOpen(Path.Combine(documentRoot, "missing.pdf"), out _, _ => throw new Exception("should not launch")), "missing PDF is not launched");
        Check(!DocumentLauncher.TryOpen("javascript:alert(1)", out _, _ => throw new Exception("should not launch")), "unsupported URI scheme is not launched");
        var originalLanguage = LanguageService.Instance.Language;
        var originalZoom = ZoomService.Instance.Percent;
        var originalTheme = ThemeService.Instance.Mode;
        DocumentationWindow? window = null;
        string folder = Path.Combine(Path.GetTempPath(), "cube-doc-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "catalogo.v1.json"), JsonSerializer.Serialize(new { version = 1, documents = new[] {
                new { id = "bad", category = "ajuda", versions = new[] { new { language = "pt-BR", title = "Bad", format = "markdown", path = "../escape.md" } } } } }));
            try { DocumentCatalog.Load(folder); throw new Exception("Invalid catalog accepted"); }
            catch (InvalidDataException) { Check(true, "invalid catalog paths are rejected"); }
            File.WriteAllText(Path.Combine(folder, "catalogo.v1.json"), "{\"version\":1,\"documents\":null}");
            try { DocumentCatalog.Load(folder); throw new Exception("Null catalog accepted"); }
            catch (InvalidDataException) { Check(true, "null catalog contents are rejected without a crash"); }
            var invalidWindow = new DocumentationWindow(folder) { ShowInTaskbar = false };
            Check(((TextBlock)invalidWindow.FindName("ResultsStatus")).Text.Length > 0, "invalid catalog displays actionable library error");
            invalidWindow.Close();
            window = new DocumentationWindow(documentRoot) { ShowInTaskbar = false };
            window.Show(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(window.Content is Grid chrome && chrome.Children.OfType<Wpf.Ui.Controls.TitleBar>().Any(),
                "documentation uses a shared title bar outside the scaled content");
            var reader = (RichTextBox)window.FindName("Reader");
            var list = (ListBox)window.FindName("DocumentsList");
            var search = (TextBox)window.FindName("SearchBox");
            foreach (var language in new[] { "pt-BR", "en-US" })
            {
                LanguageService.Instance.Preview(language);
                foreach (var theme in new[] { ApplicationTheme.Light, ApplicationTheme.Dark, ApplicationTheme.HighContrast })
                {
                    ApplicationThemeManager.Apply(theme, Wpf.Ui.Controls.WindowBackdropType.None, true);
                    foreach (var size in new[] { (1120d, 820d, 100), (460d, 640d, 100), (460d, 640d, 200) })
                    {
                        window.Width = size.Item1; window.Height = size.Item2; ZoomService.Instance.Preview(size.Item3);
                        var entry = catalog.Documents.First(d => d.Id == "presets"); window.Open(new(entry, entry.Select(language)));
                        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                        Check(reader.ActualWidth > 0 && reader.IsReadOnly && reader.IsDocumentEnabled, $"reader usable in {language}/{theme}/{size}");
                        Check(reader.Document.Foreground == Application.Current.Resources["CubeText"], "reader uses current semantic text color");
                        string output = Path.Combine("C:\\CubeControl-Build\\documentation-qa", $"{language}-{theme}-{size.Item1}-{size.Item3}.png");
                        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(window);
                        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using var stream = File.Create(output); encoder.Save(stream);
                    }
                }
            }
            window.Width = 460; ZoomService.Instance.Preview(100); window.UpdateLayout();
            window.Navigate("presets.md#edit-and-listen");
            Check(((TextBlock)window.FindName("ReadingStatus")).Text == "", "valid document anchor navigates");
            window.Navigate("missing.md");
            Check(((TextBlock)window.FindName("ReadingStatus")).Text.Length > 0, "invalid document link displays error");
            window.Navigate("#edit-and-listen");
            Check(((TextBlock)window.FindName("ReadingStatus")).Text == "", "successful navigation clears previous link error");
            ((Wpf.Ui.Controls.Button)window.FindName("BackButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Check(((FrameworkElement)window.FindName("Library")).Visibility == Visibility.Visible, "compact back action restores library");
            search.Text = "installation usb";
            Check(list.Items.Count > 0, "search stays available after returning from reader");
            list.SelectedIndex = 0;
            Check(((FrameworkElement)window.FindName("Reading")).Visibility == Visibility.Visible && reader.Focus(), "compact selection opens reader and allows keyboard focus");
            reader.SelectAll(); Check(reader.Selection.Text.Length > 0, "guide text can be selected for copying");
            var missing = new DocumentChoice(new DocumentEntry { Id = "missing" }, new DocumentVersion { Title = "Missing", Language = "en-US", Format = "markdown", Path = "guias/en-US/missing.md" });
            window.Open(missing);
            Check(((TextBlock)window.FindName("ReadingStatus")).Text.Length > 0, "missing guide gives readable error");
        }
        finally
        {
            window?.Close(); Directory.Delete(folder, true);
            LanguageService.Instance.Preview(originalLanguage); ZoomService.Instance.Preview(originalZoom); ThemeService.Instance.Preview(originalTheme);
        }
        Console.WriteLine($"Documentation: {count} checks passed; renders in C:\\CubeControl-Build\\documentation-qa.");
    }
}
