using Cube.Protocol;
using System.Buffers.Binary;

sealed class FakeConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public bool Silent { get; set; }
    public byte[] Bank = new byte[48];
    public byte[] Flash = new byte[8 * 4096];
    public byte[] Ram = new byte[4096];
    public int Erases, FlashWrites, Writes, MaxRead, MaxWrite;
    public bool CorruptFlashReadback { get; set; }
    public bool DisconnectOnFlashWrite { get; set; }
    public Action<CubePacket>? Observe { get; set; }
    private int active;
    public int MaximumActive;
    public void Send(byte[] sysex)
    {
        if (Silent) return;
        var packet = UsbSysEx.Decode(sysex);
        Observe?.Invoke(packet);
        MaximumActive = Math.Max(MaximumActive, Interlocked.Increment(ref active));
        CubePacket response;
        if (packet.Command == 0x11) response = new(0x11, "CubeBaby"u8.ToArray());
        else if (packet.Command == 0x23)
        {
            byte region = packet.Payload[0];
            uint address = BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1));
            int length = packet.Payload[5] | packet.Payload[6] << 8 | packet.Payload[7] << 16;
            MaxRead = Math.Max(MaxRead, length);
            var source = region == 5 ? Bank : region == 4 ? Ram : Flash;
            int offset = region == 0 ? (int)(address - 0x69000) : (int)address;
            byte[] data = source.AsSpan(offset, length).ToArray();
            if (region == 0 && CorruptFlashReadback && Erases > 0) data[0] ^= 1;
            var payload = packet.Payload.Concat(data).ToArray();
            response = new(0x23, payload);
        }
        else
        {
            uint address = BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1));
            if (packet.Command == 0x21) { Flash.AsSpan((int)(address - 0x69000), 4096).Fill(255); Erases++; }
            else if (packet.Command == 0x22)
            {
                Writes++; MaxWrite = Math.Max(MaxWrite, packet.Payload.Length - 8);
                byte region = packet.Payload[0];
                if (region == 0)
                {
                    FlashWrites++;
                    if (DisconnectOnFlashWrite) { Silent = true; Interlocked.Decrement(ref active); return; }
                    packet.Payload.AsSpan(8).CopyTo(Flash.AsSpan((int)(address - 0x69000)));
                }
                else if (region == 4) packet.Payload.AsSpan(8).CopyTo(Ram.AsSpan((int)address));
                else if (address == 0) Bank = packet.Payload[8..];
                else Bank[address - 0x80000000] = packet.Payload[8];
            }
            response = new(0, [0]);
        }
        var bytes = UsbSysEx.Encode(response);
        _ = Task.Run(async () =>
        {
            await Task.Delay(10);
            Interlocked.Decrement(ref active);
            Received?.Invoke(bytes[..3]); Received?.Invoke(bytes[3..]);
        });
    }
    public void Dispose() { }
}
