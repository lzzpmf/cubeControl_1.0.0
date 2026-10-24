using System.Windows.Controls;
using System.Windows.Input;
namespace Cube.Desktop;
public partial class MainWindow
{
    private void IrAuditionLoopClick(object sender, System.Windows.RoutedEventArgs e) => Model.IrAudition.Loop = !Model.IrAudition.Loop;
    private void BackingKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && BackingTracks.YouTubePreviewId != null)
        { Studio.StopPreview(); e.Handled = true; }
    }
}
