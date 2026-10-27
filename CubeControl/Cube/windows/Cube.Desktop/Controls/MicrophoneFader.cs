using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

public sealed class MicrophoneFader : Slider
{
    private static readonly BitmapImage Picture = LoadPicture();
    public BitmapImage MicrophoneImage => Picture;
    public MicrophoneFader() => RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

    private static BitmapImage LoadPicture()
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(AssetPaths.Image("pack-minimal-ribbon-microphone.png"));
        image.EndInit(); image.Freeze();
        return image;
    }
}
