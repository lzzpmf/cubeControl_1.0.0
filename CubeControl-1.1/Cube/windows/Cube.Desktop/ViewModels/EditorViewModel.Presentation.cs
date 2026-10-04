namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public sealed record OperationHistoryItem(DateTime Time, string Description)
    {
        internal Cube.Protocol.CubeSession? Device { get; init; }
        internal Func<Task<bool>>? Restore { get; init; }
        internal Func<bool>? Available { get; init; }
        internal object? Group { get; init; }
        internal int SoundRevision { get; init; }
        public bool IsRestorable => Restore != null;
        public string Hint => IsRestorable ? "Duplo clique para retornar a este ajuste após confirmação." : "Registro informativo; esta operação não pode ser desfeita pelo histórico.";
    }
    public System.Collections.ObjectModel.ObservableCollection<OperationHistoryItem> Operations { get; } = [];
    private bool restoringOperation;
    private long operationVersion;
    private int soundContentRevision;
    private string lastOperationKind = "";
    private DateTime lastOperationTime;
    private object lastOperationGroup = new();
    private object OperationGroup(string kind)
    {
        if (gesture != null) return gesture;
        if (kind != lastOperationKind || DateTime.UtcNow - lastOperationTime > TimeSpan.FromMilliseconds(600)) lastOperationGroup = new();
        lastOperationKind = kind; lastOperationTime = DateTime.UtcNow;
        return lastOperationGroup;
    }
    private void RecordOperation(string message, Func<Task<bool>>? restore = null, Func<bool>? available = null, object? group = null)
    {
        if (restoringOperation) return;
        operationVersion++;
        var item = new OperationHistoryItem(DateTime.Now, message) { Device = session, Restore = restore, Available = available, Group = group, SoundRevision = soundContentRevision };
        if (group != null && Operations.Count > 0 && ReferenceEquals(Operations[0].Group, group)) Operations[0] = item;
        else Operations.Insert(0, item);
        while (Operations.Count > 20) Operations.RemoveAt(Operations.Count - 1);
    }
    public async Task RestoreOperationAsync(OperationHistoryItem item)
    {
        bool Available() => !Busy && !blocked && item.Device != null && ReferenceEquals(item.Device, session) && item.SoundRevision == soundContentRevision && item.Available?.Invoke() == true;
        if (item.Restore == null) return;
        if (!Available()) { NotifyFailure("Este ajuste não está disponível no contexto atual. Use o mesmo aparelho e uma prévia compatível."); return; }
        if (!await Ask($"Retornar à operação de {item.Time:HH:mm:ss}?\n{item.Description}\n\nOs ajustes atuais serão substituídos na RAM. Nada será gravado permanentemente.")) return;
        if (!Available()) return;
        bool success;
        restoringOperation = true;
        try { success = await item.Restore(); }
        catch (Exception ex) { ErrorMessage = ex.Message; NotifyFailure(ex.Message); success = false; }
        finally { restoringOperation = false; }
        if (success) RecordOperation("Ajuste recuperado: " + item.Description, item.Restore, item.Available);
        else RecordOperation("Não foi possível recuperar: " + item.Description);
    }
    private byte[]? lastHistoryBank;
    private object? bankOperationGroup;
    private string bankOperationDescription = "Ajustes dos presets aplicados na RAM.";
    private void RecordBankOperation(string description)
    {
        if (confirmed == null || edited == null || !confirmed.SequenceEqual(edited) || lastHistoryBank?.SequenceEqual(confirmed) == true) return;
        byte[] snapshot = confirmed.ToArray(); lastHistoryBank = snapshot;
        RecordOperation(description, async () =>
        {
            bool done = false;
            await Execute(async () => { await SetBankLive(snapshot); done = true; }, changesEffects: true);
            return done;
        }, () => CanEdit, bankOperationGroup);
    }
    private bool loadingDevice;
    public bool LoadingDevice { get => loadingDevice; private set { if (Set(ref loadingDevice, value)) RefreshPresentation(); } }
    public bool ShowConnectAction => !Connected || LoadingDevice;
    public string ConnectActionLabel => LoadingDevice ? "Carregando dispositivo" : "Conectar e carregar";
    public string HeaderConnectionStatus => LoadingDevice || SearchingPorts ? BusyDetail : blocked ? "Conexão requer atenção" : ConnectionSummary;
    public string BankSaveStatus => Dirty ? "Alterações não gravadas" : "Banco A/B/C sem alterações pendentes";
    public Wpf.Ui.Controls.ControlAppearance SaveAppearance => Wpf.Ui.Controls.ControlAppearance.Primary;
    private string busyDetail = "Aguarde a conclusão da operação.";
    private int inventoryReadCount;
    private bool readingInventory;
    private bool searchingPorts;
    public bool SearchingPorts { get => searchingPorts; private set { if (Set(ref searchingPorts, value)) RefreshPresentation(); } }
    public bool ShowBusyPanel => Busy && !liveParameterOperation && !SearchingPorts && !IsAmpPreviewSending && !PhaserBackgroundOperation && !ChorusBackgroundOperation && !ReverbBackgroundOperation && !DelayBackgroundOperation;
    public bool ShowBusyRing => ShowBusyPanel && !ReadingInventory && !ShowTransferProgress;
    public bool ShowTransferProgress => Busy && !ReadingInventory && Ir.TransferActive && Ir.HasMeasuredProgress;
    public string BusyDetail { get => busyDetail; private set { if (Set(ref busyDetail, value)) Notify(nameof(HeaderConnectionStatus)); } }
    public int InventoryReadCount { get => inventoryReadCount; private set => Set(ref inventoryReadCount, value); }
    public bool ReadingInventory { get => readingInventory; private set { if (Set(ref readingInventory, value)) RefreshPresentation(); } }
    private string errorMessage = "";
    public string ErrorMessage { get => errorMessage; private set { if (Set(ref errorMessage, value)) Notify(nameof(HasError)); } }
    public bool HasError => ErrorMessage.Length != 0;
    public event Action<UserNotification>? NotificationRequested;
    private void Completed(string message) => NotifyUser("Concluído", message, Wpf.Ui.Controls.ControlAppearance.Success);
    private void NotifyUser(string title, string message, Wpf.Ui.Controls.ControlAppearance appearance) =>
        NotificationRequested?.Invoke(new(title, message, appearance));
    private void NotifyFailure(string message) => NotifyUser("Não foi possível concluir", message, Wpf.Ui.Controls.ControlAppearance.Danger);

    public string ConnectionSummary => Connected ? "Conectado por USB" : "Sem conexão USB";
    public System.Windows.Media.ImageSource? ConnectedDeviceImage { get; } = GearItem.ReadImage(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "imagens", "Dispositivos", "SINCO-CubeBaby.png"));
    public string EditAvailability => ShowBusyPanel ? "Operação em andamento. Aguarde para editar."
        : blocked ? "Edição suspensa. Releia os presets ou reconecte."
        : !Connected ? "Conecte o aparelho para editar e ouvir os ajustes."
        : Ir.PreviewActive && AmpPreviewActive ? "Prévias de AMP e IR ativas na RAM. Encerre as prévias para editar os presets."
        : AmpPreviewActive ? "Prévia de AMP ativa na RAM. Encerre a prévia para editar os presets."
        : Ir.PreviewActive ? "Encerre a prévia de IR para editar os presets." : "";
    public bool HasEditNotice => EditAvailability.Length > 0;
    public bool ShowEndPreviewNotice => !Busy && Connected && Ir.PreviewActive;
    public bool ShowEndAmpPreviewNotice => CanRestoreAmp;
    public string EndIrPreviewLabel => AmpPreviewActive ? "Encerrar prévia IR" : "Encerrar prévia";
    public string EndAmpPreviewLabel => Ir.PreviewActive ? "Encerrar prévia AMP" : "Encerrar prévia";
    public Wpf.Ui.Controls.InfoBarSeverity EditNoticeSeverity => !Busy && (blocked || ShowEndPreviewNotice || ShowEndAmpPreviewNotice)
        ? Wpf.Ui.Controls.InfoBarSeverity.Warning : Wpf.Ui.Controls.InfoBarSeverity.Informational;
    public string IrAvailability => Busy ? "Operação em andamento. Aguarde a conclusão."
        : blocked ? "Operação indisponível. Releia os presets ou reconecte."
        : !Connected ? "Conecte o aparelho para ouvir ou gravar o IR."
        : Ir.Audio == null ? "Selecione um WAV para ouvir ou gravar."
        : "Ao gravar, o conteúdo do slot será substituído após preservar o original em memória e confirmar.";
    public string GalleryLabel => IsAmp ? "Catálogo de amps" : IsCabinet ? "Catálogo de Gabinetes" : "Escolher modulação";
    private string Activation(string name, bool enabled) => name + " · " + (!HasBank ? "Não carregado" : enabled ? "Ligado" : "Desligado");
    public string ModulationLabel => Activation("Modulação", Current.Musical.ModulationEnabled);
    public string DelayLabel => Activation("Delay", Current.Musical.DelayEnabled);
    public string ReverbLabel => Activation("Reverb", Current.Musical.ReverbEnabled);
    public string CabinetLabel => Activation("Gabinete / IR", Current.Parameters[10].Enabled);
    public string ToneLabel => Activation("Tone", Current.Parameters[12].Enabled);
    public string FactoryAvailability => factory == null && IrPacks.Count == 0 ? "Nenhum recurso de presets ou pack de IRs disponível."
        : !Connected ? "Conecte o aparelho para aplicar packs ou restaurar presets."
        : Busy ? "Aguarde a operação em andamento." : "O conteúdo anterior é preservado em memória durante esta sessão.";
    private void RefreshPresentation()
    {
        Notify(nameof(EditNoticeSeverity));
        Notify(nameof(ShowEndPreviewNotice));
        Notify(nameof(ShowEndAmpPreviewNotice)); Notify(nameof(EndIrPreviewLabel)); Notify(nameof(EndAmpPreviewLabel));
        foreach (string name in new[] { nameof(ShowBusyPanel), nameof(ShowBusyRing), nameof(ShowTransferProgress), nameof(ShowConnectAction), nameof(ConnectActionLabel), nameof(HeaderConnectionStatus), nameof(BankSaveStatus), nameof(SaveAppearance), nameof(ConnectionSummary), nameof(EditAvailability), nameof(HasEditNotice), nameof(IrAvailability), nameof(FactoryAvailability) }) Notify(name);
    }
}
