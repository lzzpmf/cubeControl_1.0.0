using System.IO;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private readonly SemaphoreSlim advancedLifecycle = new(1);
    private readonly string[] advancedErrors = ["", "", "", "", ""];
    public IReadOnlyList<string> AdvancedErrors => advancedErrors;
    private void SetAdvancedError(int stage, string message)
    {
        advancedErrors[stage] = message; Notify(nameof(AdvancedErrors)); Notify(nameof(EffectCardError));
    }
    private void EnsureAdvancedMonitor(int stage)
    {
        switch (stage)
        {
            case 1 when ChorusPreviewActive && !chorusStopped && (chorusMonitorTask == null || chorusMonitorTask.IsCompleted):
                chorusMonitorStop?.Dispose(); chorusMonitorStop = new(); chorusMonitorTask = MonitorChorus(chorusMonitorStop.Token); break;
            case 2 when PhaserPreviewActive && !phaserStopped && (phaserMonitorTask == null || phaserMonitorTask.IsCompleted):
                phaserMonitorStop?.Dispose(); phaserMonitorStop = new(); phaserMonitorTask = MonitorPhaser(phaserMonitorStop.Token); break;
            case 3 when DelayPreviewActive && !delayStopped && (delayMonitorTask == null || delayMonitorTask.IsCompleted):
                delayMonitorStop?.Dispose(); delayMonitorStop = new(); delayMonitorTask = MonitorDelay(delayMonitorStop.Token); break;
            case 4 when ReverbPreviewActive && !reverbStopped && (reverbMonitorTask == null || reverbMonitorTask.IsCompleted):
                reverbMonitorStop?.Dispose(); reverbMonitorStop = new(); reverbMonitorTask = MonitorReverb(reverbMonitorStop.Token); break;
        }
    }

    // Only a verified change of context retires a recovery. Unknown/partial values
    // in the same context must still go through the protocol's guarded restoration.
    private async Task<bool> RetireObsoleteAdvancedRecoveryAsync(int stage)
    {
        if (session == null || !HasAdvancedPreview(stage)) return false;
        var context = stage switch
        {
            1 => (chorusRecovery!.Identity, chorusRecovery.Bank, chorusRecovery.PhysicalState, chorusRecovery.OriginalDsp),
            2 => (phaserRecovery!.Identity, phaserRecovery.Bank, phaserRecovery.PhysicalState, phaserRecovery.OriginalDsp),
            3 => (delayRecovery!.Identity, delayRecovery.Bank, delayRecovery.PhysicalState, delayRecovery.OriginalDsp),
            4 => (reverbRecovery!.Identity, reverbRecovery.Bank, reverbRecovery.PhysicalState, reverbRecovery.OriginalDsp),
            _ => throw new ArgumentOutOfRangeException(nameof(stage))
        };
        if (context.Identity != session.IdentityHex)
            throw new IOException("Conecte o mesmo pedal da prévia pendente. O snapshot foi preservado nesta sessão.");
        var current = await session.ReadEffectPreviewState(stage);
        if (CubeSession.PreviewContextMatches(stage, current, context.Bank, context.PhysicalState, context.OriginalDsp)) return false;
        const string message = "O contexto do pedal mudou. A prévia anterior foi encerrada sem escrever no pedal; leia o efeito atual para continuar.";
        switch (stage)
        {
            case 1: chorusMonitorStop?.Cancel(); chorusRecovery = null; chorusStopped = false; chorusPending = false; Chorus.Message = message; break;
            case 2: phaserMonitorStop?.Cancel(); phaserRecovery = null; phaserStopped = false; phaserPending = false; Phaser.Message = message; break;
            case 3: delayMonitorStop?.Cancel(); delayRecovery = null; delayStopped = false; delayPending = false; Delay.Message = message; break;
            case 4: reverbMonitorStop?.Cancel(); reverbRecovery = null; reverbStopped = false; reverbPending = false; Reverb.Message = message; break;
        }
        RecordOperation(message); ErrorMessage = ""; SetAdvancedError(stage, ""); Refresh();
        return true;
    }

    private async Task ReconcileAdvancedRecoveryAsync(int stage)
    {
        if (await RetireObsoleteAdvancedRecoveryAsync(stage)) return;
        try
        {
        switch (stage)
        {
            case 1: await session!.CheckChorusPreview(chorusRecovery!); chorusStopped = false; Chorus.Message = "Prévia retomada e conferida na RAM."; break;
            case 2: await session!.CheckPhaserPreview(phaserRecovery!); phaserStopped = false; Phaser.Message = "Prévia retomada e conferida na RAM."; break;
            case 3: await session!.CheckDelayPreview(delayRecovery!); delayStopped = false; Delay.Message = "Prévia retomada e conferida na RAM."; break;
            case 4: await session!.CheckReverbPreview(reverbRecovery!); reverbStopped = false; Reverb.Message = "Prévia retomada e conferida na RAM."; break;
        }
        }
        catch (IOException) when (session?.IsFaulted == false)
        {
            // A power cycle may have reloaded the original values, or a write may
            // have been interrupted. The existing restoration validates every
            // field against original/previous/target before allowing any write.
            switch (stage)
            {
                case 1: await EndChorusPreviewAsync(); break;
                case 2: await EndPhaserPreviewAsync(); break;
                case 3: await EndDelayPreviewAsync(); break;
                case 4: await EndReverbPreviewAsync(); break;
            }
        }
        ErrorMessage = ""; SetAdvancedError(stage, ""); Refresh();
    }
}
