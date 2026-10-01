using System.Buffers.Binary;
using System.Reflection;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class FootswitchTests
{
    public static async Task Run()
    {
        void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); }
        using var wire = new FootswitchConnection();
        using var session = new CubeSession(wire) { Timeout = TimeSpan.FromMilliseconds(100) };
        await session.Identify();
        using var model = new EditorViewModel(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        void Field(string name, object value) => typeof(EditorViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(model, value);
        var edits = new byte[48];
        Field("session", session); Field("baseline", new byte[48]); Field("edited", edits); Field("confirmed", new byte[48]);
        await model.TryReadFootswitch();
        wire.Slot = 1; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 1, "physical B selects its chain even with three identical presets");
        model.SelectedIndex = 0; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 0, "manual navigation is not overwritten by unchanged physical state");
        wire.Slot = 2; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 2, "physical C selects C");
        wire.Slot = 255; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 2, "bypass is not misinterpreted as a preset");
        wire.Mode = 0; wire.Slot = 0; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 2, "LIVE mode does not select a preset");
        wire.Mode = 1; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 0, "return to PRESET follows physical selection");
        var gesture = new object(); model.BeginGesture(gesture); wire.Slot = 1;
        await model.TryReadFootswitch(); Check(model.SelectedIndex == 0, "gesture is not interrupted");
        model.EndGesture(gesture); await model.TryReadFootswitch();
        Check(model.SelectedIndex == 1, "deferred switch is applied after gesture ends");
        edits[0] = 1; wire.Slot = 2; await model.TryReadFootswitch();
        Check(model.SelectedIndex == 1, "pending edits defer synchronization");
        Field("confirmed", edits.ToArray()); await model.TryReadFootswitch();
        Check(model.SelectedIndex == 2 && model.Dirty && edits[0] == 1, "confirmed unsaved edits survive physical switching");
        Check(wire.Inner.Writes == 0 && wire.Inner.Erases == 0, "monitor sends no writes or erase commands");
        using var incompatible = new FootswitchConnection { Compatible = false };
        using var unsupported = new CubeSession(incompatible); await unsupported.Identify();
        Check(await unsupported.ReadFootswitch() == null && await unsupported.ReadFootswitch() == null && incompatible.ResearchReads == 1,
            "unknown firmware stops after signature check without retrying or reading state");
        using var mismatched = new FootswitchConnection { BankMatches = false };
        using var rejected = new CubeSession(mismatched); await rejected.Identify();
        Check(await rejected.ReadFootswitch() == null && mismatched.StateReads == 0, "bank alias mismatch prevents state interpretation");
        wire.Silent = true; await model.TryReadFootswitch();
        Check(!model.Connected && model.Dirty && model.CanConnect, "timeout releases connection and preserves edits");
    }

    private sealed class FootswitchConnection : IMidiConnection
    {
        public readonly FakeConnection Inner = new();
        public event Action<byte[]>? Received;
        public byte Slot, Mode = 1;
        public bool Compatible = true, BankMatches = true, Silent;
        public int ResearchReads, StateReads;
        public FootswitchConnection() => Inner.Received += data => Received?.Invoke(data);
        public void Send(byte[] bytes)
        {
            if (Silent) return;
            var packet = UsbSysEx.Decode(bytes);
            if (packet.Command == 0x23 && packet.Payload[0] == 4 && BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1)) >= 0xFFFD0000)
            {
                ResearchReads++;
                uint address = BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1));
                byte[] result = address switch
                {
                    0xFFFFDDEC => Compatible ? Convert.FromHexString("014B194436F4B6B3B8CB8000") : new byte[12],
                    0xFFFD8558 => BankMatches ? Inner.Bank.ToArray() : Enumerable.Repeat((byte)1, 48).ToArray(),
                    0xFFFD84B2 => [Slot, Mode],
                    _ => throw new Exception("Unexpected research address")
                };
                if (address == 0xFFFD84B2) StateReads++;
                Received?.Invoke(UsbSysEx.Encode(new(0x23, packet.Payload.Concat(result).ToArray())));
            }
            else Inner.Send(bytes);
        }
        public void Dispose() => Inner.Dispose();
    }
}
