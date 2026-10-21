#if WINUI
using ControlAppearance = Cube.WinUI.Platform.ControlAppearance;
#else
using ControlAppearance = Wpf.Ui.Controls.ControlAppearance;
#endif
using Cube.Protocol;
#if WINUI
using Cube.WinUI.Platform;
#else
using System.Windows.Threading;
#endif

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private DispatcherTimer? footswitchTimer;
    private bool checkingFootswitch;
    private CubeSession? observedFootswitchSession;
    private FootswitchState? lastFootswitchState;

    private void StartFootswitchMonitoring()
    {
        footswitchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        footswitchTimer.Tick += CheckFootswitch;
        footswitchTimer.Start();
    }

    private async void CheckFootswitch(object? sender, EventArgs e) => await TryReadFootswitch();

    internal async Task TryReadFootswitch()
    {
        bool Eligible() => !disposed && CanEdit && !HasAdvancedPreviews && gesture == null &&
            edited != null && confirmed != null && edited.SequenceEqual(confirmed);
        if (checkingFootswitch || !Eligible() || session is not { } current) return;
        checkingFootswitch = true;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2.5));
            var state = await current.ReadFootswitch(timeout.Token);
            if (!ReferenceEquals(session, current) || !Eligible()) return;
            if (!ReferenceEquals(observedFootswitchSession, current))
            {
                observedFootswitchSession = current;
                lastFootswitchState = null;
                if (state == null)
                    NotifyUser("Footswitches", "A troca automática de presets não está disponível neste firmware.",
                        ControlAppearance.Caution);
            }
            // Only a physical state transition changes the editor. Manual navigation stays usable.
            if (state != lastFootswitchState && state?.PresetIndex is int slot)
                SelectedIndex = slot;
            lastFootswitchState = state;
        }
        catch (Exception ex)
        {
            if (!disposed && ReferenceEquals(session, current) && current.IsFaulted) Fault(ex);
        }
        finally { checkingFootswitch = false; }
    }

    private void StopFootswitchMonitoring()
    {
        footswitchTimer?.Stop();
        if (footswitchTimer != null) footswitchTimer.Tick -= CheckFootswitch;
        footswitchTimer = null;
        observedFootswitchSession = null;
        lastFootswitchState = null;
    }
}
