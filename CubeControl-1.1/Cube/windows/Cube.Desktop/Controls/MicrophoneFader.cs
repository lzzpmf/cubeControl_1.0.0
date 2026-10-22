using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Cube.Desktop.Controls;

public sealed class MicrophoneFader : Slider
{
    private static readonly BitmapImage Picture = LoadPicture();
    public BitmapImage MicrophoneImage => Picture;

    private static BitmapImage LoadPicture()
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(AssetPaths.Image("pack-dynamic-microphone.png"));
        image.EndInit(); image.Freeze();
        return image;
    }
}
