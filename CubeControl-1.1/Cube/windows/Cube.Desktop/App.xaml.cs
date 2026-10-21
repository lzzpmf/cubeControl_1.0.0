using System.Windows;
namespace Cube.Desktop;
public partial class App : Application
{
    static App()
    {
        Appearance.PopupMotion.Initialize();
        // Hide chrome only; retain ScrollViewer offsets, wheel and keyboard scrolling.
        EventManager.RegisterClassHandler(typeof(System.Windows.Controls.Primitives.ScrollBar), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, _) =>
            {
                var bar = (System.Windows.Controls.Primitives.ScrollBar)sender;
                bar.Visibility = Visibility.Collapsed;
                bar.Width = 0;
                bar.Height = 0;
            }));
        System.Windows.Controls.ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(1200));
        System.Windows.Controls.ToolTipService.BetweenShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(0));
        // Keep WPF's hover lifecycle: timeout hides the hint; leaving and returning rearms it.
        System.Windows.Controls.ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(4000));
        System.Windows.Controls.ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(4000));
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.FirstOrDefault() == "--firmware-worker")
        {
            Shutdown(FirmwareUpdater.RunWorker(e.Args));
            return;
        }
        base.OnStartup(e);
        Appearance.ThemeService.Instance.Initialize();
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
