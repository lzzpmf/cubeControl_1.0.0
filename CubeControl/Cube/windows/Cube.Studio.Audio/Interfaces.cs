namespace Cube.Studio.Audio;

public interface IStudioDevices
{
    StudioAudioSettings Settings { get; }
    StudioAudioCapabilities Capabilities { get; }
    Task<IReadOnlyList<StudioDevice>> EnumerateDevicesAsync(CancellationToken cancellation = default);
    Task<StudioAudioCapabilities> GetCapabilitiesAsync(StudioAudioSettings settings, CancellationToken cancellation = default);
    Task<StudioAudioCapabilities> ConfigureAsync(StudioAudioSettings settings, CancellationToken cancellation = default);
    Task ShowAsioControlPanelAsync(StudioAudioSettings settings, CancellationToken cancellation = default);
    Task ReleaseOutputAsync(CancellationToken cancellation = default);
}

public interface IStudioTransport
{
    StudioTransportSnapshot State { get; }
    Task ApplySnapshotAsync(StudioSnapshot snapshot, CancellationToken cancellation = default);
    Task ConfigureMetronomeAsync(StudioMetronomeSettings settings, bool resetPhase = false, CancellationToken cancellation = default);
    Task PlayAsync(CancellationToken cancellation = default);
    Task PauseAsync(CancellationToken cancellation = default);
    Task StopAsync(CancellationToken cancellation = default);
    Task SeekAsync(double position, CancellationToken cancellation = default);
    Task PlayPreviewAsync(StudioPreviewAudio preview, CancellationToken cancellation = default);
    Task StopPreviewAsync(CancellationToken cancellation = default);
    Task SetPreviewVolumeAsync(double volumeDb, CancellationToken cancellation = default);
}

public interface IStudioCapture
{
    Task StartRecordingAsync(StudioRecordingRequest request, CancellationToken cancellation = default);
    Task<IReadOnlyList<StudioRecordedSegment>> FinishRecordingAsync(CancellationToken cancellation = default);
}

public interface IStudioMediaAnalyzer
{
    Task<StudioMediaInfo> AnalyzeAsync(string path, CancellationToken cancellation = default);
}

public interface IStudioExporter
{
    Task ExportAsync(StudioSnapshot snapshot, string destination, StudioExportOptions options,
        IProgress<double>? progress = null, CancellationToken cancellation = default);
}

public interface IStudioAudioEngine : IStudioDevices, IStudioTransport, IStudioCapture,
    IStudioMediaAnalyzer, IStudioExporter, IDisposable, IAsyncDisposable;
