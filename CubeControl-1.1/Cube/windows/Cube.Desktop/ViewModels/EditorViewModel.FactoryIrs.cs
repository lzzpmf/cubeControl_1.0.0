using Cube.Protocol;
using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.Localization;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public IReadOnlyList<IrPack> IrPacks { get; private set; } = [];
    private IrPack? selectedIrPack;
    public IrPack? SelectedIrPack
    {
        get => selectedIrPack;
        set { if (Set(ref selectedIrPack, value)) System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
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
            Content = new TextBlock { Text = $"Slot {i + 1} · {IrPackCatalog.CabinetName(pack.Id, i)}\n{LanguageService.T(different[i] ? "Diferente do pack" : "Idêntico ao pack")}", TextWrapping = TextWrapping.Wrap },
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
