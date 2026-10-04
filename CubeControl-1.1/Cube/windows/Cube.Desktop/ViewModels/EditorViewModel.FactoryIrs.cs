using Cube.Protocol;
using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.Localization;
using System.IO;
using System.Text.Json;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Input;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public IReadOnlyList<IrPack> IrPacks { get; private set; } = [];
    public ICollectionView? IrPackGroups { get; private set; }
    public IrPackStore PackStore => new(Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses"), DataDirectory);
    private IReadOnlyList<IrPack> originalPacks = [];
    public ICommand ManageIrPacks => new UiCommand(() =>
    {
        new Cube.Desktop.IrPackManagerWindow(this) { Owner = Application.Current.MainWindow }.ShowDialog();
    }, () => !Busy);
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
            packs.AddRange(definitions.Select(PackStore.Resolve));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or JsonException) { errors.Add(ex.Message); }
        try
        {
            packs.AddRange(PackStore.LoadPersonal(out string personalErrors));
            if (personalErrors.Length > 0) errors.Add(personalErrors);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { errors.Add(ex.Message); }
        packs.AddRange(originalPacks);
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
        set { if (Set(ref selectedIrPack, value)) { Notify(nameof(SelectedPackSummary)); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); } }
    }

    public Func<bool[], Task<int[]?>>? ChoosePackIrSlots { get; set; }

    private static async Task<int[]?> ShowPackIrSelection(IrPack pack, bool[] different)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = LanguageService.T("Selecione os slots que deseja substituir pelo pack escolhido. Os demais serão preservados."),
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12)
        });
        var boxes = Enumerable.Range(0, 8).Select(i => new CheckBox
        {
            Content = new TextBlock { Text = $"Slot {i + 1} · {LanguageService.T(pack.SlotName(i))}\n{LanguageService.T(!pack.HasSlot(i) ? "Preservar slot" : different[i] ? "Diferente do pack" : "Idêntico ao pack")}", TextWrapping = TextWrapping.Wrap },
            IsEnabled = different[i], IsChecked = false, Margin = new Thickness(0, 5, 0, 5)
        }).ToArray();
        foreach (var box in boxes) panel.Children.Add(box);
        var dialog = new Wpf.Ui.Controls.MessageBox
        {
            Title = LanguageService.T("Aplicar pack de IRs") + " · " + pack.Name,
            Content = new ScrollViewer { Content = panel, MaxHeight = 440, VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
            PrimaryButtonText = LanguageService.T("Continuar"), CloseButtonText = LanguageService.T("Cancelar"),
            Owner = Application.Current.MainWindow, IsPrimaryButtonEnabled = false
        };
        foreach (var box in boxes)
        {
            box.Checked += (_, _) => dialog.IsPrimaryButtonEnabled = boxes.Any(b => b.IsChecked == true);
            box.Unchecked += (_, _) => dialog.IsPrimaryButtonEnabled = boxes.Any(b => b.IsChecked == true);
        }
        return await dialog.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary
            ? Enumerable.Range(0, 8).Where(i => boxes[i].IsChecked == true).ToArray() : null;
    }
}
