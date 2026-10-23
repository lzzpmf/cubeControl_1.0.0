namespace Cube.Protocol;

public sealed record IrRamSnapshot(byte Flag, byte[] Payload);
public sealed record TransferProgress(string Stage, int Done, int Total);

public sealed partial class CubeSession
{
    public static uint IrAddress(int slot) => slot is >= 0 and <= 7 ? 0x69000u + (uint)(slot * 4096) : throw new ArgumentOutOfRangeException(nameof(slot));

    private async Task<byte[]> ReadChunks(byte region, uint address, int length, CancellationToken ct, IProgress<TransferProgress>? progress = null)
    {
        var result = new byte[length];
        for (int offset = 0; offset < length; offset += 113)
        {
            int count = Math.Min(113, length - offset);
            (await ReadInternal(address + (uint)offset, count, ct, region).ConfigureAwait(false)).CopyTo(result, offset);
            progress?.Report(new("Lendo", offset + count, length));
        }
        return result;
    }

    public async Task<byte[]> ReadIrSlot(int slot, IProgress<TransferProgress>? progress = null, CancellationToken ct = default)
    {
        uint address = IrAddress(slot); await gate.WaitAsync(ct);
        try {
            using var measurement = MeasureOperation(); RequireIdentity(); return await ReadChunks(0, address, 4096, ct, progress); }
        finally { gate.Release(); }
    }

    public async Task<byte[][]> ReadIrInventory(IProgress<TransferProgress>? progress = null, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            using var measurement = MeasureOperation();
            RequireIdentity(); var sectors = new byte[8][];
            for (int slot = 0; slot < 8; slot++)
            {
                sectors[slot] = await ReadChunks(0, IrAddress(slot), 4096, ct);
                progress?.Report(new("Identificando cabinets", slot + 1, 8));
            }
            return sectors;
        }
        finally { gate.Release(); }
    }

    public async Task<IrRamSnapshot> ReadIrRam(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            using var measurement = MeasureOperation();
            RequireIdentity();
            var flag = await ReadInternal(0x764, 1, ct, 4);
            if (flag[0] > 3)
                throw new IOException($"Estado do IR não suportado para snapshot (0x{flag[0]:X2}). Aguarde e releia antes de iniciar a prévia.");
            return new(flag[0], await ReadChunks(4, 0x768, 2052, ct));
        }
        finally { gate.Release(); }
    }

    private async Task Ack(CubePacket packet, CancellationToken ct)
    {
        var ack = await Exchange(packet, p => p.Command == 0 && p.Payload.Length == 1, ct);
        if (ack.Payload[0] > 1) throw new IOException($"ACK IR desconhecido: {ack.Payload[0]}");
    }

    private async Task SetIrFlag(byte value, CancellationToken ct)
    {
        await WriteInternal(0x764, [value], ct, 4);
        await Ack(CubePacket.Register(0x764, 1), ct);
    }

    public async Task ApplyIrRam(IrRamSnapshot state, IProgress<TransferProgress>? progress = null, CancellationToken ct = default)
    {
        if (state.Payload.Length != 2052) throw new ArgumentException("Estado de IR inválido.");
        if (state.Flag > 3) throw new ArgumentException($"Estado de ativação do IR não suportado: 0x{state.Flag:X2}.");
        var payload = state.Payload.ToArray(); await gate.WaitAsync(ct);
        try
        {
            using var measurement = MeasureOperation();
            RequireIdentity(); await SetIrFlag(0, ct);
            for (int offset = 0; offset < payload.Length; offset += 128)
            {
                int count = Math.Min(128, payload.Length - offset);
                await WriteInternal(0x768u + (uint)offset, payload.AsSpan(offset, count).ToArray(), ct, 4);
                progress?.Report(new("Prévia em RAM", offset + count, payload.Length));
            }
            await Ack(CubePacket.Register(0x768, 2052), ct);
            // Verify while bypassed, before the DSP starts consuming the new coefficients.
            await VerifyIrPayload(payload, ct);
            // These are DSP states, not a bit mask: 1 initializes, 2 starts processing,
            // 3 is running. Reinitialize active snapshots to clear convolution history.
            byte requested = state.Flag == 0 ? (byte)0 : (byte)1;
            await SetIrFlag(requested, ct);
            await VerifyIrPayload(payload, ct);
            byte flag = (await ReadInternal(0x764, 1, ct, 4))[0];
            Log?.Invoke($"IR RAM: snapshot=0x{state.Flag:X2}, solicitado=0x{requested:X2}, recebido=0x{flag:X2}; 2052 bytes conferidos.");
            bool active = flag is 1 or 2 or 3;
            if (requested == 0 ? flag != 0 : !active)
                throw new IOException($"Ativação do IR em RAM não confirmada (solicitado 0x{requested:X2}, recebido 0x{flag:X2}). Snapshot preservado.");
        }
        finally { gate.Release(); }
    }

    private async Task VerifyIrPayload(byte[] payload, CancellationToken ct)
    {
        var actual = await ReadChunks(4, 0x768, 2052, ct);
        int first = -1;
        for (int i = 0; i < actual.Length; i++)
            if (actual[i] != payload[i]) { first = i; break; }
        if (first >= 0)
        {
            Log?.Invoke($"IR RAM divergente: endereço=0x{0x768 + first:X}, esperado=0x{payload[first]:X2}, recebido=0x{actual[first]:X2}.");
            throw new IOException($"Leitura de retorno do IR em RAM divergiu no endereço 0x{0x768 + first:X} (esperado 0x{payload[first]:X2}, recebido 0x{actual[first]:X2}). Snapshot preservado.");
        }
    }

    public async Task ReplaceIr(int slot, byte[] expected, byte[] replacement, Func<byte[], Task> backup,
        IProgress<TransferProgress>? progress = null, CancellationToken ct = default)
    {
        if (expected.Length != 4096 || replacement.Length != 4096) throw new ArgumentException("O setor IR deve ter 4096 bytes.");
        uint address = IrAddress(slot); var data = replacement.ToArray(); var baseline = expected.ToArray();
        await gate.WaitAsync(ct);
        try
        {
            using var measurement = MeasureOperation();
            RequireIdentity(); var current = await ReadChunks(0, address, 4096, ct, progress);
            if (!current.SequenceEqual(baseline)) throw new InvalidOperationException("O slot IR mudou. Leia novamente antes de gravar.");
            await backup(current); // Failure here MUST prevent erase.
            ct.ThrowIfCancellationRequested();
            // Once erasure starts, finish the sector and verification; do not cancel halfway through flash.
            var writeToken = CancellationToken.None;
            progress?.Report(new("Apagando o slot", 0, 4096));
            await Ack(CubePacket.EraseIrSector(address), writeToken);
            for (int offset = 0; offset < 4096; offset += 128)
            {
                await WriteInternal(address + (uint)offset, data.AsSpan(offset, 128).ToArray(), writeToken, 0);
                progress?.Report(new("Gravando", offset + 128, 4096));
            }
            var readback = await ReadChunks(0, address, 4096, writeToken, progress);
            if (!readback.SequenceEqual(data)) throw new IOException("IR não confirmado. O backup original está disponível para recuperação.");
            progress?.Report(new("IR verificado", 4096, 4096));
        }
        finally { gate.Release(); }
    }
}
