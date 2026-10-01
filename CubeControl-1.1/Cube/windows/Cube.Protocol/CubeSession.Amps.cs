using System.Buffers.Binary;

namespace Cube.Protocol;

public sealed partial class CubeSession
{
    public static uint AmpAddress(int slot) => slot is >= 0 and < 9 ? 0x60000u + (uint)slot * 4096 : throw new ArgumentOutOfRangeException(nameof(slot));

    public async Task<byte[]> ReadAmpSlot(int slot, CancellationToken ct = default)
    {
        uint address = AmpAddress(slot);
        await gate.WaitAsync(ct);
        try { RequireIdentity(); return await ReadChunks(0, address, AmpImage.Size, ct); }
        finally { gate.Release(); }
    }

    public async Task<byte[]> ReadAmpRam(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try { RequireIdentity(); return await ReadChunks(4, 0x1C, AmpImage.BodyLength, ct); }
        finally { gate.Release(); }
    }

    private async Task AmpAck(CubePacket packet, CancellationToken ct)
    {
        var reply = await Exchange(packet, p => p.Command == 0 && p.Payload.Length == 1, ct);
        if (reply.Payload[0] != 0) throw new IOException($"Escrita de amp recusada: ACK {reply.Payload[0]}.");
    }

    public async Task ApplyAmpRam(byte[] replacement, Func<byte[], Task>? preserve = null, CancellationToken ct = default)
    {
        if (replacement.Length != AmpImage.BodyLength) throw new ArgumentException("Corpo de amp inválido.");
        var target = replacement.ToArray();
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            var before = await ReadChunks(4, 0x1C, AmpImage.BodyLength, ct);
            if (preserve != null) await preserve(before.ToArray());
            if (before.SequenceEqual(target)) return;
            var bank = await ReadInternal(0, 48, ct);
            ct.ThrowIfCancellationRequested();
            for (int offset = 0; offset < target.Length; offset += 64)
            {
                int count = Math.Min(64, target.Length - offset);
                int first = 0, last = count - 1;
                while (first < count && before[offset + first] == target[offset + first]) first++;
                if (first == count) continue;
                while (last > first && before[offset + last] == target[offset + last]) last--;
                // Keep complete float-sized words while omitting unchanged ends of each RAM block.
                first = first / 4 * 4; last = Math.Min(count, (last / 4 + 1) * 4);
                await AmpAck(CubePacket.Write(4, 0x1Cu + (uint)offset + (uint)first, target.AsSpan(offset + first, last - first)), CancellationToken.None);
            }
            if (!(await ReadChunks(4, 0x1C, target.Length, CancellationToken.None)).SequenceEqual(target))
                throw new IOException("A prévia do amp não foi confirmada. Restaure o estado anterior.");
            if (!(await ReadInternal(0, 48, CancellationToken.None)).SequenceEqual(bank))
                throw new IOException("O banco mudou durante a prévia. Releia o pedal.");
        }
        finally { gate.Release(); }
    }

    public async Task ReplaceAmp(int slot, byte[] expected, byte[] replacement, Func<byte[], Task> preserve,
        IProgress<TransferProgress>? progress = null, CancellationToken ct = default)
    {
        AmpImage.Validate(replacement);
        if (expected.Length != AmpImage.Size) throw new ArgumentException("Setor original inválido.");
        uint address = AmpAddress(slot); var baseline = expected.ToArray(); var target = replacement.ToArray();
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            var current = await ReadChunks(0, address, AmpImage.Size, ct);
            if (!current.SequenceEqual(baseline)) throw new InvalidOperationException("O slot de amp mudou. Leia novamente.");
            await preserve(current);
            ct.ThrowIfCancellationRequested();
            var payload = new byte[5]; BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(1), address);
            await AmpAck(new CubePacket(0x21, payload), CancellationToken.None);
            for (int offset = 0; offset < AmpImage.Size; offset += 64)
            {
                await AmpAck(CubePacket.Write(0, address + (uint)offset, target.AsSpan(offset, 64)), CancellationToken.None);
                progress?.Report(new("Gravando amp", offset + 64, AmpImage.Size));
            }
            if (!(await ReadChunks(0, address, AmpImage.Size, CancellationToken.None, progress)).SequenceEqual(target))
                throw new IOException("Gravação do amp não confirmada. Use o arquivo de recuperação preservado.");
        }
        finally { gate.Release(); }
    }
}
