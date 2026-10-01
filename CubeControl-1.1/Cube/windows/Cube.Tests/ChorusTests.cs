using System.Buffers.Binary;
using System.Text.Json;
using Cube.Protocol;

internal static class ChorusTests
{
    public static async Task Run(Action<bool, string> check)
    {
        using var fake = new ChorusConnection();
        using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) };
        await session.Identify();
        int intents = 0, trials = 0; bool original = false, restored = false;
        Task Keep(string label, PhaserResearchSnapshot snapshot)
        {
            if (label == "original") original = true;
            if (label == "restored") restored = snapshot.Dsp.All(b => b == 0) && snapshot.Bank[8] == 7 && snapshot.Bank[11] == 0;
            if (label.EndsWith("-restored")) trials++;
            return Task.CompletedTask;
        }
        Task Intent(string kind, uint address, byte[] before, byte[] after)
        {
            if (!original || intents != fake.Writes) throw new Exception("Durable intent must precede each write.");
            intents++; return Task.CompletedTask;
        }
        await session.RunChorusResearch(true, Keep, Intent);
        check(trials == 20 && restored && intents == fake.Writes, "chorus fixed trials, MOD reload, durable intent and full restoration");
        check(fake.FlashWrites == 0, "chorus never writes flash");
        restored = false;
        await session.RunChorusResearch(false, Keep, Intent, transitions: true);
        check(restored && intents == fake.Writes, "chorus direct transitions reset custom mix and restore original state");
        var a = new byte[0xF70]; var b = a.ToArray(); b[0x68C] = 1;
        check(CubeSession.ChorusStableEquals(a, b), "chorus phase is dynamic");
        b[0x658] = 1;
        check(!CubeSession.ChorusStableEquals(a, b), "chorus does not inherit phaser dynamic exclusions");
        using var incompatible = new ChorusConnection { BadSignature = true };
        using var badSession = new CubeSession(incompatible);
        await badSession.Identify();
        bool rejected = false;
        try { await badSession.RunChorusResearch(true, (_, _) => Task.CompletedTask, (_, _, _, _) => Task.CompletedTask); }
        catch (IOException) { rejected = true; }
        check(rejected && incompatible.Writes == 0, "chorus rejects incompatible worker without writes");
        using var ackOnly = new ChorusConnection { IgnoreControl = true };
        using var ackSession = new CubeSession(ackOnly);
        await ackSession.Identify(); rejected = false;
        try { await ackSession.RunChorusResearch(true, (_, _) => Task.CompletedTask, (_, _, _, _) => Task.CompletedTask); }
        catch (IOException) { rejected = true; }
        check(rejected && ackOnly.Bank[11] == 0 && ackOnly.Dsp.All(x => x == 0), "chorus rejects ACK without control mutation and restores");
        using var moved = new ChorusConnection { MoveDuringControl = true };
        using var movedSession = new CubeSession(moved);
        await movedSession.Identify(); rejected = false;
        try { await movedSession.RunChorusResearch(true, (_, _) => Task.CompletedTask, (_, _, _, _) => Task.CompletedTask); }
        catch (IOException) { rejected = true; }
        check(rejected && moved.WritesAfterMove == 0, "chorus stops writes when physical context changes");
        using var lost = new ChorusConnection { DisconnectDuringControl = true };
        using var lostSession = new CubeSession(lost) { Timeout = TimeSpan.FromMilliseconds(80) };
        await lostSession.Identify(); bool preserved = false; rejected = false;
        try { await lostSession.RunChorusResearch(true, (_, _) => Task.CompletedTask, (kind, _, _, _) => { preserved |= kind == "0-depth-half"; return Task.CompletedTask; }); }
        catch (IOException) { rejected = true; }
        check(rejected && lostSession.IsFaulted && preserved, "chorus disconnect retains intent without claiming restoration");
    }
}

internal sealed class ChorusConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank = new byte[48], Dsp = new byte[0xF70], State = [0, 1];
    public int Writes, FlashWrites, WritesAfterMove;
    public bool BadSignature, IgnoreControl, MoveDuringControl, DisconnectDuringControl;
    private bool silent;
    private readonly Dictionary<uint, byte[]> code = JsonSerializer.Deserialize<Dictionary<string, string>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "chorus-live-code.json")))!
        .ToDictionary(p => uint.Parse(p.Key), p => Convert.FromHexString(p.Value));
    public ChorusConnection() { Bank[8] = 7; }
    void F(int offset, float value) => BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(offset), value);
    void Apply(int oldMod)
    {
        int mod = Bank[8];
        if (Bank[11] == 0 || mod is 7 or 8) { Dsp[0x63C] = 0; Dsp[0x694] = 0; return; }
        if (mod > 6)
        {
            Array.Clear(Dsp, 0x63C, 0x58); Dsp[0x63C] = 3; Dsp[0x63D] = 2;
            return;
        }
        if (Dsp[0x63C] == 0 || Dsp[0x63D] != 5)
        {
            Array.Clear(Dsp, 0x63C, 0x58); Dsp[0x63C] = 3; Dsp[0x63D] = 5;
            F(0x650, 1); F(0x66C, 1); F(0x688, 0.85f);
            oldMod = -1;
        }
        if (oldMod != mod) { F(0x644, 0.4f); F(0x648, mod == 3 ? 0.000031f : mod == 4 ? 0.000023f : throw new Exception("Unknown MOD")); }
    }
    public void Send(byte[] sysex)
    {
        if (silent) return;
        var p = UsbSysEx.Decode(sysex);
        if (p.Command == 0x11) { Reply(new(0x11, Convert.FromHexString("53494E434F2D437562654261627920202020200113141516171819"))); return; }
        uint address = BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1));
        int length = p.Payload[5] | p.Payload[6] << 8 | p.Payload[7] << 16; byte region = p.Payload[0];
        if (p.Command == 0x23)
        {
            byte[] data;
            if (region == 5) data = Bank.AsSpan((int)address, length).ToArray();
            else if (address == 0xFFFD827C) data = Bank.ToArray();
            else if (address == 0xFFFD8202) data = State.ToArray();
            else if (address < Dsp.Length) data = Dsp.AsSpan((int)address, length).ToArray();
            else
            {
                var range = code.First(k => address >= k.Key && (ulong)address + (uint)length <= (ulong)k.Key + (uint)k.Value.Length);
                data = range.Value.AsSpan((int)(address - range.Key), length).ToArray();
                if (BadSignature && address == unchecked(0x8326B4u - 0x8347F8u)) data[0] ^= 1;
            }
            Reply(new(0x23, p.Payload[..8].Concat(data).ToArray())); return;
        }
        if (p.Command != 0x22) throw new Exception("Unexpected command");
        Writes++; if (State[0] != 0) WritesAfterMove++;
        if (region == 5)
        {
            int oldMod = Bank[8]; int offset = (int)(address - 0x80000000u);
            Bank[offset] = p.Payload[8]; Apply(oldMod);
        }
        else if (region == 4)
        {
            bool control = Dsp[0x63C] == 3 && length <= 4;
            if (!(IgnoreControl && control)) p.Payload.AsSpan(8).CopyTo(Dsp.AsSpan((int)address));
            if (MoveDuringControl && control) State[0] = 1;
            if (DisconnectDuringControl && control) { silent = true; return; }
        }
        else FlashWrites++;
        Reply(new(0, [0]));
    }
    void Reply(CubePacket p) => Received?.Invoke(UsbSysEx.Encode(p));
    public void Dispose() { }
}
