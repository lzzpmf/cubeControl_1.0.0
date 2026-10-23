namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private bool delayFirmwareVerified;
    private async Task VerifyDelayFirmware(CancellationToken ct)
    {
        if (delayFirmwareVerified) return;
        await VerifyPhaserFirmware(ct);
        if (effectsFirmwareVd612) return;
        foreach (var (address, size, hash) in new (uint, int, string)[]
        {
            (0x831AC0, 0x484, "5f808479d3b3a74ef2f00ab184b9a61681211064260e908effb5313fcd097451"),
            (0x833724, 0x150, "0f08a002ab638d2482555648a65fb3f7d25798f1aa20a0ec80db2fff6f592a73"),
            (0x832DA8, 0x338, "b630ae7730ea2051e2fb93918c5829bc62cce70b259f89c8214a3f0315e12f46"),
            (0xC13C3C, 0xC4, "6d29cfa0a690b6ccd217df9dcdad6c48068fde3154c0d885cc56435cc2ed94bf"),
            (0x833562, 0x32, "68bb0fe53c47af78c5da900254ec78601c680d2a2e60eee0f7a919498341ea51"),
            (0x80A8BC, 0x62, "2b66bf16d3d6b3fd1407e1c51bc893c1f3ce28e3451410c97647c66d05b201b0"),
            (0xC488E0, 0x38, "2a25b41e50d042188203cc21dd4f3834beb1824934114fb5ab9c79c8d710085a"),
            (0x8359A0, 0x6C, "1b7f8ab5a9f431c89e1fec2060c3b71993b416dc20e8d5526b2169e91b59ea79"),
        })
        {
            await VerifyEffectsRange(address, size, hash, "delay", ct);
        }
        delayFirmwareVerified = true;
    }
    private async Task<DelaySnapshot> ReadDelayCore(CancellationToken ct, bool effectControlsOnly = false)
    {
        await VerifyDelayFirmware(ct);
        var p = await ReadPhaserCore(ct, effectControlsOnly);
        return new(p.Bank, p.Dsp, p.PhysicalState);
    }
    public async Task<DelaySnapshot> ReadDelayResearch(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try { RequireIdentity(); return await ReadDelayCore(ct); }
        finally { gate.Release(); }
    }

    public async Task<Dictionary<string, byte[]>> ReadDelayMapping(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            await VerifyPhaserFirmware(ct);
            var result = new Dictionary<string, byte[]>();
            foreach (var (name, address, size) in new (string, uint, int)[]
            {
                ("preset", 0x831AC0, 0x484), ("caller", 0x833724, 0x150),
                ("worker", 0x832DA8, 0x338), ("initialize", 0xC13C3C, 0xC4),
                ("oscillator", 0x833562, 0x32), ("filter", 0x80A8BC, 0x62),
                ("filters", 0xC488E0, 56), ("input-filter", 0x8359A0, 0x6C),
                ("runtime", 0x817F86, 0x2E), ("current-time", 0x80FF84, 2),
                ("input-state", 0x81823C, 20)
            }) result[name] = await ReadChunks(4, EffectsAddress(address), size, ct);
            var snapshot = await ReadPhaserCore(ct);
            result["bank"] = snapshot.Bank; result["dsp"] = snapshot.Dsp; result["physical"] = snapshot.PhysicalState;
            return result;
        }
        finally { gate.Release(); }
    }

    public async Task RunDelayResearch(string mode, Func<string, DelaySnapshot, Task> preserve,
        Func<string, byte, uint, byte[], byte[], Task> intent, CancellationToken ct = default,
        Func<string, DelayRuntimeSample, Task>? observe = null)
    {
        if (mode is not ("delay-sweep" or "delay-controls" or "delay-transitions" or "delay-modulation" or "delay-runtime")) throw new ArgumentException("Ensaio inválido.");
        if (mode == "delay-runtime" && observe == null) throw new ArgumentException("Preserve as amostras de estado externo.");
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            bool modulationSeries = mode == "delay-modulation";
            if (modulationSeries) await VerifyChorusFirmware(ct);
            var original = await ReadDelayCore(ct);
            CubeBabyProfile.ValidateBank(original.Bank);
            int slot = original.PhysicalState[0];
            if (slot > 2 || original.PhysicalState[1] != 1 || original.Dsp[0x63C] != 0 ||
                original.Dsp[0x694] != 0 || original.Bank[slot * 16 + 11] != 0 || original.Bank[slot * 16 + 8] is not (7 or 8))
                throw new IOException("Pesquisa exige PRESET, modulação desligada e seção delay/modulação inicialmente desligada.");
            await preserve("original", original);
            byte[] expected = original.Bank.ToArray();
            bool OutsideEqual(byte[] dsp)
            {
                var copy = dsp.ToArray(); original.Dsp.AsSpan(0x694, 0x4C).CopyTo(copy.AsSpan(0x694));
                // This separate experiment deliberately switches the other algorithm.
                // Its whole block is accounted for separately, never a delay dynamic mask.
                if (modulationSeries) original.Dsp.AsSpan(0x63C, 0x58).CopyTo(copy.AsSpan(0x63C));
                return DelayParameters.StableEquals(copy, original.Dsp);
            }
            async Task<DelaySnapshot> Context()
            {
                var now = await ReadDelayCore(ct);
                if (!now.Bank.SequenceEqual(expected) || !now.PhysicalState.SequenceEqual(original.PhysicalState) || !OutsideEqual(now.Dsp))
                    throw new IOException("Contexto externo mudou; envios interrompidos e backup preservado.");
                return now;
            }
            async Task<DelaySnapshot> Capture(string label)
            {
                var now = await Context(); await preserve(label, now); return now;
            }
            async Task Live(int offset, int value)
            {
                await Context(); uint address = CubeBabyProfile.LiveAddress(slot, offset);
                await intent("live", 5, address, [expected[slot * 16 + offset]], [(byte)value]);
                await WriteInternal(address, [(byte)value], ct); expected[slot * 16 + offset] = (byte)value;
                await Context();
            }
            async Task Patch(string label, DelayParameters target)
            {
                var before = await Context(); var after = target.ApplyTo(before.Dsp); var currentExpected = before.Dsp.ToArray();
                foreach (var (offset, length) in DelayParameters.Fields)
                {
                    var old = before.Dsp.AsSpan(offset, length).ToArray(); var next = after.AsSpan(offset, length).ToArray();
                    if (old.SequenceEqual(next)) continue;
                    if (!DelayParameters.StableEquals((await Context()).Dsp, currentExpected)) throw new IOException("Estado mudou entre campos.");
                    await intent(label, 4, (uint)offset, old, next);
                    await AmpAck(CubePacket.Write(4, (uint)offset, next), ct);
                    next.CopyTo(currentExpected, offset);
                }
                if (!DelayParameters.StableEquals((await Capture(label + "-checked")).Dsp, after)) throw new IOException("Escrita isolada não confirmada.");
            }
            async Task RestoreInactive()
            {
                var now = await Context();
                if (now.Dsp[0x694] != 0 || now.Dsp[0x63C] != 0 || expected[slot * 16 + 11] != 0) throw new IOException("Seção ainda ativa.");
                for (int offset = modulationSeries ? 0x63C : 0x694; offset < 0x6E0; offset += 32)
                {
                    int length = Math.Min(32, 0x6E0 - offset);
                    var before = now.Dsp.AsSpan(offset, length).ToArray(); var after = original.Dsp.AsSpan(offset, length).ToArray();
                    if (before.SequenceEqual(after)) continue;
                    await Context(); await intent("inactive-restore", 4, (uint)offset, before, after);
                    await AmpAck(CubePacket.Write(4, (uint)offset, after), ct);
                }
                if (!(await Context()).Dsp.AsSpan(0x694, 0x4C).SequenceEqual(original.Dsp.AsSpan(0x694, 0x4C))) throw new IOException("Restauro inativo divergiu.");
            }
            var start = new DelayParameters(9600, 0.25f, 0.1f, 0.01f, 0.25f);
            for (int repeat = 0; repeat < 3; repeat++) { await Capture($"idle-{repeat}"); await Task.Delay(100, ct); }
            try
            {
                await Live(6, 16); await Live(4, 32); await Live(7, 32); await Live(11, 1);
                var baseline = await Capture("baseline"); DelayParameters.ValidateActive(baseline.Bank, baseline.Dsp, baseline.PhysicalState);
                if (DelayParameters.Read(baseline.Dsp) != start) throw new IOException("Delay inicial inesperado.");
                if (mode == "delay-sweep")
                {
                    foreach (var (field, maximum, label) in new[] { (6,31,"time"), (4,127,"feedback"), (7,118,"mix") })
                    {
                        for (int raw = 0; raw <= maximum; raw++)
                        {
                            await Live(field, raw);
                            for (int repeat = 0; repeat < 3; repeat++)
                            {
                                var snapshot = await Capture($"{label}-{raw:D3}-{repeat}");
                                var target = field == 6 ? start with { TimeSamples = 2400 + 450 * raw } :
                                    field == 4 ? start with { Feedback = raw / 128f } : start with { Mix = raw / 128f };
                                if (!DelayParameters.StableEquals(snapshot.Dsp, target.ApplyTo(baseline.Dsp))) throw new IOException("Tabela divergiu.");
                                await Task.Delay(100, ct);
                            }
                        }
                        await Live(field, field == 6 ? 16 : 32);
                        if (!DelayParameters.StableEquals((await Capture(label + "-restored")).Dsp, baseline.Dsp)) throw new IOException("Restauro de tabela divergiu.");
                    }
                }
                else if (mode == "delay-controls")
                {
                    var trials = new (string, DelayParameters)[]
                    {
                        ("depth-zero", start with { Depth = 0 }), ("depth-half", start with { Depth = 0.05f }),
                        ("rate-zero", start with { Rate = 0 }), ("rate-half", start with { Rate = 0.005f }),
                        ("time-low", start with { TimeSamples = 2400 }), ("time-fine", start with { TimeSamples = 9601 }),
                        ("time-high", start with { TimeSamples = 16350 }),
                        ("feedback-zero", start with { Feedback = 0 }), ("feedback-fine", start with { Feedback = 0.123f }),
                        ("mix-zero", start with { Mix = 0 }), ("mix-fine", start with { Mix = 0.321f })
                    };
                    for (int cycle = 0; cycle < 2; cycle++) foreach (var (name, target) in trials)
                    {
                        string label = $"{cycle}-{name}"; await Patch(label, target); await Task.Delay(300, ct);
                        if (DelayParameters.Read((await Capture(label + "-delayed")).Dsp) != target) throw new IOException("Parâmetro não permaneceu.");
                        await Live(5, expected[slot * 16 + 5]); await Live(6,16); await Live(4,32); await Live(7,32);
                        if (DelayParameters.Read((await Capture(label + "-same")).Dsp) != target) throw new IOException("Reenvio recarregou parâmetros.");
                        await Patch(label + "-restore", start);
                        if (!DelayParameters.StableEquals((await Capture(label + "-restored")).Dsp, baseline.Dsp)) throw new IOException("Restauro do controle divergiu.");
                    }
                }
                else if (mode == "delay-runtime")
                {
                    foreach (var (name, rate) in new[] { ("original", 0.01f), ("half", 0.005f), ("frozen", 0f) })
                    {
                        await Patch("runtime-" + name, start with { Rate = rate });
                        byte[]? firstPhase = null;
                        for (int repeat = 0; repeat < 10; repeat++)
                        {
                            var started = DateTimeOffset.UtcNow;
                            var time = await ReadInternal(EffectsAddress(0x80FF84), 2, ct, 4);
                            var state = await ReadInternal(EffectsAddress(0x817F86), 0x2E, ct, 4);
                            await observe!($"runtime-{name}-{repeat}", new(started, DateTimeOffset.UtcNow, time, state));
                            if (System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(time) != start.TimeSamples)
                                throw new IOException("Tempo interno não convergiu ao solicitado.");
                            var phase = state.AsSpan(0x26, 4).ToArray();
                            firstPhase ??= phase;
                            if (rate == 0 && !phase.SequenceEqual(firstPhase)) throw new IOException("Fase não congelou com velocidade zero.");
                            await Task.Delay(100, ct);
                        }
                        if (DelayParameters.Read((await Capture("runtime-" + name + "-after")).Dsp) != (start with { Rate = rate }))
                            throw new IOException("Controles mudaram durante leitura do estado externo.");
                    }
                    await Patch("runtime-restore", start);
                }
                else if (mode == "delay-modulation")
                {
                    var custom = new DelayParameters(9601, 0.123f, 0.05f, 0.005f, 0.321f);
                    await Patch("custom-modulation", custom);
                    foreach (int mod in new[] { 0, 9 })
                    {
                        await Live(8, mod);
                        for (int repeat = 0; repeat < 3; repeat++)
                        {
                            var now = await Capture($"mod-{mod}-{repeat}");
                            if (now.Dsp[0x63C] != 3 || now.Dsp[0x63D] != (mod == 0 ? 5 : 2) || DelayParameters.Read(now.Dsp) != custom)
                                throw new IOException("Interação com modulação divergiu.");
                            if (!now.Dsp.AsSpan(0x694, 0x4C).SequenceEqual(custom.ApplyTo(baseline.Dsp).AsSpan(0x694, 0x4C)))
                                throw new IOException("Estrutura de delay mudou ao trocar modulação.");
                        }
                        await Live(8, original.Bank[slot * 16 + 8]); await Capture($"mod-{mod}-off");
                    }
                }
                else
                {
                    var custom = new DelayParameters(9601, 0.123f, 0.05f, 0.005f, 0.321f);
                    int otherOff = original.Bank[slot * 16 + 8] == 7 ? 8 : 7;
                    foreach (var (field,value,name) in new[] { (6,17,"time"), (4,33,"feedback"), (7,33,"mix"), (8,otherOff,"mod-off") })
                    {
                        await Patch("custom-"+name, custom); await Live(field,value);
                        var target = field == 6 ? custom with { TimeSamples = 10050 } : field == 4 ? custom with { Feedback = 33/128f } :
                            field == 7 ? custom with { Mix = 33/128f } : custom;
                        if (DelayParameters.Read((await Capture(name+"-changed")).Dsp) != target) throw new IOException("Recarga seletiva divergiu.");
                        await Live(field, field == 6 ? 16 : field == 8 ? original.Bank[slot*16+8] : 32);
                        await Patch(name+"-restore", start);
                    }
                    await Patch("custom-section", custom); await Live(11,0); await Capture("section-off"); await Live(11,1);
                    if (!DelayParameters.StableEquals((await Capture("section-on")).Dsp, baseline.Dsp)) throw new IOException("Reinicialização divergiu.");
                }
            }
            finally
            {
                if (IsFaulted) throw new IOException("Conexão perdida; restauração não confirmada. Preserve os backups e intenções.");
                await Context(); await Live(11,0);
                foreach (int field in new[] { 6,4,7,8 }) if (expected[slot*16+field] != original.Bank[slot*16+field]) await Live(field,original.Bank[slot*16+field]);
                await RestoreInactive();
                var restored = await Capture("restored");
                if (!restored.Bank.SequenceEqual(original.Bank) || !DelayParameters.StableEquals(restored.Dsp,original.Dsp)) throw new IOException("Restauro final não confirmado.");
            }
        }
        finally { gate.Release(); }
    }
}
