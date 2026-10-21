#if WINUI
using ICollectionView = Cube.WinUI.Platform.ListCollectionView;
#endif
using Cube.Protocol;
#if WINUI
using Microsoft.UI.Xaml;
using Application = Cube.WinUI.Platform.UiApplication;
#else
using System.Windows;
#endif
#if WINUI
using Microsoft.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif
using Cube.Desktop.Localization;
using System.IO;
using System.Text.Json;
#if WINUI
using Cube.WinUI.Platform;
#else
using System.Windows.Data;
#endif
using System.ComponentModel;
using System.Windows.Input;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public IReadOnlyList<IrPack> IrPacks { get; private set; } = [];
    public ICollectionView? IrPackGroups { get; private set; }
    public IrPackStore PackStore => new(Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses"), DataDirectory);
    private IReadOnlyList<IrPack> originalPacks = [];
    private IrPackEditorViewModel? packEditor;
    public IrPackEditorViewModel PackEditor => packEditor ??= new(this);
    public string PackCatalogProblems { get; private set; } = "";
    public string SelectedPackSummary => SelectedIrPack is not { } p ? "" : p.Description + "\n" +
        string.Join("\n", Enumerable.Range(0, 8).Select(i => $"{i + 1}. {p.SlotName(i)}" +
            (p.Definition?.Entries.FirstOrDefault(e => e.Slot == i) is { } entry ? $" · {entry.Distance}%" : ""))) +
        (p.Problem.Length > 0 ? "\nPendências: " + p.Problem : "");
    public void ReloadIrPacks(string? selectId = null)
    {
        selectId ??= SelectedIrPack?.Id;
        var packs = new List<IrPack>();
        var errors = new List<string>();
        try
        {
            string namesFile = Path.Combine(DataDirectory, "ir-pack-names-v1.json");
            if (File.Exists(namesFile))
                foreach (var entry in JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(namesFile)) ?? [])
                    preferences.IrNames[entry.Key] = entry.Value;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { errors.Add(ex.Message); }
        try
        {
            var definitions = JsonSerializer.Deserialize<IrPackDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "IrPacks.v1.json"))) ?? [];
            packs.AddRange(definitions.Select(d => PackStore.Resolve(d with
            { Group = string.IsNullOrWhiteSpace(d.Group) ? "Predefinidos" : d.Group })));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or JsonException) { errors.Add(ex.Message); }
        try
        {
            packs.AddRange(PackStore.LoadPersonal(out string personalErrors));
            if (personalErrors.Length > 0) errors.Add(personalErrors);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { errors.Add(ex.Message); }

        IrPacks = packs;
        IrPackGroups = new ListCollectionView(packs);
        IrPackGroups.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IrPack.Group)));
        PackCatalogProblems = string.Join("\n", errors);
        SelectedIrPack = packs.FirstOrDefault(p => p.Id == selectId) ?? packs.FirstOrDefault();
        Notify(nameof(IrPacks)); Notify(nameof(IrPackGroups)); Notify(nameof(PackCatalogProblems)); Notify(nameof(SelectedPackSummary));
    }
    private void SavePackIrNames()
    {
        Directory.CreateDirectory(DataDirectory);
        string path = Path.Combine(DataDirectory, "ir-pack-names-v1.json");
        string temp = path + ".tmp";
        try { File.WriteAllText(temp, JsonSerializer.Serialize(preferences.IrNames)); File.Move(temp, path, true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    private IrPack? selectedIrPack;
    public IrPack? SelectedIrPack
    {
        get => selectedIrPack;
        set { if (Set(ref selectedIrPack, value)) { Notify(nameof(SelectedPackSummary)); CommandManager.InvalidateRequerySuggested(); } }
    }

    public Func<bool[], Task<int[]?>>? ChoosePackIrSlots { get; set; }

    public Func<IrPackReviewViewModel, Task<bool>>? ReviewPack { get; set; }
    private bool awaitingPackReview;
    private void SetAwaitingPackReview(bool value) { awaitingPackReview = value; RefreshPresentation(); }
    private async Task<bool> ShowPackReview(IrPackReviewViewModel review)
    {
        if (ReviewPack != null) return await ReviewPack(review);
#if WINUI
        return await Cube.WinUI.Platform.Dialogs.ReviewPack(review);
#else
        var dialog = new Controls.PackDialog(LanguageService.T("Revisar e aplicar") + " · " + review.Definition.Name,
            new Controls.IrPackReview { DataContext = review }, width: 1200) { IsPrimaryEnabled = review.CanContinue };
        void StateChanged(object? sender, PropertyChangedEventArgs e) { if (e.PropertyName == nameof(review.CanContinue)) dialog.IsPrimaryEnabled = review.CanContinue; }
        review.PropertyChanged += StateChanged;
        SetAwaitingPackReview(true);
        try { return await dialog.ShowAsync() == Controls.PackDialogChoice.Primary; }
        finally { review.PropertyChanged -= StateChanged; SetAwaitingPackReview(false); }
#endif
    }
    private async Task RestoreOriginalIrsAsync()
    {
        var selector = new ComboBox { ItemsSource = originalPacks, SelectedIndex = 0, DisplayMemberPath = "Name", MinWidth = 0 };
        var dialog = new Controls.PackDialog("Restaurar IRs originais", selector);
        if (await dialog.ShowAsync() == Controls.PackDialogChoice.Primary && selector.SelectedItem is IrPack pack)
            await ApplyPreparedIrPackAsync(pack, restoration: true);
    }
}
