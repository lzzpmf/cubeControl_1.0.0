using Cube.Desktop.Localization;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

internal sealed class DialogService(ContentDialogHost host)
{
    public async Task<ContentDialogResult> ShowAsync(string title, object content, string closeButtonText = "Fechar")
    {
        var previousFocus = System.Windows.Input.Keyboard.FocusedElement;
        try
        {
            return await new ContentDialog(host)
            {
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText
            }.ShowLocalizedAsync();
        }
        finally
        {
            if (previousFocus is System.Windows.UIElement { IsVisible: true, IsEnabled: true } element) element.Focus();
        }
    }
}
