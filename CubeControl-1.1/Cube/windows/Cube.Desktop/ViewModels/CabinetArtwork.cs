using System.IO;

namespace Cube.Desktop.ViewModels;

// Metadata and name-derived illustrations only. Never use these matches as audio/equipment identities.
internal static class CabinetArtwork
{
    internal const string Generic = "cabinet-unknown.png";
    private static readonly Lazy<IReadOnlyDictionary<string, IrLibraryMetadata>> Metadata = new(() =>
        IrLibraryCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Assets", "ImpulseResponses"), out _));

    internal static string? ForIdentity(string? id) => id switch
    {
        "pack-bfi-cab-0" => "Line 6 Vetta.png",
        "pack-bfi-cab-1" => "Marshall 1960AV.png",
        "pack-bfi-cab-2" => "Marshall 1960A 4\u00d712 com G12T-75.png",
        "pack-bfi-cab-3" => "VHT Deliverance.png",
        "pack-bfi-cab-4" => "Soldano.png",
        "pack-bfi-cab-5" => "Peavey 5150 + Mesa Oversized.png",
        "pack-bfi-cab-6" => "JSX KT77 + Mesa Stiletto AKG C1000.png",
        "pack-bfi-cab-7" => "Diezel V30 SM57 (Soldano HR PowerAmp).png",
        string key when key.StartsWith("pack-bfin-cab-", StringComparison.Ordinal) => Generic,
        _ => null
    };
    internal static string? ForIr(string? relativePath, string? name, IrLibraryMetadata? metadata = null)
    {
        if (metadata == null && relativePath != null)
            Metadata.Value.TryGetValue(relativePath.Replace('\\', '/'), out metadata);
        string key = Normalize(metadata != null
            ? string.Join(" ", metadata.Brand, metadata.CabinetModel, metadata.CollectionId)
            : string.Join(" ", relativePath, name));
        // Structured configuration wins over directory and display-name hints.
        string configuration = Configuration(metadata?.SpeakerConfiguration);
        if (configuration.Length == 0) configuration = Configuration(relativePath);
        if (configuration.Length == 0) configuration = Configuration(name);
        bool Fits(string expected) => configuration.Length == 0 || configuration == expected;
        if (key.Contains("m212") && Fits("2x12")) return "m212-vintage-2x12.png";
        if ((key.Contains("e112") || key.Contains("de112")) && Fits("1x12")) return "de112-ported-1x12.png";
        if ((key.Contains("r112") || key.Contains("recto112")) && Fits("1x12")) return "mesa-rectifier-1x12.png";
        if ((key.Contains("fatboy") || key.Contains("overdriven212gov") || key.Contains("odfbgov")) && Fits("2x12")) return "zilla-fatboy-2x12.png";
        if (configuration == "2x12" && (key.Contains("vertical") || key.Contains("upright")))
            return key.Contains("orange") ? "orange-v30-vertical-2x12.png" : "cabinet-vertical-2x12.png";
        if (key.Contains("fender"))
        {
            if (key.Contains("deluxereverb") && key.Contains("oxford") && Fits("1x12")) return "fender-deluxe-oxford-1x12.png";
            if (key.Contains("twinreverb") && Fits("2x12")) return "fender-twin-reverb-2x12.png";
        }
        if (key.Contains("marshall") && Fits("4x12"))
        {
            if (key.Contains("1960av")) return "Marshall 1960AV.png";
            if (key.Contains("1960tv") && key.Contains("greenback")) return "marshall-1960tv-greenback-4x12.png";
            if (key.Contains("1960") && key.Contains("1985custom")) return "marshall-1960-custom-4x12.png";
        }
        if (key.Contains("mesaboogie") || key.Contains("mesa"))
        {
            if (key.Contains("oversized") && Fits("4x12")) return "Peavey 5150 + Mesa Oversized.png";
            if (key.Contains("4fb") && Fits("4x12")) return "mesa-4fb-v30-4x12.png";
            // Rectifier alone does not establish the number of speakers.
            if (key.Contains("rectifier") && configuration == "1x12") return "mesa-rectifier-1x12.png";
            if (key.Contains("rectifier") && configuration == "4x12") return "mesa-rectifier-v30.png";
        }
        if (key.Contains("orange") && configuration == "2x12") return "orange-v30-2x12.png";
        if (key.Contains("vox") && key.Contains("ac30") && key.Contains("blue") && Fits("2x12")) return "vox-ac30-blue-2x12.png";
        // These unbranded illustrations express count, not an equipment identity.
        return configuration switch
        {
            "1x12" => "cabinet-generic-front.png",
            "2x12" => "zilla-fatboy-2x12.png",
            "4x12" => "marshall-1960-custom-4x12.png",
            _ => null
        };
    }
    private static string Normalize(string? text) => string.Concat((text ?? "").Replace('\u00d7', 'x').Replace('*', 'x')
        .Where(char.IsLetterOrDigit)).ToLowerInvariant();
    private static string Configuration(string? text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text ?? "", @"(?<!\d)(\d+)\s*[xX\u00d7*]\s*(\d+)(?!\d)");
        return match.Success ? match.Groups[1].Value + "x" + match.Groups[2].Value : "";
    }
}
