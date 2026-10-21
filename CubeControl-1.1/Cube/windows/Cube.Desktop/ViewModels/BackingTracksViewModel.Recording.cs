using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private BackingMultitrackCapture? recording;
    private string recordingDirectory = "", playlistPath = "";
    private string? lastRecording;
    private bool recordingFinalizing, playlistLoaded, playlistLoadFailed;
    private BackingTrack? recordingTrack;
    private readonly List<(BackingStem Stem, string Path, BackingTake? Previous)> recordingTargets = [];
    private (double Position, long Timestamp) recordingAnchor;
    private (double Position, long Timestamp)? recordingEndAnchor;
    private Task recordingTransport = Task.CompletedTask;
    private readonly DispatcherTimer recordingTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };
    public ICommand ToggleRecording { get; private set; } = null!;
    public bool IsRecording => recording != null;
    public bool CanChangeRecordingInput => !IsRecording && !recordingFinalizing;
    public string RecordingLabel => recordingFinalizing ? RecordingState : IsRecording ? "Parar e salvar gravação" : "Iniciar gravação";
    public string RecordingState => recordingFinalizing ? "Finalizando gravação…" : IsRecording ? "Gravando áudio" : "Pronto para gravar";
    public string RecordingHelp => !HasRecordingInput ? "Selecione uma entrada para gravar." : "Arme as pistas e escolha seus canais de entrada antes de REC.";
    public string RecordingTime => FormatTime(recording != null && recordingTargets.Count > 0 ? recording.Snapshot(recordingTargets[0].Stem.Id).Seconds : 0, true);
    public string? LastRecording => lastRecording;
    private bool HasRecordingInput => SelectedInput != null && SelectedInput.Available && Inputs.Any(input => input.Id == SelectedInput.Id);

    private void InitializeRecording(string dataDirectory)
    {
        InitializeSession();
        recordingDirectory = Path.Combine(dataDirectory, "Recordings");
        playlistPath = Path.Combine(dataDirectory, "backing-playlist-v1.json");
        ToggleRecording = new UiCommand(() => Start(ToggleRecordingSession()),
            () => !disposed && !recordingFinalizing && (IsRecording || HasRecordingInput && SelectedOutput != null && SessionStems?.Any(s => s.IsArmed) == true));
        recordingTimer.Tick += (_, _) => UpdateRecordingWave();
        PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SelectedInput)) { Notify(nameof(RecordingHelp)); PersistSession(); Changed(); } };
        Inputs.CollectionChanged += (_, _) => { Notify(nameof(RecordingHelp)); Changed(); };
    }
    private BackingCaptureRoute[] CaptureRoutes(bool writing) => SessionStems?.Where(s => s.IsMonitoring || writing && s.IsArmed).Select(s => new BackingCaptureRoute(s.Id, s.InputChannel, s.InputChannels,
        writing ? recordingTargets.FirstOrDefault(t => t.Stem == s).Path : null,
        s.IsMonitoring, s.Volume / 100, s.Pan, s.IsMuted || SessionStems.Any(x => x.IsSolo) && !s.IsSolo)).ToArray() ?? [];

    private async Task ToggleRecordingSession()
    {
        await StopLocalPreview();
        if (disposed || recordingFinalizing) return;
        if (IsRecording) { await StopRecordingAsync(); return; }
        if (!HasRecordingInput || SelectedOutput == null || SessionStems?.Any(s => s.IsArmed) != true) return;
        CommitEditHistory();
        Directory.CreateDirectory(recordingDirectory);
        recordingTrack = SelectedTrack; recordingFinalizing = true; recordingEndAnchor = null;
        StopMonitor(); looping = false; parameterTimer.Stop();
        foreach (var stem in SessionStems.Where(s => s.IsArmed))
        {
            stem.EnsureTake();
            recordingTargets.Add((stem, Path.Combine(recordingDirectory, $"Recording-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.wav"), stem.ActiveTake));
        }
        RefreshRecording();
        try
        {
            await StopYouTubePreviewAndWait();
            if (BeforePlayback != null) await BeforePlayback();
            // Open every writer and validate every route before transport/capture starts.
            recording = new BackingMultitrackCapture(SelectedInput!.Id, CaptureRoutes(true));
            await engine.SetMonitor(SelectedOutput.Id, recording.MonitorProvider);
            await engine.SetRecordingClock(true);
            await engine.ConfigureMetronome(MetronomeConfiguration() with { Enabled = FollowTrack && metronomeEnabled && Bpm > 0 });
            await engine.Open(SelectedTrack!.Source, SelectedOutput.Id, Position, Speed / 100, Pitch, false, 0, Duration, Volume / 100, IsMuted, SessionSources());
            await engine.Play(false);
            recordingAnchor = await engine.CaptureClock();
            foreach (var target in recordingTargets) { target.Stem.State = "Gravando áudio"; target.Stem.Peaks = []; }
            recording.Start(); recordingTimer.Start(); FollowRecording = true;
            Status = "Gravação em andamento."; HasError = false;
            _ = ObserveRecording(recording);
        }
        catch (Exception ex)
        {
            if (recording != null) { recording.Dispose(); CompleteRecording(recording, ex); }
            else { RestoreRecordingTargets(); recordingTargets.Clear(); recordingTrack = null; }
            await engine.SetRecordingClock(false); await engine.Pause();
            Error("Não foi possível iniciar a gravação. Verifique a entrada e o destino.", ex);
        }
        finally { recordingFinalizing = false; RefreshRecording(); }
    }
    private async Task StopRecordingAsync()
    {
        if (recording is not { } session || recordingFinalizing) return;
        recordingFinalizing = true; RefreshRecording();
        recordingEndAnchor = await engine.CaptureClock();
        var error = await session.Stop(); CompleteRecording(session, error); await recordingTransport;
    }
    private async Task StopTransportAsync() { await StopRecordingAsync(); await StopPlayback(); }
    private async Task ObserveRecording(BackingMultitrackCapture session)
    {
        var error = await session.Completion;
        Dispatch(() => { if (!disposed) CompleteRecording(session, error); });
    }
    private void RestoreRecordingTargets()
    {
        foreach (var target in recordingTargets)
        {
            target.Stem.CanEditRouting = true;
            if (target.Previous != null) { target.Stem.Source = target.Previous.Source; target.Stem.Offset = target.Previous.Offset; target.Stem.RecordedSpeed = target.Previous.RecordedSpeed; target.Stem.RecordedPitch = target.Previous.RecordedPitch; }
            target.Stem.Peaks = []; target.Stem.Length = 0;
        }
    }
    private void CompleteRecording(BackingMultitrackCapture session, Exception? error)
    {
        if (!ReferenceEquals(recording, session)) return;
        recordingFinalizing = true; recordingTimer.Stop(); UpdateRecordingWave();
        session.Dispose(); error ??= session.Completion.GetAwaiter().GetResult(); recording = null;
        RestoreRecordingTargets();
        double outputClockRatio = 1;
        if (recordingEndAnchor is { } end && end.Timestamp - recordingAnchor.Timestamp > System.Diagnostics.Stopwatch.Frequency)
        {
            double measured = (end.Position - recordingAnchor.Position) / ((end.Timestamp - recordingAnchor.Timestamp) / (double)System.Diagnostics.Stopwatch.Frequency) / (Speed / 100);
            if (measured is >= .98 and <= 1.02) outputClockRatio = measured;
        }
        foreach (var target in recordingTargets)
        {
            var captured = session.Snapshot(target.Stem.Id);
            if (captured.Seconds <= 0 || !File.Exists(target.Path)) continue;
            double offset = recordingAnchor.Position + (session.FirstCaptureSeconds - recordingAnchor.Timestamp / (double)System.Diagnostics.Stopwatch.Frequency - LatencyCompensationMs / 1000) * Speed / 100;
            var take = new BackingTake(Guid.NewGuid(), target.Path, offset, Speed / 100 * Math.Clamp(session.ClockRatio * outputClockRatio, .98, 1.02), Pitch, error != null, LegacyMono: false);
            target.Stem.Takes.Add(take);
            if (error == null)
            {
                var alternative = new BackingClipTake(take.Id, Path.GetFileNameWithoutExtension(target.Path),
                    [new(Guid.NewGuid(), target.Path, 0, 0, captured.Seconds * take.RecordedSpeed, take.RecordedSpeed, take.RecordedPitch)]);
                var clip = new BackingClip(Guid.NewGuid(), target.Stem.Name, offset, captured.Seconds * take.RecordedSpeed, [alternative], take.Id);
                var clips = BackingClipEdit.Overdub(target.Stem.Clips, clip);
                target.Stem.Clips.Clear(); target.Stem.Clips.AddRange(clips);
                ClipWaves[target.Path] = (captured.Seconds, captured.Peaks);
            }
            target.Stem.State = error == null ? "Gravação salva" : "Gravação interrompida";
            lastRecording = target.Path;
        }
        recordingTargets.Clear(); recordingTrack = null;
        try { SaveRecordingPlaylist(); } catch (Exception ex) { error ??= ex; }
        if (error != null) Error("Gravação interrompida. As tomadas recuperáveis foram preservadas.", error);
        else { Status = "Gravação salva na sessão."; HasError = false; }
        if (!disposed) { recordingTransport = FinishRecordingTransport(); Start(recordingTransport); }
        else recordingFinalizing = false;
        Notify(nameof(LastRecording)); RefreshRecording();
    }
    private async Task FinishRecordingTransport()
    {
        try
        {
            await engine.SetMonitor(SelectedOutput?.Id ?? "", null); await engine.Pause();
            await engine.SetRecordingClock(false); await engine.RefreshSession(SessionSources());
            await LoadSessionWaves(); RefreshSessionDuration();
        }
        finally { recordingFinalizing = false; CommitEditHistory(); NotifyClips(); RefreshRecording(); if (SessionStems?.Any(s => s.IsMonitoring) == true) StartMonitor(); }
    }
    private void UpdateRecordingWave()
    {
        Notify(nameof(RecordingTime));
        if (recording is not { } session) return;
        foreach (var target in recordingTargets)
        {
            var captured = session.Snapshot(target.Stem.Id); var stem = target.Stem;
            if (double.IsFinite(session.FirstCaptureSeconds)) stem.Offset = recordingAnchor.Position + (session.FirstCaptureSeconds - recordingAnchor.Timestamp / (double)System.Diagnostics.Stopwatch.Frequency - LatencyCompensationMs / 1000) * Speed / 100;
            stem.RecordedSpeed = Speed / 100; stem.RecordedPitch = Pitch;
            stem.Length = captured.Seconds; stem.Peaks = captured.Peaks;
            Duration = Math.Max(Duration, stem.Offset + stem.TimelineLength);
        }
        if (FollowRecording && Zoom > 1 && Position >= ViewportStart + ViewportDuration * .8) ViewportStart = Position - ViewportDuration * .2;
    }
    private void RefreshRecording()
    {
        foreach (var stem in SessionStems ?? []) stem.CanEditRouting = CanEditSession;
        Notify(nameof(CanEditSession)); Notify(nameof(CanSeek)); Notify(nameof(CanLoop)); Notify(nameof(IsLooping));
        Notify(nameof(IsRecording)); Notify(nameof(CanChangeRecordingInput)); Notify(nameof(RecordingLabel));
        Notify(nameof(RecordingState)); Notify(nameof(RecordingHelp)); Notify(nameof(RecordingTime)); Notify(nameof(IsMonitoring)); Notify(nameof(MonitoredInputs)); Changed();
    }

    private sealed record SavedStem(Guid Id, string Source, double Offset, double RecordedSpeed, double RecordedPitch,
        bool Muted, bool Solo, double Volume, string State, string Name = "", string Color = "#6495ED", double Pan = 0, bool LegacyMono = true, int InputChannel = 0, int InputChannels = 1, BackingTake[]? Takes = null, Guid? ActiveTake = null, BackingClip[]? Clips = null);
    private sealed record SavedSession(Guid Id, string Source, SavedStem[] Stems, double Speed = 100, double Pitch = 0, string? InputId = null, double MasterGain = 1, double AccompanimentGain = .3, string Meter = "4/4", double Bpm = 0, double FirstBeat = 0, bool ManualBpm = false);
    private sealed record RecordingPlaylist(int Version, string[]? Paths = null, SavedSession[]? Sessions = null, double LatencyMs = 0, double MasterVolume = 1, string? InputId = null);
    private void LoadRecordingPlaylist()
    {
        if (playlistLoaded) return;
        playlistLoaded = true;
        try
        {
            if (!File.Exists(playlistPath)) return;
            var saved = JsonSerializer.Deserialize<RecordingPlaylist>(File.ReadAllText(playlistPath));
            if (saved == null || saved.Version is not (1 or 2 or 3 or 4)) throw new InvalidDataException("Unsupported playlist format.");
            var sessions = saved.Version == 1
                ? (saved.Paths ?? throw new InvalidDataException("Missing playlist paths.")).Select(path => new SavedSession(Guid.NewGuid(), path,
                    [new(Guid.NewGuid(), path, 0, 1, 0, false, false, 100, "Pronta")])).ToArray()
                : saved.Sessions ?? throw new InvalidDataException("Missing sessions.");
            var loaded = new List<BackingTrack>();
            foreach (var item in sessions)
            {
                if (!Path.IsPathFullyQualified(item.Source) || item.Stems == null) throw new InvalidDataException("Invalid session path.");
                var track = new BackingTrack(item.Source) { SessionId = item.Id, Speed = double.IsFinite(item.Speed) ? Math.Clamp(item.Speed, 50, 150) : 100, Pitch = double.IsFinite(item.Pitch) ? Math.Clamp(item.Pitch, -6, 6) : 0 };
                track.CaptureEndpoint = item.InputId; track.MasterGain = double.IsFinite(item.MasterGain) ? Math.Clamp(item.MasterGain, 0, BackingMixMath.MaximumGain) : 1; track.AccompanimentGain = double.IsFinite(item.AccompanimentGain) ? Math.Clamp(item.AccompanimentGain, 0, BackingMixMath.MaximumGain) : .3;
                track.Meter = MetronomeMeters.Contains(item.Meter) ? item.Meter : "4/4";
                if (item.Bpm > 0) { track.Bpm = item.Bpm; track.ManualBpm = item.ManualBpm; }
                if (double.IsFinite(item.FirstBeat)) metronomeOffsets[item.Source] = item.FirstBeat;
                track.Stems.Clear();
                foreach (var entry in item.Stems)
                {
                    if (!Path.IsPathFullyQualified(entry.Source) || !double.IsFinite(entry.Offset) || !double.IsFinite(entry.RecordedSpeed)
                        || entry.RecordedSpeed is < .49 or > 1.53 || !double.IsFinite(entry.RecordedPitch) || Math.Abs(entry.RecordedPitch) > 6
                        || !double.IsFinite(entry.Volume)) throw new InvalidDataException("Invalid session stem.");
                    var stem = new BackingStem { Id = entry.Id, Source = entry.Source, Offset = entry.Offset, RecordedSpeed = entry.RecordedSpeed,
                        RecordedPitch = entry.RecordedPitch, IsMuted = entry.Muted, IsSolo = entry.Solo, Volume = entry.Volume,
                        State = File.Exists(entry.Source) ? entry.State : "Arquivo ausente", Name = entry.Name, Color = entry.Color,
                        Pan = entry.Pan, LegacyMono = entry.LegacyMono, InputChannel = entry.InputChannel, InputChannels = entry.InputChannels };
                    if (entry.Takes != null)
                    {
                        foreach (var take in entry.Takes)
                        {
                            if (!Path.IsPathFullyQualified(take.Source) || !double.IsFinite(take.Offset) || !double.IsFinite(take.RecordedSpeed) || take.RecordedSpeed is < .49 or > 1.53 || !double.IsFinite(take.RecordedPitch) || Math.Abs(take.RecordedPitch) > 6) throw new InvalidDataException("Invalid take.");
                            stem.Takes.Add(take);
                        }
                        stem.ActiveTake = stem.Takes.FirstOrDefault(t => t.Id == entry.ActiveTake);
                    }
                    stem.EnsureTake();
                    if (saved.Version == 4)
                    {
                        if (entry.Clips == null) throw new InvalidDataException("Missing clips.");
                        foreach (var clip in entry.Clips) { clip.Validate(); stem.Clips.Add(clip); }
                        if (stem.Clips.Select(c => c.Id).Distinct().Count() != stem.Clips.Count) throw new InvalidDataException("Duplicate clips.");
                        stem.ClipsInitialized = true;
                    }
                    track.Stems.Add(stem); AttachStem(stem);
                }
                if (track.Stems.Select(s => s.Id).Distinct().Count() != track.Stems.Count) throw new InvalidDataException("Duplicate stem identifiers.");
                if (track.Stems.SelectMany(s => s.Clips).Select(c => c.Id).Distinct().Count() != track.Stems.Sum(s => s.Clips.Count)) throw new InvalidDataException("Duplicate session clip identifiers.");
                loaded.Add(track);
            }
            if (saved.Version < 4 && !File.Exists(playlistPath + $".v{saved.Version}.bak")) File.Copy(playlistPath, playlistPath + $".v{saved.Version}.bak", overwrite: false);
            masterVolume = double.IsFinite(saved.MasterVolume) ? Math.Clamp(saved.MasterVolume, 0, BackingMixMath.MaximumGain) : 1;
            Start(engine.SetMasterVolume(masterVolume)); Notify(nameof(MasterDb));
            savedInputId = saved.InputId;
            foreach (var track in loaded) Tracks.Add(track);
            latencyCompensationMs = double.IsFinite(saved.LatencyMs) ? Math.Clamp(saved.LatencyMs, -1000, 1000) : 0;
            Notify(nameof(LatencyCompensationMs)); SelectedTrack ??= Tracks.FirstOrDefault(); Changed();
        }
        catch (Exception ex) { playlistLoadFailed = true; Error("Não foi possível carregar a playlist salva.", ex); }
    }
    private void SaveRecordingPlaylist()
    {
        if (playlistLoadFailed) throw new IOException("The unreadable playlist was preserved; it cannot be overwritten.");
        Directory.CreateDirectory(Path.GetDirectoryName(playlistPath)!);
        string temporary = playlistPath + ".tmp";
        var sessions = Tracks.Select(track => new SavedSession(track.SessionId, track.Source,
            track.Stems
            .Where(s => !s.IsProvisional).Select(s => new SavedStem(s.Id, s.Source, s.Offset, s.RecordedSpeed, s.RecordedPitch,
                s.IsMuted, s.IsSolo, s.Volume, s.State, s.Name, s.Color, s.Pan, s.LegacyMono, s.InputChannel, s.InputChannels, s.SnapshotTakes(), s.ActiveTake?.Id, s.Clips.ToArray())).ToArray(), track.Speed, track.Pitch, track.CaptureEndpoint, track.MasterGain, track.AccompanimentGain, track.Meter, track.Bpm, metronomeOffsets.GetValueOrDefault(track.Source), track.ManualBpm)).ToArray();
        File.WriteAllText(temporary, JsonSerializer.Serialize(new RecordingPlaylist(4, Sessions: sessions, LatencyMs: LatencyCompensationMs, MasterVolume: masterVolume, InputId: SelectedInput?.Id ?? savedInputId)));
        File.Move(temporary, playlistPath, overwrite: true);
    }
    private void DisposeRecording()
    {
        sessionSaveTimer.Stop();
        if (recording is { } session)
        {
            session.Dispose();
            CompleteRecording(session, session.Completion.GetAwaiter().GetResult());
        }
        if (playlistLoaded || Tracks.Count > 0)
            try { SaveRecordingPlaylist(); } catch (Exception ex) { Error("Não foi possível salvar a playlist.", ex); }
        recordingTimer.Stop(); sessionSaveTimer.Stop();
        foreach (var stem in attachedStems.ToArray()) DetachStem(stem);
    }
}
