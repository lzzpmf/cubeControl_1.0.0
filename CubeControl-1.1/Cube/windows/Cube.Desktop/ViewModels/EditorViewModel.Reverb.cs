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
    private bool reverbRecoveryInvalid;
    private bool reverbMonitoring;
    private bool ReverbBackgroundOperation => reverbSendTask != null || reverbMonitoring;
    private string PendingReverb => Path.Combine(DataDirectory, "pending-reverb-preview.json");
    public bool ReverbPreviewActive => reverbRecovery != null;
    public string ReverbGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : reverbStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
        : ReverbPreviewActive ? "Edição automática em RAM · não salva no pedal."
        : !Current.Musical.ReverbEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartReverb ? "Recurso indisponível durante a operação ou edição em RAM atual."
        : "Abra as opções avançadas para ler o efeito ativo e iniciar a edição em RAM.";
    public bool CanStartReverb => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !ReverbPreviewActive && !File.Exists(PendingReverb)
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool CanRestoreReverb => Connected && !Busy && ReverbPreviewActive;
    public bool CanEditReverb => Reverb.Loaded && (!Busy || reverbSendTask != null) && !reverbStopped;
    public bool CanUseReverbFiles => !Busy;
    public bool CanApplyReverb => !Busy && ReverbPreviewActive && !reverbStopped;

    private async Task KeepReverbRecovery(ReverbRecovery recovery)
    {
        recovery.Validate();
        Directory.CreateDirectory(DataDirectory);
        string json = JsonSerializer.Serialize(recovery);
        string temporary = PendingReverb + ".tmp";
        await File.WriteAllTextAsync(temporary, json);
        var verified = JsonSerializer.Deserialize<ReverbRecovery>(await File.ReadAllTextAsync(temporary))!;
        verified.Validate();
        if (await File.ReadAllTextAsync(temporary) != json) throw new IOException("Backup de reverb divergiu.");
        File.Move(temporary, PendingReverb, true);
        reverbRecovery = recovery;
        Refresh();
    }

    private async Task LoadPendingReverb()
    {
        if (!File.Exists(PendingReverb)) return;
        try
        {
            if (new FileInfo(PendingReverb).Length > 65536) throw new FormatException("Backup de reverb excessivamente grande.");
            var pending = JsonSerializer.Deserialize<ReverbRecovery>(await File.ReadAllTextAsync(PendingReverb))
                ?? throw new FormatException("Backup de reverb vazio.");
            pending.Validate();
            reverbRecovery = pending;
        }
        catch
        {
            reverbRecoveryInvalid = true; reverbStopped = true;
            Reverb.Message = "Backup de reverb inválido. Edição em RAM bloqueada; o arquivo original foi preservado.";
            Refresh();
            throw;
        }
        Reverb.Load(reverbRecovery.Identity, reverbRecovery.Target);
        reverbStopped = true;
        Reverb.Message = "Edição em RAM interrompida. Conecte o mesmo pedal e use Restaurar reverb. Nenhum ajuste será reaplicado automaticamente.";
        Refresh();
    }

    public async Task ReadReverbAsync()
    {
        if (!CanStartReverb) return;
        await Execute(async () =>
        {
            await session!.BeginReverbPreview(SelectedIndex, KeepReverbRecovery);
            string directory = Path.Combine(DataDirectory, "ReverbRecovery");
            Directory.CreateDirectory(directory);
            File.Copy(PendingReverb, Path.Combine(directory, $"reverb-{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}.json"));
            Reverb.Load(session.IdentityHex, reverbRecovery!.Target);
            reverbStopped = false;
            RecordOperation("Prévia de reverb iniciada na RAM.");
            Reverb.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
        });
        if (ReverbPreviewActive && !reverbStopped)
        {
            reverbMonitorStop = new();
            reverbMonitorTask = MonitorReverb(reverbMonitorStop.Token);
        }
    }

    private async Task MonitorReverb(CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && ReverbPreviewActive && !reverbStopped)
            {
                await Task.Delay(2000, stop);
                if (Busy || reverbSendTask != null || session == null || reverbRecovery == null) continue;
                reverbMonitoring = true;
                try { await CheckPreviewInBackground(() => session.CheckReverbPreview(reverbRecovery)); }
                finally { reverbMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception ex) { Fault(ex); }
    }

    private void ReverbEdited()
    {
        if (!ReverbPreviewActive || reverbStopped)
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
            await Execute(async () =>
            {
                while (reverbPending && !reverbStopped)
                {
                    reverbPending = false;
                    try
                    {
                        if (session == null || reverbRecovery == null) throw new IOException("Reconecte para restaurar o reverb.");
                        var parameters = Reverb.Parameters;
                        var context = reverbRecovery.OriginalDsp;
                        object? group = reverbHistoryGroup;
                        await session.ApplyReverbPreview(reverbRecovery, parameters, KeepReverbRecovery);
                        RecordOperation("Prévia de reverb ajustada na RAM.", async () =>
                        {
                            Reverb.Load(Reverb.Identity, parameters); ReverbEdited();
                            if (reverbSendTask != null) await reverbSendTask;
                            return !reverbStopped && reverbRecovery?.Target == parameters;
                        }, () => Connected && !Busy && !reverbStopped && ReferenceEquals(reverbRecovery?.OriginalDsp, context), group);
                        Reverb.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        reverbStopped = true; reverbPending = false;
                        Reverb.Message = "Edição em RAM interrompida. Restauração não confirmada; backup preservado. Use Restaurar reverb.";
                        throw;
                    }
                }
            });
        }
        finally { reverbSendTask = null; Refresh(); }
    }

    public async Task EndReverbPreviewAsync()
    {
        reverbMonitorStop?.Cancel();
        if (reverbMonitorTask != null) await reverbMonitorTask;
        reverbMonitorTask = null;
        reverbMonitorStop?.Dispose(); reverbMonitorStop = null;
        reverbPending = false;
        if (reverbSendTask != null) await reverbSendTask;
        if (reverbRecovery == null) return;
        if (session == null) throw new IOException("Reconecte o pedal para restaurar o reverb. Backup preservado.");
        reverbStopped = true;
        try
        {
            await session.RestoreReverbPreview(reverbRecovery);
            Reverb.Load(reverbRecovery.Identity, ReverbParameters.Read(reverbRecovery.OriginalDsp));
            File.Delete(PendingReverb);
            reverbRecovery = null; reverbStopped = false;
            Reverb.Message = "Reverb original restaurado e conferido. Edição local disponível.";
            RecordOperation("Prévia de reverb encerrada; parâmetros anteriores restaurados.");
        }
        catch { Reverb.Message = "Restauração não confirmada. O backup do reverb continua preservado."; throw; }
        finally { Refresh(); }
    }

    public async Task RestoreReverbAsync()
    {
        if (!CanRestoreReverb) return;
        await Execute(EndReverbPreviewAsync);
    }

    public async Task ImportReverbAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Reverb (*.reverb.json)") + "|*.reverb.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await Execute(async () => { await LoadReverbFileAsync(picker.FileName); loaded = true; });
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
        await Execute(async () =>
        {
            var exported = Reverb.Parameters;
            await File.WriteAllTextAsync(picker.FileName, ReverbPreset.Export(Reverb.Identity, exported));
            RecordOperation("Ajustes de reverb exportados para arquivo.");
            if (Reverb.Parameters == exported)
                Reverb.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
