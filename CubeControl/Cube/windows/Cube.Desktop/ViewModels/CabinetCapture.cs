using System.IO;
using System.Text.Json;

namespace Cube.Desktop.ViewModels;

public sealed record CabinetCapture(IrLibraryMetadata Metadata, string SourceName)
{
    private static string Join(params string?[] values)
    {
        var parts = new List<string>();
        foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            string text = value!.Trim();
            if (text.Contains("não identificad", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("não informad", StringComparison.OrdinalIgnoreCase)) continue;
            if (!parts.Any(p => p.Contains(text, StringComparison.OrdinalIgnoreCase)))
            {
                parts.RemoveAll(p => text.Contains(p, StringComparison.OrdinalIgnoreCase));
                parts.Add(text);
            }
        }
        return string.Join(" · ", parts);
    }
    public string Title => Join(Metadata.Brand, Metadata.CabinetModel, Metadata.SpeakerConfiguration?.Replace('x', '×'));
    public string Speaker => Join(Metadata.SpeakerLabel);
    public string Summary => Join(Metadata.Microphone, Metadata.Position,
        string.IsNullOrWhiteSpace(Metadata.Microphone) ? null : $"{Metadata.CaptureDistanceMm} mm");
    public string Hint => string.Join("\n", new[] {
        Join(Metadata.MicrophoneLabel, Metadata.Position,
            string.IsNullOrWhiteSpace(Metadata.Position) ? null : Metadata.PositionDescription),
        Join(string.IsNullOrWhiteSpace(Metadata.Microphone) ? null : $"{Metadata.CaptureDistanceMm} mm", Metadata.VariantLabel,
            Metadata.IsPreview ? "Amostra SP1-PREVIEW" : null),
        Join(Metadata.PowerAmp, Metadata.MicPreamp),
        "Arquivo: " + SourceName
    }.Where(s => !string.IsNullOrWhiteSpace(s)));
}

internal sealed class CabinetCaptureStore(string path)
{
    private Dictionary<string, CabinetCapture>? entries;
    private Dictionary<string, CabinetCapture> Entries
    {
        get
        {
            if (entries != null) return entries;
            try
            {
                entries = File.Exists(path)
                    ? JsonSerializer.Deserialize<Dictionary<string, CabinetCapture>>(File.ReadAllText(path)) ?? [] : [];
                entries = entries.Where(p => p.Key.Length == 64 && p.Key.All(Uri.IsHexDigit) &&
                    p.Value?.Metadata is { Variants: not null, Microphone: not null, Position: not null,
                        PowerAmp: not null, MicPreamp: not null } && p.Value.SourceName != null)
                    .ToDictionary(p => p.Key, p => p.Value);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            { entries = []; }
            return entries;
        }
    }
    internal CabinetCapture? Get(string hash) => Entries.GetValueOrDefault(hash);
    internal void Set(string hash, CabinetCapture capture)
    {
        Entries[hash] = capture;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(Entries));
        File.Move(temporary, path, true);
    }
}
