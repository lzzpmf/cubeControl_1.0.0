using System.IO;
using System.Windows.Input;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

// The draft belongs to the session, so navigating away from IRs never discards it.
public sealed class IrPackEditorViewModel : Observable
{
    private readonly EditorViewModel owner;
    private string id = Guid.NewGuid().ToString("N"), name = "", description = "", search = "", message = "";
    private bool dirty, loading;
    private IrPack? current;
    public IrPackEditorViewModel(EditorViewModel owner)
    {
        this.owner = owner;
        Slots = Enumerable.Range(0, 8).Select(i => new IrPackSlotViewModel(this, i)).ToArray();
        owner.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EditorViewModel.SelectedIrPack) && !loading && !Dirty) Load(owner.SelectedIrPack);
            if (e.PropertyName == nameof(EditorViewModel.Busy)) RefreshState();
        };
        Load(owner.SelectedIrPack);
    }
    public EditorViewModel Owner => owner;
    public IrPackSlotViewModel[] Slots { get; }
    public IrPack? Current => current;
    public bool ReadOnly => current != null && current.Group != "Meus packs";
    public bool CanEdit => !ReadOnly && !owner.Busy;
    public bool Dirty { get => dirty; private set { if (Set(ref dirty, value)) RefreshState(); } }
    public string Name { get => name; set { if (Set(ref name, value)) Changed(); } }
    public string Description { get => description; set { if (Set(ref description, value)) Changed(); } }
    public string Search { get => search; set { if (Set(ref search, value)) Notify(nameof(Library)); } }
    public IEnumerable<IrLibraryEntry> Library => owner.IrLibrary.Entries.Where(e => Search.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(t => e.RelativePath.Contains(t, StringComparison.OrdinalIgnoreCase)));
    public IrLibraryEntry? SelectedIr { get; set; }
    public string Message { get => message; private set => Set(ref message, value); }
    public string DraftNotice => Dirty ? "Salve o pack antes de aplicar ao pedal." : ReadOnly ? "Pack somente leitura. Duplique para personalizar." : "";
    public string Problems => string.Join("\n", new[] {
        current?.Problem.Length > 0 ? "Pendências: " + current.Problem : "",
        Slots.Where(s => s.Entry != null).GroupBy(s => s.Entry!.Sha256, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1) ? "Este pack contém IRs repetidos." : ""
    }.Where(s => s.Length > 0));
    public ICommand NewPack => Action(async () => { if (await ConfirmDiscard()) { Load(null); Dirty = true; } });
    public ICommand DuplicatePack => Action(async () =>
    {
        if (current?.Definition == null) { Message = "Somente packs da biblioteca podem ser duplicados."; return; }
        if (!await ConfirmDiscard()) return;
        var source = current;
        string duplicateId = Guid.NewGuid().ToString("N");
        Load(source with { Id = duplicateId, Group = "Meus packs", Definition = source.Definition! with { Id = duplicateId, Group = "Meus packs" } });
        Name += Localization.LanguageService.T(" (cópia)"); Dirty = true;
    });
    public ICommand SavePack => Action(() => { Save(); return Task.CompletedTask; }, () => CanEdit);
    public ICommand DiscardChanges => Action(async () => { if (await ConfirmDiscard()) Load(owner.IrPacks.FirstOrDefault(p => p.Id == current?.Id) ?? owner.IrPacks.FirstOrDefault()); }, () => Dirty);
    public ICommand DeletePack => Action(async () =>
    {
        if (ReadOnly || current?.Definition == null || !owner.IrPacks.Any(p => p.Id == current.Id)) { Message = "Selecione um pack pessoal salvo."; return; }
        if (!await owner.Ask("Excluir este pack pessoal?")) return;
        owner.PackStore.Delete(current.Definition); Dirty = false; owner.ReloadIrPacks(); Load(owner.SelectedIrPack);
    });
    private ICommand Action(Func<Task> action, Func<bool>? enabled = null) => new UiCommand(async () =>
    {
        try { await action(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException or InvalidOperationException) { Message = ex.Message; }
    }, () => !owner.Busy && (enabled?.Invoke() ?? true));
    public Task<bool> ConfirmDiscard() => Dirty ? owner.Ask("Descartar alterações não salvas?") : Task.FromResult(true);
    public async Task<bool> SelectAsync(IrPack? pack)
    {
        if (ReferenceEquals(pack, current)) return true;
        if (owner.Busy || !await ConfirmDiscard()) return false;
        Load(pack); return true;
    }
    private void Load(IrPack? pack)
    {
        loading = true;
        current = pack; id = pack?.Definition?.Id ?? Guid.NewGuid().ToString("N");
        Name = pack?.Name ?? ""; Description = pack?.Description ?? "";
        foreach (var slot in Slots) slot.Load(pack?.Definition?.Entries.FirstOrDefault(e => e.Slot == slot.Index));
        owner.SelectedIrPack = pack;
        Message = ""; Dirty = false; loading = false;
        Notify(nameof(Current)); RefreshState();
    }
    internal void Changed()
    {
        if (loading) return;
        Dirty = true; Message = ""; Notify(nameof(Problems));
    }
    private void RefreshState()
    {
        Notify(nameof(ReadOnly)); Notify(nameof(CanEdit)); Notify(nameof(DraftNotice)); Notify(nameof(Problems));
        foreach (var slot in Slots) slot.Refresh();
        CommandManager.InvalidateRequerySuggested();
    }
    public IrPackDefinition Draft() => new() { Id = id, Name = Name.Trim(), Description = Description.Trim(), Entries = Slots.Select(s => s.Entry).OfType<IrPackEntry>().ToArray() };
    public void Save()
    {
        if (!CanEdit) return;
        var draft = Draft(); owner.PackStore.Save(draft);
        Dirty = false; owner.ReloadIrPacks(draft.Id); Load(owner.SelectedIrPack); Message = "Pack salvo.";
    }
    public void Import(string path)
    {
        var imported = owner.PackStore.Import(path);
        Dirty = false; owner.ReloadIrPacks(imported.Id); Load(owner.SelectedIrPack);
    }
    public void Export(string path)
    {
        var pack = ReadOnly ? current?.Definition : Draft();
        if (pack == null) { Message = "Somente packs da biblioteca podem ser exportados."; return; }
        IrPackStore.Validate(pack); IrPackStore.Export(path, pack);
        Message = "Pack exportado. O arquivo contém referências à biblioteca, sem WAVs.";
    }
    public void ReportError(string error) => Message = error;
    internal ICommand UseIr(IrPackSlotViewModel slot) => Action(() =>
    {
        if (SelectedIr == null) { Message = "Selecione um IR da biblioteca."; return Task.CompletedTask; }
        slot.Load(owner.PackStore.Reference(slot.Index, SelectedIr.RelativePath, IrViewModel.DisplayName(SelectedIr.FullPath, SelectedIr.Collection)));
        Changed(); return Task.CompletedTask;
    }, () => CanEdit);
    internal ICommand Remove(IrPackSlotViewModel slot) => Action(() => { slot.Load(null); Changed(); return Task.CompletedTask; }, () => CanEdit && slot.Entry != null);
    internal ICommand Move(IrPackSlotViewModel slot, int offset) => Action(() =>
    {
        var other = Slots[slot.Index + offset]; var entry = slot.Entry;
        slot.Load(other.Entry); other.Load(entry); Changed(); return Task.CompletedTask;
    }, () => CanEdit && slot.Index + offset is >= 0 and < 8);
}

public sealed class IrPackSlotViewModel : Observable
{
    private readonly IrPackEditorViewModel editor;
    internal IrPackSlotViewModel(IrPackEditorViewModel editor, int index) { this.editor = editor; Index = index; }
    public int Index { get; }
    public string Title => $"Slot {Index + 1}";
    public IrPackEntry? Entry { get; private set; }
    public string Name => Entry?.Name ?? (editor.ReadOnly && editor.Current?.Definition == null ? editor.Current?.SlotName(Index) : null) ?? "Preservar slot";
    public bool CanSetDistance => editor.CanEdit && Entry != null;
    public string Distance
    {
        get => (Entry?.Distance ?? 50).ToString();
        set
        {
            if (!CanSetDistance) return;
            if (int.TryParse(value, out int number) && number is >= 0 and <= 99)
            { if (number != Entry!.Distance) { Entry = Entry with { Distance = number }; editor.Changed(); } }
            else editor.ReportError("Use uma distância de 0 a 99.");
            Notify();
        }
    }
    public ICommand UseIr => editor.UseIr(this);
    public ICommand Remove => editor.Remove(this);
    public ICommand MoveUp => editor.Move(this, -1);
    public ICommand MoveDown => editor.Move(this, 1);
    internal void Load(IrPackEntry? entry) { Entry = entry == null ? null : entry with { Slot = Index }; Refresh(); }
    internal void Refresh() { Notify(nameof(Name)); Notify(nameof(Distance)); Notify(nameof(CanSetDistance)); }
}
