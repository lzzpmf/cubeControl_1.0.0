using System.IO;

namespace Cube.Desktop;

// Explicit mappings keep equipment identities independent of folder organization.
internal static class AssetPaths
{
    internal static IReadOnlyDictionary<string, string> Images { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Botão Giratório Vintage de Metal e Preto.png"] = "Images/Controls/Knobs/Botão Giratório Vintage de Metal e Preto.png",
        ["Botão Metálico de Controle Texturizado.png"] = "Images/Controls/Knobs/Botão Metálico de Controle Texturizado.png",
        ["Botão Rotativo Vintage Preto Marconi.png"] = "Images/Controls/Knobs/Botão Rotativo Vintage Preto Marconi.png",
        ["Botão Vintage de Metal Dourado.png"] = "Images/Controls/Knobs/Botão Vintage de Metal Dourado.png",
        ["Diezel V30 SM57 (Soldano HR PowerAmp).png"] = "Images/Equipment/Cabinets/Diezel V30 SM57 (Soldano HR PowerAmp).png",
        ["DirectBox.png"] = "Images/Equipment/Cabinets/DirectBox.png",
        ["Dr. Z MAZ 38.png"] = "Images/Equipment/Amps/Dr. Z MAZ 38.png",
        ["Fader Preto com Indicador Prateado.png"] = "Images/Controls/Faders/Fader Preto com Indicador Prateado.png",
        ["Fader Retrô Charcoal com Faixa Creme.png"] = "Images/Controls/Faders/Fader Retrô Charcoal com Faixa Creme.png",
        ["Fader Retrô preto com faixa creme.png"] = "Images/Controls/Faders/Fader Retrô preto com faixa creme.png",
        ["Fader Vintage de Estilo Retrô.png"] = "Images/Controls/Faders/Fader Vintage de Estilo Retrô.png",
        ["Friedman BE-100.png"] = "Images/Equipment/Amps/Friedman BE-100.png",
        ["JSX KT77 + Mesa Stiletto AKG C1000.png"] = "Images/Equipment/Cabinets/JSX KT77 + Mesa Stiletto AKG C1000.png",
        ["Koch Powertone.png"] = "Images/Equipment/Amps/Koch Powertone.png",
        ["Line 6 Vetta.png"] = "Images/Equipment/Cabinets/Line 6 Vetta.png",
        ["Marshall 1960A 4×12 com G12T-75.png"] = "Images/Equipment/Cabinets/Marshall 1960A 4×12 com G12T-75.png",
        ["Marshall 1960AV.png"] = "Images/Equipment/Cabinets/Marshall 1960AV.png",
        ["MesaBoogie JP-2C.png"] = "Images/Equipment/Amps/MesaBoogie JP-2C.png",
        ["PRS Archon.png"] = "Images/Equipment/Amps/PRS Archon.png",
        ["Peavey 5150 + Mesa Oversized.png"] = "Images/Equipment/Cabinets/Peavey 5150 + Mesa Oversized.png",
        ["Soldano.png"] = "Images/Equipment/Cabinets/Soldano.png",
        ["Two Rock Coral- variante.png"] = "Images/Equipment/Amps/Two Rock Coral- variante.png",
        ["Two-Rock Coral.png"] = "Images/Equipment/Amps/Two-Rock Coral.png",
        ["VHT Deliverance.png"] = "Images/Equipment/Cabinets/VHT Deliverance.png",
        ["VHT Pittbull.png"] = "Images/Equipment/Amps/VHT Pittbull.png",
        ["Blackface Clean.png"] = "Images/Equipment/Amps/Blackface Clean.png",
        ["Hiwatt Glass.png"] = "Images/Equipment/Amps/Hiwatt Glass.png",
        ["Dumble Bloom.png"] = "Images/Equipment/Amps/Dumble Bloom.png",
        ["Tweed Deluxe.png"] = "Images/Equipment/Amps/Tweed Deluxe.png",
        ["Recto Modern.png"] = "Images/Equipment/Amps/Recto Modern.png",
        ["Matchless Chime.png"] = "Images/Equipment/Amps/Matchless Chime.png",
        ["Brit 800 Bite.png"] = "Images/Equipment/Amps/Brit 800 Bite.png",
        ["Orange Growl.png"] = "Images/Equipment/Amps/Orange Growl.png",
        ["5150 Focus.png"] = "Images/Equipment/Amps/5150 Focus.png",
        ["Vox AC30.png"] = "Images/Equipment/Amps/Vox AC30.png",
        ["cabinet-generic-front.png"] = "Images/Equipment/Cabinets/cabinet-generic-front.png",
        ["cabinet-unknown.png"] = "Images/Equipment/Cabinets/cabinet-unknown.png",
        ["zilla-fatboy-2x12.png"] = "Images/Equipment/Cabinets/zilla-fatboy-2x12.png",
        ["de112-ported-1x12.png"] = "Images/Equipment/Cabinets/de112-ported-1x12.png",
        ["m212-vintage-2x12.png"] = "Images/Equipment/Cabinets/m212-vintage-2x12.png",
        ["mesa-rectifier-1x12.png"] = "Images/Equipment/Cabinets/mesa-rectifier-1x12.png",
        ["cabinet-vertical-2x12.png"] = "Images/Equipment/Cabinets/cabinet-vertical-2x12.png",
        ["chorusCuvave.png"] = "Images/Equipment/Effects/chorusCuvave.png",
        ["delayCuvave.png"] = "Images/Equipment/Effects/delayCuvave.png",
        ["fender-deluxe-oxford-1x12.png"] = "Images/Equipment/Cabinets/fender-deluxe-oxford-1x12.png",
        ["fender-twin-reverb-2x12.png"] = "Images/Equipment/Cabinets/fender-twin-reverb-2x12.png",
        ["gabineteGenerico.png"] = "Images/Equipment/Cabinets/gabineteGenerico.png",
        ["marshall-1960-custom-4x12.png"] = "Images/Equipment/Cabinets/marshall-1960-custom-4x12.png",
        ["marshall-1960tv-greenback-4x12.png"] = "Images/Equipment/Cabinets/marshall-1960tv-greenback-4x12.png",
        ["mesa-4fb-v30-4x12.png"] = "Images/Equipment/Cabinets/mesa-4fb-v30-4x12.png",
        ["mesa-rectifier-v30.png"] = "Images/Equipment/Cabinets/mesa-rectifier-v30.png",
        ["cabinet-distance-side.png"] = "Images/Equipment/Cabinets/cabinet-distance-side.png",
        ["microphone-distance-side.png"] = "Images/Equipment/Microphones/microphone-distance-side.png",
        ["pack-ribbon-microphone.png"] = "Images/Equipment/Microphones/pack-ribbon-microphone.png",
        ["pack-dynamic-microphone.png"] = "Images/Equipment/Microphones/pack-dynamic-microphone.png",
        ["micGenerico.png"] = "Images/Equipment/Microphones/micGenerico.png",
        ["orange-v30-2x12.png"] = "Images/Equipment/Cabinets/orange-v30-2x12.png",
        ["orange-v30-vertical-2x12.png"] = "Images/Equipment/Cabinets/orange-v30-vertical-2x12.png",
        ["phaserCuvave.png"] = "Images/Equipment/Effects/phaserCuvave.png",
        ["reverbCuvave.png"] = "Images/Equipment/Effects/reverbCuvave.png",
        ["vox-ac30-blue-2x12.png"] = "Images/Equipment/Cabinets/vox-ac30-blue-2x12.png",
    };
    internal static string Image(string name) => Images.TryGetValue(name, out var relative)
        ? Path.Combine(AppContext.BaseDirectory, "Assets", relative)
        : Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Equipment", "not-provided.png");
    internal static string CabinetSelection => Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Equipment", "Cabinets", "Selection");
    internal static string Device(string name) => Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Equipment", "Devices", name);
    internal static string Documents => Path.Combine(AppContext.BaseDirectory, "Assets", "Documents");
}
