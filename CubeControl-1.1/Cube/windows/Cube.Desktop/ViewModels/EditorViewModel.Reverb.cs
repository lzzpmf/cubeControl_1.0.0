using System.IO;
using System.Text.Json;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public ReverbViewModel Reverb { get; } = new();
    private ReverbRecovery? reverbRecovery;
    private Task? reverbSendTask;
    private object? reverbHistoryGroup;
    private Task? reverbMonitorTask;
    private CancellationTokenSource? reverbMonitorStop;
    private bool reverbPending, reverbStopped;
    private bool reverbMonitoring;
    private bool ReverbBackgroundOperation => reverbSendTask != null || reverbMonitoring;
    public bool ReverbPreviewActive => reverbRecovery != null;
    public string ReverbGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : reverbStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
        : ReverbPreviewActive && !IsAdvancedEffectAudible(4) ? "Efeito desligado. A janela permanece aberta; ligue o efeito para retomar a prévia."
        : ReverbPreviewActive ? "Edição automática em RAM · não salva no pedal."
        : !Current.Musical.ReverbEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartReverb ? "Recurso indisponível durante a operação ou edição em RAM atual."
        : "Abra as opções avançadas para ler o efeito ativo e iniciar a edição em RAM.";
    public bool CanStartReverb => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !ReverbPreviewActive
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool CanRestoreReverb => Connected && !Busy && ReverbPreviewActive;
    public bool CanEditReverb => Reverb.Loaded && (!Busy || reverbSendTask != null) && !reverbStopped;
    public bool CanUseReverbFiles => !Busy;
    public bool CanApplyReverb => !Busy && ReverbPreviewActive && !reverbStopped;

    private Task KeepReverbRecovery(ReverbRecovery recovery)
    {
        recovery.Validate();
        // Own the snapshot for this process only; never recover a previous app session.
        reverbRecovery = JsonSerializer.Deserialize<ReverbRecovery>(JsonSerializer.Serialize(recovery))!;
        reverbRecovery.Validate();
        Refresh();
        return Task.CompletedTask;
    }

    public async Task ReadReverbAsync()
    {
        await advancedLifecycle.WaitAsync();
        try
        {
            await WaitForAdvancedEffectOperationAsync();
            if (HasAdvancedPreview(4) && reverbStopped)
                await ExecuteAdvancedEffect(4, () => ReconcileAdvancedRecoveryAsync(4));
            while (Busy) await WaitForAdvancedEffectOperationAsync();
            if (!CanStartReverb) { EnsureAdvancedMonitor(4); return; }
            await ExecuteAdvancedEffect(4, async () =>
            {
                await PrepareAdvancedEffectAsync(4);
                await session!.BeginReverbPreview(SelectedIndex, KeepReverbRecovery);
                Reverb.Load(session.IdentityHex, reverbRecovery!.Target);
                reverbStopped = false;
                RecordOperation("Prévia de reverb iniciada na RAM.");
                Reverb.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
            });
            EnsureAdvancedMonitor(4);
        }
        finally { advancedLifecycle.Release(); }
    }

    private async Task MonitorReverb(CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && ReverbPreviewActive && !reverbStopped)
            {
                await Task.Delay(2000, stop);
                if (Busy || reverbSendTask != null || session == null || reverbRecovery == null || !IsAdvancedEffectAudible(4)) continue;
                reverbMonitoring = true;
                try { await CheckPreviewInBackground(4, () => session.CheckReverbPreview(reverbRecovery)); }
                finally { reverbMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception ex) { FaultAdvancedPreview(ex, 4); }
    }

    private void ReverbEdited()
    {
        if (retiringAdvanced[4]) return;
        if (!ReverbPreviewActive || reverbStopped || !IsAdvancedEffectAudible(4))
        {
            if (!reverbStopped) Reverb.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Reverb.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
        reverbHistoryGroup = OperationGroup("Reverb");
        reverbPending = true;
        reverbSendTask ??= SendReverbEdits();
    }

    private async Task SendReverbEdits()
    {
        await Task.Yield();
        try
        {
            await ExecuteAdvancedEffect(4, async () =>
            {
                while (reverbPending && !reverbStopped && IsAdvancedEffectAudible(4))
                {
                    reverbPending = false;
                    try
                    {
                        if (session == null || reverbRecovery == null) throw new IOException("Reconecte para restaurar o reverb.");
                        var parameters = Reverb.Parameters;
                        if (parameters == reverbRecovery.Target)
                        {
                            Reverb.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                            continue;
                        }
                        var context = reverbRecovery.OriginalDsp;
                        object? group = reverbHistoryGroup;
                        await session.ApplyReverbPreview(reverbRecovery, parameters, KeepReverbRecovery);
                        RecordOperation("Prévia de reverb ajustada na RAM.", async () =>
                        {
                            Reverb.Load(Reverb.Identity, parameters); ReverbEdited();
                            if (!Busy && reverbSendTask != null) await reverbSendTask;
                            return !reverbStopped && reverbRecovery?.Target == parameters;
                        }, () => Connected && !Busy && !reverbStopped && ReferenceEquals(reverbRecovery?.OriginalDsp, context), group);
                        Reverb.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        reverbStopped = true; reverbPending = false;
                        Reverb.Message = "Edição em RAM interrompida. Restauração não confirmada; snapshot preservado nesta sessão. Use Restaurar reverb.";
                        throw;
                    }
                }
            });
        }
        finally { reverbSendTask = null; Refresh(); }
    }

    public async Task EndReverbPreviewAsync()
    {
        retiringAdvanced[4] = true;
        reverbPending = false;
        try
        {
            reverbMonitorStop?.Cancel();
            if (reverbMonitorTask != null) await reverbMonitorTask;
            reverbMonitorTask = null;
            reverbMonitorStop?.Dispose(); reverbMonitorStop = null;
            reverbPending = false;
            if (!Busy && reverbSendTask != null) await reverbSendTask;
            if (reverbRecovery == null) return;
            if (session == null) throw new IOException("Reconecte o pedal para restaurar o reverb. Snapshot preservado em memória nesta sessão.");
            if (await RetireObsoleteAdvancedRecoveryAsync(4)) return;
            if (!reverbStopped && !IsAdvancedEffectAudible(4))
            {
                reverbRecovery = null;
                Reverb.Message = "Prévia encerrada. O efeito permanece desligado; edição local preservada.";
                Refresh();
                return;
            }
            reverbStopped = true;
            try
            {
                await session.RestoreReverbPreview(reverbRecovery);
                Reverb.Load(reverbRecovery.Identity, ReverbParameters.Read(reverbRecovery.OriginalDsp));
                reverbRecovery = null; reverbStopped = false;
                Reverb.Message = "Reverb original restaurado e conferido. Edição local disponível.";
                RecordOperation("Prévia de reverb encerrada; parâmetros anteriores restaurados.");
            }
            catch { Reverb.Message = "Restauração não confirmada. O snapshot do reverb continua em memória nesta sessão."; throw; }
            finally { Refresh(); }
        }
        finally { retiringAdvanced[4] = false; }
    }

    public async Task RestoreReverbAsync()
    {
        if (!CanRestoreReverb) return;
        await CloseAdvancedPreviewAsync(4);
    }

    public async Task ImportReverbAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Reverb (*.reverb.json)") + "|*.reverb.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await ExecuteAdvancedEffect(4, async () => { await LoadReverbFileAsync(picker.FileName); loaded = true; });
        if (loaded) ApplyReverbEdits();
    }

    internal async Task LoadReverbFileAsync(string path)
    {
        if (new FileInfo(path).Length > 16384) throw new FormatException("Arquivo de reverb excessivamente grande.");
        var preset = ReverbPreset.Import(await File.ReadAllTextAsync(path));
        if (reverbRecovery != null && preset.Identity != reverbRecovery.Identity)
            throw new FormatException("O arquivo pertence a outra identidade de pedal.");
        Reverb.Load(preset.Identity, preset.Parameters);
        Reverb.Message = "Arquivo carregado no computador.";
    }

    public void ApplyReverbEdits() { if (!Busy && ReverbPreviewActive && !reverbStopped) ReverbEdited(); }

    public async Task ExportReverbAsync()
    {
        if (Busy || !Reverb.Loaded) return;
        var picker = new SaveFileDialog { Filter = Localization.LanguageService.T("Reverb (*.reverb.json)") + "|*.reverb.json", FileName = Localization.LanguageService.T("Meu reverb.reverb.json") };
        if (picker.ShowDialog() != true) return;
        await ExecuteAdvancedEffect(4, async () =>
        {
            var exported = Reverb.Parameters;
            await File.WriteAllTextAsync(picker.FileName, ReverbPreset.Export(Reverb.Identity, exported));
            RecordOperation("Ajustes de reverb exportados para arquivo.");
            if (Reverb.Parameters == exported)
                Reverb.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
