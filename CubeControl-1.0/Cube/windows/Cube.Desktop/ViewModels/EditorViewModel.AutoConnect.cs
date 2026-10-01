using System.Windows.Threading;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private DispatcherTimer? connectionTimer;
    private bool checkingConnection, automaticConnectionPaused, disposed;
    private DateTime nextAutomaticAttempt;
    private bool automaticConnectionAttempt;

    internal void StartAutomaticConnection()
    {
        if (disposed || connectionTimer != null) return;
        connectionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        connectionTimer.Tick += CheckAutomaticConnection;
        connectionTimer.Start();
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
            if (DateTime.UtcNow < nextAutomaticAttempt) return;
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
        connectionTimer?.Stop();
        if (connectionTimer != null) connectionTimer.Tick -= CheckAutomaticConnection;
        connectionTimer = null;
    }
}
