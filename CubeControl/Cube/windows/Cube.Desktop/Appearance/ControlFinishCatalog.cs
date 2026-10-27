namespace Cube.Desktop.Appearance;

internal sealed record ControlFinishDefinition(ControlFinish Finish, string Label, string Folder,
    string Glyph, string LightTrack, string DarkTrack, bool ExternalKnobMarker = false, double KnobPivotY = .5);

internal static class ControlFinishCatalog
{
    internal static IReadOnlyList<ControlFinishDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        new ControlFinishDefinition(ControlFinish.Official, "Oficial — acompanha o tema", "official-light", "#25292D", "#59616A", "#89929B"),
        new ControlFinishDefinition(ControlFinish.BrushedSilver, "Prata Escovada", "brushed-silver", "#25292D", "#59616A", "#89929B"),
        new ControlFinishDefinition(ControlFinish.RetroIvory, "Marfim Retrô", "retro-ivory", "#49301D", "#76624A", "#B7A38B"),
        new ControlFinishDefinition(ControlFinish.GraphiteCopper, "Grafite Cobre", "graphite-copper", "#FFF1D3", "#895536", "#C59271"),
        new ControlFinishDefinition(ControlFinish.BlackIvory, "Preto Marfim", "black-ivory", "#FFF0D5", "#6C5F4F", "#C6BCA7", KnobPivotY: .615),
        new ControlFinishDefinition(ControlFinish.IndustrialBlack, "Preto Industrial", "industrial-black", "#202428", "#54616C", "#9DAAB4"),
        new ControlFinishDefinition(ControlFinish.TexturedSteel, "Aço Texturizado", "textured-steel", "#202428", "#535D65", "#9AA6AE"),
        new ControlFinishDefinition(ControlFinish.MarconiBlack, "Preto Marconi", "marconi-black", "#FFF0D5", "#70604A", "#C6B49C"),
        new ControlFinishDefinition(ControlFinish.VintageBrass, "Latão Vintage", "vintage-brass", "#352719", "#80632A", "#C5A25E"),
        new ControlFinishDefinition(ControlFinish.IvoryGold, "Marfim Dourado", "ivory-gold", "#49301D", "#80672F", "#CDB177"),
        new ControlFinishDefinition(ControlFinish.ChromeSkullBlue, "Crânio Azul", "chrome-skull-blue", "#102A3A", "#24718C", "#62C9EA", true)
    });

    internal static ControlFinishDefinition Get(ControlFinish finish) => All.FirstOrDefault(item => item.Finish == finish)
        ?? throw new ArgumentOutOfRangeException(nameof(finish));
}
