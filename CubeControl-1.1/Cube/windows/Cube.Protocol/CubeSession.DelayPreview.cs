namespace Cube.Protocol;

public sealed partial class CubeSession
{
    public async Task<DelayRecovery> BeginDelayPreview(int physicalSlot, Func<DelayRecovery, Task> preserve,
        CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var snapshot = await ReadDelayCore(ct);
            if (physicalSlot is < 0 or > 2 || !snapshot.PhysicalState.SequenceEqual(new byte[] { (byte)physicalSlot, 1 }))
                throw new IOException("Selecione na aplicação o preset ativo fisicamente e use modo PRESET.");
            DelayParameters.ValidateActive(snapshot.Bank, snapshot.Dsp, snapshot.PhysicalState);
            var parameters = DelayParameters.Read(snapshot.Dsp);
            var recovery = new DelayRecovery("cubecontrol-delay-recovery-v1", DelayParameters.Mapping, IdentityHex,
                snapshot.Bank, snapshot.PhysicalState, snapshot.Dsp, parameters, parameters, "").Seal();
            recovery.Validate();
            await preserve(recovery);
            return recovery;
        }
        finally { gate.Release(); }
    }

    public async Task CheckDelayPreview(DelayRecovery recovery)
    {
        await gate.WaitAsync();
        try
        {
            RequireIdentity();
            // Idle monitoring checks only context and the delay block. Full protected
            // DSP comparison remains mandatory immediately before/after every mutation.
            var bank = await ReadInternal(0, 48, CancellationToken.None);
            var state = await ReadInternal(EffectsPhysicalStateOffset, 2, CancellationToken.None, 4);
            var dsp = recovery.OriginalDsp.ToArray();
            (await ReadChunks(4, 0x694, 0x4C, CancellationToken.None)).CopyTo(dsp, 0x694);
            CheckDelayContext(recovery, new(bank, dsp, state), recovering: false);
        }
        finally { gate.Release(); }
    }

    private void CheckDelayContext(DelayRecovery recovery, DelaySnapshot current, bool recovering)
    {
        recovery.Validate();
        if (IdentityHex != recovery.Identity || !current.PhysicalState.SequenceEqual(recovery.PhysicalState) ||
            !EffectContextMatches(3, current.Bank, recovery.Bank, current.Dsp, recovery.OriginalDsp, recovery.PhysicalState[0]))
            throw new IOException("O contexto do delay mudou; prévia interrompida e backup preservado.");
        if (!recovering && DelayParameters.Read(current.Dsp) != recovery.Target)
            throw new IOException("Parâmetros recarregados ou alterados externamente; prévia interrompida.");
        if (recovering)
        {
            var p = recovery.Previous.ApplyTo(recovery.OriginalDsp);
            var t = recovery.Target.ApplyTo(recovery.OriginalDsp);
            foreach (var (offset, length) in DelayParameters.Fields)
                for (int i = offset; i < offset + length; i++)
                    if (current.Dsp[i] != p[i] && current.Dsp[i] != t[i] && current.Dsp[i] != recovery.OriginalDsp[i])
                        throw new IOException("RAM não corresponde à transação preservada; backup mantido.");
        }
    }

    public async Task<DelayRecovery> ApplyDelayPreview(DelayRecovery recovery, DelayParameters parameters,
        Func<DelayRecovery, Task> preserve, CancellationToken ct = default)
    {
        parameters.Validate();
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadDelayCore(ct);
            CheckDelayContext(recovery, before, recovering: false);
            var transaction = (recovery with { Previous = recovery.Target, Target = parameters }).Seal();
            await preserve(transaction); // Durable intent before the first byte; also covers partial writes.
            ct.ThrowIfCancellationRequested();
            await WriteDelayFields(before, parameters, CancellationToken.None);
            var after = await ReadDelayCore(CancellationToken.None);
            VerifyEffectMutation(3, new(before.Bank, before.Dsp, before.PhysicalState), new(after.Bank, after.Dsp, after.PhysicalState));
            CheckDelayContext(transaction, after, recovering: false);
            return transaction;
        }
        finally { gate.Release(); }
    }

    public async Task RestoreDelayPreview(DelayRecovery recovery, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadDelayCore(ct);
            CheckDelayContext(recovery, before, recovering: true);
            var original = DelayParameters.Read(recovery.OriginalDsp);
            await WriteDelayFields(before, original, CancellationToken.None);
            var after = await ReadDelayCore(CancellationToken.None);
            VerifyEffectMutation(3, new(before.Bank, before.Dsp, before.PhysicalState), new(after.Bank, after.Dsp, after.PhysicalState));
            CheckDelayContext((recovery with { Target = original }).Seal(), after, recovering: false);
        }
        finally { gate.Release(); }
    }

    private async Task WriteDelayFields(DelaySnapshot before, DelayParameters target, CancellationToken ct)
    {
        byte[] after = target.ApplyTo(before.Dsp);
        byte[] expected = before.Dsp.ToArray();
        foreach (var (offset, length) in DelayParameters.Fields)
        {
            if (expected.AsSpan(offset, length).SequenceEqual(after.AsSpan(offset, length))) continue;
            if (!(await ReadInternal(0, 48, ct)).SequenceEqual(before.Bank) ||
                !(await ReadInternal(EffectsPhysicalStateOffset, 2, ct, 4)).SequenceEqual(before.PhysicalState) ||
                !(await ReadChunks(4, 0x694, 0x4C, ct)).AsSpan().SequenceEqual(expected.AsSpan(0x694, 0x4C)))
                throw new IOException("Contexto mudou entre escritas; prévia interrompida e backup preservado.");
            await AmpAck(CubePacket.Write(4, (uint)offset, after.AsSpan(offset, length)), ct);
            after.AsSpan(offset, length).CopyTo(expected.AsSpan(offset, length));
        }
    }
}
