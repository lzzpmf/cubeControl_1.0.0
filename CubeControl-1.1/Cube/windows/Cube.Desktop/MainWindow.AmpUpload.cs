using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop;

public partial class MainWindow
{
    private bool ampUploadOpen;
    private async void UploadAmpClick(object sender, RoutedEventArgs e)
    {
        if (ampUploadOpen || !Model.CanUseAmp) return;
        ampUploadOpen = true;
        int destination = Model.SelectedAmp.Value;
        try
        {
            var choices = new ComboBox
            {
                ItemsSource = AmpEqPresets.All.Select(p => p.Description)
                    .Append(LanguageService.T("Importar arquivo .bin")).ToArray(),
                SelectedIndex = 0, MinWidth = 260, MaxWidth = 440,
                Margin = new Thickness(0, 12, 0, 12)
            };
            System.Windows.Automation.AutomationProperties.SetName(choices, LanguageService.T("Escolha um amp base"));
            var content = new StackPanel();
            content.Children.Add(new TextBlock { Text = $"Slot {destination + 1} · {Model.SelectedAmp.Name}", TextWrapping = TextWrapping.Wrap });
            content.Children.Add(choices);
            content.Children.Add(new TextBlock
            {
                Text = LanguageService.T("Variações de timbre criadas apenas com EQ de duas bandas. A gravação substitui o amp deste slot."),
                TextWrapping = TextWrapping.Wrap, MaxWidth = 440
            });
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
            {
                Title = LanguageService.T("Subir amp no slot selecionado"), Content = content,
                PrimaryButtonText = "Continuar", CloseButtonText = "Cancelar"
            };
            if (await dialog.ShowLocalizedAsync() != Wpf.Ui.Controls.ContentDialogResult.Primary || !Model.CanUseAmp) return;
            if (choices.SelectedIndex == AmpEqPresets.All.Count)
            {
                if (await Model.ImportAmpAsync() != true) return;
            }
            else
            {
                var preset = AmpEqPresets.All[choices.SelectedIndex];
                Model.Amp.Load(preset.Build(), preset.Name);
            }
            Model.Amp.Slot = destination;
            await Model.WriteAmpAsync();
        }
        finally { ampUploadOpen = false; }
    }
}
