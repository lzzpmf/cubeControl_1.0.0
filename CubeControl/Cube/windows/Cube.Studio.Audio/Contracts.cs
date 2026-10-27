namespace Cube.Studio.Audio;

public enum StudioDriver { Wasapi, Asio }
public enum StudioExportFormat { Wav24, Mp3 }

public sealed record StudioDevice(string Id, string Name, StudioDriver Driver, bool IsInput, int Channels);
public sealed record StudioAudioSettings
{
    public StudioDriver Driver { get; init; } = StudioDriver.Wasapi;
    public string OutputId { get; init; } = "";
    public string? InputId { get; init; }
    public int SampleRate { get; init; } = 48000;
    public int BufferFrames { get; init; } = 256;
    public int InputChannel { get; init; }
    public int InputChannels { get; init; } = 1;
    public int OutputChannel { get; init; }
    public bool Monitoring { get; init; }
    public bool Exclusive { get; init; }
    public double RecordingOffsetMilliseconds { get; init; }
}

// Clip positions/durations use reference-timeline seconds; SourceOffset uses file seconds.
public sealed record StudioClip
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Path { get; init; } = "";
    public double Start { get; init; }
    public double Duration { get; init; }
    public double SourceOffset { get; init; }
    public double RecordedSpeed { get; init; } = 1;
    public double RecordedPitch { get; init; }
    public double ClockRatio { get; init; } = 1;
    public double FadeIn { get; init; }
    public double FadeOut { get; init; }
}

public sealed record StudioTrack
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public double VolumeDb { get; init; }
    /// <summary>Pan from -1 (left) through 0 (centre) to +1 (right).</summary>
    public double Pan { get; init; }
    public bool Muted { get; init; }
    public bool Solo { get; init; }
    public IReadOnlyList<StudioClip> Clips { get; init; } = [];
}

public sealed record StudioSnapshot
{
    public double PlaybackRate { get; init; } = 1;
    public double ReferenceBpm { get; init; } = 100;
    public double Bpm { get; init; } = 100;
    public int Semitones { get; init; }
    public double Duration { get; init; } = 80;
    public bool LoopEnabled { get; init; }
    public double LoopStart { get; init; }
    public double LoopEnd { get; init; } = 80;
    public bool MetronomeEnabled { get; init; }
    public string Meter { get; init; } = "4/4";
    public string MetronomeSound { get; init; } = "Click";
    public double MetronomeVolumeDb { get; init; } = -12;
    public IReadOnlyList<StudioTrack> Tracks { get; init; } = [];
    public double Speed => PlaybackRate;
}

public sealed record StudioMetronomeSettings(bool Enabled = false, double Bpm = 100,
    string Meter = "4/4", string Sound = "Click", double VolumeDb = -12);

public sealed record StudioTransportSnapshot(bool IsPlaying, bool IsRecording, double Position,
    double Duration, float OutputPeak = 0, float InputPeak = 0, long LoopPass = 0, string? Error = null,
    bool IsMetronomeRunning = false, bool IsMetronomePulse = false,
    bool IsPreviewPlaying = false, double PreviewPosition = 0, double PreviewDuration = 0);

public sealed record StudioPreviewAudio(byte[] Pcm16, int SampleRate, int Channels, double VolumeDb = 0);

public sealed record StudioMediaInfo(string Path, double Duration, int SampleRate, int Channels,
    float[] Peaks, double? Bpm = null, string? Key = null, double TempoConfidence = 0, double KeyConfidence = 0);

public sealed record StudioRecordingRequest
{
    public Guid TrackId { get; init; }
    public string? Directory { get; init; }
    public int InputChannel { get; init; }
    public int InputChannels { get; init; } = 1;
}

public sealed record StudioRecordedSegment(Guid TrackId, string Path, double Start, double Duration,
    double SourceDuration, double RecordedSpeed, double RecordedPitch, double ClockRatio,
    long LoopPass, bool Incomplete = false, float[]? Peaks = null);

public sealed record StudioExportOptions
{
    public StudioExportFormat Format { get; init; } = StudioExportFormat.Wav24;
    public double Start { get; init; }
    public double? End { get; init; }
    public int SampleRate { get; init; } = 48000;
    public bool IncludeMetronome { get; init; }
}

public sealed record StudioAudioCapabilities(int[] SampleRates, int[] BufferFrames,
    int InputChannels, int OutputChannels, int EffectiveSampleRate, int EffectiveBufferFrames);
