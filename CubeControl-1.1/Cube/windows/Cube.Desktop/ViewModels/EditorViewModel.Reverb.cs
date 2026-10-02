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
    private Task? reverbMonitorTask;
    private CancellationTokenSource? reverbMonitorStop;
    private bool reverbPending, reverbStopped;
    private bool reverbRecoveryInvalid;
    private bool reverbMonitoring;
    private bool ReverbBackgroundOperation => reverbSendTask != null || reverbMonitoring;
    private string PendingReverb => Path.Combine(DataDirectory, "pending-reverb-preview.json");
    public bool ReverbPreviewActive => reverbRecovery != null;
    public string ReverbGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : reverbStopped ? "Prévia interrompida · restaure o efeito; consulte a mensagem abaixo."
        : ReverbPreviewActive ? "Prévia automática em RAM · não salva no pedal."
        : !Current.Musical.ReverbEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartReverb ? "Recurso indisponível durante a operação ou prévia atual."
        : "Edição local · leia o efeito ativo para iniciar a prévia.";
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
            Reverb.Message = "Backup de reverb inválido. Prévia bloqueada; o arquivo original foi preservado.";
            Refresh();
            throw;
        }
        Reverb.Load(reverbRecovery.Identity, reverbRecovery.Target);
        reverbStopped = true;
        Reverb.Message = "Prévia interrompida. Conecte o mesmo pedal e use Restaurar reverb. Nenhum ajuste será reaplicado automaticamente.";
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
            Reverb.Message = "Prévia experimental iniciada. Ajustes alteram somente a RAM; escuta ainda não validada.";
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
                try { await Execute(async () => await session.CheckReverbPreview(reverbRecovery)); }
                finally { reverbMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
    }

    private void ReverbEdited()
    {
        if (!ReverbPreviewActive || reverbStopped)
        {
            if (!reverbStopped) Reverb.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Reverb.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
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
                        await session.ApplyReverbPreview(reverbRecovery, Reverb.Parameters, KeepReverbRecovery);
                        Reverb.Message = "Ajustes conferidos na RAM · validação sonora pendente.";
                    }
                    catch
                    {
                        reverbStopped = true; reverbPending = false;
                        Reverb.Message = "Prévia interrompida. Restauração não confirmada; backup preservado. Use Restaurar reverb.";
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
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Reverb experimental (*.reverb.json)") + "|*.reverb.json" };
        if (picker.ShowDialog() != true) return;
        await Execute(() => LoadReverbFileAsync(picker.FileName));
    }

    internal async Task LoadReverbFileAsync(string path)
    {
        if (new FileInfo(path).Length > 16384) throw new FormatException("Arquivo de reverb excessivamente grande.");
        var preset = ReverbPreset.Import(await File.ReadAllTextAsync(path));
        if (reverbRecovery != null && preset.Identity != reverbRecovery.Identity)
            throw new FormatException("O arquivo pertence a outra identidade de pedal.");
        Reverb.Load(preset.Identity, preset.Parameters);
        Reverb.Message = "Arquivo carregado no computador. Use Aplicar ajustes para enviá-lo à prévia ativa.";
    }

    public void ApplyReverbEdits() { if (!Busy && ReverbPreviewActive && !reverbStopped) ReverbEdited(); }

    public async Task ExportReverbAsync()
    {
        if (Busy || !Reverb.Loaded) return;
        var picker = new SaveFileDialog { Filter = Localization.LanguageService.T("Reverb experimental (*.reverb.json)") + "|*.reverb.json", FileName = Localization.LanguageService.T("Meu reverb.reverb.json") };
        if (picker.ShowDialog() != true) return;
        await Execute(async () =>
        {
            var exported = Reverb.Parameters;
            await File.WriteAllTextAsync(picker.FileName, ReverbPreset.Export(Reverb.Identity, exported));
            if (Reverb.Parameters == exported)
                Reverb.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
