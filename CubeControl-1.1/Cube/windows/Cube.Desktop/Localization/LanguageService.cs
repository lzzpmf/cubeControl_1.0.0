using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cube.Desktop.Localization;

public sealed record LanguageOption(string Code, string Name);
public sealed class LanguagePack
{
    public int Version { get; set; } = 1;
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Dictionary<string, string> Strings { get; set; } = [];
}

public sealed class LanguageService : INotifyPropertyChanged
{
    public static LanguageService Instance { get; } = new();
    private readonly Dictionary<string, LanguagePack> packs = [];
    private readonly string preferencePath;
    private string language = "pt-BR";
    private Regex? phrases;
    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyList<LanguageOption> Languages => packs.Values.Select(p => new LanguageOption(p.Code, p.Name)).ToArray();
    public string Language => language;
    public string this[string text] => Translate(text);

    public LanguageService(string? catalogDirectory = null, string? settingsPath = null)
    {
        preferencePath = settingsPath ?? Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "CubeControl", "language-v1.json");
        packs["pt-BR"] = new() { Code = "pt-BR", Name = "Português (Brasil)" };
        string directory = catalogDirectory ?? Path.Combine(AppContext.BaseDirectory, "Localization");
        if (Directory.Exists(directory)) foreach (string path in Directory.EnumerateFiles(directory, "*.json"))
        {
            try
            {
                var pack = JsonSerializer.Deserialize<LanguagePack>(File.ReadAllText(path));
                if (pack is { Version: 1, Strings: not null } && !string.IsNullOrWhiteSpace(pack.Code) && !string.IsNullOrWhiteSpace(pack.Name)
                    && pack.Strings.All(p => !string.IsNullOrWhiteSpace(p.Key) && p.Value != null)) packs[pack.Code] = pack;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { /* Invalid optional packs do not prevent startup. */ }
        }
        try
        {
            using var saved = JsonDocument.Parse(File.ReadAllText(preferencePath));
            if (saved.RootElement.ValueKind == JsonValueKind.Object && saved.RootElement.TryGetProperty("language", out var value)
                && value.ValueKind == JsonValueKind.String && value.GetString() is string code && packs.ContainsKey(code)) language = code;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
        Rebuild();
    }

    public void Select(string code)
    {
        if (!packs.ContainsKey(code) || code == language) return;
        // Persist before applying; a failed write leaves the current language intact.
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(preferencePath))!);
        string temporary = preferencePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(new { version = 1, language = code }));
        File.Move(temporary, preferencePath, true);
        Preview(code);
    }
    internal void Preview(string code)
    {
        if (!packs.ContainsKey(code)) return;
        language = code; Rebuild();
        PropertyChanged?.Invoke(this, new(nameof(Language)));
        PropertyChanged?.Invoke(this, new("Item[]"));
    }

    private void Rebuild()
    {
        var keys = packs[language].Strings.Where(p => p.Key != p.Value).Select(p => p.Key).OrderByDescending(k => k.Length).ToArray();
        phrases = keys.Length == 0 ? null : new Regex("(?<![\\p{L}\\p{N}_])(?:" + string.Join("|", keys.Select(Regex.Escape)) + ")(?![\\p{L}\\p{N}_])", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    }
    public string Translate(string text)
    {
        if (packs[language].Strings.TryGetValue(text, out string? translated)) return translated;
        // Single replacement pass handles composed status/error messages without translating the output twice.
        return phrases?.Replace(text, match => packs[language].Strings[match.Value]) ?? text;
    }
    public static string T(string text) => Instance.Translate(text);
}

public sealed class TranslateConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values[0] is string text ? parameter is true && Path.HasExtension(text) ? text : LanguageService.T(text) : values[0];
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

// Both literals and existing one-way bindings update immediately when the language changes.
public sealed class TranslateExtension : MarkupExtension
{
    public BindingBase? Value { get; set; }
    public string? Text { get; set; }
    public bool PreserveFileNames { get; set; }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new MultiBinding { Mode = BindingMode.OneWay, Converter = new TranslateConverter(), ConverterParameter = PreserveFileNames };
        binding.Bindings.Add(Value ?? new Binding { Source = Text ?? "" });
        binding.Bindings.Add(new Binding(nameof(LanguageService.Language)) { Source = LanguageService.Instance });
        return binding.ProvideValue(serviceProvider);
    }
}
