using Wpf.Ui.Controls;

namespace Cube.Desktop.ViewModels;

public sealed record UserNotification(string Title, string Message, ControlAppearance Appearance)
{
    public TimeSpan Duration => TimeSpan.FromSeconds(Appearance == ControlAppearance.Danger ? 8 : 5);
}
