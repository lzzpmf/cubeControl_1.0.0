using System.Collections.Specialized;
using System.ComponentModel;

namespace Cube.Studio.Core;

/// <summary>Non-destructive practice editing. Deleted sources are retained on disk for undo.</summary>
public sealed class PracticeEditor : ObservableObject
{
    private sealed record EditStep(Action Undo, Action Redo);
    private readonly Stack<EditStep> _undo = [], _redo = [];
    private readonly HashSet<PracticeSource> _observed = [];
    private long _revision, _nextContentRevision;
    public PracticeEditor() : this(new PracticeSession()) { }
    public PracticeEditor(PracticeSession session)
    {
        Session = session;
        _nextContentRevision = session.Attempts.Select(a => a.ContentRevision).DefaultIfEmpty(0).Max();
        Session.PropertyChanged += SessionChanged;
        Session.Attempts.CollectionChanged += AttemptsChanged;
        Session.BackingMix.PropertyChanged += MixChanged;
        Session.AttemptMix.PropertyChanged += MixChanged;
        Rewire();
    }
    public PracticeSession Session { get; }
    public long Revision { get => _revision; private set => Set(ref _revision, value); }
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public bool HasUnexportedAttempts => Session.Attempts.Any(a => a.HasUnexportedChanges);
    public void SetBacking(PracticeSource? source, bool clearAttempts = false)
    {
        if (source != null) ValidateSource(source);
        if (clearAttempts) Clear();
        Session.Backing = source;
        Session.Position = 0; Session.LoopStart = 0; Session.LoopEnd = source?.End ?? 0; Session.LoopEnabled = false;
        _undo.Clear(); _redo.Clear(); Rewire(); HistoryChanged();
    }
    public PracticeAttempt AddAttempt(CaptureResult capture, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (capture.SampleRate is < 8000 or > 384000 || capture.Channels is < 1 or > 64 || capture.FrameCount <= capture.SourceOffsetFrames
            || capture.SourceOffsetFrames < 0 || !double.IsFinite(capture.RecordedSpeed) || capture.RecordedSpeed is < .1 or > 10
            || !double.IsFinite(capture.ClockRatio) || capture.ClockRatio is < .5 or > 2 || capture.RecordedSemitones is < -12 or > 12
            || !double.IsFinite(capture.TimelineStart) || capture.TimelineStart < 0 || capture.TimelineStart + capture.Duration > 86400)
            throw new ArgumentException("Metadados de captura inválidos.", nameof(capture));
        if (!File.Exists(capture.FilePath)) throw new FileNotFoundException("Arquivo da captura não encontrado.", capture.FilePath);
        var attempt = new PracticeAttempt
        {
            Name = name ?? "Tentativa " + (Session.Attempts.Count + 1), SourcePath = Path.GetFullPath(capture.FilePath),
            SourceSampleRate = capture.SampleRate, SourceChannels = capture.Channels, SourceFrameCount = capture.FrameCount,
            OwnsSource = true, RecordedSpeed = capture.RecordedSpeed, RecordedSemitones = capture.RecordedSemitones,
            ClockRatio = capture.ClockRatio, Start = capture.TimelineStart - capture.SourceOffsetFrames / (double)capture.SampleRate * capture.RecordedSpeed * capture.ClockRatio,
            ContentRevision = ++_nextContentRevision
        };
        attempt.SetTrimFrames(capture.SourceOffsetFrames, capture.FrameCount);
        Session.Attempts.Add(attempt); Session.SelectedAttemptId = attempt.Id;
        _redo.Clear(); HistoryChanged();
        return attempt;
    }
    public void SelectAttempt(Guid? id) => Session.SelectedAttemptId = id;
    public void TrimAttempt(Guid id, long startFrame, long endFrame)
    {
        var attempt = Find(id);
        long previousStart = attempt.TrimStartFrame, previousEnd = attempt.TrimEndFrame, previousRevision = attempt.ContentRevision;
        if (previousStart == startFrame && previousEnd == endFrame) return;
        attempt.SetTrimFrames(startFrame, endFrame);
        long nextRevision = ++_nextContentRevision; attempt.ContentRevision = nextRevision;
        Push(new(() => { attempt.SetTrimFrames(previousStart, previousEnd); attempt.ContentRevision = previousRevision; },
            () => { attempt.SetTrimFrames(startFrame, endFrame); attempt.ContentRevision = nextRevision; }));
    }
    public void DeleteAttempt(Guid id)
    {
        var attempt = Find(id); int index = Session.Attempts.IndexOf(attempt); Guid? selected = Session.SelectedAttemptId;
        void Remove() { Session.Attempts.Remove(attempt); if (selected == id) Session.SelectedAttemptId = Session.Attempts.ElementAtOrDefault(Math.Min(index, Session.Attempts.Count - 1))?.Id; }
        Remove();
        Push(new(() => { Session.Attempts.Insert(Math.Min(index, Session.Attempts.Count), attempt); Session.SelectedAttemptId = selected; }, Remove));
    }
    public bool MarkAttemptExported(Guid id, long? revision = null)
    {
        var attempt = Session.Attempts.FirstOrDefault(a => a.Id == id);
        return attempt != null && attempt.MarkExported(revision ?? attempt.ContentRevision);
    }
    public void Undo() { if (_undo.TryPop(out var edit)) { edit.Undo(); _redo.Push(edit); HistoryChanged(); } }
    public void Redo() { if (_redo.TryPop(out var edit)) { edit.Redo(); _undo.Push(edit); HistoryChanged(); } }
    public void Clear()
    {
        Session.Backing = null; Session.Attempts.Clear(); Session.SelectedAttemptId = null;
        Session.SpeedPercent = 100; Session.Semitones = 0; Session.Position = Session.LoopStart = Session.LoopEnd = 0; Session.LoopEnabled = false;
        Session.BackingMix.Reset(); Session.AttemptMix.Reset(); _undo.Clear(); _redo.Clear(); Rewire(); HistoryChanged();
    }
    private PracticeAttempt Find(Guid id) => Session.Attempts.FirstOrDefault(a => a.Id == id) ?? throw new ArgumentException("Tentativa não pertence à prática.", nameof(id));
    private static void ValidateSource(PracticeSource source)
    {
        if (source.SourceSampleRate is < 8000 or > 384000 || source.SourceChannels is < 1 or > 64 || source.SourceFrameCount <= 0 || source.TrimEndFrame <= source.TrimStartFrame || source.AudibleStart < 0 || source.End > 86400)
            throw new ArgumentException("Metadados de mídia inválidos.", nameof(source));
        if (source.IsMissing) throw new FileNotFoundException("Arquivo de áudio não encontrado.", source.SourcePath);
    }
    private void Push(EditStep edit) { _undo.Push(edit); _redo.Clear(); HistoryChanged(); }
    private void HistoryChanged() { OnPropertyChanged(nameof(CanUndo)); OnPropertyChanged(nameof(CanRedo)); OnPropertyChanged(nameof(HasUnexportedAttempts)); }
    private void SessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PracticeSession.Backing)) Rewire();
        if (e.PropertyName is not nameof(PracticeSession.Position) and not nameof(PracticeSession.SelectedAttemptId) and not nameof(PracticeSession.SelectedAttempt)) Revision++;
    }
    private void MixChanged(object? sender, PropertyChangedEventArgs e) => Revision++;
    private void AttemptsChanged(object? sender, NotifyCollectionChangedEventArgs e) { Rewire(); Revision++; HistoryChanged(); }
    private void SourceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PracticeAttempt.Exported) or nameof(PracticeAttempt.HasUnexportedChanges)) OnPropertyChanged(nameof(HasUnexportedAttempts));
        else Revision++;
    }
    private void Rewire()
    {
        var sources = Session.Attempts.Cast<PracticeSource>().Concat(Session.Backing is { } backing ? [backing] : []).ToHashSet();
        foreach (var old in _observed.Where(s => !sources.Contains(s)).ToArray()) { old.PropertyChanged -= SourceChanged; _observed.Remove(old); }
        foreach (var added in sources.Where(s => !_observed.Contains(s))) { added.PropertyChanged += SourceChanged; _observed.Add(added); }
    }
}
