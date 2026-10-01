namespace Cube.Desktop.Localization;

public static class LocalizedDialogs
{
    public static async Task<Wpf.Ui.Controls.ContentDialogResult> ShowLocalizedAsync(this Wpf.Ui.Controls.ContentDialog dialog)
    {
        dialog.PrimaryButtonIcon ??= new Wpf.Ui.Controls.SymbolIcon
        {
            Symbol = dialog.PrimaryButtonText == "Salvar associações" ? Wpf.Ui.Controls.SymbolRegular.Save24 : Wpf.Ui.Controls.SymbolRegular.Checkmark24,
            FontSize = 16
        };
        dialog.SecondaryButtonIcon ??= new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Lightbulb24, FontSize = 16 };
        dialog.CloseButtonIcon ??= new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Dismiss24, FontSize = 16 };
        if (dialog.Title is string title) dialog.Title = LanguageService.T(title);
        if (dialog.Content is string content) dialog.Content = LanguageService.T(content);
        dialog.PrimaryButtonText = LanguageService.T(dialog.PrimaryButtonText ?? "");
        dialog.SecondaryButtonText = LanguageService.T(dialog.SecondaryButtonText ?? "");
        dialog.CloseButtonText = LanguageService.T(dialog.CloseButtonText ?? "");
        var previousFocus = System.Windows.Input.Keyboard.FocusedElement;
        using var adaptive = Appearance.AdaptiveDialogs.Attach(dialog);
        dialog.PreviewMouseWheel += Controls.SmoothScroll.RouteWheel;
        try { return await dialog.ShowAsync(); }
        finally
        {
            dialog.PreviewMouseWheel -= Controls.SmoothScroll.RouteWheel;
            if (previousFocus is System.Windows.UIElement { IsVisible: true, IsEnabled: true } element)
                element.Focus();
        }
    }
}
