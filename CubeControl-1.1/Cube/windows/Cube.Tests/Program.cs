using Cube.Protocol;
using System.Buffers.Binary;

int passed = 0;
void Check(bool result, string name) { if (!result) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
if (args.Contains("--delay-only")) { await DelayTests.Run(Check); Console.WriteLine($"{passed} delay checks passed."); return; }
if (args.Contains("--reverb-only")) { await ReverbTests.Run(Check); Console.WriteLine($"{passed} reverb checks passed."); return; }
if (args.Contains("--chorus-preview-only")) { await ChorusPreviewTests.Run(Check); Console.WriteLine($"{passed} chorus preview checks passed."); return; }
if (args.Contains("--chorus-only")) { await ChorusTests.Run(Check); Console.WriteLine($"{passed} chorus checks passed."); return; }
await PhaserTests.Run(Check);
if (args.Contains("--phaser-only")) { Console.WriteLine($"{passed} phaser checks passed."); return; }
void Reject(Action action, string name) { try { action(); } catch (Exception ex) when (ex is FormatException or ArgumentOutOfRangeException) { Check(true, name); return; } throw new Exception(name); }
Check(Convert.ToHexString(CubePacket.Identify().Encode()) == "005911000000FF", "identity envelope");
Check(Convert.ToHexString(UsbSysEx.Encode(CubePacket.Identify())) == "F0003245000000407FF7", "identity matches USB capture");
Check(Convert.ToHexString(CubePacket.Read(4, 0, 13).Encode()) == "00592308000004000000000D0000EE", "read envelope");
Check(Convert.ToHexString(CubePacket.Midi([0xB0, 7, 100]).Encode()) == "005915030000B00764E4", "checksum wrap");
for (int length = 0; length < 260; length++)
{
    byte[] data = Enumerable.Range(0, length).Select(i => (byte)i).ToArray();
    var packet = CubePacket.Write(5, 0x80000000, data);
    var roundtrip = UsbSysEx.Decode(UsbSysEx.Encode(packet));
    if (!packet.Payload.SequenceEqual(roundtrip.Payload)) throw new Exception("roundtrip " + length);
}
Check(true, "7-bit roundtrip across zero padding and all byte values");
var identityReply = UsbSysEx.Decode(Convert.FromHexString("F00032455801004029491C0D7A5465503A624A090A262C1E10204000011260040A152C5C40114307F7"));
Check(identityReply.Command == 0x11, "real identity response fixture");
var wire = UsbSysEx.Encode(CubePacket.Read(5, 0, 48));
for (int split = 0; split <= wire.Length; split++)
{
    var parser = new SysExAssembler();
    var results = parser.Feed(wire.AsSpan(0, split)).Concat(parser.Feed(wire.AsSpan(split))).ToArray();
    if (results.Length != 1 || !results[0].SequenceEqual(wire)) throw new Exception("fragment " + split);
}
Check(true, "all USB fragment boundaries");
var assembler = new SysExAssembler();
Check(assembler.Feed(wire.Concat(new byte[] { 0xF8 }).Concat(wire).ToArray()).Count == 2, "concatenation and realtime");
var damaged = wire.ToArray(); damaged[3] ^= 1;
Reject(() => UsbSysEx.Decode(damaged), "corrupt checksum rejected");
Reject(() => CubePacket.Read(5, uint.MaxValue, 2), "address overflow");
Reject(() => CubeBabyProfile.LiveAddress(3, 1), "invalid preset");
Check(CubeBabyProfile.LiveAddress(2, 7) == 0x80000027, "slot C address");
Check(MidiMessage.ControlChange(16, 7, 127).SequenceEqual(new byte[] { 0xBF, 7, 127 }), "MIDI channel conversion");

using (var fake = new FakeConnection())
using (var session = new CubeSession(fake))
{
    Check(await session.Identify() == "CubeBaby", "session identity");
    var banks = await Task.WhenAll(session.ReadBank(), session.ReadBank());
    Check(banks.All(b => b.Length == 48) && fake.MaximumActive == 1, "serialized requests and response matching");
    var edited = banks[0].ToArray(); edited[1] = 3;
    await session.SaveBank(banks[0], edited);
    Check(fake.Bank.SequenceEqual(edited), "save with readback");
    bool conflict = false;
    try { await session.SaveBank(banks[0], edited); } catch (InvalidOperationException) { conflict = true; }
    Check(conflict, "external bank conflict");
    await session.WriteLive(2, 7, 42);
    Check(fake.Bank[39] == 42, "live write confirmed through low-address bank");
}
using (var fake = new FakeConnection { Silent = true })
using (var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(30) })
{
    bool timeout = false, fault = false;
    try { await session.Identify(); } catch (TimeoutException) { timeout = true; }
    try { await session.Identify(); } catch (InvalidOperationException) { fault = true; }
    Check(timeout && fault, "timeout requires reconnect to reject stale responses");
}
await ExtendedTests.Run(Check);
await MusicalTests.Run(Check);
Console.WriteLine($"{passed} checks passed.");

