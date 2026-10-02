using System.IO;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class EffectsFirmwareBDesktopTests
{
    public static async Task Run()
    {
        int passed = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception(name);
            passed++; Console.WriteLine("PASS " + name);
        }
        async Task Creator(string name, IMidiConnection inner, byte[] bank, byte[] state,
            Func<EditorViewModel, Task> read, Func<EditorViewModel, bool> active,
            Action<EditorViewModel> edit, Func<bool> applied, Func<EditorViewModel, Task> restore)
        {
            using var transport = new EffectsFirmwareBConnection(inner, bank, state);
            var session = new CubeSession(transport); await session.Identify();
            using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-B-" + Guid.NewGuid().ToString("N")));
            await model.AttachSession(session);
            await read(model);
            Check(active(model) && model.ErrorMessage == "" && session.EffectsFirmware == "cubebaby_B_20220706",
                $"B {name} opens creator without signature error");
            edit(model);
            for (int i = 0; i < 300 && (!applied() || model.Busy); i++) await Task.Delay(10);
            Check(applied() && model.ErrorMessage == "", $"B {name} sends UI edits and confirms RAM");
            await restore(model);
            Check(!active(model) && model.CanEdit && model.ErrorMessage == "", $"B {name} restore re-enables normal editor");
            await model.DisconnectAsync();
        }
        using var phaser = new PhaserConnection();
        await Creator("phaser", phaser, phaser.Bank, phaser.State,
            m => m.ReadPhaserAsync(), m => m.PhaserPreviewActive, m => m.Phaser.Lower = 1000,
            () => PhaserParameters.Read(phaser.Dsp).Lower == 1000, m => m.EndPhaserPreviewAsync());
        using var chorus = new ChorusPreviewConnection();
        await Creator("chorus", chorus, chorus.Bank, chorus.State,
            m => m.ReadChorusAsync(), m => m.ChorusPreviewActive, m => m.Chorus.Depth = 50,
            () => ChorusParameters.Read(chorus.Dsp).Depth == 0.2f, m => m.EndChorusPreviewAsync());
        using var delay = new DelayConnection(); delay.Activate();
        await Creator("delay", delay, delay.Bank, delay.State,
            m => m.ReadDelayAsync(), m => m.DelayPreviewActive, m => m.Delay.TimeSamples = 6000,
            () => DelayParameters.Read(delay.Dsp).TimeSamples == 6000, m => m.EndDelayPreviewAsync());
        using var reverb = new ReverbConnection(); reverb.Activate();
        await Creator("reverb", reverb, reverb.Bank, reverb.State,
            m => m.ReadReverbAsync(), m => m.ReverbPreviewActive, m => m.Reverb.Wet = 25,
            () => ReverbParameters.Read(reverb.Dsp).Wet == 0.25f, m => m.EndReverbPreviewAsync());
        Console.WriteLine($"{passed} firmware B desktop checks passed.");
    }
}
