namespace Cube.Protocol;

public sealed partial class CubeSession
{
    public async Task<ReverbRecovery> BeginReverbPreview(int physicalSlot, Func<ReverbRecovery, Task> preserve,
        CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var snapshot = await ReadReverbCore(ct);
            if (physicalSlot is < 0 or > 2 || !snapshot.PhysicalState.SequenceEqual(new byte[] { (byte)physicalSlot, 1 }))
                throw new IOException("Selecione na aplicação o preset ativo fisicamente e use modo PRESET.");
            ReverbParameters.ValidateActive(snapshot.Bank, snapshot.Dsp, snapshot.PhysicalState);
            var parameters = ReverbParameters.Read(snapshot.Dsp);
            var recovery = new ReverbRecovery("cubecontrol-reverb-recovery-v1", ReverbParameters.Mapping, IdentityHex,
                snapshot.Bank, snapshot.PhysicalState, snapshot.Dsp, parameters, parameters);
            recovery.Validate();
            await preserve(recovery);
            return recovery;
        }
        finally { gate.Release(); }
    }

    public async Task CheckReverbPreview(ReverbRecovery recovery)
    {
        await gate.WaitAsync();
        try
        {
            RequireIdentity();
            // Idle monitoring checks only context and the reverb block. Full protected
            // DSP comparison remains mandatory immediately before/after every mutation.
            var bank = await ReadInternal(0, 48, CancellationToken.None);
            var state = await ReadInternal(EffectsPhysicalStateOffset, 2, CancellationToken.None, 4);
            var dsp = recovery.OriginalDsp.ToArray();
            (await ReadChunks(4, 0x6E0, 0x84, CancellationToken.None)).CopyTo(dsp, 0x6E0);
            CheckReverbContext(recovery, new(bank, dsp, state), recovering: false);
        }
        finally { gate.Release(); }
    }

    private void CheckReverbContext(ReverbRecovery recovery, ReverbSnapshot current, bool recovering)
    {
        recovery.Validate();
        if (IdentityHex != recovery.Identity || !current.Bank.SequenceEqual(recovery.Bank) ||
            !current.PhysicalState.SequenceEqual(recovery.PhysicalState) ||
            !ReverbParameters.StableEquals(current.Dsp, recovery.OriginalDsp, ignoreControls: true))
            throw new IOException("O contexto do reverb mudou; prévia interrompida e backup preservado.");
        if (!recovering && ReverbParameters.Read(current.Dsp) != recovery.Target)
            throw new IOException("Parâmetros recarregados ou alterados externamente; prévia interrompida.");
        if (recovering)
        {
            var p = recovery.Previous.ApplyTo(recovery.OriginalDsp);
            var t = recovery.Target.ApplyTo(recovery.OriginalDsp);
            foreach (var (offset, length) in ReverbParameters.Fields)
                for (int i = offset; i < offset + length; i++)
                    if (current.Dsp[i] != p[i] && current.Dsp[i] != t[i] && current.Dsp[i] != recovery.OriginalDsp[i])
                        throw new IOException("RAM não corresponde à transação preservada; backup mantido.");
        }
    }

    public async Task<ReverbRecovery> ApplyReverbPreview(ReverbRecovery recovery, ReverbParameters parameters,
        Func<ReverbRecovery, Task> preserve, CancellationToken ct = default)
    {
        parameters.Validate();
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadReverbCore(ct);
            CheckReverbContext(recovery, before, recovering: false);
            var transaction = recovery with { Previous = recovery.Target, Target = parameters };
            await preserve(transaction); // Durable intent before the first byte; also covers partial writes.
            ct.ThrowIfCancellationRequested();
            await WriteReverbFields(before, parameters, CancellationToken.None);
            var after = await ReadReverbCore(CancellationToken.None);
            CheckReverbContext(transaction, after, recovering: false);
            return transaction;
        }
        finally { gate.Release(); }
    }

    public async Task RestoreReverbPreview(ReverbRecovery recovery, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadReverbCore(ct);
            CheckReverbContext(recovery, before, recovering: true);
            var original = ReverbParameters.Read(recovery.OriginalDsp);
            await WriteReverbFields(before, original, CancellationToken.None);
            var after = await ReadReverbCore(CancellationToken.None);
            CheckReverbContext(recovery with { Target = original }, after, recovering: false);
        }
        finally { gate.Release(); }
    }

    private async Task WriteReverbFields(ReverbSnapshot before, ReverbParameters target, CancellationToken ct)
    {
        byte[] after = target.ApplyTo(before.Dsp);
        byte[] expected = before.Dsp.ToArray();
        foreach (var (offset, length) in ReverbParameters.Fields)
        {
            if (expected.AsSpan(offset, length).SequenceEqual(after.AsSpan(offset, length))) continue;
            if (!(await ReadInternal(0, 48, ct)).SequenceEqual(before.Bank) ||
                !(await ReadInternal(EffectsPhysicalStateOffset, 2, ct, 4)).SequenceEqual(before.PhysicalState) ||
                !(await ReadChunks(4, 0x6E0, 0x84, ct)).AsSpan().SequenceEqual(expected.AsSpan(0x6E0, 0x84)))
                throw new IOException("Contexto mudou entre escritas; prévia interrompida e backup preservado.");
            await AmpAck(CubePacket.Write(4, (uint)offset, after.AsSpan(offset, length)), ct);
            after.AsSpan(offset, length).CopyTo(expected.AsSpan(offset, length));
        }
    }
}
