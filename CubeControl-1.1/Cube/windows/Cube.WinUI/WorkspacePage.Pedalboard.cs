using Cube.Desktop.ViewModels;
using Cube.WinUI.Platform;
using Cube.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Cube.WinUI;
public sealed partial class WorkspacePage
{
    private void BuildPedal()
    {
        Section("Cadeia de efeitos", new Views.SignalChainView(E, parameter =>
        {
            UIElement field = parameter.IsChoice ? Choice(parameter, "Options", "Choice", parameter.Name, index: true)
                : parameter.IsToggle ? Bypass(parameter, "Enabled", parameter.Name)
                : Instrument(parameter, "Value", parameter.Name, 0, parameter.Maximum);
            ToolTipService.SetToolTip(field, T(parameter.Description)); AutomationProperties.SetHelpText(field, T(parameter.Description)); return field;
        }));
        Section("Banco de presets", Buttons(Command(E, "Save", "Salvar no pedal"), Command(E, "Reload", "Reler banco"), Command(E, "Import", "Importar banco"), Command(E, "Export", "Exportar banco")));
        var gear = new ComboBox { Header = T("Equipamento"), DisplayMemberPath = "Name", HorizontalAlignment = HorizontalAlignment.Stretch };
        void RefreshGear() { gear.ItemsSource = E.Amps.Concat(E.Cabinets).Concat(E.Effects).ToArray(); gear.SelectedItem = E.FocusedGear; }
        RefreshGear(); Observe(E, "Cabinets", RefreshGear);
        Observe(E, "FocusedGear", () => { if (!ReferenceEquals(gear.SelectedItem, E.FocusedGear)) gear.SelectedItem = E.FocusedGear; });
        var picture = new Image { Height = 180, Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform };
        AutomationProperties.SetAccessibilityView(picture, Microsoft.UI.Xaml.Automation.Peers.AccessibilityView.Raw);
        gear.SelectionChanged += (_, _) => { if (gear.SelectedItem is GearItem item) picture.Source = item.Image ?? item.DefaultImage; };
        if (gear.SelectedItem is GearItem initial) picture.Source = initial.Image ?? initial.DefaultImage;
        Section("Equipamentos", gear, picture, Buttons(Action("Usar no preset", () => { if (E.CanEdit && gear.SelectedItem is GearItem item) E.ChooseGear(item); }), Action("Personalizar imagem", () =>
        {
            if (gear.SelectedItem is not GearItem item) return;
            var dialog = new OpenFileDialog { Filter = "Imagens (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (dialog.ShowDialog() == true && E.ApplyGearImage(item, dialog.FileName)) picture.Source = item.Image;
        })));
        Section("Gabinetes e histórico", Check(E, "AutomaticCabinetMatching", "Associar gabinete ao amp automaticamente"), Buttons(Command(E, "RefreshInventory", "Ler inventário de IRs"), Command(E, "MatchCabinet", "Associar gabinete"), Command(E, "Undo", "Desfazer ajuste")), Text(E, "InventoryStatus"), Text(E, "MatchDescription"));
        Section("Associações amp / gabinete", Action("Editar associações", async () =>
        {
            var rows = E.CreateMatchRows(); var panel = new StackPanel { Spacing = 8 };
            foreach (var row in rows) panel.Children.Add(Choice(row, "Choices", "Selected", row.Amp.Name, "Name"));
            if (await Dialogs.Show(new ContentDialog { Title = "Associações", Content = new ScrollViewer { Content = panel, MaxHeight = 420 }, PrimaryButtonText = "Salvar", CloseButtonText = "Cancelar" }) == ContentDialogResult.Primary) E.SaveMatches(rows);
        }));
        var history = new ListView { DisplayMemberPath = "Description", MaxHeight = 180 }; Bind(history, ItemsControl.ItemsSourceProperty, E, "Operations");
        Section("Operações recentes", history, Action("Restaurar operação selecionada", async () => { if (history.SelectedItem is EditorViewModel.OperationHistoryItem item) await E.RestoreOperationAsync(item); }));
        BuildEffects();
    }
}
