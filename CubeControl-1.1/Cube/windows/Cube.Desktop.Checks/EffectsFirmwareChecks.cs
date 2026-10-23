using System.Buffers.Binary;
using System.IO;
using Cube.Protocol;

internal static class EffectsFirmwareChecks
{
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }

    public static async Task Run()
    {
        using (var device = new Vd612Connection())
        using (var session = new CubeSession(device))
        {
            await session.Identify();
            Assert(await session.VerifyEffectsFirmware() == "CubeBaby_VD612", "VD612 is identified by pinned code ranges");
            for (int stage = 1; stage <= 4; stage++)
            {
                var state = await session.ReadEffectPreparationState(stage);
                Assert(state.Bank.SequenceEqual(device.Bank) && state.PhysicalState.SequenceEqual(device.Physical), "VD612 preparation uses its own bank alias and physical state: " + stage);
            }
            Assert(device.Writes == 0, "firmware identification and preparation never write");
        }

        for (int range = 0; range < 4; range++)
        {
            using var device = new Vd612Connection();
            device.Ranges[range].Data[0] ^= 1;
            using var session = new CubeSession(device); await session.Identify();
            await Reject(() => session.VerifyEffectsFirmware());
            Assert(device.Writes == 0 && session.EffectsFirmware == null, "corrupted VD612 code/table range is rejected before writes: " + range);
        }

        using (var device = new Vd612Connection())
        using (var session = new CubeSession(device))
        {
            device.AliasMismatch = true; await session.Identify();
            await Reject(() => session.VerifyEffectsFirmware());
            Assert(device.Writes == 0, "divergent bank alias blocks VD612 before writes");
        }
        using (var device = new Vd612Connection())
        using (var session = new CubeSession(device))
        {
            device.Ranges[0].Data[0x832510 - 0x830C00] ^= 1;
            await session.Identify(); await Reject(() => session.VerifyEffectsFirmware());
            Assert(device.Writes == 0 && session.EffectsFirmware == null, "unknown reader signature never enables effect writes");
        }
        for (int stage = 1; stage <= 4; stage++) await Cycle(stage);
        for (int stage = 1; stage <= 4; stage++)
        {
            using var device = new Vd612Connection(); device.Prepare(stage);
            device.Dsp[stage is 1 or 2 ? 0x63C : stage == 3 ? 0x694 : 0x6E0] = 1;
            using var session = new CubeSession(device); await session.Identify();
            await Reject(async () =>
            {
                switch (stage)
                {
                    case 1: await session.BeginChorusPreview(0, _ => Task.CompletedTask); break;
                    case 2: await session.BeginPhaserPreview(0, _ => Task.CompletedTask); break;
                    case 3: await session.BeginDelayPreview(0, _ => Task.CompletedTask); break;
                    case 4: await session.BeginReverbPreview(0, _ => Task.CompletedTask); break;
                }
            });
            Assert(device.Writes == 0, "VD612 initialization state is rejected before edits: " + stage);
        }
        await InterruptedDelay();
    }

    private static async Task Reject(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) when (ex is NotSupportedException or IOException or FormatException or ArgumentException) { return; }
        throw new Exception("Expected operation to be rejected");
    }

    private static async Task Cycle(int stage)
    {
        using var device = new Vd612Connection(); device.Prepare(stage);
        byte[] original = device.Dsp.ToArray(), bank = device.Bank.ToArray();
        using var session = new CubeSession(device); await session.Identify();
        switch (stage)
        {
            case 1:
                var chorus = await session.BeginChorusPreview(0, _ => Task.CompletedTask);
                chorus = await session.ApplyChorusPreview(chorus, chorus.Target with { Mix = .5f, Feedback = .05f }, _ => Task.CompletedTask);
                await session.CheckChorusPreview(chorus); await session.RestoreChorusPreview(chorus); break;
            case 2:
                var phaser = await session.BeginPhaserPreview(0, _ => Task.CompletedTask);
                phaser = await session.ApplyPhaserPreview(phaser, phaser.Target with { Level = .5f, Lower = 500 }, _ => Task.CompletedTask);
                await session.CheckPhaserPreview(phaser); await session.RestorePhaserPreview(phaser); break;
            case 3:
                var delay = await session.BeginDelayPreview(0, _ => Task.CompletedTask);
                delay = await session.ApplyDelayPreview(delay, delay.Target with { Mix = .5f, TimeSamples = 9000 }, _ => Task.CompletedTask);
                await session.CheckDelayPreview(delay); await session.RestoreDelayPreview(delay); break;
            case 4:
                var reverb = await session.BeginReverbPreview(0, _ => Task.CompletedTask);
                reverb = await session.ApplyReverbPreview(reverb, reverb.Target with { Wet = .5f, Decay = 3 }, _ => Task.CompletedTask);
                await session.CheckReverbPreview(reverb); await session.RestoreReverbPreview(reverb); break;
        }
        Assert(device.Writes > 0 && device.Dsp.SequenceEqual(original) && device.Bank.SequenceEqual(bank), "VD612 apply/monitor/restore preserves raw DSP and bank: " + stage);
        Assert(device.WriteAddresses.All(address => address is >= 0x640 and < 0x764), "VD612 only writes bounded effect controls: " + stage);
    }

    private static async Task InterruptedDelay()
    {
        using var device = new Vd612Connection(); byte[] original = device.Dsp.ToArray();
        using var session = new CubeSession(device); await session.Identify();
        var recovery = await session.BeginDelayPreview(0, _ => Task.CompletedTask);
        device.FailOnWrite = 2;
        await Reject(() => session.ApplyDelayPreview(recovery, recovery.Target with { Mix = .5f, TimeSamples = 9000 }, saved => { recovery = saved; return Task.CompletedTask; }));
        Assert(!device.Dsp.SequenceEqual(original), "interrupted write fixture contains a partial mutation");
        device.FailOnWrite = 0;
        session.Dispose();
        using var resumed = new CubeSession(device); await resumed.Identify();
        await resumed.RestoreDelayPreview(recovery);
        Assert(device.Dsp.SequenceEqual(original), "VD612 restores a preserved partial transaction");
        device.Physical[0] = 1; int writes = device.Writes;
        await Reject(() => resumed.ApplyDelayPreview(recovery, recovery.Target with { Mix = .5f }, _ => Task.CompletedTask));
        Assert(device.Writes == writes, "physical preset change blocks writes to a stale recovery");
    }

    private sealed class Vd612Connection : IMidiConnection
    {
        private const uint DspBase = 0x834724;
        public event Action<byte[]>? Received;
        public byte[] Dsp = ReadFixture("dsp.bin"), Bank = ReadFixture("bank.bin"), Physical = [0, 1];
        public List<(uint Start, byte[] Data)> Ranges = [];
        public List<uint> WriteAddresses = [];
        public int Writes, FailOnWrite;
        public bool AliasMismatch;
        private static byte[] ReadFixture(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Vd612", name));
        public Vd612Connection()
        {
            byte[] code = ReadFixture("firmware-ranges.bin"); int offset = 0;
            foreach (var (address, size) in new (uint, int)[] { (0x830C00, 0x3200), (0xC49F00, 0x600), (0xC13800, 0x800), (0xC48900, 0xA00) })
            { Ranges.Add((unchecked(address - DspBase), code.AsSpan(offset, size).ToArray())); offset += size; }
        }
        public void Prepare(int stage)
        {
            if (stage is not (1 or 2)) return;
            Bank[8] = stage == 1 ? (byte)2 : (byte)9;
            Dsp.AsSpan(0x63C, 0x58).Clear(); Dsp[0x63C] = 2; Dsp[0x63D] = stage == 1 ? (byte)5 : (byte)2;
            void F(int offset, float value) => BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(offset), value);
            if (stage == 1)
            { F(0x644, .4f); F(0x648, .000031f); F(0x688, .85f); F(0x650, 1); F(0x66C, 1); }
            else
            { Dsp[0x640] = 1; Dsp[0x641] = 2; F(0x644, .00001f); F(0x648, 5000); F(0x64C, 300); F(0x650, 3000); F(0x654, .64f); }
        }
        public void Send(byte[] sysex)
        {
            var packet = UsbSysEx.Decode(sysex); CubePacket response;
            if (packet.Command == 0x11) response = new(0x11, "SINCO-CubeBaby"u8.ToArray());
            else if (packet.Command == 0x23)
            {
                byte region = packet.Payload[0]; uint address = BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1));
                int size = packet.Payload[5] | packet.Payload[6] << 8 | packet.Payload[7] << 16; byte[] bytes;
                if (region == 5 && address == 0) bytes = Bank.AsSpan(0, size).ToArray();
                else if (region == 4 && address == unchecked(0x80CBBCu - DspBase))
                { bytes = Bank.AsSpan(0, size).ToArray(); if (AliasMismatch) bytes[0] ^= 1; }
                else if (region == 4 && address == unchecked(0x80CB16u - DspBase)) bytes = Physical.AsSpan(0, size).ToArray();
                else if (region == 4 && address < Dsp.Length) bytes = Dsp.AsSpan((int)address, size).ToArray();
                else
                {
                    var range = Ranges.FirstOrDefault(r => address >= r.Start && (ulong)address + (uint)size <= (ulong)r.Start + (uint)r.Data.Length);
                    bytes = range.Data == null ? new byte[size] : range.Data.AsSpan((int)(address - range.Start), size).ToArray();
                }
                response = new(0x23, packet.Payload.Concat(bytes).ToArray());
            }
            else if (packet.Command == 0x22 && packet.Payload[0] == 4)
            {
                Writes++; if (Writes == FailOnWrite) throw new IOException("Simulated interrupted write");
                uint address = BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1)); WriteAddresses.Add(address);
                packet.Payload.AsSpan(8).CopyTo(Dsp.AsSpan((int)address)); response = new(0, [0]);
            }
            else throw new Exception("Unexpected command: " + packet.Command);
            Received?.Invoke(UsbSysEx.Encode(response));
        }
        public void Dispose() { }
    }
}
