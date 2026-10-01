using Cube.Protocol;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.Storage.Streams;

namespace Cube.Usb;

public sealed record MidiPort(string Id, string Name)
{
    public override string ToString() => Name;
}

public sealed class WindowsMidiConnection : IMidiConnection
{
    private readonly MidiInPort input;
    private readonly IMidiOutPort output;
    public event Action<byte[]>? Received;
    private WindowsMidiConnection(MidiInPort input, IMidiOutPort output)
    {
        this.input = input;
        this.output = output;
        input.MessageReceived += OnMessage;
    }

    public static async Task<MidiPort[]> Inputs() =>
        (await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector()).AsTask().WaitAsync(TimeSpan.FromSeconds(2.5))).Select(d => new MidiPort(d.Id, d.Name)).ToArray();
    public static async Task<MidiPort[]> Outputs() =>
        (await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector()).AsTask().WaitAsync(TimeSpan.FromSeconds(2.5))).Select(d => new MidiPort(d.Id, d.Name)).ToArray();

    public static async Task<WindowsMidiConnection> Open(string inputId, string outputId)
    {
        var input = await OpenWithTimeout(MidiInPort.FromIdAsync(inputId).AsTask()) ?? throw new IOException("Não foi possível abrir a entrada MIDI.");
        try
        {
            var output = await OpenWithTimeout(MidiOutPort.FromIdAsync(outputId).AsTask()) ?? throw new IOException("Não foi possível abrir a saída MIDI.");
            return new(input, output);
        }
        catch { input.Dispose(); throw; }
    }

    private static async Task<T> OpenWithTimeout<T>(Task<T> opening) where T : IDisposable
    {
        try { return await opening.WaitAsync(TimeSpan.FromSeconds(2.5)); }
        catch (TimeoutException)
        {
            _ = DisposeLatePort(opening);
            throw new IOException("O pedal não respondeu ao abrir a porta MIDI. Verifique a conexão USB e tente novamente.");
        }
    }

    private static async Task DisposeLatePort<T>(Task<T> opening) where T : IDisposable
    {
        try { (await opening)?.Dispose(); }
        catch { /* Observe late driver errors after the UI has already reported the timeout. */ }
    }

    private void OnMessage(MidiInPort sender, MidiMessageReceivedEventArgs args)
    {
        using var reader = DataReader.FromBuffer(args.Message.RawData);
        var bytes = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(bytes);
        Received?.Invoke(bytes);
    }

    public void Send(byte[] sysex)
    {
        using var writer = new DataWriter();
        writer.WriteBytes(sysex);
        output.SendBuffer(writer.DetachBuffer());
    }

    public void Dispose() { input.MessageReceived -= OnMessage; input.Dispose(); output.Dispose(); }
}
