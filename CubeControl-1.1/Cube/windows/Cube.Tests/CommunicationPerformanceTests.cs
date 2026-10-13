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
            check(counted.Reads.Count == 19 && counted.Reads.Where(r => r.Region == 4 && r.Address < 0xF70)
                .All(r => r.Address >= 0x63C && r.Address + r.Length <= 0x764),
                "single chorus control uses 19 bounded reads instead of 83 full-capture reads");
            check(recovery.OriginalDsp.SequenceEqual(initial), "optimized edits retain the complete original recovery snapshot");
            counted.Reads.Clear(); int writes = fake.Writes;
            recovery = await session.ApplyChorusPreview(recovery, recovery.Target, _ => throw new Exception("No new recovery intent needed"));
            check(fake.Writes == writes && counted.Reads.Count == 8, "unchanged effect confirms fresh context once without writing");
            await session.RestoreChorusPreview(recovery);
            check(ChorusParameters.Read(fake.Dsp) == ChorusParameters.Read(initial), "optimized restore returns original parameters");
            recovery = recovery with { Target = ChorusParameters.Read(initial) };
            bool rejected = false;
            counted.OnWrite = () => fake.Dsp[0x700] ^= 1;
            try { await session.ApplyChorusPreview(recovery, recovery.Target with { Depth = 0.2f }, _ => Task.CompletedTask); }
            catch (IOException) { rejected = true; }
            check(rejected, "compact capture still detects a different effect block changing during write");
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
