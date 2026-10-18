using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cube.Protocol;
using Cube.Qa;
using static Cube.Qa.QaContext;

namespace Cube.Qa;

internal static class QaProtocol
{
    private static void Check(bool condition, string message) => QaContext.Check(condition, message);
    private static QaCase Test(string id, string name, string batteries, Func<QaContext, Task> run) => new(id, name, batteries.Split(','), run);
    public static Task<int> Run(string[] args)
    {
        if (args.Length == 3 && args[1] == "--all")
            args = ["--qa", "--batteries", string.Join(",", Cases().SelectMany(c => c.Batteries).Distinct()), "--results", args[2]];
        return QaRunner.Run("protocol", args, Cases());
    }
    private static QaCase[] Cases() =>
    [
        Test("wire-roundtrip", "SysEx round-trip, fragment boundaries and checksum", "QA-MIDI-001", c =>
        {
            for (int n = 0; n <= 512; n++)
            {
                byte[] payload = Enumerable.Range(0, n).Select(i => (byte)i).ToArray();
                var frame = UsbSysEx.Encode(new CubePacket(0x22, payload));
                Check(UsbSysEx.Decode(frame).Payload.SequenceEqual(payload), $"Payload length {n}");
            }
            var wire = UsbSysEx.Encode(CubePacket.Read(5, 0, 48));
            for (int split = 0; split <= wire.Length; split++)
            {
                var parser = new SysExAssembler();
                var packets = parser.Feed(wire.AsSpan(0, split)).Concat(parser.Feed(wire.AsSpan(split))).ToArray();
                Check(packets.Length == 1 && packets[0].SequenceEqual(wire), $"Fragment {split}");
            }
            var bad = wire.ToArray(); bad[3] ^= 1;
            Reject<FormatException>(() => UsbSysEx.Decode(bad));
            Check(new SysExAssembler().Feed(wire.Concat(new byte[] { 0xF8 }).Concat(wire).ToArray()).Count == 2, "Realtime/concatenation");
            c.Evidence("wire.json", new { Frame = Convert.ToHexString(wire), Lengths = 513, Fragments = wire.Length + 1 });
            return Task.CompletedTask;
        }),
        Test("session-timeout", "Timeout faults session; late reply cannot revive it", "QA-MIDI-001,QA-DEV-002,QA-RES-001", async c =>
        {
            using var fake = new QaConnection { Silent = true };
            using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(40) };
            await Reject<TimeoutException>(() => session.Identify());
            fake.Emit(new(0x11, "CubeBaby"u8.ToArray()));
            await Reject<InvalidOperationException>(() => session.Identify());
            Check(session.IsFaulted && fake.Sent.Count == 1 && fake.Writes == 0, "Late reply must not permit another request");
            c.Evidence("tx.json", fake.Sent);
        }),
        Test("session-mismatch", "Wrong readback region is rejected", "QA-MIDI-001", async c =>
        {
            using var fake = new QaConnection(); using var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(40) };
            await session.Identify();
            fake.ReplyFilter = p => { if (p.Command == 0x23) p.Payload[0] = 4; return p; };
            await Reject<TimeoutException>(() => session.ReadBank());
            Check(session.IsFaulted && fake.Writes == 0, "Wrong readback cannot authorize writes");
            c.Evidence("tx.json", fake.Sent);
        }),
        Test("bank-boundaries", "All preset fields and reserved bytes", "QA-INP-001,QA-PRE-001", c =>
        {
            foreach (var p in CubeBabyProfile.Parameters) foreach (int slot in Enumerable.Range(0, 3))
            {
                var bank = new byte[48]; bank[slot * 16 + p.Offset] = (byte)p.Maximum;
                bank[slot * 16 + 13] = 255; CubeBabyProfile.ValidateBank(bank);
                bank[slot * 16 + p.Offset]++;
                Reject<FormatException>(() => CubeBabyProfile.ValidateBank(bank));
            }
            Reject<FormatException>(() => CubeBabyProfile.ValidateBank(new byte[47]));
            Reject<ArgumentOutOfRangeException>(() => CubeBabyProfile.LiveAddress(3, 0));
            return Task.CompletedTask;
        }),
        Test("bank-conflict", "Live/save preserves reserved bytes and refuses conflicts", "QA-PRE-001,QA-PRE-002,QA-PRE-003", async c =>
        {
            using var fake = new QaConnection(); fake.Bank[13] = 0xBA; fake.Bank[47] = 0xDC;
            using var session = new CubeSession(fake); await session.Identify();
            var bank = await session.ReadBank();
            var edited = await session.WriteLiveChecked(2, 7, 42, bank);
            await session.SaveBank(edited, edited);
            Check(fake.Bank[39] == 42 && fake.Bank[13] == 0xBA && fake.Bank[47] == 0xDC, "Slot/reserved mismatch");
            var invalid = edited.ToArray(); invalid[13] = 0;
            await Reject<ArgumentException>(() => session.SaveBank(edited, invalid));
            int writes = fake.Writes; fake.Bank[1] = 2;
            await Reject<InvalidOperationException>(() => session.SaveBank(edited, edited));
            Check(fake.Writes == writes, "Conflict wrote to bank"); c.Evidence("tx.json", fake.Sent);
        }),
        Test("queue-coalescing", "Barrier-controlled coalescing retains original slot", "QA-CONC-002,QA-PRE-001", async c =>
        {
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var sent = new List<(ParameterKey Key, int Value)>(); int active = 0, maximum = 0;
            var queue = new LiveEditQueue(async (key, value) =>
            { maximum = Math.Max(maximum, ++active); sent.Add((key, value)); if (sent.Count == 1) { entered.SetResult(); await release.Task; } active--; });
            queue.Enqueue(new(0, 5), 1);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
            try
            {
                for (int i = 2; i <= 100; i++) queue.Enqueue(new(0, 5), i);
                queue.Enqueue(new(2, 5), 17);
                Check(!queue.Flush().IsCompleted, "Flush ignored in-flight write");
            }
            finally { release.TrySetResult(); }
            await queue.Flush();
            Check(maximum == 1 && sent.Count == 3 && sent[1] == (new ParameterKey(0, 5), 100) && sent[2] == (new ParameterKey(2, 5), 17), "Coalescing/target mismatch");
            c.Evidence("sequence.json", sent.Select(s => new { s.Key.Slot, s.Key.Offset, s.Value }));
        }),
        Test("queue-failure", "Failure discards pending changes and stops queue", "QA-CONC-002,QA-RES-001", async c =>
        {
            int attempted = 0, failed = 0;
            var queue = new LiveEditQueue((_, _) => { attempted++; throw new IOException("injected"); });
            queue.Failed += _ => failed++;
            queue.Enqueue(new(0, 1), 1); queue.Enqueue(new(1, 1), 2);
            await Reject<IOException>(queue.Flush);
            Reject<InvalidOperationException>(() => queue.Enqueue(new(2, 1), 3));
            Check(attempted == 1 && failed == 1, "Repeated send after fault");
        }),
        Test("backup-integrity", "IR manifest rejects traversal and damaged binary", "QA-FILE-001,QA-PERS-002,QA-IR-003", async c =>
        {
            byte[] sector = Enumerable.Range(0, 4096).Select(i => (byte)i).ToArray();
            string path = await IrBackup.Save(c.DirectoryPath, "CubeBaby", "1234", 7, sector);
            var (info, actual) = await IrBackup.Load(path); Check(actual.SequenceEqual(sector), "Backup changed bytes");
            foreach (var name in new[] { "../outside.bin", "C:\\outside.bin", "sub/other.bin" })
            { await File.WriteAllTextAsync(path, JsonSerializer.Serialize(info with { BinaryFile = name })); await Reject<FormatException>(() => IrBackup.Load(path)); }
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(info));
            await File.WriteAllBytesAsync(c.FilePath(info.BinaryFile), new byte[4096]);
            await Reject<FormatException>(() => IrBackup.Load(path));
        }),
        Test("ram-recovery", "IR recovery round-trip and hash rejection", "QA-PERS-002,QA-RES-002", async c =>
        {
            string path = c.FilePath("pending-ir-preview.json"); byte[] bytes = new byte[2052]; bytes[8] = 42;
            await IrRamBackup.Save(path, "1234", new(3, bytes));
            var loaded = await IrRamBackup.Load(path);
            Check(loaded.Snapshot.Flag == 3 && loaded.Snapshot.Payload.SequenceEqual(bytes), "RAM snapshot mismatch");
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!; json["Sha256"] = "bad";
            await File.WriteAllTextAsync(path, json.ToJsonString());
            await Reject<FormatException>(() => IrRamBackup.Load(path));
            Check(File.Exists(path), "Corrupt snapshot was removed");
        }),
        Test("ir-transaction", "Backup failure prevents erase; readback failure is reported", "QA-IR-003,QA-RES-002", async c =>
        {
            using var fake = new QaConnection(); using var session = new CubeSession(fake); await session.Identify();
            byte[] original = await session.ReadIrSlot(7), target = Enumerable.Repeat((byte)0x25, 4096).ToArray();
            await Reject<IOException>(() => session.ReplaceIr(7, original, target, _ => throw new IOException("backup denied")));
            Check(fake.Erases == 0 && fake.Writes == 0, "Erase before backup completed");
            byte[]? backup = null;
            await session.ReplaceIr(7, original, target, data => { backup = data.ToArray(); return Task.CompletedTask; });
            Check(backup!.SequenceEqual(original) && (await session.ReadIrSlot(7)).SequenceEqual(target), "Transaction/readback");
            Check(fake.Flash.AsSpan(0, 0x10000).IndexOfAnyExcept((byte)0) < 0, "Other sectors changed");
            fake.CorruptReadback = true;
            // A fresh transport tests corruption occurring after erase, not a pre-existing conflict.
            using var second = new QaConnection { CorruptReadback = true }; using var broken = new CubeSession(second);
            await broken.Identify(); await Reject<IOException>(() => broken.ReplaceIr(0, new byte[4096], target, _ => Task.CompletedTask));
            Check(second.Erases == 1, "Fault injection did not reach write verification");
            c.Evidence("tx.json", fake.Sent);
        }),
        Test("amp-validation", "Nine factory amps and finite/stable coefficient guards", "QA-AMP-001,QA-DSP-001,QA-FILE-001", c =>
        {
            using var stream = typeof(QaProtocol).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!;
            var amp = new byte[4096];
            for (int i = 0; i < 9; i++) { stream.ReadExactly(amp); AmpImage.Validate(amp); }
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, 101f, -1f })
            { var bad = amp.ToArray(); AmpImage.Set(bad, 0x0C, value); Reject<ArgumentException>(() => AmpImage.Validate(bad)); }
            var unstable = amp.ToArray(); AmpImage.Set(unstable, 0x5E8 + 16, 1); Reject<ArgumentException>(() => AmpImage.Validate(unstable));
            Reject<ArgumentException>(() => AmpImage.Validate(new byte[4095]));
            return Task.CompletedTask;
        }),
        Test("amp-transaction", "Amp flash transaction preserves other sectors", "QA-AMP-002", async c =>
        {
            using var stream = typeof(QaProtocol).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!;
            var target = new byte[4096]; stream.ReadExactly(target);
            using var fake = new QaConnection(); using var session = new CubeSession(fake); await session.Identify();
            await Reject<IOException>(() => session.ReplaceAmp(8, new byte[4096], target, _ => throw new IOException("backup")));
            Check(fake.Erases == 0, "Amp erased before backup");
            await session.ReplaceAmp(8, new byte[4096], target, data => { File.WriteAllBytes(c.FilePath("amp-before.bin"), data); return Task.CompletedTask; });
            Check((await session.ReadAmpSlot(8)).SequenceEqual(target), "Amp readback mismatch");
            Check(fake.Flash.AsSpan(0, 8 * 4096).IndexOfAnyExcept((byte)0) < 0 && fake.Flash.AsSpan(9 * 4096).IndexOfAnyExcept((byte)0) < 0, "Adjacent sector changed");
        }),
        Test("effect-files", "Effect file round-trips, culture and malformed JSON", "QA-FILE-001,QA-DSP-001", c =>
        {
            string identity = Convert.ToHexString("SINCO-CubeBaby"u8);
            var delay = new DelayParameters(2400, .5f, .05f, .001f, .5f); var reverb = new ReverbParameters(1, .4f, 8, 40, 5);
            Check(ChorusPreset.Import(ChorusPreset.Export(identity, ChorusParameters.Default)).Parameters == ChorusParameters.Default, "Chorus round-trip");
            Check(PhaserPreset.Import(PhaserPreset.Export(identity, PhaserParameters.Default)).Parameters == PhaserParameters.Default, "Phaser round-trip");
            Check(DelayPreset.Import(DelayPreset.Export(identity, delay)).Parameters == delay, "Delay round-trip");
            Check(ReverbPreset.Import(ReverbPreset.Export(identity, reverb)).Parameters == reverb, "Reverb round-trip");
            Reject<ArgumentOutOfRangeException>(() => (delay with { Feedback = float.NaN }).Validate());
            Reject<ArgumentOutOfRangeException>(() => (PhaserParameters.Default with { Lower = 3001 }).Validate());
            Reject<ArgumentOutOfRangeException>(() => (reverb with { Wet = float.PositiveInfinity }).Validate());
            Reject<JsonException>(() => ChorusPreset.Import("{"));
            Reject<FormatException>(() => ChorusPreset.Import("null"));
            var unknown = JsonNode.Parse(ChorusPreset.Export(identity, ChorusParameters.Default))!; unknown["Unknown"] = 1;
            Reject<JsonException>(() => ChorusPreset.Import(unknown.ToJsonString()));
            return Task.CompletedTask;
        }),
        Test("ir-wav", "PCM/float WAV channels, resampling and invalid input", "QA-IR-001,QA-FILE-001", c =>
        {
            foreach (int bits in new[] { 16, 24, 32 })
            {
                var audio = IrAudio.Decode(Wave(1, bits, 1, 48000, [-.5f, 0, .25f]));
                Check(audio.Prepare().Length == 512 && audio.Prepare()[0] == -.5f && audio.Prepare()[2] == .25f && audio.Prepare().Skip(3).All(v => v == 0), $"PCM {bits}");
            }
            var stereo = IrAudio.Decode(Wave(3, 32, 2, 24000, [1, -.5f, 0, .5f]));
            Check(stereo.Samples.SequenceEqual(new[] { .25f, .25f }) && stereo.Prepare()[4] == 0, "Stereo/resampling");
            var loud = IrAudio.Decode(Wave(3, 32, 1, 48000, Enumerable.Repeat(1.5f, 600).ToArray()));
            Check(loud.Prepare().All(v => v == 1.5f), "Unexpected normalization");
            Reject<FormatException>(() => IrAudio.Decode(Wave(3, 32, 1, 48000, [float.NaN])));
            Reject<FormatException>(() => IrAudio.Decode(new byte[44]));
            Reject<FormatException>(() => IrAudio.Decode(Wave(1, 16, 1, 48000, [0])[..^1]));
            c.Evidence("prepared.json", loud.Prepare()); return Task.CompletedTask;
        }),
        Test("preferences", "Atomic preference round-trip, invalid version and legacy migration", "QA-PERS-001,QA-CHAIN-002", c =>
        {
            string path = c.FilePath("visual-preferences-v1.json");
            new VisualPreferences { AutomaticCabinetMatching = true, Matches = { [0] = "content-hash", [1] = "pack-bfi-cab-1" }, IrNames = { ["hash"] = "São Paulo" } }.Save(path);
            var actual = VisualPreferences.Load(path);
            Check(actual.AutomaticCabinetMatching && actual.Matches[0] == "content-hash" && !actual.Matches.ContainsKey(1) && actual.IrNames["hash"] == "São Paulo", "Migration/round-trip");
            Check(!File.Exists(path + ".tmp"), "Temporary file left after success");
            var node = JsonNode.Parse(File.ReadAllText(path))!; node["Version"] = 99; File.WriteAllText(path, node.ToJsonString());
            Reject<FormatException>(() => VisualPreferences.Load(path)); return Task.CompletedTask;
        }),
        Test("cabinet-identity", "Match follows content only after complete inventory", "QA-CHAIN-002", c =>
        {
            var inventory = new CabinetInventory(); byte[] a = new byte[4096]; a[0] = 1;
            string hash = CabinetInventory.Hash(a); inventory.Update(0, a);
            Check(inventory.Resolve(hash) == null, "Incomplete inventory resolved match");
            for (int i = 1; i < 8; i++) inventory.Update(i, new byte[4096]);
            Check(inventory.Resolve(hash)?.Slot == 0, "Initial content match");
            inventory.Update(0, new byte[4096]); inventory.Update(7, a);
            Check(inventory.Resolve(hash)?.Slot == 7, "Match did not follow content"); return Task.CompletedTask;
        }),
        Test("effects-chorus", "Existing protocol chorus fixtures", "QA-DSP-001,QA-DSP-002", _ => ChorusTests.Run(Check)),
        Test("effects-phaser", "Existing protocol phaser fixtures", "QA-DSP-001,QA-DSP-002", _ => PhaserTests.Run(Check)),
        Test("effects-delay", "Existing protocol delay fixtures", "QA-DSP-001,QA-DSP-002", _ => DelayTests.Run(Check)),
        Test("effects-reverb", "Existing protocol reverb fixtures", "QA-DSP-001,QA-DSP-002", _ => ReverbTests.Run(Check)),
        Test("effects-firmware-b", "Existing firmware B preview signatures (not updater)", "QA-DSP-002,QA-REG-001", _ => EffectsFirmwareBTests.Run(Check)),
        Test("musical-mapping", "Existing musical controls and shared gates", "QA-CHAIN-001", _ => MusicalTests.Run(Check))
    ];

    private static byte[] Wave(short format, int bits, short channels, int rate, float[] samples)
    {
        using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
        int bytes = bits / 8; w.Write("RIFF"u8); w.Write(36 + samples.Length * bytes); w.Write("WAVEfmt "u8);
        w.Write(16); w.Write(format); w.Write(channels); w.Write(rate); w.Write(rate * channels * bytes);
        w.Write((short)(channels * bytes)); w.Write((short)bits); w.Write("data"u8); w.Write(samples.Length * bytes);
        foreach (float sample in samples)
        {
            if (format == 3) w.Write(sample);
            else { int value = (int)(sample * Math.Pow(2, bits - 1)); for (int i = 0; i < bytes; i++) w.Write((byte)(value >> (8 * i))); }
        }
        return stream.ToArray();
    }
}
