using System.IO;
using System.Text.Json;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public DelayViewModel Delay { get; } = new();
    private DelayRecovery? delayRecovery;
    private Task? delaySendTask;
    private object? delayHistoryGroup;
    private Task? delayMonitorTask;
    private CancellationTokenSource? delayMonitorStop;
    private bool delayPending, delayStopped;
    private bool delayMonitoring;
    private bool DelayBackgroundOperation => delaySendTask != null || delayMonitoring;
    public bool DelayPreviewActive => delayRecovery != null;
    public string DelayGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : delayStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
        : DelayPreviewActive && !IsAdvancedEffectAudible(3) ? "Seção desligada. A janela permanece aberta; ligue a seção para retomar a prévia."
        : DelayPreviewActive ? "Edição automática em RAM · não salva no pedal."
        : !Current.Musical.DelayEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartDelay ? "Recurso indisponível durante a operação ou edição em RAM atual."
        : "Abra as opções avançadas para ler o efeito ativo e iniciar a edição em RAM.";
    public bool CanStartDelay => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !DelayPreviewActive
        ;
    public bool CanRestoreDelay => Connected && !Busy && DelayPreviewActive;
    public bool CanEditDelay => Delay.Loaded && (!Busy || delaySendTask != null) && !delayStopped;
    public bool CanUseDelayFiles => !Busy;
    public bool CanApplyDelay => !Busy && DelayPreviewActive && !delayStopped;

    private Task KeepDelayRecovery(DelayRecovery recovery)
    {
        recovery.Validate();
        // Own the snapshot for this process only; never recover a previous app session.
        delayRecovery = JsonSerializer.Deserialize<DelayRecovery>(JsonSerializer.Serialize(recovery))!;
        delayRecovery.Validate();
        Refresh();
        return Task.CompletedTask;
    }

    public Task ReadDelayAsync() => ReadAdvancedPreviewAsync(3, () => delayStopped, () => CanStartDelay, async () =>
    {
        await session!.BeginDelayPreview(SelectedIndex, KeepDelayRecovery);
        Delay.Load(session.IdentityHex, delayRecovery!.Target);
        delayStopped = false;
        RecordOperation("Prévia de delay iniciada na RAM.");
        Delay.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
    });

    private Task MonitorDelay(CancellationToken stop) => MonitorAdvancedPreviewAsync(3, stop,
        () => DelayPreviewActive && !delayStopped,
        () => delaySendTask == null && delayRecovery != null && IsAdvancedEffectAudible(3),
        value => delayMonitoring = value, () => session!.CheckDelayPreview(delayRecovery!));

    private void DelayEdited()
    {
        if (retiringAdvanced[3]) return;
        if (!DelayPreviewActive || delayStopped || !IsAdvancedEffectAudible(3))
        {
            if (!delayStopped) Delay.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Delay.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
        delayHistoryGroup = OperationGroup("Delay");
        delayPending = true;
        delaySendTask ??= SendDelayEdits();
    }

    private async Task SendDelayEdits()
    {
        await Task.Yield();
        try
        {
            await ExecuteAdvancedEffect(3, async () =>
            {
                while (delayPending && !delayStopped && IsAdvancedEffectAudible(3))
                {
                    delayPending = false;
                    try
                    {
                        if (session == null || delayRecovery == null) throw new IOException("Reconecte para restaurar o delay.");
                        var parameters = Delay.Parameters;
                        if (parameters == delayRecovery.Target)
                        {
                            Delay.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                            continue;
                        }
                        var context = delayRecovery.OriginalDsp;
                        object? group = delayHistoryGroup;
                        await session.ApplyDelayPreview(delayRecovery, parameters, KeepDelayRecovery);
                        RecordOperation("Prévia de delay ajustada na RAM.", async () =>
                        {
                            Delay.Load(Delay.Identity, parameters); DelayEdited();
                            if (!Busy && delaySendTask != null) await delaySendTask;
                            return !delayStopped && delayRecovery?.Target == parameters;
                        }, () => Connected && !Busy && !delayStopped && ReferenceEquals(delayRecovery?.OriginalDsp, context), group);
                        Delay.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        delayStopped = true; delayPending = false;
                        Delay.Message = "Edição em RAM interrompida. Restauração não confirmada; snapshot preservado nesta sessão. Use Restaurar delay.";
                        throw;
                    }
                }
            });
        }
        finally { delaySendTask = null; Refresh(); }
    }

    public async Task EndDelayPreviewAsync()
    {
        retiringAdvanced[3] = true;
        delayPending = false;
        try
        {
            delayMonitorStop?.Cancel();
            if (delayMonitorTask != null) await delayMonitorTask;
            delayMonitorTask = null;
            delayMonitorStop?.Dispose(); delayMonitorStop = null;
            delayPending = false;
            if (!Busy && delaySendTask != null) await delaySendTask;
            if (delayRecovery == null) return;
            if (session == null) throw new IOException("Reconecte o pedal para restaurar o delay. Snapshot preservado em memória nesta sessão.");
            if (await RetireObsoleteAdvancedRecoveryAsync(3)) return;
            if (!delayStopped && !IsAdvancedEffectAudible(3))
            {
                delayRecovery = null;
                Delay.Message = "Prévia encerrada. A seção permanece desligada; edição local preservada.";
                Refresh();
                return;
            }
            delayStopped = true;
            await RestoreAdvancedSnapshotAsync(
                () => session.RestoreDelayPreview(delayRecovery),
                () =>
                {
                    Delay.Load(delayRecovery.Identity, DelayParameters.Read(delayRecovery.OriginalDsp));
                    delayRecovery = null; delayStopped = false;
                },
                value => Delay.Message = value,
                "Delay original restaurado e conferido. Edição local disponível.",
                "Prévia de delay encerrada; parâmetros anteriores restaurados.",
                "Restauração não confirmada. O snapshot do delay continua em memória nesta sessão.");
        }
        finally { retiringAdvanced[3] = false; }
    }

    public async Task RestoreDelayAsync()
    {
        if (!CanRestoreDelay) return;
        await CloseAdvancedPreviewAsync(3);
    }

    public async Task ImportDelayAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Delay (*.delay.json)") + "|*.delay.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await ExecuteAdvancedEffect(3, async () => { await LoadDelayFileAsync(picker.FileName); loaded = true; });
        if (loaded) ApplyDelayEdits();
    }

    internal async Task LoadDelayFileAsync(string path)
    {
        if (new FileInfo(path).Length > 16384) throw new FormatException("Arquivo de delay excessivamente grande.");
        var preset = DelayPreset.Import(await File.ReadAllTextAsync(path));
        if (delayRecovery != null && preset.Identity != delayRecovery.Identity)
            throw new FormatException("O arquivo pertence a outra identidade de pedal.");
        Delay.Load(preset.Identity, preset.Parameters);
        Delay.Message = "Arquivo carregado no computador.";
    }

    public void ApplyDelayEdits() { if (!Busy && DelayPreviewActive && !delayStopped) DelayEdited(); }

    public async Task ExportDelayAsync()
    {
        if (Busy || !Delay.Loaded) return;
        var picker = new SaveFileDialog { Filter = Localization.LanguageService.T("Delay (*.delay.json)") + "|*.delay.json", FileName = Localization.LanguageService.T("Meu delay.delay.json") };
        if (picker.ShowDialog() != true) return;
        await ExecuteAdvancedEffect(3, async () =>
        {
            var exported = Delay.Parameters;
            await File.WriteAllTextAsync(picker.FileName, DelayPreset.Export(Delay.Identity, exported));
            RecordOperation("Ajustes de delay exportados para arquivo.");
            if (Delay.Parameters == exported)
                Delay.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
