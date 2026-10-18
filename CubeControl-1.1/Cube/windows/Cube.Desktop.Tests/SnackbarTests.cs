using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Wpf.Ui.Controls;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class SnackbarTests
{
    public static async Task Run()
    {
        void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); }
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-snackbar-" + Guid.NewGuid().ToString("N")));
        var window = new MainWindow(model, initializeDevices: false) { Width = 640, Height = 720, ShowInTaskbar = false };
        var messages = new List<UserNotification>();
        model.NotificationRequested += messages.Add;
        window.Show();
        await Task.Delay(350);
        while (model.Busy) await Task.Delay(20);
        var presenter = (SnackbarPresenter)window.FindName("Notifications");
        using var fake = new FakeConnection();
        var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(100) };
        await session.Identify();
        await model.Execute(() => model.AttachSession(session));
        await Task.Delay(350);
        Check(messages.Count == 1 && messages[0].Appearance == ControlAppearance.Success && messages[0].Title == "Pedal reconhecido",
            "confirmed connection emits one Success");
        Check(presenter.Content?.Appearance == ControlAppearance.Success && presenter.Content.IsCloseButtonEnabled,
            "native Success snackbar is visible and dismissible");
        await model.TryAutomaticConnection();
        Check(messages.Count == 1 && fake.Writes == 0, "heartbeat is silent and read-only");
        var focus = Keyboard.FocusedElement;
        fake.Silent = true;
        await model.TryAutomaticConnection();
        await Task.Delay(400);
        Check(messages.Count == 2 && messages[^1].Appearance == ControlAppearance.Danger,
            "power-off emits one Danger");
        Check(presenter.Content?.Appearance == ControlAppearance.Danger && presenter.Content.Timeout == TimeSpan.FromSeconds(8),
            "Danger immediately replaces Success with longer reading time");
        Check(Keyboard.FocusedElement == focus, "notification does not steal focus");
        Check(!model.Connected && model.HasError, "persistent error remains after transient notification");
        await model.TryAutomaticConnection(() => Task.CompletedTask, () => Task.CompletedTask);
        Check(messages.Count == 2, "disconnected polling does not repeat Danger");
        async Task Capture(string name)
        {
            await Task.Delay(400); window.UpdateLayout();
            var bar = presenter.Content!;
            var bounds = bar.TransformToAncestor(window).TransformBounds(new Rect(bar.RenderSize));
            Check(bounds.Left >= 0 && bounds.Right <= window.ActualWidth + 1 && bounds.Bottom <= window.ActualHeight + 1,
                name + " fits window");
            string directory = Path.GetFullPath(TestPaths.Report("C:/CubeControl-Build/reports/snackbar-3.3.65")); Directory.CreateDirectory(directory);
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(directory, name + ".png")); encoder.Save(file);
        }
        ThemeService.Instance.Preview(ThemeMode.Dark);
        await Capture("danger-dark-640");
        LanguageService.Instance.Preview("en-US");
        ThemeService.Instance.Preview(ThemeMode.Light);
        window.ShowNotification(messages[0]);
        await Task.Delay(400);
        Check((string)presenter.Content!.Title! == "Pedal recognized", "snackbar title is localized");
        await Capture("success-light-en-640");
        window.ShowNotification(new("Gravação pendente", "Banco importado e aplicado. Use Gravar A/B/C para salvar no aparelho.", ControlAppearance.Caution));
        await Capture("caution-light-en-640");
        Check(presenter.Content!.Appearance == ControlAppearance.Caution, "pending save uses Caution");
        await model.Execute(() => throw new IOException("test failure"));
        Check(messages[^1].Appearance == ControlAppearance.Danger, "failed user operation emits Danger");
        int count = messages.Count;
        await model.TryAutomaticConnection(() =>
        {
            model.Connection.Input = new("input", "USB-Midi"); model.Connection.Output = new("output", "USB-Midi");
            return Task.CompletedTask;
        }, () => throw new IOException("booting"));
        Check(messages.Count == count, "automatic retries do not flood snackbars");
        LanguageService.Instance.Preview("pt-BR");
        window.Close();
    }
}
