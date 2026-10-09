using Cube.Protocol;
using Cube.Usb;
using System.Text.Json;

internal static class PhysicalInputsResearch
{
    public static async Task RunState(string[] args)
    {
        if (args.Length != 3 || !int.TryParse(args[2], out int seconds) || seconds is < 1 or > 600)
            throw new ArgumentException("Uso: observe-physical-state PASTA-NOVA SEGUNDOS (1–600)");
        if (System.Diagnostics.Process.GetProcessesByName("CubeControl").Length != 0 ||
            System.Diagnostics.Process.GetProcessesByName("CubeSuite").Length != 0)
            throw new InvalidOperationException("Feche CubeControl e CubeSuite antes da captura.");
        string folder = Path.GetFullPath(args[1]);
        if (Directory.Exists(folder)) throw new IOException("Use uma pasta nova para preservar as evidências.");
        var ports = CubeMidiPorts.Select(await WindowsMidiConnection.Inputs(), await WindowsMidiConnection.Outputs());
        if (ports.Input == null || ports.Output == null) throw new IOException("Par MIDI único não encontrado.");
        using var connection = await WindowsMidiConnection.Open(ports.Input.Id, ports.Output.Id);
        using var session = new CubeSession(connection);
        Directory.CreateDirectory(folder);
        using var log = new StreamWriter(Path.Combine(folder, "transport.log")) { AutoFlush = true };
        object sync = new();
        session.Log += line => { lock (sync) log.WriteLine($"{DateTimeOffset.UtcNow:O} {line}"); };
        await session.Identify();
        var first = await session.ReadPhaserResearch(); // Verifies firmware signatures and bank alias.
        var previous = first;
        int captures = 0, transitions = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine($"READY STATE {seconds}s firmware={session.EffectsFirmware} physical={Convert.ToHexString(first.PhysicalState)}");
        await File.WriteAllTextAsync(Path.Combine(folder, "initial.json"), JsonSerializer.Serialize(first));
        while (watch.Elapsed < TimeSpan.FromSeconds(seconds))
        {
            await Task.Delay(1000);
            PhaserResearchSnapshot current;
            try { current = await session.ReadPhaserResearch(); }
            catch (IOException ex) when (!session.IsFaulted && ex.Message is "Banco mudou durante a captura." or "Estado físico mudou durante a captura.")
            {
                Console.WriteLine("TRANSITION DURING SNAPSHOT; retry next sample");
                continue;
            }
            captures++;
            await File.WriteAllTextAsync(Path.Combine(folder, $"sample-{captures:D3}.json"), JsonSerializer.Serialize(new
            { elapsedSeconds = watch.Elapsed.TotalSeconds, snapshot = current }));
            var bankOffsets = Enumerable.Range(0, 48).Where(i => previous.Bank[i] != current.Bank[i]).ToArray();
            var dspOffsets = Enumerable.Range(0, current.Dsp.Length).Where(i => previous.Dsp[i] != current.Dsp[i]).ToArray();
            if (!previous.PhysicalState.SequenceEqual(current.PhysicalState)) transitions++;
            Console.WriteLine($"SAMPLE {captures} t={watch.Elapsed.TotalSeconds:F1}s physical={Convert.ToHexString(current.PhysicalState)} bankChanges={string.Join(',', bankOffsets)} dspChanges={dspOffsets.Length} dspHead={Convert.ToHexString(current.Dsp.AsSpan(0, 32))}");
            previous = current;
        }
        await File.WriteAllTextAsync(Path.Combine(folder, "summary.json"), JsonSerializer.Serialize(new
        { firmware = session.EffectsFirmware, captures, transitions, writes = 0,
            initialPhysical = Convert.ToHexString(first.PhysicalState), finalPhysical = Convert.ToHexString(previous.PhysicalState),
            bankUnchanged = first.Bank.SequenceEqual(previous.Bank) }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("COMPLETE STATE; nenhuma escrita enviada.");
    }

    // Identify once to verify the receive path, then send nothing during observation.
    public static async Task Run(string[] args)
    {
        if (args.Length != 3 || !int.TryParse(args[2], out int seconds) || seconds is < 1 or > 600)
            throw new ArgumentException("Uso: observe-physical-inputs PASTA-NOVA SEGUNDOS (1–600)");
        if (System.Diagnostics.Process.GetProcessesByName("CubeControl").Length != 0 ||
            System.Diagnostics.Process.GetProcessesByName("CubeSuite").Length != 0)
            throw new InvalidOperationException("Feche CubeControl e CubeSuite antes da captura.");
        string folder = Path.GetFullPath(args[1]);
        if (Directory.Exists(folder)) throw new IOException("Use uma pasta nova para preservar as evidências.");
        var ports = CubeMidiPorts.Select(await WindowsMidiConnection.Inputs(), await WindowsMidiConnection.Outputs());
        if (ports.Input == null || ports.Output == null)
            throw new InvalidOperationException("Nenhum par MIDI Cube Baby único encontrado.");
        using var connection = await WindowsMidiConnection.Open(ports.Input.Id, ports.Output.Id);
        using var session = new CubeSession(connection);
        Directory.CreateDirectory(folder);
        using var log = new StreamWriter(Path.Combine(folder, "transport.log")) { AutoFlush = true };
        object sync = new();
        bool observing = false;
        int messages = 0;
        void Record(string line) { lock (sync) log.WriteLine($"{DateTimeOffset.UtcNow:O} {line}"); }
        connection.Received += bytes =>
        {
            lock (sync)
            {
                if (observing) messages++;
                Record("RX RAW " + Convert.ToHexString(bytes));
            }
        };
        session.Log += Record;
        await session.Identify();
        var started = DateTimeOffset.UtcNow;
        lock (sync) { observing = true; Record("PASSIVE START: no requests during observation"); }
        Console.WriteLine($"READY PASSIVE {seconds}s; identidade={session.Identity}; nenhuma consulta durante a observação.");
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        int count;
        lock (sync) { observing = false; count = messages; Record($"PASSIVE END rawMessages={count}"); }
        await File.WriteAllTextAsync(Path.Combine(folder, "summary.json"), JsonSerializer.Serialize(new
        {
            started, finished = DateTimeOffset.UtcNow, seconds, identity = session.Identity,
            identityHex = session.IdentityHex, rawMessages = count, writes = 0,
            requestsDuringObservation = 0,
            limitation = "Physical actions and operating modes must be confirmed by the participant. Silence only applies to this observation window."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"COMPLETE PASSIVE: {count} mensagem(ns) bruta(s) recebida(s).");
    }
}
