using Cube.Desktop.Localization;
using Cube.Desktop.Services;

namespace Cube.Desktop.Studio.ViewModels;

public sealed class PlaybackResultViewModel(YouTubeResult video) : ViewModelBase
{
    public YouTubeResult Video { get; } = video;
    public string Title => Video.Title;
    public string Genre => Video.Channel;
    public string Details => Video.Channel + " · " + LanguageService.T("Origem") + ": YouTube";
    public double Duration => Video.Duration;
    public double? Bpm { get; internal set; }
    public string? Key { get; internal set; }
    public string Metadata => string.Join(" · ", new[] { Video.DurationLabel,
        Bpm is { } bpm ? LanguageService.T($"BPM estimado: {bpm:0.#}") : null,
        string.IsNullOrEmpty(Key) ? null : LanguageService.T("Tom estimado: ") + Key }.Where(s => !string.IsNullOrEmpty(s)));
    public bool IsPreviewing { get; internal set; }
    public string PreviewActionText => LanguageService.T(IsPreviewing ? "Parar" : "Ouvir");
    public bool IsPreparingWaveform { get; internal set; }
    public bool HasFullWaveform { get; internal set; }
    public string? WaveformError { get; internal set; }
    public string WaveformStatus => IsPreparingWaveform ? LanguageService.T("Preparando áudio…") : WaveformError ?? (Peaks.Length == 0 ? LanguageService.T("Passe o mouse para preparar a prévia") : HasFullWaveform ? LanguageService.T("Áudio completo") : LanguageService.T("Prévia · até 30 s"));
    public bool IsPinned { get; internal set; }
    public bool IsPreviewPinned => IsPinned;
    public double PreviewPosition { get; internal set; }
    public double PreviewDuration { get; internal set; } = 30;
    public float[] Peaks { get; internal set; } = [];
    public double PreviewProgress => Math.Clamp(PreviewPosition / Math.Max(.001, PreviewDuration), 0, 1);
    public string PreviewText => IsPreviewing ? $"{TimeSpan.FromSeconds(PreviewPosition):m\\:ss} / {TimeSpan.FromSeconds(PreviewDuration):m\\:ss}" : LanguageService.T("Prévia · até 30 s");
    public string DownloadStatus { get; internal set; } = "";
    public double DownloadProgress { get; internal set; }
    public bool IsDownloading { get; internal set; }
    public void Refresh() { foreach (var name in new[] { nameof(Metadata), nameof(Details), nameof(IsPreviewing), nameof(PreviewActionText), nameof(IsPreparingWaveform), nameof(WaveformStatus), nameof(IsPinned), nameof(IsPreviewPinned), nameof(PreviewPosition), nameof(PreviewProgress), nameof(PreviewText), nameof(Peaks), nameof(DownloadStatus), nameof(DownloadProgress), nameof(IsDownloading) }) OnPropertyChanged(name); }
}
