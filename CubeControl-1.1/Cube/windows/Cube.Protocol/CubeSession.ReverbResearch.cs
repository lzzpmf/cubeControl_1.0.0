namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private bool reverbFirmwareVerified;
    private async Task VerifyReverbFirmware(CancellationToken ct)
    {
        if (reverbFirmwareVerified) return;
        await VerifyPhaserFirmware(ct);
        foreach (var (address, size, hash) in new (uint, int, string)[]
        {
            (0x831AC0, 0x484, "5f808479d3b3a74ef2f00ab184b9a61681211064260e908effb5313fcd097451"),
            (0x833724, 0x150, "0f08a002ab638d2482555648a65fb3f7d25798f1aa20a0ec80db2fff6f592a73"),
            (0x833978, 0x284, "5077275ea347538b4f81759ef870bcb755b7d5138bbc531e9c0a6a61f721f574"),
            (0x833904, 0x74, "cd3a976ccd8269dd40349421699f08b1653a8873ed8ccdd17ad902b628230478"),
            (0x8338E0, 0x24, "03d2bc1b561b932e926c6575cc100a5ffd00f2c7c23ca8eb34df69b672f2942a"),
            (0xC13D00, 0x200, "a443976b8c6482dd623812644665dcb387df77fc9eded5f15b0941820ce9c182"),
            (0x80A8BC, 0x62, "2b66bf16d3d6b3fd1407e1c51bc893c1f3ce28e3451410c97647c66d05b201b0"),
            (0xC48C98, 0x84, "caee96fe76d1f0a2a3d2bc732290ba8836e9537475aaf5037b1acb0d78191284"),
            (0xC4881C, 0xB4, "f875a55d71d4c82934f456afa1c681a7f68a33deaa49a272bdf9a8a4abb19502"),
            (0xC488D0, 0x10, "bb0f1d792c9c2475436b133de1c381ab37f3d8fd243caca4b66656456fd3b0ef"),
            (0xC48D38, 0x78, "c4e784896d3fc6bf501bbf71537cacbbff8ad4f0a6060df5d9995bf7da523fc2"),
            (0x8359A0, 0x6C, "1b7f8ab5a9f431c89e1fec2060c3b71993b416dc20e8d5526b2169e91b59ea79"),
        })
        {
            await VerifyEffectsRange(address, size, hash, "reverb", ct);
        }
        reverbFirmwareVerified = true;
    }
    private async Task<ReverbSnapshot> ReadReverbCore(CancellationToken ct)
    {
        await VerifyReverbFirmware(ct);
        var p = await ReadPhaserCore(ct);
        return new(p.Bank, p.Dsp, p.PhysicalState);
    }
    public async Task<ReverbSnapshot> ReadReverbResearch(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try { RequireIdentity(); return await ReadReverbCore(ct); }
        finally { gate.Release(); }
    }

    // Read-only, bounded ranges traced from the preserved live preset and DSP caller.
    public async Task<Dictionary<string, byte[]>> ReadReverbMapping(CancellationToken ct = default)
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
                ("worker", 0x833978, 0x284), ("diffusion", 0x833904, 0x74),
                ("damping", 0x8338E0, 0x24), ("initialize", 0xC13D00, 0x200),
                ("filter", 0x80A8BC, 0x62), ("default", 0xC48C98, 0x84),
                ("feedback-table", 0xC4881C, 180), ("damping-table", 0xC488D0, 16),
                ("mix-table", 0xC48D38, 120), ("runtime", 0x8181BC, 0x94),
                ("input-filter", 0x8359A0, 0x6C), ("input-coefficients", 0x818228, 20)
            }) result[name] = await ReadChunks(4, EffectsAddress(address), size, ct);
            var snapshot = await ReadPhaserCore(ct);
            result["bank"] = snapshot.Bank; result["dsp"] = snapshot.Dsp; result["physical"] = snapshot.PhysicalState;
            return result;
        }
        finally { gate.Release(); }
    }
    public async Task RunReverbResearch(string mode, Func<string, ReverbSnapshot, Task> preserve,
        Func<string, byte, uint, byte[], byte[], Task> intent, CancellationToken ct = default)
    {
        if (mode is not ("reverb-sweep" or "reverb-controls" or "reverb-transitions")) throw new ArgumentException("Ensaio inválido.");
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            var original = await ReadReverbCore(ct);
            int slot = original.PhysicalState[0];
            if (slot > 2 || original.PhysicalState[1] != 1 || original.Bank[slot * 16 + 3] != 0 ||
                original.Bank[slot * 16 + 10] != 1 || original.Dsp[0x6E0] != 0 || original.Dsp[0x63C] != 0)
                throw new IOException("Pesquisa exige PRESET, reverb em zero, seção reverb/cabinet ligada e modulação desligada.");
            await preserve("original", original);
            byte[] expected = original.Bank.ToArray();
            bool OutsideEqual(byte[] current)
            {
                var copy = current.ToArray();
                if (expected[slot * 16 + 10] == 0 && copy[0x764] == 0) copy[0x764] = original.Dsp[0x764];
                original.Dsp.AsSpan(0x6E0, 0x84).CopyTo(copy.AsSpan(0x6E0));
                return ReverbParameters.StableEquals(copy, original.Dsp);
            }
            async Task<ReverbSnapshot> Context()
            {
                var now = await ReadReverbCore(ct);
                if (!now.Bank.SequenceEqual(expected) || !now.PhysicalState.SequenceEqual(original.PhysicalState) || !OutsideEqual(now.Dsp))
                    throw new IOException("Contexto externo mudou; envios interrompidos e backup preservado.");
                return now;
            }
            async Task<ReverbSnapshot> Capture(string label)
            {
                var now = await Context(); await preserve(label, now); return now;
            }
            async Task Live(int offset, int value)
            {
                await Context();
                uint address = CubeBabyProfile.LiveAddress(slot, offset);
                await intent("live", 5, address, [expected[slot * 16 + offset]], [(byte)value]);
                await WriteInternal(address, [(byte)value], ct);
                expected[slot * 16 + offset] = (byte)value;
                await Context();
            }
            async Task Patch(string label, ReverbSnapshot before, ReverbParameters target)
            {
                var current = await Context();
                if (!ReverbParameters.StableEquals(current.Dsp, before.Dsp)) throw new IOException("Estado mudou antes da escrita.");
                var after = target.ApplyTo(before.Dsp);
                foreach (var (offset, length) in ReverbParameters.Fields)
                {
                    var old = before.Dsp.AsSpan(offset, length).ToArray(); var next = after.AsSpan(offset, length).ToArray();
                    if (old.SequenceEqual(next)) continue;
                    await intent(label, 4, (uint)offset, old, next);
                    await AmpAck(CubePacket.Write(4, (uint)offset, next), ct);
                }
                if (!ReverbParameters.StableEquals((await Capture(label + "-checked")).Dsp, after))
                    throw new IOException("Escrita isolada não confirmada.");
            }
            async Task RestoreInactive()
            {
                var now = await Context();
                if (now.Dsp[0x6E0] != 0 || expected[slot * 16 + 3] != 0) throw new IOException("Reverb ainda ativo.");
                for (int offset = 0x6E0; offset < 0x764; offset += 32)
                {
                    int length = Math.Min(32, 0x764 - offset);
                    var before = now.Dsp.AsSpan(offset, length).ToArray(); var after = original.Dsp.AsSpan(offset, length).ToArray();
                    if (before.SequenceEqual(after)) continue;
                    await intent("inactive-restore", 4, (uint)offset, before, after);
                    await AmpAck(CubePacket.Write(4, (uint)offset, after), ct);
                }
                if (!(await Context()).Dsp.AsSpan(0x6E0, 0x84).SequenceEqual(original.Dsp.AsSpan(0x6E0, 0x84)))
                    throw new IOException("Restauro inativo divergiu.");
            }
            for (int repeat = 0; repeat < 3; repeat++) { await Capture($"idle-{repeat}"); await Task.Delay(100, ct); }
            try
            {
                if (mode == "reverb-sweep")
                {
                    for (int raw = 0; raw <= 15; raw++)
                    {
                        await Live(3, raw);
                        for (int repeat = 0; repeat < 3; repeat++) { await Capture($"raw-{raw:D2}-{repeat}"); await Task.Delay(100, ct); }
                        await Live(3, 0); await RestoreInactive();
                    }
                }
                else
                {
                    await Live(3, 8);
                    var baseline = await Capture("baseline");
                    ReverbParameters.ValidateActive(baseline.Bank, baseline.Dsp, baseline.PhysicalState);
                    var start = ReverbParameters.Read(baseline.Dsp);
                    if (start != new ReverbParameters(1, 0.55f, 8, 80, 10)) throw new IOException("Reverb inicial inesperado.");
                    if (mode == "reverb-controls")
                    {
                        var trials = new (string Name, ReverbParameters Parameters)[]
                        {
                            ("dry-zero", start with { Dry = 0 }), ("dry-half", start with { Dry = 0.5f }),
                            ("wet-zero", start with { Wet = 0 }), ("wet-half", start with { Wet = 0.275f }),
                            ("decay-low", start with { Decay = 1 }), ("decay-high", start with { Decay = 15 }),
                            ("return-zero", start with { ReturnLevel = 0 }), ("return-half", start with { ReturnLevel = 40 }),
                            ("damping-zero", start with { Damping = 0 }), ("damping-half", start with { Damping = 5 })
                        };
                        for (int cycle = 0; cycle < 2; cycle++) foreach (var trial in trials)
                        {
                            string label = $"{cycle}-{trial.Name}";
                            await Patch(label, baseline, trial.Parameters);
                            await Task.Delay(300, ct);
                            var delayed = await Capture(label + "-delayed");
                            if (ReverbParameters.Read(delayed.Dsp) != trial.Parameters) throw new IOException("Parâmetro não permaneceu.");
                            await Live(5, expected[slot * 16 + 5]); await Live(3, 8);
                            var same = await Capture(label + "-same");
                            if (!ReverbParameters.StableEquals(same.Dsp, trial.Parameters.ApplyTo(baseline.Dsp))) throw new IOException("Reenvio recarregou parâmetros.");
                            await Live(3, 9);
                            var changed = await Capture(label + "-raw9");
                            var reload = (trial.Parameters with { Dry = 1, Wet = 0.6f, Decay = 9 }).ApplyTo(baseline.Dsp); reload[0x6E2] = 72;
                            if (!ReverbParameters.StableEquals(changed.Dsp, reload)) throw new IOException("Recarga do reverb divergiu.");
                            await Live(3, 8);
                            await Patch(label + "-restore", await Context(), start);
                            if (!ReverbParameters.StableEquals((await Capture(label + "-restored")).Dsp, baseline.Dsp)) throw new IOException("Restauro de controle divergiu.");
                        }
                    }
                    else
                    {
                        foreach (string transition in new[] { "off", "section" })
                        {
                            // Customize all five controls so reinitialization is observable for each.
                            await Patch("custom-" + transition, baseline, new(0.5f, 0.275f, 1, 40, 5));
                            await Live(transition == "off" ? 3 : 10, 0);
                            await Capture(transition + "-off");
                            await Live(transition == "off" ? 3 : 10, transition == "off" ? 8 : 1);
                            if (!ReverbParameters.StableEquals((await Capture(transition + "-on")).Dsp, baseline.Dsp)) throw new IOException("Reinicialização divergiu.");
                        }
                    }
                    await Live(3, 0); await RestoreInactive();
                }
            }
            finally
            {
                if (IsFaulted) throw new IOException("Conexão perdida; restauração não confirmada. Preserve os backups e intenções.");
                await Context();
                if (expected[slot * 16 + 10] != original.Bank[slot * 16 + 10]) await Live(10, original.Bank[slot * 16 + 10]);
                await Live(3, 0); await RestoreInactive();
                var restored = await Capture("restored");
                if (!restored.Bank.SequenceEqual(original.Bank) || !ReverbParameters.StableEquals(restored.Dsp, original.Dsp))
                    throw new IOException("Restauro final não confirmado.");
            }
        }
        finally { gate.Release(); }
    }

}
