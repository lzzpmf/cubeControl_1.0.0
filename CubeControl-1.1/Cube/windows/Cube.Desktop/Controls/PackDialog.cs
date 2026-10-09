using System.Windows;
using Cube.Desktop.Localization;

namespace Cube.Desktop.Controls;

internal enum PackDialogChoice { Primary, Secondary, Cancel }

// Use the app's scaled, adaptive dialog host. Standalone editor hosts retain a window fallback.
internal sealed class PackDialog
{
    private readonly Wpf.Ui.Controls.ContentDialog? dialog;
    private readonly Wpf.Ui.Controls.MessageBox? fallback;
    public PackDialog(string title, object content, string primary = "Continuar", string secondary = "", double width = 680)
    {
        if (Application.Current.MainWindow is MainWindow window && window.FindName("DialogHost") is Wpf.Ui.Controls.ContentDialogHost host)
            dialog = new(host) { Title = title, Content = content, PrimaryButtonText = primary,
                SecondaryButtonText = secondary, CloseButtonText = "Cancelar", DialogMaxWidth = width, DialogMaxHeight = 900 };
        else fallback = new() { Title = LanguageService.T(title), Content = content, PrimaryButtonText = LanguageService.T(primary),
            SecondaryButtonText = LanguageService.T(secondary), CloseButtonText = LanguageService.T("Cancelar"), Owner = Application.Current.MainWindow };
    }
    public bool IsPrimaryEnabled
    {
        set { if (dialog != null) dialog.IsPrimaryButtonEnabled = value; else fallback!.IsPrimaryButtonEnabled = value; }
    }
    public async Task<PackDialogChoice> ShowAsync()
    {
        if (dialog != null) return await dialog.ShowLocalizedAsync() switch {
            Wpf.Ui.Controls.ContentDialogResult.Primary => PackDialogChoice.Primary,
            Wpf.Ui.Controls.ContentDialogResult.Secondary => PackDialogChoice.Secondary,
            _ => PackDialogChoice.Cancel };
        return await fallback!.ShowDialogAsync() switch {
            Wpf.Ui.Controls.MessageBoxResult.Primary => PackDialogChoice.Primary,
            Wpf.Ui.Controls.MessageBoxResult.Secondary => PackDialogChoice.Secondary,
            _ => PackDialogChoice.Cancel };
    }
}
