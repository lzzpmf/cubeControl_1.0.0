#if WINUI
using Cube.WinUI.Platform;
#else
using Wpf.Ui.Controls;
#endif

namespace Cube.Desktop.ViewModels;

public sealed record UserNotification(string Title, string Message, ControlAppearance Appearance)
{
    public TimeSpan Duration => TimeSpan.FromSeconds(Appearance == ControlAppearance.Danger ? 8 : 5);
}
