using System.Windows.Controls;
using Cube.Protocol;
using System.Windows;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;

public partial class AmpEditor : UserControl
{
    private readonly EditorViewModel model;
    public AmpEditor(EditorViewModel model)
    {
        this.model = model; InitializeComponent(); DataContext = model;
        BaseSelector.ItemsSource = EquipmentCatalog.Amps.Select(a => a.Name).ToArray();
    }
    private int selectedBase = -1;
    private bool selectingBase;
    public void ChooseBase() { BaseSelector.Focus(); BaseSelector.IsDropDownOpen = true; }
    private async void BaseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || selectingBase || BaseSelector.SelectedIndex < 0) return;
        int next = BaseSelector.SelectedIndex;
        selectingBase = true;
        try
        {
            if (!model.CanEditAmpLocally) { BaseSelector.SelectedIndex = selectedBase; return; }
            var dialog = new Wpf.Ui.Controls.MessageBox
            {
                Owner = Window.GetWindow(this), Title = LanguageService.T("Carregar amp"),
                Content = LanguageService.T("Carregar o amp selecionado? Os ajustes atuais serão perdidos."),
                PrimaryButtonText = LanguageService.T("Carregar"), CloseButtonText = LanguageService.T("Cancelar")
            };
            if (await dialog.ShowDialogAsync() != Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                BaseSelector.SelectedIndex = selectedBase; return;
            }
            if (await model.ReadAmpAsync(next))
            {
                selectedBase = next; model.Amp.Slot = next;
                await ApplyControls();
            }
            else
            {
                BaseSelector.SelectedIndex = selectedBase;
                AmpFeedback.SetCurrentValue(TextBlock.TextProperty, LanguageService.T("Não foi possível carregar o amp. Os ajustes anteriores foram mantidos."));
            }
        }
        finally { selectingBase = false; }
    }
    private void ShowResult(bool? success, string done, string failed)
    {
        if (success.HasValue) AmpFeedback.SetCurrentValue(TextBlock.TextProperty, LanguageService.T(success.Value ? done : failed));
    }
    private async void ImportClick(object sender, RoutedEventArgs e)
    {
        var imported = await model.ImportAmpAsync();
        if (imported == true) await ApplyControls();
        else ShowResult(imported, "Preset importado.", "Não foi possível importar esse amp.");
    }
    private async void ExportClick(object sender, RoutedEventArgs e) => ShowResult(await model.ExportAmpAsync(), "Seu amp foi exportado.", "Não foi possível exportar seu amp.");
    private async Task ApplyControls()
    {
        bool applied = await model.PreviewAmpAsync();
        AmpFeedback.SetCurrentValue(TextBlock.TextProperty, LanguageService.T(applied ? "Seu timbre está pronto para ouvir." : "Não foi possível ouvir no pedal. Seus ajustes continuam no computador."));
    }
    private void ResetControlClick(object sender, RoutedEventArgs e)
    {
        if (!model.CanAdjustAmp || sender is not FrameworkElement { DataContext: AmpControl control }) return;
        control.Restore();
    }
    private async void RestoreClick(object sender, RoutedEventArgs e) => ShowResult(await model.RestoreAmpAsync(), "O som anterior foi restaurado.", "Não foi possível restaurar o som anterior.");
    private async void WriteClick(object sender, RoutedEventArgs e) => ShowResult(await model.WriteAmpAsync(async message =>
    {
        var dialog = new Wpf.Ui.Controls.MessageBox
        {
            Owner = Window.GetWindow(this), Title = LanguageService.T("Salvar no pedal"), Content = LanguageService.T(message),
            PrimaryButtonText = LanguageService.T("Confirmar"), CloseButtonText = LanguageService.T("Cancelar")
        };
        return await dialog.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary;
    }), "Amp salvo. Troque de amp no pedal e volte para ouvir o som salvo.", "Não foi possível salvar seu amp no pedal.");
}
