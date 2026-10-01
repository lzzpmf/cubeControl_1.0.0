namespace Cube.Protocol;

public sealed record Parameter(string Name, int Offset, int Maximum, string Module = "", string Description = "", string[]? Options = null);

public static class CubeBabyProfile
{
    public static readonly Parameter[] Parameters =
    [
        new("Amplificador", 0, 8, "Amp", "Referências de amplificadores segundo o manual.",
            ["1 · Power-Zone Clean · Clean", "2 · US Gold 100 Clean · Clean", "3 · Two Stone Coral OD · Overdrive", "4 · Doctor3 B · Overdrive", "5 · Cali JP A · Overdrive", "6 · Day Tripper OD · Distortion", "7 · Shittcow Dist · Distortion", "8 · Wo Stone Coral OD · Distortion", "9 · Mr Smith Dist · Distortion"]),
        new("Gain", 1, 7, "Amp", "Quantidade de ganho do amplificador."),
        new("Tone", 2, 15, "Amp", "Brilho do timbre."),
        new("Reverb", 3, 15, "Reverb", "Mistura de reverberação. Em zero, sem reverb."),
        new("Repetições", 4, 127, "Delay", "Quantidade de realimentação das repetições (FB)."),
        new("Volume", 5, 127, "Saída", "Nível geral do preset."),
        new("Tempo", 6, 31, "Delay", "Ajuste TIME do pedal; escala própria, sem conversão para milissegundos."),
        new("Mistura", 7, 118, "Delay", "Mistura do delay com o sinal direto (MIX). Em zero, sem sinal de delay."),
        new("Modulação", 8, 15, "Modulação", "0–6 Chorus · 7–8 desligado · 9–15 Phaser."),
        new("Cabinet", 9, 8, "IR Cab", "Nomes abaixo são referências de fábrica; o conteúdo do slot pode ter sido substituído.",
            ["Desligado", "1 · Line 6 Vetta · 2×12", "2 · Marshall 1960AV · 4×12", "3 · Marshall 1960A T75 LA610 · 4×12", "4 · VHT Deliverance · 2×12", "5 · Soldano · 2×12", "6 · Peavey 5150 + Mesa Oversized · 4×12", "7 · JSX KT77 + Mesa Stiletto AKG C1000 · 4×12", "8 · Diezel V30 SM57 (Soldano HR PowerAmp) · 4×12"]),
        new("Seção IR", 10, 1, "IR Cab", "Interruptor da seção IR do protocolo."),
        new("Seção delay", 11, 1, "Delay", "Interruptor da seção delay do protocolo."),
        new("Seção tone", 12, 1, "Amp", "Interruptor da seção tone do protocolo; não representa bypass geral.")
    ];

    public static string Display(int offset, int value) => offset switch
    {
        8 => value <= 6 ? $"Chorus · {value}" : value <= 8 ? "Desligado" : $"Phaser · {value}",
        3 or 7 when value == 0 => "Desligado",
        10 or 11 or 12 => value == 0 ? "Desligado" : "Ligado",
        _ => value.ToString()
    };

    public static uint LiveAddress(int slot, int offset)
    {
        if (slot is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(slot));
        if (offset is < 0 or > 12) throw new ArgumentOutOfRangeException(nameof(offset));
        return 0x80000000u + (uint)(slot * 16 + offset);
    }

    public static void ValidateBank(byte[] bank)
    {
        if (bank.Length != 48) throw new FormatException("O banco Cube Baby deve ter 48 bytes.");
        for (int slot = 0; slot < 3; slot++)
            foreach (var parameter in Parameters)
                if (bank[slot * 16 + parameter.Offset] > parameter.Maximum)
                    throw new FormatException($"Preset {(char)('A' + slot)}: {parameter.Name} fora do mapa conhecido. Edição bloqueada para este firmware.");
    }
}
