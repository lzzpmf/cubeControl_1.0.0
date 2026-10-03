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
    private bool delayRecoveryInvalid;
    private bool delayMonitoring;
    private bool DelayBackgroundOperation => delaySendTask != null || delayMonitoring;
    private string PendingDelay => Path.Combine(DataDirectory, "pending-delay-preview.json");
    public bool DelayPreviewActive => delayRecovery != null;
    public string DelayGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : delayStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
        : DelayPreviewActive ? "Edição automática em RAM · não salva no pedal."
        : !Current.Musical.DelayEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartDelay ? "Recurso indisponível durante a operação ou edição em RAM atual."
        : "Abra as opções avançadas para ler o efeito ativo e iniciar a edição em RAM.";
    public bool CanStartDelay => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !DelayPreviewActive && !File.Exists(PendingDelay)
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool CanRestoreDelay => Connected && !Busy && DelayPreviewActive;
    public bool CanEditDelay => Delay.Loaded && (!Busy || delaySendTask != null) && !delayStopped;
    public bool CanUseDelayFiles => !Busy;
    public bool CanApplyDelay => !Busy && DelayPreviewActive && !delayStopped;

    private async Task KeepDelayRecovery(DelayRecovery recovery)
    {
        recovery.Validate();
        Directory.CreateDirectory(DataDirectory);
        string json = JsonSerializer.Serialize(recovery);
        string temporary = PendingDelay + ".tmp";
        await using (var file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None,
            4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
        {
            await file.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json));
            file.Flush(flushToDisk: true);
        }
        var verified = JsonSerializer.Deserialize<DelayRecovery>(await File.ReadAllTextAsync(temporary))!;
        verified.Validate();
        if (await File.ReadAllTextAsync(temporary) != json) throw new IOException("Backup de delay divergiu.");
        File.Move(temporary, PendingDelay, true);
        delayRecovery = recovery;
        Refresh();
    }

    private async Task LoadPendingDelay()
    {
        if (!File.Exists(PendingDelay)) return;
        try
        {
            if (new FileInfo(PendingDelay).Length > 65536) throw new FormatException("Backup de delay excessivamente grande.");
            var pending = JsonSerializer.Deserialize<DelayRecovery>(await File.ReadAllTextAsync(PendingDelay))
                ?? throw new FormatException("Backup de delay vazio.");
            pending.Validate();
            delayRecovery = pending;
        }
        catch
        {
            delayRecoveryInvalid = true; delayStopped = true;
            Delay.Message = "Backup de delay inválido. Edição em RAM bloqueada; o arquivo original foi preservado.";
            Refresh();
            throw;
        }
        Delay.Load(delayRecovery.Identity, delayRecovery.Target);
        delayStopped = true;
        Delay.Message = "Edição em RAM interrompida. Conecte o mesmo pedal e use Restaurar delay. Nenhum ajuste será reaplicado automaticamente.";
        Refresh();
    }

    public async Task ReadDelayAsync()
    {
        if (!CanStartDelay) return;
        await Execute(async () =>
        {
            await session!.BeginDelayPreview(SelectedIndex, KeepDelayRecovery);
            string directory = Path.Combine(DataDirectory, "DelayRecovery");
            Directory.CreateDirectory(directory);
            File.Copy(PendingDelay, Path.Combine(directory, $"delay-{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}.json"));
            Delay.Load(session.IdentityHex, delayRecovery!.Target);
            delayStopped = false;
            RecordOperation("Prévia de delay iniciada na RAM.");
            Delay.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
        });
        if (DelayPreviewActive && !delayStopped)
        {
            delayMonitorStop = new();
            delayMonitorTask = MonitorDelay(delayMonitorStop.Token);
        }
    }

    private async Task MonitorDelay(CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && DelayPreviewActive && !delayStopped)
            {
                await Task.Delay(2000, stop);
                if (Busy || delaySendTask != null || session == null || delayRecovery == null) continue;
                delayMonitoring = true;
                try { await CheckPreviewInBackground(() => session.CheckDelayPreview(delayRecovery)); }
                finally { delayMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (Exception ex) { Fault(ex); }
    }

    private void DelayEdited()
    {
        if (!DelayPreviewActive || delayStopped)
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
            await Execute(async () =>
            {
                while (delayPending && !delayStopped)
                {
                    delayPending = false;
                    try
                    {
                        if (session == null || delayRecovery == null) throw new IOException("Reconecte para restaurar o delay.");
                        var parameters = Delay.Parameters;
                        var context = delayRecovery.OriginalDsp;
                        object? group = delayHistoryGroup;
                        await session.ApplyDelayPreview(delayRecovery, parameters, KeepDelayRecovery);
                        RecordOperation("Prévia de delay ajustada na RAM.", async () =>
                        {
                            Delay.Load(Delay.Identity, parameters); DelayEdited();
                            if (delaySendTask != null) await delaySendTask;
                            return !delayStopped && delayRecovery?.Target == parameters;
                        }, () => Connected && !Busy && !delayStopped && ReferenceEquals(delayRecovery?.OriginalDsp, context), group);
                        Delay.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        delayStopped = true; delayPending = false;
                        Delay.Message = "Edição em RAM interrompida. Restauração não confirmada; backup preservado. Use Restaurar delay.";
                        throw;
                    }
                }
            });
        }
        finally { delaySendTask = null; Refresh(); }
    }

    public async Task EndDelayPreviewAsync()
    {
        delayMonitorStop?.Cancel();
        if (delayMonitorTask != null) await delayMonitorTask;
        delayMonitorTask = null;
        delayMonitorStop?.Dispose(); delayMonitorStop = null;
        delayPending = false;
        if (delaySendTask != null) await delaySendTask;
        if (delayRecovery == null) return;
        if (session == null) throw new IOException("Reconecte o pedal para restaurar o delay. Backup preservado.");
        delayStopped = true;
        try
        {
            await session.RestoreDelayPreview(delayRecovery);
            Delay.Load(delayRecovery.Identity, DelayParameters.Read(delayRecovery.OriginalDsp));
            File.Delete(PendingDelay);
            delayRecovery = null; delayStopped = false;
            Delay.Message = "Delay original restaurado e conferido. Edição local disponível.";
            RecordOperation("Prévia de delay encerrada; parâmetros anteriores restaurados.");
        }
        catch { Delay.Message = "Restauração não confirmada. O backup do delay continua preservado."; throw; }
        finally { Refresh(); }
    }

    public async Task RestoreDelayAsync()
    {
        if (!CanRestoreDelay) return;
        await Execute(EndDelayPreviewAsync);
    }

    public async Task ImportDelayAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Delay (*.delay.json)") + "|*.delay.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await Execute(async () => { await LoadDelayFileAsync(picker.FileName); loaded = true; });
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
        await Execute(async () =>
        {
            var exported = Delay.Parameters;
            await File.WriteAllTextAsync(picker.FileName, DelayPreset.Export(Delay.Identity, exported));
            RecordOperation("Ajustes de delay exportados para arquivo.");
            if (Delay.Parameters == exported)
                Delay.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
