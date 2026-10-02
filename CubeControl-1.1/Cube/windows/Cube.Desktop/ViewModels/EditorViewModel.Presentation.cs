namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
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
    public bool ShowBusyPanel => Busy && !SearchingPorts && !IsAmpPreviewSending && !PhaserBackgroundOperation && !ChorusBackgroundOperation && !ReverbBackgroundOperation && !DelayBackgroundOperation;
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
    public string EditAvailability => ShowBusyPanel ? "Operação em andamento. Aguarde para editar."
        : blocked ? "Edição suspensa. Releia os presets ou reconecte."
        : !Connected ? "Conecte o aparelho para editar e ouvir os ajustes."
        : Ir.PreviewActive ? "Encerre a prévia de IR para editar os presets." : "";
    public bool HasEditNotice => EditAvailability.Length > 0;
    public bool ShowEndPreviewNotice => !Busy && !blocked && Connected && Ir.PreviewActive;
    public Wpf.Ui.Controls.InfoBarSeverity EditNoticeSeverity => !Busy && (blocked || ShowEndPreviewNotice)
        ? Wpf.Ui.Controls.InfoBarSeverity.Warning : Wpf.Ui.Controls.InfoBarSeverity.Informational;
    public string IrAvailability => Busy ? "Operação em andamento. Aguarde a conclusão."
        : blocked ? "Operação indisponível. Releia os presets ou reconecte."
        : !Connected ? "Conecte o aparelho para ouvir ou gravar o IR."
        : Ir.Audio == null ? "Selecione um WAV para ouvir ou gravar."
        : "Ao gravar, o conteúdo do slot será substituído após backup e confirmação.";
    public string GalleryLabel => IsAmp ? "Catálogo de amps" : IsCabinet ? "Catálogo de Gabinetes" : "Escolher modulação";
    private string Activation(string name, bool enabled) => name + " · " + (!HasBank ? "Não carregado" : enabled ? "Ligado" : "Desligado");
    public string ModulationLabel => Activation("Modulação", Current.Musical.ModulationEnabled);
    public string DelayLabel => Activation("Delay", Current.Musical.DelayEnabled);
    public string ReverbLabel => Activation("Reverb", Current.Musical.ReverbEnabled);
    public string CabinetLabel => Activation("Gabinete / IR", Current.Parameters[10].Enabled);
    public string ToneLabel => Activation("Tone", Current.Parameters[12].Enabled);
    public string FactoryAvailability => factory == null ? "Recuperação indisponível: recursos de fábrica não encontrados."
        : !Connected ? "Conecte o aparelho para restaurar os dados de fábrica."
        : Busy ? "Aguarde a operação em andamento." : "Backups são criados antes da confirmação de restauração.";
    private void RefreshPresentation()
    {
        Notify(nameof(EditNoticeSeverity));
        Notify(nameof(ShowEndPreviewNotice));
        foreach (string name in new[] { nameof(ShowBusyPanel), nameof(ShowBusyRing), nameof(ShowTransferProgress), nameof(ShowConnectAction), nameof(ConnectActionLabel), nameof(HeaderConnectionStatus), nameof(BankSaveStatus), nameof(SaveAppearance), nameof(ConnectionSummary), nameof(EditAvailability), nameof(HasEditNotice), nameof(IrAvailability), nameof(FactoryAvailability) }) Notify(name);
    }
}
