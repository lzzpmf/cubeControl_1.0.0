using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class MicrophoneTests
{
    public static async Task Run()
    {
        using var fake = new FakeConnection();
        using var session = new CubeSession(fake);
        await session.Identify();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-mic-" + Guid.NewGuid().ToString("N")));
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model, initializeDevices: false) { ShowInTaskbar = false };
        window.Show();
        await Task.Delay(250);
        while (model.Busy) await Task.Delay(30);
        ((TabControl)window.FindName("Tabs")).SelectedIndex = 1;
        var scene = (MicrophoneDistance)window.FindName("IrMicrophone");
        var canvas = (Canvas)scene.Child;
        var thumb = (Thumb)canvas.Children[1];
        window.UpdateLayout();
        thumb.BringIntoView();
        window.Activate();
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        thumb.ApplyTemplate();
        thumb.Focus();
        var focusFrame = (Border)thumb.Template.FindName("FocusFrame", thumb);
        if (!thumb.IsKeyboardFocused || focusFrame.Background is not SolidColorBrush { Color.A: 0 })
            throw new Exception("Focused microphone must retain a transparent background.");
        var folder = Path.GetFullPath(TestPaths.Report("C:/CubeControl-Build/reports/microphone-check"));
        Directory.CreateDirectory(folder);
        foreach (var width in new[] { 1280, 680 })
        foreach (var distance in new[] { 0, 50, 99 })
        {
            window.Width = width; window.Height = 900;
            model.Ir.Distance = distance;
            window.UpdateLayout(); await Task.Delay(80);
            ((ScrollViewer)window.FindName("IrScroll")).ScrollToVerticalOffset(230);
            scene.BringIntoView(); window.UpdateLayout();
            if (scene.Distance != distance || Math.Abs(Canvas.GetLeft(thumb) - (210 + 160d * distance / 99)) > .01)
                throw new Exception("Distance binding or microphone placement failed.");
            if (Math.Abs(Canvas.GetTop(thumb) - 92) > .01 || Canvas.GetTop(thumb) + thumb.Height > canvas.Height || Canvas.GetLeft(thumb) < 0 || Canvas.GetLeft(thumb) + thumb.Width > canvas.Width)
                throw new Exception("Microphone clipped at distance endpoint.");
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var output = File.Create(Path.Combine(folder, $"ir-{width}-{distance}.png")); encoder.Save(output);
        }
        scene.Distance = 27;
        if (model.Ir.Distance != 27) throw new Exception("Microphone change does not reach IR preparation.");
        scene.Distance = 150;
        if (model.Ir.Distance != 99) throw new Exception("Distance upper limit failed.");
        scene.Distance = -20;
        if (model.Ir.Distance != 0) throw new Exception("Distance lower limit failed.");
        if (fake.Writes != 0) throw new Exception("Distance preparation unexpectedly wrote to the device.");
        scene.Distance = 50;
        thumb.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, 120)
            { RoutedEvent = Mouse.PreviewMouseWheelEvent });
        if (scene.Distance != 50) throw new Exception("Wheel must not change microphone distance.");
        var audio = new IrAudio(48000, 1, 32, "float", [.5f, .25f, -.1f]);
        await model.Execute(() => model.LoadWavAndPreviewAsync("test.wav", audio));
        if (!model.Ir.PreviewConfirmed || fake.Writes == 0 || fake.FlashWrites != 0)
            throw new Exception("WAV selection must automatically preview only in RAM.");
        int beforeDrag = fake.Writes;
        scene.Distance = 63;
        if (focusFrame.Background is not SolidColorBrush { Color.A: 0 })
            throw new Exception("Moving microphone must retain a transparent background.");
        if (fake.Writes != beforeDrag) throw new Exception("Dragging must not transmit before release.");
        thumb.RaiseEvent(new DragCompletedEventArgs(0, 0, true) { RoutedEvent = Thumb.DragCompletedEvent });
        if (fake.Writes != beforeDrag) throw new Exception("Canceled drag must not apply.");
        thumb.RaiseEvent(new DragCompletedEventArgs(0, 0, false) { RoutedEvent = Thumb.DragCompletedEvent });
        while (model.Busy) await Task.Delay(30);
        if (!model.Ir.PreviewConfirmed || fake.Writes <= beforeDrag || fake.FlashWrites != 0)
            throw new Exception("Mouse release must apply new preview only in RAM.");
        Console.WriteLine("PASS automatic WAV preview, apply on release, canceled drag, no flash writes.");
        int beforeAccessibleEdit = fake.Writes;
        var range = (IRangeValueProvider)UIElementAutomationPeer.CreatePeerForElement(thumb)!.GetPattern(PatternInterface.RangeValue)!;
        range.SetValue(57);
        while (model.Busy) await Task.Delay(30);
        if (model.Ir.Distance != 57 || !model.Ir.PreviewConfirmed || fake.Writes <= beforeAccessibleEdit || fake.FlashWrites != 0)
            throw new Exception("Completed accessibility edit must apply preview only in RAM.");
        Console.WriteLine("PASS microphone accessibility edit applies the same RAM-only preview as release.");
        ((TabControl)window.FindName("Tabs")).SelectedIndex = 0;
        window.UpdateLayout();
        var endPreview = (Wpf.Ui.Controls.Button)window.FindName("EndPreviewNoticeButton");
        foreach (var width in new[] { 640, 1920 })
        {
            window.Width = width;
            window.UpdateLayout();
            if (!endPreview.IsVisible || !endPreview.IsEnabled || endPreview.ActualWidth <= 0 || PresentationSource.FromVisual(endPreview) == null)
                throw new Exception("Preview notice action is not rendered and available.");
        }
        endPreview.Command.Execute(null);
        while (model.Busy) await Task.Delay(30);
        window.UpdateLayout();
        if (model.Ir.PreviewActive || !model.CanEdit || endPreview.IsVisible || fake.FlashWrites != 0)
            throw new Exception("Notice action must end preview, restore preset editing and hide without flash writes.");
        Console.WriteLine("PASS visible notice action at 640/1920 ends preview and restores editing.");
        await model.Execute(model.DisconnectAsync);
        if (scene.IsEnabled) throw new Exception("Disconnected distance control remains enabled.");
        if (focusFrame.Background is not SolidColorBrush { Color.A: 0 })
            throw new Exception("Disabled microphone must retain a transparent background.");
        window.Close();
        Console.WriteLine("PASS microphone: two-way binding, endpoints, compact/wide rendering, disabled state and no device writes.");
    }
}
