using System.IO;

namespace Cube.Desktop.ViewModels;

public sealed record IrLibraryEntry(string RelativePath, string FullPath, string Collection, string Name);

public sealed class IrLibraryNode(string name, IrLibraryEntry? entry, Action<IrLibraryNode> select) : Observable
{
    private bool expanded, selected;
    public string Name { get; } = name;
    public IrLibraryEntry? Entry { get; } = entry;
    public List<IrLibraryNode> Children { get; } = [];
    public bool IsFolder => Entry == null;
    public bool IsBrand { get; init; }
    internal IReadOnlyList<IrLibraryNode> Siblings { get; init; } = [];
    public int EntryCount => Entry != null ? 1 : Children.Sum(n => n.EntryCount);
    public string DisplayName => IsFolder ? $"{Name} ({EntryCount})" : Name;
    public string ToolTip => Entry?.RelativePath ?? DisplayName;
    public bool IsExpanded
    {
        get => expanded;
        set
        {
            if (!IsFolder || !Set(ref expanded, value)) return;
            if (value && IsBrand)
            {
                foreach (var sibling in Siblings)
                    if (sibling != this) sibling.IsExpanded = false;
            }
        }
    }
    public bool IsSelected { get => selected; set { if (Set(ref selected, value) && value) select(this); } }
}

public sealed class IrLibraryViewModel : Observable
{
    private readonly Func<bool> canSelect;
    private string search = "";
    private IrLibraryEntry? selected;
    private bool synchronizingSelection;
    public IReadOnlyList<IrLibraryNode> Roots { get; private set; } = [];
    public IReadOnlyList<IrLibraryEntry> Entries { get; }
    public IReadOnlyList<IrLibraryEntry> VisibleEntries { get; private set; } = [];
    public string Message { get; private set; } = "";
    public event Action<IrLibraryEntry?>? SelectionChanged;
    public string Search { get => search; set { if (canSelect() && Set(ref search, value)) Filter(); } }
    public IrLibraryEntry? Selected
    {
        get => selected;
        set
        {
            if (!canSelect() || !Set(ref selected, value)) return;
            SynchronizeSelection();
            SelectionChanged?.Invoke(value);
        }
    }
    internal void ClearSelection()
    {
        bool changed = Set(ref selected, null, nameof(Selected));
        SynchronizeSelection();
        if (changed) SelectionChanged?.Invoke(null);
    }
    public IrLibraryViewModel(Func<bool> canSelect, string? directory = null)
    {
        this.canSelect = canSelect;
        directory ??= Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses");
        try
        {
            Entries = Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(p => string.Equals(Path.GetExtension(p), ".wav", StringComparison.OrdinalIgnoreCase))
                .Select(p => new IrLibraryEntry(Path.GetRelativePath(directory, p), p,
                    Path.GetDirectoryName(Path.GetRelativePath(directory, p)) is { Length: > 0 } folder ? folder : "ImpulseResponses",
                    Path.GetFileNameWithoutExtension(p)))
                .OrderBy(e => e.Collection, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToArray() : [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { Entries = []; }
        Filter();
    }
    private void Filter()
    {
        ClearSelection();
        var terms = Search.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        VisibleEntries = Entries.Where(e => terms.All(term =>
            e.RelativePath.Contains(term, StringComparison.OrdinalIgnoreCase))).ToArray();
        var roots = new List<IrLibraryNode>();
        foreach (var entry in VisibleEntries)
        {
            var children = roots;
            string? directory = Path.GetDirectoryName(entry.RelativePath);
            string[] parts = (directory ?? "").Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
            // The disk separates WAV/NAM assets; the WAV-only picker does not need that extra level.
            if (parts.Length > 1 && parts[1].Equals("IRs WAV", StringComparison.OrdinalIgnoreCase))
                parts = parts.Where((_, index) => index != 1).ToArray();
            foreach (string part in parts)
            {
                var folder = children.FirstOrDefault(n => n.IsFolder && n.Name.Equals(part, StringComparison.OrdinalIgnoreCase));
                if (folder == null)
                {
                    folder = new(part, null, SelectNode) { IsBrand = ReferenceEquals(children, roots), Siblings = children };
                    children.Add(folder);
                }
                children = folder.Children;
            }
            string label = entry.Name;
            string collection = Path.GetFileName(directory) ?? "";
            if (collection.Length > 0 && label.StartsWith(collection + " ", StringComparison.OrdinalIgnoreCase))
                label = label[collection.Length..].TrimStart(' ', '-');
            children.Add(new(label, entry, SelectNode));
        }
        Roots = roots.OrderBy(n => n.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        // Search reveals the first matching path while keeping the accordion invariant.
        if (terms.Length > 0)
        {
            var level = Roots;
            while (level.FirstOrDefault(n => n.IsFolder) is { } folder)
            {
                folder.IsExpanded = true;
                level = folder.Children;
            }
        }
        Message = Entries.Count == 0 ? "Biblioteca indisponível ou vazia. Você pode selecionar um WAV externo."
            : VisibleEntries.Count == 0 ? "Nenhum IR encontrado." : "Selecione um IR para ouvir no pedal conectado. A gravação no slot é separada.";
        Notify(nameof(Roots)); Notify(nameof(VisibleEntries)); Notify(nameof(Message));
    }
    private void SelectNode(IrLibraryNode node)
    {
        if (synchronizingSelection) return;
        if (!canSelect()) { node.IsSelected = false; return; }
        synchronizingSelection = true;
        try
        {
            foreach (var other in Nodes(Roots)) if (other != node) other.IsSelected = false;
            Selected = node.Entry; // Folders cancel pending loading but never change the prepared audio.
        }
        finally { synchronizingSelection = false; }
    }
    private void SynchronizeSelection()
    {
        if (synchronizingSelection) return;
        synchronizingSelection = true;
        try { foreach (var node in Nodes(Roots)) node.IsSelected = selected != null && node.Entry == selected; }
        finally { synchronizingSelection = false; }
    }
    private static IEnumerable<IrLibraryNode> Nodes(IEnumerable<IrLibraryNode> roots)
    {
        foreach (var node in roots) { yield return node; foreach (var child in Nodes(node.Children)) yield return child; }
    }
}
