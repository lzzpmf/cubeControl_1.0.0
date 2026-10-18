using System.IO;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class ChainTests
{
    public static async Task Run()
    {
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
        var writes = new List<(ParameterKey Key, int Value)>();
        var preset = new PresetViewModel(2, (k, v) => writes.Add((k, v)));
        var bank = new byte[48]; bank[40] = 7; bank[43] = 1; preset.Load(bank);
        var musical = preset.Musical;
        Check(!musical.ChorusEnabled && !musical.PhaserEnabled && writes.Count == 0 && preset.Parameters[8].Value == 7, "legacy off 7 loads without canonicalization");
        musical.ChorusDial.Value = 6; musical.PhaserDial.Value = 2;
        Check(writes.Count == 0, "both inactive cards prepare amounts without MIDI");
        musical.ChorusEnabled = true;
        Check(writes.Count == 1 && writes[^1] == (new ParameterKey(2, 8), 1) && !musical.PhaserEnabled, "Chorus maps position 6 to raw 1 on original preset C");
        musical.PhaserDial.Value = 7;
        Check(writes.Count == 1 && musical.ChorusEnabled && musical.ChorusDial.Value == 6, "editing inactive Phaser preserves active Chorus sound and amount");
        musical.PhaserEnabled = true;
        Check(writes.Count == 2 && writes[^1].Value == 15 && !musical.ChorusEnabled, "Phaser activation replaces Chorus with one MIDI value");
        musical.ChorusEnabled = false;
        Check(writes.Count == 2 && musical.PhaserEnabled, "turning off an already inactive card cannot bypass its sibling");
        musical.PhaserEnabled = false;
        Check(writes.Count == 3 && writes[^1].Value == 8 && !musical.ChorusEnabled && !musical.PhaserEnabled, "shared bypass sends raw 8");
        musical.ChorusEnabled = true;
        Check(writes[^1].Value == 1 && musical.PhaserDial.Value == 7, "reactivation restores the independent prepared values");
        preset.Parameters[8].Load(11);
        Check(musical.PhaserEnabled && musical.PhaserDial.Value == 3 && !musical.ChorusEnabled, "device bank values update the correct card");
        preset.Parameters[11].Load(0); writes.Clear();
        Check(!musical.ChorusEnabled && !musical.PhaserEnabled, "bypassed hardware gate never reports modulation as audible");
        musical.PhaserDial.Value = 5;
        Check(writes.Count == 0, "adjusting modulation behind the bypassed gate stays local");
        musical.DelayEnabled = true;
        Check(preset.Parameters[8].Value == 8 && musical.DelayEnabled && !musical.PhaserEnabled, "enabling Delay does not revive bypassed modulation");

        string directory = Path.Combine(Path.GetTempPath(), "cube-chain-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            using var fake = new FakeConnection(); fake.Bank[8] = 8; fake.Bank[7] = 33;
            using var session = new CubeSession(fake); await session.Identify();
            using var model = new EditorViewModel(directory); await model.Execute(() => model.AttachSession(session));
            var window = new Cube.Desktop.MainWindow(model) { ShowInTaskbar = false };
            model.Ask = _ => Task.FromResult(true);
            window.Show();
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Check(model.Stages.Select(s => s.Title).SequenceEqual(new[] { "Amp", "Chorus", "Phaser", "Delay", "Reverb", "Cabinet" }), "six presentation cards retain device order");
            foreach (var stage in model.Stages) model.SelectedStage = stage.Index;
            Check(fake.Writes == 0, "navigating all six cards never edits MIDI");
            IEnumerable<T> Find<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                    if (child is T match) yield return match;
                    foreach (var nested in Find<T>(child)) yield return nested;
                }
            }
            var switches = Find<Wpf.Ui.Controls.ToggleSwitch>((System.Windows.DependencyObject)window.FindName("EffectChain")).ToArray();
            async Task Toggle(int index)
            {
                var peer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(switches[index])!;
                ((System.Windows.Automation.Provider.IToggleProvider)peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle)!).Toggle();
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                await model.Flush();
            }
            await Toggle(1);
            Check(fake.Bank[8] < 7 && model.Stages[1].IsEffectEnabled, "actual Chorus toggle activates the device modulation block");
            Check(fake.Bank[11] == 1, "Chorus also enables the shared hardware Delay/Mod gate");
            Check(fake.Bank[7] == 0 && !model.Stages[3].IsEffectEnabled, "enabling Chorus does not revive bypassed delay repetitions");
            model.Undo.Execute(null); while (model.Busy) await Task.Delay(10);
            Check(fake.Bank[8] == 8 && fake.Bank[7] == 33 && fake.Bank[11] == 0, "one undo restores modulation, mix and shared gate together");
            await Toggle(1);
            await Toggle(3);
            Check(fake.Bank[7] == 33 && fake.Bank[11] == 1 && model.Stages[1].IsEffectEnabled, "Delay restores its remembered mix alongside Chorus");
            await Toggle(3);
            Check(fake.Bank[7] == 0 && fake.Bank[11] == 1 && model.Stages[1].IsEffectEnabled, "disabling Delay leaves Chorus audible");
            await Toggle(2);
            Check(fake.Bank[8] > 8 && model.Stages[2].IsEffectEnabled, "actual Phaser toggle replaces Chorus on the device");
            await Toggle(2);
            Check(fake.Bank[8] == 8, "actual Phaser toggle bypasses the shared block");
            model.SelectedStage = 1; model.EditorDials[0].Value = 4;
            model.Stages[1].IsEffectEnabled = true; await model.Flush();
            Check(fake.Bank[8] == 3 && model.Stages[1].IsEffectEnabled && !model.Stages[2].IsEffectEnabled, "Chorus card switch uses original parameter offset 8");
            int count = fake.Writes;
            model.SelectedStage = 2; model.EditorDials[0].Value = 6; await model.Flush();
            Check(fake.Writes == count && fake.Bank[8] == 3 && model.FocusedGear.Kind == "phaser", "Phaser card opens and prepares its own editor without changing Chorus");
            model.Stages[2].IsEffectEnabled = true; await model.Flush();
            Check(fake.Bank[8] == 14 && !model.Stages[1].IsEffectEnabled, "Phaser card turns Chorus off using its original raw range");
            model.Undo.Execute(null); while (model.Busy) await Task.Delay(10);
            Check(fake.Bank[8] == 3 && model.Stages[1].IsEffectEnabled && !model.Stages[2].IsEffectEnabled, "undo restores the previous shared modulation state");
            model.Stages[0].IsEffectEnabled = true; model.Stages[3].IsEffectEnabled = true;
            model.Stages[4].IsEffectEnabled = true; model.Stages[5].IsEffectEnabled = true; await model.Flush();
            Check(fake.Bank[12] == 1 && fake.Bank[11] == 1 && fake.Bank[3] == 8 && fake.Bank[10] == 1, "Tone Delay Reverb and IR card switches preserve device offsets");
            model.SelectedIndex = 1;
            Check(!model.Stages[3].IsEffectEnabled && !model.Stages[5].IsEffectEnabled, "card states follow the preset being edited");
            Check(fake.FlashWrites == 0 && fake.Erases == 0, "presentation changes do not write IR flash");
            window.Close();
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) && Path.GetFileName(resolved).StartsWith("cube-chain-tests-")) Directory.Delete(resolved, true);
        }
        Console.WriteLine($"{passed} chain checks passed.");
    }
}
