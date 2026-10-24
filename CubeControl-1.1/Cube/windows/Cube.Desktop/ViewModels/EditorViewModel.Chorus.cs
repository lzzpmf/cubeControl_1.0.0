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
    private object? chorusHistoryGroup;
    private Task? chorusMonitorTask;
    private CancellationTokenSource? chorusMonitorStop;
    private bool chorusPending, chorusStopped;
    private bool chorusMonitoring;
    private bool ChorusBackgroundOperation => chorusSendTask != null || chorusMonitoring;
    public bool ChorusPreviewActive => chorusRecovery != null;
    public string ChorusGraphState => !Connected ? "Dispositivo desconectado · edição local quando disponível."
        : chorusStopped ? "Edição em RAM interrompida · restaure o efeito; consulte a mensagem abaixo."
        : ChorusPreviewActive && !IsModulationPreviewAudible(1) ? "Ajustes preservados. Use Ouvir chorus para selecionar este som; as outras prévias continuam abertas."
        : ChorusPreviewActive ? "Edição automática em RAM · não salva no pedal."
        : !Current.Musical.ChorusEnabled ? "Efeito desligado no preset do aplicativo · confira também o preset físico."
        : !CanStartChorus ? "Recurso indisponível durante a operação ou edição em RAM atual."
        : "Abra as opções avançadas para ler o efeito ativo e iniciar a edição em RAM.";
    public bool CanStartChorus => CanOperate && !AmpPreviewActive && !Ir.PreviewActive && !ChorusPreviewActive
        ;
    public bool CanRestoreChorus => Connected && !Busy && ChorusPreviewActive;
    public bool CanEditChorus => Chorus.Loaded && (!Busy || chorusSendTask != null) && !chorusStopped;
    public bool CanUseChorusFiles => !Busy;
    public bool CanApplyChorus => !Busy && ChorusPreviewActive && !chorusStopped;

    private Task KeepChorusRecovery(ChorusRecovery recovery)
    {
        recovery.Validate();
        // Own the snapshot for this process only; never recover a previous app session.
        chorusRecovery = JsonSerializer.Deserialize<ChorusRecovery>(JsonSerializer.Serialize(recovery))!;
        chorusRecovery.Validate();
        Refresh();
        return Task.CompletedTask;
    }

    public Task ReadChorusAsync() => ReadAdvancedPreviewAsync(1, () => chorusStopped, () => CanStartChorus, async () =>
    {
        await session!.BeginChorusPreview(SelectedIndex, KeepChorusRecovery);
        Chorus.Load(session.IdentityHex, chorusRecovery!.Target);
        chorusStopped = false;
        RecordOperation("Prévia de chorus iniciada na RAM.");
        Chorus.Message = "Edição em RAM iniciada · ajustes não salvos no pedal.";
    });

    private Task MonitorChorus(CancellationToken stop) => MonitorAdvancedPreviewAsync(1, stop,
        () => ChorusPreviewActive && !chorusStopped,
        () => chorusSendTask == null && chorusRecovery != null && IsModulationPreviewAudible(1),
        value => chorusMonitoring = value, () => session!.CheckChorusPreview(chorusRecovery!));

    private void ChorusEdited()
    {
        if (retiringAdvanced[1]) return;
        if (!ChorusPreviewActive || chorusStopped || !IsModulationPreviewAudible(1))
        {
            if (!chorusStopped) Chorus.Message = "Edição local alterada · ainda não aplicada nem exportada.";
            return;
        }
        Chorus.Message = "Alterações pendentes de aplicação na RAM · não salvas no pedal.";
        chorusHistoryGroup = OperationGroup("Chorus");
        chorusPending = true;
        chorusSendTask ??= SendChorusEdits();
    }

    private async Task SendChorusEdits()
    {
        await Task.Yield();
        try
        {
            await ExecuteAdvancedEffect(1, async () =>
            {
                while (chorusPending && !chorusStopped && IsModulationPreviewAudible(1))
                {
                    chorusPending = false;
                    try
                    {
                        if (session == null || chorusRecovery == null) throw new IOException("Reconecte para restaurar o chorus.");
                        var parameters = Chorus.Parameters;
                        if (parameters == chorusRecovery.Target)
                        {
                            Chorus.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                            continue;
                        }
                        var context = chorusRecovery.OriginalDsp;
                        object? group = chorusHistoryGroup;
                        await session.ApplyChorusPreview(chorusRecovery, parameters, KeepChorusRecovery);
                        RecordOperation("Prévia de chorus ajustada na RAM.", async () =>
                        {
                            Chorus.Load(Chorus.Identity, parameters); ChorusEdited();
                            if (!Busy && chorusSendTask != null) await chorusSendTask;
                            return !chorusStopped && chorusRecovery?.Target == parameters;
                        }, () => Connected && !Busy && !chorusStopped && ReferenceEquals(chorusRecovery?.OriginalDsp, context), group);
                        Chorus.Message = "Ajustes conferidos na RAM · não salvos no pedal.";
                    }
                    catch
                    {
                        chorusStopped = true; chorusPending = false;
                        Chorus.Message = "Edição em RAM interrompida. Restauração não confirmada; snapshot preservado nesta sessão. Use Restaurar chorus.";
                        throw;
                    }
                }
            });
        }
        finally { chorusSendTask = null; Refresh(); }
    }

    public async Task EndChorusPreviewAsync()
    {
        retiringAdvanced[1] = true;
        chorusPending = false;
        try
        {
            chorusMonitorStop?.Cancel();
            if (chorusMonitorTask != null) await chorusMonitorTask;
            chorusMonitorTask = null;
            chorusMonitorStop?.Dispose(); chorusMonitorStop = null;
            chorusPending = false;
            if (!Busy && chorusSendTask != null) await chorusSendTask;
            if (chorusRecovery == null) return;
            if (session == null) throw new IOException("Reconecte o pedal para restaurar o chorus. Snapshot preservado em memória nesta sessão.");
            if (await RetireObsoleteAdvancedRecoveryAsync(1)) return;
            if (!chorusStopped && !IsModulationPreviewAudible(1))
            {
                chorusRecovery = null;
                Chorus.Message = "Prévia encerrada. O outro efeito de modulação permanece selecionado; edição local preservada.";
                Refresh();
                return;
            }
            chorusStopped = true;
            await RestoreAdvancedSnapshotAsync(
                () => session.RestoreChorusPreview(chorusRecovery),
                () =>
                {
                    Chorus.Load(chorusRecovery.Identity, ChorusParameters.Read(chorusRecovery.OriginalDsp));
                    chorusRecovery = null; chorusStopped = false;
                },
                value => Chorus.Message = value,
                "Chorus original restaurado e conferido. Edição local disponível.",
                "Prévia de chorus encerrada; parâmetros anteriores restaurados.",
                "Restauração não confirmada. O snapshot do chorus continua em memória nesta sessão.");
        }
        finally { retiringAdvanced[1] = false; }
    }

    public async Task RestoreChorusAsync()
    {
        if (!CanRestoreChorus) return;
        await CloseAdvancedPreviewAsync(1);
    }

    public async Task ImportChorusAsync()
    {
        if (Busy) return;
        var picker = new OpenFileDialog { Filter = Localization.LanguageService.T("Chorus (*.chorus.json)") + "|*.chorus.json" };
        if (picker.ShowDialog() != true) return;
        bool loaded = false;
        await ExecuteAdvancedEffect(1, async () => { await LoadChorusFileAsync(picker.FileName); loaded = true; });
        if (loaded) ApplyChorusEdits();
    }

    internal async Task LoadChorusFileAsync(string path)
    {
        if (new FileInfo(path).Length > 16384) throw new FormatException("Arquivo de chorus excessivamente grande.");
        var preset = ChorusPreset.Import(await File.ReadAllTextAsync(path));
        if (chorusRecovery != null && preset.Identity != chorusRecovery.Identity)
            throw new FormatException("O arquivo pertence a outra identidade de pedal.");
        Chorus.Load(preset.Identity, preset.Parameters);
        Chorus.Message = "Arquivo carregado no computador.";
    }

    public void ApplyChorusEdits() { if (!Busy && ChorusPreviewActive && !chorusStopped) ChorusEdited(); }

    public async Task ExportChorusAsync()
    {
        if (Busy || !Chorus.Loaded) return;
        var picker = new SaveFileDialog { Filter = Localization.LanguageService.T("Chorus (*.chorus.json)") + "|*.chorus.json", FileName = Localization.LanguageService.T("Meu chorus.chorus.json") };
        if (picker.ShowDialog() != true) return;
        await ExecuteAdvancedEffect(1, async () =>
        {
            var exported = Chorus.Parameters;
            await File.WriteAllTextAsync(picker.FileName, ChorusPreset.Export(Chorus.Identity, exported));
            RecordOperation("Ajustes de chorus exportados para arquivo.");
            if (Chorus.Parameters == exported)
                Chorus.Message = "Arquivo exportado no computador · isso não salva os ajustes no pedal.";
        });
    }
}
