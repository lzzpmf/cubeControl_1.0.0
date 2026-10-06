using System.Windows;
using Cube.Desktop.Services;

namespace Cube.Desktop;

public partial class MainWindow
{
    private void BackingPlaylistLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Expander expander &&
            expander.Template.FindName("ContentPresenterBorder", expander) is System.Windows.Controls.Border content)
            content.SetResourceReference(System.Windows.Controls.Border.BackgroundProperty, "CubeCard");
    }
    private async void BackingYouTubeDownloadClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: YouTubeResult video })
            await BackingTracks.DownloadYouTube(video);
    }
}
