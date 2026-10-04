using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop;

public sealed class IrPackManagerWindow : Window
{
    private readonly EditorViewModel model;
    private readonly ComboBox packs = new() { MinWidth = 260, DisplayMemberPath = "Name" };
    private readonly TextBox name = new(), description = new() { AcceptsReturn = true, MinHeight = 55, TextWrapping = TextWrapping.Wrap };
    private readonly TextBox search = new();
    private readonly ListBox library = new() { Height = 160, DisplayMemberPath = "RelativePath" };
    private readonly StackPanel slots = new();
    private readonly TextBlock message = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
    private readonly IrPackEntry?[] entries = new IrPackEntry?[8];
    private readonly List<Control> editControls = [];
    private string id = Guid.NewGuid().ToString("N");
    private bool readOnly, dirty, loading;
    private int destination;
    private IrPack? current;
    private static string T(string value) => LanguageService.T(value);

    public IrPackManagerWindow(EditorViewModel model)
    {
        this.model = model;
        Title = T("Gerenciar packs de IRs"); Width = 980; Height = 850; MinWidth = 600; MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SetResourceReference(BackgroundProperty, "ApplicationBackgroundBrush");
        SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
        var body = new StackPanel { MinWidth = 760, Margin = new Thickness(20), LayoutTransform = new ScaleTransform(UiDensity.Factor, UiDensity.Factor) };
        Content = new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        var toolbar = new WrapPanel(); toolbar.Children.Add(packs);
        AddButton(toolbar, "Novo pack", New);
        AddButton(toolbar, "Duplicar pack", Duplicate);
        AddButton(toolbar, "Excluir pack", Delete);
        AddButton(toolbar, "Importar pack…", Import);
        AddButton(toolbar, "Exportar pack…", Export);
        body.Children.Add(toolbar);
        Label(body, "Nome do pack"); body.Children.Add(name);
        Label(body, "Descrição do pack"); body.Children.Add(description);
        Label(body, "Buscar IR na biblioteca"); body.Children.Add(search); body.Children.Add(library);
        Label(body, "Selecione um destino e use o IR da biblioteca. Slots vazios serão preservados.");
        body.Children.Add(slots);
        Label(body, "Distância no pedal (%), independente da posição de captura do WAV.");
        body.Children.Add(message);
        var bottom = new WrapPanel();
        var save = AddButton(bottom, "Salvar pack", Save); editControls.Add(save);
        AddButton(bottom, "Fechar", Close); body.Children.Add(bottom);
        name.TextChanged += (_, _) => Changed(); description.TextChanged += (_, _) => Changed();
        search.TextChanged += (_, _) => Filter();
        packs.SelectionChanged += (_, _) =>
        {
            if (loading) return;
            var next = packs.SelectedItem as IrPack;
            if (!Discard()) { loading = true; packs.SelectedItem = current; loading = false; return; }
            Load(next);
        };
        Closing += (_, e) => { if (!Discard()) e.Cancel = true; };
        Filter(); Refresh(model.SelectedIrPack?.Id);
    }
    private static void Label(Panel parent, string text) => parent.Children.Add(new TextBlock { Text = T(text), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 5) });
    private Button AddButton(Panel panel, string label, Action action)
    {
        var button = new Button { Content = T(label), Margin = new Thickness(4), Padding = new Thickness(8, 5, 8, 5) };
        button.Click += (_, _) =>
        {
            try { action(); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException or InvalidOperationException)
            { message.Text = T(ex.Message); }
        };
        panel.Children.Add(button); return button;
    }
    private void Changed() { if (!loading) dirty = true; }
    private bool Discard() => !dirty || System.Windows.MessageBox.Show(this, T("Descartar alterações não salvas?"), Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    private void Filter()
    {
        var terms = search.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        library.ItemsSource = model.IrLibrary.Entries.Where(e => terms.All(t => e.RelativePath.Contains(t, StringComparison.OrdinalIgnoreCase))).ToArray();
    }
    private void Refresh(string? selected)
    {
        model.ReloadIrPacks(selected);
        loading = true; packs.ItemsSource = model.IrPacks;
        packs.SelectedItem = model.IrPacks.FirstOrDefault(p => p.Id == selected) ?? model.IrPacks.FirstOrDefault();
        loading = false; Load(packs.SelectedItem as IrPack);
    }
    private void Load(IrPack? pack)
    {
        loading = true; current = pack; readOnly = pack != null && pack.Group != "Meus packs";
        id = pack?.Definition?.Id ?? Guid.NewGuid().ToString("N");
        name.Text = pack?.Name ?? ""; description.Text = pack?.Description ?? "";
        Array.Clear(entries);
        if (pack?.Definition != null) foreach (var entry in pack.Definition.Entries) entries[entry.Slot] = entry;
        name.IsReadOnly = description.IsReadOnly = readOnly;
        foreach (var control in editControls) control.IsEnabled = !readOnly;
        message.Text = T(pack?.Problem.Length > 0 ? "Pendências: " + pack.Problem : readOnly ? "Pack somente leitura. Duplique para personalizar." : "");
        loading = false; dirty = false; RenderSlots();
    }
    private void RenderSlots()
    {
        slots.Children.Clear();
        for (int i = 0; i < 8; i++)
        {
            int slot = i;
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new() { Width = new GridLength(45) });
            row.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            var choose = new RadioButton { Content = (i + 1).ToString(), GroupName = "Destination", IsChecked = destination == i, VerticalAlignment = VerticalAlignment.Center };
            choose.Checked += (_, _) => destination = slot; row.Children.Add(choose);
            string label = entries[i]?.Name ?? (readOnly && current?.Definition == null ? current?.SlotName(i) : null) ?? T("Preservar slot");
            var text = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4) };
            Grid.SetColumn(text, 1); row.Children.Add(text);
            var actions = new WrapPanel { IsEnabled = !readOnly }; Grid.SetColumn(actions, 2); row.Children.Add(actions);
            var distance = new TextBox { Text = (entries[i]?.Distance ?? 50).ToString(), Width = 40, Margin = new Thickness(4), IsEnabled = entries[i] != null, ToolTip = T("Distância no pedal (%)") };
            distance.LostFocus += (_, _) =>
            {
                if (entries[slot] == null) return;
                if (int.TryParse(distance.Text, out int value) && value is >= 0 and <= 99) { entries[slot] = entries[slot]! with { Distance = value }; Changed(); }
                else { distance.Text = entries[slot]!.Distance.ToString(); message.Text = T("Use uma distância de 0 a 99."); }
            };
            actions.Children.Add(distance);
            AddButton(actions, "Usar IR", () =>
            {
                if (library.SelectedItem is not IrLibraryEntry selected) { message.Text = T("Selecione um IR da biblioteca."); return; }
                destination = slot;
                entries[slot] = model.PackStore.Reference(slot, selected.RelativePath, IrViewModel.DisplayName(selected.FullPath, selected.Collection));
                Changed(); RenderSlots();
            });
            AddButton(actions, "Remover", () => { entries[slot] = null; Changed(); RenderSlots(); });
            var up = AddButton(actions, "↑", () => Move(slot, slot - 1)); up.IsEnabled = slot > 0;
            var down = AddButton(actions, "↓", () => Move(slot, slot + 1)); down.IsEnabled = slot < 7;
            slots.Children.Add(row);
        }
        if (entries.Where(e => e != null).GroupBy(e => e!.Sha256, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1)) message.Text = T("Este pack contém IRs repetidos.");
    }
    private void Move(int from, int to)
    {
        (entries[from], entries[to]) = (entries[to], entries[from]);
        if (entries[from] != null) entries[from] = entries[from]! with { Slot = from };
        if (entries[to] != null) entries[to] = entries[to]! with { Slot = to };
        destination = to; Changed(); RenderSlots();
    }
    private IrPackDefinition Draft() => new() { Id = id, Name = name.Text.Trim(), Description = description.Text.Trim(), Entries = entries.OfType<IrPackEntry>().ToArray() };
    private void New() { if (!Discard()) return; loading = true; packs.SelectedItem = null; loading = false; Load(null); }
    private void Duplicate()
    {
        if (current?.Definition == null) { message.Text = T("Somente packs da biblioteca podem ser duplicados."); return; }
        if (!Discard()) return;
        var source = current;
        loading = true; packs.SelectedItem = null; loading = false;
        Load(source with { Id = Guid.NewGuid().ToString("N"), Group = "Meus packs", Definition = source.Definition with { Id = Guid.NewGuid().ToString("N"), Group = "Meus packs" } });
        name.Text += T(" (cópia)"); dirty = true;
    }
    private void Save()
    {
        if (readOnly) return;
        var draft = Draft(); model.PackStore.Save(draft); dirty = false; Refresh(draft.Id); message.Text = T("Pack salvo.");
    }
    private void Delete()
    {
        if (readOnly || current?.Definition == null) { message.Text = T("Selecione um pack pessoal salvo."); return; }
        if (System.Windows.MessageBox.Show(this, T("Excluir este pack pessoal?"), Title, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        model.PackStore.Delete(current.Definition); dirty = false; Refresh(null);
    }
    private void Import()
    {
        if (!Discard()) return;
        var dialog = new OpenFileDialog { Filter = "CubeControl IR Pack|*.cubeirpack.json" };
        if (dialog.ShowDialog(this) != true) return;
        var imported = model.PackStore.Import(dialog.FileName); dirty = false; Refresh(imported.Id);
    }
    private void Export()
    {
        var pack = readOnly ? current?.Definition : Draft();
        if (pack == null) { message.Text = T("Somente packs da biblioteca podem ser exportados."); return; }
        IrPackStore.Validate(pack);
        var dialog = new SaveFileDialog { Filter = "CubeControl IR Pack|*.cubeirpack.json", FileName = "Pack.cubeirpack.json" };
        if (dialog.ShowDialog(this) == true) { IrPackStore.Export(dialog.FileName, pack); message.Text = T("Pack exportado. O arquivo contém referências à biblioteca, sem WAVs."); }
    }
}
