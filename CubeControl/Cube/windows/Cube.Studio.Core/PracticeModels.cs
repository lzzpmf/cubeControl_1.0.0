using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cube.Studio.Core;

/// <summary>An immutable audio source with an untrimmed reference-time origin and frame-accurate audible limits.</summary>
public class PracticeSource : AudioSourceModel
{
    private string _name = "Áudio";
    private double _start;
    private long _trimStartFrame;
    private long? _trimEndFrame;
    public PracticeSource() => PropertyChanged += SourceChanged;
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get => _name; set => Set(ref _name, string.IsNullOrWhiteSpace(value) ? "Áudio" : value.Trim()); }
    /// <summary>Position of source frame zero, before trimming, in reference seconds.</summary>
    public double Start { get => _start; set => Set(ref _start, Bounded(value, -86400, 86400)); }
    public long TrimStartFrame => Math.Clamp(_trimStartFrame, 0, Math.Max(0, TrimEndFrame - 1));
    public long TrimEndFrame => Math.Clamp(_trimEndFrame ?? SourceFrameCount, 0, SourceFrameCount);
    public double AudibleStart => Start + FramesToReferenceSeconds(TrimStartFrame);
    public double End => Start + FramesToReferenceSeconds(TrimEndFrame);
    public double Duration => FramesToReferenceSeconds(TrimEndFrame - TrimStartFrame);
    public double SourceOffsetSeconds => SourceSampleRate > 0 ? TrimStartFrame / (double)SourceSampleRate : 0;
    public double SourceTimeAt(double referenceTime) => (referenceTime - Start) / (RecordedSpeed * ClockRatio);
    public long SourceFrameAt(double referenceTime) => (long)Math.Round(SourceTimeAt(referenceTime) * SourceSampleRate);
    public double FramesToReferenceSeconds(long frames) => SourceSampleRate > 0 ? frames / (double)SourceSampleRate * RecordedSpeed * ClockRatio : 0;
    internal void SetTrimFrames(long startFrame, long endFrame)
    {
        if (startFrame < 0 || endFrame <= startFrame || endFrame > SourceFrameCount) throw new ArgumentOutOfRangeException(nameof(endFrame), "Limites de corte fora da fonte de áudio.");
        _trimStartFrame = startFrame; _trimEndFrame = endFrame;
        RaiseGeometry();
    }
    private void SourceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Start) or nameof(SourceSampleRate) or nameof(SourceFrameCount) or nameof(RecordedSpeed) or nameof(ClockRatio)) RaiseGeometry();
    }
    private void RaiseGeometry()
    {
        foreach (var name in new[] { nameof(TrimStartFrame), nameof(TrimEndFrame), nameof(AudibleStart), nameof(End), nameof(Duration), nameof(SourceOffsetSeconds) }) OnPropertyChanged(name);
    }
}

public sealed class PracticeAttempt : PracticeSource
{
    private long _contentRevision;
    private readonly HashSet<long> _exportedRevisions = [];
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public long ContentRevision { get => _contentRevision; internal set { if (Set(ref _contentRevision, value)) ExportStateChanged(); } }
    public bool Exported => _exportedRevisions.Contains(ContentRevision);
    public bool HasUnexportedChanges => !Exported;
    internal bool MarkExported(long revision)
    {
        if (revision != ContentRevision) return false;
        if (_exportedRevisions.Add(revision)) ExportStateChanged();
        return true;
    }
    private void ExportStateChanged() { OnPropertyChanged(nameof(Exported)); OnPropertyChanged(nameof(HasUnexportedChanges)); }
}

public sealed class PracticeMix : ObservableObject
{
    private double _volumeDb, _pan;
    private bool _muted, _solo;
    public double VolumeDb { get => _volumeDb; set => Set(ref _volumeDb, Bounded(value, -61, 6)); }
    public double Pan { get => _pan; set => Set(ref _pan, Bounded(value, -1, 1)); }
    public bool Muted { get => _muted; set => Set(ref _muted, value); }
    public bool Solo { get => _solo; set => Set(ref _solo, value); }
    internal void Reset() { VolumeDb = 0; Pan = 0; Muted = false; Solo = false; }
}

/// <summary>One backing and independent performances for this application run. No project persistence.</summary>
public sealed class PracticeSession : ObservableObject
{
    private PracticeSource? _backing;
    private Guid? _selectedAttemptId;
    private double _speedPercent = 100, _position, _loopStart, _loopEnd;
    private int _semitones;
    private bool _loopEnabled;
    private readonly HashSet<PracticeAttempt> _observed = [];
    public PracticeSession() => Attempts.CollectionChanged += AttemptsChanged;
    public PracticeSource? Backing
    {
        get => _backing;
        internal set
        {
            if (ReferenceEquals(_backing, value)) return;
            if (_backing != null) _backing.PropertyChanged -= SourceChanged;
            _backing = value;
            if (_backing != null) _backing.PropertyChanged += SourceChanged;
            OnPropertyChanged(); OnPropertyChanged(nameof(Duration));
        }
    }
    public ObservableCollection<PracticeAttempt> Attempts { get; } = [];
    public Guid? SelectedAttemptId
    {
        get => _selectedAttemptId;
        set { if (Set(ref _selectedAttemptId, value is { } id && Attempts.Any(a => a.Id == id) ? id : null)) OnPropertyChanged(nameof(SelectedAttempt)); }
    }
    public PracticeAttempt? SelectedAttempt => Attempts.FirstOrDefault(a => a.Id == SelectedAttemptId);
    public double SpeedPercent { get => _speedPercent; set { if (Set(ref _speedPercent, Bounded(value, 25, 200, 100))) OnPropertyChanged(nameof(Speed)); } }
    public double Speed => SpeedPercent / 100;
    public int Semitones { get => _semitones; set => Set(ref _semitones, Math.Clamp(value, -12, 12)); }
    public double Position { get => _position; set => Set(ref _position, Bounded(value, 0, 86400)); }
    public bool LoopEnabled { get => _loopEnabled; set => Set(ref _loopEnabled, value); }
    public double LoopStart { get => _loopStart; set => Set(ref _loopStart, Bounded(value, 0, 86400)); }
    public double LoopEnd { get => _loopEnd; set => Set(ref _loopEnd, Bounded(value, 0, 86400)); }
    public PracticeMix BackingMix { get; } = new();
    public PracticeMix AttemptMix { get; } = new();
    public double Duration => Math.Max(Backing?.End ?? 0, Attempts.Select(a => a.End).DefaultIfEmpty(0).Max());
    private void AttemptsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var old in _observed.Where(a => !Attempts.Contains(a)).ToArray()) { old.PropertyChanged -= SourceChanged; _observed.Remove(old); }
        foreach (var added in Attempts.Where(a => !_observed.Contains(a))) { added.PropertyChanged += SourceChanged; _observed.Add(added); }
        if (SelectedAttemptId is { } id && Attempts.All(a => a.Id != id)) SelectedAttemptId = null;
        OnPropertyChanged(nameof(Duration)); OnPropertyChanged(nameof(SelectedAttempt));
    }
    private void SourceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PracticeSource.End)) OnPropertyChanged(nameof(Duration));
    }
}
