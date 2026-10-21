using Microsoft.UI.Xaml.Controls;
namespace Cube.Desktop.Controls;
public enum PackDialogChoice { Primary, Secondary, Cancel }
public sealed class PackDialog
{
    private readonly ContentDialog dialog;
    public bool IsPrimaryEnabled { get => dialog.IsPrimaryButtonEnabled; set => dialog.IsPrimaryButtonEnabled = value; }
    public PackDialog(string title, object content, string primary = "Continuar", string secondary = "", double width = 680, string close = "Cancelar")
    { dialog = new() { Title = title, Content = content, PrimaryButtonText = primary, CloseButtonText = close, SecondaryButtonText = secondary, DefaultButton = ContentDialogButton.Close }; }
    public async Task<PackDialogChoice> ShowAsync() => await Cube.WinUI.Platform.Dialogs.Show(dialog) switch { ContentDialogResult.Primary => PackDialogChoice.Primary, ContentDialogResult.Secondary => PackDialogChoice.Secondary, _ => PackDialogChoice.Cancel };
}
