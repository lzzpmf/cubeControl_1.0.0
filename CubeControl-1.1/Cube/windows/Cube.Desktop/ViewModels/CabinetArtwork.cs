namespace Cube.Desktop.ViewModels;

// Name-derived illustrations only. Never use these matches as audio/equipment identities.
internal static class CabinetArtwork
{
    internal const string Generic = "cabinet-generic-front.png";
    internal static string? ForIr(string? relativePath, string? name)
        => Match(relativePath) ?? Match(name);

    private static string? Match(string? text)
    {
        string key = string.Concat((text ?? "").Replace('×', 'x').Where(char.IsLetterOrDigit)).ToLowerInvariant();
        if (key.Contains("2x12") && (key.Contains("vertical") || key.Contains("upright")))
            return key.Contains("orange") ? "orange-v30-vertical-2x12.png" : "cabinet-vertical-2x12.png";
        if (key.Contains("fender"))
        {
            if (key.Contains("deluxereverb") && key.Contains("oxford")) return "fender-deluxe-oxford-1x12.png";
            if (key.Contains("twinreverb")) return "fender-twin-reverb-2x12.png";
        }
        if (key.Contains("marshall"))
        {
            if (key.Contains("1960av")) return "Marshall 1960AV.png";
            if (key.Contains("1960tv") && key.Contains("greenback")) return "marshall-1960tv-greenback-4x12.png";
            if (key.Contains("1960") && key.Contains("1985custom")) return "marshall-1960-custom-4x12.png";
        }
        if (key.Contains("mesaboogie") || key.Contains("mesa"))
        {
            if (key.Contains("oversized")) return "Peavey 5150 + Mesa Oversized.png";
            if (key.Contains("4fb") && key.Contains("vintage30")) return "mesa-4fb-v30-4x12.png";
            if (key.Contains("rectifier") && key.Contains("vintage30")) return "mesa-rectifier-v30.png";
        }
        if (key.Contains("orange") && key.Contains("2x12") && key.Contains("vintage30")) return "orange-v30-2x12.png";
        if (key.Contains("vox") && key.Contains("ac30") && key.Contains("blue")) return "vox-ac30-blue-2x12.png";
        return null;
    }
}
