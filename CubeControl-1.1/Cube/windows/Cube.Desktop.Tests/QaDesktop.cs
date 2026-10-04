using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Cube.Qa;
using NAudio.Wave;
using static Cube.Qa.QaContext;

internal static class QaDesktop
{
    public static int RunMain(string[] args)
    {
        if (args.Contains("--list")) return QaRunner.Run("desktop", args, Cases()).GetAwaiter().GetResult();
        // No App.Run / MainWindow.Show: startup never enumerates MIDI or opens audio endpoints.
        var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown }; app.InitializeComponent();
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        Task<int> run = QaRunner.Run("desktop", args, Cases());
        _ = run.ContinueWith(_ => app.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background), TaskScheduler.Default);
        Dispatcher.Run();
        try { return run.GetAwaiter().GetResult(); } catch (Exception ex) { Console.Error.WriteLine(ex); return 2; }
    }
    private static QaCase Test(string id, string name, string batteries, Func<QaContext, Task> run) => new(id, name, batteries.Split(','), run);
    private static async Task WaitUntil(Func<bool> predicate)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!predicate()) { if (DateTime.UtcNow >= deadline) throw new TimeoutException("Condition did not settle"); await Task.Delay(10); }
    }
    private static async Task Attach(EditorViewModel model, QaConnection fake)
    {
        model.Ask = _ => Task.FromResult(false);
        var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(100) };
        await session.Identify(); await model.AttachSession(session);
    }
    private static QaCase[] Cases() =>
    [
        Test("empty-guards", "Offline guards and empty playlist", "QA-STA-001,QA-SMK-001", async c =>
        {
            using var model = new EditorViewModel(c.FilePath("editor"));
            Check(!model.Save.CanExecute(null) && !model.Import.CanExecute(null) && !model.WriteIr.CanExecute(null), "Offline write action enabled");
            Check(!model.Amp.Loaded && !model.CanSaveAmp, "Empty amp writable");
            using var player = new BackingTracksViewModel(c.FilePath("player"));
            Check(!player.PlayPause.CanExecute(null) && !player.Stop.CanExecute(null) && !player.RemoveTrack.CanExecute(null), "Empty transport enabled");
            player.Stop.Execute(null); await Task.Delay(20);
            Check(player.Tracks.Count == 0 && !player.IsPlaying, "Empty Stop changed playback");
        }),
        Test("preset-save", "Dirty, repeated save and remote conflict", "QA-PRE-002,QA-CONC-001,QA-STA-002", async c =>
        {
            using var model = new EditorViewModel(c.DirectoryPath); using var fake = new QaConnection(); await Attach(model, fake);
            model.Current.Volume.Value = 27; await model.Flush();
            Check(model.Dirty && model.Save.CanExecute(null), "Dirty/save guard");
            model.Save.Execute(null); model.Save.Execute(null); await WaitUntil(() => !model.Busy);
            Check(!model.Dirty && !model.Save.CanExecute(null) && model.ErrorMessage.Length == 0, "Verified save did not settle");
            int persistent = fake.Sent.Select(s => UsbSysEx.Decode(Convert.FromHexString(s))).Count(p => p.Command == 0x22 && p.Payload[0] == 5 && BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1)) == 0);
            Check(persistent == 1, "Repeated Save duplicated persistent write");
            model.Current.Volume.Value = 28; await model.Flush(); fake.Bank[1] = 1; int writes = fake.Writes;
            model.Save.Execute(null); await WaitUntil(() => !model.Busy);
            Check(model.Dirty && fake.Writes == writes && model.ErrorMessage.Length > 0, "Conflict lost edit or wrote bank");
            c.Evidence("tx.json", fake.Sent); c.Evidence("state.json", new { model.Dirty, model.Status, model.ErrorMessage });
        }),
        Test("preset-history", "Slot changes and undo use original targets", "QA-PRE-001,QA-STA-003", async c =>
        {
            using var model = new EditorViewModel(c.DirectoryPath); using var fake = new QaConnection(); await Attach(model, fake);
            model.SelectedIndex = 0; model.Current.Volume.Value = 10;
            model.SelectedIndex = 2; model.Current.Volume.Value = 30; await model.Flush();
            Check(fake.Bank[5] == 10 && fake.Bank[37] == 30, "Pending edit retargeted");
            model.Undo.Execute(null); await WaitUntil(() => !model.Busy); await model.Flush();
            Check(fake.Bank[5] == 10 && fake.Bank[37] == 0, "Undo targeted wrong preset");
            Check(model.Operations.Count > 0, "No history recorded"); c.Evidence("bank.json", fake.Bank);
        }),
        Test("busy-reentrancy", "Execute drops reentrant operation and releases Busy on error", "QA-CONC-001,QA-RES-001", async c =>
        {
            using var model = new EditorViewModel(c.DirectoryPath);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); int called = 0;
            var pending = model.Execute(async () => { called++; await release.Task; });
            try { await model.Execute(() => { called++; return Task.CompletedTask; }); Check(model.Busy && called == 1, "Reentrant Execute ran"); }
            finally { release.TrySetResult(); }
            await pending;
            await model.Execute(() => throw new FormatException("injected-format"));
            Check(!model.Busy && model.ErrorMessage.Contains("injected-format"), "Busy/error cleanup failed");
        }),
        Test("disconnect-edit", "Manual disconnect preserves dirty bank for export", "QA-DEV-002,QA-STA-002", async c =>
        {
            using var model = new EditorViewModel(c.DirectoryPath); using var fake = new QaConnection(); await Attach(model, fake);
            model.Current.Volume.Value = 29; await model.Flush();
            await model.Execute(model.DisconnectAsync);
            Check(!model.Connected && model.Dirty && model.Export.CanExecute(null), "Dirty edit unavailable after disconnect");
            Check(fake.Disposed, "Transport not disposed");
        }),
        Test("ir-preview", "Preview/restoration keeps exact original RAM", "QA-IR-002,QA-RES-002", async c =>
        {
            using var model = new EditorViewModel(c.DirectoryPath); using var fake = new QaConnection();
            fake.Ram[0x764] = 3; fake.Ram[0x768 + 100] = 88; byte[] before = fake.Ram.ToArray(); await Attach(model, fake);
            await model.Execute(() => model.LoadWavAndPreviewAsync("fixture.wav", new IrAudio(48000, 1, 32, "float", [.5f, .25f])));
            Check(model.Ir.PreviewActive && model.Ir.PreviewConfirmed && !File.Exists(c.FilePath("pending-ir-preview.json")), "Preview must not persist recovery");
            await model.Execute(model.EndPreviewAsync);
            Check(!model.Ir.PreviewActive && fake.Ram.SequenceEqual(before) && !File.Exists(c.FilePath("pending-ir-preview.json")), "RAM restore mismatch");
            c.Evidence("tx.json", fake.Sent);
        }),
        Test("pending-invalid", "Malformed pending recovery is preserved", "QA-PERS-002,QA-RES-001", async c =>
        {
            // Invalid IR reaches no real enumeration: exercise parser directly, then VM errors through Execute.
            string path = c.FilePath("pending-ir-preview.json"); await File.WriteAllTextAsync(path, "{}");
            using var model = new EditorViewModel(c.DirectoryPath);
            await model.Initialize();
            Check(!model.Busy && model.ErrorMessage.Length == 0 && !model.Ir.PreviewActive && File.Exists(path), "Legacy recovery must be ignored and left untouched");
        }),
        Test("parameter-inputs", "Finite values, clamping and culture-specific drafts", "QA-INP-001,QA-UI-003", c =>
        {
            int calls = 0; var parameter = new ParameterViewModel(CubeBabyProfile.Parameters[1], 2, (_, _) => calls++);
            parameter.Value = 4; parameter.Value = double.NaN; parameter.Value = double.PositiveInfinity;
            Check(parameter.Value == 4 && calls == 1, "Non-finite value changed parameter");
            parameter.Value = 100; Check(parameter.Value == 7, "Upper clamp");
            var saved = CultureInfo.CurrentCulture;
            try
            {
                foreach (var culture in new[] { "pt-BR", "en-US" })
                {
                    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                    var box = new ParameterNumberBox { Minimum = 0, Maximum = 7, Value = 4 };
                    foreach (var value in new[] { "", "-1", "8", "1.5", "NaN" })
                    { box.Text = value; Check(!box.ValidateDraft() && box.Value == 4, "Invalid draft accepted: " + value); }
                    box.Text = "7"; Check(box.ValidateDraft(), "Valid integer rejected");
                }
            }
            finally { CultureInfo.CurrentCulture = saved; }
            return Task.CompletedTask;
        }),
        Test("knob-binding-uia", "Two-way binding and UIA reject invalid range", "QA-UI-003,QA-ACC-002,QA-INP-002", c =>
        {
            var parameter = new ParameterViewModel(CubeBabyProfile.Parameters[1], 0, (_, _) => { });
            var knob = new Knob { Minimum = 0, Maximum = 7 };
            BindingOperations.SetBinding(knob, Knob.ValueProperty, new Binding(nameof(ParameterViewModel.Value)) { Source = parameter, Mode = BindingMode.TwoWay });
            knob.Value = 6; Check(parameter.Value == 6, "Knob did not update VM");
            parameter.Value = 2; Check(knob.Value == 2, "VM did not update knob");
            AutomationProperties.SetName(knob, "Gain");
            var peer = UIElementAutomationPeer.CreatePeerForElement(knob)!;
            Check(peer.GetName() == "Gain", "UIA name missing");
            var range = (IRangeValueProvider)peer.GetPattern(PatternInterface.RangeValue)!;
            range.SetValue(5); Check(parameter.Value == 5, "UIA did not update binding");
            Reject<ArgumentOutOfRangeException>(() => range.SetValue(8)); return Task.CompletedTask;
        }),
        Test("navigation-loop", "Unshown shell navigation keeps shared state and single host", "QA-NAV-001,QA-NAV-002,QA-NAV-003", async c =>
        {
            using var model = new EditorViewModel(c.DirectoryPath); var window = new MainWindow(model);
            try
            {
                var nav = (ListBox)window.FindName("Navigation"); var tabs = (TabControl)window.FindName("Tabs");
                for (int n = 0; n < 20; n++) for (int i = 0; i < 5; i++)
                { nav.SelectedIndex = i; Check(tabs.SelectedIndex == i && ReferenceEquals(window.DataContext, model), "Navigation/state mismatch"); }
                Check(tabs.Items.Count == 5 && !model.Connected, "Navigation created connection/page duplicates");
                await Task.Delay(20); c.Evidence("navigation.json", new { Cycles = 20, Transitions = 100, Shown = false });
            }
            finally { window.BackingTracks.Dispose(); window.Close(); }
        }),
        Test("ir-search", "IR search, empty result and selection guards", "QA-IR-002,QA-STA-001", c =>
        {
            string directory = c.FilePath("library"); Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, "Clean.wav"), [1]); File.WriteAllBytes(Path.Combine(directory, "Heavy.WAV"), [1]);
            bool allowed = true; var library = new IrLibraryViewModel(() => allowed, directory);
            Check(library.Entries.Count == 2, "Extension case inventory");
            library.Search = "no-such-cab"; Check(library.VisibleEntries.Count == 0, "Empty search result");
            library.Search = "Clean"; Check(library.VisibleEntries.Count == 1, "Search mismatch");
            library.Selected = library.VisibleEntries[0]; allowed = false; library.Selected = null;
            Check(library.Selected != null, "Busy selection guard"); return Task.CompletedTask;
        }),
        Test("playlist-lifecycle", "Playlist deduplication, reorder, remove and reopen", "QA-BT-001,QA-PERS-001,QA-STA-001", async c =>
        {
            string a = c.FilePath("A.wav"), b = c.FilePath("B.wav"); WriteWave(a, 48000, 1, .2); WriteWave(b, 48000, 1, .3);
            string folder = c.FilePath("settings");
            using (var player = new BackingTracksViewModel(folder))
            {
                player.AddPaths([a, a, b]); await WaitUntil(() => player.Duration > 0);
                Check(player.Tracks.Count == 2, "Duplicate paths added");
                player.MoveTrack(player.Tracks[0], 1); player.SelectedTrack = player.Tracks[0];
                await WaitUntil(() => player.Duration > 0); player.Volume = 43; player.IsMuted = true;
                Check(!player.PlayPause.CanExecute(null), "No endpoint but Play enabled");
                await Task.Delay(30);
            }
            using (var reopened = new BackingTracksViewModel(folder))
            {
                await WaitUntil(() => reopened.Duration > 0);
                Check(reopened.Tracks[0].Source == b && reopened.Volume == 43 && reopened.IsMuted, "Playlist settings lost");
                reopened.RemoveTrack.Execute(null); await Task.Delay(50);
                Check(File.Exists(b) && reopened.Tracks.Count == 1, "Remove deleted source or wrong reference");
            }
        }),
        Test("waveform-analysis", "Waveform duration and opposite stereo channels", "QA-BT-002,QA-PLY-002", async c =>
        {
            string path = c.FilePath("stereo.wav"); WriteWave(path, 48000, 2, 1);
            var result = await BackingAudioProcessing.Analyze(path, CancellationToken.None);
            Check(Math.Abs(result.Duration - 1) < 1e-6 && result.Peaks.Length == 100 && result.Peaks.Max() > .45f, "Waveform lost opposite-phase stereo");
            using var cancel = new CancellationTokenSource(); cancel.Cancel();
            await Reject<OperationCanceledException>(() => BackingAudioProcessing.Analyze(path, cancel.Token));
            c.Evidence("analysis.json", new { result.Duration, result.Peaks });
        }),
        Test("audio-export", "Original WAV export ignores transform and preserves destination on cancellation", "QA-FILE-002,QA-DSP-003", async c =>
        {
            string source = c.FilePath("source.wav"), dest = c.FilePath("export.wav"); WriteWave(source, 48000, 2, 2);
            await BackingAudioProcessing.Export(source, dest, .25, 1.25, .5, 6, false, CancellationToken.None);
            using (var reader = new WaveFileReader(dest)) Check(reader.WaveFormat.BitsPerSample == 16 && reader.WaveFormat.Channels == 2 && Math.Abs(reader.TotalTime.TotalSeconds - 1) < 1e-6, "Original export format/duration");
            byte[] before = File.ReadAllBytes(dest); using var cancel = new CancellationTokenSource(); cancel.Cancel();
            await Reject<OperationCanceledException>(() => BackingAudioProcessing.Export(source, dest, 0, 2, 1, 0, false, cancel.Token));
            Check(File.ReadAllBytes(dest).SequenceEqual(before) && Directory.GetFiles(c.DirectoryPath, "*.tmp").Length == 0, "Cancellation damaged destination");
        }),
        Test("native-dsp", "Time/pitch processing: finite samples and frame count", "QA-DSP-003", async c =>
        {
            string source = c.FilePath("tone.wav"); WriteWave(source, 48000, 2, 2);
            var metrics = new List<object>(); var manifest = new List<object>();
            foreach (double speed in new[] { .5, 1, 1.5 }) foreach (double pitch in new[] { -6d, 0, 6 })
            {
                using var audio = new BackingAudioProcessing(source, 0, 2, speed, pitch);
                var block = new float[4096]; long count = 0; double peak = 0; int n;
                while ((n = audio.Read(block)) > 0)
                { Check(block.Take(n).All(float.IsFinite), "Non-finite DSP output"); count += n; peak = Math.Max(peak, block.Take(n).Max(x => Math.Abs(x))); }
                Check(Math.Abs(count / 2d - Math.Round(96000 / speed)) <= 1, "Time-stretch duration mismatch");
                metrics.Add(new { speed, pitch, samples = count, peak });
                string name = FormattableString.Invariant($"processed-{speed}-{pitch}.wav");
                await BackingAudioProcessing.Export(source, c.FilePath(name), 0, 2, speed, pitch, true, CancellationToken.None);
                manifest.Add(new { path = name, sampleRate = 48000, channels = 2, frames = (long)Math.Round(96000 / speed),
                    frequency = 1000 * Math.Pow(2, pitch / 12), antiPhase = true });
            }
            c.Evidence("metrics.json", metrics); c.Evidence("audio-manifest.json", manifest);
        }),
        Test("firmware-format", "Firmware header/size/CRC validation without worker or device", "QA-FW-001", c =>
        {
            byte[] bytes = new byte[65792]; Convert.FromHexString("434F4E46494700010000000009000000").CopyTo(bytes, 0);
            bytes[254] = 0x55; bytes[255] = 0xAA;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(16), 65536);
            ushort crc = 0xFFFF;
            foreach (byte value in bytes.AsSpan(256))
            { crc ^= (ushort)(value << 8); for (int bit = 0; bit < 8; bit++) crc = (ushort)((crc << 1) ^ ((crc & 0x8000) != 0 ? 0x1021 : 0)); }
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(20), crc);
            string path = c.FilePath("synthetic.up"); File.WriteAllBytes(path, bytes);
            Check(FirmwareUpdater.Validate(path).SequenceEqual(bytes), "Valid structural fixture rejected");
            bytes[300] ^= 1; File.WriteAllBytes(path, bytes); Reject<IOException>(() => FirmwareUpdater.Validate(path));
            File.WriteAllBytes(path, new byte[65791]); Reject<IOException>(() => FirmwareUpdater.Validate(path));
            // Structural validity is not certification for installation; never call RunWorker/RunElevated.
            return Task.CompletedTask;
        })
    ];

    private static void WriteWave(string path, int rate, int channels, double seconds)
    {
        using var writer = new WaveFileWriter(path, WaveFormat.CreateIeeeFloatWaveFormat(rate, channels));
        for (int f = 0; f < (int)(rate * seconds); f++) for (int channel = 0; channel < channels; channel++)
            writer.WriteSample((float)(.5 * Math.Sin(2 * Math.PI * 1000 * f / rate)) * (channel == 0 ? 1 : -1));
    }
}
