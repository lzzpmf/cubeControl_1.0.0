using Cube.Desktop.Localization;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Cube.Protocol;
using Cube.Usb;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel : Observable, IDisposable
{
    private CubeSession? session;
#if CUBE_SIMULATION
    private readonly FakeConnection simulatedDevice = new();
#endif
    private LiveEditQueue? queue;
    private byte[]? baseline, confirmed, edited;
    private IrRamSnapshot? originalRam;
    private string? ramIdentity;
    private FactoryResources? factory;
    private int selected;
    private bool busy, blocked;
    private string status = "Conecte seu Cube Baby por USB para começar.", log = "";
    public string DataDirectory { get; }
    public ConnectionViewModel Connection { get; } = new();
    public IrViewModel Ir { get; } = new();
    public IrAuditionViewModel IrAudition { get; }
    public PresetViewModel[] Presets { get; }
    private TaskCompletionSource? operationCompletion;
    public bool Busy
    {
        get => busy;
        private set
        {
            if (value && !busy) operationCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var completion = operationCompletion;
            Set(ref busy, value); Refresh();
            if (!value) completion?.TrySetResult();
        }
    }
    public bool Connected => session != null;
    public bool CanEdit => Connected && (!Busy || liveParameterOperation) && !blocked && edited != null && !Ir.PreviewActive && !AmpPreviewActive;
    public bool CanOperate => Connected && !Busy && !blocked;
    public bool CanConnect => !Connected && !Busy;
    public bool HasBank => edited != null;
    public bool Dirty => baseline != null && edited != null && !baseline.SequenceEqual(edited);
    public string Status { get => status; private set => Set(ref status, value); }
    public string Log { get => log; private set => Set(ref log, value); }
    public int SelectedIndex
    {
        get => selected;
        set
        {
            if (value is < 0 or > 2 || value == selected) return;
            if (HasAdvancedPreviews) { _ = SelectPresetAsync(value); return; }
            SetSelectedPreset(value);
        }
    }
    private void SetSelectedPreset(int value)
    {
        if (Set(ref selected, value, nameof(SelectedIndex))) { Notify(nameof(Current)); RefreshEditor(); }
    }
    private async Task SelectPresetAsync(int value)
    {
        await Execute(() => { SetSelectedPreset(value); return Task.CompletedTask; }, changesEffects: true);
        Notify(nameof(SelectedIndex));
    }
    public PresetViewModel Current => Presets[SelectedIndex];
    public ICommand RefreshPorts { get; }
    public ICommand Connect { get; }
    public ICommand Disconnect { get; }
    public ICommand Save { get; }
    public ICommand Reload { get; }
    public ICommand Export { get; }
    public ICommand Import { get; }
    public ICommand ChooseWav { get; }
    public ICommand PreviewIr { get; }
    public ICommand EndPreview { get; }
    public ICommand WriteIr { get; }
    public ICommand ReadSlot { get; }
    public ICommand RestoreIr { get; }
    public ICommand ResetPresets { get; }
    public ICommand ApplyIrPack { get; }
    public ICommand RestoreOriginalIrs { get; }

    public EditorViewModel(string? dataDirectory = null, Cube.Desktop.Services.IIrAuditionService? auditionService = null)
    {
        IrAudition = new IrAuditionViewModel(this, auditionService);
        DataDirectory = dataDirectory ?? Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "CubeControl");
#if CUBE_SIMULATION
        DataDirectory = dataDirectory ?? Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "CubeControl-Simulation");
        for (int slot = 0; slot < 3; slot++)
        {
            simulatedDevice.Bank[slot * 16] = (byte)(slot * 2);
            simulatedDevice.Bank[slot * 16 + 1] = 4;
            simulatedDevice.Bank[slot * 16 + 2] = 8;
            simulatedDevice.Bank[slot * 16 + 5] = 33;
        }
        Status = "SIMULAÇÃO — conecte o pedal virtual. Sem áudio do equipamento.";
#endif
        Presets = Enumerable.Range(0, 3).Select(i => new PresetViewModel(i, Change)).ToArray();
        IrLibrary = new IrLibraryViewModel(() => !Busy, importedDirectory: PackStore.ImportedDirectory);
        IrLibrary.SelectionChanged += SelectLibraryIr;
        RandomizeCabinetIr = new UiCommand(async () => await RandomizeCabinetIrAsync(), () => CanRandomizeCabinetIr);
        IrLibrary.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(IrLibrary.Entries)) CommandManager.InvalidateRequerySuggested(); };
        InitializeVisual();
        Amp.Edited += AmpEdited;
        Amp.PropertyChanged += AmpPresentationChanged;
        Phaser.Edited += PhaserEdited;
        Chorus.Edited += ChorusEdited;
        Reverb.Edited += ReverbEdited;
        Delay.Edited += DelayEdited;
        RefreshPorts = Command(async () => { SearchingPorts = true;
        try { await Connection.Refresh(); }
        finally { SearchingPorts = false; } Status = Connection.Input == null ? "Portas MIDI do Cube Baby não selecionadas. Verifique o cabo USB e os detalhes da conexão." : "Cube Baby encontrado. Conecte para ler os presets."; }, () => CanConnect);
        Connect = Command(ConnectAsync, () => CanConnect);
        Disconnect = Command(DisconnectAsync, () => Connected && !Busy, changesEffects: true);
        Reload = Command(async () => { if (!Dirty || await Ask("Reler o banco atual e descartar as alterações locais?")) { await RestorePreviewAsync(); await LoadBank(); Completed("Presets atualizados com a leitura do pedal."); } }, () => Connected && !Busy, changesEffects: true);
        Save = new UiCommand(async () => { if (CanEdit && Dirty) await Execute(SaveAsync, changesEffects: true); }, () => CanEdit && Dirty);
        Export = Command(ExportAsync, () => HasBank && !Busy);
        Import = Command(ImportAsync, () => CanEdit, changesEffects: true);
        ChooseWav = Command(ChooseWavAsync, () => !Busy);
        PreviewIr = Command(PreviewAsync, () => CanOperate && Ir.Audio != null);
        EndPreview = Command(EndPreviewAsync, () => Connected && !Busy && originalRam != null);
        EndAmpPreview = new UiCommand(async () => await RestoreAmpAsync(), () => CanRestoreAmp);
        WriteIr = new UiCommand(async () => await ConfirmWriteIrAsync(), () => CanOperate && Ir.Audio != null && !confirmingIrWrite);
        ReadSlot = Command(ReadSlotAsync, () => CanOperate);
        Ir.PropertyChanged += (_, _) => RefreshPresentation();
        RestoreIr = Command(RestoreIrAsync, () => CanOperate, changesEffects: true);
        ResetPresets = Command(ResetPresetsAsync, () => CanOperate && factory != null, changesEffects: true);
        ApplyIrPack = Command(ApplyIrPackAsync, () => CanOperate && (packEditor?.HasEntries ?? (SelectedIrPack != null)));
        RestoreOriginalIrs = Command(RestoreOriginalIrsAsync, () => CanOperate && originalPacks.Count > 0, changesEffects: true);
        InitializeFirmware();
        LoadFactory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CubeSuite", "CubeSuite.exe"));
    }

    private UiCommand Command(Func<Task> action, Func<bool> can, bool changesEffects = false) => new(async () => await Execute(action, changesEffects), can);
    private async Task WaitForAdvancedEffectOperationAsync()
    {
        while (Busy && operationCompletion != null) await operationCompletion.Task;
    }
    private async Task ExecuteAdvancedEffect(int stage, Func<Task> action)
    {
        bool started = false;
        while (!started)
        {
            await WaitForAdvancedEffectOperationAsync();
            SetAdvancedError(stage, "");
            await Execute(() => { started = true; return action(); }, advancedEffect: true, advancedStage: stage);
            if (!started && (!Connected || session?.IsFaulted == true || ErrorMessage.Length > 0)) return;
        }
    }
    private Task? previewCheckTask;
    private int previewCheckStage;
    private async Task CheckPreviewInBackground(int stage, Func<Task> check)
    {
        if (Busy || retiringAdvanced.Any(value => value) || previewCheckTask != null) return;
        previewCheckStage = stage;
        previewCheckTask = check();
        try { await previewCheckTask; }
        finally { previewCheckTask = null; }
    }
    private bool liveParameterOperation;
    public async Task Execute(Func<Task> action, bool changesEffects = false, bool advancedEffect = false, bool liveEdit = false, int? advancedStage = null)
    {
        if (Busy) return;
        if (!advancedEffect)
        {
            foreach (var sending in new[] { chorusSendTask, phaserSendTask, delaySendTask, reverbSendTask })
                if (sending != null) await sending;
            if (Busy) return;
        }
        string previousStatus = Status;
        long previousOperation = operationVersion;
        irSelectionGeneration++;
        transferGeneration++;
        if (!blocked) ErrorMessage = "";
        BusyDetail = "Aguarde a conclusão da operação.";
        InventoryReadCount = 0; ReadingInventory = false;
        liveParameterOperation = liveEdit;
        Busy = true;
        try
        {
            if (previewCheckTask != null)
            {
                try { await previewCheckTask; }
                catch (Exception ex) { if (session?.IsFaulted == true) throw; else FaultAdvancedPreview(ex, previewCheckStage); }
            }
            if (queue != null && !blocked) await queue.Flush();
            if (changesEffects && !await ConfirmAdvancedPreviewChangeAsync()) return;
            await action();
        }
        catch (Exception ex)
        {
            Status = ex.Message; ErrorMessage = ex.Message; AddLog("ERRO " + ex.Message);
            if (advancedEffect) FaultAdvancedPreview(ex, advancedStage);
            else if (session != null && (session.IsFaulted || ex is InvalidOperationException or IOException)) Fault(ex);
            else if (!automaticConnectionAttempt) NotifyFailure(ex.Message);
        }
        finally
        {
            Ir.TransferActive = false; ReadingInventory = false; liveParameterOperation = false; Busy = false;
            if (operationVersion == previousOperation && Status != previousStatus) RecordOperation(Status);
        }
    }

    private void Refresh()
    {
        Notify(nameof(CanChooseFirmware));
        Notify(nameof(DelayGraphState)); Notify(nameof(ChorusGraphState));
        Notify(nameof(PhaserGraphState)); Notify(nameof(ReverbGraphState));
        Notify(nameof(DelayPreviewActive)); Notify(nameof(CanStartDelay)); Notify(nameof(CanRestoreDelay));
        Notify(nameof(CanEditDelay)); Notify(nameof(CanUseDelayFiles)); Notify(nameof(CanApplyDelay));
        Notify(nameof(ReverbPreviewActive)); Notify(nameof(CanStartReverb)); Notify(nameof(CanRestoreReverb));
        Notify(nameof(CanEditReverb)); Notify(nameof(CanUseReverbFiles)); Notify(nameof(CanApplyReverb));
        Notify(nameof(ChorusPreviewActive)); Notify(nameof(CanStartChorus)); Notify(nameof(CanRestoreChorus));
        Notify(nameof(CanEditChorus)); Notify(nameof(CanUseChorusFiles)); Notify(nameof(CanApplyChorus));
        Notify(nameof(PhaserPreviewActive)); Notify(nameof(CanStartPhaser)); Notify(nameof(CanRestorePhaser));
        Notify(nameof(CanListenChorus)); Notify(nameof(CanListenPhaser));
        Notify(nameof(CanEditPhaser)); Notify(nameof(CanUsePhaserFiles));
        Notify(nameof(CanUseAmp)); Notify(nameof(CanAdjustAmp)); Notify(nameof(CanOpenGearGallery)); Notify(nameof(CanEditAmpLocally)); Notify(nameof(AmpPreviewActive));
        RefreshAmpAvailability();
        Notify(nameof(CanChooseIr));
        Notify(nameof(Connected)); Notify(nameof(CanConnect)); Notify(nameof(CanEdit)); Notify(nameof(CanOperate)); Notify(nameof(HasBank)); Notify(nameof(Dirty));
        Notify(nameof(CanMatch)); RefreshEditor();
        RefreshPresentation();
        CommandManager.InvalidateRequerySuggested();
    }
    private void AddLog(string value)
    {
        if (!Application.Current.Dispatcher.CheckAccess()) { Application.Current.Dispatcher.BeginInvoke(() => AddLog(value)); return; }
        Log = (Log.Length > 30000 ? Log[^18000..] : Log) + $"{DateTime.Now:HH:mm:ss.fff} {value}\n";
    }
    public async Task Initialize() => await Execute(async () =>
    {
        BusyDetail = "Procurando portas USB-MIDI…";
        SearchingPorts = true;
        try { await Connection.Refresh(); }
        finally { SearchingPorts = false; }
    });
    public async Task ConnectAsync()
    {
        LoadingDevice = true;
        try { await ConnectDeviceCore(); }
        finally { LoadingDevice = false; }
    }
    private async Task ConnectDeviceCore()
    {
        if (Dirty && !automaticConnectionAttempt && !await Ask("Há uma edição local não salva. Conectar e substituí-la pela leitura do pedal?")) return;
        await Connection.Refresh();
        if (Connection.Input == null || Connection.Output == null) throw new InvalidOperationException("Portas MIDI do Cube Baby não selecionadas. Verifique o cabo USB e os detalhes da conexão.");
        BusyDetail = Status = "Identificando Cube Baby…";
#if CUBE_SIMULATION
        var opened = new CubeSession(simulatedDevice);
#else
        var opened = new CubeSession(await WindowsMidiConnection.Open(Connection.Input.Id, Connection.Output.Id));
#endif
        try
        {
            opened.Log += AddLog; Connection.Identity = await opened.Identify();
            // An interrupted preview snapshot survives disconnect and can be restored after reconnect.
            await AttachSession(opened);
            automaticConnectionPaused = false;
            if (originalRam != null) { Ir.PreviewActive = true; Ir.PreviewConfirmed = false; Status = "Prévia interrompida: use Encerrar prévia para restaurar o estado anterior."; Ir.State = Status; }
        }
        catch { opened.Dispose(); session = null; Connection.Identity = "Desconectado"; throw; }
    }

    internal async Task AttachSession(CubeSession opened)
    {
        var previousEdits = automaticConnectionAttempt && Dirty ? edited!.ToArray() : null;
        var previousBaseline = baseline?.ToArray();
        if (previousEdits != null) await BackupBank(previousEdits, "edicao-antes-reconexao");
        session = opened; Connection.Identity = opened.Identity;
        bool wasLoading = LoadingDevice;
        LoadingDevice = true;
        try
        {
            await LoadBank();
            // USB unplugging can preserve RAM; power-off can restore the saved bank.
            // Keep Save available when the pedal still contains the unsaved edits.
            if (previousEdits != null && previousEdits.SequenceEqual(confirmed!))
            {
                baseline = previousBaseline;
                UpdateStates();
            }
            await RefreshInventoryAsync();
        }
        finally { LoadingDevice = wasLoading; }
        NotifyUser("Pedal reconhecido", originalRam == null
            ? previousEdits == null ? "Conexão USB confirmada. Presets e IRs carregados."
                : "Pedal reconectado e pronto para editar. O estado atual foi lido do pedal."
            : "Conexão USB confirmada. Encerre a prévia interrompida para restaurar o IR anterior.",
            Wpf.Ui.Controls.ControlAppearance.Success);
    }

    private async Task LoadBank()
    {
        BusyDetail = "Lendo presets A/B/C do aparelho…";
        var bank = await session!.ReadBank(); CubeBabyProfile.ValidateBank(bank); await BackupBank(bank, "leitura");
        baseline = bank.ToArray(); confirmed = bank.ToArray(); edited = bank.ToArray();
        history.Clear(); pendingMatches.Clear(); revisions.Clear(); gesture = null;
        blocked = false; ErrorMessage = ""; SetupQueue();
        foreach (var preset in Presets) { preset.Load(bank); preset.State = "Lido do aparelho"; }
        await RefreshKnownAmpArtworkAsync();
        Status = "Pronto. Ajustes são enviados durante o gesto; Salvar grava o banco A/B/C."; Refresh();
    }
    private void SetupQueue()
    {
        queue?.Stop();
        queue = new(async (key, value) =>
        {
            Presets[key.Slot].State = "Enviando…";
            confirmed = await session!.WriteLiveConfirmed(key.Slot, key.Offset, value, confirmed!);
            UpdateStates();
        });
        queue.Failed += Fault;
    }
    private void Change(ParameterKey key, int value)
    {
        if (edited == null) return;
        if (!CanEdit) { Presets[key.Slot].Parameters[key.Offset].Load(edited[key.Slot * 16 + key.Offset]); return; }
        if (HasAdvancedPreviews || liveParameterSending)
        {
            advancedParameterChanges[key] = value;
            if (!liveParameterSending) _ = ApplyAdvancedParameterChangesAsync();
            return;
        }
        ApplyParameterChange(key, value);
    }
    private readonly Dictionary<ParameterKey, int> advancedParameterChanges = new();
    private bool liveParameterSending;
    private async Task ApplyAdvancedParameterChangesAsync()
    {
        liveParameterSending = true;
        await Task.Yield();
        try
        {
            while (advancedParameterChanges.Count > 0 && Connected && !blocked)
            {
                await WaitForAdvancedEffectOperationAsync();
                await Execute(async () =>
                {
                    var offsets = new HashSet<int>();
                    while (advancedParameterChanges.Count > 0)
                    {
                        var changes = advancedParameterChanges.ToArray();
                        advancedParameterChanges.Clear();
                        foreach (var change in changes)
                        {
                            ApplyParameterChange(change.Key, change.Value);
                            offsets.Add(change.Key.Offset);
                        }
                        await Flush();
                    }
                    await SynchronizeAdvancedEffectsAsync(offsets);
                }, liveEdit: true);
            }
        }
        finally { advancedParameterChanges.Clear(); liveParameterSending = false; }
    }
    private async Task SynchronizeAdvancedEffectsAsync(IEnumerable<int> offsets, int? excludeStage = null)
    {
        var changed = offsets.ToHashSet();
        try
        {
            if (changed.Overlaps([8, 11]))
            {
                if (excludeStage != 1 && ChorusPreviewActive && !chorusStopped && IsModulationPreviewAudible(1))
                {
                    await session!.BeginChorusPreview(SelectedIndex, KeepChorusRecovery);
                    Chorus.Load(chorusRecovery!.Identity, chorusRecovery.Target);
                }
                if (excludeStage != 2 && PhaserPreviewActive && !phaserStopped && IsModulationPreviewAudible(2))
                {
                    await session!.BeginPhaserPreview(SelectedIndex, KeepPhaserRecovery);
                    Phaser.Load(phaserRecovery!.Identity, phaserRecovery.Target);
                }
            }
            if (changed.Overlaps([4, 6, 7, 11]) && excludeStage != 3 && DelayPreviewActive && !delayStopped && IsAdvancedEffectAudible(3))
            {
                await session!.BeginDelayPreview(SelectedIndex, KeepDelayRecovery);
                Delay.Load(delayRecovery!.Identity, delayRecovery.Target);
            }
            if (changed.Overlaps([3, 10]) && excludeStage != 4 && ReverbPreviewActive && !reverbStopped && IsAdvancedEffectAudible(4))
            {
                await session!.BeginReverbPreview(SelectedIndex, KeepReverbRecovery);
                Reverb.Load(reverbRecovery!.Identity, reverbRecovery.Target);
            }
            Refresh();
        }
        catch (Exception ex) { FaultAdvancedPreview(ex); }
    }
    private void ApplyParameterChange(ParameterKey key, int value)
    {
        history.Record(key, edited![key.Slot * 16 + key.Offset], gesture);
        bankOperationGroup = gesture ?? new object();
        bankOperationDescription = $"Preset {(char)('A' + key.Slot)} · {Presets[key.Slot].Parameters[key.Offset].Name}: {value}";
        edited[key.Slot * 16 + key.Offset] = (byte)value;
        revisions[key] = ++revision;
        Presets[key.Slot].State = "Enviando…"; queue!.Enqueue(key, value); Refresh();
    }
    private void UpdateStates()
    {
        for (int i = 0; i < 3; i++)
        {
            bool pending = !edited!.AsSpan(i * 16, 16).SequenceEqual(confirmed!.AsSpan(i * 16, 16));
            bool dirty = !edited.AsSpan(i * 16, 16).SequenceEqual(baseline!.AsSpan(i * 16, 16));
            Presets[i].State = pending ? "Enviando…" : dirty ? "Aplicado ao aparelho · não gravado" : "Sem alterações locais";
        }
        if (!Busy) RecordBankOperation(bankOperationDescription);
        Refresh();
    }
    private void Fault(Exception ex)
    {
        RecordOperation("Falha: " + ex.Message);
        if (DelayPreviewActive)
        {
            delayStopped = true; delayPending = false;
            Delay.Message = "Prévia interrompida. Restauração do delay não confirmada; snapshot preservado nesta sessão.";
        }
        if (ReverbPreviewActive)
        {
            reverbStopped = true; reverbPending = false;
            Reverb.Message = "Prévia interrompida. Restauração do reverb não confirmada; snapshot preservado nesta sessão.";
        }
        if (ChorusPreviewActive)
        {
            chorusStopped = true; chorusPending = false;
            Chorus.Message = "Prévia interrompida. Restauração do chorus não confirmada; snapshot preservado nesta sessão.";
        }
        if (PhaserPreviewActive)
        {
            phaserStopped = true; phaserPending = false;
            Phaser.Message = "Prévia interrompida. Restauração do phaser não confirmada; snapshot preservado nesta sessão.";
        }
        Ir.PreviewConfirmed = false;
        blocked = true;
        if (queue != null) { queue.Failed -= Fault; queue.Stop(); queue = null; }
        inventory.Clear(); RebuildCabinets();
        foreach (var preset in Presets) preset.State = "Erro · edição preservada";
        Status = ex.Message + (originalRam == null ? " Releia ou reconecte. Você pode exportar sua edição." : " Restauração do IR não confirmada; snapshot preservado.");
        ErrorMessage = Status;
        if (session?.IsFaulted == true)
        {
            session.Dispose(); session = null; Connection.Identity = "Conexão perdida";
            Connection.Input = null; Connection.Output = null;
            NotifyUser("Pedal desconectado", "A comunicação com o pedal foi perdida. Verifique o cabo USB e a alimentação. Sua edição local foi preservada.", Wpf.Ui.Controls.ControlAppearance.Danger);
        }
        else if (!automaticConnectionAttempt) NotifyFailure(Status);
        Refresh();
    }
    public async Task DisconnectAsync()
    {
        automaticConnectionPaused = true;
        await EndDelayPreviewAsync(); await EndReverbPreviewAsync(); await EndChorusPreviewAsync(); await EndPhaserPreviewAsync();
        await RestoreAmpCore();
        await RestorePreviewAsync(); queue?.Stop(); session?.Dispose(); session = null;
        inventory.Clear(); RebuildCabinets();
        Connection.Identity = "Desconectado"; Status = "Desconectado. Sua edição local continua disponível para exportação."; Refresh();
        NotifyUser("Pedal desconectado", Status, Wpf.Ui.Controls.ControlAppearance.Danger);
    }

    private async Task SaveAsync()
    {
        BusyDetail = "Conferindo presets antes da gravação…";
        var current = await session!.ReadBank();
        if (!current.SequenceEqual(confirmed!)) throw new InvalidOperationException("O banco mudou no aparelho. Releia antes de salvar.");
        await BackupBank(baseline!, "antes-de-salvar");
        await BackupBank(current, "banco-reconciliado");
        BusyDetail = "Gravando e verificando presets A/B/C…";
        await session!.SaveBank(confirmed!, edited!);
        baseline = edited!.ToArray(); confirmed = edited!.ToArray();
        foreach (var p in Presets) p.State = "Gravado · verificado";
        Status = "Presets A/B/C salvos e confirmados por leitura."; Completed(Status); Refresh();
    }
    private static string BankJson(byte[] bank) => JsonSerializer.Serialize(new { format = "cubecontrol-bank-v1", device = "SINCO-CubeBaby", capturedAt = DateTimeOffset.Now, bankHex = Convert.ToHexString(bank) }, new JsonSerializerOptions { WriteIndented = true });
    private byte[]? bankRecovery;
    private readonly Dictionary<(string Identity, int Slot), byte[]> irRecovery = [];
    private Task BackupBank(byte[] bank, string reason)
    {
        CubeBabyProfile.ValidateBank(bank);
        bankRecovery = bank.ToArray();
        return Task.CompletedTask;
    }
    private async Task ExportAsync()
    {
        var dlg = new SaveFileDialog { Filter = LanguageService.T("Banco CubeControl|*.json"), FileName = "CubeBaby-ABC.json" };
        if (dlg.ShowDialog() == true) { await File.WriteAllTextAsync(dlg.FileName, BankJson(edited!)); Status = "Edição local exportada."; Completed(Status); }
    }
    private async Task ImportAsync()
    {
        var dlg = new OpenFileDialog { Filter = LanguageService.T("Banco CubeControl|*.json") }; if (dlg.ShowDialog() != true) return;
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(dlg.FileName)); var root = json.RootElement;
        if (root.GetProperty("format").GetString() != "cubecontrol-bank-v1" || root.GetProperty("device").GetString() != "SINCO-CubeBaby") throw new FormatException("Arquivo de banco incompatível.");
        byte[] imported = Convert.FromHexString(root.GetProperty("bankHex").GetString()!); CubeBabyProfile.ValidateBank(imported);
        string details = string.Join("\n", Enumerable.Range(0, 3).Select(i => $"{(char)('A' + i)}: {CubeBabyProfile.Parameters[0].Options![imported[i * 16]]}; volume {imported[i * 16 + 5]}"));
        if (!await Ask($"Prévia de {Path.GetFileName(dlg.FileName)}\n\n{details}\n\nCarregar estes parâmetros no pedal? O salvamento permanece separado.")) return;
        await SetBankLive(imported);
        bankOperationGroup = new object(); RecordBankOperation("Banco importado e aplicado na RAM.");
        Status = "Banco importado e aplicado. Use Gravar A/B/C para salvar no aparelho.";
        NotifyUser("Gravação pendente", Status, Wpf.Ui.Controls.ControlAppearance.Caution);
    }
    private async Task SetBankLive(byte[] target)
    {
        history.Clear();
        await BackupBank(confirmed!, "antes-de-carregar");
        for (int s = 0; s < 3; s++) foreach (var p in CubeBabyProfile.Parameters)
        {
            int index = s * 16 + p.Offset;
            edited![index] = target[index]; Presets[s].Parameters[p.Offset].Load(target[index]);
            if (confirmed![index] == target[index]) continue;
            confirmed = await session!.WriteLiveChecked(s, p.Offset, target[index], confirmed);
        }
        UpdateStates();
    }

    private async Task ChooseWavAsync()
    {
        var dlg = new OpenFileDialog { Filter = LanguageService.T("Impulse response WAV|*.wav") }; if (dlg.ShowDialog() != true) return;
        var info = new FileInfo(dlg.FileName); if (info.Length > 32 * 1024 * 1024) throw new FormatException("WAV excede 32 MB.");
        await LoadWavAndPreviewAsync(dlg.FileName, IrAudio.Decode(await File.ReadAllBytesAsync(dlg.FileName)));
    }
    internal async Task LoadWavAndPreviewAsync(string path, IrAudio audio, string? collection = null, Func<bool>? stillSelected = null)
    {
        if (collection == null) IrLibrary.ClearSelection();
        Ir.Load(path, audio, collection);
        if (Connected && !blocked) await PreviewCoreAsync(stillSelected);
        else Status = "Arquivo preparado. Conecte o pedal e solte o microfone para ouvir.";
    }
    private int transferGeneration;
    private IProgress<TransferProgress> Progress()
    {
        int generation = ++transferGeneration;
        Ir.HasMeasuredProgress = false; Ir.Progress = 0; Ir.TransferActive = true;
        return new TransferProgressThrottle(value => { if (generation == transferGeneration && Busy && Ir.TransferActive) Ir.Report(value); });
    }
    private Task PreviewAsync() => PreviewCoreAsync(null);
    private async Task PreviewCoreAsync(Func<bool>? stillSelected)
    {
        var auditionVersion = await IrAudition.SuspendForTransfer();
        bool auditionSuccess = false;
        try
        {
            if (stillSelected != null && !stillSelected()) return;
            BusyDetail = "Preparando prévia de IR…";
            if (originalRam == null)
            {
                var snapshot = await session!.ReadIrRam();
                if (stillSelected != null && !stillSelected()) return;
                if (snapshot.Payload.Length != 2052) throw new FormatException("Snapshot RAM inválido.");
                snapshot = new(snapshot.Flag, snapshot.Payload.ToArray());
                originalRam = snapshot; ramIdentity = session.IdentityHex;
            }
            if (ramIdentity != session!.IdentityHex) throw new InvalidOperationException("O snapshot de prévia pertence a outra identidade de aparelho.");
            Ir.PreviewActive = true; // Set before mutation so interrupted transfers retain recovery state.
            Ir.PreviewConfirmed = false;
            var audio = Ir.Audio!;
            string sourcePath = Ir.SourcePath, collection = Ir.SourceCollection;
            int distance = Ir.Distance;
            var bankContext = confirmed?.ToArray();
            object group = OperationGroup("IR");
            await session!.ApplyIrRam(new(1, audio.LivePayload(distance)), Progress());
            Ir.PreviewConfirmed = true;
            Ir.State = "Prévia confirmada em RAM. Nenhum slot substituído."; Status = Ir.State; Refresh();
            RecordOperation("Prévia de IR: " + Ir.File, async () =>
            {
                bool done = false;
                await Execute(async () =>
                {
                    IrLibrary.ClearSelection(); Ir.Load(sourcePath, audio, collection.Length == 0 ? null : collection); Ir.Distance = distance;
                    await PreviewAsync(); done = true;
                });
                return done;
            }, () => CanOperate && bankContext != null && confirmed?.SequenceEqual(bankContext) == true, group);
            auditionSuccess = true;
        }
        finally { await IrAudition.CompleteTransfer(auditionVersion, auditionSuccess); }
    }
    public async Task EndPreviewAsync()
    {
        await RestorePreviewAsync();
        // Clear the picker only after restoration succeeds; failed previews stay recoverable.
        irSelectionGeneration++;
        IrLibrary.ClearSelection();
        Ir.ClearLoadedFile();
        Refresh();
    }
    private async Task RestorePreviewAsync()
    {
        if (originalRam == null) return;
        var auditionVersion = await IrAudition.SuspendForTransfer();
        bool restored = false;
        try
        {
            BusyDetail = "Restaurando IR anterior…";
            if (session == null) throw new IOException("Reconecte para restaurar o IR anterior. O snapshot foi preservado.");
            if (ramIdentity != session.IdentityHex) throw new InvalidOperationException("Conecte o aparelho correspondente ao snapshot de prévia.");
            Ir.PreviewConfirmed = false;
            await session.ApplyIrRam(originalRam, Progress());
            originalRam = null; ramIdentity = null;
            Ir.PreviewActive = false; Ir.State = "IR anterior restaurado e conferido."; Status = Ir.State;
            restored = true; Refresh();
        }
        finally { await IrAudition.CompleteTransfer(auditionVersion, restored); }
    }
    private Task BackupIr(int slot, byte[] bytes)
    {
        if (slot is < 0 or > 7 || bytes.Length != 4096) throw new ArgumentException("Snapshot IR inválido.");
        irRecovery[(session!.IdentityHex, slot)] = bytes.ToArray();
        return Task.CompletedTask;
    }
    private async Task ReadSlotAsync()
    {
        BusyDetail = "Lendo o slot de IR…";
        var data = await session!.ReadIrSlot(Ir.Slot, Progress());
        await BackupIr(Ir.Slot, data);
        UpdateCabinet(Ir.Slot, data);
        Ir.SlotInfo = "Conteúdo lido do pedal.";
        var picker = new SaveFileDialog { Filter = "Backup de IR CubeControl|*.json", FileName = $"IR-{Ir.Slot + 1}.json" };
        if (picker.ShowDialog() == true)
        {
            await IrBackup.Export(picker.FileName, session.Identity, session.IdentityHex, Ir.Slot, data);
            Status = Ir.State = "Backup de IR exportado.";
        }
        else Status = Ir.State = "Slot lido nesta sessão. Exportação cancelada.";
        Completed(Status);
    }
    private bool confirmingIrWrite;
    public Func<string, Task<(bool Confirmed, string? ImagePath)>>? ConfirmIrWrite { get; set; }
    private async Task ConfirmWriteIrAsync()
    {
        if (!CanOperate || Ir.Audio == null || confirmingIrWrite) return;
        confirmingIrWrite = true;
        irSelectionGeneration++;
        int slot = Ir.Slot, distance = Ir.Distance;
        var audio = Ir.Audio;
        string file = Ir.File;
        try
        {
            string message = $"Gravar {file}?\nDestino: {Ir.Slots[slot]}\n\nDistância: {distance}%\nO conteúdo atual será substituído.\nO conteúdo anterior será mantido em memória durante esta sessão.";
            var choice = ConfirmIrWrite != null ? await ConfirmIrWrite(message) : (Confirmed: await Ask(message), ImagePath: (string?)null);
            if (!choice.Confirmed || !CanOperate) return;
            await Execute(() => WriteIrAsync(slot, audio, distance, file, choice.ImagePath));
        }
        catch (Exception ex) { Status = ErrorMessage = ex.Message; }
        finally { confirmingIrWrite = false; Refresh(); }
    }
    private async Task WriteIrAsync(int slot, IrAudio audio, int distance, string file, string? imagePath, bool announceCompletion = true)
    {
        BusyDetail = "Preservando o slot de IR em memória…";
        var data = audio.Sector(distance);
        var current = await session!.ReadIrSlot(slot, Progress()); await BackupIr(slot, current);
        await RestorePreviewAsync();
        BusyDetail = "Gravando e verificando IR…";
        await session.ReplaceIr(slot, current, data, async bytes => { await BackupIr(slot, bytes); }, Progress());
        string hash = CabinetInventory.Hash(data);
        preferences.IrNames[hash] = file;
        resolvedIrNames[hash] = file;
        if (imagePath != null)
        {
            try
            {
                string imageName = hash + ".png";
                Directory.CreateDirectory(Path.Combine(DataDirectory, "Images"));
                File.Copy(imagePath, Path.Combine(DataDirectory, "Images", imageName), true);
                preferences.Images[hash] = imageName;
                SaveImages();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { ErrorMessage = "IR gravado, mas não foi possível salvar a imagem: " + ex.Message; NotifyFailure(ErrorMessage); }
        }
        UpdateCabinet(slot, data);
        Ir.SlotInfo = "Nome local confirmado pelo conteúdo: " + file;
        Status = Ir.State = $"IR gravado e verificado no slot {slot + 1}. Selecione outro cabinet e volte a este slot para recarregá-lo.";
        if (announceCompletion) Completed("IR gravado e verificado.");
    }
    private async Task RestoreIrAsync()
    {
        var dlg = new OpenFileDialog { Filter = LanguageService.T("Backup de IR CubeControl|*.json") }; if (dlg.ShowDialog() != true) return;
        BusyDetail = "Preservando o slot de IR em memória…";
        var (info, data) = await IrBackup.Load(dlg.FileName);
        var current = await session!.ReadIrSlot(info.Slot, Progress()); await BackupIr(info.Slot, current);
        if (!await Ask($"Restaurar o backup de {info.CapturedAt:g} no slot {info.Slot + 1}?\nO conteúdo atual está preservado em memória nesta sessão.")) return;
        await RestorePreviewAsync();
        BusyDetail = "Gravando e verificando IR…";
        await session.ReplaceIr(info.Slot, current, data, async bytes => { await BackupIr(info.Slot, bytes); }, Progress());
        UpdateCabinet(info.Slot, data);
        Ir.Slot = info.Slot; Ir.SlotInfo = "Backup restaurado · conteúdo confirmado por checksum."; Status = Ir.State = "Slot restaurado e verificado.";
        Completed(Status);
    }
    private void LoadFactory(string path)
    {
        try { factory = FactoryResources.Load(path); }
        catch (Exception) { factory = null; }
        originalPacks = IrPackCatalog.LoadAvailable(Path.Combine(Path.GetDirectoryName(path)!, "bin"));
        ReloadIrPacks();
        inventory.SetBfiReference(factory);
        inventory.Clear(); RebuildCabinets();
        Refresh();
    }
    private async Task ResetPresetsAsync()
    {
        await RestorePreviewAsync(); var current = await session!.ReadBank(); await BackupBank(current, "antes-reset-fabrica");
        if (!await Ask("Restaurar os três presets A/B/C de fábrica?\nOs presets atuais foram preservados em memória nesta sessão. Os IRs não serão alterados.")) return;
        if (!current.SequenceEqual(confirmed!)) throw new InvalidOperationException("Banco mudou. Releia antes do reset.");
        await SetBankLive(factory!.Presets); await SaveAsync(); Status = "Presets de fábrica aplicados, salvos e conferidos. IRs preservados.";
    }
    private async Task ApplyIrPackAsync()
    {
        IrPack pack;
        if (packEditor != null && ReferenceEquals(packEditor.Current, SelectedIrPack))
            pack = PackStore.Resolve(packEditor.ApplicationDraft());
        else
        {
            pack = SelectedIrPack ?? throw new InvalidOperationException("Selecione um pack de IRs.");
            if (pack.Definition != null) pack = PackStore.Resolve(pack.Definition);
        }
        await ApplyPreparedIrPackAsync(pack);
    }
    private async Task ApplyPreparedIrPackAsync(IrPack pack, bool restoration = false)
    {
        // Legacy selection hooks are retained for callers/tests; the new review supports per-slot repair states.
        if (ChoosePackIrSlots != null && pack.Problem.Length > 0) throw new FormatException("Pendências: " + pack.Problem);
        var originals = new byte[8][];
        for (int i = 0; i < 8; i++)
        {
            // Empty draft slots cannot be selected by the review; do not read or back them up.
            if (!pack.HasSlot(i)) { originals[i] = []; continue; }
            BusyDetail = Status = $"Leitura do IR {i + 1}/8…";
            originals[i] = await session!.ReadIrSlot(i, Progress());
            await BackupIr(i, originals[i]); UpdateCabinet(i, originals[i]);
        }
        bool[] Differences(IrPack value) => Enumerable.Range(0, 8).Select(i => value.HasSlot(i) && value.Sectors[i].Length == 4096 && !originals[i].SequenceEqual(value.Sectors[i])).ToArray();
        var different = Differences(pack);
        int[]? selection;
        if (restoration)
        {
            var panel = new System.Windows.Controls.StackPanel();
            var boxes = Enumerable.Range(0, 8).Select(i => new System.Windows.Controls.CheckBox {
                Content = $"Slot {i + 1} · {LanguageService.T(pack.SlotName(i))}", IsChecked = different[i], IsEnabled = different[i], Margin = new Thickness(0,4,0,4) }).ToArray();
            foreach (var box in boxes) panel.Children.Add(box);
            var dialog = new Controls.PackDialog("Restaurar IRs originais", panel) { IsPrimaryEnabled = boxes.Any(b => b.IsChecked == true) };
            foreach (var box in boxes) { box.Checked += (_,_) => dialog.IsPrimaryEnabled = boxes.Any(b => b.IsChecked == true); box.Unchecked += (_,_) => dialog.IsPrimaryEnabled = boxes.Any(b => b.IsChecked == true); }
            selection = await dialog.ShowAsync() == Controls.PackDialogChoice.Primary ? Enumerable.Range(0,8).Where(i => boxes[i].IsChecked == true).ToArray() : null;
        }
        else if (ChoosePackIrSlots != null)
        {
            if (!different.Any(v => v)) { Status = Ir.State = "Os slots preenchidos já correspondem ao pack selecionado. Nenhuma gravação necessária."; Completed(Status); return; }
            selection = await ChoosePackIrSlots(different);
        }
        else
        {
            if (pack.Definition == null) throw new InvalidOperationException("Use Restaurar IRs originais para packs binários.");
            // Optional host hook; the app edits volumes directly on the pack cards.
            if (ReviewPack != null)
            {
                var review = new IrPackReviewViewModel(pack.Definition, PackStore, originals, PackEditor);
                if (!await ReviewPack(review)) { Status = "Aplicação de IRs cancelada."; return; }
                if (!review.CanContinue) throw new InvalidOperationException("Selecione ao menos um IR disponível.");
                pack = PackStore.Resolve(review.SelectedDefinition());
            }
            if (pack.Problem.Length > 0) throw new FormatException("Pendências: " + pack.Problem);
            different = Differences(pack); selection = pack.Definition!.Entries.Select(e => e.Slot).ToArray();
        }
        if (selection == null || selection.Length == 0) { Status = "Aplicação de IRs cancelada."; return; }
        if (selection.Any(i => i < 0 || i >= 8 || !pack.HasSlot(i) || pack.Sectors[i].Length != 4096)) throw new InvalidOperationException("Seleção de IRs inválida.");
        var selected = selection.Distinct().Order().Where(i => different[i]).ToArray();
        if (selected.Length == 0) { Status = Ir.State = "Os slots preenchidos já correspondem ao pack selecionado. Nenhuma gravação necessária."; Completed(Status); return; }
        if (!await (restoration || ChoosePackIrSlots != null || ReviewPack != null ? Ask : ConfirmPackApply)($"Aplicar {pack.Name} nos slots: {string.Join(", ", selected.Select(i => i + 1))}?\nOs IRs atuais foram preservados em memória nesta sessão. Somente os slots selecionados serão substituídos.")) return;
        if (!await ConfirmAdvancedPreviewChangeAsync()) return;
        await RestorePreviewAsync();
        var completedSlots = new List<int>();
        try
        {
            foreach (int i in selected)
            {
                int slot = i; BusyDetail = Status = $"Aplicando {pack.Name} no slot {i + 1}…";
                await session!.ReplaceIr(i, originals[i], pack.Sectors[i], async bytes => { await BackupIr(slot, bytes); }, Progress());
                completedSlots.Add(i + 1);
                if (pack.Definition != null)
                {
                    string hash = CabinetInventory.Hash(pack.Sectors[i]);
                    preferences.IrNames[hash] = pack.SlotName(i);
                    resolvedIrNames[hash] = pack.SlotName(i);
                    SavePackIrNames();
                }
                UpdateCabinet(i, pack.Sectors[i]);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Aplicação interrompida. Slots concluídos: {string.Join(", ", completedSlots)}. {ex.Message}", ex);
        }
        Ir.SlotInfo = $"{pack.Name} aplicado e conferido nos slots: {string.Join(", ", selected.Select(i => i + 1))}."; Status = Ir.State = Ir.SlotInfo;
        Completed(Status);
    }
    public Func<string, Task<bool>> ConfirmPackApply { get; set; } = async message =>
    {
        var dialog = new Controls.PackDialog(LanguageService.T("Aplicar"),
            new System.Windows.Controls.TextBlock { Text = LanguageService.T(message), TextWrapping = TextWrapping.Wrap }, primary: "OK", close: "Sair");
        return await dialog.ShowAsync() == Controls.PackDialogChoice.Primary;
    };
    public Func<string, Task<bool>> Ask { get; set; } = async message =>
    {
        var dialog = new Wpf.Ui.Controls.MessageBox { PrimaryButtonIcon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Checkmark24), CloseButtonIcon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Dismiss24), Title = "CubeControl", Content = LanguageService.T(message), PrimaryButtonText = LanguageService.T("Confirmar"), CloseButtonText = LanguageService.T("Cancelar"), Owner = Application.Current.MainWindow };
        return await dialog.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary;
    };
    public async Task Flush() { if (queue != null) await queue.Flush(); }
    public bool HasAdvancedPreview(int stage) => stage switch
    { 1 => ChorusPreviewActive, 2 => PhaserPreviewActive, 3 => DelayPreviewActive, 4 => ReverbPreviewActive, _ => false };
    private bool HasAdvancedPreviews => Enumerable.Range(1, 4).Any(HasAdvancedPreview);
    private bool IsAdvancedEffectAudible(int stage)
    {
        int? slot = stage == 3 ? delayRecovery?.PhysicalState[0] : reverbRecovery?.PhysicalState[0];
        if (slot == null || confirmed == null) return false;
        return stage == 3 ? confirmed[slot.Value * 16 + 11] == 1
            : confirmed[slot.Value * 16 + 10] == 1 && confirmed[slot.Value * 16 + 3] > 0;
    }
    private bool IsModulationPreviewAudible(int stage)
    {
        int? slot = stage == 1 ? chorusRecovery?.PhysicalState[0] : phaserRecovery?.PhysicalState[0];
        if (slot == null || confirmed == null || confirmed[slot.Value * 16 + 11] != 1) return false;
        int mod = confirmed[slot.Value * 16 + 8];
        return stage == 1 ? mod <= 6 : mod >= 9;
    }
    public bool CanListenChorus => Connected && !Busy && ChorusPreviewActive && !chorusStopped && !IsModulationPreviewAudible(1);
    public bool CanListenPhaser => Connected && !Busy && PhaserPreviewActive && !phaserStopped && !IsModulationPreviewAudible(2);
    public async Task ListenAdvancedModulationAsync(int stage)
    {
        await advancedLifecycle.WaitAsync();
        try
        {
            await WaitForAdvancedEffectOperationAsync();
            if (stage == 1 ? !CanListenChorus : stage != 2 || !CanListenPhaser) return;
            await ExecuteAdvancedEffect(stage, async () =>
            {
                await PrepareAdvancedEffectAsync(stage);
                if (stage == 1)
                {
                    var original = chorusRecovery!;
                    var target = Chorus.Parameters;
                    await session!.BeginChorusPreview(SelectedIndex, next => KeepChorusRecovery(next with { OriginalDsp = original.OriginalDsp }));
                    await session.ApplyChorusPreview(chorusRecovery!, target, KeepChorusRecovery);
                    Chorus.Message = "Chorus em prévia na RAM. Os ajustes do phaser foram preservados.";
                }
                else
                {
                    var original = phaserRecovery!;
                    var target = Phaser.Parameters;
                    await session!.BeginPhaserPreview(SelectedIndex, next => KeepPhaserRecovery(next with { OriginalDsp = original.OriginalDsp }));
                    await session.ApplyPhaserPreview(phaserRecovery!, target, KeepPhaserRecovery);
                    Phaser.Message = "Phaser em prévia na RAM. Os ajustes do chorus foram preservados.";
                }
            });
        }
        finally { advancedLifecycle.Release(); }
    }

    private async Task<bool> PrepareAdvancedEffectAsync(int stage)
    {
        if (session == null || confirmed == null || edited == null) throw new IOException("Conecte o pedal para iniciar a prévia.");
        var snapshot = await session.ReadEffectPreparationState(stage);
        if (snapshot.PhysicalState[1] != 1 || snapshot.PhysicalState[0] > 2)
            throw new IOException("Selecione o modo PRESET no pedal. O protocolo não oferece troca remota desse modo.");
        if (!snapshot.Bank.SequenceEqual(confirmed)) throw new IOException("O banco mudou no pedal. Releia os presets antes de iniciar a prévia.");
        foreach (var context in new[]
        {
            (chorusRecovery?.Identity, chorusRecovery?.PhysicalState), (phaserRecovery?.Identity, phaserRecovery?.PhysicalState),
            (delayRecovery?.Identity, delayRecovery?.PhysicalState), (reverbRecovery?.Identity, reverbRecovery?.PhysicalState)
        })
            if (context.Item2 != null && (context.Item1 != session.IdentityHex || !context.Item2.SequenceEqual(snapshot.PhysicalState)))
                throw new IOException("O aparelho ou preset físico mudou. Restaure as prévias pendentes antes de iniciar outra edição.");
        int slot = snapshot.PhysicalState[0];
        SetSelectedPreset(slot);
        int mod = snapshot.Bank[slot * 16 + 8];
        int desiredMod = stage == 1 ? chorusRecovery?.Bank[slot * 16 + 8] ?? (mod <= 6 ? mod : 3)
            : phaserRecovery?.Bank[slot * 16 + 8] ?? (mod >= 9 ? mod : 12);
        var requirements = stage switch
        {
            1 or 2 => new[] { (Offset: 8, Value: desiredMod), (Offset: 11, Value: 1) },
            3 => new[] { (Offset: 7, Value: snapshot.Bank[slot * 16 + 7] == 0 ? 32 : (int)snapshot.Bank[slot * 16 + 7]), (Offset: 11, Value: 1) },
            _ => new[] { (Offset: 3, Value: snapshot.Bank[slot * 16 + 3] == 0 ? 8 : (int)snapshot.Bank[slot * 16 + 3]), (Offset: 10, Value: 1) }
        };
        bool changed = false;
        var changedOffsets = new HashSet<int>();
        foreach (var (offset, value) in requirements)
        {
            if (confirmed[slot * 16 + offset] == value) continue;
            confirmed = await session.WriteLiveChecked(slot, offset, value, confirmed);
            edited[slot * 16 + offset] = (byte)value;
            Presets[slot].Parameters[offset].Load(value);
            changed = true;
            changedOffsets.Add(offset);
        }
        if (changed)
        {
            await Task.Delay(200);
            await SynchronizeAdvancedEffectsAsync(changedOffsets, stage);
        }
        UpdateStates();
        return changed;
    }
    private void FaultAdvancedPreview(Exception ex, int? stage = null)
    {
        if (session?.IsFaulted == true) { Fault(ex); return; }
        if ((stage == null || stage == 1) && ChorusPreviewActive) { chorusStopped = true; chorusPending = false; chorusMonitorStop?.Cancel(); Chorus.Message = "Prévia interrompida. Use Restaurar chorus; snapshot preservado nesta sessão."; }
        if ((stage == null || stage == 2) && PhaserPreviewActive) { phaserStopped = true; phaserPending = false; phaserMonitorStop?.Cancel(); Phaser.Message = "Prévia interrompida. Use Restaurar phaser; snapshot preservado nesta sessão."; }
        if ((stage == null || stage == 3) && DelayPreviewActive) { delayStopped = true; delayPending = false; delayMonitorStop?.Cancel(); Delay.Message = "Prévia interrompida. Use Restaurar delay; snapshot preservado nesta sessão."; }
        if ((stage == null || stage == 4) && ReverbPreviewActive) { reverbStopped = true; reverbPending = false; reverbMonitorStop?.Cancel(); Reverb.Message = "Prévia interrompida. Use Restaurar reverb; snapshot preservado nesta sessão."; }
        Status = ErrorMessage = "Operação do efeito não concluída. Restauração não confirmada: " + ex.Message;
        foreach (int affected in Enumerable.Range(1, 4).Where(i => stage == null || stage == i)) SetAdvancedError(affected, Status);
        RecordOperation(Status); NotifyFailure(Status); Refresh();
    }
    private async Task<bool> ConfirmAdvancedPreviewChangeAsync()
    {
        if (!HasAdvancedPreviews) return true;
        if (!await Ask("Esta ação encerra a prévia dos efeitos e restaura os parâmetros anteriores. A edição continuará disponível no card. Continuar?")) return false;
        try
        {
            await EndDelayPreviewAsync(); await EndReverbPreviewAsync(); await EndChorusPreviewAsync(); await EndPhaserPreviewAsync();
            return true;
        }
        catch (Exception ex) { FaultAdvancedPreview(ex); return false; }
    }
    public bool AdvancedPreviewHasChanges(int stage) => stage switch
    {
        1 => chorusRecovery != null && Chorus.Parameters != ChorusParameters.Read(chorusRecovery.OriginalDsp),
        2 => phaserRecovery != null && Phaser.Parameters != PhaserParameters.Read(phaserRecovery.OriginalDsp),
        3 => delayRecovery != null && Delay.Parameters != DelayParameters.Read(delayRecovery.OriginalDsp),
        4 => reverbRecovery != null && Reverb.Parameters != ReverbParameters.Read(reverbRecovery.OriginalDsp),
        _ => false
    };
    public async Task<bool> CloseAdvancedPreviewAsync(int stage)
    {
        await advancedLifecycle.WaitAsync();
        try
        {
            Task? sending = stage switch { 1 => chorusSendTask, 2 => phaserSendTask, 3 => delaySendTask, 4 => reverbSendTask, _ => null };
            if (sending != null) await sending;
            while (Busy && operationCompletion != null) await operationCompletion.Task;
            if (!HasAdvancedPreview(stage)) return true;
            bool restored = false;
            await ExecuteAdvancedEffect(stage, async () =>
            {
                switch (stage)
                {
                    case 1: await EndChorusPreviewAsync(); break;
                    case 2: await EndPhaserPreviewAsync(); break;
                    case 3: await EndDelayPreviewAsync(); break;
                    case 4: await EndReverbPreviewAsync(); break;
                }
                restored = true;
            });
            return restored;
        }
        catch (Exception ex) { FaultAdvancedPreview(ex, stage); return false; }
        finally { advancedLifecycle.Release(); }
    }
    public void Dispose() { IrAudition.Dispose(); delayMonitorStop?.Cancel(); Delay.Edited -= DelayEdited; reverbMonitorStop?.Cancel(); Reverb.Edited -= ReverbEdited; chorusMonitorStop?.Cancel(); Chorus.Edited -= ChorusEdited; phaserMonitorStop?.Cancel(); Phaser.Edited -= PhaserEdited; Amp.Edited -= AmpEdited; Amp.PropertyChanged -= AmpPresentationChanged; StopAutomaticConnection(); irSelectionGeneration++; queue?.Stop(); session?.Dispose(); session = null; }
}
