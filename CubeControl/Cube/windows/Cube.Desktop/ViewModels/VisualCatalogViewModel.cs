using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class GearItem(string id, int value, string name, string category, string kind, string color, string description = "", string alias = "", string? cabinetId = null, string? ampImage = null, AmpEqPreset? uploadPreset = null, CabinetCapture? capture = null) : Observable
{
    public AmpEqPreset? UploadPreset => uploadPreset;
    public bool RequiresUpload => uploadPreset != null;
    public string Id => id;
    public int Value => value;
    public string Name => string.IsNullOrWhiteSpace(capture?.Title) ? name : capture.Title;
    public string CaptureSummary => capture?.Summary ?? "";
    public string CaptureHint => capture?.Hint ?? "";
    public bool HasCapture => CaptureSummary.Length > 0;
    // Model specifications, not an analysis of the recorded IR. Sources: docs/referencias/cabinet-characteristics.md.
    public string Category => kind == "cab" ? cabinetId switch
    {
        "pack-bfi-cab-0" or "pack-bfin-cab-0" => "2×12 · Traseira aberta · Celestion customizados",
        "pack-bfi-cab-1" or "pack-bfin-cab-1" => "4×12 · Frente inclinada · Celestion G12-Vintage",
        "pack-bfi-cab-2" or "pack-bfin-cab-2" => "4×12 · Frente inclinada · Celestion G12T-75",
        "pack-bfi-cab-3" or "pack-bfin-cab-3" => "2×12 · Frente reta",
        "pack-bfi-cab-4" or "pack-bfin-cab-4" => "2×12 · Ilustração vertical",
        "pack-bfi-cab-5" or "pack-bfin-cab-5" => "4×12 · Caixa oversized · Graves encorpados",
        "pack-bfi-cab-6" or "pack-bfin-cab-6" => "4×12 · Graves firmes · Médios pronunciados",
        "pack-bfi-cab-7" or "pack-bfin-cab-7" => "4×12 · Celestion Vintage 30",
        _ => category
    } : category;
    public string? CategoryHint => kind == "cab" && EquipmentCatalog.FindCabinet(cabinetId) is { } reference
        ? reference.Category == "2×12"
            ? "Ilustração vertical; não representa necessariamente o formato físico do modelo de referência."
            : "Características do modelo de referência; a captura do IR pode variar."
        : kind == "cab" && CabinetArtwork.ForIr(null, name, capture?.Metadata) != null
            ? "Arte ilustrativa baseada nos metadados ou no nome do IR; não confirma o gabinete da captura." : null;
    public string Kind => kind;
    public string Color => Appearance.ThemeService.ModuleColor(color);
    public void RefreshColor() => Notify(nameof(Color));
    public string Description => capture?.Hint ?? description;
    public string Alias => alias;
    public ImageSource? DefaultImage => ReadImage(AssetPaths.Image(ampImage ?? ((cabinetId ?? id) switch
    {
        "amp-0" => "Koch Powertone.png",
        "amp-1" => "Friedman BE-100.png",
        "amp-2" => "Two-Rock Coral.png",
        "amp-3" => "Dr. Z MAZ 38.png",
        "amp-4" => "MesaBoogie JP-2C.png",
        "amp-5" => "Vox AC30.png",
        "amp-6" => "VHT Pittbull.png",
        "amp-7" => "Two Rock Coral- variante.png",
        "amp-8" => "PRS Archon.png",
        "cab-off" => "DirectBox.png",
        "chorus" => "chorusCuvave.png",
        "phaser" => "phaserCuvave.png",
        "delay" => "delayCuvave.png",
        "reverb" => "reverbCuvave.png",
        _ when kind == "cab" => CabinetArtwork.ForIdentity(cabinetId ?? id)
            ?? CabinetArtwork.ForIr(null, name, capture?.Metadata) ?? CabinetArtwork.Generic,
        _ => "not-provided.png"
    })));
    // Brief effect explanations; sources in docs/referencias/effect-descriptions.md.
    public string Subtitle => capture != null ? capture.Speaker : kind switch
    {
        "chorus" => "Mistura o som original com uma cópia levemente desafinada, criando corpo e movimento.",
        "phaser" => "Cria uma varredura de fase que dá ao timbre um movimento ondulante.",
        "delay" => "Repete o som após um intervalo, criando ecos e padrões rítmicos.",
        "reverb" => "Simula reflexões sonoras de um ambiente, acrescentando espaço e profundidade.",
        _ => Category
    };
    public string SlotLabel => kind is "chorus" or "phaser" or "delay" or "reverb" ? "" : kind == "cab" && value > 0 ? $"Slot {value}" : category;
    private ImageSource? image;
    public ImageSource? Image { get => image; set => Set(ref image, value); }
    public static ImageSource? ReadImage(string? path)
    {
        if (path == null || !File.Exists(path)) return null;
        try
        {
            using var stream = File.OpenRead(path);
            var bitmap = new BitmapImage(); bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 1200; bitmap.StreamSource = stream; bitmap.EndInit(); bitmap.Freeze(); return bitmap;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or ArgumentException or FormatException) { return null; }
    }
}

public sealed class MatchRow : Observable
{
    private GearItem? selected;
    public GearItem Amp { get; }
    public GearItem[] Choices { get; }
    public string Warning { get; }
    public GearItem? Selected { get => selected; set => Set(ref selected, value); }
    public MatchRow(GearItem amp, GearItem[] cabinets, GearItem? choice, bool stale)
    {
        Amp = amp;
        var none = new GearItem("no-match", 0, "Sem associação", "", "cab", "#A0ADA9");
        var unavailable = new GearItem("unavailable", -1, "Associação indisponível — manter", "", "cab", "#A0ADA9");
        Choices = (stale ? new[] { unavailable, none } : new[] { none }).Concat(cabinets.Where(c => c.Value > 0)).ToArray();
        selected = stale ? unavailable : choice?.Value > 0 ? choice : none;
        Warning = stale ? "Associação indisponível: conteúdo não encontrado." : "";
    }
}
