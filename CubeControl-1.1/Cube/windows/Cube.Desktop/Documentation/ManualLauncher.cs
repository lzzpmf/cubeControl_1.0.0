using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Cube.Desktop.Documentation;

internal static class ManualLauncher
{
    internal static bool TryOpen(string documentsRoot, string language, bool light, out string error, Action<string>? launch = null)
    {
        try
        {
            string locale = language == "en-US" ? "en-US" : "pt-BR";
            string file = Path.GetFullPath(Path.Combine(documentsRoot, "manual", locale, "index.html"));
            if (!File.Exists(file)) throw new FileNotFoundException();
            var uri = new UriBuilder(new Uri(file)) { Query = "theme=" + (light ? "light" : "dark") };
            (launch ?? (target => Process.Start(new ProcessStartInfo(target) { UseShellExecute = true })))(uri.Uri.AbsoluteUri);
            error = "";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException)
        {
            error = "Não foi possível abrir o manual. Confira a pasta Assets/Documents/manual do pacote e o navegador padrão do Windows.";
            return false;
        }
    }
}
