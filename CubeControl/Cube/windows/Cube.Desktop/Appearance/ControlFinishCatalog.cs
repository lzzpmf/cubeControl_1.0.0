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
        new ControlFinishDefinition(ControlFinish.ChromeSkullBlue, "Crânio Azul", "chrome-skull-blue", "#102A3A", "#24718C", "#62C9EA", true),
        new ControlFinishDefinition(ControlFinish.Vasco, "Times — Vasco", "vasco", "#FFFFFF", "#555D65", "#C3CBD1"),
        new ControlFinishDefinition(ControlFinish.Flamengo, "Times — Flamengo", "flamengo", "#FFFFFF", "#A52834", "#EF737D"),
        new ControlFinishDefinition(ControlFinish.Fluminense, "Times — Fluminense", "fluminense", "#30271E", "#596E60", "#A9C8B2"),
        new ControlFinishDefinition(ControlFinish.Botafogo, "Times — Botafogo", "botafogo", "#FFFFFF", "#555D65", "#C3CBD1"),
        new ControlFinishDefinition(ControlFinish.Skeleton, "Skeleton", "skeleton", "#30271E", "#796852", "#D4C1A0"),
        new ControlFinishDefinition(ControlFinish.Cartoon, "Cartoon", "cartoon", "#25243B", "#327F88", "#75DCE3"),
        new ControlFinishDefinition(ControlFinish.Anime, "Anime", "anime", "#E1F8FF", "#6755A1", "#B4A1F4"),
        new ControlFinishDefinition(ControlFinish.Boho, "Boho", "boho", "#493526", "#7E684E", "#CFB896"),
        new ControlFinishDefinition(ControlFinish.Papercut, "Papercut", "papercut", "#243E43", "#427D82", "#8BCBD0"),
        new ControlFinishDefinition(ControlFinish.ReceiverSilverBlue, "Receiver 70 — Prata Azul", "receiver-silver-blue", "#202E3D", "#42668A", "#8DB8DE"),
        new ControlFinishDefinition(ControlFinish.ReceiverChampagne, "Receiver 70 — Champagne", "receiver-champagne", "#3B2A19", "#846333", "#D5B679"),
        new ControlFinishDefinition(ControlFinish.TurntableWalnut, "Vitrola — Nogueira", "turntable-walnut", "#FFF0D5", "#795337", "#D6AD83"),
        new ControlFinishDefinition(ControlFinish.TurntableDirectDrive, "Vitrola — Direct Drive", "turntable-direct-drive", "#F3EEE3", "#59636A", "#AEBBC3"),
        new ControlFinishDefinition(ControlFinish.MiniSystemTitanium, "Mini System 90 — Titânio", "minisystem-titanium", "#F2FFE7", "#536D3D", "#B3D98A"),
        new ControlFinishDefinition(ControlFinish.MiniSystemBlackRed, "Mini System 90 — Preto Vermelho", "minisystem-black-red", "#FFF1DB", "#924550", "#E4989E")
    });

    internal static ControlFinishDefinition Get(ControlFinish finish) => All.FirstOrDefault(item => item.Finish == finish)
        ?? throw new ArgumentOutOfRangeException(nameof(finish));
}
