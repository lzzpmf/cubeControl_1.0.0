using System.IO;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class IrAuditionTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-audition-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            using var audio = new FakeAudio();
            using var owner = new EditorViewModel(folder, audio);
            var player = owner.IrAudition;
            foreach (var track in player.Tracks)
            {
                var wav = IrAudio.Decode(await File.ReadAllBytesAsync(track.Path));
                Check(wav.Duration > 0 && wav.Samples.All(float.IsFinite), "published raw clip decodes: " + track.Name);
            }
            Check(player.Tracks.Count == 5 && player.Loop && player.Volume == 30, "five clips and agreed defaults");
            player.SelectedOutput = new("fake-output", "Pedal");
            Check(player.OutputStateLabel == "Saída selecionada" && player.ConnectionLabel == "Pedal desconectado", "output selection does not imply pedal connection");
            await player.ToggleAsync(); Check(!player.IsPlaying, "no playback without operational MIDI");
            using var connection = new FakeConnection(); using var session = new CubeSession(connection); await session.Identify();
            await owner.Execute(() => owner.AttachSession(session));
            var before = await session.ReadIrRam();
            int presetWrites = owner.WriteCommandCount;
            Check(player.CanStart && !owner.Ir.PreviewActive, "current preset can be auditioned without loading an IR");
            await player.ToggleAsync();
            Check(player.IsPlaying && audio.Opens == 1, "raw playback starts for the current preset");
            Check(owner.WriteCommandCount == presetWrites, "starting preset audition does not write device state");
            player.PanelVisible = false;
            Check(player.IsPlaying && player.ShowMiniPlayer, "audition remains available outside the IR page");
            player.PanelVisible = true;
            await player.ToggleAsync();
            var ir = new IrAudio(48000, 1, 32, "Float", [1, 0, 0]);
            await owner.Execute(() => owner.LoadWavAndPreviewAsync("test-ir.wav", ir));
            int writes = owner.WriteCommandCount;
            await player.ToggleAsync(); Check(player.IsPlaying && audio.Opens == 1, "confirmed preview enables raw playback");
            audio.Position(3);
            var token = await player.SuspendForTransfer(); Check(!player.IsPlaying && audio.Muted, "transfer pauses and mutes playback");
            await player.CompleteTransfer(token, true);
            Check(player.IsPlaying && audio.Snapshot.Position == 3 && audio.Opens == 1, "IR transfer resumes same clip position");
            await owner.Execute(() => owner.LoadWavAndPreviewAsync("second-ir.wav", ir));
            Check(player.IsPlaying && audio.Snapshot.Position == 3 && audio.Opens == 1, "actual RAM preview keeps clip and position");
            await player.ToggleAsync(); Check(!player.IsPlaying && audio.Snapshot.Position == 3, "pause preserves position");
            await player.ToggleAsync();
            token = await player.SuspendForTransfer(); await player.CompleteTransfer(token, false);
            Check(!player.IsPlaying, "failed transfer never resumes");
            await player.ToggleAsync(); token = await player.SuspendForTransfer(); await player.StopAsync(); await player.CompleteTransfer(token, true);
            Check(!player.IsPlaying && audio.Snapshot.Position == 0, "stop cancels transfer resume");
            player.Volume = 42; await player.ToggleAsync(); player.SelectedTrack = player.Tracks[1]; await player.PendingOperation;
            Check(player.Volume == 42, "clip change preserves audition volume");
            Check(audio.Snapshot.Source == player.Tracks[1].Path && audio.Snapshot.Position == 0, "clip change opens new raw clip at zero");
            player.SelectedOutput = null; await player.StopAsync(); Check(!player.IsPlaying && !player.CanStart, "output loss stops and disables playback");
            player.SelectedOutput = new("fake-output", "Pedal"); await player.ToggleAsync();
            await owner.Execute(owner.EndPreviewAsync);
            Check(player.IsPlaying && player.CanStart && !owner.Ir.PreviewActive, "ending RAM preview resumes audition of the restored preset");
            var after = await session.ReadIrRam();
            Check(before.Flag == after.Flag && before.Payload.SequenceEqual(after.Payload), "ending preview restores original RAM");
            Check(owner.WriteCommandCount == writes, "audition does not save slots or presets");
            if (player.SelectedOutput is { } fixtureOutput) player.Outputs.Add(fixtureOutput);
            int originalZoom = Cube.Desktop.Appearance.ZoomService.Instance.Percent;
            var window = new Cube.Desktop.MainWindow(owner, initializeDevices: false) { ShowInTaskbar = false, Height = 900 };
            window.Show();
            try
            {
                var tabs = (System.Windows.Controls.TabControl)window.FindName("Tabs");
                tabs.SelectedIndex = 1;
                foreach (int page in new[] { 0, 3 })
                {
                    tabs.SelectedIndex = page;
                    await player.PendingOperation;
                    Check(player.IsPlaying && player.ShowMiniPlayer, "pedalboard and amp creator navigation preserve audition");
                }
                tabs.SelectedIndex = 1;
                var card = (System.Windows.FrameworkElement)window.FindName("IrAuditionCard");
                string reports = TestPaths.Report("C:/CubeControl-Build/reports/ir-audition"); Directory.CreateDirectory(reports);
                foreach (var language in new[] { "pt-BR", "en-US" })
                foreach (var theme in new[] { Cube.Desktop.Appearance.ThemeMode.Light, Cube.Desktop.Appearance.ThemeMode.Dark })
                foreach (int width in new[] { 640, 1280 })
                foreach (int zoom in new[] { 100, 200 })
                {
                    Cube.Desktop.Localization.LanguageService.Instance.Preview(language);
                    Cube.Desktop.Appearance.ThemeService.Instance.Preview(theme);
                    Cube.Desktop.Appearance.ZoomService.Instance.Preview(zoom);
                    window.Width = width; await Task.Delay(150); window.UpdateLayout(); card.BringIntoView();
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    window.UpdateLayout();
                    var bounds = card.TransformToAncestor(window).TransformBounds(new System.Windows.Rect(0, 0, card.ActualWidth, card.ActualHeight));
                    Check(card.ActualWidth > 0 && bounds.Width <= window.ActualWidth, "audition card fits " + language + "/" + theme + "/" + width + " (actual " + card.ActualWidth + ")");
                    var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)card.ActualWidth, (int)card.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    var visual = new System.Windows.Media.DrawingVisual();
                    using (var drawing = visual.RenderOpen())
                    {
                        var rect = new System.Windows.Rect(card.RenderSize);
                        drawing.DrawRectangle((System.Windows.Media.Brush)window.FindResource("CubeCanvas"), null, rect);
                        drawing.DrawRectangle(new System.Windows.Media.VisualBrush(card), null, rect);
                    }
                    bitmap.Render(visual);
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    using var image = File.Create(Path.Combine(reports, $"{language}-{theme}-{width}-{zoom}.png")); encoder.Save(image);
                }
            }
            finally { Cube.Desktop.Appearance.ZoomService.Instance.Preview(originalZoom); Cube.Desktop.Localization.LanguageService.Instance.Preview("pt-BR"); Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark); window.Close(); }

        }
        finally { Directory.Delete(folder, true); }
    }
    private sealed class FakeAudio : IIrAuditionService
    {
        public BackingPlaybackSnapshot Snapshot { get; private set; } = new(BackingPlaybackState.Empty, 0, 0, null, 0);
        public int Opens; public bool Muted;
        public void Position(double value) => Snapshot = Snapshot with { Position = value };
        public Task Open(string path, string output, double duration, bool loop, double volume) { Opens++; Snapshot = new(BackingPlaybackState.Ready, 0, duration, output, 0, Source: path); return Task.CompletedTask; }
        public Task Play() { Snapshot = Snapshot with { State = BackingPlaybackState.Playing }; return Task.CompletedTask; }
        public Task Pause() { Snapshot = Snapshot with { State = BackingPlaybackState.Paused }; return Task.CompletedTask; }
        public Task Clear() { Snapshot = new(BackingPlaybackState.Empty, 0, 0, null, 0); return Task.CompletedTask; }
        public Task Configure(bool loop, double duration) => Task.CompletedTask;
        public Task Volume(double volume, bool muted) { Muted = muted; return Task.CompletedTask; }
        public void Dispose() { }
    }
}
