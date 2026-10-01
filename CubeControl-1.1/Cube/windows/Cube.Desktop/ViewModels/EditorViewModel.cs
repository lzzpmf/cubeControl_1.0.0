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
    private int writeCommandCount;
    public int WriteCommandCount => writeCommandCount;
    private string status = "Conecte seu Cube Baby por USB para começar.", log = "";
    public string DataDirectory { get; }
    private string Backups => Path.Combine(DataDirectory, "Backups");
    private string PendingPreview => Path.Combine(DataDirectory, "pending-ir-preview.json");
    public ConnectionViewModel Connection { get; } = new();
    public IrViewModel Ir { get; } = new();
    public PresetViewModel[] Presets { get; }
    public bool Busy { get => busy; private set { Set(ref busy, value); Refresh(); } }
    public bool Connected => session != null;
    public bool CanEdit => Connected && !Busy && !blocked && edited != null && !Ir.PreviewActive && !AmpPreviewActive;
    public bool CanOperate => Connected && !Busy && !blocked;
    public bool CanConnect => !Connected && !Busy;
    public bool HasBank => edited != null;
    public bool Dirty => baseline != null && edited != null && !baseline.SequenceEqual(edited);
    public string Status { get => status; private set => Set(ref status, value); }
    public string Log { get => log; private set => Set(ref log, value); }
    public int SelectedIndex { get => selected; set { if (value is >= 0 and <= 2 && Set(ref selected, value)) { Notify(nameof(Current)); RefreshEditor(); } } }
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
    public ICommand ResetIrs { get; }

    public EditorViewModel(string? dataDirectory = null)
    {
        DataDirectory = dataDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CubeControl");
#if CUBE_SIMULATION
        DataDirectory = dataDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CubeControl-Simulation");
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
        IrLibrary = new IrLibraryViewModel(() => !Busy);
        IrLibrary.SelectionChanged += SelectLibraryIr;
        InitializeVisual();
        Amp.Edited += AmpEdited;
        RefreshPorts = Command(async () => { SearchingPorts = true;
        try { await Connection.Refresh(); }
        finally { SearchingPorts = false; } Status = Connection.Input == null ? "USB-Midi não encontrado. Verifique o cabo de dados." : "USB-Midi encontrado. Conecte para ler os presets."; }, () => CanConnect);
        Connect = Command(ConnectAsync, () => CanConnect);
        Disconnect = Command(DisconnectAsync, () => Connected && !Busy);
        Reload = Command(async () => { if (!Dirty || await Ask("Reler o banco atual e descartar as alterações locais?")) { await RestorePreviewAsync(); await LoadBank(); Completed("Presets atualizados com a leitura do pedal."); } }, () => Connected && !Busy);
        Save = new UiCommand(async () => { if (CanEdit && Dirty) await Execute(SaveAsync); }, () => CanEdit && Dirty);
        Export = Command(ExportAsync, () => HasBank && !Busy);
        Import = Command(ImportAsync, () => CanEdit);
        ChooseWav = Command(ChooseWavAsync, () => !Busy);
        PreviewIr = Command(PreviewAsync, () => CanOperate && Ir.Audio != null);
        EndPreview = Command(EndPreviewAsync, () => Connected && !Busy && originalRam != null);
        WriteIr = new UiCommand(async () => await ConfirmWriteIrAsync(), () => CanOperate && Ir.Audio != null && !confirmingIrWrite);
        ReadSlot = Command(ReadSlotAsync, () => CanOperate);
        Ir.PropertyChanged += (_, _) => RefreshPresentation();
        RestoreIr = Command(RestoreIrAsync, () => CanOperate);
        ResetPresets = Command(ResetPresetsAsync, () => CanOperate && factory != null);
        ResetIrs = Command(ResetIrsAsync, () => CanOperate && factory != null);
        LoadFactory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CubeSuite", "CubeSuite.exe"));
    }

    private UiCommand Command(Func<Task> action, Func<bool> can) => new(async () => await Execute(action), can);
    public async Task Execute(Func<Task> action)
    {
        if (Busy) return;
        irSelectionGeneration++;
        transferGeneration++;
        if (!blocked) ErrorMessage = "";
        BusyDetail = "Aguarde a conclusão da operação.";
        InventoryReadCount = 0; ReadingInventory = false;
        Busy = true;
        try { if (queue != null && !blocked) await queue.Flush(); await action(); }
        catch (Exception ex)
        {
            Status = ex.Message; ErrorMessage = ex.Message; AddLog("ERRO " + ex.Message);
            if (session != null && (session.IsFaulted || ex is InvalidOperationException or IOException)) Fault(ex);
            else if (!automaticConnectionAttempt) NotifyFailure(ex.Message);
        }
        finally { Ir.TransferActive = false; ReadingInventory = false; Busy = false; }
    }

    private void Refresh()
    {
        Notify(nameof(CanUseAmp)); Notify(nameof(CanAdjustAmp)); Notify(nameof(CanOpenGearGallery)); Notify(nameof(CanEditAmpLocally)); Notify(nameof(AmpPreviewActive));
        Notify(nameof(CanChooseIr));
        Notify(nameof(Connected)); Notify(nameof(CanConnect)); Notify(nameof(CanEdit)); Notify(nameof(CanOperate)); Notify(nameof(HasBank)); Notify(nameof(Dirty));
        Notify(nameof(CanMatch)); RefreshEditor();
        RefreshPresentation();
        CommandManager.InvalidateRequerySuggested();
    }
    private void AddLog(string value)
    {
        if (!Application.Current.Dispatcher.CheckAccess()) { Application.Current.Dispatcher.BeginInvoke(() => AddLog(value)); return; }
        if (value.StartsWith("TX ") && UsbSysEx.Decode(Convert.FromHexString(value[3..])).Command is not (0x11 or 0x23)) Interlocked.Increment(ref writeCommandCount);
        Log = (Log.Length > 30000 ? Log[^18000..] : Log) + $"{DateTime.Now:HH:mm:ss.fff} {value}\n";
    }
    public async Task Initialize() => await Execute(async () =>
    {
        BusyDetail = "Procurando portas USB-MIDI…";
        SearchingPorts = true;
        try { await Connection.Refresh(); }
        finally { SearchingPorts = false; }
        if (File.Exists(PendingPreview))
        {
            (ramIdentity, originalRam) = await IrRamBackup.Load(PendingPreview);
            Ir.PreviewActive = true;
            Status = "Prévia anterior sem restauração confirmada. Conecte o mesmo aparelho e use Encerrar prévia.";
            Ir.State = Status;
        }
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
        if (Connection.Input == null || Connection.Output == null) throw new InvalidOperationException("USB-Midi não encontrado. Verifique o cabo de dados.");
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
                : "Pedal reconectado e pronto para editar. A edição anterior foi preservada em um backup local."
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
        Status = "Pronto. Ajustes são enviados durante o gesto; Salvar grava o banco A/B/C."; Refresh();
    }
    private void SetupQueue()
    {
        queue?.Stop();
        queue = new(async (key, value) =>
        {
            Presets[key.Slot].State = "Enviando…";
            confirmed = await session!.WriteLiveChecked(key.Slot, key.Offset, value, confirmed);
            UpdateStates();
        });
        queue.Failed += Fault;
    }
    private void Change(ParameterKey key, int value)
    {
        if (!CanEdit || edited == null) return;
        history.Record(key, edited[key.Slot * 16 + key.Offset], gesture);
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
        Refresh();
    }
    private void Fault(Exception ex)
    {
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
    private async Task BackupBank(byte[] bank, string reason)
    {
        Directory.CreateDirectory(Backups);
        await File.WriteAllTextAsync(Path.Combine(Backups, $"{DateTime.Now:yyyyMMdd-HHmmss-fff}-{reason}-{Guid.NewGuid():N}.json"), BankJson(bank));
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
    internal async Task LoadWavAndPreviewAsync(string path, IrAudio audio, string? collection = null)
    {
        if (collection == null) IrLibrary.ClearSelection();
        Ir.Load(path, audio, collection);
        if (Connected && !blocked) await PreviewAsync();
        else Status = "Arquivo preparado. Conecte o pedal e solte o microfone para ouvir.";
    }
    private int transferGeneration;
    private IProgress<TransferProgress> Progress()
    {
        int generation = ++transferGeneration;
        Ir.HasMeasuredProgress = false; Ir.Progress = 0; Ir.TransferActive = true;
        return new Progress<TransferProgress>(value => { if (generation == transferGeneration && Busy && Ir.TransferActive) Ir.Report(value); });
    }
    private async Task PreviewAsync()
    {
        BusyDetail = "Preparando prévia de IR…";
        if (originalRam == null)
        {
            var snapshot = await session!.ReadIrRam();
            Directory.CreateDirectory(Backups);
            string path = Path.Combine(Backups, $"ram-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json");
            await IrRamBackup.Save(path, session.IdentityHex, snapshot);
            File.Copy(path, PendingPreview, true);
            _ = await IrRamBackup.Load(PendingPreview);
            originalRam = snapshot; ramIdentity = session.IdentityHex;
        }
        if (ramIdentity != session!.IdentityHex) throw new InvalidOperationException("O snapshot de prévia pertence a outra identidade de aparelho.");
        Ir.PreviewActive = true; // Set before mutation so interrupted transfers retain recovery state.
        Ir.PreviewConfirmed = false;
        await session!.ApplyIrRam(new(1, Ir.Audio!.LivePayload(Ir.Distance)), Progress());
        Ir.PreviewConfirmed = true;
        Ir.State = "Prévia confirmada em RAM. Nenhum slot substituído."; Status = Ir.State; Refresh();
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
        BusyDetail = "Restaurando IR anterior…";
        if (session == null) throw new IOException("Reconecte para restaurar o IR anterior. O snapshot foi preservado.");
        if (ramIdentity != session.IdentityHex) throw new InvalidOperationException("Conecte o aparelho correspondente ao snapshot de prévia.");
        Ir.PreviewConfirmed = false;
        await session.ApplyIrRam(originalRam, Progress());
        File.Delete(PendingPreview); originalRam = null; ramIdentity = null;
        Ir.PreviewActive = false; Ir.State = "IR anterior restaurado e conferido."; Status = Ir.State; Refresh();
    }
    private async Task<string> BackupIr(int slot, byte[] bytes) => await IrBackup.Save(Backups, session!.Identity, session.IdentityHex, slot, bytes);
    private string MetadataPath(byte[] data) => Path.Combine(DataDirectory, "IrNames", Convert.ToHexString(SHA256.HashData(data)) + ".txt");
    private async Task ReadSlotAsync()
    {
        BusyDetail = "Lendo e preservando o slot de IR…";
        var data = await session!.ReadIrSlot(Ir.Slot, Progress()); string path = await BackupIr(Ir.Slot, data);
        UpdateCabinet(Ir.Slot, data);
        string metadata = MetadataPath(data);
        Ir.SlotInfo = File.Exists(metadata) ? "Nome local confirmado pelo conteúdo: " + await File.ReadAllTextAsync(metadata) : "Conteúdo lido · nome original não fornecido pelo pedal.";
        Ir.State = "Backup completo criado: " + Path.GetFileName(path); Status = $"Slot {Ir.Slot + 1} lido e preservado.";
        Completed(Status);
    }
    private bool confirmingIrWrite;
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
            if (!await Ask($"Gravar {file} no slot {slot + 1}?\n\nDistância: {distance}%\nO conteúdo atual será substituído.\nUm backup será criado antes da gravação.")) return;
            await Execute(() => WriteIrAsync(slot, audio, distance, file));
        }
        catch (Exception ex) { Status = ErrorMessage = ex.Message; }
        finally { confirmingIrWrite = false; Refresh(); }
    }
    private async Task WriteIrAsync(int slot, IrAudio audio, int distance, string file)
    {
        BusyDetail = "Criando backup do slot de IR…";
        var data = audio.Sector(distance);
        var current = await session!.ReadIrSlot(slot, Progress()); await BackupIr(slot, current);
        await RestorePreviewAsync();
        BusyDetail = "Gravando e verificando IR…";
        await session.ReplaceIr(slot, current, data, async bytes => { await BackupIr(slot, bytes); }, Progress());
        Directory.CreateDirectory(Path.GetDirectoryName(MetadataPath(data))!); await File.WriteAllTextAsync(MetadataPath(data), file);
        preferences.IrNames[CabinetInventory.Hash(data)] = file; SavePreferences(); UpdateCabinet(slot, data);
        Ir.SlotInfo = "Nome local confirmado pelo conteúdo: " + file;
        Status = Ir.State = $"IR gravado e verificado no slot {slot + 1}. Selecione outro cabinet e volte a este slot para recarregá-lo.";
        Completed("IR gravado e verificado.");
    }
    private async Task RestoreIrAsync()
    {
        var dlg = new OpenFileDialog { Filter = LanguageService.T("Backup de IR CubeControl|*.json"), InitialDirectory = Backups }; if (dlg.ShowDialog() != true) return;
        BusyDetail = "Criando backup do slot de IR…";
        var (info, data) = await IrBackup.Load(dlg.FileName);
        var current = await session!.ReadIrSlot(info.Slot, Progress()); await BackupIr(info.Slot, current);
        if (!await Ask($"Restaurar o backup de {info.CapturedAt:g} no slot {info.Slot + 1}?\nO conteúdo atual já foi preservado em outro backup.")) return;
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
        inventory.SetFactory(factory);
        inventory.Clear(); RebuildCabinets();
        Refresh();
    }
    private async Task ResetPresetsAsync()
    {
        await RestorePreviewAsync(); var current = await session!.ReadBank(); await BackupBank(current, "antes-reset-fabrica");
        if (!await Ask("Restaurar os três presets A/B/C de fábrica?\nOs presets atuais foram copiados para backup. Os IRs não serão alterados.")) return;
        if (!current.SequenceEqual(confirmed!)) throw new InvalidOperationException("Banco mudou. Releia antes do reset.");
        await SetBankLive(factory!.Presets); await SaveAsync(); Status = "Presets de fábrica aplicados, salvos e conferidos. IRs preservados.";
    }
    private async Task ResetIrsAsync()
    {
        await RestorePreviewAsync(); var originals = new byte[8][];
        for (int i = 0; i < 8; i++) { BusyDetail = Status = $"Backup completo do IR {i + 1}/8…"; originals[i] = await session!.ReadIrSlot(i, Progress()); await BackupIr(i, originals[i]); }
        if (!await Ask("Restaurar os oito IRs de fábrica?\nTodos os slots atuais foram copiados para backup. A operação substituirá os oito IRs; presets A/B/C não serão alterados.")) return;
        for (int i = 0; i < 8; i++)
        {
            int slot = i; BusyDetail = Status = $"Restaurando IR de fábrica {i + 1}/8…";
            await session!.ReplaceIr(i, originals[i], factory!.IrSectors[i], async bytes => { await BackupIr(slot, bytes); }, Progress());
            UpdateCabinet(i, factory.IrSectors[i]);
        }
        Ir.SlotInfo = "Oito IRs de fábrica restaurados e conferidos."; Status = Ir.State = Ir.SlotInfo;
        Completed(Status);
    }
    public Func<string, Task<bool>> Ask { get; set; } = async message =>
    {
        var dialog = new Wpf.Ui.Controls.MessageBox { PrimaryButtonIcon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Checkmark24, FontSize = 16 }, CloseButtonIcon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Dismiss24, FontSize = 16 }, Title = "CubeControl", Content = LanguageService.T(message), PrimaryButtonText = LanguageService.T("Confirmar"), CloseButtonText = LanguageService.T("Cancelar"), Owner = Application.Current.MainWindow };
        return await dialog.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary;
    };
    public async Task Flush() { if (queue != null) await queue.Flush(); }
    public void Dispose() { Amp.Edited -= AmpEdited; StopAutomaticConnection(); irSelectionGeneration++; queue?.Stop(); session?.Dispose(); session = null; }
}
