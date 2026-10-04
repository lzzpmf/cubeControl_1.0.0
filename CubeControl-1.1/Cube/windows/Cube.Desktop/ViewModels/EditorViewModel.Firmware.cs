using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Input;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public sealed record FirmwareChoice(string Path)
    {
        public string Name => System.IO.Path.GetFileName(Path);
    }
    public ObservableCollection<FirmwareChoice> Firmwares { get; } = new();
    private FirmwareChoice? selectedFirmware;
    public FirmwareChoice? SelectedFirmware
    {
        get => selectedFirmware;
        set { Set(ref selectedFirmware, value); CommandManager.InvalidateRequerySuggested(); }
    }
    public bool CanChooseFirmware => !Busy;
    public ICommand ChooseFirmware { get; private set; } = null!;
    public ICommand RefreshFirmwareList { get; private set; } = null!;
    public ICommand InstallFirmware { get; private set; } = null!;
    private bool CanInstallFirmware => !Busy && !Connected && SelectedFirmware != null
#if CUBE_SIMULATION
        && false
#endif
        ;

    private void InitializeFirmware()
    {
        RefreshFirmwareList = Command(() => { LoadFirmwares(); return Task.CompletedTask; }, () => !Busy);
        ChooseFirmware = Command(() =>
        {
            var dialog = new OpenFileDialog { Filter = "Firmware Cube Baby (*.up)|*.up", CheckFileExists = true };
            if (dialog.ShowDialog() == true)
            {
                try { FirmwareUpdater.Validate(dialog.FileName); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    ErrorMessage = Status = ex.Message;
                    NotifyFailure(ex.Message);
                    return Task.CompletedTask;
                }
                var choice = new FirmwareChoice(dialog.FileName);
                if (!Firmwares.Any(f => f.Path == choice.Path)) Firmwares.Add(choice);
                SelectedFirmware = Firmwares.First(f => f.Path == choice.Path);
            }
            return Task.CompletedTask;
        }, () => !Busy);
        InstallFirmware = Command(InstallFirmwareAsync, () => CanInstallFirmware);
        LoadFirmwares();
    }

    private void LoadFirmwares()
    {
        var previous = SelectedFirmware;
        Firmwares.Clear();
        if (Directory.Exists(FirmwareUpdater.Folder))
            foreach (string path in Directory.EnumerateFiles(FirmwareUpdater.Folder, "*", SearchOption.AllDirectories)
                .Where(p => Path.GetExtension(p).Equals(".up", StringComparison.OrdinalIgnoreCase)).OrderBy(Path.GetFileName))
                Firmwares.Add(new(path));
        if (previous != null && File.Exists(previous.Path) && !Firmwares.Any(f => f.Path == previous.Path)) Firmwares.Add(previous);
        SelectedFirmware = Firmwares.FirstOrDefault(f => f.Path == previous?.Path)
            ?? Firmwares.FirstOrDefault(f => f.Name.Equals("cubebaby_B_20220706.up", StringComparison.OrdinalIgnoreCase))
            ?? Firmwares.FirstOrDefault();
    }

    private async Task InstallFirmwareAsync()
    {
        ErrorMessage = "";
        string path = SelectedFirmware!.Path;
        byte[] data = FirmwareUpdater.Validate(path);
        string hash = Convert.ToHexString(SHA256.HashData(data));
        if (!await Ask($"Reinstalar/atualizar o firmware com {Path.GetFileName(path)}?\n\nConfirme que esta variante é adequada ao seu pedal. Presets e IRs podem ser substituídos.\n\nFeche o flash_loader. Ligue o pedal conectado ao PC segurando o terceiro footswitch para entrar em modo flash. Não desconecte nem desligue durante a gravação.\n\nO Windows solicitará permissão de administrador.")) return;
        BusyDetail = Status = "Aguardando autorização do Windows…";
        await FirmwareUpdater.RunElevated(path, hash, message => BusyDetail = Status = message.Text);
        automaticConnectionPaused = false;
        Completed(Status);
    }
}
