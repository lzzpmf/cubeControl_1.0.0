using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Views;
using Cube.Protocol;

internal static class PhaserDesktopTests
{
    public static async Task Run()
    {
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
        using (var factory = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!)
        {
            var bytes = new byte[AmpImage.Size];
            for (int slot = 0; slot < 9; slot++)
            {
                factory.ReadExactly(bytes);
                var amp = new AmpViewModel(); amp.LoadSlot(bytes, slot);
                Check(amp.Build().SequenceEqual(bytes), $"factory amp {slot + 1} loads and builds unchanged");
                amp.MainControls[0].Position = 1000;
                AmpImage.Validate(amp.Build());
                Check(true, $"factory amp {slot + 1} gain control maximum stays valid");
            }
        }
        string directory = Path.Combine(Path.GetTempPath(), "cube-phaser-test-" + Guid.NewGuid().ToString("N"));
        var fake = new PhaserConnection();
        var session = new CubeSession(fake);
        await session.Identify();
        using var model = new EditorViewModel(directory);
        await model.AttachSession(session);
        await model.ReadPhaserAsync();
        Check(model.PhaserPreviewActive && model.CanEditPhaser && !model.CanEdit && !model.CanUseAmp && !model.CanOperate && !model.CanStartDelay,
            "phaser preview excludes preset edits and amp/IR operations");
        Check(File.Exists(Path.Combine(directory, "pending-phaser-preview.json")), "phaser durable recovery created");
        model.Phaser.Lower = 400; model.Phaser.Lower = 500; model.Phaser.Lower = 600;
        for (int i = 0; i < 200 && (PhaserParameters.Read(fake.Dsp).Lower != 600 || model.Busy); i++) await Task.Delay(10);
        Check(PhaserParameters.Read(fake.Dsp).Lower == 600 && fake.Writes == 1, "phaser rapid edits coalesce to final value");
        var view = new PhaserEditor { DataContext = model, Width = 560, Background = Brushes.White, Foreground = Brushes.Black };
        view.SetResourceReference(Control.BackgroundProperty, "CubeCanvas");
        view.SetResourceReference(Control.ForegroundProperty, "CubeText");
        var host = new Window { Content = view, Width = 600, Height = 900, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, SizeToContent = SizeToContent.Height };
        host.Show();
        ((Expander)view.Content).IsExpanded = true;
        await Task.Delay(400);
        view.Measure(new Size(560, 900)); view.Arrange(new Rect(0, 0, 560, view.DesiredSize.Height)); view.UpdateLayout();
        Check(view.ActualHeight > 300 && view.ActualHeight < 1200, $"phaser editor lays out without clipping ({view.ActualHeight})");
        string render = Path.GetFullPath("artifacts/phaser/phaser-editor.png"); Directory.CreateDirectory(Path.GetDirectoryName(render)!);
        var bitmap = new RenderTargetBitmap(560, (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = File.Create(render)) png.Save(output);
        host.Close();
        await model.DisconnectAsync();
        Check(!model.PhaserPreviewActive && !model.Connected && PhaserParameters.Read(fake.Dsp) == PhaserParameters.Default,
            "normal disconnect restores phaser before closing transport");
        Check(!File.Exists(Path.Combine(directory, "pending-phaser-preview.json")), "verified restore clears pending recovery");
        var second = new CubeSession(fake); await second.Identify(); await model.AttachSession(second); await model.ReadPhaserAsync();
        model.Phaser.Level = 50;
        for (int i = 0; i < 200 && (PhaserParameters.Read(fake.Dsp).Level != 0.32f || model.Busy); i++) await Task.Delay(10);
        model.Dispose(); // Abnormal shutdown: retain pending record, never silently discard it.
        using var reopened = new EditorViewModel(directory);
        int writes = fake.Writes;
        await reopened.Initialize();
        Check(reopened.PhaserPreviewActive && !reopened.CanEditPhaser && fake.Writes == writes,
            "restart loads pending recovery without reapplying controls");
        var third = new CubeSession(fake); await third.Identify(); await reopened.AttachSession(third);
        await reopened.EndPhaserPreviewAsync();
        Check(PhaserParameters.Read(fake.Dsp) == PhaserParameters.Default && !reopened.PhaserPreviewActive,
            "explicit recovery survives process restart");
        await reopened.ReadPhaserAsync();
        fake.State[0] = 1;
        await Task.Delay(2300);
        Check(!reopened.CanEditPhaser && reopened.PhaserPreviewActive && File.Exists(Path.Combine(directory, "pending-phaser-preview.json")),
            "idle monitoring stops on physical context change and keeps recovery");
        fake.State[0] = 0;
        await reopened.EndPhaserPreviewAsync();
        await reopened.DisconnectAsync();
        fake.BadSignature = true;
        var unsupported = new CubeSession(fake); await unsupported.Identify(); await reopened.AttachSession(unsupported);
        writes = fake.Writes;
        await reopened.ReadPhaserAsync();
        Check(reopened.CanEdit && reopened.CanOperate && !reopened.PhaserPreviewActive && fake.Writes == writes,
            "unsupported effect firmware leaves normal editor usable without writes");
        Check(!reopened.Status.Contains("Releia ou reconecte"), "unsupported firmware does not suggest reconnecting");
        Console.WriteLine($"{passed} phaser desktop checks passed. Render: {render}");
    }
}
