using Cube.Usb;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace Cube.Desktop.Services;

// One inventory for manual refresh and automatic connection. Device notifications only
// invalidate it; enumeration and projection do not run on the WPF dispatcher.
internal sealed class MidiDeviceDiscovery : IDisposable
{
    private readonly object gate = new();
    private DeviceWatcher? inputs, outputs;
    private Task<(MidiPort[] Inputs, MidiPort[] Outputs)>? pending;
    private (MidiPort[] Inputs, MidiPort[] Outputs) inventory = ([], []);
    private long revision;
    private bool dirty = true, disposed;
    internal event Action? Changed;

    internal void Start()
    {
        if (disposed || inputs != null) return;
        try
        {
            inputs = CreateWatcher(MidiInPort.GetDeviceSelector());
            outputs = CreateWatcher(MidiOutPort.GetDeviceSelector());
            inputs.Start(); outputs.Start();
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or UnauthorizedAccessException or InvalidOperationException)
        {
            // Enumeration/manual refresh and the recovery timer remain available if
            // Windows cannot supply device notifications on this machine.
            System.Diagnostics.Debug.WriteLine(ex);
            Stop(inputs); Stop(outputs); inputs = outputs = null;
        }
    }
    private DeviceWatcher CreateWatcher(string selector)
    {
        var watcher = DeviceInformation.CreateWatcher(selector);
        watcher.Added += Added;
        watcher.Removed += Updated;
        watcher.Updated += Updated;
        return watcher;
    }
    private void Added(DeviceWatcher sender, DeviceInformation device) => Invalidate();
    private void Updated(DeviceWatcher sender, DeviceInformationUpdate device) => Invalidate();
    private void Invalidate()
    {
        lock (gate) { if (disposed) return; revision++; dirty = true; }
        Changed?.Invoke();
    }
    internal Task<(MidiPort[] Inputs, MidiPort[] Outputs)> Refresh(bool force)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (pending is { IsCompleted: false }) return pending;
            if (!force && !dirty) return Task.FromResult(inventory);
            return pending = ReadInventory(revision);
        }
    }
    private async Task<(MidiPort[] Inputs, MidiPort[] Outputs)> ReadInventory(long startedRevision)
    {
        var foundInputs = WindowsMidiConnection.Inputs();
        var foundOutputs = WindowsMidiConnection.Outputs();
        await Task.WhenAll(foundInputs, foundOutputs).ConfigureAwait(false);
        var result = (await foundInputs.ConfigureAwait(false), await foundOutputs.ConfigureAwait(false));
        lock (gate) { inventory = result; dirty = revision != startedRevision; }
        return result;
    }
    public void Dispose()
    {
        lock (gate) { disposed = true; Changed = null; }
        Stop(inputs); Stop(outputs); inputs = outputs = null;
    }
    private void Stop(DeviceWatcher? watcher)
    {
        if (watcher == null) return;
        watcher.Added -= Added; watcher.Removed -= Updated; watcher.Updated -= Updated;
        if (watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted) watcher.Stop();
    }
}
