using Cube.Desktop.Localization;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

internal sealed class DialogService(ContentDialogHost host)
{
    public Task<ContentDialogResult> ShowAsync(string title, object content, string closeButtonText = "Fechar")
        => new ContentDialog(host)
        {
            Title = title,
            Content = content,
            CloseButtonText = closeButtonText
        }.ShowLocalizedAsync();
}
