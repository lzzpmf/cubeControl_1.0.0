using System.Text.Json;
using System.Text.RegularExpressions;
using Cube.Protocol;
using Cube.Usb;

internal static class ChorusResearch
{
    public static async Task Run(string[] args)
    {
        if (args.Length != 2 || args[0] is not ("chorus-map" or "chorus-sweep" or "chorus-controls" or "chorus-transitions"))
            throw new ArgumentException("chorus-map|chorus-sweep|chorus-controls|chorus-transitions PASTA-NOVA");
        if (System.Diagnostics.Process.GetProcessesByName("CubeControl").Length != 0 ||
            System.Diagnostics.Process.GetProcessesByName("CubeSuite").Length != 0)
            throw new InvalidOperationException("Feche CubeControl e CubeSuite antes da pesquisa.");
        string folder = Path.GetFullPath(args[1]);
        if (Directory.Exists(folder)) throw new IOException("Use uma pasta nova.");
        Directory.CreateDirectory(folder);
        using var log = new StreamWriter(Path.Combine(folder, "transport.log")) { AutoFlush = true };
        bool IsCube(string name) => Regex.IsMatch(name, @"^USB-Midi(?: \[\d+\])?$");
        var inputs = (await WindowsMidiConnection.Inputs()).Where(p => IsCube(p.Name)).ToArray();
        var outputs = (await WindowsMidiConnection.Outputs()).Where(p => IsCube(p.Name)).ToArray();
        if (inputs.Length != 1 || outputs.Length != 1) throw new IOException("Portas Cube Baby não são únicas.");
        using var session = new CubeSession(await WindowsMidiConnection.Open(inputs[0].Id, outputs[0].Id));
        session.Log += line => log.WriteLine($"{DateTimeOffset.UtcNow:O} {line}");
        await session.Identify();
        if (args[0] != "chorus-map")
        {
            int sequence = 0;
            async Task Save(string name, string json)
            {
                string path = Path.Combine(folder, name + ".json");
                await File.WriteAllTextAsync(path, json);
                if (await File.ReadAllTextAsync(path) != json) throw new IOException("Backup em disco divergiu.");
            }
            await session.RunChorusResearch(args[0] == "chorus-controls", async (label, snapshot) =>
            {
                await Save(label, JsonSerializer.Serialize(new { identity = session.IdentityHex, snapshot }, new JsonSerializerOptions { WriteIndented = true }));
                await File.WriteAllBytesAsync(Path.Combine(folder, label + ".bin"), snapshot.Dsp);
                Console.WriteLine(label);
            }, async (kind, address, before, after) =>
                await Save($"intent-{sequence++:D4}", JsonSerializer.Serialize(new { kind, address, before, after })),
                transitions: args[0] == "chorus-transitions");
            return;
        }
        var result = await session.ReadChorusMapping();
        foreach (var (name, bytes) in result) await File.WriteAllBytesAsync(Path.Combine(folder, name + ".bin"), bytes);
        var report = JsonSerializer.Serialize(new { identity = session.IdentityHex,
            mapping = result.ToDictionary(p => p.Key, p => Convert.ToHexString(p.Value)) }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(folder, "mapping.json"), report);
        Console.WriteLine(report);
    }
}
