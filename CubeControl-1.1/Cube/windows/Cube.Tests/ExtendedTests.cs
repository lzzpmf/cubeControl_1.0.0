using Cube.Protocol;
using System.Buffers.Binary;
using System.Text.Json;

static class ExtendedTests
{
    public static async Task Run(Action<bool, string> check)
    {
        async Task Reject<T>(Func<Task> action, string name) where T : Exception
        {
            try { await action(); } catch (T) { check(true, name); return; }
            throw new Exception(name + " was accepted");
        }
        var amp = CubeBabyProfile.Parameters[0]; var cab = CubeBabyProfile.Parameters[9];
        check(amp.Options!.Length == 9 && amp.Maximum == 8 && amp.Options[0].Contains("Power-Zone") && amp.Options[8].Contains("Mr Smith"), "nine named amps map to zero-based indices");
        check(cab.Options!.Length == 9 && cab.Options[0] == "Desligado" && cab.Maximum == 8, "cabinet off plus eight slots");
        check(Enumerable.Range(0, 16).All(v => CubeBabyProfile.Display(8, v).Contains(v < 7 ? "Chorus" : v < 9 ? "Desligado" : "Phaser")), "complete modulation mapping preserves both off values");
        check(CubeBabyProfile.Parameters.Where(p => p.Maximum == 1).Select(p => p.Offset).SequenceEqual(new[] { 10, 11, 12 }), "binary fields keep original addresses");
        check(CubeBabyProfile.Display(3, 0) == "Desligado" && CubeBabyProfile.Display(7, 0) == "Desligado", "reverb and delay mix minimum labels");
        check(Enumerable.Range(0, 8).All(i => CubeSession.IrAddress(i) == 0x69000 + i * 4096), "all eight IR addresses");
        await Reject<ArgumentOutOfRangeException>(() => Task.FromResult(CubeSession.IrAddress(8)), "slot range guard");

        foreach (int bits in new[] { 16, 24, 32 })
        {
            var decoded = IrAudio.Decode(Wav(1, bits, 1, 48000, [-.5f, 0, .25f]));
            var prepared = decoded.Prepare();
            check(prepared.Length == 512 && prepared[0] == -.5f && prepared[2] == .25f && prepared.Skip(3).All(v => v == 0), $"PCM {bits} sign, amplitude and zero padding");
        }
        var stereo = IrAudio.Decode(Wav(3, 32, 2, 24000, [1, -.5f, 0, .5f]));
        check(IrAudio.Decode(Wav(1, 24, 1, 48000, [-.5f, .25f], extensible: true)).Samples.SequenceEqual(new[] { -.5f, .25f }), "WAVE_FORMAT_EXTENSIBLE PCM accepted");
        check(IrAudio.Decode(Wav(3, 32, 1, 48000, [.125f], extensible: true)).Samples[0] == .125f, "WAVE_FORMAT_EXTENSIBLE float accepted");
        check(stereo.Samples.SequenceEqual(new[] { .25f, .25f }) && stereo.Prepare().Take(4).All(v => v == .25f) && stereo.Prepare()[4] == 0, "float stereo mean and 24-to-48 kHz conversion");
        var loud = IrAudio.Decode(Wav(3, 32, 1, 48000, Enumerable.Repeat(1.5f, 600).ToArray()));
        check(loud.Prepare().Length == 512 && loud.Prepare().All(v => v == 1.5f) && loud.Conversion.Contains("88"), "truncation explicit; no silent normalization or clipping");
        await Reject<FormatException>(() => Task.FromResult(IrAudio.Decode(new byte[44])), "invalid WAV header");
        await Reject<FormatException>(() => Task.FromResult(IrAudio.Decode(Wav(3, 32, 1, 48000, [float.NaN]))), "NaN samples rejected");
        await Reject<FormatException>(() => Task.FromResult(IrAudio.Decode(Wav(1, 16, 3, 48000, [0, 0, 0]))), "unsupported channels rejected");
        await Reject<FormatException>(() => Task.FromResult(IrAudio.Decode(Wav(1, 16, 1, 48000, [0])[..^1])), "truncated chunk rejected");
        check(IrAudio.DistanceValue(0) == 0 && IrAudio.DistanceValue(37) == .37f && IrAudio.DistanceValue(100) == 1, "distance matches original CubeSuite percentage conversion");
        check(loud.LivePayload(50).Length == 2052 && loud.Sector(50).Length == 4096 && BinaryPrimitives.ReadSingleLittleEndian(loud.Sector(50).AsSpan(4)) == .5f, "RAM and flash format layout");

        var calls = new List<(ParameterKey Key, int Value, long Time)>(); int active = 0, maximum = 0;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var queue = new LiveEditQueue(async (key, value) =>
        {
            maximum = Math.Max(maximum, ++active); calls.Add((key, value, Environment.TickCount64));
            if (calls.Count == 1) { started.SetResult(); await release.Task; }
            await Task.Delay(10); active--;
        });
        for (int i = 0; i < 50; i++) queue.Enqueue(new(0, 5), i);
        await started.Task;
        for (int i = 50; i < 100; i++) queue.Enqueue(new(0, 5), i);
        queue.Enqueue(new(2, 5), 17); // Changing selected preset must not retarget A.
        var flush = queue.Flush(); check(!flush.IsCompleted, "save flush waits for active transmission");
        release.SetResult(); await flush;
        check(calls.Count == 3 && calls[0].Value == 49 && calls[1].Key.Slot == 0 && calls[1].Value == 99 && calls[2].Key.Slot == 2 && calls[2].Value == 17, "fast gesture coalesces and sends final values to original presets");
        check(maximum == 1, "one transaction at a time without an artificial pacing requirement");
        int attempted = 0, failed = 0;
        var broken = new LiveEditQueue((_, _) => { attempted++; throw new IOException("disconnected"); });
        broken.Failed += _ => failed++;
        broken.Enqueue(new(0, 1), 2); broken.Enqueue(new(1, 1), 3);
        await Reject<IOException>(broken.Flush, "queue failure surfaces to save");
        check(attempted == 1 && failed == 1, "failure discards pending edits");
        await Reject<InvalidOperationException>(() => { broken.Enqueue(new(2, 1), 4); return Task.CompletedTask; }, "failed queue rejects future edits");

        using (var fake = new FakeConnection()) using (var session = new CubeSession(fake))
        {
            fake.Bank[13] = 0xB7; fake.Bank[30] = 0xE2; fake.Bank[47] = 0xF1;
            await session.Identify(); var initial = await session.ReadBank();
            var changed = await session.WriteLiveChecked(1, 8, 7, initial);
            await session.SaveBank(changed, changed);
            check(fake.Bank[13] == 0xB7 && fake.Bank[30] == 0xE2 && fake.Bank[47] == 0xF1 && fake.Bank[24] == 7, "live and save preserve reserved bytes and modulation 7");
            var invalid = changed.ToArray(); invalid[13]++;
            await Reject<ArgumentException>(() => session.SaveBank(changed, invalid), "reserved byte mutation rejected");
            fake.Bank[1] = 1; int writes = fake.Writes;
            await Reject<InvalidOperationException>(() => session.WriteLiveChecked(0, 2, 5, changed), "external conflict detected before live write");
            check(fake.Writes == writes, "conflict does not overwrite external change");
        }

        string folder = Path.Combine(Path.GetTempPath(), "cube-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            byte[] sector = loud.Sector(20);
            string path = await IrBackup.Save(folder, "SINCO-CubeBaby", "1234", 7, sector);
            var (info, data) = await IrBackup.Load(path);
            check(data.SequenceEqual(sector) && info.Slot == 7 && info.IdentityHex == "1234", "exact binary backup with device, slot and checksum");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(info with { BinaryFile = "../outside.bin" }));
            await Reject<FormatException>(() => IrBackup.Load(path), "backup path traversal rejected");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(info));
            await File.WriteAllBytesAsync(Path.Combine(folder, info.BinaryFile), new byte[4096]);
            await Reject<FormatException>(() => IrBackup.Load(path), "corrupt backup rejected before device access");
            string ramPath = Path.Combine(folder, "pending-ram.json");
            await IrRamBackup.Save(ramPath, "1234", new(3, loud.LivePayload(70)));
            var restored = await IrRamBackup.Load(ramPath);
            check(restored.IdentityHex == "1234" && restored.Snapshot.Flag == 3 && restored.Snapshot.Payload.SequenceEqual(loud.LivePayload(70)), "RAM snapshot survives process restart with exact bytes");
            var ramRecord = JsonSerializer.Deserialize<IrRamBackup>(await File.ReadAllTextAsync(ramPath))!;
            await File.WriteAllTextAsync(ramPath, JsonSerializer.Serialize(ramRecord with { Sha256 = "bad" }));
            await Reject<FormatException>(() => IrRamBackup.Load(ramPath), "corrupt RAM recovery snapshot rejected");
        }
        finally { Directory.Delete(folder, true); }

        using (var fake = new FakeConnection()) using (var session = new CubeSession(fake))
        {
            await session.Identify(); fake.Ram[0x764] = 3;
            var original = await session.ReadIrRam();
            await session.ApplyIrRam(new(1, loud.LivePayload(40)));
            check(fake.Ram.AsSpan(0x768, 2052).SequenceEqual(loud.LivePayload(40)), "IR RAM preview verified");
            await session.ApplyIrRam(original);
            check(fake.Ram[0x764] == 3 && fake.Ram.AsSpan(0x768, 2052).SequenceEqual(original.Payload), "IR preview restores original flag and data");
            await Reject<IOException>(() => session.ReplaceIr(7, new byte[4096], loud.Sector(50), _ => throw new IOException("disk full")), "backup failure aborts flash operation");
            check(fake.Erases == 0 && fake.FlashWrites == 0, "backup failure never erases");
            await Reject<InvalidOperationException>(() => session.ReplaceIr(7, loud.Sector(50), loud.Sector(40), _ => Task.CompletedTask), "external slot conflict aborts before erase");
            using var cancel = new CancellationTokenSource();
            await Reject<OperationCanceledException>(() => session.ReplaceIr(7, new byte[4096], loud.Sector(50), _ => { cancel.Cancel(); return Task.CompletedTask; }, ct: cancel.Token), "cancellation before erase leaves sector intact");
            check(fake.Erases == 0, "conflict and cancellation never erase");
            bool backed = false;
            var transfer = session.ReplaceIr(7, new byte[4096], loud.Sector(50), _ => { backed = true; return Task.CompletedTask; });
            var live = session.WriteLive(0, 1, 2);
            await Task.WhenAll(transfer, live);
            check(backed && fake.Flash.AsSpan(7 * 4096, 4096).SequenceEqual(loud.Sector(50)) && fake.FlashWrites == 32 && fake.MaxRead <= 113 && fake.MaxWrite <= 128, "full-sector write and verification with bounded chunks");
            check(fake.MaximumActive == 1 && fake.Bank[1] == 2, "IR transfer and live writes share exclusive session gate");
            fake.CorruptFlashReadback = true;
            // Reset count so preflight reads are correct; corruption starts after the next erase.
            fake.Erases = 0;
            await Reject<IOException>(() => session.ReplaceIr(7, loud.Sector(50), loud.Sector(60), _ => Task.CompletedTask), "ACK alone cannot pass divergent readback");
        }
        using (var fake = new FakeConnection { DisconnectOnFlashWrite = true })
        using (var session = new CubeSession(fake) { Timeout = TimeSpan.FromMilliseconds(50) })
        {
            await session.Identify(); bool backed = false;
            await Reject<TimeoutException>(() => session.ReplaceIr(0, new byte[4096], loud.Sector(50), _ => { backed = true; return Task.CompletedTask; }), "transfer interruption times out");
            check(backed && session.IsFaulted && fake.FlashWrites == 1, "interrupted transfer keeps backup and stops session");
        }
    }

    private static byte[] Wav(int format, int bits, int channels, int rate, float[] samples, bool extensible = false)
    {
        using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
        int size = samples.Length * bits / 8;
        w.Write("RIFF"u8); w.Write(36 + size + (extensible ? 24 : 0)); w.Write("WAVEfmt "u8); w.Write(extensible ? 40 : 16);
        w.Write((ushort)(extensible ? 0xFFFE : format)); w.Write((ushort)channels); w.Write(rate); w.Write(rate * channels * bits / 8);
        w.Write((ushort)(channels * bits / 8)); w.Write((ushort)bits);
        if (extensible) { w.Write((ushort)22); w.Write((ushort)bits); w.Write(0); w.Write((ushort)format); w.Write(new byte[] { 0, 0, 0, 0, 0x10, 0, 0x80, 0, 0, 0xAA, 0, 0x38, 0x9B, 0x71 }); }
        w.Write("data"u8); w.Write(size);
        foreach (float value in samples)
        {
            if (format == 3) w.Write(value);
            else if (bits == 16) w.Write((short)(value * 32768));
            else if (bits == 24) { int number = (int)(value * 8388608); w.Write((byte)number); w.Write((byte)(number >> 8)); w.Write((byte)(number >> 16)); }
            else w.Write((int)(value * 2147483648d));
        }
        return stream.ToArray();
    }
}
