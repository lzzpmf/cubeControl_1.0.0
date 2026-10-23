using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    public HashSet<Guid> SelectedClips { get; } = [];
    public Dictionary<string, (double Duration, float[] Peaks)> ClipWaves { get; } = new(StringComparer.OrdinalIgnoreCase);
    private int clipRevision;
    public int ClipRevision => clipRevision;
    private bool snapEnabled = true;
    public bool SnapEnabled { get => snapEnabled; set { if (CanEditSession && Set(ref snapEnabled, value)) MarkProjectChanged(); } }
    public string[] SnapDivisions { get; } = ["Bar", "1/2", "1/4", "1/8", "1/16"];
    private string snapDivision = "1/4";
    public string SnapDivision { get => snapDivision; set { if (CanEditSession && SnapDivisions.Contains(value) && Set(ref snapDivision, value)) MarkProjectChanged(); } }
    public bool ExportInterval { get; set; }
    private string saveState = "Salvo automaticamente";
    public string SaveState { get => saveState; private set => Set(ref saveState, value); }
    public double TransportBpm { get => SelectedTrack != null && Bpm >= 20 ? Bpm : MetronomeBpm; set { if (SelectedTrack != null) Bpm = value; else MetronomeBpm = value; Notify(); } }
    public string SessionMeter { get => SelectedTrack?.Meter ?? "4/4"; set { if (!CanEditSession || SelectedTrack == null || !MetronomeMeters.Contains(value)) return; SelectedTrack.Meter = value; Notify(); Notify(nameof(MetronomeMeter)); Notify(nameof(CurrentBpm)); UpdateMetronome(); PersistSession(); } }
    public double SnapTime(double time, bool bypass = false)
    {
        if (!SnapEnabled || bypass || TransportBpm < 20) return Math.Max(0, time);
        var parts = SessionMeter.Split('/');
        double quarters = SnapDivision == "Bar" ? int.Parse(parts[0]) * 4d / int.Parse(parts[1]) : 4d / int.Parse(SnapDivision[2..]);
        double step = 60 / TransportBpm * quarters;
        return Math.Max(0, FirstBeatOffset + Math.Round((time - FirstBeatOffset) / step) * step);
    }
    private sealed record EditStem(Guid Id, string Source, string Name, string Color, double Volume, double Pan,
        bool Muted, bool Solo, int Input, int Channels, bool LegacyMono, BackingClip[] Clips, BackingTake[] Takes, Guid? ActiveTake);
    private sealed record EditState(EditStem[] Stems, double Master, double Bus);
    private sealed class EditHistory { public EditState? Current; public Stack<EditState> Undo = new(); public Stack<EditState> Redo = new(); }
    private readonly Dictionary<Guid, EditHistory> editHistories = [];
    private bool restoringEdit, editingGesture;
    private EditHistory? History => SelectedTrack == null ? null : editHistories.GetValueOrDefault(SelectedTrack.SessionId);
    private EditState SnapshotEdit() => new((SessionStems ?? []).Select(s => new EditStem(s.Id, s.Source, s.Name, s.Color, s.Volume, s.Pan,
        s.IsMuted, s.IsSolo, s.InputChannel, s.InputChannels, s.LegacyMono, s.Clips.ToArray(), s.SnapshotTakes(), s.ActiveTake?.Id)).ToArray(), masterVolume, Volume);
    private void InitializeHistory()
    {
        if (SelectedTrack == null) return;
        if (!editHistories.TryGetValue(SelectedTrack.SessionId, out var h)) editHistories[SelectedTrack.SessionId] = h = new();
        h.Current ??= SnapshotEdit();
    }
    private void CommitEditHistory()
    {
        if (restoringEdit || editingGesture || recordingFinalizing || IsRecording || History is not { } h) return;
        var next = SnapshotEdit();
        if (h.Current != null && JsonSerializer.Serialize(h.Current) != JsonSerializer.Serialize(next)) { h.Undo.Push(h.Current); h.Redo.Clear(); }
        h.Current = next; CommandManager.InvalidateRequerySuggested();
    }
    private Dictionary<BackingStem, BackingClip[]>? gestureSnapshot;
    public void BeginClipGesture() { CommitEditHistory(); gestureSnapshot = (SessionStems ?? []).ToDictionary(s => s, s => s.Clips.ToArray()); editingGesture = true; }
    public void CancelClipGesture()
    {
        if (gestureSnapshot != null) foreach (var (stem, clips) in gestureSnapshot) { stem.Clips.Clear(); stem.Clips.AddRange(clips); }
        gestureSnapshot = null; editingGesture = false; ClipsChanged();
    }
    public void EndClipGesture() { gestureSnapshot = null; editingGesture = false; ClipsChanged(); CommitEditHistory(); }
    public void UndoEdit(bool redo = false)
    {
        if (!CanEditSession || History is not { } h) return;
        CommitEditHistory(); var from = redo ? h.Redo : h.Undo; var to = redo ? h.Undo : h.Redo;
        if (from.Count == 0) return;
        to.Push(SnapshotEdit()); var state = from.Pop(); var routes = SessionStems!.ToDictionary(s => s.Id, s => (s.IsArmed, s.IsMonitoring)); restoringEdit = true;
        try
        {
            foreach (var stem in SessionStems!.ToArray()) DetachStem(stem);
            SessionStems!.Clear();
            foreach (var x in state.Stems)
            {
                var stem = new BackingStem { Id = x.Id, Source = x.Source, Name = x.Name, Color = x.Color, Volume = x.Volume,
                    Pan = x.Pan, IsMuted = x.Muted, IsSolo = x.Solo, InputChannel = x.Input, InputChannels = x.Channels, LegacyMono = x.LegacyMono, ClipsInitialized = true };
                foreach (var take in x.Takes) stem.Takes.Add(take);
                stem.ActiveTake = stem.Takes.FirstOrDefault(t => t.Id == x.ActiveTake);
                if (routes.TryGetValue(x.Id, out var route)) { stem.IsArmed = route.IsArmed; stem.IsMonitoring = route.IsMonitoring; }
                stem.Clips.AddRange(x.Clips); SessionStems.Add(stem); AttachStem(stem);
            }
            MasterDb = BackingMixMath.ToDb(state.Master); Volume = state.Bus;
            SelectedStem = SessionStems.FirstOrDefault(); SelectedClips.Clear(); h.Current = state;
            ClipsChanged(); Notify(nameof(MonitoredInputs)); RestartMonitorForDeviceChange(); Start(LoadSessionWaves());
        }
        finally { restoringEdit = false; }
    }
    public void SelectClip(BackingStem stem, BackingClip clip, bool additive)
    {
        SelectedStem = stem;
        if (!additive) SelectedClips.Clear();
        if (additive && SelectedClips.Contains(clip.Id)) SelectedClips.Remove(clip.Id); else SelectedClips.Add(clip.Id);
        NotifyClips();
    }
    public IEnumerable<(BackingStem Stem, BackingClip Clip)> SelectedClipItems() => (SessionStems ?? []).SelectMany(s => s.Clips.Where(c => SelectedClips.Contains(c.Id)).Select(c => (s, c)));
    private void NotifyClips() { clipRevision++; Notify(nameof(ClipRevision)); RefreshClipPresentation(); CommandManager.InvalidateRequerySuggested(); }
    private void ClipsChanged()
    {
        NotifyClips();
        if (editingGesture) return;
        RefreshSessionDuration(); PersistSession();
        Start(engine.RefreshSession(SessionSources()));
    }
    public void ReplaceClip(BackingStem stem, BackingClip clip)
    {
        if (!CanEditSession) return; clip.Validate();
        int index = stem.Clips.FindIndex(c => c.Id == clip.Id); if (index < 0) return;
        stem.Clips[index] = clip; ClipsChanged();
    }
    public void MoveClips(double delta, BackingStem? destination = null)
    {
        if (!CanEditSession || !double.IsFinite(delta) || destination != null && SessionStems?.Contains(destination) != true) return;
        var selected = SelectedClipItems().ToArray(); if (selected.Length == 0) return;
        delta = Math.Max(delta, -selected.Min(x => x.Clip.Position));
        int trackDelta = destination == null || SelectedStem == null ? 0 : SessionStems!.IndexOf(destination) - SessionStems.IndexOf(SelectedStem);
        int minimumTrack = selected.Min(x => SessionStems!.IndexOf(x.Stem)), maximumTrack = selected.Max(x => SessionStems!.IndexOf(x.Stem));
        trackDelta = Math.Clamp(trackDelta, -minimumTrack, SessionStems!.Count - 1 - maximumTrack);
        foreach (var (stem, clip) in selected) { stem.Clips.Remove(clip); SessionStems[SessionStems.IndexOf(stem) + trackDelta].Clips.Add(clip with { Position = clip.Position + delta }); }
        ClipsChanged();
    }
    public void DeleteClips()
    {
        if (!CanEditSession) return; CommitEditHistory();
        foreach (var (stem, clip) in SelectedClipItems().ToArray()) stem.Clips.Remove(clip);
        SelectedClips.Clear(); ClipsChanged(); CommitEditHistory();
    }
    public void SplitClips()
    {
        if (!CanEditSession) return; CommitEditHistory();
        foreach (var (stem, clip) in SelectedClipItems().ToArray())
        {
            if (Position <= clip.Position || Position >= clip.End) continue;
            stem.Clips.Remove(clip); var left = clip.Slice(clip.Position, Position); var right = clip.Slice(Position, clip.End);
            stem.Clips.AddRange([left, right]); SelectedClips.Remove(clip.Id); SelectedClips.Add(left.Id); SelectedClips.Add(right.Id);
        }
        ClipsChanged(); CommitEditHistory();
    }
    private (int Track, BackingClip Clip)[] clipClipboard = [];
    public void CopyClips() { var items = SelectedClipItems().ToArray(); if (items.Length == 0) return; int first = items.Min(x => SessionStems!.IndexOf(x.Stem)); clipClipboard = items.Select(x => (SessionStems!.IndexOf(x.Stem) - first, x.Clip)).ToArray(); }
    public void PasteClips(bool duplicate = false)
    {
        if (!CanEditSession || SessionStems == null || SelectedStem == null) return;
        if (duplicate) CopyClips(); if (clipClipboard.Length == 0) return;
        CommitEditHistory(); double start = clipClipboard.Min(x => x.Clip.Position);
        double delta = (duplicate ? clipClipboard.Max(x => x.Clip.End) : Position) - start;
        int baseIndex = SessionStems.IndexOf(SelectedStem); SelectedClips.Clear();
        foreach (var (track, original) in clipClipboard)
        {
            while (SessionStems.Count <= baseIndex + track) { var added = new BackingStem { Source = Path.Combine(recordingDirectory, Guid.NewGuid() + ".wav"), Name = "Nova pista", ClipsInitialized = true }; SessionStems.Add(added); AttachStem(added); }
            var clip = original with { Id = Guid.NewGuid(), Position = Math.Max(0, original.Position + delta) };
            SessionStems[baseIndex + track].Clips.Add(clip); SelectedClips.Add(clip.Id);
        }
        ClipsChanged(); CommitEditHistory();
    }
    public void CrossfadeClips()
    {
        if (!CanEditSession) return;
        var selected = SelectedClipItems().OrderBy(x => x.Clip.Position).ToArray();
        if (selected.Length != 2 || selected[0].Stem != selected[1].Stem) return;
        var a = selected[0].Clip; var b = selected[1].Clip;
        if (b.Position <= a.Position || b.Position >= a.End || b.End < a.End) return;
        BeginClipGesture(); double overlap = a.End - b.Position;
        ReplaceClip(selected[0].Stem, a with { FadeOut = overlap, EqualPowerOut = true });
        ReplaceClip(selected[1].Stem, b with { FadeIn = overlap, EqualPowerIn = true }); EndClipGesture();
    }
    public async Task RelinkClipSource(BackingStem stem, string oldPath, string newPath)
    {
        try { await RelinkClipSourceCore(stem, oldPath, newPath); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Error("Não foi possível localizar áudio do clipe.", ex); }
    }
    private async Task RelinkClipSourceCore(BackingStem stem, string oldPath, string newPath)
    {
        if (!CanEditSession || !File.Exists(newPath)) return;
        var wave = await BackingAudioProcessing.Analyze(newPath, lifetime.Token, 32768);
        if (!CanEditSession || editingGesture || SessionStems?.Contains(stem) != true) return;
        BeginClipGesture(); ClipWaves[newPath] = wave;
        for (int i = 0; i < stem.Clips.Count; i++)
        {
            var clip = stem.Clips[i];
            var takes = clip.Takes.Select(t => t with { Layers = t.Layers.Select(l => string.Equals(l.Source, oldPath, StringComparison.OrdinalIgnoreCase)
                ? l with { Source = newPath, Duration = l.Duration > 0 ? Math.Min(l.Duration, Math.Max(0, wave.Duration - l.SourceStart) * l.RecordedSpeed) : wave.Duration * l.RecordedSpeed } : l).ToArray() }).ToArray();
            stem.Clips[i] = clip with { Takes = takes, Duration = clip.Duration > 0 ? clip.Duration : takes.SelectMany(t => t.Layers).Select(l => l.Position + l.Duration).DefaultIfEmpty(0).Max() };
        }
        for (int i = 0; i < stem.Takes.Count; i++)
        {
            if (!string.Equals(stem.Takes[i].Source, oldPath, StringComparison.OrdinalIgnoreCase)) continue;
            var replacement = stem.Takes[i] with { Source = newPath };
            bool active = stem.ActiveTake?.Id == replacement.Id;
            stem.Takes[i] = replacement;
            if (active && !replacement.Incomplete) stem.ActiveTake = replacement;
        }
        if (string.Equals(stem.Source, oldPath, StringComparison.OrdinalIgnoreCase)) stem.Source = newPath;
        stem.State = "Pronta"; ClipsChanged(); EndClipGesture();
    }
    private void EnsureClips(BackingStem stem)
    {
        if (stem.ClipsInitialized) return;
        stem.EnsureTake();
        var takes = stem.SnapshotTakes();
        if (takes.Length == 0) { stem.ClipsInitialized = true; return; }
        double Length(string source)
        {
            if (ClipWaves.TryGetValue(source, out var wave)) return wave.Duration;
            if (!File.Exists(source)) return 0;
            try { using var decoder = new BackingDecoder(source); return decoder.Duration; } catch (Exception ex) { Log(ex); return 0; }
        }
        var valid = takes.Where(t => !t.Incomplete).ToArray(); if (valid.Length == 0) { stem.ClipsInitialized = true; return; }
        double start = valid.Min(t => t.Offset), end = valid.Max(t => t.Offset + Length(t.Source) * t.RecordedSpeed);
        var alternatives = takes.Select(t => new BackingClipTake(t.Id, Path.GetFileNameWithoutExtension(t.Source),
            [new(Guid.NewGuid(), t.Source, t.Offset - start, 0, Length(t.Source) * t.RecordedSpeed, t.RecordedSpeed, t.RecordedPitch, t.LegacyMono)], t.Incomplete)).ToArray();
        stem.Clips.Add(new(Guid.NewGuid(), stem.Name, start, Math.Max(0, end - start), alternatives, stem.ActiveTake?.Id ?? valid[0].Id));
        stem.ClipsInitialized = true;
    }
}
