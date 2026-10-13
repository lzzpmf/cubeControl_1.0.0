namespace Cube.Protocol;

public sealed record PhaserResearchSnapshot(byte[] Bank, byte[] Dsp, byte[] PhysicalState);

public sealed partial class CubeSession
{
    private bool phaserFirmwareVerified;
    public async Task RunPhaserSweep(int physicalSlot, Func<string, PhaserResearchSnapshot, Task> preserve,
        Func<PhaserRecovery, Task>? preservePreview = null,
        CancellationToken ct = default)
    {
        if (physicalSlot is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(physicalSlot));
        var original = await ReadPhaserResearch(ct);
        if (!original.PhysicalState.SequenceEqual(new byte[] { (byte)physicalSlot, 1 }))
            throw new IOException("Preset físico não corresponde ao confirmado pelo usuário.");
        // Initial study deliberately starts with modulation/delay disabled. This permits
        // restoring dormant control bytes without overwriting running DSP state.
        if (original.Bank[physicalSlot * 16 + 11] != 0 || original.Dsp[0x63C] != 0 || original.Dsp[0x694] != 0)
            throw new IOException("Esta série exige a seção delay/modulação inicialmente desligada.");
        await preserve("original", original);
        byte[] expected = original.Bank.ToArray();
        try
        {
            foreach (int value in preservePreview == null ? new[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 3, 6 } : new[] { 12 })
            {
                expected = await WriteLiveChecked(physicalSlot, 11, 1, expected, ct);
                expected = await WriteLiveChecked(physicalSlot, 8, value, expected, ct);
                await Task.Delay(200, ct);
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    var capture = await ReadPhaserResearch(ct);
                    if (!capture.Bank.SequenceEqual(expected) || !capture.PhysicalState.SequenceEqual(original.PhysicalState))
                        throw new IOException("Contexto mudou durante a série.");
                    await preserve($"mod-{value:D2}-{repeat}", capture);
                    await Task.Delay(100, ct);
                }
                if (preservePreview != null)
                {
                    PhaserRecovery? active = null;
                    async Task Keep(PhaserRecovery record) { await preservePreview(record); active = record; }
                    try
                    {
                        active = await BeginPhaserPreview(physicalSlot, Keep, ct);
                        var start = active.Target;
                        var trials = new (string Name, PhaserParameters Parameters)[]
                        {
                            ("step-low", start with { Step = 0.00001f }), ("step-high", start with { Step = 0.00013f }),
                            ("lower", start with { Lower = 1000 }), ("upper", start with { Upper = 2000 }),
                            ("level", start with { Level = 0.32f }), ("sections-one", start with { Sections = 1 }),
                            ("sections-two", start with { Sections = 2 })
                        };
                        for (int cycle = 0; cycle < 2; cycle++)
                            foreach (var trial in trials)
                            {
                                active = await ApplyPhaserPreview(active!, trial.Parameters, Keep, ct);
                                await preserve($"{cycle}-{trial.Name}-immediate", await ReadPhaserResearch(ct));
                                await Task.Delay(300, ct);
                                expected = await WriteLiveChecked(physicalSlot, 5, expected[physicalSlot * 16 + 5], expected, ct);
                                var delayed = await ReadPhaserResearch(ct);
                                if (PhaserParameters.Read(delayed.Dsp) != trial.Parameters)
                                    throw new IOException("Parâmetro não permaneceu após espera/comando de volume.");
                                await preserve($"{cycle}-{trial.Name}-delayed", delayed);
                                await RestorePhaserPreview(active!, ct);
                                active = active! with { Previous = start, Target = start };
                                await Keep(active);
                                await preserve($"{cycle}-{trial.Name}-restored", await ReadPhaserResearch(ct));
                            }
                    }
                    finally
                    {
                        if (active != null && !IsFaulted) await RestorePhaserPreview(active, CancellationToken.None);
                    }
                }
                expected = await WriteLiveChecked(physicalSlot, 8, original.Bank[physicalSlot * 16 + 8], expected, ct);
                expected = await WriteLiveChecked(physicalSlot, 11, 0, expected, ct);
                await Task.Delay(200, ct);
                await RestoreInactivePhaserResearch(original, ct);
            }
        }
        finally
        {
            if (IsFaulted) throw new IOException("Conexão perdida; restauração não confirmada. Backup original preservado.");
            var current = await ReadPhaserResearch(CancellationToken.None);
            if (!current.PhysicalState.SequenceEqual(original.PhysicalState))
                throw new IOException("Preset físico mudou; recuperação automática interrompida. Backup preservado.");
            var allowable = original.Bank.ToArray();
            allowable[physicalSlot * 16 + 8] = current.Bank[physicalSlot * 16 + 8];
            allowable[physicalSlot * 16 + 11] = current.Bank[physicalSlot * 16 + 11];
            if (!allowable.SequenceEqual(current.Bank)) throw new IOException("Banco mudou externamente; backup preservado.");
            expected = await WriteLiveChecked(physicalSlot, 8, original.Bank[physicalSlot * 16 + 8], current.Bank);
            await WriteLiveChecked(physicalSlot, 11, 0, expected);
            await Task.Delay(200);
            await RestoreInactivePhaserResearch(original, CancellationToken.None);
            var restored = await ReadPhaserResearch();
            await preserve("restored", restored);
            if (!restored.Bank.SequenceEqual(original.Bank) || !PhaserParameters.StableEquals(restored.Dsp, original.Dsp))
                throw new IOException("Restauração integral da série não foi confirmada.");
        }
    }

    private async Task RestoreInactivePhaserResearch(PhaserResearchSnapshot original, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try
        {
            var bank = await ReadInternal(0, 48, ct);
            var state = await ReadInternal(EffectsPhysicalStateOffset, 2, ct, 4);
            var bytes = await ReadChunks(4, 0x63C, 0xA4, ct);
            if (!bank.SequenceEqual(original.Bank) || !state.SequenceEqual(original.PhysicalState) || bytes[0] != 0 || bytes[0x58] != 0)
                throw new IOException("Restauração dos blocos inativos exige banco original e efeitos desligados.");
            var target = original.Dsp.AsSpan(0x63C, 0xA4).ToArray();
            for (int offset = 0; offset < target.Length; offset += 32)
            {
                int count = Math.Min(32, target.Length - offset);
                if (!bytes.AsSpan(offset, count).SequenceEqual(target.AsSpan(offset, count)))
                    await AmpAck(CubePacket.Write(4, 0x63Cu + (uint)offset, target.AsSpan(offset, count)), ct);
            }
            if (!(await ReadChunks(4, 0x63C, 0xA4, ct)).SequenceEqual(target))
                throw new IOException("Restauração dos blocos inativos divergiu.");
        }
        finally { gate.Release(); }
    }

    public async Task<byte[]> ReadPhaserCodeEvidence(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            // Bounded window around code already returned by the mapping probes.
            // Raw evidence only: never treat it as a firmware compatibility approval.
            return await ReadChunks(4, 0xFFFFAC00, 0x5400, ct);
        }
        finally { gate.Release(); }
    }

    public async Task<Dictionary<string, string>> InspectPhaserMapping(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            var result = new Dictionary<string, string>();
            result["bank"] = Convert.ToHexString(await ReadInternal(0, 48, ct));
            result["modulation"] = Convert.ToHexString(await ReadInternal(0x63C, 0x58, ct, 4));
            // Second base is a hypothesis from an exact 10-byte match of the first live read.
            foreach (uint baseAddress in new uint[] { 0x834660, 0x835898 })
                foreach (uint address in new uint[] { 0x83244C, 0x83343C, 0x832186, 0x80C390, 0x80CBB8, 0x80CB12 })
                    result[$"base-{baseAddress:X8}-cpu-{address:X8}"] = Convert.ToHexString(
                        await ReadInternal(unchecked(address - baseAddress), 32, ct, 4));
            return result;
        }
        finally { gate.Release(); }
    }

    // The captured live profile and the B update have separate pinned code ranges,
    // sharing the DSP parameter layout at 0x8347F8.
    // These are bounded research reads, not an arbitrary memory API.
    public async Task<PhaserResearchSnapshot> ReadPhaserResearch(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try { return await ReadPhaserCore(ct); }
        finally { gate.Release(); }
    }

    private async Task<PhaserResearchSnapshot> ReadPhaserCore(CancellationToken ct, bool effectControlsOnly = false)
    {
            RequireIdentity();
            await VerifyPhaserFirmware(ct).ConfigureAwait(false);
            var bank = await ReadInternal(0, 48, ct).ConfigureAwait(false);
            var alias = await ReadInternal(EffectsBankAliasOffset, 48, ct, 4).ConfigureAwait(false);
            if (!bank.SequenceEqual(alias)) throw new IOException("Alias do banco diverge; mapeamento não confirmado.");
            var state = await ReadInternal(EffectsPhysicalStateOffset, 2, ct, 4).ConfigureAwait(false);
            // Preview mutations only need the three effect blocks. Keep the absolute-offset
            // buffer shape used by parameter codecs; bytes outside this range are NOT a capture.
            byte[] dsp;
            if (effectControlsOnly)
            {
                dsp = new byte[0xF70];
                (await ReadChunks(4, EffectControlsStart, EffectControlsLength, ct).ConfigureAwait(false))
                    .CopyTo(dsp, EffectControlsStart);
            }
            else dsp = await ReadChunks(4, 0, 0xF70, ct).ConfigureAwait(false);
            if (!(await ReadInternal(0, 48, ct).ConfigureAwait(false)).SequenceEqual(bank))
                throw new IOException("Banco mudou durante a captura.");
            if (!(await ReadInternal(EffectsPhysicalStateOffset, 2, ct, 4).ConfigureAwait(false)).SequenceEqual(state))
                throw new IOException("Estado físico mudou durante a captura.");
            return new(bank, dsp, state);
    }

    private async Task VerifyPhaserFirmware(CancellationToken ct)
    {
        if (phaserFirmwareVerified) return;
        var reader = await ReadInternal(unchecked(0x832108u - 0x8347F8u), 12, ct, 4);
        if (HashMatches(reader, "e3b09baf62e6fc2895eceaaef0db5acfe035b53a171f3e00b52af442f1dbd965"))
            effectsFirmwareB = false;
        else
        {
            var bReader = await ReadInternal(unchecked(0x832150u - 0x8347F8u), 12, ct, 4);
            if (!HashMatches(bReader, FirmwareBSignatures[0].Hash))
                throw new NotSupportedException("Os criadores de efeitos não são compatíveis com o firmware deste pedal. A edição normal de presets continua disponível. Reconectar não altera essa compatibilidade.");
            effectsFirmwareB = true;
        }
        foreach (var (address, length, hash) in new (uint, int, string)[]
        {
            (0x832108, 12, "e3b09baf62e6fc2895eceaaef0db5acfe035b53a171f3e00b52af442f1dbd965"),
            (0x83239C, 0x104, "2760034108cfb0e40ed4436cc1ef2b8811aa320c50f0242d1bf25003b724a996"),
            (0x831D84, 0x46, "0a4a8557dfbaa001fcc7f2c275560d0aea87d4ca5c9d494c6314885722854f97"),
            (0x831F5C, 0x1B8, "2cf1fb33692f2cb6daed436f2e2ae082b89ba713f4d02fb33e297ab43399cc75"),
            (0x8327C4, 0x88, "a9754bb9e6b31fb02270fb1ee1977d3b8118b193c51977afc12457c1018793ef")
        })
        {
            await VerifyEffectsRange(address, length, hash, "efeitos", ct);
        }
        var entry = await ReadInternal(EffectsAddress(0x80C37C), 4, ct, 4);
        if (!entry.SequenceEqual(Convert.FromHexString("9D238300"))) throw new NotSupportedException("Os criadores de efeitos não são compatíveis com o dispatcher deste firmware. A edição normal de presets continua disponível.");
        phaserFirmwareVerified = true;
        EffectsFirmware = effectsFirmwareB ? "cubebaby_B_20220706" : "cube-baby-live-20261001";
    }
}
