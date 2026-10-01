using System.Security.Cryptography;
using System.Buffers.Binary;

namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private bool chorusFirmwareVerified;
    // Bounded read-only evidence collection for the captured 2026-10-01 layout.
    public async Task<Dictionary<string, byte[]>> ReadChorusMapping(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            await VerifyPhaserFirmware(ct);
            var result = new Dictionary<string, byte[]>();
            foreach (var (name, address, size) in new (string, uint, int)[]
            {
                ("dispatch", 0x80C374, 24), ("oscillators", 0x80C3DC, 16),
                ("worker", 0x8326B4, 0x110), ("delay-and-phase", 0x832188, 0xCC),
                ("preset", 0x831AC0, 0x49C), ("initial-filters", 0xC487AC, 56),
                ("mod-table", 0xC487E4, 56), ("waveforms", 0x833540, 0x54),
                ("initialize", 0xC13B04, 0x100), ("filter-helper", 0x80A91E, 0x80)
            }) result[name] = await ReadChunks(4, unchecked(address - 0x8347F8u), size, ct);
            return result;
        }
        finally { gate.Release(); }
    }

    private async Task VerifyChorusFirmware(CancellationToken ct)
    {
        if (chorusFirmwareVerified) return;
        await VerifyPhaserFirmware(ct);
        foreach (var (address, size, hash) in new (uint, int, string)[]
        {
            (0x8326B4, 0x110, "4b3f852a0f15934beacea107471de2877f89052d3eaef4652bead312a28369aa"),
            (0x832188, 0xCC, "1eb86dcfd8f7d15dfc4bee9fde2641522a88b315d3ddd26040552b19547cd5fb"),
            (0x833540, 0x54, "78adc5ec3bc6558618428322d2d5d2394a0e9d3d091a32e297bc01572b0697bb"),
            (0x831AC0, 0x49C, "cf811d5442a4ecd134f315d7c89f408efcbf363c926ab029db7a790c48d94484"),
            (0xC13B04, 0x30, "31fa77b842bbc324aa73a921d1a5986d9f8f06de57b4a898adb49db2c4584bd7"),
            (0x80A91E, 0x2A, "465961c91038ea88c1490bc3e4cf0dbf9acc3b550e6f096398d37e25bf685b83")
        })
        {
            var bytes = await ReadChunks(4, unchecked(address - 0x8347F8u), size, ct);
            if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(hash, StringComparison.OrdinalIgnoreCase))
                throw new IOException($"Assinatura do chorus diverge em {address:X8}.");
        }
        if (!(await ReadInternal(unchecked(0x80C388u - 0x8347F8u), 4, ct, 4)).SequenceEqual(Convert.FromHexString("B5268300")) ||
            !(await ReadInternal(unchecked(0x80C3DCu - 0x8347F8u), 12, ct, 4)).SequenceEqual(Convert.FromHexString("633583004135830061358300")))
            throw new IOException("Dispatcher/osciladores do chorus incompatíveis.");
        chorusFirmwareVerified = true;
    }

    public static bool ChorusStableEquals(byte[] a, byte[] b) => a.Length == 0xF70 && b.Length == a.Length &&
        Enumerable.Range(0, a.Length).All(i => i is >= 0x68C and < 0x694 or >= 0xF6C and < 0xF70 || a[i] == b[i]);

    // Fixed experiments only; no arbitrary address/value write API or flash operations.
    public async Task RunChorusResearch(bool controls, Func<string, PhaserResearchSnapshot, Task> preserve,
        Func<string, uint, byte[], byte[], Task> intent, CancellationToken ct = default, bool transitions = false)
    {
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            await VerifyChorusFirmware(ct);
            var original = await ReadPhaserCore(ct);
            int slot = original.PhysicalState[0];
            if (slot > 2 || original.PhysicalState[1] != 1 || original.Bank[slot * 16 + 11] != 0 ||
                original.Dsp[0x63C] != 0 || original.Dsp[0x694] != 0)
                throw new IOException("Pesquisa exige modo PRESET e seção delay/modulação desligada.");
            await preserve("original", original);
            byte[] expected = original.Bank.ToArray();
            int sequence = 0;
            async Task Context()
            {
                if (!(await ReadInternal(0, 48, ct)).SequenceEqual(expected) ||
                    !(await ReadInternal(0xFFFD8202, 2, ct, 4)).SequenceEqual(original.PhysicalState))
                    throw new IOException("Contexto externo mudou; backup preservado.");
            }
            async Task Live(int offset, int value)
            {
                await Context();
                uint address = CubeBabyProfile.LiveAddress(slot, offset);
                await intent("live", address, [expected[slot * 16 + offset]], [(byte)value]);
                await WriteInternal(address, [(byte)value], ct);
                expected[slot * 16 + offset] = (byte)value;
                await Context();
            }
            async Task<PhaserResearchSnapshot> Capture(string label)
            {
                var snapshot = await ReadPhaserCore(ct);
                if (!snapshot.Bank.SequenceEqual(expected) || !snapshot.PhysicalState.SequenceEqual(original.PhysicalState))
                    throw new IOException("Contexto mudou na captura.");
                await preserve(label, snapshot);
                return snapshot;
            }
            async Task RestoreInactive()
            {
                await Context();
                var inactive = await ReadChunks(4, 0x63C, 0xA4, ct);
                if (inactive[0] != 0 || inactive[0x58] != 0) throw new IOException("Efeitos ainda ativos.");
                var target = original.Dsp.AsSpan(0x63C, 0xA4).ToArray();
                for (int i = 0; i < target.Length; i += 32)
                {
                    int count = Math.Min(32, target.Length - i);
                    var before = inactive.AsSpan(i, count).ToArray(); var after = target.AsSpan(i, count).ToArray();
                    if (before.SequenceEqual(after)) continue;
                    await intent("inactive-restore", 0x63Cu + (uint)i, before, after);
                    await AmpAck(CubePacket.Write(4, 0x63Cu + (uint)i, after), ct);
                }
                if (!(await ReadChunks(4, 0x63C, 0xA4, ct)).SequenceEqual(target)) throw new IOException("Restauro inativo divergiu.");
            }
            try
            {
                if (transitions)
                {
                    await Live(11, 1); await Live(8, 3); await Task.Delay(200, ct);
                    var baseline = await Capture("start-chorus");
                    if (baseline.Dsp[0x63C] != 3 || baseline.Dsp[0x63D] != 5 ||
                        BinaryPrimitives.ReadSingleLittleEndian(baseline.Dsp.AsSpan(0x688)) != 0.85f)
                        throw new IOException("Chorus inicial inesperado.");
                    var patch = new byte[4]; BinaryPrimitives.WriteSingleLittleEndian(patch, 0.425f);
                    var expectedDsp = baseline.Dsp.ToArray(); patch.CopyTo(expectedDsp, 0x688);
                    async Task CustomMix(string label)
                    {
                        await Context();
                        if (!ChorusStableEquals((await Capture("before-transition-" + label)).Dsp, baseline.Dsp)) throw new IOException("Estado mudou antes da transição.");
                        await intent("transition-mix-" + label, 0x688, baseline.Dsp.AsSpan(0x688, 4).ToArray(), patch);
                        await AmpAck(CubePacket.Write(4, 0x688, patch), ct);
                        if (!ChorusStableEquals((await Capture("custom-mix-" + label)).Dsp, expectedDsp)) throw new IOException("Mistura não aplicada.");
                    }
                    foreach (int next in new[] { 12, 7, 8 })
                    {
                        await CustomMix(next.ToString());
                        await Live(8, next); await Task.Delay(200, ct);
                        var changed = await Capture($"transition-{next}");
                        if (next == 12 ? changed.Dsp[0x63C] != 3 || changed.Dsp[0x63D] != 2 : changed.Dsp[0x63C] != 0)
                            throw new IOException("Transição não confirmada.");
                        await Live(8, 3); await Task.Delay(200, ct);
                        if (!ChorusStableEquals((await Capture($"return-{next}")).Dsp, baseline.Dsp))
                            throw new IOException("Reinicialização do chorus divergiu.");
                    }
                    await CustomMix("section");
                    await Live(11, 0); await Task.Delay(200, ct);
                    var off = await Capture("section-off");
                    if (off.Dsp[0x63C] != 0 || off.Dsp[0x694] != 0) throw new IOException("Seção não desligou.");
                    await Live(11, 1); await Task.Delay(200, ct);
                    if (!ChorusStableEquals((await Capture("section-on")).Dsp, baseline.Dsp)) throw new IOException("Seção não reinicializou.");
                }
                foreach (int mod in transitions ? Array.Empty<int>() : controls ? new[] { 3 } : new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 3 })
                {
                    await Live(11, 1); await Live(8, mod); await Task.Delay(200, ct);
                    PhaserResearchSnapshot? baseline = null;
                    for (int repeat = 0; repeat < 3; repeat++)
                    {
                        baseline = await Capture($"mod-{mod:D2}-{repeat}" + (mod == 3 && !controls ? $"-{sequence++}" : ""));
                        await Task.Delay(100, ct);
                    }
                    if (controls)
                    {
                        if (baseline!.Dsp[0x63C] != 3 || baseline.Dsp[0x63D] != 5 || baseline.Dsp[0x640] != 0 ||
                            BinaryPrimitives.ReadSingleLittleEndian(baseline.Dsp.AsSpan(0x644)) != 0.4f ||
                            BinaryPrimitives.ReadSingleLittleEndian(baseline.Dsp.AsSpan(0x648)) != 0.000031f ||
                            BinaryPrimitives.ReadSingleLittleEndian(baseline.Dsp.AsSpan(0x64C)) != 0 ||
                            BinaryPrimitives.ReadSingleLittleEndian(baseline.Dsp.AsSpan(0x688)) != 0.85f)
                            throw new IOException("Valores iniciais do chorus inesperados.");
                        byte[] F(float value) { var b = new byte[4]; BinaryPrimitives.WriteSingleLittleEndian(b, value); return b; }
                        var identityFilters = new byte[56]; F(1).CopyTo(identityFilters, 0); F(1).CopyTo(identityFilters, 28);
                        if (!baseline.Dsp.AsSpan(0x650, 56).SequenceEqual(identityFilters)) throw new IOException("Filtros não são identidade; feedback não habilitado.");
                        var trials = new (string Name, int Offset, byte[] Value)[]
                        {
                            ("depth-half", 0x644, F(0.2f)), ("depth-zero", 0x644, F(0)),
                            ("rate-low", 0x648, F(0.000008f)), ("rate-high", 0x648, F(0.000063f)),
                            ("mix-zero", 0x688, F(0)), ("mix-half", 0x688, F(0.425f)),
                            ("feedback-small", 0x64C, F(0.1f)), ("wave-triangle", 0x640, [1]),
                            ("wet-filter-gain", 0x650, F(0.5f)), ("combined-filter-gain", 0x66C, F(0.5f))
                        };
                        foreach (int cycle in new[] { 0, 1 })
                            foreach (var trial in trials)
                            {
                                string label = $"{cycle}-{trial.Name}";
                                var before = await Capture(label + "-before");
                                if (!ChorusStableEquals(before.Dsp, baseline.Dsp)) throw new IOException("Estado estável mudou antes do ensaio.");
                                var target = baseline.Dsp.ToArray(); trial.Value.CopyTo(target, trial.Offset);
                                await intent(label, (uint)trial.Offset, before.Dsp.AsSpan(trial.Offset, trial.Value.Length).ToArray(), trial.Value);
                                await AmpAck(CubePacket.Write(4, (uint)trial.Offset, trial.Value), ct);
                                if (!ChorusStableEquals((await Capture(label + "-immediate")).Dsp, target)) throw new IOException("Escrita isolada não confirmada.");
                                await Task.Delay(300, ct);
                                await Live(5, expected[slot * 16 + 5]);
                                await Live(8, 3);
                                if (!ChorusStableEquals((await Capture(label + "-delayed")).Dsp, target)) throw new IOException("Parâmetro não permaneceu.");
                                // Same algorithm reloads only depth and rate from the MOD table.
                                await Live(8, 4);
                                var changed = await Capture(label + "-mod4");
                                var reload = target.ToArray(); F(0.4f).CopyTo(reload, 0x644); F(0.000023f).CopyTo(reload, 0x648);
                                if (!ChorusStableEquals(changed.Dsp, reload)) throw new IOException("Recarga MOD divergiu.");
                                await Live(8, 3);
                                byte[] restore = baseline.Dsp.AsSpan(trial.Offset, trial.Value.Length).ToArray();
                                await intent(label + "-restore", (uint)trial.Offset,
                                    (await ReadInternal((uint)trial.Offset, trial.Value.Length, ct, 4)), restore);
                                await AmpAck(CubePacket.Write(4, (uint)trial.Offset, restore), ct);
                                if (!ChorusStableEquals((await Capture(label + "-restored")).Dsp, baseline.Dsp)) throw new IOException("Restauro de controle divergiu.");
                            }
                    }
                    await Live(8, original.Bank[slot * 16 + 8]); await Live(11, 0); await Task.Delay(200, ct);
                    await RestoreInactive();
                }
            }
            finally
            {
                if (IsFaulted) throw new IOException("Conexão perdida; restauração não confirmada. Use os backups e intenções preservados.");
                await Context();
                await Live(8, original.Bank[slot * 16 + 8]); await Live(11, 0); await Task.Delay(200, ct);
                await RestoreInactive();
                var restored = await Capture("restored");
                if (!ChorusStableEquals(restored.Dsp, original.Dsp)) throw new IOException("Restauro final divergiu fora dos campos dinâmicos.");
            }
        }
        finally { gate.Release(); }
    }
}
