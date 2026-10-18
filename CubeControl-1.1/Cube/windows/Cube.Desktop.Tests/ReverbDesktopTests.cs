using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Views;
using Cube.Protocol;

internal static class ReverbDesktopTests
{
    public static async Task Run()
    {
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
        string directory = Path.Combine(Path.GetTempPath(), "cube-reverb-test-" + Guid.NewGuid().ToString("N"));
        var fake = new ReverbConnection(); fake.Activate();
        var session = new CubeSession(fake);
        await session.Identify();
        using var model = new EditorViewModel(directory);
        await model.AttachSession(session);
        await model.ReadReverbAsync();
        Check(model.ReverbPreviewActive && model.CanEditReverb && model.CanEdit && model.CanUseAmp && model.CanOperate && model.CanStartPhaser && model.CanStartChorus && model.CanStartDelay,
            "reverb preview preserves live preset controls and allows independent effect sessions");
        Check(!File.Exists(Path.Combine(directory, "pending-reverb-preview.json")), "reverb snapshot stays in memory");
        model.Reverb.Wet = 10; model.Reverb.Wet = 20; model.Reverb.Wet = 30;
        for (int i = 0; i < 200 && (ReverbParameters.Read(fake.Dsp).Wet != 0.3f || model.Busy); i++) await Task.Delay(10);
        Check(ReverbParameters.Read(fake.Dsp).Wet == 0.3f && fake.Writes == 1, "reverb rapid edits coalesce to final value");
        var view = new ReverbEditor { DataContext = model, Width = 560, Background = Brushes.White, Foreground = Brushes.Black };
        view.SetResourceReference(Control.BackgroundProperty, "CubeCanvas");
        view.SetResourceReference(Control.ForegroundProperty, "CubeText");
        var host = new Window { Content = view, Width = 600, Height = 900, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, SizeToContent = SizeToContent.Height };
        host.Show();
        await Task.Delay(400);
        view.Measure(new Size(560, 900)); view.Arrange(new Rect(0, 0, 560, view.DesiredSize.Height)); view.UpdateLayout();
        Check(view.ActualHeight > 300 && view.ActualHeight < 1200, $"reverb editor lays out without clipping ({view.ActualHeight})");
        string render = Path.GetFullPath(TestPaths.Report("C:/CubeControl-Build/reverb-research/reverb-editor.png")); Directory.CreateDirectory(Path.GetDirectoryName(render)!);
        var bitmap = new RenderTargetBitmap(560, (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = File.Create(render)) png.Save(output);
        host.Close();
        await model.DisconnectAsync();
        Check(!model.ReverbPreviewActive && !model.Connected && ReverbParameters.Read(fake.Dsp) == new ReverbParameters(1, 0.55f, 8, 80, 10),
            "normal disconnect restores reverb before closing transport");
        Check(!File.Exists(Path.Combine(directory, "pending-reverb-preview.json")), "verified restore clears pending recovery");
        var second = new CubeSession(fake); await second.Identify(); await model.AttachSession(second); await model.ReadReverbAsync();
        model.Reverb.ReturnLevel = 50;
        for (int i = 0; i < 200 && (ReverbParameters.Read(fake.Dsp).ReturnLevel != 40 || model.Busy); i++) await Task.Delay(10);
        model.Dispose(); // Abnormal shutdown ends the app session; only the current pedal RAM survives.
        using var reopened = new EditorViewModel(directory);
        int writes = fake.Writes;
        // Construction loads local state; do not enumerate physical MIDI ports here.
        Check(!reopened.ReverbPreviewActive && !reopened.CanEditReverb && fake.Writes == writes,
            "restart ignores previous session without reapplying controls");
        var third = new CubeSession(fake); await third.Identify(); await reopened.AttachSession(third);
        var currentDsp = fake.Dsp.ToArray();
        await reopened.EndReverbPreviewAsync();
        Check(fake.Dsp.SequenceEqual(currentDsp) && !reopened.ReverbPreviewActive,
            "new session never restores the previous snapshot");
        await reopened.ReadReverbAsync();
        fake.State[0] = 1;
        await Task.Delay(2300);
        Check(!reopened.CanEditReverb && reopened.ReverbPreviewActive && !File.Exists(Path.Combine(directory, "pending-reverb-preview.json")),
            "idle monitoring stops on physical context change and keeps recovery");
        fake.State[0] = 0;
        await reopened.EndReverbPreviewAsync();
        string presetFile = Path.Combine(directory, "import.reverb.json");
        var imported = new ReverbParameters(0.5f, 0.2f, 2, 40, 5);
        await File.WriteAllTextAsync(presetFile, ReverbPreset.Export(third.IdentityHex, imported));
        writes = fake.Writes;
        await reopened.LoadReverbFileAsync(presetFile);
        Check(reopened.Reverb.Parameters == imported && fake.Writes == writes, "import only prepares local parameters");
        string invalidDirectory = Path.Combine(directory, "invalid"); Directory.CreateDirectory(invalidDirectory);
        string invalidPath = Path.Combine(invalidDirectory, "pending-reverb-preview.json");
        await File.WriteAllTextAsync(invalidPath, "{}");
        using var invalid = new EditorViewModel(invalidDirectory);
        // Legacy recovery is checked without physical port discovery.
        Check(!invalid.CanStartReverb && await File.ReadAllTextAsync(invalidPath) == "{}", "malformed recovery is preserved and cannot be overwritten");
        Console.WriteLine($"{passed} reverb desktop checks passed. Render: {render}");
    }
}
