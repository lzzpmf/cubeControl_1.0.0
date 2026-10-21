using Forms = System.Windows.Forms;
namespace Cube.WinUI.Platform;
internal sealed class DialogOwner : Forms.IWin32Window
{
    public nint Handle => WinRT.Interop.WindowNative.GetWindowHandle(Dialogs.Window!);
}
public sealed class OpenFileDialog
{
    public string Filter { get; set; } = "Todos os arquivos|*.*";
    public bool Multiselect { get; set; }
    public bool CheckFileExists { get; set; } = true;
    public string FileName { get; set; } = "";
    public string[] FileNames { get; private set; } = [];
    public bool? ShowDialog()
    {
        using var picker = new Forms.OpenFileDialog { Filter = Filter, Multiselect = Multiselect, CheckFileExists = CheckFileExists, FileName = FileName };
        if (picker.ShowDialog(new DialogOwner()) != Forms.DialogResult.OK) return false;
        FileName = picker.FileName; FileNames = picker.FileNames; return true;
    }
}
public sealed class SaveFileDialog
{
    public string Filter { get; set; } = "Todos os arquivos|*.*";
    public string FileName { get; set; } = "";
    public bool? ShowDialog()
    {
        using var picker = new Forms.SaveFileDialog { Filter = Filter, FileName = FileName, OverwritePrompt = true };
        if (picker.ShowDialog(new DialogOwner()) != Forms.DialogResult.OK) return false;
        FileName = picker.FileName; return true;
    }
}
public sealed class OpenFolderDialog
{
    public bool Multiselect { get; set; }
    public string Title { get; set; } = "Selecionar pasta";
    public string InitialDirectory { get; set; } = "";
    public string FolderName { get; private set; } = "";
    public bool? ShowDialog()
    {
        using var picker = new Forms.FolderBrowserDialog { Description = Title, UseDescriptionForTitle = true, InitialDirectory = InitialDirectory };
        if (picker.ShowDialog(new DialogOwner()) != Forms.DialogResult.OK) return false;
        FolderName = picker.SelectedPath; return true;
    }
}
