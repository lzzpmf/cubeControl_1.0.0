using System.Buffers.Binary;
using System.IO;
using System.Text.Json;
using Cube.Protocol;

internal static class DelayTests
{
    public static async Task Run(Action<bool, string> check)
    {
        using var fake = new DelayConnection();
        using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) };
        await session.Identify();
        int intents = 0, restored = 0; bool original = false;
        Task Keep(string label, DelaySnapshot s)
        {
            if (label == "original") original = true;
            if (label == "restored") { check(s.Dsp.All(b => b == 0) && s.Bank[11] == 0, "research restores original RAM"); restored++; }
            return Task.CompletedTask;
        }
        Task Intent(string kind, byte region, uint address, byte[] before, byte[] after)
        {
            if (!original || intents != fake.Writes || region is not (4 or 5)) throw new Exception("Intent/region mismatch");
            intents++; return Task.CompletedTask;
        }
        int runtimeSamples = 0;
        foreach (string mode in new[] { "delay-sweep", "delay-controls", "delay-transitions", "delay-modulation", "delay-runtime" })
            await session.RunDelayResearch(mode, Keep, Intent, observe: (_, sample) =>
            {
                if (sample.CurrentTime.Length != 2 || sample.Runtime.Length != 46) throw new Exception("Runtime sample size");
                runtimeSamples++; return Task.CompletedTask;
            });
        check(restored == 5 && fake.Writes == intents && runtimeSamples == 30, "all research writes have prior durable intent; external state sampled read-only; no flash");
        fake.Activate();
        var baseline = fake.Dsp.ToArray();
        DelayRecovery? saved = null;
        Task Save(DelayRecovery r) { saved = JsonSerializer.Deserialize<DelayRecovery>(JsonSerializer.Serialize(r)); return Task.CompletedTask; }
        var recovery = await session.BeginDelayPreview(0, Save);
        var target = new DelayParameters(9601, 0.123f, 0.05f, 0.005f, 0.321f);
        recovery = await session.ApplyDelayPreview(recovery, target, Save);
        await session.CheckDelayPreview(recovery);
        check(DelayParameters.Read(fake.Dsp) == target, "five independent controls applied and monitored with bounded USB reads");
        check(DelayPreset.Import(DelayPreset.Export(session.IdentityHex, target)).Parameters == target, "delay file roundtrip");
        bool corruptRejected = false;
        try { (recovery with { Target = target with { Depth = 0 } }).Validate(); } catch (FormatException) { corruptRejected = true; }
        check(corruptRejected, "valid-looking recovery parameter corruption fails integrity check");
        await session.RestoreDelayPreview(recovery);
        check(DelayParameters.StableEquals(baseline, fake.Dsp), "preview restores original fields");
        foreach (var invalid in new[] { target with { Feedback = float.NaN }, target with { Mix = float.PositiveInfinity },
            target with { Mix = 1 }, target with { TimeSamples = 16384 }, target with { Depth = 0.11f }, target with { Rate = 0.011f } })
        {
            bool rejected = false; try { invalid.Validate(); } catch (ArgumentOutOfRangeException) { rejected = true; }
            check(rejected, "invalid/unproven range rejected");
        }
        foreach (string json in new[] { "{}", DelayPreset.Export(session.IdentityHex, target).Replace(DelayParameters.Mapping, "other"),
            DelayPreset.Export(session.IdentityHex, target).Replace("\"Depth\"", "\"Address\"") })
        {
            bool rejected = false; try { DelayPreset.Import(json); } catch (Exception e) when (e is FormatException or JsonException) { rejected = true; }
            check(rejected, "invalid preset rejected");
        }
        recovery = await session.BeginDelayPreview(0, Save);
        fake.IgnoreWrites = true; bool failed = false;
        try { await session.ApplyDelayPreview(recovery, target, Save); } catch (IOException) { failed = true; }
        check(failed, "ACK without mutation rejected"); fake.IgnoreWrites = false;
        fake.Dsp[0x69C] ^= 1; int writes = fake.Writes; failed = false;
        try { await session.ApplyDelayPreview(recovery, target, Save); } catch (Exception e) when (e is IOException or ArgumentException) { failed = true; }
        check(failed && writes == fake.Writes, "external reload blocks new writes"); fake.Dsp = baseline.ToArray();
        fake.State[0] = 1; failed = false;
        try { await session.RestoreDelayPreview(recovery); } catch (IOException) { failed = true; }
        check(failed && writes == fake.Writes, "physical change blocks restoration"); fake.State[0] = 0;
        fake.FailAfterWrites = fake.Writes + 2; failed = false;
        try { await session.ApplyDelayPreview(recovery, target, Save); } catch (TimeoutException) { failed = true; }
        check(failed && session.IsFaulted && saved!.Target == target, "partial disconnect preserves target before writes");
        fake.FailAfterWrites = int.MaxValue; fake.Silent = false;
        using var reconnect = new CubeSession(fake); await reconnect.Identify(); await reconnect.RestoreDelayPreview(saved!);
        check(DelayParameters.StableEquals(fake.Dsp, baseline), "serialized recovery works after reconnect");
        var partial = baseline.ToArray();
        // A torn multi-byte field may not decode as a table row, but its bytes
        // must belong to the durable transaction before restoration is allowed.
        var targetBits = BitConverter.GetBytes(target.Depth); partial[0x69C] = targetBits[0]; fake.Dsp = partial;
        await reconnect.RestoreDelayPreview(saved!);
        check(DelayParameters.StableEquals(fake.Dsp, baseline), "partial float field can be restored from durable intent");
        fake.Dsp[0x695] = 1; writes = fake.Writes; failed = false;
        try { await reconnect.RestoreDelayPreview(saved!); } catch (IOException) { failed = true; }
        check(failed && fake.Writes == writes, "unknown non-control byte blocks recovery without writes"); fake.Dsp = baseline.ToArray();
        fake.Bank[4] = 1; failed = false;
        try { await reconnect.RestoreDelayPreview(saved!); } catch (IOException) { failed = true; }
        check(failed && fake.Writes == writes, "external bank edit blocks recovery"); fake.Bank[4] = 32;
        fake.BadSignature = true; using var bad = new CubeSession(fake); await bad.Identify(); writes = fake.Writes; failed = false;
        try { await bad.BeginDelayPreview(0, Save); } catch (NotSupportedException) { failed = true; }
        check(failed && fake.Writes == writes, "incompatible firmware never written"); fake.BadSignature = false;
        fake.BadReadHeader = true; using var header = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) }; await header.Identify(); failed = false;
        try { await header.ReadDelayResearch(); } catch (TimeoutException) { failed = true; }
        check(failed && header.IsFaulted, "wrong read address faults session");
        var dynamic = baseline.ToArray(); dynamic[0x658] ^= 1;
        check(!DelayParameters.StableEquals(baseline, dynamic), "delay does not inherit phaser dynamic mask");
        using var moved = new DelayConnection(); moved.Activate();
        using var moving = new CubeSession(moved); await moving.Identify();
        var movingRecovery = await moving.BeginDelayPreview(0, Save);
        moved.MoveAfterWrites = 1; failed = false;
        try { await moving.ApplyDelayPreview(movingRecovery, target, Save); } catch (IOException) { failed = true; }
        check(failed && moved.Writes == 1, "physical switch between fields stops remaining writes");
    }
}

internal sealed class DelayConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank = new byte[48];
    public byte[] Dsp = new byte[0xF70];
    public byte[] State = [0, 1];
    public int Writes;
    public int FailAfterWrites = int.MaxValue;
    public int MoveAfterWrites = int.MaxValue;
    public bool Silent, IgnoreWrites, BadSignature, BadReadHeader;
    private float runtimePhase;
    private int runtimeIndex;
    private readonly Dictionary<uint, byte[]> code;
    public DelayConnection()
    {
        code = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "delay-live-code.json")))!
            .ToDictionary(p => uint.Parse(p.Key), p => Convert.FromHexString(p.Value));
        foreach (var item in JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","phaser-live-code.json")))!)
            code.TryAdd(uint.Parse(item.Key),Convert.FromHexString(item.Value));
        foreach (var item in JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","chorus-live-code.json")))!)
        {
            uint address = uint.Parse(item.Key); var bytes = Convert.FromHexString(item.Value);
            if (!code.TryGetValue(address, out var existing) || existing.Length < bytes.Length) code[address] = bytes;
        }
        Bank[8] = 7; Bank[6] = 16; Bank[4] = 32; Bank[7] = 32;
    }
    private byte[] At(uint cpu) => code[unchecked(cpu - 0x8347F8u)];
    public void Activate(int value = 16)
    {
        Bank[11] = 1; Bank[6] = (byte)value; Apply(-1, -1);
    }
    private void Apply(int offset, int previous)
    {
        if (Bank[11] == 0) { Dsp[0x694] = 0; Dsp[0x63C] = 0; return; }
        Dsp[0x63C] = Bank[8] is 7 or 8 ? (byte)0 : (byte)3;
        if (Dsp[0x63C] == 3) Dsp[0x63D] = Bank[8] <= 6 ? (byte)5 : (byte)2;
        bool initialize = Dsp[0x694] == 0;
        if (initialize)
        {
            BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(0x69C),0.1f);
            BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(0x6A0),0.01f);
            At(0xC488E0).CopyTo(Dsp,0x6A4);
        }
        Dsp[0x694] = 3;
        if (initialize || offset == 6 && previous != Bank[6]) BinaryPrimitives.WriteInt16LittleEndian(Dsp.AsSpan(0x696),(short)(2400+450*Bank[6]));
        if (initialize || offset == 4 && previous != Bank[4]) BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(0x698),Bank[4]/128f);
        if (initialize || offset == 7 && previous != Bank[7]) BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(0x6DC),Bank[7]/128f);
    }
    public void Send(byte[] sysex)
    {
        if (Silent) return;
        var p = UsbSysEx.Decode(sysex);
        CubePacket reply;
        if (p.Command == 0x11) reply = new(0x11, Convert.FromHexString("53494E434F2D437562654261627920202020200113141516171819"));
        else
        {
            uint address = BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1));
            if (p.Command == 0x23)
            {
                int length = p.Payload[5] | p.Payload[6] << 8 | p.Payload[7] << 16;
                if (length > 113) throw new Exception("Read exceeds the established USB chunk size");
                byte[] data;
                if (p.Payload[0] == 0) data = new byte[length];
                else if (p.Payload[0] == 5 || address == 0xFFFD827C) data = Bank.AsSpan(0, length).ToArray();
                else if (address == 0xFFFD8202) data = State.ToArray();
                else if (address == unchecked(0x80FF84u - 0x8347F8u)) data = Dsp.AsSpan(0x696, 2).ToArray();
                else if (address == unchecked(0x817F86u - 0x8347F8u))
                {
                    data = new byte[46];
                    runtimePhase += BinaryPrimitives.ReadSingleLittleEndian(Dsp.AsSpan(0x6A0)) * 256 / 9600;
                    if (runtimePhase > 1) runtimePhase -= 1;
                    runtimeIndex = (runtimeIndex + 256) % 16384;
                    BinaryPrimitives.WriteInt16LittleEndian(data, (short)runtimeIndex);
                    BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(0x26), runtimePhase);
                }
                else if (address < Dsp.Length) data = Dsp.AsSpan((int)address, length).ToArray();
                else
                {
                    var range = code.First(x => address >= x.Key && (ulong)address + (uint)length <= (ulong)x.Key + (uint)x.Value.Length);
                    data = range.Value.AsSpan((int)(address - range.Key), length).ToArray();
                    if (BadSignature && address == unchecked(0x832DA8u - 0x8347F8u)) data[0] ^= 1;
                }
                var payload = p.Payload.Concat(data).ToArray();
                if (BadReadHeader) payload[1] ^= 1;
                reply = new(0x23, payload);
            }
            else
            {
                if (p.Command != 0x22 || p.Payload[0] is not (4 or 5)) throw new Exception("Unexpected mutation");
                Writes++;
                if (Writes == MoveAfterWrites) State[0] = 1;
                if (!IgnoreWrites)
                {
                    if (p.Payload[0] == 4) p.Payload.AsSpan(8).CopyTo(Dsp.AsSpan((int)address));
                    else
                    {
                        int offset = (int)(address - 0x80000000u); int previous = Bank[offset];
                        Bank[offset] = p.Payload[8]; Apply(offset, previous);
                    }
                }
                if (Writes >= FailAfterWrites) { Silent = true; return; }
                reply = new(0, [0]);
            }
        }
        Received?.Invoke(UsbSysEx.Encode(reply));
    }
    public void Dispose() { }
}
