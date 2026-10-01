using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cube.Protocol;
using Cube.Usb;

internal static class PhaserResearch
{
    public static async Task Run(string[] args)
    {
        if (args.Length != (args[0] is "phaser-sweep" or "phaser-controls" ? 3 : 2))
            throw new ArgumentException("Uso: phaser-snapshot|phaser-map PASTA-NOVA; phaser-sweep PASTA-NOVA SLOT-FISICO-0-2");
        if (System.Diagnostics.Process.GetProcessesByName("CubeControl").Length != 0 ||
            System.Diagnostics.Process.GetProcessesByName("CubeSuite").Length != 0)
            throw new InvalidOperationException("Feche CubeControl e CubeSuite antes da pesquisa.");
        string folder = Path.GetFullPath(args[1]);
        if (Directory.Exists(folder)) throw new IOException("Use uma pasta nova para preservar as evidências.");
        Directory.CreateDirectory(folder);
        using var log = new StreamWriter(Path.Combine(folder, "transport.log")) { AutoFlush = true };
        bool IsCube(string name) => Regex.IsMatch(name, @"^USB-Midi(?: \[\d+\])?$", RegexOptions.CultureInvariant);
        var inputs = (await WindowsMidiConnection.Inputs()).Where(p => IsCube(p.Name)).ToArray();
        var outputs = (await WindowsMidiConnection.Outputs()).Where(p => IsCube(p.Name)).ToArray();
        if (inputs.Length != 1 || outputs.Length != 1) throw new IOException("Portas Cube Baby não são únicas.");
        using var session = new CubeSession(await WindowsMidiConnection.Open(inputs[0].Id, outputs[0].Id));
        session.Log += line => log.WriteLine($"{DateTimeOffset.UtcNow:O} {line}");
        await session.Identify();
        if (args[0] is "phaser-sweep" or "phaser-controls")
        {
            await session.RunPhaserSweep(int.Parse(args[2]), async (label, snapshot) =>
            {
                string json = JsonSerializer.Serialize(new { identity = session.IdentityHex, snapshot }, new JsonSerializerOptions { WriteIndented = true });
                string path = Path.Combine(folder, label + ".json");
                await File.WriteAllTextAsync(path, json);
                if (await File.ReadAllTextAsync(path) != json) throw new IOException("Backup em disco divergiu.");
                await File.WriteAllBytesAsync(Path.Combine(folder, label + ".bin"), snapshot.Dsp);
                Console.WriteLine($"{label} {Convert.ToHexString(snapshot.Dsp.AsSpan(0x63C, 0x20))}");
            }, args[0] == "phaser-controls" ? async recovery =>
            {
                string json = JsonSerializer.Serialize(recovery);
                string path = Path.Combine(folder, "preview-recovery.json");
                await File.WriteAllTextAsync(path, json);
                if (await File.ReadAllTextAsync(path) != json) throw new IOException("Backup de prévia divergiu.");
            } : null);
            return;
        }
        if (args[0] == "phaser-map")
        {
            string mapping = JsonSerializer.Serialize(await session.InspectPhaserMapping(), new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(folder, "mapping.json"), mapping);
            await File.WriteAllBytesAsync(Path.Combine(folder, "code-before-dsp.bin"), await session.ReadPhaserCodeEvidence());
            Console.WriteLine(mapping);
            return;
        }
        var captures = new List<PhaserResearchSnapshot>();
        for (int i = 0; i < 3; i++)
        {
            var snapshot = await session.ReadPhaserResearch();
            captures.Add(snapshot);
            await File.WriteAllBytesAsync(Path.Combine(folder, $"dsp-{i}.bin"), snapshot.Dsp);
            await File.WriteAllBytesAsync(Path.Combine(folder, $"bank-{i}.bin"), snapshot.Bank);
            await Task.Delay(250);
        }
        var first = captures[0];
        var changes = Enumerable.Range(0, first.Dsp.Length)
            .Where(i => captures.Any(s => s.Dsp[i] != first.Dsp[i])).ToArray();
        var report = new
        {
            format = "cubecontrol-phaser-research-v1", capturedUtc = DateTimeOffset.UtcNow,
            identity = session.IdentityHex, input = inputs[0], output = outputs[0],
            physicalStates = captures.Select(s => Convert.ToHexString(s.PhysicalState)),
            bankUnchanged = captures.All(s => s.Bank.SequenceEqual(first.Bank)),
            dspSha256 = captures.Select(s => Convert.ToHexString(SHA256.HashData(s.Dsp))),
            dynamicOffsets = changes.Select(i => $"0x{i:X}"),
            modulationHex = captures.Select(s => Convert.ToHexString(s.Dsp.AsSpan(0x63C, 0x58)))
        };
        string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(folder, "report.json"), json);
        Console.WriteLine(json);
    }
}
