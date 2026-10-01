using System.IO;
using System.Windows;
using System.Windows.Input;
using Cube.Desktop;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class SavePresetsTests
{
    public static async Task Run()
    {
        using var fake = new FakeConnection();
        using var session = new CubeSession(fake);
        await session.Identify();
        var folder = Path.Combine(Path.GetTempPath(), "cube-save-" + Guid.NewGuid().ToString("N"));
        using var model = new EditorViewModel(folder);
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        Check(!model.Save.CanExecute(null), "Disconnected save enabled");
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model) { ShowInTaskbar = false };
        window.Show();
        async Task Settle() { while (model.Busy) await Task.Delay(25); CommandManager.InvalidateRequerySuggested(); await Task.Delay(80); window.UpdateLayout(); }
        await Settle();
        var button = (Wpf.Ui.Controls.Button)window.FindName("SavePresetsButton");
        var notice = (System.Windows.Controls.TextBlock)window.FindName("UnsavedPresetNotice");
        Check(notice.FontWeight == FontWeights.Normal, "Clean bank status must start without warning emphasis");
        try
        {
            foreach (var width in new[] { 640, 1024, 1280, 1920 })
            {
                window.Width = width; window.Height = 940; await Settle();
                Point? position = null;
                for (int preset = 0; preset < 3; preset++)
                for (int stage = 0; stage < 6; stage++)
                {
                    model.SelectedIndex = preset; model.SelectedStage = stage; await Settle();
                    Check(!model.Dirty && !model.Save.CanExecute(null) && !button.IsEnabled, "Navigation enabled clean save");
                    var point = button.TranslatePoint(new Point(), (UIElement)window.FindName("EditHeaderGrid"));
                    position ??= point;
                    Check((point - position.Value).Length < 1, "Module navigation moved save button");
                    Check(button.Appearance == Wpf.Ui.Controls.ControlAppearance.Primary, "Module navigation changed save appearance");
                }
                Console.WriteLine($"PASS clean save disabled; A/B/C and all six modules stable at {width}.");
            }
            Check(fake.Writes == 0, "Navigation wrote to device");
            model.Save.Execute(null);
            Check(!model.Busy && fake.Writes == 0, "Direct invocation saved a clean bank");
            foreach (var width in new[] { 640, 1024, 1280, 1920 })
            {
                window.Width = width; await Settle();
                var chain = (UIElement)window.FindName("EffectChain");
                var before = chain.TranslatePoint(new Point(), window);
                var saveBefore = button.TranslatePoint(new Point(), window);
                double original = model.Current.Volume.Value;
                model.Current.Volume.Value = original + 1; await Settle();
                Check((chain.TranslatePoint(new Point(), window) - before).Length < 1, "Dirty warning moved effect cards");
                Check((button.TranslatePoint(new Point(), window) - saveBefore).Length < 1, "Dirty warning moved save button");
                model.Current.Volume.Value = original; await Settle();
                Check((chain.TranslatePoint(new Point(), window) - before).Length < 1, "Clearing warning moved effect cards");
                Console.WriteLine($"PASS warning preserves cards and save position at {width}.");
            }

            for (int preset = 0; preset < 3; preset++)
            {
                model.SelectedIndex = preset;
                double original = model.Current.Volume.Value;
                model.Current.Volume.Value = original + 1; await Settle();
                Check(model.Dirty && button.IsEnabled && model.Save.CanExecute(null), "Edit did not enable save");
                Check(notice.FontWeight == FontWeights.SemiBold && notice.ToolTip != null, "Editing must show persistent save reminder");
                model.Current.Volume.Value = original; await Settle();
                Check(!model.Dirty && !button.IsEnabled, "Returning to saved value did not disable save");
                Check(notice.FontWeight == FontWeights.Normal, "Reverting changes must clear warning emphasis");
                model.Current.Volume.Value = original + 1; await Settle();
                model.Undo.Execute(null); await Settle();
                Check(!model.Dirty && !button.IsEnabled, "Undo did not disable save");
            }
            model.Current.Volume.Value += 1; await Settle();
            model.Save.Execute(null);
            Check(model.Busy && !model.Save.CanExecute(null), "Save not locked during transfer");
            model.Save.Execute(null); // A repeated invocation must be ignored while busy.
            await Settle();
            Check(!model.Dirty && !button.IsEnabled && model.ErrorMessage.Length == 0, "Verified save did not clear pending changes");
            Check(notice.FontWeight == FontWeights.Normal, "Confirmed save must clear warning emphasis");
            Check(Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories).Length >= 2, "Save backups missing");
            model.Current.Volume.Value += 1; await Settle();
            await model.Execute(() => model.LoadWavAndPreviewAsync("test.wav", new IrAudio(48000, 1, 32, "float", [.5f, .2f])));
            await Settle();
            Check(model.Dirty && !button.IsEnabled && !model.Save.CanExecute(null), "IR preview allowed save");
            model.EndPreview.Execute(null); await Settle();
            Check(model.Dirty && button.IsEnabled, "Ending preview lost pending save");
            await model.Execute(() => Task.CompletedTask);
            fake.Bank[1] ^= 1;
            model.Save.Execute(null); await Settle();
            Check(model.Dirty && !button.IsEnabled && model.ErrorMessage.Length > 0, "External bank conflict did not preserve edits and block save");
            Check(notice.FontWeight == FontWeights.SemiBold, "Failed save must retain reminder");
            await model.Execute(model.DisconnectAsync); await Settle();
            Check(model.Dirty && !button.IsEnabled, "Disconnect did not preserve dirty state and disable save");
            Console.WriteLine("PASS edit/revert/undo on A/B/C, verified save, backups, busy lock, IR preview, restoration and disconnect.");
        }
        finally { window.Close(); }
    }
}
