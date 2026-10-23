using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop;

public partial class MainWindow
{
    private void ConfigureBackingProjects()
    {
        BackingTracks.ConfirmProjectChanges = async () =>
        {
            if (BackingDialogOpen) return ProjectCloseChoice.Cancel;
            BackingDialogOpen = true;
            try
            {
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
            {
                Title = LanguageService.T("Salvar alterações no projeto?"),
                Content = new TextBlock { Text = BackingTracks.ProjectName, TextWrapping = TextWrapping.Wrap },
                PrimaryButtonText = LanguageService.T("Salvar"), SecondaryButtonText = LanguageService.T("Descartar"),
                CloseButtonText = LanguageService.T("Cancelar")
            };
            return await dialog.ShowLocalizedAsync() switch
            {
                Wpf.Ui.Controls.ContentDialogResult.Primary => ProjectCloseChoice.Save,
                Wpf.Ui.Controls.ContentDialogResult.Secondary => ProjectCloseChoice.Discard,
                _ => ProjectCloseChoice.Cancel
            };
            }
            finally { BackingDialogOpen = false; }
        };
        BackingTracks.ConfirmRecovery = async name =>
        {
            if (BackingDialogOpen) return false;
            BackingDialogOpen = true;
            try
            {
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost)
            {
                Title = LanguageService.T("Recuperar projeto interrompido?"), Content = name,
                PrimaryButtonText = LanguageService.T("Restaurar"), CloseButtonText = LanguageService.T("Descartar")
            };
            return await dialog.ShowLocalizedAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary;
            }
            finally { BackingDialogOpen = false; }
        };
        PreviewKeyDown += (_, e) =>
        {
            if (BackingDialogOpen || Tabs.SelectedIndex != 2 || System.Windows.Input.Keyboard.Modifiers != System.Windows.Input.ModifierKeys.Control) return;
            var command = e.Key switch
            {
                System.Windows.Input.Key.S => BackingTracks.SaveProject,
                System.Windows.Input.Key.N => BackingTracks.NewProject,
                System.Windows.Input.Key.O => BackingTracks.OpenProject,
                _ => null
            };
            if (command?.CanExecute(null) == true)
            {
                if (CommitBackingInput()) command.Execute(null);
                e.Handled = true;
            }
        };
    }
    private bool CommitBackingInput()
    {
        if (System.Windows.Input.Keyboard.FocusedElement is not TextBox input || !BackingView.IsAncestorOf(input)) return true;
        input.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        return !Validation.GetHasError(input);
    }
}
