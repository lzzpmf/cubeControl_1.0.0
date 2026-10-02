using System.Buffers.Binary;
using System.IO;
using System.Text.Json;
using Cube.Protocol;

internal static class EffectsFirmwareBTests
{
    public static async Task Run(Action<bool, string> check)
    {
        using var phaser = new PhaserConnection();
        using var transport = new EffectsFirmwareBConnection(phaser, phaser.Bank, phaser.State);
        using var session = new CubeSession(transport);
        await session.Identify();
        check(await session.VerifyEffectsFirmware() == "cubebaby_B_20220706" && transport.Writes == 0,
            "B profile verifies all four effects with no writes");
        check(transport.Reads.Contains(0xFFFFD958) && transport.Reads.Contains(0xFFFD8270) && transport.Reads.Contains(0xFFFD81F6),
            "B reader, bank alias and physical state use traced addresses");
        var pOriginal = phaser.Dsp.ToArray();
        var p = await session.BeginPhaserPreview(0, _ => Task.CompletedTask);
        p = await session.ApplyPhaserPreview(p, p.Target with { Lower = 1000, Level = 0.32f }, _ => Task.CompletedTask);
        await session.CheckPhaserPreview(p);
        check(PhaserParameters.Read(phaser.Dsp) == p.Target, "B phaser apply and idle monitor use correct context");
        await session.RestorePhaserPreview(p);
        check(PhaserParameters.StableEquals(phaser.Dsp, pOriginal), "B phaser restores original DSP");

        using var chorus = new ChorusPreviewConnection();
        using var cTransport = new EffectsFirmwareBConnection(chorus, chorus.Bank, chorus.State);
        using var cSession = new CubeSession(cTransport); await cSession.Identify();
        var cOriginal = chorus.Dsp.ToArray();
        var c = await cSession.BeginChorusPreview(0, _ => Task.CompletedTask);
        c = await cSession.ApplyChorusPreview(c, c.Target with { Depth = 0.2f, Mix = 0.425f, Feedback = 0.05f }, _ => Task.CompletedTask);
        await cSession.CheckChorusPreview(c);
        check(ChorusParameters.Read(chorus.Dsp) == c.Target, "B chorus apply and idle monitor pass");
        await cSession.RestoreChorusPreview(c);
        check(ChorusParameters.StableEquals(chorus.Dsp, cOriginal), "B chorus restores original DSP");

        using var delay = new DelayConnection(); delay.Activate();
        using var dTransport = new EffectsFirmwareBConnection(delay, delay.Bank, delay.State);
        using var dSession = new CubeSession(dTransport); await dSession.Identify();
        var dOriginal = delay.Dsp.ToArray();
        var d = await dSession.BeginDelayPreview(0, _ => Task.CompletedTask);
        d = await dSession.ApplyDelayPreview(d, d.Target with { TimeSamples = 6000, Feedback = 0.1f, Depth = 0.05f, Rate = 0.005f, Mix = 0.125f }, _ => Task.CompletedTask);
        await dSession.CheckDelayPreview(d);
        check(DelayParameters.Read(delay.Dsp) == d.Target, "B delay applies all five controls and monitors context");
        await dSession.RestoreDelayPreview(d);
        check(DelayParameters.StableEquals(delay.Dsp, dOriginal), "B delay restores original DSP");

        using var reverb = new ReverbConnection(); reverb.Activate();
        using var rTransport = new EffectsFirmwareBConnection(reverb, reverb.Bank, reverb.State);
        using var rSession = new CubeSession(rTransport); await rSession.Identify();
        var rOriginal = reverb.Dsp.ToArray();
        var r = await rSession.BeginReverbPreview(0, _ => Task.CompletedTask);
        r = await rSession.ApplyReverbPreview(r, r.Target with { Dry = 0.5f, Wet = 0.275f, Decay = 1, ReturnLevel = 40, Damping = 5 }, _ => Task.CompletedTask);
        await rSession.CheckReverbPreview(r);
        check(ReverbParameters.Read(reverb.Dsp) == r.Target, "B reverb applies all five controls and monitors context");
        await rSession.RestoreReverbPreview(r);
        check(ReverbParameters.StableEquals(reverb.Dsp, rOriginal), "B reverb restores original DSP");

        foreach (uint address in transport.Code.Keys)
        {
            using var wrong = new PhaserConnection();
            using var wrongTransport = new EffectsFirmwareBConnection(wrong, wrong.Bank, wrong.State) { CorruptAddress = address };
            using var wrongSession = new CubeSession(wrongTransport); await wrongSession.Identify();
            bool rejected = false;
            try { await wrongSession.VerifyEffectsFirmware(); } catch (NotSupportedException) { rejected = true; }
            check(rejected && wrongTransport.Writes == 0 && !wrongSession.IsFaulted,
                $"B rejects altered code/table/dispatch at {address:X8} without writes or global fault");
        }
        using var changed = new PhaserConnection();
        using var changedTransport = new EffectsFirmwareBConnection(changed, changed.Bank, changed.State) { BadBankAlias = true };
        using var changedSession = new CubeSession(changedTransport); await changedSession.Identify();
        bool badAlias = false;
        try { await changedSession.BeginPhaserPreview(0, _ => Task.CompletedTask); } catch (IOException) { badAlias = true; }
        check(badAlias && changedTransport.Writes == 0, "B bank alias mismatch blocks preview before writing");
        phaser.State[0] = 1;
        int writes = transport.Writes;
        bool physicalChanged = false;
        try { await session.ApplyPhaserPreview(p with { Target = PhaserParameters.Default }, p.Target, _ => Task.CompletedTask); }
        catch (IOException) { physicalChanged = true; }
        check(physicalChanged && transport.Writes == writes, "B physical preset change blocks further writes");
    }
}

// Code comes from the supplied update, while the established DSP fakes implement
// only audio-parameter writes. Wrong legacy context addresses deliberately fail.
internal sealed class EffectsFirmwareBConnection : IMidiConnection
{
    private readonly IMidiConnection inner;
    private readonly byte[] bank, state;
    public event Action<byte[]>? Received;
    public Dictionary<uint, byte[]> Code { get; }
    public HashSet<uint> Reads { get; } = [];
    public uint? CorruptAddress;
    public bool BadBankAlias;
    public int Writes;
    public EffectsFirmwareBConnection(IMidiConnection inner, byte[] bank, byte[] state)
    {
        this.inner = inner; this.bank = bank; this.state = state;
        inner.Received += Forward;
        Code = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "effects-firmware-b.json")))!
            .ToDictionary(p => uint.Parse(p.Key), p => Convert.FromHexString(p.Value));
    }
    private void Forward(byte[] bytes) => Received?.Invoke(bytes);
    public void Send(byte[] sysex)
    {
        var request = UsbSysEx.Decode(sysex);
        if (request.Command == 0x22) Writes++;
        if (request.Command != 0x23 || request.Payload[0] != 4) { inner.Send(sysex); return; }
        uint address = BinaryPrimitives.ReadUInt32LittleEndian(request.Payload.AsSpan(1));
        int size = request.Payload[5] | request.Payload[6] << 8 | request.Payload[7] << 16;
        Reads.Add(address);
        if (address < 0xF70) { inner.Send(sysex); return; }
        byte[] bytes;
        if (address == 0xFFFD8270)
        {
            bytes = bank.AsSpan(0, size).ToArray();
            if (BadBankAlias) bytes[0] ^= 1;
        }
        else if (address == 0xFFFD81F6) bytes = state.ToArray();
        else
        {
            var range = Code.First(p => address >= p.Key && (ulong)address + (uint)size <= (ulong)p.Key + (uint)p.Value.Length);
            bytes = range.Value.AsSpan((int)(address - range.Key), size).ToArray();
            if (CorruptAddress is uint bad && bad >= address && (ulong)bad < (ulong)address + (uint)size)
                bytes[(int)(bad - address)] ^= 1;
        }
        Received?.Invoke(UsbSysEx.Encode(new CubePacket(0x23, request.Payload.Concat(bytes).ToArray())));
    }
    public void Dispose() => inner.Received -= Forward;
}
