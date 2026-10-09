using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

internal static class LocalizationTests
{
    public static async Task Run()
    {
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
        string folder = Path.Combine(Path.GetTempPath(), "cube-language-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string original = LanguageService.Instance.Language;
        try
        {
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Localization", "en-US.json"), Path.Combine(folder, "en-US.json"));
            File.WriteAllText(Path.Combine(folder, "broken.json"), "{broken");
            var third = new LanguagePack { Code = "fr-FR", Name = "Français", Strings = new() { ["Idioma"] = "Langue" } };
            File.WriteAllText(Path.Combine(folder, "fr-FR.json"), JsonSerializer.Serialize(third));
            string preference = Path.Combine(folder, "settings", "language-v1.json");
            var language = new LanguageService(folder, preference);
            Check(language.Language == "pt-BR" && language.Languages.Count == 3, "Portuguese default and discovery of additional valid language packs");
            language.Select("en-US");
            Check(language.Translate("Aço Texturizado") == "Textured Steel" && language.Translate("Crânio Azul") == "Blue Skull" && language.Translate("Marfim Dourado") == "Golden Ivory", "archive finish names are translated");
            Check(language.Translate("Acabamento dos controles") == "Control finish" && language.Translate("Marfim Retrô") == "Retro Ivory" && language.Translate("Oficial — acompanha o tema") == "Official — follows theme", "control finish names are translated");
            Check(language.Translate("Salvar A/B/C no aparelho") == "Save A/B/C to device", "English actions and confirmations");
            Check(language.Translate("IR personalizado — slot 7") == "Custom IR — slot 7" && language.Translate("Lendo · 113/4096 bytes") == "Reading · 113/4096 bytes", "dynamic status retains numbers and translates surrounding text");
            Check(language.Translate("Mesa/Boogie JP-2C") == "Mesa/Boogie JP-2C", "equipment names preserved");
            Check(new LanguageService(folder, preference).Language == "en-US", "language preference survives restart");
            language.Select("fr-FR"); Check(language.Translate("Idioma") == "Langue" && language.Translate("Conectar") == "Conectar", "third language loads with Portuguese fallback for missing keys");
            language.Select("not-installed"); Check(language.Language == "fr-FR", "unknown language does not replace current selection");
            File.WriteAllText(preference, "invalid"); Check(new LanguageService(folder, preference).Language == "pt-BR", "invalid preference falls back safely");
            using var model = new EditorViewModel(Path.Combine(folder, "model"));
            LanguageService.Instance.Preview("pt-BR");
            var text = (TextBlock)XamlReader.Parse("<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:loc='clr-namespace:Cube.Desktop.Localization;assembly=CubeControl' Text='{loc:Translate Text=Conectar}'/>");
            var bound = (TextBlock)XamlReader.Parse("<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:loc='clr-namespace:Cube.Desktop.Localization;assembly=CubeControl' Text='{loc:Translate Value={Binding Status}}'/>");
            bound.DataContext = model;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(text.Text == "Conectar", "Portuguese WPF literal binding");
            LanguageService.Instance.Preview("en-US");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(text.Text == "Connect" && bound.Text == "Connect your Cube Baby via USB to get started.", "language switch updates literals and existing viewmodel bindings immediately");
            Check(model.WriteCommandCount == 0 && model.Current.Parameters[0].Value == 0, "language switch does not send MIDI or edit preset values");
            var converter = new TranslateConverter();
            Check((string)converter.Convert(["Desligado.wav", "en-US"], typeof(string), true, System.Globalization.CultureInfo.InvariantCulture) == "Desligado.wav", "user filenames are not translated");
            LanguageService.Instance.Preview("pt-BR"); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(text.Text == "Conectar", "switch back to Portuguese updates existing controls");
        }
        finally
        {
            LanguageService.Instance.Preview(original);
            string resolved = Path.GetFullPath(folder);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("cube-language-tests-")) throw new IOException("Unexpected test directory");
            Directory.Delete(resolved, true);
        }
        Console.WriteLine($"{passed} localization checks passed.");
    }
}
