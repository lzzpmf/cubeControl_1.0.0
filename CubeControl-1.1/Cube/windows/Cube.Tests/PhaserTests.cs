using System.Buffers.Binary;
using System.IO;
using System.Text.Json;
using Cube.Protocol;

internal static class PhaserTests
{
    public static async Task Run(Action<bool, string> check)
    {
        const string identity = "53494E434F2D437562654261627920202020200113141516171819";
        check(PhaserPreset.Import(PhaserPreset.Export(identity, PhaserParameters.Default)).Parameters == PhaserParameters.Default, "phaser file roundtrip");
        foreach (var p in new[] { PhaserParameters.Default with { Step = float.NaN }, PhaserParameters.Default with { Lower = 3100 },
            PhaserParameters.Default with { Level = 0.65f }, PhaserParameters.Default with { Sections = 0 }, PhaserParameters.Default with { Step = 1 } })
        {
            bool rejected = false;
            try { p.Validate(); } catch (ArgumentOutOfRangeException) { rejected = true; }
            check(rejected, "phaser rejects invalid or ungrounded range");
        }
        bool badFile = false;
        try { PhaserPreset.Import(PhaserPreset.Export(identity, PhaserParameters.Default).Replace(PhaserParameters.Mapping, "other")); }
        catch (FormatException) { badFile = true; }
        check(badFile, "phaser rejects incompatible mapping");
        using var fake = new PhaserConnection();
        using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) };
        await session.Identify();
        PhaserRecovery? saved = null;
        Task Keep(PhaserRecovery r) { saved = r; return Task.CompletedTask; }
        var recovery = await session.BeginPhaserPreview(0, Keep);
        check(saved != null && fake.Writes == 0, "phaser backup precedes mutation");
        var original = fake.Dsp.ToArray();
        var target = recovery.Target with { Lower = 1000, Upper = 2000, Level = 0.32f, Sections = 1 };
        recovery = await session.ApplyPhaserPreview(recovery, target, Keep);
        check(PhaserParameters.Read(fake.Dsp) == target && fake.Bank[8] == 12, "phaser controls independent of MOD");
        check(PhaserParameters.StableEquals(original, fake.Dsp, true) && fake.Writes == 4, "phaser preserves all unrelated bytes");
        await session.RestorePhaserPreview(recovery);
        check(PhaserParameters.StableEquals(original, fake.Dsp), "phaser restore verified");
        recovery = recovery with { Previous = PhaserParameters.Default, Target = PhaserParameters.Default };
        fake.IgnoreWrites = true;
        bool noWrite = false;
        try { await session.ApplyPhaserPreview(recovery, target, Keep); } catch (IOException) { noWrite = true; }
        check(noWrite && saved!.Target == target, "phaser ACK without mutation rejected and intent retained");
        fake.IgnoreWrites = false;
        await session.RestorePhaserPreview(saved!);
        recovery = await session.ApplyPhaserPreview(recovery, target, Keep);
        fake.SetParameters(PhaserParameters.Default);
        int writes = fake.Writes;
        bool reloaded = false;
        try { await session.ApplyPhaserPreview(recovery, target with { Level = 0.2f }, Keep); } catch (IOException) { reloaded = true; }
        check(reloaded && fake.Writes == writes, "phaser detects reload before writing");
        await session.RestorePhaserPreview(recovery);
        fake.State[0] = 1;
        bool changed = false;
        try { await session.RestorePhaserPreview(recovery); } catch (IOException) { changed = true; }
        check(changed && fake.Writes == writes, "phaser refuses restoring a different physical preset");
        fake.State[0] = 0;
        recovery = await session.BeginPhaserPreview(0, Keep);
        fake.FailAfterWrites = fake.Writes + 1;
        bool lost = false;
        try { await session.ApplyPhaserPreview(recovery, target, Keep); } catch (TimeoutException) { lost = true; }
        check(lost && session.IsFaulted && saved!.Target == target, "phaser disconnect during write retains partial transaction");
        fake.FailAfterWrites = int.MaxValue; fake.Silent = false;
        using var reconnected = new CubeSession(fake);
        await reconnected.Identify();
        await reconnected.RestorePhaserPreview(saved!);
        check(PhaserParameters.StableEquals(original, fake.Dsp), "phaser explicit recovery after reconnect");
        fake.BadSignature = true;
        using var incompatible = new CubeSession(fake);
        await incompatible.Identify();
        writes = fake.Writes;
        bool unsupported = false;
        try { await incompatible.BeginPhaserPreview(0, Keep); } catch (NotSupportedException) { unsupported = true; }
        check(unsupported && writes == fake.Writes, "phaser incompatible firmware never written");
        check(!incompatible.IsFaulted && (await incompatible.ReadBank()).SequenceEqual(fake.Bank), "unsupported creator leaves normal preset reads available");
        fake.BadSignature = false;
        fake.BadReadHeader = true;
        using var inconsistent = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(80) };
        await inconsistent.Identify();
        bool timedOut = false;
        try { await inconsistent.ReadPhaserResearch(); } catch (TimeoutException) { timedOut = true; }
        check(timedOut && inconsistent.IsFaulted, "phaser mismatched memory response faults session");
    }
}

internal sealed class PhaserConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank = new byte[48];
    public byte[] Dsp = new byte[0xF70];
    public byte[] State = [0, 1];
    public int Writes;
    public int FailAfterWrites = int.MaxValue;
    public bool Silent, IgnoreWrites, BadSignature, BadReadHeader;
    private readonly Dictionary<uint, byte[]> code;
    public PhaserConnection()
    {
        code = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "phaser-live-code.json")))!
            .ToDictionary(p => uint.Parse(p.Key), p => Convert.FromHexString(p.Value));
        code.TryAdd(0xFFFFD958, new byte[12]); // Unknown B reader on this legacy fixture.
        Bank[8] = 12; Bank[11] = 1;
        Dsp[0x63C] = 3; Dsp[0x63D] = 2; Dsp[0x640] = 1;
        BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(0x648), 5000);
        SetParameters(PhaserParameters.Default);
    }
    public void SetParameters(PhaserParameters p)
    {
        Dsp[0x641] = (byte)(p.Sections - 1);
        foreach (var (offset, value) in new[] { (0x644, p.Step), (0x64C, p.Lower), (0x650, p.Upper), (0x654, p.Level) })
            BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(offset), value);
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
                byte[] data;
                if (p.Payload[0] == 0) data = new byte[length];
                else if (p.Payload[0] == 5 || address == 0xFFFD827C) data = Bank.AsSpan(0, length).ToArray();
                else if (address == 0xFFFD8202) data = State.ToArray();
                else if (address < Dsp.Length) data = Dsp.AsSpan((int)address, length).ToArray();
                else
                {
                    var range = code.First(x => address >= x.Key && (ulong)address + (uint)length <= (ulong)x.Key + (uint)x.Value.Length);
                    data = range.Value.AsSpan((int)(address - range.Key), length).ToArray();
                    if (BadSignature) data[0] ^= 1;
                }
                var payload = p.Payload.Concat(data).ToArray();
                if (BadReadHeader) payload[1] ^= 1;
                reply = new(0x23, payload);
            }
            else
            {
                if (p.Command != 0x22 || p.Payload[0] != 4 || !PhaserParameters.IsControlOffset((int)address))
                    throw new Exception("Unexpected mutation in phaser test");
                Writes++;
                if (!IgnoreWrites) p.Payload.AsSpan(8).CopyTo(Dsp.AsSpan((int)address));
                if (Writes >= FailAfterWrites) { Silent = true; return; }
                reply = new(0, [0]);
            }
        }
        Received?.Invoke(UsbSysEx.Encode(reply));
    }
    public void Dispose() { }
}
