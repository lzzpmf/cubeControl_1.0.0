using System.Buffers.Binary;
using System.Text;

namespace Cube.Protocol;

public interface IMidiConnection : IDisposable
{
    event Action<byte[]>? Received;
    void Send(byte[] sysex);
}

public sealed partial class CubeSession : IDisposable
{
    private readonly IMidiConnection connection;
    private readonly SemaphoreSlim gate = new(1);
    private readonly object sync = new();
    private readonly SysExAssembler assembler = new();
    private TaskCompletionSource<CubePacket>? pending;
    private Func<CubePacket, bool>? match;
    private bool faulted;
    private bool identified;
    public string Identity { get; private set; } = "";
    public string IdentityHex { get; private set; } = "";
    public bool IsFaulted => faulted;
    public event Action<string>? Log;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(1.5);

    public CubeSession(IMidiConnection connection)
    {
        this.connection = connection;
        connection.Received += Receive;
    }

    private void Receive(byte[] data)
    {
        lock (sync)
        {
            foreach (var bytes in assembler.Feed(data))
            {
                Log?.Invoke("RX " + Convert.ToHexString(bytes));
                try
                {
                    var packet = UsbSysEx.Decode(bytes);
                    if (match?.Invoke(packet) == true) pending?.TrySetResult(packet);
                }
                catch (FormatException ex) { Log?.Invoke(ex.Message); }
            }
        }
    }

    private async Task<CubePacket> Exchange(CubePacket packet, Func<CubePacket, bool> predicate, CancellationToken ct)
    {
        if (faulted) throw new InvalidOperationException("Reconecte o aparelho antes de tentar novamente.");
        var waiter = new TaskCompletionSource<CubePacket>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (sync) { pending = waiter; match = predicate; }
        try
        {
            byte[] bytes = UsbSysEx.Encode(packet);
            Log?.Invoke("TX " + Convert.ToHexString(bytes));
            connection.Send(bytes);
            return await waiter.Task.WaitAsync(Timeout, ct).ConfigureAwait(false);
        }
        catch { faulted = true; throw; } // No transaction ID: never let a late reply satisfy a later request.
        finally { lock (sync) { pending = null; match = null; } }
    }

    public async Task<string> Identify(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var reply = await Exchange(CubePacket.Identify(), p => p.Command == 0x11 && p.Payload.Length > 0, ct);
            string name = Encoding.ASCII.GetString(reply.Payload.TakeWhile(b => b is >= 32 and <= 126).ToArray()).Trim();
            identified = name.Equals("CubeBaby", StringComparison.OrdinalIgnoreCase)
                || name.Equals("SINCO-CubeBaby", StringComparison.OrdinalIgnoreCase);
            if (!identified) throw new InvalidOperationException($"Identidade recebida: {name}. Perfil Cube Baby guitarra não confirmado.");
            Identity = name; IdentityHex = Convert.ToHexString(reply.Payload);
            return name;
        }
        finally { gate.Release(); }
    }

    private void RequireIdentity()
    {
        if (!identified) throw new InvalidOperationException("Identifique o Cube Baby antes de acessar os presets.");
    }

    private async Task<byte[]> ReadInternal(uint address, int length, CancellationToken ct, byte region = 5)
    {
        var response = await Exchange(CubePacket.Read(region, address, length), p =>
            p.Command == 0x23 && p.Payload.Length == length + 8 && p.Payload[0] == region &&
            BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1)) == address &&
            CubePacket.Read24(p.Payload.AsSpan(5)) == length, ct).ConfigureAwait(false);
        return response.Payload[8..];
    }

    public async Task<byte[]> ReadBank(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try { RequireIdentity(); return await ReadInternal(0, 48, ct); }
        finally { gate.Release(); }
    }

    private async Task WriteInternal(uint address, byte[] data, CancellationToken ct, byte region = 5)
    {
        var ack = await Exchange(CubePacket.Write(region, address, data), p => p.Command == 0 && p.Payload.Length == 1, ct);
        // Captures use raw ACK 0 on live writes. Readback, where supported, is authoritative.
        Log?.Invoke($"ACK bruto: {ack.Payload[0]}");
        if (ack.Payload[0] > 1) throw new IOException($"Resposta de escrita desconhecida: {ack.Payload[0]}");
    }

    public async Task WriteLive(int slot, int offset, int value, CancellationToken ct = default)
        => await WriteLiveChecked(slot, offset, value, null, ct);

    public async Task<byte[]> WriteLiveChecked(int slot, int offset, int value, byte[]? expected, CancellationToken ct = default)
    {
        uint address = CubeBabyProfile.LiveAddress(slot, offset);
        if (value < 0 || value > CubeBabyProfile.Parameters[offset].Maximum) throw new ArgumentOutOfRangeException(nameof(value));
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            if (expected != null && !(await ReadInternal(0, 48, ct)).SequenceEqual(expected))
                throw new InvalidOperationException("O banco mudou no aparelho. Releia antes de continuar.");
            await WriteInternal(address, [(byte)value], ct);
            // Physical test: high-address reads return zero; the bank at address 0 reflects live edits.
            var bank = await ReadInternal(0, 48, ct);
            if (bank[slot * 16 + offset] != value) throw new IOException("O banco retornado não confirmou a alteração. Releia o aparelho.");
            if (expected != null)
            {
                var target = expected.ToArray(); target[slot * 16 + offset] = (byte)value;
                if (!bank.SequenceEqual(target)) throw new InvalidOperationException("Outros parâmetros mudaram no aparelho. Releia o banco.");
            }
            return bank;
        }
        finally { gate.Release(); }
    }

    public async Task SaveBank(byte[] expected, byte[] edited, CancellationToken ct = default)
    {
        var baseline = expected.ToArray();
        var target = edited.ToArray();
        CubeBabyProfile.ValidateBank(baseline);
        CubeBabyProfile.ValidateBank(target);
        for (int slot = 0; slot < 3; slot++)
            for (int offset = 13; offset < 16; offset++)
                if (baseline[slot * 16 + offset] != target[slot * 16 + offset])
                    throw new ArgumentException("Bytes reservados devem ser preservados.", nameof(edited));
        await gate.WaitAsync(ct);
        try
        {
            RequireIdentity();
            var current = await ReadInternal(0, 48, ct);
            if (!current.SequenceEqual(baseline)) throw new InvalidOperationException("O banco mudou no aparelho. Releia antes de salvar.");
            await WriteInternal(0, target, ct);
            var readback = await ReadInternal(0, 48, ct);
            if (!readback.SequenceEqual(target)) throw new IOException("A leitura de retorno não confirmou o salvamento. Releia o aparelho.");
        }
        finally { gate.Release(); }
    }

    public void Dispose()
    {
        faulted = true;
        lock (sync) { pending?.TrySetException(new IOException("Conexão encerrada.")); assembler.Reset(); }
        connection.Received -= Receive;
        connection.Dispose();
    }
}
