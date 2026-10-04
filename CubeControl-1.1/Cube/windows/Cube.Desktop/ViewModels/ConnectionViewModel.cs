using Cube.Usb;

namespace Cube.Desktop.ViewModels;

public sealed class ConnectionViewModel : Observable
{
    private MidiPort[] inputs = [], outputs = [];
    private MidiPort? input, output;
    private string identity = "Desconectado";
    public MidiPort[] Inputs { get => inputs; private set => Set(ref inputs, value); }
    public MidiPort[] Outputs { get => outputs; private set => Set(ref outputs, value); }
    public MidiPort? Input { get => input; set => Set(ref input, value); }
    public MidiPort? Output { get => output; set => Set(ref output, value); }
    public string Identity { get => identity; set => Set(ref identity, value); }
    public async Task Refresh()
    {
#if CUBE_SIMULATION
        await Task.CompletedTask;
        Inputs = [new MidiPort("simulation-input", "Cube Baby SIMULADO")];
        Outputs = [new MidiPort("simulation-output", "Cube Baby SIMULADO")];
        Input = Inputs[0]; Output = Outputs[0];
#else
        var foundInputs = await WindowsMidiConnection.Inputs();
        var foundOutputs = await WindowsMidiConnection.Outputs();
        Inputs = foundInputs; Outputs = foundOutputs;
        var previousInput = Inputs.FirstOrDefault(p => p.Id == Input?.Id);
        var previousOutput = Outputs.FirstOrDefault(p => p.Id == Output?.Id);
        var selected = CubeMidiPorts.Select(Inputs, Outputs);
        Input = previousInput != null && previousOutput != null ? previousInput : selected.Input;
        Output = previousInput != null && previousOutput != null ? previousOutput : selected.Output;
#endif
    }
}
