using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Cube.Usb;

internal static class AdvancedHardwareTests
{
    public static async Task Run(bool recoveryOnly = false, bool cardFlow = false)
    {
        if (Process.GetProcessesByName("CubeControl").Length != 0 || Process.GetProcessesByName("CubeSuite").Length != 0)
            throw new InvalidOperationException("Close other pedal editors before this explicit hardware test.");
        var input = (await WindowsMidiConnection.Inputs()).Single(p => p.Name == "USB-Midi");
        var output = (await WindowsMidiConnection.Outputs()).Single(p => p.Name == "USB-Midi");
        string folder = Path.Combine("C:/CubeControl-Build/advanced-lifecycle", "hardware-edit-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(folder);
        using var connection = await WindowsMidiConnection.Open(input.Id, output.Id);
        using var session = new CubeSession(connection); await session.Identify();
        using var log = new StreamWriter(Path.Combine(folder, "transport.log")) { AutoFlush = true };
        session.Log += line => log.WriteLine(line);
        var initial = await session.ReadEffectPreviewState(1);
        await File.WriteAllTextAsync(Path.Combine(folder, "initial.json"), JsonSerializer.Serialize(initial));
        using var model = new EditorViewModel(folder); model.Ask = _ => Task.FromResult(true);
        await model.AttachSession(session);
        if (recoveryOnly)
        {
            string pending = Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "CubeControl", "pending-chorus-preview.json");
            File.Copy(pending, Path.Combine(folder, "pending-chorus-preview.json"));
            await (Task)typeof(EditorViewModel).GetMethod("LoadPendingChorus", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(model, null)!;
        }
        async Task Settle()
        {
            await Task.Delay(150);
            var timeout = Stopwatch.StartNew();
            while (model.Busy && timeout.Elapsed < TimeSpan.FromMinutes(2)) await Task.Delay(50);
            if (model.Busy || model.ErrorMessage.Length > 0) throw new IOException(model.ErrorMessage + " Busy=" + model.Busy);
        }
        object Parameters(int stage, byte[] dsp) => stage switch
        {
            1 => ChorusParameters.Read(dsp), 2 => PhaserParameters.Read(dsp),
            3 => DelayParameters.Read(dsp), _ => ReverbParameters.Read(dsp)
        };
        object Draft(int stage) => stage switch
        {
            1 => model.Chorus.Parameters, 2 => model.Phaser.Parameters,
            3 => model.Delay.Parameters, _ => model.Reverb.Parameters
        };
        async Task CheckCard(int stage, int offset)
        {
            if (!cardFlow) return;
            model.SelectedStage = stage;
            await model.ToggleEffectCardAsync();
            var before = Parameters(stage, (await session.ReadEffectPreviewState(stage)).Dsp);
            await model.ToggleEffectCardAsync();
            if (!model.IsSimpleCard || !model.HasAdvancedPreview(stage) ||
                !before.Equals(Parameters(stage, (await session.ReadEffectPreviewState(stage)).Dsp)))
                throw new Exception("Flipping card changed RAM");
            int old = (int)model.Current.Parameters[offset].Value;
            int limit = stage == 1 ? 6 : stage == 2 ? 15 : stage == 3 ? 31 : 15;
            int next = old < limit ? old + 1 : old - 1;
            model.Current.Parameters[offset].Value = next;
            await Settle();
            var simple = await session.ReadEffectPreviewState(stage);
            if (simple.Bank[simple.PhysicalState[0] * 16 + offset] != next || !Parameters(stage, simple.Dsp).Equals(Draft(stage)))
                throw new Exception("Simple controls did not synchronize advanced state");
            await model.ToggleEffectCardAsync();
            if (!Parameters(stage, simple.Dsp).Equals(Parameters(stage, (await session.ReadEffectPreviewState(stage)).Dsp)))
                throw new Exception("Returning to advanced reapplied an old draft");
            Console.WriteLine("PASS HARDWARE card " + stage + ": flip retains RAM; simple edit wins; advanced reflects new state");
        }
        try
        {
            await model.ReadChorusAsync();
            if (!model.CanEditChorus) throw new Exception("Hardware chorus is not editable: " + model.ErrorMessage);
            if (recoveryOnly)
            {
                if (!Directory.Exists(Path.Combine(folder, "ArchivedEffectRecovery"))) throw new Exception("Obsolete pending recovery was not archived");
                Console.WriteLine("PASS HARDWARE exact persisted chorus recovery: archived, fresh preview editable, original user backup untouched");
                return;
            }
            model.Chorus.Depth = 50; model.Chorus.Speed = 1.2; model.Chorus.Mix = 60;
            model.Chorus.Feedback = 1; model.Chorus.Waveform = 1; model.Chorus.WetGain = 80; model.Chorus.CombinedGain = 80;
            await Settle();
            var chorus = model.Chorus.Parameters;
            if (ChorusParameters.Read((await session.ReadEffectPreviewState(1)).Dsp) != chorus) throw new Exception("Chorus readback differs");
            Console.WriteLine("PASS HARDWARE chorus: 7 controls and RAM readback");
            await CheckCard(1, 8); chorus = model.Chorus.Parameters;
            await model.ReadDelayAsync();
            if (!model.CanEditDelay) throw new Exception("Hardware delay is not editable: " + model.ErrorMessage);
            model.Delay.TimeSamples = 6000; model.Delay.Feedback = 20; model.Delay.Depth = 50; model.Delay.Rate = 50; model.Delay.Mix = 20;
            await Settle();
            if (DelayParameters.Read((await session.ReadEffectPreviewState(3)).Dsp) != model.Delay.Parameters) throw new Exception("Delay readback differs");
            Console.WriteLine("PASS HARDWARE delay: 5 controls alongside chorus");
            await CheckCard(3, 6);
            await model.ReadReverbAsync();
            if (!model.CanEditReverb) throw new Exception("Hardware reverb is not editable: " + model.ErrorMessage);
            model.Reverb.Dry = 90; model.Reverb.Wet = 25; model.Reverb.ReturnLevel = 75; model.Reverb.Decay = 7; model.Reverb.Damping = 7;
            await Settle();
            if (ReverbParameters.Read((await session.ReadEffectPreviewState(4)).Dsp) != model.Reverb.Parameters) throw new Exception("Reverb readback differs");
            Console.WriteLine("PASS HARDWARE reverb: 5 controls alongside chorus/delay");
            await CheckCard(4, 3);
            await model.ReadPhaserAsync();
            if (!model.CanEditPhaser) throw new Exception("Hardware phaser is not editable: " + model.ErrorMessage);
            model.Phaser.Lower = 500; model.Phaser.Upper = 2500; model.Phaser.Speed = 1.2; model.Phaser.Level = 75; model.Phaser.Sections = 2;
            await Settle();
            if (PhaserParameters.Read((await session.ReadEffectPreviewState(2)).Dsp) != model.Phaser.Parameters) throw new Exception("Phaser readback differs");
            Console.WriteLine("PASS HARDWARE phaser: 5 controls, four previews open");
            await CheckCard(2, 8);
            await model.ListenAdvancedModulationAsync(1); await Settle();
            if (ChorusParameters.Read((await session.ReadEffectPreviewState(1)).Dsp) != chorus) throw new Exception("Chorus draft not preserved");
            await Task.Delay(2500); await Settle();
            if (!model.CanEditChorus || !model.CanEditPhaser || !model.CanEditDelay || !model.CanEditReverb) throw new Exception("A preview stopped while idle");
            Console.WriteLine("PASS HARDWARE switch back and idle monitoring");
        }
        finally
        {
            bool restored = true;
            foreach (int stage in new[] { 2, 1, 3, 4 }) restored &= await model.CloseAdvancedPreviewAsync(stage);
            if (!restored) throw new IOException("Hardware restore incomplete. Recovery preserved in " + folder);
            var current = await session.ReadBank();
            for (int i = 0; i < current.Length; i++)
                if (current[i] != initial.Bank[i]) current = await session.WriteLiveChecked(i / 16, i % 16, initial.Bank[i], current);
            if (!(await session.ReadBank()).SequenceEqual(initial.Bank)) throw new IOException("Initial bank was not restored");
            await File.WriteAllTextAsync(Path.Combine(folder, "restored.txt"), "Initial live bank restored and verified; no flash save commands issued.");
            Console.WriteLine("PASS HARDWARE previews closed; initial bank restored and verified. Evidence: " + folder);
        }
    }
}
