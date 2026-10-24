using System.Collections.Specialized;
using System.ComponentModel;

namespace Cube.Studio.Core;

/// <summary>Non-destructive session editing. Audio capture/rendering is performed by the host.</summary>
public sealed class ProjectEditor : ObservableObject
{
    private readonly List<string> _undo = [], _redo = [];
    private readonly List<INotifyPropertyChanged> _observedObjects = [];
    private readonly List<INotifyCollectionChanged> _observedCollections = [];
    private ProjectModel _project;
    private string _savedSnapshot = "", _lastSnapshot = "";
    private string? _editBefore;
    private int _editDepth;
    private bool _isDirty, _isRecording, _suppress, _normalizing;
    private Guid? _recordTrackId, _recordClipId;
    private double _recordStart;
    private long _revision;
    private bool _saving;

    public ProjectEditor() : this(CreateEmptyProject()) { }
    public ProjectEditor(ProjectModel project)
    {
        _project = project;
        _project.ReferenceLocked |= _project.Tracks.Any(track => track.Clips.Count > 0);
        Rewire();
        NormalizeArmAndMute();
        _savedSnapshot = _lastSnapshot = Snapshot();
    }
    public ProjectModel Project => _project;
    public bool IsDirty { get => _isDirty; private set => Set(ref _isDirty, value); }
    public bool IsRecording { get => _isRecording; private set => Set(ref _isRecording, value); }
    public bool CanUndo => _undo.Count > 0 && !IsRecording;
    public bool CanRedo => _redo.Count > 0 && !IsRecording;
    public ClipModel? SelectedClip => Project.Tracks.SelectMany(t => t.Clips).FirstOrDefault(c => c.Id == Project.SelectedClipId);
    public TrackModel? SelectedTrack => Project.Tracks.FirstOrDefault(t => t.Id == Project.SelectedTrackId);
    public string? FilePath { get; private set; }
    public long Revision { get => _revision; private set => Set(ref _revision, value); }
    public IReadOnlyList<AudioSourceModel> MissingSources => EnumerateSources().Where(source => source.IsMissing).ToArray();
    public IEnumerable<AudioSourceModel> EnumerateSources() => Project.Tracks.SelectMany(track => track.Clips)
        .SelectMany(clip => clip.Takes.Count == 0 ? new AudioSourceModel[] { clip } : clip.Takes.Cast<AudioSourceModel>());
    public bool TrySetReferenceBpm(double bpm)
    {
        if (Project.ReferenceLocked || !double.IsFinite(bpm) || bpm is < 30 or > 300) return false;
        Edit(() => { Project.ReferenceBpm = bpm; Project.Bpm = bpm; });
        return true;
    }
    public bool RelinkMedia(Guid mediaId, string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) return false;
        var sources = Project.Tracks.SelectMany(track => track.Clips)
            .SelectMany(clip => new AudioSourceModel[] { clip }.Concat(clip.Takes)).Where(source => source.MediaId == mediaId).ToArray();
        if (sources.Length == 0) return false;
        Edit(() => { foreach (var source in sources) source.SourcePath = fullPath; });
        OnPropertyChanged(nameof(MissingSources));
        return true;
    }

    public void BeginEdit()
    {
        if (_editDepth++ == 0) _editBefore = Snapshot();
    }
    public void CommitEdit()
    {
        if (_editDepth == 0 || --_editDepth > 0) return;
        var after = Snapshot();
        if (_editBefore is not null && _editBefore != after) PushUndo(_editBefore);
        _editBefore = null;
        _lastSnapshot = after;
        IsDirty = after != _savedSnapshot;
    }
    public void CancelEdit()
    {
        var before = _editBefore;
        _editDepth = 0; _editBefore = null;
        if (before is not null) Restore(before, true);
    }
    private void Edit(Action action)
    {
        BeginEdit();
        try { action(); CommitEdit(); }
        catch { CancelEdit(); throw; }
    }

    public void SelectClip(TrackModel track, ClipModel? clip)
    {
        RequireTrack(track);
        if (clip is not null && !track.Clips.Contains(clip)) throw new ArgumentException("O clipe não pertence à track.");
        Project.SelectedTrackId = track.Id;
        Project.SelectedClipId = clip?.Id;
        foreach (var current in Project.Tracks.SelectMany(t => t.Clips)) current.IsSelected = current == clip;
        OnPropertyChanged(nameof(SelectedTrack)); OnPropertyChanged(nameof(SelectedClip));
    }

    public void SetArmed(TrackModel track, bool armed)
    {
        if (IsRecording) return;
        RequireTrack(track);
        Edit(() => { foreach (var item in Project.Tracks) item.IsArmed = armed && item == track; });
    }

    public ClipModel? SplitSelected(double? position = null)
    {
        if (IsRecording || SelectedClip is not { } clip) return null;
        var track = FindTrack(clip);
        var at = position ?? Project.Position;
        if (!double.IsFinite(at) || at - clip.Start < ClipModel.MinimumDuration || clip.End - at < ClipModel.MinimumDuration) return null;
        ClipModel? right = null;
        Edit(() =>
        {
            var leftDuration = at - clip.Start;
            right = CloneClip(clip);
            right.Start = at;
            right.Duration = clip.Duration - leftDuration;
            right.SourceOffset = clip.SourceOffset + leftDuration;
            clip.Duration = leftDuration;
            track.Clips.Insert(track.Clips.IndexOf(clip) + 1, right);
            SelectClip(track, right);
        });
        return right;
    }

    public void MoveClip(ClipModel clip, TrackModel destination, double start)
    {
        if (IsRecording) return;
        var source = FindTrack(clip);
        RequireTrack(destination);
        if (!double.IsFinite(start)) return;
        Edit(() =>
        {
            clip.Start = Math.Clamp(start, 0, Math.Max(0, 86400 - clip.Duration));
            Project.Duration = Math.Max(Project.Duration, Math.Ceiling(clip.End));
            if (source != destination) { source.Clips.Remove(clip); destination.Clips.Add(clip); }
            SelectClip(destination, clip);
        });
    }

    /// <summary>Moves the visible left/right edges while preserving the source-to-timeline mapping.</summary>
    public void TrimClip(ClipModel clip, double start, double duration)
    {
        if (IsRecording || !double.IsFinite(start) || !double.IsFinite(duration)) return;
        FindTrack(clip);
        Edit(() =>
        {
            var sourceOrigin = clip.Start - clip.SourceOffset;
            var earliest = Math.Max(0, sourceOrigin);
            var latest = Math.Min(Project.Duration, sourceOrigin + clip.SourceDuration);
            var desiredEnd = start + duration;
            var newStart = Math.Clamp(start, earliest, Math.Max(earliest, latest - ClipModel.MinimumDuration));
            var newEnd = Math.Clamp(desiredEnd, newStart + ClipModel.MinimumDuration, latest);
            clip.SourceOffset = newStart - sourceOrigin;
            clip.Start = newStart;
            clip.Duration = newEnd - newStart;
        });
    }

    public void AddClip(TrackModel track, ClipModel clip)
    {
        if (IsRecording) return;
        RequireTrack(track);
        if (Project.Tracks.SelectMany(t => t.Clips).Any(c => c.Id == clip.Id)) throw new ArgumentException("Clipe já adicionado.");
        Edit(() =>
        {
            clip.SourceDuration = Math.Max(clip.SourceDuration, clip.SourceOffset + ClipModel.MinimumDuration);
            clip.SourceOffset = Math.Clamp(clip.SourceOffset, 0, clip.SourceDuration - ClipModel.MinimumDuration);
            clip.Duration = Math.Clamp(clip.Duration, ClipModel.MinimumDuration, clip.SourceDuration - clip.SourceOffset);
            clip.Start = Math.Clamp(clip.Start, 0, 86400 - clip.Duration);
            Project.Duration = Math.Max(Project.Duration, Math.Ceiling(clip.End));
            track.Clips.Add(clip);
            Project.ReferenceLocked = true;
            SelectClip(track, clip);
        });
    }

    public bool DeleteSelected()
    {
        if (IsRecording || SelectedClip is not { } clip) return false;
        var track = FindTrack(clip);
        Edit(() => { track.Clips.Remove(clip); SelectClip(track, null); });
        return true;
    }

    public ClipModel? DuplicateSelected()
    {
        if (IsRecording || SelectedClip is not { } selected) return null;
        if (selected.End + selected.Duration > 86400) return null;
        var duplicate = CloneClip(selected);
        duplicate.Name = selected.Name + " · cópia";
        duplicate.Start = selected.End;
        AddClip(FindTrack(selected), duplicate);
        return duplicate;
    }

    public static IReadOnlyList<CrossfadeRegion> GetCrossfades(TrackModel track)
    {
        var regions = new List<CrossfadeRegion>();
        var clips = track.Clips.OrderBy(c => c.Start).ThenBy(c => c.End).ToArray();
        for (var i = 0; i < clips.Length; i++)
        for (var j = i + 1; j < clips.Length && clips[j].Start < clips[i].End; j++)
        {
            var start = Math.Max(clips[i].Start, clips[j].Start);
            var end = Math.Min(clips[i].End, clips[j].End);
            if (end > start) regions.Add(new CrossfadeRegion(clips[i].Id, clips[j].Id, start, end - start));
        }
        return regions;
    }

    public bool BeginRecording(out string error)
    {
        error = "";
        if (IsRecording) { error = "Já existe uma gravação em andamento."; return false; }
        var track = Project.Tracks.SingleOrDefault(t => t.IsArmed);
        if (track is null) { error = "Arme uma track com R antes de gravar."; return false; }
        if (track.InputChannel == "Nenhuma") { error = "Selecione a entrada da track armada."; return false; }
        if (Project.Position >= 86400 - ClipModel.MinimumDuration) { error = "A sessão atingiu o limite de 24 horas."; return false; }
        BeginEdit();
        _recordTrackId = track.Id;
        _recordStart = Project.Position;
        // Repeated passes from the same insertion point become takes of the same clip.
        _recordClipId = SelectedClip is { } selected && track.Clips.Contains(selected) && Math.Abs(selected.Start - _recordStart) < 0.05
            ? selected.Id : null;
        IsRecording = true;
        HistoryChanged();
        return true;
    }

    /// <summary>Commits a finalized real capture. A loop pass can be committed, followed by BeginRecording for the next pass.</summary>
    public ClipModel? CommitRecording(CaptureResult capture)
    {
        if (!IsRecording || _recordTrackId is not { } trackId) return null;
        try
        {
            ValidateCapture(capture);
            if (capture.Duration < ClipModel.MinimumDuration) { CancelRecording(); return null; }
            var result = AppendCapturedSource(trackId, capture, _recordClipId, false);
            IsRecording = false;
            _recordTrackId = _recordClipId = null;
            CommitEdit(); HistoryChanged();
            return result;
        }
        catch { CancelRecording(); throw; }
    }

    /// <summary>Accepts finalized loop passes from the engine, without relying on a UI timer.</summary>
    public ClipModel? AppendRecordedTake(Guid trackId, CaptureResult capture, Guid? targetClipId = null)
    {
        if (IsRecording) throw new InvalidOperationException("Finalize a transação de gravação antes de adicionar uma passagem.");
        ValidateCapture(capture);
        if (capture.Duration < ClipModel.MinimumDuration) return null;
        ClipModel? result = null;
        Edit(() => result = AppendCapturedSource(trackId, capture, targetClipId));
        return result;
    }

    private ClipModel AppendCapturedSource(Guid trackId, CaptureResult capture, Guid? targetClipId, bool matchLoopStart = true)
    {
        var track = Project.Tracks.SingleOrDefault(track => track.Id == trackId)
            ?? throw new ArgumentException("Track não pertence ao projeto.", nameof(trackId));
        var clip = targetClipId is { } id ? track.Clips.FirstOrDefault(item => item.Id == id)
            : matchLoopStart ? track.Clips.FirstOrDefault(item => item.Takes.Any(take => take.OwnsSource)
                && Math.Abs(item.Start - capture.TimelineStart) < .000001) : null;
        if (targetClipId.HasValue && clip is null) throw new ArgumentException("Clipe não pertence à track.", nameof(targetClipId));
        // Device correlation can place the first sample after the requested cursor.
        // A take sharing this clip must start on the same source-frame boundary;
        // otherwise preserve both placements in separate clips instead of shifting audio.
        var frameDuration = capture.RecordedSpeed * capture.ClockRatio / capture.SampleRate;
        if (clip is not null && Math.Abs(clip.Start - capture.TimelineStart) > frameDuration + 1e-9)
            clip = null;
        if (clip is null)
        {
            clip = new ClipModel { Name = "Gravação", Start = capture.TimelineStart };
            track.Clips.Add(clip);
        }
        else if (clip.Takes.Count == 0)
        {
            var original = new TakeModel { Name = "Take 1", Seed = clip.Seed, Duration = clip.SourceDuration };
            clip.CopySourceTo(original); clip.Takes.Add(original);
        }
        var sourceDuration = capture.FrameCount / (double)capture.SampleRate * capture.RecordedSpeed * capture.ClockRatio;
        var take = new TakeModel
        {
            Name = "Take " + (clip.Takes.Count + 1), SourcePath = Path.GetFullPath(capture.FilePath),
            SourceSampleRate = capture.SampleRate, SourceChannels = capture.Channels, SourceFrameCount = capture.FrameCount,
            RecordedSpeed = capture.RecordedSpeed, RecordedSemitones = capture.RecordedSemitones,
            ClockRatio = capture.ClockRatio,
            OwnsSource = true, Duration = sourceDuration
        };
        clip.Takes.Add(take); clip.ActiveTakeIndex = clip.Takes.Count - 1;
        clip.SourceOffset = capture.SourceOffsetFrames / (double)capture.SampleRate * capture.RecordedSpeed * capture.ClockRatio;
        clip.SourceDuration = sourceDuration;
        clip.Duration = capture.Duration;
        Project.Duration = Math.Max(Project.Duration, Math.Ceiling(clip.End));
        Project.ReferenceLocked = true;
        SelectClip(track, clip);
        return clip;
    }

    private static void ValidateCapture(CaptureResult capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (capture.SampleRate is < 8000 or > 384000 || capture.Channels is < 1 or > 64 || capture.FrameCount < 0
            || capture.SourceOffsetFrames < 0 || capture.SourceOffsetFrames > capture.FrameCount
            || !double.IsFinite(capture.RecordedSpeed) || capture.RecordedSpeed is < .1 or > 10
            || !double.IsFinite(capture.ClockRatio) || capture.ClockRatio is < .5 or > 2
            || capture.RecordedSemitones is < -12 or > 12 || !double.IsFinite(capture.TimelineStart)
            || capture.TimelineStart < 0 || capture.TimelineStart + capture.Duration > 86400
            || capture.FrameCount / (double)capture.SampleRate * capture.RecordedSpeed * capture.ClockRatio > 86400)
            throw new ArgumentException("Metadados de captura inválidos.", nameof(capture));
        if (!File.Exists(capture.FilePath)) throw new FileNotFoundException("Arquivo da captura não encontrado.", capture.FilePath);
    }

    /// <summary>Legacy demo-only compatibility. Production must call CommitRecording with real media.</summary>
    public ClipModel? FinishRecording(double endPosition)
    {
        if (!IsRecording) return null;
        var track = Project.Tracks.FirstOrDefault(t => t.Id == _recordTrackId);
        if (track is null) { CancelRecording(); return null; }
        var duration = double.IsFinite(endPosition) ? endPosition - _recordStart : 0;
        if (duration < 0 && Project.LoopEnabled && Project.LoopEnd > Project.LoopStart)
            duration = Project.LoopEnd - _recordStart + Math.Max(0, endPosition - Project.LoopStart);
        duration = Math.Clamp(duration, ClipModel.MinimumDuration, Project.Duration - _recordStart);
        var clip = track.Clips.FirstOrDefault(c => c.Id == _recordClipId);
        var created = clip is null;
        if (clip is null)
        {
            clip = new ClipModel { Name = "Gravação · demonstração", Start = _recordStart, Duration = duration, SourceDuration = duration, Seed = Random.Shared.Next(1, 10000) };
            track.Clips.Add(clip);
        }
        if (!created && clip.Takes.Count == 0) clip.Takes.Add(new TakeModel { Name = "Take 1", Seed = clip.Seed, Duration = Math.Max(clip.SourceDuration, clip.Duration) });
        clip.Takes.Add(new TakeModel { Name = "Take " + (clip.Takes.Count + 1), Seed = Random.Shared.Next(1, 10000), Duration = duration });
        clip.ActiveTakeIndex = clip.Takes.Count - 1;
        // A take is a new source beginning at this punch-in point. Longer former takes remain stored.
        clip.SourceOffset = 0;
        clip.SourceDuration = duration;
        clip.Duration = duration;
        SelectClip(track, clip);
        IsRecording = false;
        _recordTrackId = _recordClipId = null;
        CommitEdit();
        HistoryChanged();
        return clip;
    }

    public void CancelRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
        _recordTrackId = _recordClipId = null;
        CancelEdit();
        HistoryChanged();
    }

    /// <summary>Switches take and fits the visible clip to that take's available source.</summary>
    public void SelectTake(ClipModel clip, int index)
    {
        if (IsRecording || clip.Takes.Count == 0) return;
        FindTrack(clip);
        Edit(() =>
        {
            clip.ActiveTakeIndex = index;
            var take = clip.ActiveTake!;
            clip.SourceDuration = take.Duration;
            clip.SourceOffset = Math.Min(clip.SourceOffset, Math.Max(0, take.Duration - ClipModel.MinimumDuration));
            clip.Duration = Math.Min(clip.Duration, take.Duration - clip.SourceOffset);
        });
    }

    public void Undo()
    {
        if (!CanUndo) return;
        var target = _undo[^1]; _undo.RemoveAt(_undo.Count - 1);
        _redo.Add(Snapshot());
        Restore(target, true);
        HistoryChanged();
    }
    public void Redo()
    {
        if (!CanRedo) return;
        var target = _redo[^1]; _redo.RemoveAt(_redo.Count - 1);
        _undo.Add(Snapshot());
        Restore(target, true);
        HistoryChanged();
    }
    public void Save(string path)
    {
        if (_saving) throw new InvalidOperationException("Uma sessão já está sendo salva.");
        if (IsRecording) throw new InvalidOperationException("Pare a gravação antes de salvar.");
        ProjectFileStore.Deserialize(ProjectFileStore.Serialize(Project)); // Validate metadata before replacing the target.
        _suppress = true;
        try { ProjectFileStore.Save(Project, path); }
        finally { _suppress = false; }
        FilePath = Path.GetFullPath(path);
        _savedSnapshot = _lastSnapshot = Snapshot();
        IsDirty = false;
    }
    /// <summary>Captures metadata before leaving the caller context; only immutable media copying runs on the worker.</summary>
    public async Task SaveAsync(string path, CancellationToken cancellation = default)
    {
        if (_saving) throw new InvalidOperationException("Uma sessão já está sendo salva.");
        if (IsRecording) throw new InvalidOperationException("Pare a gravação antes de salvar.");
        var current = Project;
        var detached = ProjectFileStore.Deserialize(ProjectFileStore.Serialize(current));
        var originalPaths = AllSources(detached).GroupBy(source => source.MediaId)
            .ToDictionary(group => group.Key, group => group.First().SourcePath);
        _saving = true;
        try
        {
            await Task.Run(() => ProjectFileStore.Save(detached, path, cancellation), cancellation);
            if (!ReferenceEquals(current, Project)) return;
            var movedPaths = AllSources(detached).GroupBy(source => source.MediaId)
                .ToDictionary(group => group.Key, group => group.First().SourcePath);
            _suppress = true;
            try
            {
                foreach (var source in AllSources(current))
                    if (originalPaths.TryGetValue(source.MediaId, out var original) && source.SourcePath == original
                        && movedPaths.TryGetValue(source.MediaId, out var relocated)) source.SourcePath = relocated;
            }
            finally { _suppress = false; }
            FilePath = Path.GetFullPath(path);
            _savedSnapshot = ProjectFileStore.Serialize(detached, true);
            _lastSnapshot = Snapshot();
            IsDirty = _lastSnapshot != _savedSnapshot;
            Revision++;
        }
        finally { _saving = false; }
    }
    private static IEnumerable<AudioSourceModel> AllSources(ProjectModel project) => project.Tracks.SelectMany(track => track.Clips)
        .SelectMany(clip => new AudioSourceModel[] { clip }.Concat(clip.Takes));
    public void Load(string path)
    {
        var loaded = ProjectFileStore.Load(path); // Validate completely before replacing the open project.
        ReplaceProject(loaded, false);
        FilePath = Path.GetFullPath(path);
    }
    public void NewProject()
    {
        var project = CreateEmptyProject();
        ReplaceProject(project, false);
        FilePath = null;
    }
    public void LoadDemo() => ReplaceProject(CreateDemoProject(), false);

    private string Snapshot() => ProjectFileStore.Serialize(Project, true);
    private void ReplaceProject(ProjectModel project, bool preserveHistory)
    {
        _suppress = true;
        try
        {
            _project = project;
            _editBefore = null; _editDepth = 0; IsRecording = false;
            Rewire(); NormalizeArmAndMute();
            _lastSnapshot = Snapshot();
            if (!preserveHistory) { _undo.Clear(); _redo.Clear(); _savedSnapshot = _lastSnapshot; }
            IsDirty = _lastSnapshot != _savedSnapshot;
            OnPropertyChanged(nameof(Project)); OnPropertyChanged(nameof(SelectedClip)); OnPropertyChanged(nameof(SelectedTrack));
            Revision++;
            HistoryChanged();
        }
        finally { _suppress = false; }
    }
    private void Restore(string snapshot, bool preserveCursor)
    {
        var position = Project.Position;
        var selectedTrack = Project.SelectedTrackId;
        var selectedClip = Project.SelectedClipId;
        var restored = ProjectFileStore.Deserialize(snapshot);
        restored.ReferenceLocked |= Project.ReferenceLocked;
        if (preserveCursor)
        {
            restored.Position = position;
            // Undoing a move changes the selected clip's owner. Follow its stable ID
            // so keyboard edits and the take selector continue targeting that clip.
            var clipOwner = restored.Tracks.FirstOrDefault(t => t.Clips.Any(c => c.Id == selectedClip));
            restored.SelectedTrackId = clipOwner?.Id
                ?? (restored.Tracks.Any(t => t.Id == selectedTrack) ? selectedTrack : restored.Tracks[0].Id);
            restored.SelectedClipId = clipOwner is not null ? selectedClip : null;
            foreach (var clip in restored.Tracks.SelectMany(t => t.Clips)) clip.IsSelected = clip.Id == restored.SelectedClipId;
        }
        ReplaceProject(restored, true);
    }
    private void PushUndo(string before)
    {
        _undo.Add(before);
        if (_undo.Count > 100) _undo.RemoveAt(0);
        _redo.Clear();
        HistoryChanged();
    }
    private void HistoryChanged() { OnPropertyChanged(nameof(CanUndo)); OnPropertyChanged(nameof(CanRedo)); }
    private void RequireTrack(TrackModel track)
    {
        if (!Project.Tracks.Contains(track)) throw new ArgumentException("Track não pertence ao projeto.");
    }
    private TrackModel FindTrack(ClipModel clip) => Project.Tracks.FirstOrDefault(t => t.Clips.Contains(clip)) ?? throw new ArgumentException("Clipe não pertence ao projeto.");

    private void Rewire()
    {
        foreach (var item in _observedObjects) item.PropertyChanged -= ContentChanged;
        foreach (var item in _observedCollections) item.CollectionChanged -= CollectionChanged;
        _observedObjects.Clear(); _observedCollections.Clear();
        Observe(Project); ObserveCollection(Project.Tracks);
        foreach (var track in Project.Tracks)
        {
            Observe(track); ObserveCollection(track.Clips);
            foreach (var clip in track.Clips)
            {
                Observe(clip); ObserveCollection(clip.Takes);
                foreach (var take in clip.Takes) Observe(take);
            }
        }
    }
    private void Observe(INotifyPropertyChanged item) { _observedObjects.Add(item); item.PropertyChanged += ContentChanged; }
    private void ObserveCollection(INotifyCollectionChanged item) { _observedCollections.Add(item); item.CollectionChanged += CollectionChanged; }
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) { Rewire(); Changed(); }
    private void ContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppress || _normalizing) return;
        if (e.PropertyName is nameof(ProjectModel.Position) or nameof(ProjectModel.SelectedTrackId) or nameof(ProjectModel.SelectedClipId) or nameof(ProjectModel.SpeedPercent)
            or nameof(ClipModel.IsSelected) or nameof(ClipModel.End) or nameof(ClipModel.ActiveTake) or nameof(TrackModel.IsEffectivelyMuted)) return;
        NormalizeArmAndMute(sender as TrackModel);
        Changed();
    }
    private void Changed()
    {
        if (_suppress || _normalizing) return;
        Revision++;
        var current = Snapshot();
        if (current == _lastSnapshot) return;
        if (_editDepth == 0) PushUndo(_lastSnapshot);
        _lastSnapshot = current;
        IsDirty = current != _savedSnapshot;
    }
    private void NormalizeArmAndMute(TrackModel? preferred = null)
    {
        if (_normalizing) return;
        _normalizing = true;
        try
        {
            if (Project.Tracks.Any(track => track.Clips.Count > 0)) Project.ReferenceLocked = true;
            var armed = preferred is { IsArmed: true } ? preferred : Project.Tracks.FirstOrDefault(t => t.IsArmed);
            var anySolo = Project.Tracks.Any(t => t.IsSolo);
            foreach (var track in Project.Tracks)
            {
                if (track.IsArmed && track != armed) track.IsArmed = false;
                track.IsEffectivelyMuted = track.IsMuted || (anySolo && !track.IsSolo);
            }
        }
        finally { _normalizing = false; }
    }

    public static ClipModel CloneClip(ClipModel original)
    {
        var clone = new ClipModel { Name = original.Name, Start = original.Start, Duration = original.Duration, SourceOffset = original.SourceOffset, SourceDuration = original.SourceDuration, Seed = original.Seed, SourcePath = original.SourcePath };
        original.CopySourceTo(clone);
        foreach (var take in original.Takes)
        {
            var copied = new TakeModel { Name = take.Name, Seed = take.Seed, Duration = take.Duration };
            take.CopySourceTo(copied); clone.Takes.Add(copied);
        }
        clone.ActiveTakeIndex = original.ActiveTakeIndex;
        return clone;
    }

    public static ProjectModel CreateDemoProject()
    {
        var project = new ProjectModel { Name = "Estudo de guitarra", Position = 27.2, LoopStart = 18.4, LoopEnd = 52, LoopEnabled = true };
        var playback = new TrackModel { Name = "Playback", VolumeDb = -6 };
        var guitar = new TrackModel { Name = "Guitarra 1", VolumeDb = -3, InputChannel = "Entrada 1", IsArmed = true };
        var rhythm = new TrackModel { Name = "Guitarra 2", VolumeDb = -10, Pan = -25, InputChannel = "Entrada 2" };
        playback.Clips.Add(DemoClip("Blues backing · exemplo", 1.6, 76.8, 2, 1));
        guitar.Clips.Add(DemoClip("Solo · demonstração", 18.4, 29.6, 7, 3));
        guitar.Clips.Add(DemoClip("Final", 42.4, 23.2, 9, 1));
        rhythm.Clips.Add(DemoClip("Base · take 1", 18.4, 33.6, 5, 1));
        project.Tracks.Add(playback); project.Tracks.Add(guitar); project.Tracks.Add(rhythm); project.Tracks.Add(new TrackModel { Name = "Track 4" });
        project.SelectedTrackId = guitar.Id;
        project.SelectedClipId = guitar.Clips[0].Id;
        guitar.Clips[0].IsSelected = true;
        return project;
    }

    public static ProjectModel CreateEmptyProject()
    {
        var project = new ProjectModel();
        for (var i = 1; i <= 4; i++) project.Tracks.Add(new TrackModel { Name = "Track " + i });
        project.SelectedTrackId = project.Tracks[0].Id;
        return project;
    }
    private static ClipModel DemoClip(string name, double start, double duration, int seed, int takes)
    {
        var clip = new ClipModel { Name = name, Start = start, Duration = duration, SourceDuration = 80, Seed = seed };
        for (var i = 0; i < takes; i++) clip.Takes.Add(new TakeModel { Name = "Take " + (i + 1), Seed = seed + i * 2, Duration = 80 });
        clip.ActiveTakeIndex = takes - 1;
        return clip;
    }
}
