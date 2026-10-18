using System.Buffers.Binary;
using Cube.Protocol;

internal static class CommunicationPerformanceTests
{
    public static async Task Run(Action<bool, string> check)
    {
        using (var fake = new FakeConnection())
        using (var session = new CubeSession(fake))
        {
            await session.Identify();
            var bank = await session.ReadBank();
            int exchanges = 0;
            fake.Observe = _ => exchanges++;
            bank = await session.WriteLiveConfirmed(0, 5, 20, bank);
            check(exchanges == 2 && bank[5] == 20, "interactive edit uses write + verified bank read only");
            fake.Bank[1] = 1;
            bool rejected = false;
            try { await session.WriteLiveConfirmed(0, 5, 21, bank); }
            catch (InvalidOperationException) { rejected = true; }
            check(rejected && fake.Bank[1] == 1, "interactive edit detects and preserves unrelated external changes");
            bank = await session.ReadBank();
            fake.Observe = packet => { if (packet.Command == 0x23) fake.Bank[5] = 0; };
            rejected = false;
            try { await session.WriteLiveConfirmed(0, 5, 22, bank); }
            catch (IOException) { rejected = true; }
            check(rejected, "interactive edit still rejects ACK without confirmed target");
        }
        using (var fake = new ChorusPreviewConnection())
        using (var counted = new CountingConnection(fake))
        using (var session = new CubeSession(counted))
        {
            await session.Identify();
            var recovery = await session.BeginChorusPreview(0, _ => Task.CompletedTask);
            var initial = fake.Dsp.ToArray();
            counted.Reads.Clear();
            var preparation = await session.ReadEffectPreparationState(1);
            check(counted.Reads.Count == 3 && preparation.Bank.SequenceEqual(fake.Bank) && preparation.PhysicalState.SequenceEqual(fake.State),
                "effect preparation reads only bank, alias and physical selection");
            counted.Reads.Clear();
            recovery = await session.ApplyChorusPreview(recovery, recovery.Target with { Depth = 0.2f }, _ => Task.CompletedTask);
            check(counted.Reads.Count == 16 && counted.Reads.Where(r => r.Region == 4 && r.Address < 0xF70)
                .All(r => r.Address >= 0x63C && r.Address + r.Length <= 0x764),
                "single chorus control uses 16 bounded reads instead of 19 with per-field checks");
            check(recovery.OriginalDsp.SequenceEqual(initial), "optimized edits retain the complete original recovery snapshot");
            counted.Reads.Clear(); int writes = fake.Writes;
            recovery = await session.ApplyChorusPreview(recovery, recovery.Target, _ => throw new Exception("No new recovery intent needed"));
            check(fake.Writes == writes && counted.Reads.Count == 8, "unchanged effect confirms fresh context once without writing");
            counted.Reads.Clear();
            await session.RestoreChorusPreview(recovery);
            check(counted.Reads.Count == 16, "restore brackets changed fields with initial and final captures only");
            check(ChorusParameters.Read(fake.Dsp) == ChorusParameters.Read(initial), "optimized restore returns original parameters");
            recovery = recovery with { Target = ChorusParameters.Read(initial) };
            bool rejected = false;
            counted.OnWrite = () => fake.Dsp[0x700] ^= 1;
            try { await session.ApplyChorusPreview(recovery, recovery.Target with { Depth = 0.2f }, _ => Task.CompletedTask); }
            catch (IOException) { rejected = true; }
            check(rejected, "compact capture still detects a different effect block changing during write");
        }
        using (var fake = new DelayConnection())
        using (var counted = new CountingConnection(fake))
        using (var session = new CubeSession(counted))
        {
            fake.Activate(); await session.Identify();
            var recovery = await session.BeginDelayPreview(0, _ => Task.CompletedTask);
            counted.Reads.Clear();
            recovery = await session.ApplyDelayPreview(recovery, new DelayParameters(9601, 0.123f, 0.05f, 0.005f, 0.321f), _ => Task.CompletedTask);
            int batchReads = counted.Reads.Count;
            counted.Reads.Clear();
            await session.RestoreDelayPreview(recovery);
            check(batchReads == 16 && counted.Reads.Count == 16,
                "Delay multi-field update and restore each use two bounded captures (16 reads)");
            check(DelayParameters.Read(fake.Dsp) == DelayParameters.Read(recovery.OriginalDsp),
                "Delay optimized restore preserves original parameters");
        }
        using (var fake = new ReverbConnection())
        using (var counted = new CountingConnection(fake))
        using (var session = new CubeSession(counted))
        {
            fake.Activate(); await session.Identify();
            var recovery = await session.BeginReverbPreview(0, _ => Task.CompletedTask);
            counted.Reads.Clear();
            recovery = await session.ApplyReverbPreview(recovery, new ReverbParameters(0.5f, 0.275f, 1, 40, 5), _ => Task.CompletedTask);
            int batchReads = counted.Reads.Count;
            counted.Reads.Clear();
            await session.RestoreReverbPreview(recovery);
            check(batchReads == 16 && counted.Reads.Count == 16,
                "Reverb multi-field update and restore each use two bounded captures (16 reads)");
            check(ReverbParameters.Read(fake.Dsp) == ReverbParameters.Read(recovery.OriginalDsp),
                "Reverb optimized restore preserves original parameters");
        }
    }

    private sealed class CountingConnection : IMidiConnection
    {
        private readonly IMidiConnection inner;
        public CountingConnection(IMidiConnection inner) { this.inner = inner; inner.Received += Forward; }
        public event Action<byte[]>? Received;
        public List<(byte Region, uint Address, uint Length)> Reads { get; } = [];
        public Action? OnWrite;
        private void Forward(byte[] bytes) => Received?.Invoke(bytes);
        public void Send(byte[] bytes)
        {
            var packet = UsbSysEx.Decode(bytes);
            if (packet.Command == 0x23) Reads.Add((packet.Payload[0],
                BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1)),
                (uint)(packet.Payload[5] | packet.Payload[6] << 8 | packet.Payload[7] << 16)));
            if (packet.Command == 0x22) OnWrite?.Invoke();
            inner.Send(bytes);
        }
        public void Dispose() { inner.Received -= Forward; }
    }
}
