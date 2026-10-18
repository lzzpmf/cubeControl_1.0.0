using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using System.Windows;
using Cube.Desktop;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class AdvancedLifecycleTests
{
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); }
    static async Task Wait(EditorViewModel model)
    {
        await Task.Delay(100);
        for (int i = 0; i < 500 && model.Busy; i++) await Task.Delay(10);
        Check(!model.Busy && model.ErrorMessage == "", "operation completed: " + model.ErrorMessage);
    }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-lifecycle-" + Guid.NewGuid().ToString("N"));
        using var device = new MultiEffectConnection();
        using var transport = new EffectsFirmwareBConnection(device, device.Bank, device.State);
        using var session = new CubeSession(transport); await session.Identify();
        using var model = new EditorViewModel(folder); model.Ask = _ => Task.FromResult(true);
        await model.AttachSession(session);
        await Task.WhenAll(model.ReadChorusAsync(), model.ReadDelayAsync(), model.ReadReverbAsync(), model.ReadPhaserAsync());
        Check(Enumerable.Range(1, 4).All(model.HasAdvancedPreview) && model.ErrorMessage == "", "four concurrent openings retain independent previews: " + model.ErrorMessage + " active=" + string.Join(",", Enumerable.Range(1,4).Select(model.HasAdvancedPreview)));
        model.Phaser.Lower = 900; model.Delay.TimeSamples = 6000; model.Reverb.Wet = 25;
        await Wait(model);
        Check(PhaserParameters.Read(device.Dsp).Lower == 900 && DelayParameters.Read(device.Dsp).TimeSamples == 6000 && ReverbParameters.Read(device.Dsp).Wet == .25f, "simultaneous edits reach three independent DSP blocks");
        var dspBeforeVolume = device.Dsp.ToArray();
        model.Current.Volume.Value = 35; await Wait(model);
        Check(device.Bank[5] == 35 && device.Dsp.SequenceEqual(dspBeforeVolume), "ordinary volume edit preserves advanced DSP blocks");
        model.Chorus.Depth = 50;
        await model.ListenAdvancedModulationAsync(1); await Wait(model);
        Check(ChorusParameters.Read(device.Dsp).Depth == .2f && model.Phaser.Lower == 900, "switch to chorus retains phaser draft");
        await model.ListenAdvancedModulationAsync(2); await Wait(model);
        Check(PhaserParameters.Read(device.Dsp).Lower == 900, "switch back reapplies phaser draft");
        await Task.Delay(2200); await Wait(model);
        Check(model.CanEditChorus && model.CanEditPhaser && model.CanEditDelay && model.CanEditReverb, "idle monitoring does not stop other windows");
        Check(await model.CloseAdvancedPreviewAsync(1), "inactive chorus closes without restoring over phaser");
        Check(PhaserParameters.Read(device.Dsp).Lower == 900 && model.PhaserPreviewActive, "closing chorus preserves phaser RAM");
        await model.ReadChorusAsync(); await Wait(model);
        Check(model.ChorusPreviewActive, "closed chorus reopens");
        // A physical change affects the chorus context but must not poison delay/reverb.
        device.Bank[8] = 7; device.Dsp[0x63C] = 0;
        await Task.Delay(2200);
        Check(!model.CanEditChorus && model.CanEditDelay && model.CanEditReverb, "chorus context fault is isolated");
        Check(model.AdvancedErrors[1].Length > 0 && model.AdvancedErrors[3] == "" && model.AdvancedErrors[4] == "", "error banner belongs only to the affected window");
        int writes = device.DspWrites;
        Check(await model.CloseAdvancedPreviewAsync(1), "obsolete recovery can be closed");
        Check(device.DspWrites == writes && !Directory.Exists(Path.Combine(folder, "ArchivedEffectRecovery")),
            "obsolete recovery retired in memory without DSP writes");
        Check(await model.CloseAdvancedPreviewAsync(2), "inactive phaser closes after physical change");
        Check(await model.CloseAdvancedPreviewAsync(3) && await model.CloseAdvancedPreviewAsync(4), "delay and reverb restore independently");
        // Reopening must read the current pedal rather than restore an earlier app session.
        using var freshDevice = new MultiEffectConnection();
        using var freshTransport = new EffectsFirmwareBConnection(freshDevice, freshDevice.Bank, freshDevice.State);
        using var freshSession = new CubeSession(freshTransport); await freshSession.Identify();
        string recoveredFolder = Path.Combine(Path.GetTempPath(), "cube-recovery-" + Guid.NewGuid().ToString("N"));
        using (var first = new EditorViewModel(recoveredFolder))
        {
            await first.AttachSession(freshSession); await first.ReadChorusAsync();
            first.Chorus.Depth = 50; await Wait(first);
            // Stop monitoring without a normal close, leaving in-memory recovery intact.
            _ = typeof(EditorViewModel).GetField("chorusMonitorStop", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(first) is CancellationTokenSource stop ? Stop(stop) : throw new Exception("monitor missing");
            // Prevent disposal of the shared transport before the next simulated session.
            typeof(EditorViewModel).GetField("session", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(first, null);
        }
        using var resumed = new EditorViewModel(recoveredFolder);
        Check(!resumed.ChorusPreviewActive && !resumed.Chorus.Loaded, "new process has no previous preview or draft");
        await resumed.AttachSession(freshSession); await resumed.ReadChorusAsync();
        Check(resumed.CanEditChorus && resumed.Chorus.Depth == 50, "new session reads the current DSP after reopen");
        await resumed.CloseAdvancedPreviewAsync(1);
        await Task.WhenAll(resumed.ReadChorusAsync(), resumed.CloseAdvancedPreviewAsync(1));
        Check(!resumed.ChorusPreviewActive && !File.Exists(Path.Combine(recoveredFolder, "pending-chorus-preview.json")), "close during opening waits for initialization and restores");
        await resumed.ReadChorusAsync();
        freshDevice.Bank[8] = 7; freshDevice.Dsp[0x63C] = 0;
        await Task.Delay(2200);
        // Refresh only the test model's bank to simulate reconnecting to the current pedal.
        await resumed.AttachSession(freshSession);
        await resumed.ReadChorusAsync();
        Check(resumed.CanEditChorus && freshDevice.Bank[8] <= 6 && resumed.ErrorMessage == "", "opening an obsolete preview retires its memory snapshot and reads current effect");
        // Unknown values in an unchanged context must not be mistaken for an obsolete session.
        BinaryPrimitives.WriteSingleLittleEndian(freshDevice.Dsp.AsSpan(0x644), .12f);
        int beforeFailedRestore = freshDevice.DspWrites;
        Check(!await resumed.CloseAdvancedPreviewAsync(1) && resumed.ChorusPreviewActive && !File.Exists(Path.Combine(recoveredFolder, "pending-chorus-preview.json")), "failed restoration retains preview and in-memory recovery");
        Check(freshDevice.DspWrites == beforeFailedRestore, "unrecognized RAM is not overwritten");
        await Windows();
        Console.WriteLine("PASS advanced lifecycle complete");
    }
    static async Task Windows()
    {
        using var device = new MultiEffectConnection();
        using var transport = new EffectsFirmwareBConnection(device, device.Bank, device.State);
        using var session = new CubeSession(transport); await session.Identify();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-card-" + Guid.NewGuid().ToString("N")));
        model.Ask = _ => Task.FromResult(true); await model.AttachSession(session);
        var shell = new MainWindow(model, initializeDevices: false) { ShowActivated = false, ShowInTaskbar = false, Width = 1280, Height = 1000, Left = -10000 };
        shell.Show();
        var toggle = (System.Windows.Controls.Primitives.ButtonBase)shell.FindName("EffectCardToggle");
        var simple = (FrameworkElement)shell.FindName("SimpleEffectCard");
        var advanced = (FrameworkElement)shell.FindName("AdvancedEffectCard");
        void Flip() => toggle.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        try
        {
            for (int stage = 1; stage <= 4; stage++)
            {
                model.SelectedStage = stage; shell.UpdateLayout();
                Check(simple.IsVisible && !advanced.IsVisible, "effect starts on simple face " + stage);
                Flip(); await Wait(model); shell.UpdateLayout();
                Check(model.IsAdvancedCard && advanced.IsVisible && !simple.IsVisible && shell.OwnedWindows.Count == 0, "advanced face uses same card without popup " + stage);
                switch (stage)
                {
                    case 1: model.Chorus.Depth = 50; break;
                    case 2: model.Phaser.Lower = 900; break;
                    case 3: model.Delay.TimeSamples = 6000; break;
                    case 4: model.Reverb.Wet = 25; break;
                }
                await Wait(model);
                var dsp = device.Dsp.ToArray(); int writes = device.DspWrites;
                Flip(); await Task.Delay(50); shell.UpdateLayout();
                Check(simple.IsVisible && !advanced.IsVisible && model.HasAdvancedPreview(stage) && writes == device.DspWrites && device.Dsp.SequenceEqual(dsp), "flip back preserves RAM and preview " + stage);
                int offset = stage switch { 1 or 2 => 8, 3 => 6, _ => 3 };
                model.Current.Parameters[offset].Value += 1;
                await Wait(model);
                Check(stage switch {
                    1 => model.Chorus.Parameters == ChorusParameters.Read(device.Dsp) && model.Chorus.Depth != 50,
                    2 => model.Phaser.Parameters == PhaserParameters.Read(device.Dsp) && model.Phaser.Lower != 900,
                    3 => model.Delay.Parameters == DelayParameters.Read(device.Dsp) && model.Delay.TimeSamples != 6000,
                    _ => model.Reverb.Parameters == ReverbParameters.Read(device.Dsp) && model.Reverb.Wet != 25
                }, "simple control supersedes advanced values and refreshes model " + stage);
                Flip(); await Wait(model);
                Check(device.DspWrites == writes, "returning to advanced does not reapply stale draft " + stage);
                shell.UpdateLayout();
                Check(toggle.ActualWidth < 280 && toggle.ActualHeight < 60, "compact card button " + stage);
                var card = (FrameworkElement)shell.FindName("EffectDetailSurface");
                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)card.ActualWidth, (int)card.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                bitmap.Render(card);
                var png = new System.Windows.Media.Imaging.PngBitmapEncoder(); png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                Directory.CreateDirectory(TestPaths.Report("C:/CubeControl-Build/advanced-card/reports"));
                using (var file = File.Create(TestPaths.Report($"C:/CubeControl-Build/advanced-card/reports/card-{stage}.png"))) png.Save(file);
                Flip(); await Task.Delay(50);
            }
            model.SelectedStage = 1;
            foreach (double width in new[] { 640d, 1024d })
            {
                shell.Width = width;
                foreach (bool advancedFace in new[] { false, true })
                {
                    if (model.IsAdvancedCard != advancedFace) { Flip(); await Wait(model); }
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    shell.UpdateLayout();
                    var card = (FrameworkElement)shell.FindName("EffectDetailSurface");
                    Point buttonPosition = toggle.TranslatePoint(new Point(), card);
                    Check(buttonPosition.X >= 0 && buttonPosition.X + toggle.ActualWidth <= card.ActualWidth + 1, "responsive toggle remains inside card");
                    var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)card.ActualWidth, (int)card.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    bitmap.Render(card);
                    var png = new System.Windows.Media.Imaging.PngBitmapEncoder(); png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    using var file = File.Create(TestPaths.Report($"C:/CubeControl-Build/advanced-card/reports/card-{width}-{advancedFace}.png")); png.Save(file);
                }
            }
        }
        finally
        {
            shell.Close();
            for (int i = 0; i < 500 && shell.IsVisible; i++) await Task.Delay(10);
            Check(!shell.IsVisible, "test shell closes cleanly");
        }
    }
    static bool Stop(CancellationTokenSource stop) { stop.Cancel(); return true; }
}

// Shared hardware model: live preset writes reload their corresponding DSP block.
// All four editors use one transport, bank, physical preset and DSP image.
internal sealed class MultiEffectConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank = new byte[48], Dsp = new byte[0xF70], State = [0, 1];
    public int DspWrites;
    readonly ChorusPreviewConnection chorus = new();
    readonly PhaserConnection phaser = new();
    readonly DelayConnection delay = new();
    readonly ReverbConnection reverb = new();
    public MultiEffectConnection()
    {
        delay.Activate(); reverb.Activate();
        Bank[8] = 3; Bank[11] = 1; Bank[7] = 16; Bank[3] = 8; Bank[10] = 1;
        Reload(8); Reload(7); Reload(3);
    }
    void Reload(int offset)
    {
        if (offset is 8 or 11)
        {
            var source = Bank[8] <= 6 ? chorus.Dsp : phaser.Dsp;
            source.AsSpan(0x63C, 0x58).CopyTo(Dsp.AsSpan(0x63C));
            if (Bank[8] is 7 or 8 || Bank[11] == 0) Dsp[0x63C] = 0;
        }
        if (offset is 4 or 6 or 7 or 11) delay.Dsp.AsSpan(0x694, 0x4C).CopyTo(Dsp.AsSpan(0x694));
        if (offset is 3 or 10) { reverb.Activate(Bank[3]); reverb.Dsp.AsSpan(0x6E0, 0x84).CopyTo(Dsp.AsSpan(0x6E0)); }
    }
    public void Send(byte[] bytes)
    {
        var p = UsbSysEx.Decode(bytes); CubePacket response;
        if (p.Command == 0x11) response = new(0x11, Convert.FromHexString("53494E434F2D437562654261627920202020200113141516171819"));
        else
        {
            uint address = BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1));
            if (p.Command == 0x23)
            {
                int length = p.Payload[5] | p.Payload[6] << 8 | p.Payload[7] << 16;
                var data = p.Payload[0] == 0 ? new byte[length] : p.Payload[0] == 5 ? Bank.AsSpan((int)address, length).ToArray() : Dsp.AsSpan((int)address, length).ToArray();
                response = new(0x23, p.Payload.Concat(data).ToArray());
            }
            else if (p.Command == 0x22 && p.Payload[0] == 4)
            { DspWrites++; p.Payload.AsSpan(8).CopyTo(Dsp.AsSpan((int)address)); response = new(0, [0]); }
            else if (p.Command == 0x22 && p.Payload[0] == 5 && address >= 0x80000000)
            { int index = (int)(address - 0x80000000); Bank[index] = p.Payload[8]; if (index / 16 == State[0]) Reload(index % 16); response = new(0, [0]); }
            else throw new Exception("Unexpected mutation " + p.Command);
        }
        Received?.Invoke(UsbSysEx.Encode(response));
    }
    public void Dispose() { }
}
