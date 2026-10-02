using System.IO;
using System.Text.Json;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public ChorusViewModel Chorus { get; } = new();
    private ChorusRecovery? chorusRecovery;
    private Task? chorusSendTask;
    private Task? chorusMonitorTask;
    private CancellationTokenSource? chorusMonitorStop;
    private bool chorusPending, chorusStopped;
    private bool chorusRecoveryInvalid;
    private bool chorusMonitoring;
    private bool ChorusBackgroundOperation => chorusSendTask != null || chorusMonitoring;
    private string PendingChorus => Path.Combine(DataDirectory, "pending-chorus-preview.json");
    public bool ChorusPreviewActive => chorusRecovery != null;
    public string ChorusGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : chorusStopped ? "Prévia interrompida · restaure o efeito; consulte a mensagem abaixo."
        : ChorusPreviewActive ? "Prévia automática em RAM · não salva no pedal."
        : !Current.Musical.ChorusEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartChorus ? "Recurso indisponível durante a operação ou prévia atual."
        : "Edição local · leia o efeito ativo para iniciar a prévia.";
    public bool CanStartChorus => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !ChorusPreviewActive && !File.Exists(PendingChorus)
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool CanRestoreChorus => Connected && !Busy && ChorusPreviewActive;
    public bool CanEditChorus => Chorus.Loaded && (!Busy || chorusSendTask != null) && !chorusStopped;
    public bool CanUseChorusFiles => !Busy;
    public bool CanApplyChorus => !Busy && ChorusPreviewActive && !chorusStopped;

    private async Task KeepChorusRecovery(ChorusRecovery recovery)
    {
        recovery.Validate();
        Directory.CreateDirectory(DataDirectory);
        string json = JsonSerializer.Serialize(recovery);
        string temporary = PendingChorus + ".tmp";
        await File.WriteAllTextAsync(temporary, json);
        var verified = JsonSerializer.Deserialize<ChorusRecovery>(await File.ReadAllTextAsync(temporary))!;
        verified.Validate();
        if (await File.ReadAllTextAsync(temporary) != json) throw new IOException("Backup de chorus divergiu.");
        File.Move(temporary, PendingChorus, true);
        chorusRecovery = recovery;
        Refresh();
    }

    private async Task LoadPendingChorus()
    {
        if (!File.Exists(PendingChorus)) return;
        try
        {
            if (new FileInfo(PendingChorus).Length > 65536) throw new FormatException("Backup de chorus excessivamente grande.");
            var pending = JsonSerializer.Deserialize<ChorusRecovery>(await File.ReadAllTextAsync(PendingChorus))
                ?? throw new FormatException("Backup de chorus vazio.");
            pending.Validate();
            chorusRecovery = pending;
        }
        catch
        {
            chorusRecoveryInvalid = true; chorusStopped = true;
            Chorus.Message = "Backup de chorus inválido. Prévia bloqueada; o arquivo original foi preservado.";
            Refresh();
            throw;
        }
        Chorus.Load(chorusRecovery.Identity, chorusRecovery.Target);
        chorusStopped = true;
        Chorus.Message = "Prévia interrompida. Conecte o mesmo pedal e use Restaurar chorus. Nenhum ajuste será reaplicado automaticamente.";
        Refresh();
    }

    public async Task ReadChorusAsync()
    {
        if (!CanStartChorus) return;
        await Execute(async () =>
        {
            await session!.BeginChorusPreview(SelectedIndex, KeepChorusRecovery);
            string directory = Path.Combine(DataDirectory, "ChorusRecovery");
            Directory.CreateDirectory(directory);
            File.Copy(PendingChorus, Path.Combine(directory, $"chorus-{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}.json"));
            Chorus.Load(session.IdentityHex, chorusRecovery!.Target);
            chorusStopped = false;
            Chorus.Message = "Prévia experimental iniciada. Ajustes alteram somente a RAM; escuta ainda não validada.";
        });
        if (ChorusPreviewActive && !chorusStopped)
        {
            chorusMonitorStop = new();
            chorusMonitorTask = MonitorChorus(chorusMonitorStop.Token);
        }
    }

    private async Task MonitorChorus(CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && ChorusPreviewActive && !chorusStopped)
            {
                await Task.Delay(2000, stop);
                if (Busy || chorusSendTask != null || session == null || chorusRecovery == null) continue;
                chorusMonitoring = true;
                try { await Execute(async () => await session.CheckChorusPreview(chorusRecovery)); }
                finally { chorusMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
    }

    private void ChorusEdited()
    {
        if (!ChorusPreviewActive || chorusStopped)
        {
            if (!chorusStopped) Chorus.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Chorus.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
        chorusPending = true;
        chorusSendTask ??= SendChorusEdits();
    }

    private async Task SendChorusEdits()
    {
        await Task.Yield();
        try
        {
            await Execute(async () =>
            {
                while (chorusPending && !chorusStopped)
                {
                    chorusPending = false;
                    try
                    {
                        if (session == null || chorusRecovery == null) throw new IOException("Reconecte para restaurar o chorus.");
                        await session.ApplyChorusPreview(chorusRecovery, Chorus.Parameters, KeepChorusRecovery);
                        Chorus.Message = "Ajustes conferidos na RAM · validação sonora pendente.";
                    }
                    catch
                    {
                        chorusStopped = true; chorusPending = false;
                        Chorus.Message = "Prévia interrompida. Restauração não confirmada; backup preservado. Use Restaurar chorus.";
                        throw;
                    }
                }
            });
        }
        finally { chorusSendTask = null; Refresh(); }
    }

    public async Task EndChorusPreviewAsync()
    {
        chorusMonitorStop?.Cancel();
        if (chorusMonitorTask != null) await chorusMonitorTask;
        chorusMonitorTask = null;
        chorusMonitorStop?.Dispose(); chorusMonitorStop = null;
        chorusPending = false;
        if (chorusSendTask != null) await chorusSendTask;
        if (chorusRecovery == null) return;
        if (session == null) throw new IOException("Reconecte o pedal para restaurar o chorus. Backup preservado.");
        chorusStopped = true;
        try
        {
            await session.RestoreChorusPreview(chorusRecovery);
            Chorus.Load(chorusRecovery.Identity, ChorusParameters.Read(chorusRecovery.OriginalDsp));
            File.Delete(PendingChorus);
            chorusRecovery = null; chorusStopped = false;
            Chorus.Message = "Chorus original restaurado e conferido. Edição local disponível.";
        }
        catch { Chorus.Message = "Restauração não confirmada. O backup do chorus continua preservado."; throw; }
        finally { Refresh(); }
    }

    public async Task RestoreChorusAsync()
    {
        if (!CanRestoreChorus) return;
        await Execute(EndChorusPreviewAsync);
    }

    public async Task ImportChorusAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Chorus experimental (*.chorus.json)") + "|*.chorus.json" };
        if (picker.ShowDialog() != true) return;
        await Execute(() => LoadChorusFileAsync(picker.FileName));
    }

    internal async Task LoadChorusFileAsync(string path)
    {
        if (new FileInfo(path).Length > 16384) throw new FormatException("Arquivo de chorus excessivamente grande.");
        var preset = ChorusPreset.Import(await File.ReadAllTextAsync(path));
        if (chorusRecovery != null && preset.Identity != chorusRecovery.Identity)
            throw new FormatException("O arquivo pertence a outra identidade de pedal.");
        Chorus.Load(preset.Identity, preset.Parameters);
        Chorus.Message = "Arquivo carregado no computador. Use Aplicar ajustes para enviá-lo à prévia ativa.";
    }

    public void ApplyChorusEdits() { if (!Busy && ChorusPreviewActive && !chorusStopped) ChorusEdited(); }

    public async Task ExportChorusAsync()
    {
        if (Busy || !Chorus.Loaded) return;
        var picker = new SaveFileDialog { Filter = Localization.LanguageService.T("Chorus experimental (*.chorus.json)") + "|*.chorus.json", FileName = Localization.LanguageService.T("Meu chorus.chorus.json") };
        if (picker.ShowDialog() != true) return;
        await Execute(async () =>
        {
            var exported = Chorus.Parameters;
            await File.WriteAllTextAsync(picker.FileName, ChorusPreset.Export(Chorus.Identity, exported));
            if (Chorus.Parameters == exported)
                Chorus.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
