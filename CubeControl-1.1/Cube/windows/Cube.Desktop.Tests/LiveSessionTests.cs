using System.IO;
using System.Text.Json;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class LiveSessionTests
{
    private static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }

    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-live-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            foreach (string effect in new[] { "chorus", "phaser", "delay", "reverb", "ir" })
                File.WriteAllText(Path.Combine(folder, $"pending-{effect}-preview.json"), "{invalid");
            File.WriteAllText(Path.Combine(folder, "backing-tracks-v1.json"), "{invalid");
            var legacy = new VisualPreferences { AutomaticCabinetMatching = true };
            legacy.Images["amp-0"] = "custom.png";
            legacy.IrNames["old"] = "Previous IR";
            legacy.Matches[0] = "old";
            legacy.Save(Path.Combine(folder, "visual-preferences-v1.json"));
            using var device = new MultiEffectConnection();
            using var transport = new EffectsFirmwareBConnection(device, device.Bank, device.State);
            using var session = new CubeSession(transport);
            await session.Identify();
            string validLegacyRecovery = "";
            using (var model = new EditorViewModel(folder))
            {
                await model.Initialize();
                Check(model.ErrorMessage == "" && !model.HasBank && !model.Ir.PreviewActive && !model.Chorus.Loaded,
                    "legacy recovery does not populate or block a new session");
                Check(!model.AutomaticCabinetMatching, "matching starts at its session default");
                var images = GearImagePreferences.Load(Path.Combine(folder, "gear-images-v1.json"), "unused");
                Check(images["amp-0"] == "custom.png", "legacy migration retains only artwork associations");
                var before = Directory.GetFiles(folder).ToDictionary(p => p, File.ReadAllText);
                await model.AttachSession(session);
                Check(model.CanEdit && model.ErrorMessage == "", "corrupt previous recovery does not prevent live editing");
                await model.ReadDelayAsync();
                Check(model.DelayPreviewActive, "preview starts with an in-memory snapshot");
                var recovery = typeof(EditorViewModel).GetField("delayRecovery",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(model);
                validLegacyRecovery = JsonSerializer.Serialize(recovery);
                await model.EndDelayPreviewAsync();
                Check(!model.DelayPreviewActive, "preview restores from memory");
                Check(Directory.GetFiles(folder).Length == before.Count && before.All(p => File.ReadAllText(p.Key) == p.Value),
                    "connect, read, preview and restore leave all persisted files unchanged");
            }
            File.WriteAllText(Path.Combine(folder, "pending-delay-preview.json"), validLegacyRecovery);
            File.WriteAllText(Path.Combine(folder, "pending-ir-preview.json"), "{}");
            using (var fresh = new EditorViewModel(folder))
            {
                await fresh.Initialize();
                Check(!fresh.HasBank && !fresh.Delay.Loaded && !fresh.DelayPreviewActive && !fresh.AutomaticCabinetMatching,
                    "reopening has no bank, draft, preview or matching state");
                Check(fresh.ErrorMessage == "" && !fresh.Ir.PreviewActive,
                    "valid effect recovery and incomplete IR recovery are both ignored");
                Check(File.ReadAllText(Path.Combine(folder, "pending-delay-preview.json")) == validLegacyRecovery,
                    "legacy recovery is not deleted or rewritten");
            }
            using (var tracks = new BackingTracksViewModel(folder))
                Check(tracks.Tracks.Count == 0 && tracks.Volume == 30 && !tracks.IsMuted,
                    "backing tracks ignore previous playlist and preferences");
            string export = Path.Combine(folder, "explicit.json");
            byte[] data = new byte[4096]; data[100] = 42;
            await IrBackup.Export(export, "SINCO-CubeBaby", "test", 2, data);
            var loaded = await IrBackup.Load(export);
            Check(loaded.Info.Slot == 2 && loaded.Data.SequenceEqual(data), "explicit IR export preserves import compatibility");
        }
        finally { Directory.Delete(folder, true); }
    }
}
