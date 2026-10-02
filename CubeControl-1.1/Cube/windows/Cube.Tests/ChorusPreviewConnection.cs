using System.Buffers.Binary;
using System.IO;
using System.Text.Json;
using Cube.Protocol;

internal sealed class ChorusPreviewConnection : IMidiConnection
{
    public event Action<byte[]>? Received;
    public byte[] Bank = new byte[48];
    public byte[] Dsp = new byte[0xF70];
    public byte[] State = [0, 1];
    public int Writes;
    public List<uint> Addresses = new();
    public int FailAfterWrites = int.MaxValue;
    public int ChangePhysicalAfterWrites = int.MaxValue;
    public bool Silent = false, IgnoreWrites = false, BadSignature = false, BadReadHeader = false;
    private readonly Dictionary<uint, byte[]> code;
    public ChorusPreviewConnection()
    {
        code = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "chorus-live-code.json")))!
            .ToDictionary(p => uint.Parse(p.Key), p => Convert.FromHexString(p.Value));
        code.TryAdd(0xFFFFD958, new byte[12]);
        Bank[8] = 3; Bank[11] = 1;
        Dsp[0x63C] = 3; Dsp[0x63D] = 5; Dsp[0x640] = 0;
        SetParameters(ChorusParameters.Default);
    }
    public void SetParameters(ChorusParameters p)
    {
        Dsp[0x640] = (byte)p.Waveform;
        foreach (var (offset, value) in new[] { (0x644, p.Depth), (0x648, p.Step), (0x64C, p.Feedback), (0x650, p.WetGain), (0x66C, p.CombinedGain), (0x688, p.Mix) })
            BinaryPrimitives.WriteSingleLittleEndian(Dsp.AsSpan(offset), value);
    }
    public void Send(byte[] sysex)
    {
        if (Silent) return;
        var p = UsbSysEx.Decode(sysex);
        CubePacket reply;
        if (p.Command == 0x11) reply = new(0x11, Convert.FromHexString("53494E434F2D437562654261627920202020200113141516171819"));
        else
        {
            uint address = BinaryPrimitives.ReadUInt32LittleEndian(p.Payload.AsSpan(1));
            if (p.Command == 0x23)
            {
                int length = p.Payload[5] | p.Payload[6] << 8 | p.Payload[7] << 16;
                byte[] data;
                if (p.Payload[0] == 0) data = new byte[length];
                else if (p.Payload[0] == 5 || address == 0xFFFD827C) data = Bank.AsSpan(0, length).ToArray();
                else if (address == 0xFFFD8202) data = State.ToArray();
                else if (address < Dsp.Length) data = Dsp.AsSpan((int)address, length).ToArray();
                else
                {
                    var range = code.First(x => address >= x.Key && (ulong)address + (uint)length <= (ulong)x.Key + (uint)x.Value.Length);
                    data = range.Value.AsSpan((int)(address - range.Key), length).ToArray();
                    if (BadSignature) data[0] ^= 1;
                }
                var payload = p.Payload.Concat(data).ToArray();
                if (BadReadHeader) payload[1] ^= 1;
                reply = new(0x23, payload);
            }
            else
            {
                if (p.Command != 0x22 || p.Payload[0] != 4 || !ChorusParameters.IsControlOffset((int)address))
                    throw new Exception("Unexpected mutation in chorus preview test");
                Writes++; Addresses.Add(address);
                if (Writes >= ChangePhysicalAfterWrites) State[0] = 1;
                if (!IgnoreWrites) p.Payload.AsSpan(8).CopyTo(Dsp.AsSpan((int)address));
                if (Writes >= FailAfterWrites) { Silent = true; return; }
                reply = new(0, [0]);
            }
        }
        Received?.Invoke(UsbSysEx.Encode(reply));
    }
    public void Dispose() { }
}
