using System.Buffers.Binary;
using System.IO;
using System.Text.Json;
using Cube.Protocol;

internal static class ReverbTests
{
    public static async Task Run(Action<bool, string> check)
    {
        using var fake = new ReverbConnection();
        using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) };
        await session.Identify();
        int intents = 0, restored = 0; bool original = false;
        Task Keep(string label, ReverbSnapshot s)
        {
            if (label == "original") original = true;
            if (label == "restored") { check(s.Dsp.All(b => b == 0) && s.Bank[3] == 0, "research restores original RAM"); restored++; }
            return Task.CompletedTask;
        }
        Task Intent(string kind, byte region, uint address, byte[] before, byte[] after)
        {
            if (!original || intents != fake.Writes || region is not (4 or 5)) throw new Exception("Intent/region mismatch");
            intents++; return Task.CompletedTask;
        }
        foreach (string mode in new[] { "reverb-sweep", "reverb-controls", "reverb-transitions" })
            await session.RunReverbResearch(mode, Keep, Intent);
        check(restored == 3 && fake.Writes == intents, "all research writes have prior durable intent; no flash");
        fake.Activate();
        var baseline = fake.Dsp.ToArray();
        ReverbRecovery? saved = null;
        Task Save(ReverbRecovery r) { saved = JsonSerializer.Deserialize<ReverbRecovery>(JsonSerializer.Serialize(r)); return Task.CompletedTask; }
        var recovery = await session.BeginReverbPreview(0, Save);
        var target = new ReverbParameters(0.5f, 0.275f, 1, 40, 5);
        recovery = await session.ApplyReverbPreview(recovery, target, Save);
        await session.CheckReverbPreview(recovery);
        check(ReverbParameters.Read(fake.Dsp) == target, "five independent controls applied and monitored with bounded USB reads");
        check(ReverbPreset.Import(ReverbPreset.Export(session.IdentityHex, target)).Parameters == target, "reverb file roundtrip");
        await session.RestoreReverbPreview(recovery);
        check(ReverbParameters.StableEquals(baseline, fake.Dsp), "preview restores original fields");
        foreach (var invalid in new[] { target with { Dry = float.NaN }, target with { Wet = float.PositiveInfinity },
            target with { Wet = 1 }, target with { Decay = 0 }, target with { ReturnLevel = 81 }, target with { Damping = 11 } })
        {
            bool rejected = false; try { invalid.Validate(); } catch (ArgumentOutOfRangeException) { rejected = true; }
            check(rejected, "invalid/unproven range rejected");
        }
        foreach (string json in new[] { "{}", ReverbPreset.Export(session.IdentityHex, target).Replace(ReverbParameters.Mapping, "other"),
            ReverbPreset.Export(session.IdentityHex, target).Replace("\"Dry\"", "\"Address\"") })
        {
            bool rejected = false; try { ReverbPreset.Import(json); } catch (Exception e) when (e is FormatException or JsonException) { rejected = true; }
            check(rejected, "invalid preset rejected");
        }
        recovery = await session.BeginReverbPreview(0, Save);
        fake.IgnoreWrites = true; bool failed = false;
        try { await session.ApplyReverbPreview(recovery, target, Save); } catch (IOException) { failed = true; }
        check(failed, "ACK without mutation rejected"); fake.IgnoreWrites = false;
        fake.Dsp[0x708] ^= 1; int writes = fake.Writes; failed = false;
        try { await session.ApplyReverbPreview(recovery, target, Save); } catch (Exception e) when (e is IOException or ArgumentException) { failed = true; }
        check(failed && writes == fake.Writes, "external reload blocks new writes"); fake.Dsp = baseline.ToArray();
        fake.State[0] = 1; failed = false;
        try { await session.RestoreReverbPreview(recovery); } catch (IOException) { failed = true; }
        check(failed && writes == fake.Writes, "physical change blocks restoration"); fake.State[0] = 0;
        fake.FailAfterWrites = fake.Writes + 2; failed = false;
        try { await session.ApplyReverbPreview(recovery, target, Save); } catch (TimeoutException) { failed = true; }
        check(failed && session.IsFaulted && saved!.Target == target, "partial disconnect preserves target before writes");
        fake.FailAfterWrites = int.MaxValue; fake.Silent = false;
        using var reconnect = new CubeSession(fake); await reconnect.Identify(); await reconnect.RestoreReverbPreview(saved!);
        check(ReverbParameters.StableEquals(fake.Dsp, baseline), "serialized recovery works after reconnect");
        var partial = baseline.ToArray();
        // A torn multi-byte field may not decode as a table row, but its bytes
        // must belong to the durable transaction before restoration is allowed.
        fake.Activate(1); partial[0x6EA] = fake.Dsp[0x6EA]; fake.Bank[3] = 8; fake.Dsp = partial;
        await reconnect.RestoreReverbPreview(saved!);
        check(ReverbParameters.StableEquals(fake.Dsp, baseline), "partial coefficient row can be restored from durable intent");
        fake.Dsp[0x6E9] = 1; writes = fake.Writes; failed = false;
        try { await reconnect.RestoreReverbPreview(saved!); } catch (IOException) { failed = true; }
        check(failed && fake.Writes == writes, "unknown non-control byte blocks recovery without writes"); fake.Dsp = baseline.ToArray();
        fake.Bank[4] = 1; failed = false;
        try { await reconnect.RestoreReverbPreview(saved!); } catch (IOException) { failed = true; }
        check(failed && fake.Writes == writes, "external bank edit blocks recovery"); fake.Bank[4] = 0;
        fake.BadSignature = true; using var bad = new CubeSession(fake); await bad.Identify(); writes = fake.Writes; failed = false;
        try { await bad.BeginReverbPreview(0, Save); } catch (IOException) { failed = true; }
        check(failed && fake.Writes == writes, "incompatible firmware never written"); fake.BadSignature = false;
        fake.BadReadHeader = true; using var header = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) }; await header.Identify(); failed = false;
        try { await header.ReadReverbResearch(); } catch (TimeoutException) { failed = true; }
        check(failed && header.IsFaulted, "wrong read address faults session");
        var dynamic = baseline.ToArray(); dynamic[0x658] ^= 1;
        check(!ReverbParameters.StableEquals(baseline, dynamic), "reverb does not inherit phaser dynamic mask");
        using var moved = new ReverbConnection(); moved.Activate();
        using var moving = new CubeSession(moved); await moving.Identify();
        var movingRecovery = await moving.BeginReverbPreview(0, Save);
        moved.MoveAfterWrites = 1; failed = false;
        try { await moving.ApplyReverbPreview(movingRecovery, target, Save); } catch (IOException) { failed = true; }
        check(failed && moved.Writes == 1, "physical switch between fields stops remaining writes");
    }
}

internal sealed class ReverbConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank = new byte[48];
    public byte[] Dsp = new byte[0xF70];
    public byte[] State = [0, 1];
    public int Writes;
    public int FailAfterWrites = int.MaxValue;
    public int MoveAfterWrites = int.MaxValue;
    public bool Silent, IgnoreWrites, BadSignature, BadReadHeader;
    private readonly Dictionary<uint, byte[]> code;
    public ReverbConnection()
    {
        code = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "reverb-live-code.json")))!
            .ToDictionary(p => uint.Parse(p.Key), p => Convert.FromHexString(p.Value));
        Bank[8] = 7; Bank[10] = 1;
    }
    private byte[] At(uint cpu) => code[unchecked(cpu - 0x8347F8u)];
    public void Activate(int value = 8)
    {
        Bank[3] = (byte)value; Apply(-1);
    }
    private void Apply(int previous)
    {
        int raw = Bank[3];
        if (raw == 0 || Bank[10] == 0) { Dsp[0x6E0] = 0; return; }
        if (Dsp[0x6E0] == 0) { At(0xC48C98).CopyTo(Dsp, 0x6E0); previous = -1; }
        Dsp[0x6E0] = 3;
        if (previous != raw)
        {
            At(0xC4881C).AsSpan((raw - 1) * 12, 12).CopyTo(Dsp.AsSpan(0x6EA));
            At(0xC48D38).AsSpan((raw - 1) * 8, 8).CopyTo(Dsp.AsSpan(0x708));
            Dsp[0x6E2] = (byte)(raw * 8);
        }
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
                else if (address < Dsp.Length) data = Dsp.AsSpan((int)address, length).ToArray();
                else
                {
                    var range = code.First(x => address >= x.Key && (ulong)address + (uint)length <= (ulong)x.Key + (uint)x.Value.Length);
                    data = range.Value.AsSpan((int)(address - range.Key), length).ToArray();
                    if (BadSignature && address == unchecked(0x833978u - 0x8347F8u)) data[0] ^= 1;
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
                        int previous = Bank[3]; int offset = (int)(address - 0x80000000u);
                        Bank[offset] = p.Payload[8]; Apply(offset == 10 ? -1 : previous);
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
