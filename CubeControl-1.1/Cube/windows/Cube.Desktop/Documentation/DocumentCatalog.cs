using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Cube.Desktop.Documentation;

internal sealed class DocumentVersion
{
    public string Language { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string[] Keywords { get; set; } = [];
    public string Format { get; set; } = "";
    public string Path { get; set; } = "";
}

internal sealed class DocumentEntry
{
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public DocumentVersion[] Versions { get; set; } = [];
    public DocumentVersion Select(string language) => Versions.FirstOrDefault(v => v.Language == language)
        ?? Versions.FirstOrDefault(v => v.Language == "pt-BR") ?? Versions[0];
}

internal sealed record DocumentChoice(DocumentEntry Entry, DocumentVersion Version)
{
    public string Title => Localization.LanguageService.T(Version.Title);
    public string Description => $"{Localization.LanguageService.T(Version.Summary)} · {Version.Language}";
}

internal sealed class DocumentCatalog
{
    public int Version { get; set; }
    public DocumentEntry[] Documents { get; set; } = [];
    public string Root { get; private set; } = "";
    internal static readonly string[] Categories = ["comecar", "timbres", "criadores", "praticar", "ajuda", "manuais"];
    public static DocumentCatalog Load(string root)
    {
        var catalog = JsonSerializer.Deserialize<DocumentCatalog>(File.ReadAllText(System.IO.Path.Combine(root, "catalogo.v1.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("Catálogo inválido.");
        catalog.Root = System.IO.Path.GetFullPath(root);
        if (catalog.Version != 1 || catalog.Documents is not { Length: > 0 } || catalog.Documents.Any(d => d == null) ||
            catalog.Documents.Select(d => d.Id).Distinct().Count() != catalog.Documents.Length)
            throw new InvalidDataException("Catálogo inválido.");
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in catalog.Documents)
        {
            if (string.IsNullOrWhiteSpace(entry.Id) || !Categories.Contains(entry.Category) || entry.Versions is not { Length: > 0 } || entry.Versions.Any(v => v == null) ||
                entry.Versions.Select(v => v.Language).Distinct().Count() != entry.Versions.Length)
                throw new InvalidDataException("Catálogo inválido.");
            foreach (var version in entry.Versions)
            {
                if (string.IsNullOrWhiteSpace(version.Title) || string.IsNullOrWhiteSpace(version.Language) || string.IsNullOrWhiteSpace(version.Path) || version.Keywords == null ||
                    version.Format is not ("markdown" or "pdf") ||
                    !version.Path.EndsWith(version.Format == "pdf" ? ".pdf" : ".md", StringComparison.OrdinalIgnoreCase) ||
                    !paths.Add(catalog.FilePath(version))) throw new InvalidDataException("Catálogo inválido.");
            }
        }
        return catalog;
    }
    public string FilePath(DocumentVersion version) => Resolve(version.Path);
    private string Resolve(string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || System.IO.Path.IsPathRooted(relative)) throw new InvalidDataException("Caminho documental inválido.");
        var full = System.IO.Path.GetFullPath(System.IO.Path.Combine(Root, relative));
        if (!full.StartsWith(Root.TrimEnd('\\', '/') + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Caminho documental inválido.");
        return full;
    }
    public IEnumerable<DocumentChoice> Search(string language, string category, string query)
    {
        var words = Normalize(query).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return Documents.Where(d => category == "" || d.Category == category).Select(d => new DocumentChoice(d, d.Select(language)))
            .Where(c => words.All(w => Normalize(c.Title + " " + c.Version.Summary + " " + string.Join(' ', c.Version.Keywords)).Contains(w, StringComparison.Ordinal)));
    }
    public DocumentChoice? FindLink(DocumentVersion current, string target)
    {
        var file = target.Split('#')[0];
        if (file.Length == 0) file = System.IO.Path.GetFileName(current.Path);
        var relative = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(current.Path)!, Uri.UnescapeDataString(file));
        var full = Resolve(relative);
        return Documents.SelectMany(d => d.Versions.Select(v => new DocumentChoice(d, v)))
            .FirstOrDefault(c => string.Equals(FilePath(c.Version), full, StringComparison.OrdinalIgnoreCase));
    }
    internal static string Normalize(string value) => new(value.Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).Select(char.ToLowerInvariant).ToArray());
}
