using System.IO;
using System.Text.Json;

namespace Cube.Desktop.ViewModels;

public sealed record IrLibraryMetadata
{
    public string RelativePath { get; init; } = "";
    public string Sha256 { get; init; } = "";
    public string CollectionId { get; init; } = "";
    public string Collection { get; init; } = "";
    public string Brand { get; init; } = "Marca não identificada";
    public string SpeakerConfiguration { get; init; } = "Configuração não informada";
    public string CabinetModel { get; init; } = "";
    public string SpeakerLabel { get; init; } = "";
    public string Identification { get; init; } = "unknown";
    public string IdentificationEvidence { get; init; } = "";
    public string[] Sources { get; init; } = [];
    public string Cabinet { get; init; } = "";
    public string Speaker { get; init; } = "";
    public string PowerAmp { get; init; } = "";
    public string MicPreamp { get; init; } = "";
    public string Microphone { get; init; } = "";
    public string? MicrophoneModel { get; init; }
    public string Position { get; init; } = "";
    public int CaptureDistanceMm { get; init; }
    public string[] Variants { get; init; } = [];
    public bool IsPreview { get; init; }
    public string CapturePrefix { get; init; } = "";
    public string OriginalName { get; init; } = "";
    public int SampleRate { get; init; }
    public int Channels { get; init; }
    public int Bits { get; init; }
    public string Format { get; init; } = "";
    public double DurationMs { get; init; }
    public string MicrophoneLabel => MicrophoneModel is { Length: > 0 } ? $"{Microphone} · inspirado em {MicrophoneModel}" : Microphone;
    public string IdentificationLabel => Identification switch { "confirmed" => "Documentada", "probable" => "", _ => "Não identificada" };
    public string ModelLabel => CabinetModel.Length > 0 ? CabinetModel + " · " + SpeakerLabel : Collection;
    public string VariantLabel => string.Join(" · ", Variants.Select(v => v switch
    {
        "LC" => "Corte de graves (LC)", "LC45" => "Corte de graves 45 Hz (LC45)",
        "LC75" => "Corte de graves 75 Hz (LC75)", "L" => "Falante esquerdo (L)",
        "UA610" => "Pré UA 610", "EQ" => "Equalização (EQ)", _ => v
    }));
    public string LeafName => $"{MicrophoneLabel} · {Position} · {PositionDescription} · {CaptureDistanceMm} mm"
        + (Variants.Length > 0 ? " · " + VariantLabel : "") + (IsPreview ? " · Amostra SP1-PREVIEW" : "");
    public string PositionDescription => Position switch
    {
        "P00" => "Centro da calota", "P05" => "Sobre a calota", "P09" => "Próximo da borda da calota",
        "P10" => "Borda da calota", "P20" => "Cone", "P30" => "Próximo da borda externa",
        _ => "Localização exata não documentada"
    };
    public string SearchText => string.Join(" ", "Overdriven.fr", Brand, SpeakerConfiguration, ModelLabel, LeafName, CollectionId, Collection, Cabinet, Speaker, PowerAmp, MicPreamp,
        Microphone, MicrophoneModel, Position, CaptureDistanceMm, string.Join(" ", Variants), CapturePrefix, IsPreview ? "SP1-PREVIEW" : "");
}

internal sealed record IrLibraryCatalog(int Version, string Provider, IrLibraryMetadata[] Entries)
{
    public static IReadOnlyDictionary<string, IrLibraryMetadata> Load(string directory, out string problem)
    {
        problem = "";
        string path = Path.Combine(directory, "Overdriven.catalog.v1.json");
        if (!File.Exists(path)) return new Dictionary<string, IrLibraryMetadata>();
        try
        {
            if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new FormatException("Catálogo excede 4 MB.");
            var catalog = JsonSerializer.Deserialize<IrLibraryCatalog>(File.ReadAllText(path));
            if (catalog is not { Version: 1, Provider: "Overdriven.fr", Entries: not null }) throw new FormatException("Versão de catálogo não suportada.");
            var result = new Dictionary<string, IrLibraryMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.RelativePath) || !entry.RelativePath.StartsWith("Overdriven.fr/", StringComparison.Ordinal)
                    || entry.RelativePath.Contains('\\') || entry.RelativePath.Split('/').Any(p => p is "" or "." or "..")
                    || entry.RelativePath.Contains(':') || !entry.RelativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                    || entry.Sha256 is not { Length: 64 } || !entry.Sha256.All(Uri.IsHexDigit)
                    || string.IsNullOrWhiteSpace(entry.Collection) || string.IsNullOrWhiteSpace(entry.CollectionId)
                    || string.IsNullOrWhiteSpace(entry.Cabinet) || string.IsNullOrWhiteSpace(entry.Speaker)
                    || string.IsNullOrWhiteSpace(entry.Brand) || string.IsNullOrWhiteSpace(entry.SpeakerConfiguration)
                    || entry.CabinetModel == null || entry.SpeakerLabel == null || entry.IdentificationEvidence == null
                    || entry.Identification is not ("confirmed" or "probable" or "unknown")
                    || entry.Sources == null || entry.Sources.Any(string.IsNullOrWhiteSpace)
                    || string.IsNullOrWhiteSpace(entry.Microphone) || string.IsNullOrWhiteSpace(entry.Position)
                    || entry.CaptureDistanceMm < 0 || entry.Variants == null || entry.Variants.Any(v => v == null)
                    || entry.SampleRate is < 8000 or > 384000 || entry.Channels is < 1 or > 2 || entry.Bits is not (16 or 24 or 32)
                    || entry.Format is not ("PCM" or "Float") || !double.IsFinite(entry.DurationMs) || entry.DurationMs <= 0
                    || !result.TryAdd(entry.RelativePath, entry)) throw new FormatException("Entrada de catálogo inválida ou duplicada.");
            }
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException)
        {
            problem = "Metadados indisponíveis. Os WAVs continuam acessíveis pela estrutura de pastas.";
            return new Dictionary<string, IrLibraryMetadata>();
        }
    }
}
