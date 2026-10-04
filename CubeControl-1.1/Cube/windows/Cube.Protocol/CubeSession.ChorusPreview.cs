namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private async Task<PhaserResearchSnapshot> ReadChorusPreviewCore(CancellationToken ct)
    {
        RequireIdentity();
        await VerifyChorusFirmware(ct);
        return await ReadPhaserCore(ct);
    }

    public async Task<ChorusRecovery> BeginChorusPreview(int physicalSlot, Func<ChorusRecovery, Task> preserve,
        CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var snapshot = await ReadChorusPreviewCore(ct);
            if (physicalSlot is < 0 or > 2 || !snapshot.PhysicalState.SequenceEqual(new byte[] { (byte)physicalSlot, 1 }))
                throw new IOException("Selecione na aplicação o preset ativo fisicamente e use modo PRESET.");
            if (snapshot.Bank[physicalSlot * 16 + 8] is > 6 || snapshot.Bank[physicalSlot * 16 + 11] != 1 || snapshot.Dsp[0x63C] != 3)
                throw new ArgumentException("Ative o chorus e a seção delay/modulação no preset físico antes de ler.");
            var parameters = ChorusParameters.Read(snapshot.Dsp);
            var recovery = new ChorusRecovery("cubecontrol-chorus-recovery-v1", ChorusParameters.Mapping, IdentityHex,
                snapshot.Bank, snapshot.PhysicalState, snapshot.Dsp, parameters, parameters);
            recovery.Validate();
            await preserve(recovery);
            return recovery;
        }
        finally { gate.Release(); }
    }

    public async Task CheckChorusPreview(ChorusRecovery recovery)
    {
        await gate.WaitAsync();
        try
        {
            RequireIdentity();
            // Idle monitoring checks only context and the modulation block. Full protected
            // DSP comparison remains mandatory immediately before/after every mutation.
            var bank = await ReadInternal(0, 48, CancellationToken.None);
            var state = await ReadInternal(EffectsPhysicalStateOffset, 2, CancellationToken.None, 4);
            var dsp = recovery.OriginalDsp.ToArray();
            (await ReadInternal(0x63C, 0x58, CancellationToken.None, 4)).CopyTo(dsp, 0x63C);
            CheckChorusContext(recovery, new(bank, dsp, state), recovering: false);
        }
        finally { gate.Release(); }
    }

    private void CheckChorusContext(ChorusRecovery recovery, PhaserResearchSnapshot current, bool recovering)
    {
        recovery.Validate();
        if (IdentityHex != recovery.Identity || !current.PhysicalState.SequenceEqual(recovery.PhysicalState) ||
            !EffectContextMatches(1, current.Bank, recovery.Bank, current.Dsp, recovery.OriginalDsp, recovery.PhysicalState[0]))
            throw new IOException("O contexto do chorus mudou; prévia interrompida e backup preservado.");
        var parameters = ChorusParameters.Read(current.Dsp);
        if (!recovering && parameters != recovery.Target)
            throw new IOException("Parâmetros recarregados ou alterados externamente; prévia interrompida.");
        if (recovering)
        {
            var p = recovery.Previous; var t = recovery.Target; var original = ChorusParameters.Read(recovery.OriginalDsp);
            bool IsKnown<T>(T actual, T previous, T target, T initial) =>
                EqualityComparer<T>.Default.Equals(actual, previous) || EqualityComparer<T>.Default.Equals(actual, target) ||
                EqualityComparer<T>.Default.Equals(actual, initial);
            if (!IsKnown(parameters.Depth, p.Depth, t.Depth, original.Depth) ||
                !IsKnown(parameters.Step, p.Step, t.Step, original.Step) ||
                !IsKnown(parameters.Mix, p.Mix, t.Mix, original.Mix) ||
                !IsKnown(parameters.Feedback, p.Feedback, t.Feedback, original.Feedback) ||
                !IsKnown(parameters.Waveform, p.Waveform, t.Waveform, original.Waveform) ||
                !IsKnown(parameters.WetGain, p.WetGain, t.WetGain, original.WetGain) ||
                !IsKnown(parameters.CombinedGain, p.CombinedGain, t.CombinedGain, original.CombinedGain))
                throw new IOException("RAM não corresponde à transação preservada. Recuperação automática interrompida.");
        }
    }

    public async Task<ChorusRecovery> ApplyChorusPreview(ChorusRecovery recovery, ChorusParameters parameters,
        Func<ChorusRecovery, Task> preserve, CancellationToken ct = default)
    {
        parameters.Validate();
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadChorusPreviewCore(ct);
            CheckChorusContext(recovery, before, recovering: false);
            var transaction = recovery with { Previous = recovery.Target, Target = parameters };
            await preserve(transaction); // Durable intent before the first byte; also covers partial writes.
            ct.ThrowIfCancellationRequested();
            await WriteChorusFields(transaction, before.Dsp, parameters, CancellationToken.None);
            var after = await ReadChorusPreviewCore(CancellationToken.None);
            VerifyEffectMutation(1, new(before.Bank, before.Dsp, before.PhysicalState), new(after.Bank, after.Dsp, after.PhysicalState));
            CheckChorusContext(transaction, after, recovering: false);
            return transaction;
        }
        finally { gate.Release(); }
    }

    public async Task RestoreChorusPreview(ChorusRecovery recovery, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadChorusPreviewCore(ct);
            CheckChorusContext(recovery, before, recovering: true);
            var original = ChorusParameters.Read(recovery.OriginalDsp);
            await WriteChorusFields(recovery, before.Dsp, original, CancellationToken.None);
            var after = await ReadChorusPreviewCore(CancellationToken.None);
            VerifyEffectMutation(1, new(before.Bank, before.Dsp, before.PhysicalState), new(after.Bank, after.Dsp, after.PhysicalState));
            CheckChorusContext(recovery with { Target = original }, after, recovering: false);
        }
        finally { gate.Release(); }
    }

    private async Task WriteChorusFields(ChorusRecovery recovery, byte[] before, ChorusParameters target, CancellationToken ct)
    {
        byte[] after = target.ApplyTo(before);
        // Reduce feedback before other changes; increase it only after bounded gains are installed.
        int[] offsets = target.Feedback < ChorusParameters.Read(before).Feedback
            ? [0x64C, 0x640, 0x644, 0x648, 0x650, 0x66C, 0x688]
            : [0x640, 0x644, 0x648, 0x650, 0x66C, 0x688, 0x64C];
        var expected = before.ToArray();
        foreach (int offset in offsets)
        {
            int length = offset == 0x640 ? 1 : 4;
            if (!before.AsSpan(offset, length).SequenceEqual(after.AsSpan(offset, length)))
            {
                // Recheck the physical context between field writes, including partial imports/restores.
                var bank = await ReadInternal(0, 48, ct);
                var state = await ReadInternal(EffectsPhysicalStateOffset, 2, ct, 4);
                var current = expected.ToArray();
                (await ReadInternal(0x63C, 0x58, ct, 4)).CopyTo(current, 0x63C);
                CheckChorusContext(recovery with { Target = ChorusParameters.Read(expected) }, new(bank, current, state), recovering: false);
                await AmpAck(CubePacket.Write(4, (uint)offset, after.AsSpan(offset, length)), ct);
                after.AsSpan(offset, length).CopyTo(expected.AsSpan(offset));
            }
        }
    }
}
