using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Cube.Desktop.Documentation;

internal static class DocumentLauncher
{
    internal static bool TryOpen(string path, out string error, Action<string>? launch = null)
    {
        try
        {
            if (!Uri.TryCreate(path, UriKind.Absolute, out var uri) || uri.IsFile)
            {
                if (!File.Exists(path)) throw new FileNotFoundException();
            }
            else if (uri.Scheme is not ("http" or "https" or "mailto")) throw new IOException();
            (launch ?? (target => Process.Start(new ProcessStartInfo(target) { UseShellExecute = true })))(path);
            error = ""; return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException)
        {
            error = "Não foi possível abrir o documento. Confira os arquivos do pacote e o aplicativo associado no Windows.";
            return false;
        }
    }
}
