using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Controls;

// WPF UI 4.3.0 has no public light-dismiss API. Keep its template dependency here.
internal sealed class GalleryDialogDismissal : IDisposable
{
    private readonly Window owner;
    private readonly ContentDialog dialog;

    public GalleryDialogDismissal(Window owner, ContentDialog dialog)
    {
        this.owner = owner;
        this.dialog = dialog;
        owner.PreviewMouseDown += OnMouseDown;
    }

    internal static Border? Surface(ContentDialog dialog) =>
        (dialog.Template?.FindName("DialogRootGrid", dialog) as Grid)?.Children.OfType<Border>().FirstOrDefault();

    internal static bool IsOutside(ContentDialog dialog, Point pointInDialog)
    {
        var surface = Surface(dialog);
        // If a future template changes, retain explicit Cancel instead of dismissing incorrectly.
        return surface != null && !new Rect(surface.RenderSize).Contains(dialog.TranslatePoint(pointInDialog, surface));
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs args)
    {
        if (args.ChangedButton != MouseButton.Left || !IsOutside(dialog, args.GetPosition(dialog))) return;
        args.Handled = true;
        dialog.Hide(ContentDialogResult.None);
    }

    public void Dispose() => owner.PreviewMouseDown -= OnMouseDown;
}
