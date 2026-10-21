namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private async Task ReadAdvancedPreviewAsync(int stage, Func<bool> stopped, Func<bool> canStart, Func<Task> begin)
    {
        await advancedLifecycle.WaitAsync();
        try
        {
            await WaitForAdvancedEffectOperationAsync();
            if (HasAdvancedPreview(stage) && stopped())
                await ExecuteAdvancedEffect(stage, () => ReconcileAdvancedRecoveryAsync(stage));
            while (Busy) await WaitForAdvancedEffectOperationAsync();
            if (!canStart()) { EnsureAdvancedMonitor(stage); return; }
            await ExecuteAdvancedEffect(stage, async () =>
            {
                await PrepareAdvancedEffectAsync(stage);
                await begin();
            });
            EnsureAdvancedMonitor(stage);
        }
        finally { advancedLifecycle.Release(); }
    }

    private async Task MonitorAdvancedPreviewAsync(int stage, CancellationToken stop, Func<bool> active,
        Func<bool> canCheck, Action<bool> monitoring, Func<Task> check)
    {
        try
        {
            while (!stop.IsCancellationRequested && active())
            {
                await Task.Delay(2000, stop);
                if (Busy || session == null || !canCheck()) continue;
                monitoring(true);
                try { await CheckPreviewInBackground(stage, check); }
                finally { monitoring(false); RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception ex) { FaultAdvancedPreview(ex, stage); }
    }
    private async Task RestoreAdvancedSnapshotAsync(Func<Task> restore, Action restored,
        Action<string> message, string success, string operation, string failure)
    {
        try
        {
            await restore();
            restored();
            message(success);
            RecordOperation(operation);
        }
        catch { message(failure); throw; }
        finally { Refresh(); }
    }
}
