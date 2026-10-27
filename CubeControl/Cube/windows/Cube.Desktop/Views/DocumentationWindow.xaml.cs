using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Cube.Desktop.Appearance;
using Cube.Desktop.Documentation;
using Cube.Desktop.Localization;

namespace Cube.Desktop.Views;

public partial class DocumentationWindow : Wpf.Ui.Controls.FluentWindow
{
    private sealed record Category(string Id, string Label);
    private DocumentCatalog? catalog;
    private DocumentChoice? current;
    private RenderedDocument? rendered;
    private bool reading, initialized, selecting;
    private readonly string catalogRoot;
    public DocumentationWindow() : this(AssetPaths.Documents) { }
    internal DocumentationWindow(string root)
    {
        catalogRoot = root;
        InitializeComponent();
        LanguageService.Instance.PropertyChanged += LanguageChanged;
        ZoomService.Instance.Changed += RefreshScale;
        Closed += (_, _) =>
        {
            LanguageService.Instance.PropertyChanged -= LanguageChanged;
            ZoomService.Instance.Changed -= RefreshScale;
        };
        initialized = true;
        PreviewKeyDown += (_, e) =>
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
            int? percent = e.Key switch
            {
                Key.Add or Key.OemPlus => ZoomService.Instance.Percent + 10,
                Key.Subtract or Key.OemMinus => ZoomService.Instance.Percent - 10,
                Key.D0 or Key.NumPad0 => 100,
                _ => null
            };
            if (percent.HasValue) { ZoomService.Instance.Select(percent.Value); e.Handled = true; }
        };
        RefreshCategories(); RefreshScale();
        ReadingStatus.Text = LanguageService.T("Selecione um documento para ler.");
        try { catalog = DocumentCatalog.Load(catalogRoot); RefreshList(); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            ReadingStatus.Text = LanguageService.T("Não foi possível carregar a documentação. Extraia novamente o pacote completo.");
            ResultsStatus.Text = ReadingStatus.Text;
        }
    }
    private void RefreshCategories()
    {
        var id = (CategoryBox.SelectedItem as Category)?.Id ?? "";
        string[] names = ["Todos os assuntos", "Começar", "Pedaleira e timbres", "Criadores", "Praticar", "Ajuda", "Manuais do pedal"];
        var ids = new[] { "" }.Concat(DocumentCatalog.Categories).ToArray();
        CategoryBox.ItemsSource = ids.Select((value, i) => new Category(value, LanguageService.T(names[i]))).ToArray();
        CategoryBox.SelectedIndex = Array.IndexOf(ids, id);
    }
    private void RefreshList()
    {
        if (!initialized || catalog == null) return;
        DocumentsList.ItemsSource = catalog.Search(LanguageService.Instance.Language, (CategoryBox.SelectedItem as Category)?.Id ?? "", SearchBox.Text).ToArray();
        ResultsStatus.Text = DocumentsList.Items.Count == 0 ? LanguageService.T("Nenhum documento encontrado.") : LanguageService.T("Selecione um documento para ler.");
    }
    private void LanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LanguageService.Language)) return;
        RefreshCategories(); RefreshList();
        if (current != null) Open(new(current.Entry, current.Entry.Select(LanguageService.Instance.Language)));
    }
    private void RefreshScale()
    {
        Root.LayoutTransform = new ScaleTransform(UiDensity.Factor, UiDensity.Factor); UpdateLayoutMode();
    }
    private void WindowSizeChanged(object sender, SizeChangedEventArgs e) { if (initialized) UpdateLayoutMode(); }
    private void UpdateLayoutMode()
    {
        bool compact = (ActualWidth > 0 ? ActualWidth : Width) / UiDensity.Factor < 900;
        Header.Visibility = compact && reading ? Visibility.Collapsed : Visibility.Visible;
        Reading.Padding = new Thickness(compact ? 12 : 24);
        ListColumn.Width = compact ? new GridLength(reading ? 0 : 1, GridUnitType.Star) : new GridLength(320);
        ReaderColumn.Width = compact ? new GridLength(reading ? 1 : 0, GridUnitType.Star) : new GridLength(1, GridUnitType.Star);
        Library.Visibility = compact && reading ? Visibility.Collapsed : Visibility.Visible;
        Library.Margin = new Thickness(0, 0, compact ? 0 : 16, 0);
        Reading.Visibility = compact && !reading ? Visibility.Collapsed : Visibility.Visible;
        BackButton.Visibility = compact ? Visibility.Visible : Visibility.Collapsed;
    }
    private void FilterChanged(object sender, TextChangedEventArgs e) => RefreshList();
    private void CategoryChanged(object sender, SelectionChangedEventArgs e) => RefreshList();
    private void DocumentSelected(object sender, SelectionChangedEventArgs e)
    { if (!selecting && DocumentsList.SelectedItem is DocumentChoice choice) Open(choice); }
    internal void Open(DocumentChoice choice)
    {
        current = choice; reading = true;
        selecting = true;
        DocumentsList.SelectedItem = DocumentsList.Items.Cast<DocumentChoice>().FirstOrDefault(c => c.Version.Path == choice.Version.Path);
        selecting = false;
        PdfButton.Visibility = choice.Version.Format == "pdf" ? Visibility.Visible : Visibility.Collapsed;
        ContentsBox.ItemsSource = null; ContentsBox.Visibility = Visibility.Collapsed;
        rendered = null; ReadingStatus.Text = "";
        try
        {
            string path = catalog!.FilePath(choice.Version);
            if (!File.Exists(path)) throw new FileNotFoundException();
            var text = choice.Version.Format == "markdown" ? File.ReadAllText(path) :
                "# " + choice.Title + "\n\n" + choice.Description + "\n\n" + LanguageService.T("Use Abrir PDF para consultar o manual no leitor do Windows.");
            rendered = MarkdownDocument.Render(text, Navigate);
            Reader.Document = rendered.Document; Reader.ScrollToHome();
            ContentsBox.ItemsSource = rendered.Headings;
            ContentsBox.SelectedIndex = 0;
            ContentsBox.Visibility = choice.Version.Format == "markdown" ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException)
        {
            Reader.Document = MarkdownDocument.Render("# " + choice.Title, Navigate).Document;
            ReadingStatus.Text = LanguageService.T("Documento ausente ou inválido. Extraia novamente o pacote completo.");
        }
        UpdateLayoutMode(); Reader.Focus();
    }
    internal void Navigate(string target)
    {
        if (current == null || catalog == null) return;
        ReadingStatus.Text = "";
        try
        {
            if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme is not ("http" or "https" or "mailto")) throw new InvalidDataException();
                if (!DocumentLauncher.TryOpen(target, out var error)) ReadingStatus.Text = LanguageService.T(error);
                return;
            }
            var choice = catalog.FindLink(current.Version, target) ?? throw new InvalidDataException();
            var anchor = target.Contains('#') ? Uri.UnescapeDataString(target[(target.IndexOf('#') + 1)..]) : "";
            if (choice.Version.Path != current.Version.Path) Open(choice);
            if (anchor.Length > 0)
            {
                var heading = rendered?.Headings.FirstOrDefault(h => h.Anchor == anchor) ?? throw new InvalidDataException();
                heading.Paragraph.BringIntoView(); ContentsBox.SelectedItem = heading;
            }
            else Reader.ScrollToHome();
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or UriFormatException)
        { ReadingStatus.Text = LanguageService.T("Link documental inválido ou indisponível."); }
    }
    private void HeadingSelected(object sender, SelectionChangedEventArgs e)
    { if (ContentsBox.SelectedItem is DocumentHeading heading) heading.Paragraph.BringIntoView(); }
    private void BackClick(object sender, RoutedEventArgs e)
    {
        reading = false; UpdateLayoutMode();
        // Clearing allows opening the same item again in compact mode.
        DocumentsList.SelectedItem = null; DocumentsList.Focus();
    }
    private void PdfClick(object sender, RoutedEventArgs e)
    {
        ReadingStatus.Text = "";
        if (current != null && catalog != null && !DocumentLauncher.TryOpen(catalog.FilePath(current.Version), out var error))
            ReadingStatus.Text = LanguageService.T(error);
    }
}
