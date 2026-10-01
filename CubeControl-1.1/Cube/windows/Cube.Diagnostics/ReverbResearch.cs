using Cube.Protocol;
using Cube.Usb;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class ReverbResearch
{
    public static async Task Run(string[] args)
    {
        if (args.Length != 2 || args[0] is not ("reverb-map" or "reverb-sweep" or "reverb-controls" or "reverb-transitions"))
            throw new ArgumentException("reverb-map|reverb-sweep|reverb-controls|reverb-transitions PASTA-NOVA");
        if (System.Diagnostics.Process.GetProcessesByName("CubeControl").Length != 0 ||
            System.Diagnostics.Process.GetProcessesByName("CubeSuite").Length != 0)
            throw new IOException("Feche CubeControl e CubeSuite antes da pesquisa.");
        string folder = Path.GetFullPath(args[1]);
        if (Directory.Exists(folder)) throw new IOException("Use uma pasta nova.");
        Directory.CreateDirectory(folder);
        using var log = new StreamWriter(Path.Combine(folder, "transport.log")) { AutoFlush = true };
        bool Match(string name) => Regex.IsMatch(name, @"^USB-Midi(?: \[\d+\])?$");
        var inputs = (await WindowsMidiConnection.Inputs()).Where(p => Match(p.Name)).ToArray();
        var outputs = (await WindowsMidiConnection.Outputs()).Where(p => Match(p.Name)).ToArray();
        if (inputs.Length != 1 || outputs.Length != 1) throw new IOException("Portas Cube Baby não são únicas.");
        using var session = new CubeSession(await WindowsMidiConnection.Open(inputs[0].Id, outputs[0].Id));
        session.Log += line => log.WriteLine($"{DateTimeOffset.UtcNow:O} {line}");
        await session.Identify();
        if (args[0] != "reverb-map")
        {
            int sequence = 0;
            async Task Save(string name, object data)
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                string path = Path.Combine(folder, name + ".json");
                await File.WriteAllTextAsync(path, json);
                if (await File.ReadAllTextAsync(path) != json) throw new IOException("Evidência em disco divergiu.");
            }
            await session.RunReverbResearch(args[0], async (label, snapshot) =>
            {
                await Save(label, new { identity = session.IdentityHex, snapshot });
                await File.WriteAllBytesAsync(Path.Combine(folder, label + ".bin"), snapshot.Dsp);
                Console.WriteLine(label);
            }, async (kind, region, address, before, after) =>
                await Save($"intent-{sequence++:D4}", new { kind, region, address, before, after }));
            return;
        }
        var map = await session.ReadReverbMapping();
        foreach (var (name, data) in map) await File.WriteAllBytesAsync(Path.Combine(folder, name + ".bin"), data);
        await File.WriteAllTextAsync(Path.Combine(folder, "mapping.json"), JsonSerializer.Serialize(new {
            identity = session.IdentityHex, mapping = map.ToDictionary(p => p.Key, p => Convert.ToHexString(p.Value))
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Mapa de reverb preservado: " + folder);
    }
}
