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
    private Task? phaserMonitorTask;
    private CancellationTokenSource? phaserMonitorStop;
    private bool phaserPending, phaserStopped;
    private bool phaserMonitoring;
    private bool PhaserBackgroundOperation => phaserSendTask != null || phaserMonitoring;
    private string PendingPhaser => Path.Combine(DataDirectory, "pending-phaser-preview.json");
    public bool PhaserPreviewActive => phaserRecovery != null;
    public string PhaserGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : phaserStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
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

    private async Task KeepPhaserRecovery(PhaserRecovery recovery)
    {
        recovery.Validate();
        Directory.CreateDirectory(DataDirectory);
        string json = JsonSerializer.Serialize(recovery);
        string temporary = PendingPhaser + ".tmp";
        await File.WriteAllTextAsync(temporary, json);
        var verified = JsonSerializer.Deserialize<PhaserRecovery>(await File.ReadAllTextAsync(temporary))!;
        verified.Validate();
        if (await File.ReadAllTextAsync(temporary) != json) throw new IOException("Backup de phaser divergiu.");
        File.Move(temporary, PendingPhaser, true);
        phaserRecovery = recovery;
        Refresh();
    }

    private async Task LoadPendingPhaser()
    {
        if (!File.Exists(PendingPhaser)) return;
        phaserRecovery = JsonSerializer.Deserialize<PhaserRecovery>(await File.ReadAllTextAsync(PendingPhaser))
            ?? throw new FormatException("Backup de phaser vazio.");
        phaserRecovery.Validate();
        Phaser.Load(phaserRecovery.Identity, phaserRecovery.Target);
        phaserStopped = true;
        Phaser.Message = "Edição em RAM interrompida. Conecte o mesmo pedal e use Restaurar phaser. Nenhum ajuste será reaplicado automaticamente.";
        Refresh();
    }

    public async Task ReadPhaserAsync()
    {
        if (!CanStartPhaser) return;
        await Execute(async () =>
        {
            await session!.BeginPhaserPreview(SelectedIndex, KeepPhaserRecovery);
            string directory = Path.Combine(DataDirectory, "PhaserRecovery");
            Directory.CreateDirectory(directory);
            File.Copy(PendingPhaser, Path.Combine(directory, $"phaser-{DateTime.UtcNow:yyyyMMdd-HHmmss-fffffff}.json"));
            Phaser.Load(session.IdentityHex, phaserRecovery!.Target);
            phaserStopped = false;
            Phaser.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
        });
        if (PhaserPreviewActive && !phaserStopped)
        {
            phaserMonitorStop = new();
            phaserMonitorTask = MonitorPhaser(phaserMonitorStop.Token);
        }
    }

    private async Task MonitorPhaser(CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && PhaserPreviewActive && !phaserStopped)
            {
                await Task.Delay(2000, stop);
                if (Busy || phaserSendTask != null || session == null || phaserRecovery == null) continue;
                phaserMonitoring = true;
                try { await Execute(async () => await session.CheckPhaserPreview(phaserRecovery)); }
                finally { phaserMonitoring = false; RefreshPresentation(); }
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
    }

    private void PhaserEdited()
    {
        if (!PhaserPreviewActive || phaserStopped)
        {
            if (!phaserStopped) Phaser.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Phaser.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
        phaserPending = true;
        phaserSendTask ??= SendPhaserEdits();
    }

    private async Task SendPhaserEdits()
    {
        await Task.Yield();
        try
        {
            await Execute(async () =>
            {
                while (phaserPending && !phaserStopped)
                {
                    phaserPending = false;
                    try
                    {
                        if (session == null || phaserRecovery == null) throw new IOException("Reconecte para restaurar o phaser.");
                        await session.ApplyPhaserPreview(phaserRecovery, Phaser.Parameters, KeepPhaserRecovery);
                        Phaser.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        phaserStopped = true; phaserPending = false;
                        Phaser.Message = "Edição em RAM interrompida. Restauração não confirmada; backup preservado. Use Restaurar phaser.";
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
        if (phaserSendTask != null) await phaserSendTask;
        if (phaserRecovery == null) return;
        if (session == null) throw new IOException("Reconecte o pedal para restaurar o phaser. Backup preservado.");
        phaserStopped = true;
        try
        {
            await session.RestorePhaserPreview(phaserRecovery);
            Phaser.Load(phaserRecovery.Identity, PhaserParameters.Read(phaserRecovery.OriginalDsp));
            File.Delete(PendingPhaser);
            phaserRecovery = null; phaserStopped = false;
            Phaser.Message = "Phaser original restaurado e conferido. Edição local disponível.";
        }
        catch { Phaser.Message = "Restauração não confirmada. O backup do phaser continua preservado."; throw; }
        finally { Refresh(); }
    }

    public async Task RestorePhaserAsync()
    {
        if (!CanRestorePhaser) return;
        await Execute(EndPhaserPreviewAsync);
    }

    public async Task ImportPhaserAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = "Phaser (*.phaser.json)|*.phaser.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await Execute(async () =>
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
        await Execute(async () =>
        {
            var exported = Phaser.Parameters;
            await File.WriteAllTextAsync(picker.FileName, PhaserPreset.Export(Phaser.Identity, exported));
            if (Phaser.Parameters == exported)
                Phaser.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
