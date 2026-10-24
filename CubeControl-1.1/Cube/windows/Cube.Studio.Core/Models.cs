using System.Collections.ObjectModel;

namespace Cube.Studio.Core;

/// <summary>Original immutable audio is shared by edits and takes. Durations on clips are session-reference seconds.</summary>
public abstract class AudioSourceModel : ObservableObject
{
    private Guid _mediaId = Guid.NewGuid();
    private string? _sourcePath;
    private int _sourceSampleRate, _sourceChannels, _recordedSemitones;
    private long _sourceFrameCount;
    private bool _ownsSource;
    private double _recordedSpeed = 1, _clockRatio = 1;
    public Guid MediaId { get => _mediaId; set => Set(ref _mediaId, value); }
    public string? SourcePath { get => _sourcePath; set => Set(ref _sourcePath, value); }
    public int SourceSampleRate { get => _sourceSampleRate; set => Set(ref _sourceSampleRate, Math.Max(0, value)); }
    public int SourceChannels { get => _sourceChannels; set => Set(ref _sourceChannels, Math.Max(0, value)); }
    public long SourceFrameCount { get => _sourceFrameCount; set => Set(ref _sourceFrameCount, Math.Max(0, value)); }
    public bool OwnsSource { get => _ownsSource; set => Set(ref _ownsSource, value); }
    public double RecordedSpeed { get => _recordedSpeed; set => Set(ref _recordedSpeed, Bounded(value, .1, 10, 1)); }
    /// <summary>Session wall seconds represented by one captured source second; compensates independent device clocks.</summary>
    public double ClockRatio { get => _clockRatio; set => Set(ref _clockRatio, Bounded(value, .5, 2, 1)); }
    public int RecordedSemitones { get => _recordedSemitones; set => Set(ref _recordedSemitones, Math.Clamp(value, -12, 12)); }
    public double AudioDuration => SourceSampleRate > 0 ? SourceFrameCount / (double)SourceSampleRate : 0;
    public bool IsMissing => string.IsNullOrWhiteSpace(SourcePath) || !File.Exists(SourcePath);
    public double PlaybackSpeed(double projectSpeed) => projectSpeed / (RecordedSpeed * ClockRatio);
    public int PlaybackSemitones(int projectSemitones) => projectSemitones - RecordedSemitones;
    public void CopySourceTo(AudioSourceModel destination)
    {
        destination.MediaId = MediaId; destination.SourcePath = SourcePath;
        destination.SourceSampleRate = SourceSampleRate; destination.SourceChannels = SourceChannels;
        destination.SourceFrameCount = SourceFrameCount; destination.OwnsSource = OwnsSource;
        destination.RecordedSpeed = RecordedSpeed; destination.RecordedSemitones = RecordedSemitones;
        destination.ClockRatio = ClockRatio;
    }
}

/// <summary>FrameCount is captured audio frames; TimelineStart and the optional source offset use different clocks explicitly.</summary>
public sealed record CaptureResult(string FilePath, int SampleRate, int Channels, long FrameCount,
    double RecordedSpeed = 1, int RecordedSemitones = 0, double TimelineStart = 0, long SourceOffsetFrames = 0, double ClockRatio = 1)
{
    public double Duration => (FrameCount - SourceOffsetFrames) / (double)SampleRate * RecordedSpeed * ClockRatio;
}

public sealed class TakeModel : AudioSourceModel
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Take 1";
    private int _seed = 1;
    private double _duration = 10;
    public Guid Id { get => _id; set => Set(ref _id, value); }
    public string Name { get => _name; set => Set(ref _name, string.IsNullOrWhiteSpace(value) ? "Take" : value.Trim()); }
    public int Seed { get => _seed; set => Set(ref _seed, value); }
    public double Duration { get => _duration; set => Set(ref _duration, Bounded(value, ClipModel.MinimumDuration, 86400, 10)); }
}

public sealed class ClipModel : AudioSourceModel
{
    public const double MinimumDuration = 0.15;
    private Guid _id = Guid.NewGuid();
    private string _name = "Clipe";
    private double _start, _duration = 10, _sourceOffset, _sourceDuration = 80;
    private int _seed = 1, _activeTakeIndex;
    private bool _isSelected;

    public Guid Id { get => _id; set => Set(ref _id, value); }
    public string Name { get => _name; set => Set(ref _name, string.IsNullOrWhiteSpace(value) ? "Clipe" : value.Trim()); }
    public double Start { get => _start; set { if (Set(ref _start, Bounded(value, 0, 86400))) OnPropertyChanged(nameof(End)); } }
    public double Duration { get => _duration; set { if (Set(ref _duration, Bounded(value, MinimumDuration, 86400, 10))) OnPropertyChanged(nameof(End)); } }
    public double End => Start + Duration;
    public double SourceOffset { get => _sourceOffset; set => Set(ref _sourceOffset, Bounded(value, 0, 86400)); }
    public double SourceDuration { get => _sourceDuration; set => Set(ref _sourceDuration, Bounded(value, MinimumDuration, 86400, 80)); }
    public int Seed { get => Takes.Count > 0 && ActiveTakeIndex < Takes.Count ? Takes[ActiveTakeIndex].Seed : _seed; set => Set(ref _seed, value); }
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
    public ObservableCollection<TakeModel> Takes { get; } = [];
    public int ActiveTakeIndex
    {
        get => _activeTakeIndex;
        set
        {
            if (!Set(ref _activeTakeIndex, Math.Clamp(value, 0, Math.Max(0, Takes.Count - 1)))) return;
            OnPropertyChanged(nameof(Seed));
            OnPropertyChanged(nameof(ActiveTake));
        }
    }
    public TakeModel? ActiveTake => Takes.Count == 0 ? null : Takes[Math.Clamp(ActiveTakeIndex, 0, Takes.Count - 1)];
    public AudioSourceModel ActiveSource => ActiveTake ?? (AudioSourceModel)this;
    public double SourceTimeAt(double timelineTime) => (SourceOffset + timelineTime - Start) / (ActiveSource.RecordedSpeed * ActiveSource.ClockRatio);
    public long SourceFrameAt(double timelineTime) => (long)Math.Round(SourceTimeAt(timelineTime) * ActiveSource.SourceSampleRate);
}

public sealed class TrackModel : ObservableObject
{
    public static IReadOnlyList<string> AvailableInputs { get; } =
        new[] { "Nenhuma", "Entrada 1", "Entrada 2", "Estéreo 1+2" };
    private Guid _id = Guid.NewGuid();
    private string _name = "Track", _inputChannel = "Nenhuma";
    private bool _isSolo, _isMuted, _isArmed, _isEffectivelyMuted;
    private double _volumeDb, _pan, _heightWeight = 1;
    public Guid Id { get => _id; set => Set(ref _id, value); }
    public string Name { get => _name; set => Set(ref _name, string.IsNullOrWhiteSpace(value) ? "Track" : value.Trim()); }
    public bool IsSolo { get => _isSolo; set => Set(ref _isSolo, value); }
    public bool IsMuted { get => _isMuted; set => Set(ref _isMuted, value); }
    public bool IsArmed { get => _isArmed; set => Set(ref _isArmed, value); }
    public bool IsEffectivelyMuted { get => _isEffectivelyMuted; internal set => Set(ref _isEffectivelyMuted, value); }
    public double VolumeDb { get => _volumeDb; set => Set(ref _volumeDb, Bounded(value, -61, 6)); }
    public double Pan { get => _pan; set => Set(ref _pan, Bounded(value, -100, 100)); }
    public double HeightWeight { get => _heightWeight; set => Set(ref _heightWeight, Bounded(value, 0.5, 2, 1)); }
    public string InputChannel { get => _inputChannel; set => Set(ref _inputChannel, string.IsNullOrWhiteSpace(value) ? "Nenhuma" : value.Trim()); }
    public ObservableCollection<ClipModel> Clips { get; } = [];
}

public sealed class ProjectModel : ObservableObject
{
    public static IReadOnlyList<string> AvailableMeters { get; } = new[] { "4/4", "3/4", "6/8" };
    public static IReadOnlyList<string> AvailableMetronomeSounds { get; } = new[] { "Click", "Madeira", "Bloco", "Beep" };
    private string _name = "Nova sessão", _meter = "4/4", _metronomeSound = "Click";
    private double _bpm = 100, _referenceBpm = 100, _position, _duration = 80;
    private double _loopStart, _loopEnd = 16, _metronomeVolumeDb = -12;
    private int _semitones;
    private bool _metronomeEnabled, _loopEnabled, _snapEnabled = true, _referenceLocked;
    private Guid? _selectedTrackId, _selectedClipId;

    public string Name { get => _name; set => Set(ref _name, string.IsNullOrWhiteSpace(value) ? "Sem título" : value.Trim()); }
    public double Bpm
    {
        get => _bpm;
        set { if (Set(ref _bpm, Math.Round(Bounded(value, 30, 300, 100), 1))) OnPropertyChanged(nameof(SpeedPercent)); }
    }
    // Original tempo of the reference playback; this is metadata, not a second project clock.
    public double ReferenceBpm
    {
        get => _referenceBpm;
        set { if (!ReferenceLocked && Set(ref _referenceBpm, Bounded(value, 30, 300, 100))) OnPropertyChanged(nameof(SpeedPercent)); }
    }
    public bool ReferenceLocked { get => _referenceLocked; internal set => Set(ref _referenceLocked, value); }
    public double SpeedPercent { get => Bpm / ReferenceBpm * 100; set => Bpm = ReferenceBpm * value / 100; }
    public int Semitones { get => _semitones; set => Set(ref _semitones, Math.Clamp(value, -12, 12)); }
    public string Meter { get => _meter; set => Set(ref _meter, AvailableMeters.Contains(value) ? value : "4/4"); }
    public bool MetronomeEnabled { get => _metronomeEnabled; set => Set(ref _metronomeEnabled, value); }
    public double MetronomeVolumeDb { get => _metronomeVolumeDb; set => Set(ref _metronomeVolumeDb, Bounded(value, -61, 6)); }
    public string MetronomeSound { get => _metronomeSound; set => Set(ref _metronomeSound, AvailableMetronomeSounds.Contains(value) ? value : "Click"); }
    public bool LoopEnabled { get => _loopEnabled; set => Set(ref _loopEnabled, value); }
    public bool SnapEnabled { get => _snapEnabled; set => Set(ref _snapEnabled, value); }
    public double LoopStart { get => _loopStart; set => Set(ref _loopStart, Bounded(value, 0, Math.Max(0, Duration - ClipModel.MinimumDuration))); }
    public double LoopEnd { get => _loopEnd; set => Set(ref _loopEnd, Bounded(value, Math.Min(Duration, LoopStart + ClipModel.MinimumDuration), Duration, Duration)); }
    public double Position { get => _position; set => Set(ref _position, Bounded(value, 0, Duration)); }
    public double Duration { get => _duration; set => Set(ref _duration, Bounded(value, 1, 86400, 80)); }
    public Guid? SelectedTrackId { get => _selectedTrackId; set => Set(ref _selectedTrackId, value); }
    public Guid? SelectedClipId { get => _selectedClipId; set => Set(ref _selectedClipId, value); }
    public ObservableCollection<TrackModel> Tracks { get; } = [];
}

public sealed record CrossfadeRegion(Guid FirstClipId, Guid SecondClipId, double Start, double Duration)
{
    public double End => Start + Duration;
}
