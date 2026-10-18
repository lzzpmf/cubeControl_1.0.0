using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

// The draft belongs to the session, so navigating away from IRs never discards it.
public sealed class IrPackEditorViewModel : Observable
{
    private readonly EditorViewModel owner;
    private string id = Guid.NewGuid().ToString("N"), name = "", description = "", message = "";
    private bool dirty, loading;
    private IrPack? current, origin;
    private string basedOnName = "";
    public IrPackEditorViewModel(EditorViewModel owner)
    {
        this.owner = owner;
        PackLibrary = owner.IrLibrary;
        PackLibrary.SelectionChanged += value => SelectedIr = value;
        SelectedIr = PackLibrary.Selected;
        Slots = Enumerable.Range(0, 8).Select(i => new IrPackSlotViewModel(this, i)).ToArray();
        PackLibrary.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IrLibraryViewModel.VisibleEntries)) { Notify(nameof(Library)); Notify(nameof(LibraryEmpty)); }
            if (e.PropertyName == nameof(IrLibraryViewModel.Entries)) foreach (var slot in Slots) slot.Refresh();
        };
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
    public IrPack? SelectedCatalogPack => owner.IrPacks.FirstOrDefault(p => p.Id == current?.Id);
    public bool ReadOnly => current != null && current.Group != "Meus packs";
    public bool EditablePack => true;
    public bool IsCreating => IsDraftSelection && Dirty;
    public bool HasDraftNotice => Dirty;
    public string BasedOnName => basedOnName;
    public bool HasBasis => basedOnName.Length > 0;
    public string DraftNoticeTitle => IsCreating ? "Criando pack" : "Alterações não salvas";
    public string DisplayTitle => string.IsNullOrWhiteSpace(Name) ? "Montagem atual" : Name;
    public bool CanEdit => !owner.Busy && (current == null || current.Definition != null);
    public bool Dirty { get => dirty; private set { if (Set(ref dirty, value)) RefreshState(); } }
    public string Name { get => name; set { if (Set(ref name, value)) { Notify(nameof(DisplayTitle)); Changed(); } } }
    public string Description { get => description; set { if (Set(ref description, value)) Changed(); } }
    public IrLibraryViewModel PackLibrary { get; }
    public string Search { get => PackLibrary.Search; set => PackLibrary.Search = value; }
    public IEnumerable<IrLibraryEntry> Library => PackLibrary.VisibleEntries;
    public bool LibraryEmpty => !Library.Any();
    public bool CanExport => !owner.Busy && HasEntries;
    public bool IsDraftSelection => current == null || !owner.IrPacks.Any(p => p.Id == current.Id);
    public bool HasEntries => Slots.Any(s => s.Entry != null);
    public string PackSummary => $"{Slots.Count(s => s.Entry != null)} / 8 IRs";
    public bool CanDelete => !ReadOnly && current?.Definition != null && owner.IrPacks.Any(p => p.Id == current.Id);
    private IrLibraryEntry? selectedIr;
    public IrLibraryEntry? SelectedIr
    {
        get => selectedIr;
        set { if (Set(ref selectedIr, value)) { Notify(nameof(SelectedIrName)); CommandManager.InvalidateRequerySuggested(); } }
    }
    public string SelectedIrName => SelectedIr == null ? "Nenhum arquivo selecionado" : IrViewModel.DisplayName(SelectedIr.FullPath, SelectedIr.Collection);
    private readonly Dictionary<string, ImageSource?> images = [];
    internal ImageSource? SlotImage(int index, IrPackEntry? entry)
    {
        // Content identity takes precedence; name-derived art is presentation only.
        string? cabinetId = null;
        int sourceSlot = current?.Definition == null ? index : entry == null ? -1 :
            current.Definition.Entries.FirstOrDefault(e => e.Sha256 == entry.Sha256 && e.RelativePath == entry.RelativePath)?.Slot ?? -1;
        if (current != null && sourceSlot >= 0 && sourceSlot < current.Sectors.Length)
        {
            var sector = current.Sectors[sourceSlot];
            if (sector.Length == 4096 && IrPackCatalog.Identify(CabinetInventory.Hash(sector)) is { } match)
                cabinetId = IrPackCatalog.CabinetId(match.Pack, match.Index);
        }
        string? illustration = entry == null ? null : CabinetArtwork.ForIr(entry.RelativePath, entry.Name,
            PackLibrary.Entries.FirstOrDefault(item => string.Equals(item.RelativePath, entry.RelativePath, StringComparison.OrdinalIgnoreCase))?.Metadata);
        string key = cabinetId ?? illustration ?? CabinetArtwork.Generic;
        if (!images.TryGetValue(key, out var image))
        {
            image = cabinetId == null ? null : new GearItem(cabinetId, 0, "", "", "cab", "#C5B77F", cabinetId: cabinetId).DefaultImage;
            if (image == null && illustration != null)
                image = GearItem.ReadImage(AssetPaths.Image(illustration));
            image ??= GearItem.ReadImage(AssetPaths.Image(CabinetArtwork.Generic));
            images[key] = image;
        }
        return image;
    }
    public string Message { get => message; private set => Set(ref message, value); }
    public string DraftNotice => IsCreating
        ? "Sua montagem ainda não foi salva. Você pode continuar editando, salvar ou revisar e aplicar."
        : "Salvar atualiza este pack pessoal. Você também pode descartar as alterações.";

    public string Problems => string.Join("\n", new[] {
        string.Join("\n", Slots.Select(s => s.Problem).Where(p => p.Length > 0)),
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
        string suffix = Localization.LanguageService.T(" (cópia)");
        Name = Name[..Math.Min(Name.Length, 120 - suffix.Length)] + suffix; Dirty = true;
    });
    public ICommand SavePack => Action(async () => { if (await EnsureDetailsAsync(true)) Save(); }, () => CanEdit && !ReadOnly && HasEntries);
    public ICommand DiscardChanges => Action(async () => { if (await owner.Ask("Descartar alterações não salvas?")) Load(owner.IrPacks.FirstOrDefault(p => p.Id == origin?.Id) ?? owner.IrPacks.FirstOrDefault()); }, () => Dirty);
    public ICommand DeletePack => Action(async () =>
    {
        if (ReadOnly || current?.Definition == null || !owner.IrPacks.Any(p => p.Id == current.Id)) { Message = "Selecione um pack pessoal salvo."; return; }
        if (!await owner.Ask("Excluir este pack pessoal?")) return;
        owner.PackStore.Delete(current.Definition); Dirty = false; owner.ReloadIrPacks(); Load(owner.SelectedIrPack);
    }, () => CanDelete);
    private ICommand Action(Func<Task> action, Func<bool>? enabled = null) => new UiCommand(async () =>
    {
        try { await action(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException or InvalidOperationException) { Message = ex.Message; }
    }, () => !owner.Busy && (enabled?.Invoke() ?? true));
    public Func<Task<PackDraftDecision>>? ChooseDraftDecision { get; set; }
    public async Task<bool> ConfirmDiscard()
    {
        if (!Dirty) return true;
        PackDraftDecision choice;
        if (ChooseDraftDecision != null) choice = await ChooseDraftDecision();
        else
        {
            var dialog = new Controls.PackDialog("Alterações no pack",
                Localization.LanguageService.T("Salve sua montagem para reutilizar, descarte as alterações ou continue editando."), "Salvar", "Descartar") { IsPrimaryEnabled = HasEntries };
            choice = await dialog.ShowAsync() switch {
                Controls.PackDialogChoice.Primary => PackDraftDecision.Save,
                Controls.PackDialogChoice.Secondary => PackDraftDecision.Discard,
                _ => PackDraftDecision.Cancel };
        }
        if (choice == PackDraftDecision.Cancel) return false;
        if (choice == PackDraftDecision.Save)
        {
            if (!await EnsureDetailsAsync(true)) return false;
            try { Save(); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException) { ReportError(ex.Message); return false; }
        }
        return true;
    }
    public Func<string, string, Task<(string Name, string Description)?>>? ChooseDetails { get; set; }
    public async Task<bool> EnsureDetailsAsync(bool always = false)
    {
        if (!always && !string.IsNullOrWhiteSpace(Name)) return true;
        if (ChooseDetails != null)
        {
            var details = await ChooseDetails(Name, Description);
            if (details == null || string.IsNullOrWhiteSpace(details.Value.Name)) return false;
            Name = details.Value.Name; Description = details.Value.Description; return true;
        }
        var title = new System.Windows.Controls.TextBox { Text = Name, MinWidth = 0, MaxLength = 120 };
        var notes = new System.Windows.Controls.TextBox { Text = Description, AcceptsReturn = true, TextWrapping = System.Windows.TextWrapping.Wrap, MinHeight = 64, MaxLength = 4000 };
        System.Windows.Automation.AutomationProperties.SetName(title, Localization.LanguageService.T("Nome do pack"));
        System.Windows.Automation.AutomationProperties.SetName(notes, Localization.LanguageService.T("Descrição do pack"));
        var panel = new System.Windows.Controls.StackPanel();
        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = Localization.LanguageService.T("Nome do pack") }); panel.Children.Add(title);
        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = Localization.LanguageService.T("Descrição do pack"), Margin = new System.Windows.Thickness(0,12,0,8) }); panel.Children.Add(notes);
        var dialog = new Controls.PackDialog("Detalhes do pack", panel, always ? "Salvar" : "Continuar") { IsPrimaryEnabled = !string.IsNullOrWhiteSpace(Name) };
        title.TextChanged += (_, _) => dialog.IsPrimaryEnabled = !string.IsNullOrWhiteSpace(title.Text);
        title.Loaded += (_, _) => title.Focus();
        if (await dialog.ShowAsync() != Controls.PackDialogChoice.Primary) return false;
        Name = title.Text; Description = notes.Text; return true;
    }
    public async Task<bool> SelectAsync(IrPack? pack)
    {
        if (ReferenceEquals(pack, current)) return true;
        if (owner.Busy || !await ConfirmDiscard()) return false;
        // Saving during the switch reloads the catalog and replaces its item instances.
        Load(pack == null ? null : owner.IrPacks.FirstOrDefault(p => p.Id == pack.Id) ?? pack); return true;
    }
    private void Load(IrPack? pack)
    {
        loading = true;
        current = pack; origin = owner.IrPacks.FirstOrDefault(p => p.Id == pack?.Id); basedOnName = ""; id = pack?.Definition?.Id ?? Guid.NewGuid().ToString("N");
        Name = pack?.Name ?? ""; Description = pack?.Description ?? "";
        foreach (var slot in Slots) slot.Load(pack?.Definition?.Entries.FirstOrDefault(e => e.Slot == slot.Index));
        owner.SelectedIrPack = pack;
        Message = ""; Dirty = false; loading = false;
        Notify(nameof(Current)); RefreshState();
    }
    internal void Changed()
    {
        if (loading) return;
        if (ReadOnly && current?.Definition != null)
        {
            origin = current; basedOnName = current.Name;
            id = Guid.NewGuid().ToString("N");
            current = current with { Id = id, Group = "Meus packs", Definition = current.Definition with { Id = id, Group = "Meus packs" } };
            loading = true;
            try { owner.SelectedIrPack = current; } finally { loading = false; }
            Notify(nameof(Current));
        }
        Dirty = true; Message = ""; RefreshState();
    }
    private void RefreshState()
    {
        Notify(nameof(IsCreating)); Notify(nameof(HasDraftNotice)); Notify(nameof(DraftNoticeTitle)); Notify(nameof(BasedOnName)); Notify(nameof(HasBasis));
        Notify(nameof(SelectedCatalogPack)); Notify(nameof(IsDraftSelection)); Notify(nameof(CanExport)); Notify(nameof(HasEntries)); Notify(nameof(PackSummary)); Notify(nameof(CanDelete)); Notify(nameof(ReadOnly)); Notify(nameof(EditablePack)); Notify(nameof(DisplayTitle)); Notify(nameof(CanEdit)); Notify(nameof(DraftNotice)); Notify(nameof(Problems));
        foreach (var slot in Slots) slot.Refresh();
        CommandManager.InvalidateRequerySuggested();
    }
    public IrPackDefinition Draft() => new() { Id = id, Name = Name.Trim(), Description = Description.Trim(), Entries = Slots.Select(s => s.Entry).OfType<IrPackEntry>().ToArray() };
    public void Save()
    {
        if (!CanEdit || ReadOnly || !HasEntries) return;
        var draft = Draft(); owner.PackStore.Save(draft);
        Dirty = false; owner.ReloadIrPacks(draft.Id); Load(owner.SelectedIrPack); Message = "Pack salvo.";
    }
    public void Import(string path)
    {
        if (owner.Busy) return;
        var previousOrigin = origin;
        var imported = owner.PackStore.ImportDraft(path);
        PackLibrary.Reload();
        Load(owner.PackStore.Resolve(imported)); origin = previousOrigin; Dirty = true;
        Message = "Pack importado para revisão. Salvar e aplicar são ações separadas.";
    }
    public void Export(string path)
    {
        if (owner.Busy) return;
        var pack = ReadOnly ? current?.Definition : Draft();
        if (pack == null) { Message = "Selecione ou monte um pack com IRs da biblioteca."; return; }
        IrPackStore.Validate(pack);
        owner.PackStore.ExportPortable(path, pack);
        Message = "Pack exportado com os WAVs. Pode ser importado em outro computador.";
    }
    public IrPackDefinition ApplicationDraft() => Draft() with { Name = string.IsNullOrWhiteSpace(Name) ? "Montagem atual" : Name.Trim() };
    public void ReportError(string error) => Message = error;
    public bool AssignIr(int index, IrLibraryEntry entry)
    {
        if (!CanEdit || index is < 0 or > 7 || !PackLibrary.Entries.Contains(entry)) return false;
        var reference = owner.PackStore.Reference(index, entry.RelativePath, IrViewModel.DisplayName(entry.FullPath, entry.Collection));
        if (Slots[index].Entry == reference) return false;
        Slots[index].Load(reference);
        Changed(); return true;
    }
    public bool SwapSlots(int source, int destination)
    {
        if (!CanEdit || source is < 0 or > 7 || destination is < 0 or > 7 || source == destination || Slots[source].Entry == null) return false;
        var entry = Slots[source].Entry;
        if (entry == (Slots[destination].Entry == null ? null : Slots[destination].Entry! with { Slot = source })) return false;
        Slots[source].Load(Slots[destination].Entry); Slots[destination].Load(entry); Changed(); return true;
    }
    internal ICommand UseIr(IrPackSlotViewModel slot) => Action(() =>
    {
        if (SelectedIr != null) AssignIr(slot.Index, SelectedIr);
        return Task.CompletedTask;
    }, () => CanEdit && SelectedIr != null);
    internal ICommand Remove(IrPackSlotViewModel slot) => Action(() => { slot.Load(null); Changed(); return Task.CompletedTask; }, () => CanEdit && slot.Entry != null);
    internal ICommand Move(IrPackSlotViewModel slot, int offset) => Action(() =>
    {
        SwapSlots(slot.Index, slot.Index + offset); return Task.CompletedTask;
    }, () => CanEdit && slot.Entry != null && slot.Index + offset is >= 0 and < 8);

}

public enum PackDraftDecision { Save, Discard, Cancel }

public sealed class IrPackSlotViewModel : Observable
{
    private readonly IrPackEditorViewModel editor;
    internal IrPackSlotViewModel(IrPackEditorViewModel editor, int index) { this.editor = editor; Index = index; }
    public int Index { get; }
    public string Title => $"Slot {Index + 1}";
    public IrPackEntry? Entry { get; private set; }
    public string Name => Entry?.Name ?? (editor.ReadOnly && editor.Current?.Definition == null ? editor.Current?.SlotName(Index) : null) ?? "Preservar slot do pedal";
    public string CaptureSummary
    {
        get
        {
            var metadata = editor.PackLibrary.Entries.FirstOrDefault(e => e.RelativePath == Entry?.RelativePath)?.Metadata;
            if (metadata == null) return "";
            return string.Join(" · ", new[] { metadata.MicrophoneLabel, metadata.Position,
                metadata.SampleRate > 0 && metadata.Bits > 0 ? $"{metadata.SampleRate / 1000d:0.###} kHz / {metadata.Bits}-bit" : "" }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
    public string Problem { get; private set; } = "";
    public bool Editable => editor.EditablePack;
    public bool HasEntry => Entry != null;
    public bool ShowRemove => Editable && HasEntry;
    public ImageSource? Image => editor.SlotImage(Index, Entry);
    public string ImageHint => Entry != null && CabinetArtwork.ForIr(Entry.RelativePath, Entry.Name) != null
        ? "Arte ilustrativa baseada nos metadados ou no nome do IR; não confirma o gabinete da captura."
        : "Imagem de referência do gabinete.";
    public double DistanceValue
    {
        get => Entry?.Distance ?? 80;
        set { if (double.IsFinite(value)) Distance = Math.Clamp((int)Math.Round(value), 0, 99).ToString(); }
    }
    public bool CanSetDistance => editor.CanEdit && Entry != null;
    public string Distance
    {
        get => (Entry?.Distance ?? 80).ToString();
        set
        {
            if (!CanSetDistance) return;
            if (int.TryParse(value, out int number) && number is >= 0 and <= 99)
            { if (number != Entry!.Distance) { Entry = Entry with { Distance = number }; editor.Changed(); } }
            else editor.ReportError("Use uma distância de 0 a 99.");
            Notify(); Notify(nameof(DistanceValue));
        }
    }
    public ICommand UseIr => editor.UseIr(this);
    public ICommand Remove => editor.Remove(this);
    public ICommand MoveUp => editor.Move(this, -1);
    public ICommand MoveDown => editor.Move(this, 1);
    internal void Load(IrPackEntry? entry)
    {
        Entry = entry == null ? null : entry with { Slot = Index };
        Problem = Entry == null ? "" : editor.Owner.PackStore.Resolve(new IrPackDefinition { Name = "Slot", Entries = [Entry] }).Problem;
        Refresh();
    }
    internal void Refresh() { Notify(nameof(ShowRemove)); Notify(nameof(Editable)); Notify(nameof(HasEntry)); Notify(nameof(Name)); Notify(nameof(CaptureSummary)); Notify(nameof(Problem)); Notify(nameof(Distance)); Notify(nameof(DistanceValue)); Notify(nameof(Image)); Notify(nameof(ImageHint)); Notify(nameof(CanSetDistance)); }
}
