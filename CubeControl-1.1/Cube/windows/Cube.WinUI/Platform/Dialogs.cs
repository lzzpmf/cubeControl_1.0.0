using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Cube.Desktop.ViewModels;
namespace Cube.WinUI.Platform;
internal static class Dialogs
{
    internal static Window? Window { get; set; }
    internal static bool IsOpen { get; private set; }
    private static readonly SemaphoreSlim gate = new(1);
    public static async Task<ContentDialogResult> Show(ContentDialog dialog)
    {
        static string T(string text) => Cube.Desktop.Localization.LanguageService.T(text);
        if (dialog.Title is string title) dialog.Title = T(title);
        if (dialog.Content is string message) dialog.Content = T(message);
        else if (dialog.Content is TextBlock text) text.Text = T(text.Text);
        dialog.PrimaryButtonText = T(dialog.PrimaryButtonText); dialog.SecondaryButtonText = T(dialog.SecondaryButtonText); dialog.CloseButtonText = T(dialog.CloseButtonText);
        await gate.WaitAsync();
        try
        {
            IsOpen = true;
            dialog.XamlRoot = ((FrameworkElement)Window!.Content).XamlRoot;
            if (dialog.Content is Panel panel) dialog.Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            if (dialog.Content is ScrollViewer scroll) scroll.MaxHeight = Math.Max(120, dialog.XamlRoot.Size.Height - 240);
            var previous = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(dialog.XamlRoot) as Control;
            var result = await dialog.ShowAsync();
            if (previous is { IsLoaded: true, IsEnabled: true, Visibility: Visibility.Visible }) previous.Focus(FocusState.Programmatic);
            return result;
        }
        finally { IsOpen = false; gate.Release(); }
    }
    public static async Task<bool> Confirm(string message) => await Show(new ContentDialog { Title = "CubeControl", Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap }, PrimaryButtonText = "Confirmar", CloseButtonText = "Cancelar", DefaultButton = ContentDialogButton.Close }) == ContentDialogResult.Primary;
    public static async Task<bool> ReviewPack(IrPackReviewViewModel review)
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = review.Definition.Name, TextWrapping = TextWrapping.Wrap });
        var dialog = new ContentDialog { Title = "Revisar e aplicar", Content = new ScrollViewer { Content = panel }, PrimaryButtonText = "Aplicar", CloseButtonText = "Cancelar", IsPrimaryButtonEnabled = review.CanContinue };
        foreach (var slot in review.Slots)
        {
            var box = new CheckBox { Content = slot.Title + " · " + slot.Name + " · " + slot.Comparison, IsChecked = slot.Selected, IsEnabled = slot.Available };
            box.Checked += (_, _) => { slot.Selected = true; dialog.IsPrimaryButtonEnabled = review.CanContinue; };
            box.Unchecked += (_, _) => { slot.Selected = false; dialog.IsPrimaryButtonEnabled = review.CanContinue; };
            panel.Children.Add(box);
        }
        return await Show(dialog) == ContentDialogResult.Primary;
    }
}
