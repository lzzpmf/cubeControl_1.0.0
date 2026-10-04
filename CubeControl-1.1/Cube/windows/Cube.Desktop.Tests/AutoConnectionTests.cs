using Cube.Desktop.ViewModels;
using Cube.Usb;

internal static class AutoConnectionTests
{
    public static async Task Run()
    {
        await FootswitchTests.Run();
        using var model = new EditorViewModel(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        int attempts = 0;
        Task Connect() { attempts++; return Task.CompletedTask; }
        Task Ports(bool input, bool output)
        {
            model.Connection.Input = input ? new MidiPort("in", "USB-Midi") : null;
            model.Connection.Output = output ? new MidiPort("out", "USB-Midi") : null;
            return Task.CompletedTask;
        }
        void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); }
        await model.TryAutomaticConnection(() => Ports(false, false), Connect);
        await model.TryAutomaticConnection(() => Ports(true, false), Connect);
        Check(attempts == 0, "waits for both MIDI ports");
        await model.TryAutomaticConnection(() => Ports(true, true), () => { attempts++; throw new System.IO.IOException("booting"); });
        await model.TryAutomaticConnection(() => Ports(true, true), Connect);
        Check(attempts == 2 && !model.Busy, "retries when pedal finishes booting");
        var enumeration = new TaskCompletionSource();
        var pending = model.TryAutomaticConnection(() => enumeration.Task, Connect);
        await model.TryAutomaticConnection(() => Ports(true, true), Connect);
        Check(attempts == 2, "no overlapping detection attempts");
        enumeration.SetResult(); await pending;
        Check(attempts == 3, "connects after delayed enumeration");
        await model.Execute(async () => await model.TryAutomaticConnection(() => Ports(true, true), Connect));
        Check(attempts == 3, "does not interrupt an operation");
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(EditorViewModel).GetField("baseline", flags)!.SetValue(model, new byte[] { 0 });
        typeof(EditorViewModel).GetField("edited", flags)!.SetValue(model, new byte[] { 1 });
        await model.TryAutomaticConnection(() => Ports(true, true), Connect);
        Check(attempts == 4 && model.Dirty, "unsaved edits no longer prevent automatic detection");
        typeof(EditorViewModel).GetField("edited", flags)!.SetValue(model, new byte[] { 0 });
        await model.DisconnectAsync();
        await model.TryAutomaticConnection(() => Ports(true, true), Connect);
        Check(attempts == 4, "respects manual disconnect");
        using var closing = new EditorViewModel();
        var wait = new TaskCompletionSource();
        closing.Connection.Input = new MidiPort("in", "USB-Midi");
        closing.Connection.Output = new MidiPort("out", "USB-Midi");
        var scan = closing.TryAutomaticConnection(() => wait.Task, Connect);
        closing.Dispose(); wait.SetResult(); await scan;
        Check(attempts == 4, "shutdown cancels pending detection");
        using var device = new FakeConnection();
        using var connected = new EditorViewModel(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var liveSession = new Cube.Protocol.CubeSession(device) { Timeout = TimeSpan.FromMilliseconds(100) };
        await liveSession.Identify();
        typeof(EditorViewModel).GetField("session", flags)!.SetValue(connected, liveSession);
        var originalBank = new byte[48];
        var localEdits = new byte[48]; localEdits[1] = 1;
        typeof(EditorViewModel).GetField("baseline", flags)!.SetValue(connected, originalBank);
        typeof(EditorViewModel).GetField("edited", flags)!.SetValue(connected, localEdits);
        await connected.TryAutomaticConnection();
        Check(connected.Connected && device.Writes == 0, "connected monitoring is read-only and preserves edits");
        device.Silent = true;
        await connected.TryAutomaticConnection();
        Check(!connected.Connected && connected.CanConnect && !connected.Busy && connected.HasError,
            "power-off clears session and releases connect action");
        Check(connected.Dirty && connected.ErrorMessage.Contains("desconectado ou desligado"),
            "disconnect is visible and unsaved edits survive");
        Check(connected.Connect.CanExecute(null), "manual reconnect remains available with missing ports and unsaved edits");
        Check(connected.Connection.Input == null && connected.Connection.Output == null && device.Writes == 0,
            "stale ports are cleared without writing to pedal");
        device.Silent = false;
        device.Bank = localEdits.ToArray();
        connected.Ask = _ => throw new Exception("Automatic reconnection must not prompt");
        Task RefreshPorts()
        {
            connected.Connection.Input = new MidiPort("new-in", "USB-Midi");
            connected.Connection.Output = new MidiPort("new-out", "USB-Midi");
            return Task.CompletedTask;
        }
        async Task Reconnect()
        {
            var replacement = new Cube.Protocol.CubeSession(device);
            await replacement.Identify();
            await connected.AttachSession(replacement);
        }
        await connected.TryAutomaticConnection(RefreshPorts, Reconnect);
        Check(connected.Connected && connected.CanEdit && !connected.Busy && !connected.HasError,
            "real reconnect reloads inventory and releases editing without stale error");
        Check(connected.Dirty && connected.Save.CanExecute(null), "unsaved RAM edits remain available to save after USB reconnect");
        Check(!System.IO.Directory.Exists(System.IO.Path.Combine(connected.DataDirectory, "Backups")),
            "automatic reconnection creates no disk backups");
        Check(device.Writes == 0, "automatic reconnection only reads pedal");
        connected.Current.Parameters[1].Value = 2;
        await connected.Flush();
        Check(device.Bank[1] == 2 && device.Writes == 1, "fresh queue sends user edits after reconnection");
        device.Silent = true;
        await connected.TryAutomaticConnection();
        device.Silent = false; device.Bank = originalBank.ToArray();
        int beforeReconnect = device.Writes;
        await connected.TryAutomaticConnection(RefreshPorts, Reconnect);
        Check(connected.CanEdit && !connected.HasError && !connected.Dirty && connected.Current.Parameters[1].Value == 0,
            "power cycle loads actual pedal bank and releases controls");
        Check(device.Writes == beforeReconnect && !System.IO.Directory.Exists(System.IO.Path.Combine(connected.DataDirectory, "Backups")),
            "power cycle preserves draft without replaying edits to pedal");
        var stalled = new TaskCompletionSource<IDisposable>();
        var openMethod = typeof(WindowsMidiConnection).GetMethod("OpenWithTimeout", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.MakeGenericMethod(typeof(IDisposable));
        var opening = (Task<IDisposable>)openMethod.Invoke(null, new object[] { stalled.Task })!;
        bool timedOut = false;
        try { await opening; } catch (System.IO.IOException) { timedOut = true; }
        Check(timedOut, "unresponsive MIDI driver has a bounded open timeout");
        var late = new LatePort(); stalled.SetResult(late);
        await late.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Check(true, "port arriving after timeout is disposed");
    }

    private sealed class LatePort : IDisposable
    {
        public TaskCompletionSource Disposed { get; } = new();
        public void Dispose() => Disposed.TrySetResult();
    }
}
