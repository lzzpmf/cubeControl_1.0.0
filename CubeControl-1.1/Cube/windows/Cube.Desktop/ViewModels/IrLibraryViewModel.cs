using System.IO;

namespace Cube.Desktop.ViewModels;

public sealed record IrLibraryEntry(string RelativePath, string FullPath, string Collection, string Name)
{
    public IrLibraryMetadata? Metadata { get; init; }
    public string SearchText => RelativePath + " " + Name + " " + Metadata?.SearchText;
}

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
    public string ToolTip => Entry is { } entry ? Name + "\n" + entry.RelativePath : DisplayName;
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
    public IReadOnlyList<IrLibraryEntry> Entries { get; private set; } = [];
    private readonly string directory;
    private readonly string? importedDirectory;
    private readonly bool packPicker;
    public IReadOnlyList<IrLibraryEntry> VisibleEntries { get; private set; } = [];
    public string Message { get; private set; } = "";
    private string catalogProblem = "";
    public string SelectedDetails
    {
        get
        {
            if (Selected?.Metadata is not { } m) return Selected == null ? "" : $"Nome original: {Selected.Name}\n{Selected.RelativePath}\nMetadados de captura não informados.";
            return $"Origem: Overdriven.fr · {m.Collection}" + (m.IsPreview ? " · SP1-PREVIEW" : "")
                + $"\nCatálogo: {m.Brand} · {m.SpeakerConfiguration} · {m.ModelLabel}\nIdentificação: {m.IdentificationLabel}\nEvidência: {m.IdentificationEvidence}\nFontes: {string.Join(" · ", m.Sources)}"
                + $"\nCaixa: {m.Cabinet}\nFalante: {m.Speaker}\nMicrofone: {m.MicrophoneLabel}"
                + $"\nPosição: {m.Position} · {m.PositionDescription}\nDistância de captura aproximada: {m.CaptureDistanceMm} mm"
                + $"\nPotência: {m.PowerAmp} · Pré de microfone: {m.MicPreamp}"
                + $"\nVariantes: {(m.Variants.Length == 0 ? "Nenhuma" : string.Join("-", m.Variants))} · {m.CapturePrefix}"
                + $"\nFormato original: {m.Format} · {m.Bits} bits · {m.SampleRate} Hz · {m.Channels} canal · {m.DurationMs:0.##} ms"
                + "\nConversão no pedal: mono · 48 kHz · 512 amostras (≈10,7 ms), sem normalização."
                + "\nDistância de captura não altera o controle Distance do pedal."
                + $"\nNome original: {m.OriginalName}\n{Selected.RelativePath}";
        }
    }
    public event Action<IrLibraryEntry?>? SelectionChanged;
    public string Search { get => search; set { if (canSelect() && Set(ref search, value)) Filter(); } }
    public IrLibraryEntry? Selected
    {
        get => selected;
        set
        {
            if (!canSelect() || !Set(ref selected, value)) return;
            SynchronizeSelection();
            Notify(nameof(SelectedDetails));
            SelectionChanged?.Invoke(value);
        }
    }
    internal void ClearSelection()
    {
        bool changed = Set(ref selected, null, nameof(Selected));
        SynchronizeSelection();
        Notify(nameof(SelectedDetails));
        if (changed) SelectionChanged?.Invoke(null);
    }
    public IrLibraryViewModel(Func<bool> canSelect, string? directory = null, string? importedDirectory = null, bool packPicker = false)
    {
        this.canSelect = canSelect;
        this.directory = directory ?? Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses");
        this.importedDirectory = importedDirectory;
        this.packPicker = packPicker;
        Reload();
    }
    public void Reload()
    {
        var entries = new List<IrLibraryEntry>();
        var catalog = IrLibraryCatalog.Load(directory, out catalogProblem);
        void Scan(string folder, string prefix)
        {
            try
            {
                if (!Directory.Exists(folder)) return;
                foreach (var p in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                    .Where(p => string.Equals(Path.GetExtension(p), ".wav", StringComparison.OrdinalIgnoreCase)))
                {
                    string relative = prefix + Path.GetRelativePath(folder, p);
                    string collection = prefix.Length > 0 ? "IRs importados" : Path.GetDirectoryName(relative) ?? "ImpulseResponses";
                    catalog.TryGetValue(relative.Replace('\\', '/'), out var metadata);
                    if (prefix.Length > 0) metadata = null;
                    entries.Add(new(relative, p, metadata?.Collection ?? collection, Path.GetFileNameWithoutExtension(p)) { Metadata = metadata });
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        Scan(directory, "");
        if (importedDirectory != null) Scan(importedDirectory, "Imported/");
        Entries = entries.OrderBy(e => e.Metadata is { } m ? m.Brand + "/" + m.SpeakerConfiguration + "/" + m.ModelLabel : e.Collection, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        Notify(nameof(Entries)); Filter();
    }
    private void Filter()
    {
        ClearSelection();
        var terms = Search.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        VisibleEntries = Entries.Where(e => terms.All(term => e.SearchText.Contains(term, StringComparison.OrdinalIgnoreCase))).ToArray();
        var roots = new List<IrLibraryNode>();
        foreach (var entry in VisibleEntries)
        {
            var children = roots;
            string? directory = entry.RelativePath.StartsWith("Imported/") ? "IRs importados" : Path.GetDirectoryName(entry.RelativePath);
            string[] parts = (directory ?? "").Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
            if (entry.Metadata is { } metadata)
                parts = [metadata.Brand, metadata.SpeakerConfiguration, metadata.ModelLabel];
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
            if (entry.Metadata is { } details) label = details.LeafName;
            string collection = Path.GetFileName(directory) ?? "";
            if (entry.Metadata == null && collection.Length > 0 && label.StartsWith(collection + " ", StringComparison.OrdinalIgnoreCase))
                label = label[collection.Length..].TrimStart(' ', '-');
            children.Add(new(label, entry, SelectNode));
        }
        foreach (var parent in Nodes(roots).Where(n => n.IsFolder))
        {
            var duplicates = parent.Children.Where(n => !n.IsFolder).GroupBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1).SelectMany(g => g).ToArray();
            foreach (var node in duplicates)
            {
                int index = parent.Children.IndexOf(node);
                string suffix = duplicates.Count(n => n.Entry!.Name == node.Entry!.Name) > 1 ? node.Entry!.RelativePath : node.Entry!.Name;
                parent.Children[index] = new(node.Name + " · " + suffix, node.Entry, SelectNode);
            }
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
        Message = Entries.Count == 0 ? packPicker ? "Biblioteca vazia. Importe um pack com WAVs para montar seus IRs." : "Biblioteca indisponível ou vazia. Você pode selecionar um WAV externo."
            : VisibleEntries.Count == 0 ? "Nenhum IR encontrado." : packPicker ? "Selecione um IR e use-o no slot desejado. Nenhum áudio é enviado ao pedal." : "Selecione um IR para ouvir no pedal conectado. A gravação no slot é separada.";
        if (catalogProblem.Length > 0) Message += " " + catalogProblem;
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
