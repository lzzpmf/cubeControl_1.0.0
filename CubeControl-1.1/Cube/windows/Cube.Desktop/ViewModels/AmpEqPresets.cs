using System.IO;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed record AmpEqPreset(string Name, int BaseIndex, double FrequencyA, double LevelA, double QA,
    double FrequencyB, double LevelB, double QB)
{
    public string BaseName => EquipmentCatalog.Amps[BaseIndex].Name;
    public string ImageName => Name + ".png";
    public string Description => $"{Name} · EQ de {BaseName}";

    public byte[] Build()
    {
        using var stream = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps")
            ?? throw new InvalidOperationException("Modelos de fábrica indisponíveis.");
        stream.Position = BaseIndex * AmpImage.Size;
        var bytes = new byte[AmpImage.Size];
        stream.ReadExactly(bytes);
        AmpImage.SetEq(bytes, 0x5E8, FrequencyA, LevelA, QA);
        AmpImage.SetEq(bytes, 0x604, FrequencyB, LevelB, QB);
        AmpImage.Validate(bytes);
        return bytes;
    }
}

public static class AmpEqPresets
{
    // Tonal inspirations only: the factory nonlinearity and gain remain untouched.
    // Values replace the two final peaking filters; they are not additive EQ deltas.
    public static IReadOnlyList<AmpEqPreset> All { get; } = Array.AsReadOnly<AmpEqPreset>(
    [
        new("Blackface Clean", 0, 750, -3.5, .7, 3200, 2.5, .8),
        new("Hiwatt Glass", 1, 180, -2, .7, 2200, 3, .8),
        new("Dumble Bloom", 2, 850, 3.5, .8, 3600, -2.5, .7),
        new("Tweed Deluxe", 3, 300, 3, .8, 2800, -3, .7),
        new("Recto Modern", 4, 750, -4, .9, 120, 3, .8),
        new("Matchless Chime", 5, 1600, 3, .8, 250, -2.5, .7),
        new("Brit 800 Bite", 6, 1400, 3.5, .9, 140, -3, .7),
        new("Orange Growl", 7, 450, 4, .8, 3500, -3, .8),
        new("5150 Focus", 8, 120, -3, .8, 1800, 3.5, .9)
    ]);
}
