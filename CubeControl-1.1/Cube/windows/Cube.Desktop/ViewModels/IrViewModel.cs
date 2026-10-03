using System.IO;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed class IrViewModel : Observable
{
    private string file = "Nenhum arquivo selecionado", information = "Selecione um WAV para preparar a prévia.", state = "Nenhuma prévia ativa", slotInfo = "Conteúdo ainda não lido do aparelho.";
    private int slot = 0, distance = 50;
    private double progress;
    private bool preview, previewConfirmed, transferActive;
    public string Origin { get; private set; } = "";
    public string SourceCollection { get; private set; } = "";
    public string SourcePath { get; private set; } = "";
    public IrAudio? Audio { get; private set; }
    public string File { get => file; private set => Set(ref file, value); }
    public string Information { get => information; private set => Set(ref information, value); }
    public string State { get => state; set => Set(ref state, value); }
    public string SlotInfo { get => slotInfo; set => Set(ref slotInfo, value); }
    public bool PreviewActive { get => preview; set { Set(ref preview, value); Notify(nameof(PreviewLabel)); } }
    public bool PreviewConfirmed { get => previewConfirmed; set { Set(ref previewConfirmed, value); Notify(nameof(PreviewLabel)); } }
    public bool TransferActive { get => transferActive; set => Set(ref transferActive, value); }
    public string PreviewLabel => PreviewActive ? PreviewConfirmed ? "Prévia confirmada em RAM · não gravada no slot" : "Prévia pendente · restauração não confirmada" : "Nenhuma prévia ativa";
    public string[] Slots { get; private set; } = Enumerable.Range(1, 8).Select(i => $"Slot {i} — IR não identificado").ToArray();
    public void UpdateSlots(IEnumerable<string> names)
    {
        int selectedSlot = slot;
        Slots = names.ToArray(); Notify(nameof(Slots));
        slot = selectedSlot; Notify(nameof(Slot));
    }
    public int Slot { get => slot; set { if (Set(ref slot, Math.Clamp(value, 0, 7))) SlotInfo = "Conteúdo ainda não lido do aparelho."; } }
    public int Distance { get => distance; set { Set(ref distance, Math.Clamp(value, 0, 99)); Notify(nameof(DistanceLabel)); } }
    public string DistanceLabel => $"{Distance}%";
    private bool hasMeasuredProgress;
    public bool HasMeasuredProgress { get => hasMeasuredProgress; set => Set(ref hasMeasuredProgress, value); }
    public double Progress { get => progress; set => Set(ref progress, value); }
    public float[] Waveform => Audio?.Prepare() ?? [];
    public string Conversion => Audio?.Conversion ?? "WAV PCM 16/24/32 bits ou float 32 bits · mono/estéreo";
    public string PreparationSummary => Audio == null ? "Selecione um WAV para preparar a prévia." : "O pedal usará 512 amostras preparadas a 48 kHz (aprox. 10,7 ms).";
    public void Load(string path, IrAudio audio, string? collection = null)
    {
        Origin = collection == null ? "Arquivo externo" : "Biblioteca";
        SourceCollection = collection ?? "";
        Notify(nameof(Origin)); Notify(nameof(SourceCollection));
        SourcePath = path;
        Audio = audio; File = DisplayName(path, collection);
        Information = $"{audio.Format} {audio.Bits} bits · {audio.SampleRate:N0} Hz · {audio.Channels} canal(is) · {audio.Duration * 1000:0.##} ms";
        Notify(nameof(Waveform)); Notify(nameof(Conversion)); Notify(nameof(PreparationSummary)); Notify(nameof(Audio));
    }
    public static string DisplayName(string path, string? collection = null)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        var parts = (collection ?? "").Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p is not ("IRs WAV" or "Configuração Não Informada" or "ImpulseResponses"))
            .Append(name).Distinct(StringComparer.OrdinalIgnoreCase);
        return string.Join(" ", parts);
    }
    public void ClearLoadedFile()
    {
        Audio = null;
        File = "Nenhum arquivo selecionado";
        Information = "Selecione um WAV para preparar a prévia.";
        Origin = SourceCollection = SourcePath = "";
        Notify(nameof(Audio)); Notify(nameof(Origin)); Notify(nameof(SourceCollection));
        Notify(nameof(Waveform)); Notify(nameof(Conversion)); Notify(nameof(PreparationSummary));
    }
    public void Report(TransferProgress value)
    { HasMeasuredProgress = value.Total > 0; State = $"{value.Stage} · {value.Done}/{value.Total} bytes"; Progress = value.Total == 0 ? 0 : 100d * value.Done / value.Total; }
}
