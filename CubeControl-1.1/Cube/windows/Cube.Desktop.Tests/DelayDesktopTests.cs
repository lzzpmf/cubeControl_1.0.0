using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Views;
using Cube.Desktop.Localization;
using Cube.Protocol;

internal static class DelayDesktopTests
{
    public static async Task Run()
    {
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
        string directory = Path.Combine(Path.GetTempPath(), "cube-delay-test-" + Guid.NewGuid().ToString("N"));
        var fake = new DelayConnection(); fake.Activate();
        var session = new CubeSession(fake);
        await session.Identify();
        using var model = new EditorViewModel(directory);
        await model.AttachSession(session);
        await model.ReadDelayAsync();
        Check(model.DelayPreviewActive && model.CanEditDelay && !model.CanEdit && !model.CanUseAmp && !model.CanOperate && !model.CanStartPhaser && !model.CanStartChorus && !model.CanStartReverb,
            "delay preview excludes preset edits and amp/IR operations");
        Check(!File.Exists(Path.Combine(directory, "pending-delay-preview.json")), "delay snapshot stays in memory");
        model.Delay.Mix = 10; model.Delay.Mix = 20; model.Delay.Mix = 30;
        for (int i = 0; i < 200 && (DelayParameters.Read(fake.Dsp).Mix != 0.3f || model.Busy); i++) await Task.Delay(10);
        Check(DelayParameters.Read(fake.Dsp).Mix == 0.3f && fake.Writes == 1, "delay rapid edits coalesce to final value");
        var view = new DelayEditor { DataContext = model, Width = 560, Background = Brushes.White, Foreground = Brushes.Black };
        view.SetResourceReference(Control.BackgroundProperty, "CubeCanvas");
        view.SetResourceReference(Control.ForegroundProperty, "CubeText");
        var host = new Window { Content = view, Width = 600, Height = 900, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, SizeToContent = SizeToContent.Height };
        host.Show();
        ((Expander)view.Content).IsExpanded = true;
        await Task.Delay(400);
        view.Measure(new Size(560, 900)); view.Arrange(new Rect(0, 0, 560, view.DesiredSize.Height)); view.UpdateLayout();
        Check(view.ActualHeight > 300 && view.ActualHeight < 1200, $"delay editor lays out without clipping ({view.ActualHeight})");
        string render = Path.GetFullPath("C:/CubeControl-Build/delay-research/delay-editor.png"); Directory.CreateDirectory(Path.GetDirectoryName(render)!);
        var bitmap = new RenderTargetBitmap(560, (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = File.Create(render)) png.Save(output);
        LanguageService.Instance.Preview("en-US");
        view.Width = 320; host.Width = 360;
        await Task.Delay(150);
        view.Measure(new Size(320, double.PositiveInfinity)); view.Arrange(new Rect(0, 0, 320, view.DesiredSize.Height)); view.UpdateLayout();
        Check(((Expander)view.Content).Header.ToString() == "Delay creator" && view.ActualHeight < 1280,
            "delay compact editor translates live and wraps content");
        Check(LanguageService.T("Profundidade da modulação (% do original)") == "Modulation depth (% of stock)", "delay parameter labels translated");
        var compact = new RenderTargetBitmap(320, (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        compact.Render(view); var compactPng = new PngBitmapEncoder(); compactPng.Frames.Add(BitmapFrame.Create(compact));
        using (var output = File.Create(Path.Combine(Path.GetDirectoryName(render)!, "delay-editor-en-compact.png"))) compactPng.Save(output);
        LanguageService.Instance.Preview("pt-BR");
        host.Close();
        await model.DisconnectAsync();
        Check(!model.DelayPreviewActive && !model.Connected && DelayParameters.Read(fake.Dsp) == new DelayParameters(9600, 0.25f, 0.1f, 0.01f, 0.25f),
            "normal disconnect restores delay before closing transport");
        Check(!File.Exists(Path.Combine(directory, "pending-delay-preview.json")), "verified restore clears pending recovery");
        var second = new CubeSession(fake); await second.Identify(); await model.AttachSession(second); await model.ReadDelayAsync();
        model.Delay.Depth = 50;
        for (int i = 0; i < 200 && (DelayParameters.Read(fake.Dsp).Depth != 0.05f || model.Busy); i++) await Task.Delay(10);
        model.Dispose(); // Abnormal shutdown ends the app session; only the current pedal RAM survives.
        using var reopened = new EditorViewModel(directory);
        int writes = fake.Writes;
        await reopened.Initialize();
        Check(!reopened.DelayPreviewActive && !reopened.CanEditDelay && fake.Writes == writes,
            "restart ignores previous session without reapplying controls");
        var third = new CubeSession(fake); await third.Identify(); await reopened.AttachSession(third);
        var currentDsp = fake.Dsp.ToArray();
        await reopened.EndDelayPreviewAsync();
        Check(fake.Dsp.SequenceEqual(currentDsp) && !reopened.DelayPreviewActive,
            "new session never restores the previous snapshot");
        await reopened.ReadDelayAsync();
        fake.State[0] = 1;
        await Task.Delay(2300);
        Check(!reopened.CanEditDelay && reopened.DelayPreviewActive && !File.Exists(Path.Combine(directory, "pending-delay-preview.json")),
            "idle monitoring stops on physical context change and keeps recovery");
        fake.State[0] = 0;
        await reopened.EndDelayPreviewAsync();
        // A physical-context fault intentionally requires a new bank read/reconnect
        // even after explicit restoration; do not bypass the editor's blocked state.
        var refreshed = new CubeSession(fake); await refreshed.Identify(); await reopened.AttachSession(refreshed);
        string presetFile = Path.Combine(directory, "import.delay.json");
        var imported = new DelayParameters(9601, 0.123f, 0.05f, 0.005f, 0.321f);
        await File.WriteAllTextAsync(presetFile, DelayPreset.Export(third.IdentityHex, imported));
        writes = fake.Writes;
        await reopened.LoadDelayFileAsync(presetFile);
        Check(reopened.Delay.Parameters == imported && fake.Writes == writes, "import only prepares local parameters");
        await reopened.ReadDelayAsync();
        writes = fake.Writes;
        await reopened.LoadDelayFileAsync(presetFile);
        await Task.Delay(100);
        Check(fake.Writes == writes && DelayParameters.Read(fake.Dsp) != imported, "active preview import waits for explicit apply");
        reopened.ApplyDelayEdits();
        for (int i = 0; i < 200 && (DelayParameters.Read(fake.Dsp) != imported || reopened.Busy); i++) await Task.Delay(10);
        Check(DelayParameters.Read(fake.Dsp) == imported, "explicit apply sends imported parameters");
        await reopened.EndDelayPreviewAsync();
        string invalidDirectory = Path.Combine(directory, "invalid"); Directory.CreateDirectory(invalidDirectory);
        string invalidPath = Path.Combine(invalidDirectory, "pending-delay-preview.json");
        await File.WriteAllTextAsync(invalidPath, "{}");
        using var invalid = new EditorViewModel(invalidDirectory);
        try { await invalid.Initialize(); } catch (Exception) { }
        var invalidSession = new CubeSession(fake); await invalidSession.Identify(); await invalid.AttachSession(invalidSession);
        Check(invalid.CanStartDelay && invalid.CanStartPhaser && invalid.CanStartChorus && invalid.CanStartReverb && invalid.CanUseAmp &&
            await File.ReadAllTextAsync(invalidPath) == "{}", "malformed legacy recovery does not block previews");
        var closeFake = new DelayConnection(); closeFake.Activate();
        var closeSession = new CubeSession(closeFake); await closeSession.Identify();
        using var closeModel = new EditorViewModel(Path.Combine(directory, "closing"));
        await closeModel.AttachSession(closeSession);
        // Construct the real window without showing it or enabling automatic USB connection.
        var main = new Cube.Desktop.MainWindow(closeModel);
        closeModel.SelectedStage = 3;
        await closeModel.ReadDelayAsync();
        closeModel.Delay.Depth = 50;
        for (int i = 0; i < 200 && (DelayParameters.Read(closeFake.Dsp).Depth != 0.05f || closeModel.Busy); i++) await Task.Delay(10);
        main.Close();
        for (int i = 0; i < 200 && closeModel.DelayPreviewActive; i++) await Task.Delay(10);
        Check(!closeModel.DelayPreviewActive && DelayParameters.Read(closeFake.Dsp).Depth == 0.1f &&
            !File.Exists(Path.Combine(directory, "closing", "pending-delay-preview.json")), "real window close restores delay and removes verified recovery");
        Console.WriteLine($"{passed} delay desktop checks passed. Render: {render}");
    }
}
