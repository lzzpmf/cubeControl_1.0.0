using System.IO;
using System.Text.Json;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public PhaserViewModel Phaser { get; } = new();
    private PhaserRecovery? phaserRecovery;
    private Task? phaserSendTask;
    private object? phaserHistoryGroup;
    private Task? phaserMonitorTask;
    private CancellationTokenSource? phaserMonitorStop;
    private bool phaserPending, phaserStopped;
    private bool phaserMonitoring;
    private bool PhaserBackgroundOperation => phaserSendTask != null || phaserMonitoring;
    public bool PhaserPreviewActive => phaserRecovery != null;
    public string PhaserGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : phaserStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
        : PhaserPreviewActive && !IsModulationPreviewAudible(2) ? "Ajustes preservados. Use Ouvir phaser para selecionar este som; as outras prévias continuam abertas."
        : PhaserPreviewActive ? "Edição automática em RAM · não salva no pedal."
        : !Current.Musical.PhaserEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartPhaser ? "Recurso indisponível durante a operação ou edição em RAM atual."
        : "Abra as opções avançadas para ler o efeito ativo e iniciar a edição em RAM.";
    public bool CanStartPhaser => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !PhaserPreviewActive
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool CanRestorePhaser => Connected && !Busy && PhaserPreviewActive;
    public bool CanEditPhaser => Phaser.Loaded && (!Busy || phaserSendTask != null) && !phaserStopped;
    public bool CanUsePhaserFiles => !Busy;

    private Task KeepPhaserRecovery(PhaserRecovery recovery)
    {
        recovery.Validate();
        // Own the snapshot for this process only; never recover a previous app session.
        phaserRecovery = JsonSerializer.Deserialize<PhaserRecovery>(JsonSerializer.Serialize(recovery))!;
        phaserRecovery.Validate();
        Refresh();
        return Task.CompletedTask;
    }

    public async Task ReadPhaserAsync()
    {
        await advancedLifecycle.WaitAsync();
        try
        {
            await WaitForAdvancedEffectOperationAsync();
            if (HasAdvancedPreview(2) && phaserStopped)
                await ExecuteAdvancedEffect(2, () => ReconcileAdvancedRecoveryAsync(2));
            while (Busy) await WaitForAdvancedEffectOperationAsync();
            if (!CanStartPhaser) { EnsureAdvancedMonitor(2); return; }
            await ExecuteAdvancedEffect(2, async () =>
            {
                await PrepareAdvancedEffectAsync(2);
                await session!.BeginPhaserPreview(SelectedIndex, KeepPhaserRecovery);
                Phaser.Load(session.IdentityHex, phaserRecovery!.Target);
                phaserStopped = false;
                RecordOperation("Prévia de phaser iniciada na RAM.");
                Phaser.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
            });
            EnsureAdvancedMonitor(2);
        }
        finally { advancedLifecycle.Release(); }
    }

    private async Task MonitorPhaser(CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && PhaserPreviewActive && !phaserStopped)
            {
                await Task.Delay(2000, stop);
                if (Busy || phaserSendTask != null || session == null || phaserRecovery == null || !IsModulationPreviewAudible(2)) continue;
                phaserMonitoring = true;
                try { await CheckPreviewInBackground(2, () => session.CheckPhaserPreview(phaserRecovery)); }
                finally { phaserMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception ex) { FaultAdvancedPreview(ex, 2); }
    }

    private void PhaserEdited()
    {
        if (!PhaserPreviewActive || phaserStopped || !IsModulationPreviewAudible(2))
        {
            if (!phaserStopped) Phaser.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Phaser.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
        phaserHistoryGroup = OperationGroup("Phaser");
        phaserPending = true;
        phaserSendTask ??= SendPhaserEdits();
    }

    private async Task SendPhaserEdits()
    {
        await Task.Yield();
        try
        {
            await ExecuteAdvancedEffect(2, async () =>
            {
                while (phaserPending && !phaserStopped && IsModulationPreviewAudible(2))
                {
                    phaserPending = false;
                    try
                    {
                        if (session == null || phaserRecovery == null) throw new IOException("Reconecte para restaurar o phaser.");
                        var parameters = Phaser.Parameters;
                        if (parameters == phaserRecovery.Target)
                        {
                            Phaser.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                            continue;
                        }
                        var context = phaserRecovery.OriginalDsp;
                        object? group = phaserHistoryGroup;
                        await session.ApplyPhaserPreview(phaserRecovery, parameters, KeepPhaserRecovery);
                        RecordOperation("Prévia de phaser ajustada na RAM.", async () =>
                        {
                            Phaser.Load(Phaser.Identity, parameters); PhaserEdited();
                            if (!Busy && phaserSendTask != null) await phaserSendTask;
                            return !phaserStopped && phaserRecovery?.Target == parameters;
                        }, () => Connected && !Busy && !phaserStopped && ReferenceEquals(phaserRecovery?.OriginalDsp, context), group);
                        Phaser.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        phaserStopped = true; phaserPending = false;
                        Phaser.Message = "Edição em RAM interrompida. Restauração não confirmada; snapshot preservado nesta sessão. Use Restaurar phaser.";
                        throw;
                    }
                }
            });
        }
        finally { phaserSendTask = null; Refresh(); }
    }

    public async Task EndPhaserPreviewAsync()
    {
        phaserMonitorStop?.Cancel();
        if (phaserMonitorTask != null) await phaserMonitorTask;
        phaserMonitorTask = null;
        phaserMonitorStop?.Dispose(); phaserMonitorStop = null;
        phaserPending = false;
        if (!Busy && phaserSendTask != null) await phaserSendTask;
        if (phaserRecovery == null) return;
        if (session == null) throw new IOException("Reconecte o pedal para restaurar o phaser. Snapshot preservado em memória nesta sessão.");
        if (await RetireObsoleteAdvancedRecoveryAsync(2)) return;
        if (!phaserStopped && !IsModulationPreviewAudible(2))
        {
            phaserRecovery = null;
            Phaser.Message = "Prévia encerrada. O outro efeito de modulação permanece selecionado; edição local preservada.";
            Refresh();
            return;
        }
        phaserStopped = true;
        try
        {
            await session.RestorePhaserPreview(phaserRecovery);
            Phaser.Load(phaserRecovery.Identity, PhaserParameters.Read(phaserRecovery.OriginalDsp));
            phaserRecovery = null; phaserStopped = false;
            Phaser.Message = "Phaser original restaurado e conferido. Edição local disponível.";
            RecordOperation("Prévia de phaser encerrada; parâmetros anteriores restaurados.");
        }
        catch { Phaser.Message = "Restauração não confirmada. O snapshot do phaser continua em memória nesta sessão."; throw; }
        finally { Refresh(); }
    }

    public async Task RestorePhaserAsync()
    {
        if (!CanRestorePhaser) return;
        await CloseAdvancedPreviewAsync(2);
    }

    public async Task ImportPhaserAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = "Phaser (*.phaser.json)|*.phaser.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await ExecuteAdvancedEffect(2, async () =>
        {
            if (new FileInfo(picker.FileName).Length > 16384) throw new FormatException("Arquivo de phaser excessivamente grande.");
            var preset = PhaserPreset.Import(await File.ReadAllTextAsync(picker.FileName));
            if (phaserRecovery != null && preset.Identity != phaserRecovery.Identity)
                throw new FormatException("O arquivo pertence a outra identidade de pedal.");
            Phaser.Load(preset.Identity, preset.Parameters);
            loaded = true;
            Phaser.Message = "Arquivo carregado no computador.";
        });
        if (loaded) ApplyPhaserEdits();
    }

    public void ApplyPhaserEdits() { if (!Busy && PhaserPreviewActive && !phaserStopped) PhaserEdited(); }

    public async Task ExportPhaserAsync()
    {
        if (Busy || !Phaser.Loaded) return;
        var picker = new SaveFileDialog { Filter = "Phaser (*.phaser.json)|*.phaser.json", FileName = "Meu phaser.phaser.json" };
        if (picker.ShowDialog() != true) return;
        await ExecuteAdvancedEffect(2, async () =>
        {
            var exported = Phaser.Parameters;
            await File.WriteAllTextAsync(picker.FileName, PhaserPreset.Export(Phaser.Identity, exported));
            RecordOperation("Ajustes de phaser exportados para arquivo.");
            if (Phaser.Parameters == exported)
                Phaser.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
