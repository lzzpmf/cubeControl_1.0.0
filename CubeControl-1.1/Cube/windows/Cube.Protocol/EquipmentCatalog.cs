namespace Cube.Protocol;

/// <summary>Local reference descriptions from docs/referencias/ampsEirs.md; never protocol values.</summary>
public sealed record EquipmentReference(string Name, string Alias, string Category, string Details)
{
    public string Description => $"No Cube Baby: {Alias}. {Details}";
}

public static class EquipmentCatalog
{
    public static IReadOnlyList<EquipmentReference> Amps { get; } = Array.AsReadOnly<EquipmentReference>(
    [
        new("Koch Powertone", "Power-Zone Clean", "Clean", "Canal Clean."),
        new("Friedman BE-100", "US Gold 100 Clean", "Clean", "Canal Clean."),
        new("Two-Rock Coral", "Two Stone Coral OD", "Overdrive", "Canal Overdrive."),
        new("Dr. Z MAZ 38", "Doctor3 B", "Overdrive", "Canal B. Categoria Overdrive do catálogo do pedal."),
        new("Mesa/Boogie JP-2C", "Cali JP A", "Overdrive", "Canal A. Categoria Overdrive do catálogo do pedal."),
        new("Vox AC30", "Day Tripper OD", "Overdrive", "Canal Overdrive, conforme a referência local."),
        new("VHT Pittbull", "Shittcow Dist", "Distortion", "Canal Distortion."),
        new("Two-Rock Coral / variante", "Wo Stone Coral OD", "Distortion", "Canal Distortion. Identificação provável (~70% na referência local), não confirmada."),
        new("PRS Archon", "Mr Smith Dist", "Distortion", "Canal Distortion.")
    ]);

    // Index 0 is IR slot 1. OFF is a separate protocol value, not an IR sector.
    public static IReadOnlyList<EquipmentReference> Cabinets { get; } = Array.AsReadOnly<EquipmentReference>(
    [
        new("Line 6 Vetta 2×12", "Line 6 Vetta", "2×12", "Microfone desconhecido."),
        new("Marshall 1960AV 4×12", "Marshall 1960AV", "4×12", "Falantes Celestion Vintage 30. Microfone desconhecido."),
        new("Marshall 1960A 4×12 · G12T-75", "Marshall 1960A T75 LA610", "4×12", "Falantes Celestion G12T-75. LA610 provavelmente indica UA LA-610 na cadeia de captura; não identifica o microfone."),
        new("VHT/Fryette Deliverance 2×12", "VHT Deliverance", "2×12", "Falantes e microfone desconhecidos."),
        new("Soldano 2×12", "Soldano", "2×12", "Prováveis falantes Celestion Vintage 30. Microfone desconhecido."),
        new("Mesa/Boogie Oversized 4×12", "Peavey 5150 + Mesa Oversized", "4×12", "Amp/power amp Peavey 5150. Prováveis falantes Celestion Vintage 30. Microfone desconhecido."),
        new("Mesa/Boogie Stiletto 4×12", "JSX KT77 + Mesa Stiletto AKG C1000", "4×12", "Amp/power amp Peavey JSX; válvulas KT77. Microfone AKG C1000."),
        new("Diezel 4×12 · Vintage 30", "Diezel V30 SM57 (Soldano HR PowerAmp)", "4×12", "Falantes Celestion Vintage 30. Microfone Shure SM57. Power amp Soldano HR.")
    ]);

    public static EquipmentReference? FindCabinet(string? cabinetId)
        => cabinetId != null && cabinetId.StartsWith("pack-bfi-cab-", StringComparison.Ordinal)
            && int.TryParse(cabinetId[13..], out int index) && index >= 0 && index < Cabinets.Count ? Cabinets[index] : null;
}
