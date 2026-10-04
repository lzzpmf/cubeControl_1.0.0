using System.Buffers.Binary;
using System.IO;
using Cube.Protocol;

namespace Cube.Qa;

// In-memory transport only. No Windows MIDI dependency, physical ports, or background callbacks.
internal sealed class QaConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank { get; } = new byte[48];
    public byte[] Ram { get; } = new byte[0x1000];
    public byte[] Flash { get; } = new byte[0x11000]; // amp 0x60000 through final IR sector
    public List<string> Sent { get; } = [];
    public int Writes { get; private set; }
    public int Erases { get; private set; }
    public bool Silent { get; set; }
    public bool CorruptReadback { get; set; }
    public bool Disposed { get; private set; }
    public Func<CubePacket, CubePacket?>? ReplyFilter { get; set; }
    public void Emit(CubePacket packet) => Received?.Invoke(UsbSysEx.Encode(packet));
    public void Send(byte[] sysex)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        Sent.Add(Convert.ToHexString(sysex));
        if (Silent) return;
        var p = UsbSysEx.Decode(sysex); CubePacket reply;
        if (p.Command == 0x11) reply = new(0x11, "CubeBaby"u8.ToArray());
        else
        {
            byte region = p.Payload[0]; uint address = BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1));
            int offset = region == 0 ? checked((int)(address - 0x60000)) : region == 5 && address >= 0x80000000 ? checked((int)(address - 0x80000000)) : checked((int)address);
            var target = region switch { 0 => Flash, 4 => Ram, 5 => Bank, _ => throw new InvalidOperationException("Unexpected region") };
            if (p.Command == 0x23)
            {
                int length = p.Payload[5] | p.Payload[6] << 8 | p.Payload[7] << 16;
                byte[] data = target.AsSpan(offset, length).ToArray();
                if (CorruptReadback && Erases > 0 && region == 0) data[0] ^= 1;
                reply = new(0x23, p.Payload.Concat(data).ToArray());
            }
            else if (p.Command == 0x21) { target.AsSpan(offset, 4096).Fill(255); Erases++; reply = new(0, [0]); }
            else if (p.Command == 0x22) { p.Payload.AsSpan(8).CopyTo(target.AsSpan(offset)); Writes++; reply = new(0, [0]); }
            else if (p.Command == 0x24) reply = new(0, [0]); // RAM register notification, not another memory write
            else throw new InvalidOperationException($"Unexpected command {p.Command:X2}");
        }
        if (ReplyFilter != null) { var filtered = ReplyFilter(reply); if (filtered == null) return; reply = filtered; }
        var bytes = UsbSysEx.Encode(reply);
        Received?.Invoke(bytes[..3]); Received?.Invoke(bytes[3..]);
    }
    public void Dispose() { Disposed = true; Received = null; }
}
