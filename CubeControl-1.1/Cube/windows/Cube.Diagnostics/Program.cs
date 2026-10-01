using Cube.Protocol;
using Cube.Usb;
using System.Text.Json;

var inputs = await WindowsMidiConnection.Inputs();
var outputs = await WindowsMidiConnection.Outputs();
Console.WriteLine(JsonSerializer.Serialize(new { inputs, outputs }, new JsonSerializerOptions { WriteIndented = true }));
if (args.Length > 0 && args[0] is "observe-state" or "snapshot-state" or "firmware-state")
{
    if (args[0] == "firmware-state" && (System.Diagnostics.Process.GetProcessesByName("CubeControl").Length != 0 ||
        System.Diagnostics.Process.GetProcessesByName("CubeSuite").Length != 0))
        throw new InvalidOperationException("Close CubeControl and CubeSuite before the read-only research capture.");
    using var connection = await WindowsMidiConnection.Open(inputs.Single(p => p.Name == "USB-Midi").Id, outputs.Single(p => p.Name == "USB-Midi").Id);
    string path = args.Length > 1 ? args[1] : "windows/artifacts/physical-state.log";
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
    using var log = new StreamWriter(path) { AutoFlush = true };
    var assembler = new SysExAssembler();
    TaskCompletionSource<CubePacket>? pending = null;
    CubePacket? expected = null;
    object sync = new();
    void Record(string line) { lock (sync) { line = $"{DateTimeOffset.Now:O} {line}"; log.WriteLine(line); Console.WriteLine(line); } }
    connection.Received += data =>
    {
        lock (sync)
        {
            foreach (var bytes in assembler.Feed(data))
            {
                var packet = UsbSysEx.Decode(bytes);
                if (expected != null && packet.Command == expected.Command &&
                    (expected.Command != 0x23 || packet.Payload.AsSpan().StartsWith(expected.Payload)))
                    pending?.TrySetResult(packet);
            }
        }
    };
    async Task<CubePacket> Probe(CubePacket request)
    {
        var reply = new TaskCompletionSource<CubePacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (sync) { pending = reply; expected = request; }
        connection.Send(UsbSysEx.Encode(request));
        var result = await reply.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Record($"STATE CMD={result.Command:X2} DATA={Convert.ToHexString(result.Payload)}");
        return result;
    }
    var researchIdentity = await Probe(CubePacket.Identify());
    if (args[0] == "firmware-state" && !System.Text.Encoding.ASCII.GetString(researchIdentity.Payload).StartsWith("SINCO-CubeBaby", StringComparison.Ordinal))
        throw new InvalidOperationException("Unexpected device identity; research capture stopped without writes.");
    await Probe(new CubePacket(0x12, []));
    if (args[0] == "firmware-state")
    {
        // Research-only addresses derived from cubebaby_A_20220706.up, never written.
        // Verify the relocated reader code and bank alias before interpreting state.
        var code = await Probe(CubePacket.Read(4, 0xFFFFDDEC, 12));
        if (!code.Payload.AsSpan(8).SequenceEqual(Convert.FromHexString("014B194436F4B6B3B8CB8000")))
            throw new InvalidOperationException("Firmware signature differs; state capture stopped without writes.");
        var researchBank = await Probe(CubePacket.Read(5, 0, 48));
        var alias = await Probe(CubePacket.Read(4, 0xFFFD8558, 48));
        if (!researchBank.Payload.AsSpan(8).SequenceEqual(alias.Payload.AsSpan(8)))
            throw new InvalidOperationException("RAM bank alias differs; state capture stopped without writes.");
        Record("FIRMWARE READER SIGNATURE AND BANK ALIAS VERIFIED; read-only sampling ready.");
        int seconds = args.Length > 2 ? int.Parse(args[2]) : 180;
        if (seconds is < 1 or > 600) throw new ArgumentOutOfRangeException(nameof(seconds), "Use 1 to 600 seconds.");
        for (int i = 0; i < seconds * 4; i++)
        {
            var state = await Probe(CubePacket.Read(4, 0xFFFD84B2, 2));
            if (state.Payload.Length != 10) throw new InvalidDataException("Unexpected state response length.");
            Record($"FOOTSWITCH SLOT={state.Payload[8]:X2} MODE={state.Payload[9]:X2}");
            await Task.Delay(250);
        }
        var finalBank = await Probe(CubePacket.Read(5, 0, 48));
        Record($"CAPTURE COMPLETE; BANK UNCHANGED={researchBank.Payload.AsSpan(8).SequenceEqual(finalBank.Payload.AsSpan(8))}");
        return;
    }
    if (args[0] == "snapshot-state")
    {
        for (uint address = 0; address < 0x764; address += 128)
            await Probe(CubePacket.Read(4, address, Math.Min(128, 0x764 - (int)address)));
        Record("SNAPSHOT COMPLETE");
        return;
    }
    Record("READY: read-only device state sampling; press A, B, C with 5 seconds between switches.");
    for (int i = 0; i < 120; i++)
    {
        await Probe(CubePacket.Read(4, 0, 128));
        await Task.Delay(1000);
    }
    Record("CAPTURE COMPLETE");
    return;
}
if (args.Length > 0 && args[0] == "observe-switches")
{
    var input = inputs.Single(p => p.Name == "USB-Midi");
    var output = outputs.Single(p => p.Name == "USB-Midi");
    var connection = await WindowsMidiConnection.Open(input.Id, output.Id);
    string path = args.Length > 1 ? args[1] : "windows/artifacts/physical-switches.log";
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
    using var log = new StreamWriter(path) { AutoFlush = true };
    object sync = new();
    void Record(string value)
    {
        lock (sync) { string line = $"{DateTimeOffset.Now:O} {value}"; log.WriteLine(line); Console.WriteLine(line); }
    }
    connection.Received += data => Record("RX RAW " + Convert.ToHexString(data));
    using var observer = new CubeSession(connection);
    Record("IDENTITY " + await observer.Identify());
    byte[] previous = await observer.ReadBank();
    Record("INITIAL BANK " + Convert.ToHexString(previous));
    Record("READY: press physical A, B, C in PRESET mode; read-only capture, no writes.");
    int seconds = args.Length > 2 ? int.Parse(args[2]) : 180;
    for (int i = 0; i < seconds; i++)
    {
        await Task.Delay(1000);
        var current = await observer.ReadBank();
        if (!previous.SequenceEqual(current)) Record("BANK CHANGED " + Convert.ToHexString(current));
        previous = current;
    }
    Record("CAPTURE COMPLETE");
    return;
}
if (args.Length == 0 || args[0] is not ("read" or "verify-live" or "verify-live-queue" or "verify-musical" or "ir-inspect" or "verify-ir" or "verify-factory")) return;
// Deliberate exact-name matching; never send to a guessed output index.
var candidateIn = inputs.Where(p => p.Name == "USB-Midi").ToArray();
var candidateOut = outputs.Where(p => p.Name == "USB-Midi").ToArray();
if (candidateIn.Length != 1 || candidateOut.Length != 1) throw new InvalidOperationException("Seleção USB-Midi não é única.");
using var session = new CubeSession(await WindowsMidiConnection.Open(candidateIn[0].Id, candidateOut[0].Id));
session.Log += line => { if (args[0] is "read" or "verify-live") Console.WriteLine(line); };
string identity = await session.Identify();
byte[] bank = await session.ReadBank();
Console.WriteLine($"Identidade: {identity}\nBanco: {Convert.ToHexString(bank)}");
CubeBabyProfile.ValidateBank(bank);
if (args[0] == "verify-musical")
{
    string folder = args.Length > 1 ? args[1] : "windows/artifacts/hardware-musical";
    Directory.CreateDirectory(folder);
    await File.WriteAllTextAsync(Path.Combine(folder, "bank-before.json"), JsonSerializer.Serialize(new { format = "cubecontrol-bank-v1", device = "SINCO-CubeBaby", bankHex = Convert.ToHexString(bank) }));
    var ram = await session.ReadIrRam(); await IrRamBackup.Save(Path.Combine(folder, "ram-before.json"), session.IdentityHex, ram);
    var sectors = await session.ReadIrInventory();
    var inventory = new CabinetInventory();
    inventory.SetFactory(FactoryResources.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CubeSuite", "CubeSuite.exe")));
    for (int slot = 0; slot < 8; slot++) inventory.Update(slot, sectors[slot]);
    var cabinet = inventory.Resolve("factory-cab-2") ?? inventory.Slots[0];
    var expected = bank.ToArray();
    async Task Set(int offset, int value) { expected = await session.WriteLiveChecked(0, offset, value, expected); await Task.Delay(100); }
    try
    {
        var modulation = new ModulationState(); modulation.Load(bank[8]); modulation.SetEnabled(false); modulation.Select(ModulationKind.Chorus);
        await Set(8, modulation.SetEnabled(true)); await Set(8, modulation.Adjust(7)!.Value);
        await Set(8, modulation.Select(ModulationKind.Phaser)!.Value); await Set(8, modulation.SetEnabled(false));
        var reverb = new ReverbState(); reverb.Load(bank[3]); await Set(3, reverb.SetEnabled(false)); reverb.Adjust(10); await Set(3, reverb.SetEnabled(true));
        if (CabinetInventory.Hash(await session.ReadIrSlot(cabinet.Slot)) != cabinet.Hash) throw new IOException("IR changed before match.");
        await Set(0, 1); await Set(9, cabinet.Slot + 1);
        if (expected[10] != bank[10]) throw new IOException("Match changed section activation.");
        Console.WriteLine("SEMANTIC MODULATION, REVERB AND CONTENT-VERIFIED CAB MATCH PASSED");
    }
    finally
    {
        if (session.IsFaulted) throw new IOException("Reconexão necessária para recuperação. Captura inicial: " + folder);
        expected = await session.ReadBank();
        foreach (var p in CubeBabyProfile.Parameters) if (expected[p.Offset] != bank[p.Offset]) await Set(p.Offset, bank[p.Offset]);
        await session.ApplyIrRam(ram);
        if (!(await session.ReadBank()).SequenceEqual(bank)) throw new IOException("Original bank restoration not confirmed.");
        Console.WriteLine("INITIAL BANK AND IR RAM RESTORED EXACTLY; NO FLASH SECTOR WRITTEN");
    }
    await File.WriteAllTextAsync(Path.Combine(folder, "validation.json"), JsonSerializer.Serialize(new { testedAt = DateTimeOffset.Now, identity, semanticEditsVerified = true, cabinetMatchVerified = true, originalStateRestored = true, irFlashWrites = 0 }, new JsonSerializerOptions { WriteIndented = true }));
    return;
}
if (args[0] == "verify-live-queue")
{
    string folder = args.Length > 1 ? args[1] : "windows/artifacts/hardware-live-queue";
    Directory.CreateDirectory(folder); await File.WriteAllBytesAsync(Path.Combine(folder, "bank-before.bin"), bank);
    byte[] confirmed = bank.ToArray(), target = bank.ToArray();
    int transmissions = 0;
    var queue = new LiveEditQueue(async (key, value) =>
    { confirmed = await session.WriteLiveChecked(key.Slot, key.Offset, value, confirmed); transmissions++; });
    try
    {
        for (int i = 0; i < 40; i++)
        {
            int slot = i < 20 ? 0 : 2; int value = (bank[slot * 16 + 5] + i + 1) % 128;
            target[slot * 16 + 5] = (byte)value; queue.Enqueue(new(slot, 5), value);
            await Task.Delay(8);
        }
        // This flush starts while the final value is still pending.
        await queue.Flush();
        if (!confirmed.SequenceEqual(target)) throw new IOException("Últimos valores não confirmados.");
        await session.SaveBank(confirmed, target);
        Console.WriteLine($"LIVE QUEUE, PRESET SWITCH AND SAVE VERIFIED ({transmissions} writes / 40 changes)");
    }
    finally
    {
        queue.Stop(); try { await queue.Flush(); } catch (IOException) { }
        if (session.IsFaulted) throw new IOException("Reconecte para restaurar o banco; backup: " + folder);
        var current = await session.ReadBank();
        for (int s = 0; s < 3; s++) foreach (var p in CubeBabyProfile.Parameters)
        {
            int index = s * 16 + p.Offset;
            if (current[index] != bank[index]) current = await session.WriteLiveChecked(s, p.Offset, bank[index], current);
        }
        await session.SaveBank(current, bank);
        Console.WriteLine("INITIAL BANK RESTORED AND VERIFIED");
    }
    await File.WriteAllTextAsync(Path.Combine(folder, "validation.json"), JsonSerializer.Serialize(new
    { testedAt = DateTimeOffset.Now, identity, queuedChanges = 40, transmissions, saveVerified = true, originalStateRestored = true }));
    return;
}
if (args[0] is "ir-inspect" or "verify-ir" or "verify-factory")
{
    string folder = args.Length > 1 ? args[1] : "windows/artifacts/hardware";
    Directory.CreateDirectory(folder);
    await File.WriteAllBytesAsync(Path.Combine(folder, "bank-before.bin"), bank);
    var ram = await session.ReadIrRam();
    await File.WriteAllBytesAsync(Path.Combine(folder, "ram-before.bin"), ram.Payload);
    await File.WriteAllTextAsync(Path.Combine(folder, "ram-flag.txt"), ram.Flag.ToString());
    var ir = await session.ReadIrSlot(7);
    string backup = await IrBackup.Save(folder, session.Identity, session.IdentityHex, 7, ir);
    Console.WriteLine($"IR READ VERIFIED: RAM flag={ram.Flag}, bytes={ram.Payload.Length}; slot8={ir.Length}; backup={backup}");
    if (args[0] == "ir-inspect") return;
    if (args[0] == "verify-ir")
    {
        var sample = IrAudio.Decode(await File.ReadAllBytesAsync(".tools/cubecontrol/test/fixtures/ir/033-tonehub-impulse.wav"));
        try
        {
            await session.ApplyIrRam(new(1, sample.LivePayload(50)));
            Console.WriteLine("IR PREVIEW VERIFIED");
            await session.ReplaceIr(7, ir, sample.Sector(50), async bytes => { await IrBackup.Save(folder, session.Identity, session.IdentityHex, 7, bytes); });
            Console.WriteLine("IR SLOT8 WRITE VERIFIED");
        }
        finally
        {
            // If a protocol timeout invalidated the session, leave the on-disk backup and fail explicitly.
            if (session.IsFaulted) throw new IOException("Conexão perdida. Recuperação exige reconectar; backups: " + folder);
            var now = await session.ReadIrSlot(7);
            if (!now.SequenceEqual(ir)) await session.ReplaceIr(7, now, ir, async bytes => { await IrBackup.Save(folder, session.Identity, session.IdentityHex, 7, bytes); });
            await session.ApplyIrRam(ram);
            Console.WriteLine("ORIGINAL IR SLOT8 AND RAM RESTORED EXACTLY");
        }
    }
    if (args[0] == "verify-factory")
    {
        var resources = FactoryResources.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CubeSuite", "CubeSuite.exe"));
        var originals = new byte[8][];
        for (int slot = 0; slot < 8; slot++)
        {
            originals[slot] = await session.ReadIrSlot(slot);
            await IrBackup.Save(folder, session.Identity, session.IdentityHex, slot, originals[slot]);
            Console.WriteLine($"FACTORY TEST BACKUP {slot + 1}/8 COMPLETE");
        }
        async Task ApplyBank(CubeSession active, byte[] target)
        {
            var actual = await active.ReadBank();
            for (int s = 0; s < 3; s++) foreach (var parameter in CubeBabyProfile.Parameters)
            {
                int index = s * 16 + parameter.Offset;
                if (actual[index] != target[index]) actual = await active.WriteLiveChecked(s, parameter.Offset, target[index], actual);
            }
            await active.SaveBank(actual, target);
        }
        try
        {
            await ApplyBank(session, resources.Presets);
            Console.WriteLine("FACTORY PRESETS WRITTEN AND VERIFIED");
            for (int slot = 0; slot < 8; slot++)
            {
                int targetSlot = slot;
                await session.ReplaceIr(slot, originals[slot], resources.IrSectors[slot], async bytes => { await IrBackup.Save(folder, session.Identity, session.IdentityHex, targetSlot, bytes); });
                Console.WriteLine($"FACTORY IR {slot + 1}/8 WRITTEN AND VERIFIED");
            }
        }
        finally
        {
            CubeSession active = session; CubeSession? recovery = null;
            if (session.IsFaulted)
            {
                session.Dispose(); recovery = new(await WindowsMidiConnection.Open(candidateIn[0].Id, candidateOut[0].Id));
                await recovery.Identify(); active = recovery;
            }
            try
            {
                for (int slot = 0; slot < 8; slot++)
                {
                    int targetSlot = slot; var now = await active.ReadIrSlot(slot);
                    if (!now.SequenceEqual(originals[slot])) await active.ReplaceIr(slot, now, originals[slot], async bytes => { await IrBackup.Save(folder, active.Identity, active.IdentityHex, targetSlot, bytes); });
                    Console.WriteLine($"USER IR {slot + 1}/8 RESTORED AND VERIFIED");
                }
                await ApplyBank(active, bank); await active.ApplyIrRam(ram);
                Console.WriteLine("USER BANK AND RAM RESTORED EXACTLY");
            }
            finally { recovery?.Dispose(); }
        }
        await File.WriteAllTextAsync(Path.Combine(folder, "factory-validation.json"), JsonSerializer.Serialize(new
        { testedAt = DateTimeOffset.Now, identity, presetsVerified = true, irSlotsVerified = 8, originalStateRestored = true, exeSha256 = FactoryResources.ExeHash, bfiSha256 = FactoryResources.BfiHash }, new JsonSerializerOptions { WriteIndented = true }));
    }
    if (!(await session.ReadBank()).SequenceEqual(bank)) throw new IOException("Banco de presets mudou durante o ensaio IR.");
    return;
}
if (args[0] == "verify-live")
{
    // Reversible live test. Readback uses the low-address bank, not the high RAM alias.
    int target = bank[1] == 7 ? 6 : bank[1] + 1;
    try
    {
        await session.WriteLive(0, 1, target);
        Console.WriteLine($"LIVE WRITE AND READBACK VERIFIED: gain {bank[1]} -> {target}");
    }
    finally
    {
        await session.WriteLive(0, 1, bank[1]);
        Console.WriteLine($"ORIGINAL GAIN RESTORED AND VERIFIED: {bank[1]}");
    }
    if (!(await session.ReadBank()).SequenceEqual(bank)) throw new IOException("Banco atual difere da captura inicial após o teste.");
    Console.WriteLine("FULL INITIAL BANK RESTORED EXACTLY");
}
if (args.Length > 1)
{
    Directory.CreateDirectory(args[1]);
    await File.WriteAllTextAsync(Path.Combine(args[1], "cube-baby-read.json"), JsonSerializer.Serialize(new
    { capturedAt = DateTimeOffset.Now, identity, bankHex = Convert.ToHexString(bank), operation = args[0] }, new JsonSerializerOptions { WriteIndented = true }));
}
