using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Views;
using Cube.Protocol;
using Cube.Desktop.Localization;

internal static class ChorusDesktopTests
{
    public static async Task Run()
    {
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
        string directory = Path.Combine(Path.GetTempPath(), "cube-chorus-test-" + Guid.NewGuid().ToString("N"));
        var fake = new ChorusPreviewConnection();
        var session = new CubeSession(fake);
        await session.Identify();
        using var model = new EditorViewModel(directory);
        await model.AttachSession(session);
        await model.ReadChorusAsync();
        Check(model.ChorusPreviewActive && model.CanEditChorus && !model.CanEdit && !model.CanUseAmp && !model.CanOperate && !model.CanStartPhaser && !model.CanStartDelay,
            "chorus preview excludes preset edits and amp/IR operations");
        Check(File.Exists(Path.Combine(directory, "pending-chorus-preview.json")), "chorus durable recovery created");
        model.Chorus.Depth = 60; model.Chorus.Depth = 70; model.Chorus.Depth = 50;
        for (int i = 0; i < 200 && (ChorusParameters.Read(fake.Dsp).Depth != 0.2f || model.Busy); i++) await Task.Delay(10);
        Check(ChorusParameters.Read(fake.Dsp).Depth == 0.2f && fake.Writes == 1, "chorus rapid edits coalesce to final value");
        string presetPath = Path.Combine(directory, "import.chorus.json");
        var imported = ChorusParameters.Default with { Depth = 0.1f, Mix = 0.425f };
        await File.WriteAllTextAsync(presetPath, ChorusPreset.Export(session.IdentityHex, imported));
        int beforeImport = fake.Writes;
        await model.LoadChorusFileAsync(presetPath);
        Check(fake.Writes == beforeImport && model.Chorus.Parameters == imported, "chorus import remains local until explicitly applied");
        model.ApplyChorusEdits();
        for (int i = 0; i < 200 && (ChorusParameters.Read(fake.Dsp) != imported || model.Busy); i++) await Task.Delay(10);
        Check(ChorusParameters.Read(fake.Dsp) == imported, "chorus imported controls apply to active preview");
        await File.WriteAllTextAsync(presetPath, ChorusPreset.Export(session.IdentityHex + "01", imported));
        bool rejected = false;
        try { await model.LoadChorusFileAsync(presetPath); } catch (FormatException) { rejected = true; }
        Check(rejected, "chorus import rejects a different identity during preview");
        var view = new ChorusEditor { DataContext = model, Width = 560, Background = Brushes.White, Foreground = Brushes.Black };
        view.SetResourceReference(Control.BackgroundProperty, "CubeCanvas");
        view.SetResourceReference(Control.ForegroundProperty, "CubeText");
        var host = new Window { Content = view, Width = 600, Height = 900, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, SizeToContent = SizeToContent.Height };
        host.Show();
        ((Expander)view.Content).IsExpanded = true;
        await Task.Delay(400);
        view.Measure(new Size(560, 900)); view.Arrange(new Rect(0, 0, 560, view.DesiredSize.Height)); view.UpdateLayout();
        Check(view.ActualHeight > 300 && view.ActualHeight < 900, $"experimental chorus editor lays out without clipping ({view.ActualHeight})");
        string render = Path.GetFullPath("artifacts/chorus/chorus-editor.png"); Directory.CreateDirectory(Path.GetDirectoryName(render)!);
        var bitmap = new RenderTargetBitmap(560, (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = File.Create(render)) png.Save(output);
        LanguageService.Instance.Preview("en-US");
        view.Width = 320; host.Width = 360;
        await Task.Delay(150);
        view.Measure(new Size(320, double.PositiveInfinity)); view.Arrange(new Rect(0, 0, 320, view.DesiredSize.Height)); view.UpdateLayout();
        Check(((Expander)view.Content).Header.ToString() == "Chorus creator · experimental" && view.ActualHeight < 1200,
            "chorus compact editor translates live and wraps content");
        var compact = new RenderTargetBitmap(320, (int)Math.Ceiling(view.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        compact.Render(view); var compactPng = new PngBitmapEncoder(); compactPng.Frames.Add(BitmapFrame.Create(compact));
        using (var output = File.Create(Path.GetFullPath("artifacts/chorus/chorus-editor-en-compact.png"))) compactPng.Save(output);
        LanguageService.Instance.Preview("pt-BR");
        host.Close();
        await model.DisconnectAsync();
        Check(!model.ChorusPreviewActive && !model.Connected && ChorusParameters.Read(fake.Dsp) == ChorusParameters.Default,
            "normal disconnect restores chorus before closing transport");
        Check(!File.Exists(Path.Combine(directory, "pending-chorus-preview.json")), "verified restore clears pending recovery");
        var second = new CubeSession(fake); await second.Identify(); await model.AttachSession(second); await model.ReadChorusAsync();
        model.Chorus.Mix = 50;
        for (int i = 0; i < 200 && (ChorusParameters.Read(fake.Dsp).Mix != 0.425f || model.Busy); i++) await Task.Delay(10);
        model.Dispose(); // Abnormal shutdown: retain pending record, never silently discard it.
        using var reopened = new EditorViewModel(directory);
        int writes = fake.Writes;
        await reopened.Initialize();
        Check(reopened.ChorusPreviewActive && !reopened.CanEditChorus && fake.Writes == writes,
            "restart loads pending recovery without reapplying controls");
        var third = new CubeSession(fake); await third.Identify(); await reopened.AttachSession(third);
        await reopened.EndChorusPreviewAsync();
        Check(ChorusParameters.Read(fake.Dsp) == ChorusParameters.Default && !reopened.ChorusPreviewActive,
            "explicit recovery survives process restart");
        await reopened.ReadChorusAsync();
        fake.State[0] = 1;
        await Task.Delay(2300);
        Check(!reopened.CanEditChorus && reopened.ChorusPreviewActive && File.Exists(Path.Combine(directory, "pending-chorus-preview.json")),
            "idle monitoring stops on physical context change and keeps recovery");
        fake.State[0] = 0;
        await reopened.EndChorusPreviewAsync();
        string corruptDirectory = Path.Combine(directory, "corrupt"); Directory.CreateDirectory(corruptDirectory);
        string corruptPath = Path.Combine(corruptDirectory, "pending-chorus-preview.json");
        await File.WriteAllTextAsync(corruptPath, "{invalid");
        using var corrupted = new EditorViewModel(corruptDirectory);
        await corrupted.Initialize();
        using var corruptFake = new ChorusPreviewConnection(); var corruptSession = new CubeSession(corruptFake);
        await corruptSession.Identify(); await corrupted.AttachSession(corruptSession);
        Check(!corrupted.CanStartChorus && !corrupted.CanStartPhaser && !corrupted.CanEdit && !corrupted.CanUseAmp &&
            File.ReadAllText(corruptPath) == "{invalid" && corruptFake.Writes == 0, "corrupt recovery blocks new previews without overwriting evidence");
        Console.WriteLine($"{passed} chorus desktop checks passed. Render: {render}");
    }
}
