using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private BackingRecording? recording;
    private string recordingDirectory = "", playlistPath = "";
    private string? recordingDestination, lastRecording;
    private bool recordingFinalizing, playlistLoaded, playlistLoadFailed;
    private BackingStem? recordingStem;
    private BackingTrack? recordingTrack;
    private (double Position, long Timestamp) recordingAnchor;
    private Task recordingTransport = Task.CompletedTask;
    private readonly DispatcherTimer recordingTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };
    public ICommand ToggleRecording { get; private set; } = null!;
    public bool IsRecording => recording != null;
    public bool CanChangeRecordingInput => !IsRecording && !recordingFinalizing;
    public string RecordingLabel => recordingFinalizing ? RecordingState : IsRecording ? "Parar e salvar gravação" : "Iniciar gravação";
    public string RecordingState => recordingFinalizing ? (IsRecording ? "Finalizando gravação…" : "Preparando gravação…")
        : IsRecording ? "Gravando áudio" : "Pronto para gravar";
    public string RecordingHelp => !HasRecordingInput && !IsRecording ? "Selecione uma entrada para gravar."
        : SelectedOutput == null && !IsRecording ? "Escolha uma saída de áudio para gravar em sincronia."
        : "REC grava a entrada selecionada em uma nova pista sincronizada. Clique novamente para salvar.";
    public string RecordingTime => FormatTime(recording?.Seconds ?? 0, true);
    public string? LastRecording => lastRecording;
    private bool HasRecordingInput => SelectedInput != null && Inputs.Any(input => input.Id == SelectedInput.Id);

    private void InitializeRecording(string dataDirectory)
    {
        InitializeSession();
        recordingDirectory = Path.Combine(dataDirectory, "Recordings");
        playlistPath = Path.Combine(dataDirectory, "backing-playlist-v1.json");
        ToggleRecording = new UiCommand(() => Start(ToggleRecordingSession()),
            () => !disposed && !recordingFinalizing && (IsRecording || HasRecordingInput && SelectedOutput != null));
        recordingTimer.Tick += (_, _) => UpdateRecordingWave();
        PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SelectedInput)) { Notify(nameof(RecordingHelp)); Changed(); } };
        Inputs.CollectionChanged += (_, _) => { Notify(nameof(RecordingHelp)); Changed(); };
    }

    private async Task ToggleRecordingSession()
    {
        if (disposed || recordingFinalizing) return;
        if (IsRecording) { await StopRecordingAsync(); return; }
        if (!HasRecordingInput) return;
        string endpoint = SelectedInput!.Id;
        if (SelectedOutput == null) return;
        Directory.CreateDirectory(recordingDirectory);
        recordingDestination = Path.Combine(recordingDirectory, $"Recording-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.wav");
        if (SelectedTrack == null)
        {
            var empty = new BackingTrack(recordingDestination);
            empty.Stems.Clear();
            empty.Stems.Add(new() { Source = recordingDestination, IsProvisional = true });
            Tracks.Add(empty); SelectedTrack = empty;
        }
        recordingTrack = SelectedTrack;
        looping = false; parameterTimer.Stop();
        recordingFinalizing = true; RefreshRecording();
        try
        {
            await StopYouTubePreviewAndWait();
            if (BeforePlayback != null) await BeforePlayback();
            await engine.SetRecordingClock(true);
            await engine.ConfigureMetronome(MetronomeConfiguration() with { Enabled = FollowTrack && metronomeEnabled && Bpm > 0 });
            await engine.Open(SelectedTrack!.Source, SelectedOutput.Id, Position, Speed / 100, Pitch, false, 0, Duration, Volume / 100, IsMuted, SessionSources());
            recordingStem = recordingTrack!.Stems.FirstOrDefault(s => s.IsProvisional) ?? new BackingStem { Source = recordingDestination, IsProvisional = true };
            recordingStem.RecordedSpeed = Speed / 100; recordingStem.RecordedPitch = Pitch;
            recordingStem.Offset = Position; recordingStem.State = "Gravando áudio";
            if (!recordingTrack.Stems.Contains(recordingStem)) recordingTrack.Stems.Add(recordingStem);
            AttachStem(recordingStem); SelectedStem = recordingStem; FollowRecording = true;
            await engine.Play(false);
            recordingAnchor = await engine.CaptureClock();
            recording = new BackingRecording(endpoint, recordingDestination);
            HasError = false; Status = "Gravação em andamento.";
            recordingTimer.Start();
            _ = ObserveRecording(recording);
        }
        catch (Exception ex)
        {
            await engine.SetRecordingClock(false); await engine.Pause();
            if (recordingStem != null) recordingTrack?.Stems.Remove(recordingStem);
            recordingStem = null;
            if (ex.Data["RecordingPath"] is string path)
            {
                lastRecording = path;
                Error("Não foi possível iniciar a gravação. O WAV recuperável está em: " + path, ex);
                Notify(nameof(LastRecording));
            }
            else Error("Não foi possível iniciar a gravação. Verifique a entrada e o destino.", ex);
        }
        finally
        {
            recordingFinalizing = false;
            if (recording == null && recordingTrack != null)
            {
                foreach (var provisional in recordingTrack.Stems.Where(s => s.IsProvisional).ToArray()) recordingTrack.Stems.Remove(provisional);
                if (recordingTrack.Stems.Count == 0) RemoveTrackItem(recordingTrack);
                recordingTrack = null;
            }
            RefreshRecording();
        }
    }

    private async Task StopRecordingAsync()
    {
        if (recording is not { } session || recordingFinalizing) return;
        recordingFinalizing = true; RefreshRecording();
        var error = await session.Stop();
        CompleteRecording(session, error);
        await recordingTransport;
    }

    private async Task StopTransportAsync()
    {
        await StopRecordingAsync();
        await StopPlayback();
    }

    private async Task ObserveRecording(BackingRecording session)
    {
        var error = await session.Completion;
        Dispatch(() => { if (!disposed) CompleteRecording(session, error); });
    }

    private void CompleteRecording(BackingRecording session, Exception? error)
    {
        if (!ReferenceEquals(recording, session)) return;
        recordingFinalizing = true;
        recordingTimer.Stop();
        UpdateRecordingWave();
        if (recordingStem != null) recordingStem.State = "Finalizando gravação…";
        session.Dispose();
        error ??= session.Completion.GetAwaiter().GetResult();
        recording = null;
        try
        {
            if (error != null) throw error;
            if (session.Seconds <= 0)
            {
                lastRecording = session.TemporaryPath;
                if (recordingStem != null) recordingTrack?.Stems.Remove(recordingStem);
                Status = "Gravação sem amostras. Nenhuma faixa foi adicionada.";
                return;
            }
            File.Move(session.TemporaryPath, recordingDestination!, overwrite: false);
            lastRecording = recordingDestination;
            try
            {
                if (recordingStem != null)
                {
                    recordingStem.Source = lastRecording!; recordingStem.IsProvisional = false;
                    recordingStem.State = "Gravação salva";
                }
                SaveRecordingPlaylist();
                Status = "Gravação salva na sessão."; HasError = false;
            }
            catch (Exception ex)
            { Error("WAV salvo, mas não foi possível registrar a faixa na playlist: " + lastRecording, ex); }
        }
        catch (Exception ex)
        {
            lastRecording = session.TemporaryPath;
            if (recordingStem != null)
            {
                if (session.Seconds <= 0) recordingTrack?.Stems.Remove(recordingStem);
                else { recordingStem.Source = session.TemporaryPath; recordingStem.IsProvisional = false; recordingStem.State = "Gravação interrompida · WAV recuperável"; }
            }
            PersistSession();
            Error("Gravação interrompida ou não salva no destino. O WAV recuperável está em: " + session.TemporaryPath, ex);
        }
        finally
        {
            recordingFinalizing = false;
            if (recordingTrack?.Stems.Count == 0) RemoveTrackItem(recordingTrack);
            recordingStem = null; recordingTrack = null;
            RefreshSessionDuration();
            if (!disposed) { recordingFinalizing = true; recordingTransport = FinishRecordingTransport(); Start(recordingTransport); }
            Notify(nameof(LastRecording)); RefreshRecording();
        }
    }

    private async Task FinishRecordingTransport()
    {
        try { await engine.Pause(); await engine.SetRecordingClock(false); await engine.RefreshSession(SessionSources()); }
        finally { recordingFinalizing = false; RefreshRecording(); }
    }
    private void UpdateRecordingWave()
    {
        Notify(nameof(RecordingTime));
        if (recording is not { } session || recordingStem is not { } stem) return;
        if (double.IsFinite(session.FirstCaptureSeconds))
            stem.Offset = recordingAnchor.Position + (session.FirstCaptureSeconds - recordingAnchor.Timestamp / (double)System.Diagnostics.Stopwatch.Frequency - LatencyCompensationMs / 1000) * stem.RecordedSpeed;
        stem.Length = session.Seconds; stem.Peaks = session.Peaks;
        Duration = Math.Max(Duration, stem.Offset + stem.TimelineLength);
        if (FollowRecording && Zoom > 1 && Position >= ViewportStart + ViewportDuration * .8)
            ViewportStart = Position - ViewportDuration * .2;
    }
    private void RefreshRecording()
    {
        Notify(nameof(CanEditSession)); Notify(nameof(CanSeek)); Notify(nameof(CanLoop)); Notify(nameof(IsLooping));
        Notify(nameof(IsRecording)); Notify(nameof(CanChangeRecordingInput)); Notify(nameof(RecordingLabel));
        Notify(nameof(RecordingState)); Notify(nameof(RecordingHelp)); Notify(nameof(RecordingTime)); Changed();
    }

    private sealed record SavedStem(Guid Id, string Source, double Offset, double RecordedSpeed, double RecordedPitch,
        bool Muted, bool Solo, double Volume, string State);
    private sealed record SavedSession(Guid Id, string Source, SavedStem[] Stems, double Speed = 100, double Pitch = 0);
    private sealed record RecordingPlaylist(int Version, string[]? Paths = null, SavedSession[]? Sessions = null, double LatencyMs = 0);
    private void LoadRecordingPlaylist()
    {
        if (playlistLoaded) return;
        playlistLoaded = true;
        try
        {
            if (!File.Exists(playlistPath)) return;
            var saved = JsonSerializer.Deserialize<RecordingPlaylist>(File.ReadAllText(playlistPath));
            if (saved == null || saved.Version is not (1 or 2)) throw new InvalidDataException("Unsupported playlist format.");
            var sessions = saved.Version == 1
                ? (saved.Paths ?? throw new InvalidDataException("Missing playlist paths.")).Select(path => new SavedSession(Guid.NewGuid(), path,
                    [new(Guid.NewGuid(), path, 0, 1, 0, false, false, 100, "Pronta")])).ToArray()
                : saved.Sessions ?? throw new InvalidDataException("Missing sessions.");
            var loaded = new List<BackingTrack>();
            foreach (var item in sessions)
            {
                if (!Path.IsPathFullyQualified(item.Source) || item.Stems == null) throw new InvalidDataException("Invalid session path.");
                var track = new BackingTrack(item.Source) { SessionId = item.Id, Speed = double.IsFinite(item.Speed) ? Math.Clamp(item.Speed, 50, 150) : 100, Pitch = double.IsFinite(item.Pitch) ? Math.Clamp(item.Pitch, -6, 6) : 0 };
                track.Stems.Clear();
                foreach (var entry in item.Stems)
                {
                    if (!Path.IsPathFullyQualified(entry.Source) || !double.IsFinite(entry.Offset) || !double.IsFinite(entry.RecordedSpeed)
                        || entry.RecordedSpeed is < .5 or > 1.5 || !double.IsFinite(entry.RecordedPitch) || Math.Abs(entry.RecordedPitch) > 6
                        || !double.IsFinite(entry.Volume)) throw new InvalidDataException("Invalid session stem.");
                    var stem = new BackingStem { Id = entry.Id, Source = entry.Source, Offset = entry.Offset, RecordedSpeed = entry.RecordedSpeed,
                        RecordedPitch = entry.RecordedPitch, IsMuted = entry.Muted, IsSolo = entry.Solo, Volume = entry.Volume,
                        State = File.Exists(entry.Source) ? entry.State : "Arquivo ausente" };
                    track.Stems.Add(stem); AttachStem(stem);
                }
                if (track.Stems.Select(s => s.Id).Distinct().Count() != track.Stems.Count) throw new InvalidDataException("Duplicate stem identifiers.");
                loaded.Add(track);
            }
            if (saved.Version == 1 && !File.Exists(playlistPath + ".v1.bak")) File.Copy(playlistPath, playlistPath + ".v1.bak", overwrite: false);
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
                s.IsMuted, s.IsSolo, s.Volume, s.State)).ToArray(), track.Speed, track.Pitch)).ToArray();
        File.WriteAllText(temporary, JsonSerializer.Serialize(new RecordingPlaylist(2, Sessions: sessions, LatencyMs: LatencyCompensationMs)));
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
