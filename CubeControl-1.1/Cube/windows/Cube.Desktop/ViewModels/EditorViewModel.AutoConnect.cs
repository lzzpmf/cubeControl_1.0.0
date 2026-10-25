using System.Windows.Threading;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private DispatcherTimer? connectionTimer;
    private bool checkingConnection, automaticConnectionPaused, disposed;
    private DateTime nextAutomaticAttempt;
    private bool automaticConnectionAttempt;
    private DateTime nextPortRefresh;
    private DispatcherTimer? portsChangedTimer;

    internal void StartAutomaticConnection()
    {
        if (disposed || connectionTimer != null) return;
        connectionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        connectionTimer.Tick += CheckAutomaticConnection;
        portsChangedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        portsChangedTimer.Tick += PortsChanged_Tick;
        Connection.PortsChanged += PortsChanged;
        Connection.StartWatching();
        connectionTimer.Start();
    }
    private void PortsChanged()
    {
        var timer = portsChangedTimer;
        if (timer == null) return;
        timer.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (disposed || portsChangedTimer == null) return;
            nextPortRefresh = nextAutomaticAttempt = DateTime.MinValue;
            portsChangedTimer.Stop(); portsChangedTimer.Start();
        }));
    }
    private async void PortsChanged_Tick(object? sender, EventArgs e)
    {
        portsChangedTimer?.Stop();
        await TryAutomaticConnection();
    }

    private async void CheckAutomaticConnection(object? sender, EventArgs e) =>
        await TryAutomaticConnection();

    internal async Task TryAutomaticConnection(Func<Task>? refreshPorts = null, Func<Task>? connect = null)
    {
        bool Eligible() => !disposed && !automaticConnectionPaused && CanConnect;
        if (checkingConnection || disposed || Busy || (!Connected && !Eligible())) return;
        checkingConnection = true;
        try
        {
            if (session is { } current)
            {
                // A read-only identity request also catches power-off when Windows retains the ports.
                try
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2.5));
                    await current.Identify(timeout.Token);
                }
                catch (Exception ex)
                {
                    if (!disposed && ReferenceEquals(session, current))
                    {
                        Connection.Input = null; Connection.Output = null;
                        Connection.Identity = "Desconectado";
                        Fault(new System.IO.IOException("Pedal desconectado ou desligado. Verifique o cabo USB e a alimentação.", ex));
                    }
                }
                return;
            }
            if (DateTime.UtcNow < nextAutomaticAttempt || refreshPorts == null && DateTime.UtcNow < nextPortRefresh) return;
            nextPortRefresh = DateTime.UtcNow.AddSeconds(10);
            await (refreshPorts ?? Connection.Refresh)();
            // Enumeration yields to the UI: a manual operation or shutdown may have started.
            if (Eligible() && Connection.Input != null && Connection.Output != null)
            {
                automaticConnectionAttempt = true;
                try { await Execute(connect ?? ConnectAsync); }
                finally { automaticConnectionAttempt = false; }
                if (!Connected && connect == null) nextAutomaticAttempt = DateTime.UtcNow.AddSeconds(7.5);
            }
        }
        catch (Exception ex)
        {
            // USB enumeration can fail transiently while Windows initializes the device.
            if (!disposed) AddLog("USB-MIDI: " + ex.Message);
        }
        finally { checkingConnection = false; }
    }

    private void StopAutomaticConnection()
    {
        disposed = true;
        Connection.PortsChanged -= PortsChanged;
        Connection.StopWatching();
        portsChangedTimer?.Stop();
        if (portsChangedTimer != null) portsChangedTimer.Tick -= PortsChanged_Tick;
        portsChangedTimer = null;
        connectionTimer?.Stop();
        if (connectionTimer != null) connectionTimer.Tick -= CheckAutomaticConnection;
        connectionTimer = null;
    }
}
