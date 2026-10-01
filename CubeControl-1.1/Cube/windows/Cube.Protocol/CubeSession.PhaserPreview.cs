namespace Cube.Protocol;

public sealed partial class CubeSession
{
    public async Task<PhaserRecovery> BeginPhaserPreview(int physicalSlot, Func<PhaserRecovery, Task> preserve,
        CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var snapshot = await ReadPhaserCore(ct);
            if (physicalSlot is < 0 or > 2 || !snapshot.PhysicalState.SequenceEqual(new byte[] { (byte)physicalSlot, 1 }))
                throw new IOException("Selecione na aplicação o preset ativo fisicamente e use modo PRESET.");
            if (snapshot.Bank[physicalSlot * 16 + 8] is < 9 or > 15 || snapshot.Bank[physicalSlot * 16 + 11] != 1 || snapshot.Dsp[0x63C] != 3)
                throw new ArgumentException("Ative o phaser e a seção delay/modulação no preset físico antes de ler.");
            var parameters = PhaserParameters.Read(snapshot.Dsp);
            var recovery = new PhaserRecovery("cubecontrol-phaser-recovery-v1", PhaserParameters.Mapping, IdentityHex,
                snapshot.Bank, snapshot.PhysicalState, snapshot.Dsp, parameters, parameters);
            recovery.Validate();
            await preserve(recovery);
            return recovery;
        }
        finally { gate.Release(); }
    }

    public async Task CheckPhaserPreview(PhaserRecovery recovery)
    {
        await gate.WaitAsync();
        try
        {
            RequireIdentity();
            // Idle monitoring checks only context and the modulation block. Full protected
            // DSP comparison remains mandatory immediately before/after every mutation.
            var bank = await ReadInternal(0, 48, CancellationToken.None);
            var state = await ReadInternal(0xFFFD8202, 2, CancellationToken.None, 4);
            var dsp = recovery.OriginalDsp.ToArray();
            (await ReadInternal(0x63C, 0x58, CancellationToken.None, 4)).CopyTo(dsp, 0x63C);
            CheckPhaserContext(recovery, new(bank, dsp, state), recovering: false);
        }
        finally { gate.Release(); }
    }

    private void CheckPhaserContext(PhaserRecovery recovery, PhaserResearchSnapshot current, bool recovering)
    {
        recovery.Validate();
        if (IdentityHex != recovery.Identity || !current.Bank.SequenceEqual(recovery.Bank) ||
            !current.PhysicalState.SequenceEqual(recovery.PhysicalState) ||
            !PhaserParameters.StableEquals(current.Dsp, recovery.OriginalDsp, ignoreControls: true))
            throw new IOException("O contexto do phaser mudou; prévia interrompida e backup preservado.");
        var parameters = PhaserParameters.Read(current.Dsp);
        if (!recovering && parameters != recovery.Target)
            throw new IOException("Parâmetros recarregados ou alterados externamente; prévia interrompida.");
        if (recovering)
        {
            var p = recovery.Previous; var t = recovery.Target; var original = PhaserParameters.Read(recovery.OriginalDsp);
            bool IsKnown<T>(T actual, T previous, T target, T initial) =>
                EqualityComparer<T>.Default.Equals(actual, previous) || EqualityComparer<T>.Default.Equals(actual, target) ||
                EqualityComparer<T>.Default.Equals(actual, initial);
            if (!IsKnown(parameters.Step, p.Step, t.Step, original.Step) ||
                !IsKnown(parameters.Lower, p.Lower, t.Lower, original.Lower) ||
                !IsKnown(parameters.Upper, p.Upper, t.Upper, original.Upper) ||
                !IsKnown(parameters.Level, p.Level, t.Level, original.Level) ||
                !IsKnown(parameters.Sections, p.Sections, t.Sections, original.Sections))
                throw new IOException("RAM não corresponde à transação preservada. Recuperação automática interrompida.");
        }
    }

    public async Task<PhaserRecovery> ApplyPhaserPreview(PhaserRecovery recovery, PhaserParameters parameters,
        Func<PhaserRecovery, Task> preserve, CancellationToken ct = default)
    {
        parameters.Validate();
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadPhaserCore(ct);
            CheckPhaserContext(recovery, before, recovering: false);
            var transaction = recovery with { Previous = recovery.Target, Target = parameters };
            await preserve(transaction); // Durable intent before the first byte; also covers partial writes.
            ct.ThrowIfCancellationRequested();
            await WritePhaserFields(before.Dsp, parameters, CancellationToken.None);
            var after = await ReadPhaserCore(CancellationToken.None);
            CheckPhaserContext(transaction, after, recovering: false);
            return transaction;
        }
        finally { gate.Release(); }
    }

    public async Task RestorePhaserPreview(PhaserRecovery recovery, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var before = await ReadPhaserCore(ct);
            CheckPhaserContext(recovery, before, recovering: true);
            var original = PhaserParameters.Read(recovery.OriginalDsp);
            await WritePhaserFields(before.Dsp, original, CancellationToken.None);
            var after = await ReadPhaserCore(CancellationToken.None);
            CheckPhaserContext(recovery with { Target = original }, after, recovering: false);
        }
        finally { gate.Release(); }
    }

    private async Task WritePhaserFields(byte[] before, PhaserParameters target, CancellationToken ct)
    {
        byte[] after = target.ApplyTo(before);
        // Order the two endpoints so every intermediate interval remains valid.
        bool raiseUpperFirst = target.Lower > PhaserParameters.Read(before).Upper;
        int[] offsets = [0x641, 0x644, raiseUpperFirst ? 0x650 : 0x64C, raiseUpperFirst ? 0x64C : 0x650, 0x654];
        foreach (int offset in offsets)
        {
            int length = offset == 0x641 ? 1 : 4;
            if (!before.AsSpan(offset, length).SequenceEqual(after.AsSpan(offset, length)))
                await AmpAck(CubePacket.Write(4, (uint)offset, after.AsSpan(offset, length)), ct);
        }
    }
}
