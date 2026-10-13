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
    private readonly DispatcherTimer recordingTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    public ICommand ToggleRecording { get; private set; } = null!;
    public bool IsRecording => recording != null;
    public bool CanChangeRecordingInput => !IsRecording && !recordingFinalizing;
    public string RecordingLabel => recordingFinalizing ? RecordingState : IsRecording ? "Parar e salvar gravação" : "Iniciar gravação";
    public string RecordingState => recordingFinalizing ? (IsRecording ? "Finalizando gravação…" : "Preparando gravação…")
        : IsRecording ? "Gravando áudio" : "Pronto para gravar";
    public string RecordingHelp => !HasRecordingInput && !IsRecording ? "Selecione uma entrada para gravar."
        : "REC grava somente a entrada selecionada. Clique novamente para salvar uma nova faixa.";
    public string RecordingTime => FormatTime(recording?.Seconds ?? 0, true);
    public string? LastRecording => lastRecording;
    private bool HasRecordingInput => SelectedInput != null && Inputs.Any(input => input.Id == SelectedInput.Id);

    private void InitializeRecording(string dataDirectory)
    {
        recordingDirectory = Path.Combine(dataDirectory, "Recordings");
        playlistPath = Path.Combine(dataDirectory, "backing-playlist-v1.json");
        ToggleRecording = new UiCommand(() => Start(ToggleRecordingSession()),
            () => !disposed && !recordingFinalizing && (IsRecording || HasRecordingInput));
        recordingTimer.Tick += (_, _) => Notify(nameof(RecordingTime));
        PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SelectedInput)) { Notify(nameof(RecordingHelp)); Changed(); } };
        Inputs.CollectionChanged += (_, _) => { Notify(nameof(RecordingHelp)); Changed(); };
    }

    private async Task ToggleRecordingSession()
    {
        if (disposed || recordingFinalizing) return;
        if (IsRecording) { await StopRecordingAsync(); return; }
        if (!HasRecordingInput) return;
        string endpoint = SelectedInput!.Id;
        recordingFinalizing = true; RefreshRecording();
        try
        {
            Directory.CreateDirectory(recordingDirectory);
            recordingDestination = Path.Combine(recordingDirectory, $"Recording-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.wav");
            recording = new BackingRecording(endpoint, recordingDestination);
            HasError = false; Status = "Gravação em andamento.";
            recordingTimer.Start();
            _ = ObserveRecording(recording);
        }
        catch (Exception ex)
        {
            if (ex.Data["RecordingPath"] is string path)
            {
                lastRecording = path;
                Error("Não foi possível iniciar a gravação. O WAV recuperável está em: " + path, ex);
                Notify(nameof(LastRecording));
            }
            else Error("Não foi possível iniciar a gravação. Verifique a entrada e o destino.", ex);
        }
        finally { recordingFinalizing = false; RefreshRecording(); }
    }

    private async Task StopRecordingAsync()
    {
        if (recording is not { } session || recordingFinalizing) return;
        recordingFinalizing = true; RefreshRecording();
        var error = await session.Stop();
        CompleteRecording(session, error);
    }

    private async Task StopTransportAsync()
    {
        var captureStop = StopRecordingAsync();
        try { await StopPlayback(); }
        finally { await captureStop; }
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
        session.Dispose();
        error ??= session.Completion.GetAwaiter().GetResult();
        recording = null;
        try
        {
            if (error != null) throw error;
            if (session.Seconds <= 0)
            {
                lastRecording = session.TemporaryPath;
                Status = "Gravação sem amostras. Nenhuma faixa foi adicionada.";
                return;
            }
            File.Move(session.TemporaryPath, recordingDestination!, overwrite: false);
            lastRecording = recordingDestination;
            try
            {
                AddPathsCore([lastRecording!], selectWhenUnselected: Tracks.Count == 0);
                if (!Tracks.Any(track => string.Equals(track.Source, lastRecording, StringComparison.OrdinalIgnoreCase)))
                    throw new IOException("The saved WAV could not be added to the playlist.");
                SaveRecordingPlaylist();
                Status = "Gravação salva e adicionada à playlist."; HasError = false;
            }
            catch (Exception ex)
            { Error("WAV salvo, mas não foi possível registrar a faixa na playlist: " + lastRecording, ex); }
        }
        catch (Exception ex)
        {
            lastRecording = session.TemporaryPath;
            Error("Gravação interrompida ou não salva no destino. O WAV recuperável está em: " + session.TemporaryPath, ex);
        }
        finally
        {
            recordingFinalizing = false;
            Notify(nameof(LastRecording)); RefreshRecording();
        }
    }

    private void RefreshRecording()
    {
        Notify(nameof(IsRecording)); Notify(nameof(CanChangeRecordingInput)); Notify(nameof(RecordingLabel));
        Notify(nameof(RecordingState)); Notify(nameof(RecordingHelp)); Notify(nameof(RecordingTime)); Changed();
    }

    private sealed record RecordingPlaylist(int Version, string[] Paths);
    private void LoadRecordingPlaylist()
    {
        if (playlistLoaded) return;
        playlistLoaded = true;
        try
        {
            if (!File.Exists(playlistPath)) return;
            var saved = JsonSerializer.Deserialize<RecordingPlaylist>(File.ReadAllText(playlistPath));
            if (saved is not { Version: 1, Paths: { } }) throw new InvalidDataException("Unsupported playlist format.");
            // Retain missing files so the existing Locate file command can repair them.
            foreach (string path in saved.Paths)
            {
                if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)
                    || Path.GetExtension(path).ToLowerInvariant() is not (".mp3" or ".wav")) continue;
                string source = Path.GetFullPath(path);
                if (!Tracks.Any(track => string.Equals(track.Source, source, StringComparison.OrdinalIgnoreCase))) Tracks.Add(new(source));
            }
            SelectedTrack ??= Tracks.FirstOrDefault(); Changed();
        }
        catch (Exception ex) { playlistLoadFailed = true; Error("Não foi possível carregar a playlist salva.", ex); }
    }
    private void SaveRecordingPlaylist()
    {
        if (playlistLoadFailed) throw new IOException("The unreadable playlist was preserved; it cannot be overwritten.");
        Directory.CreateDirectory(Path.GetDirectoryName(playlistPath)!);
        string temporary = playlistPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(new RecordingPlaylist(1, Tracks.Select(track => track.Source).ToArray())));
        File.Move(temporary, playlistPath, overwrite: true);
    }
    private void DisposeRecording()
    {
        if (recording is { } session)
        {
            session.Dispose();
            CompleteRecording(session, session.Completion.GetAwaiter().GetResult());
        }
        if (playlistLoaded || Tracks.Count > 0)
            try { SaveRecordingPlaylist(); } catch (Exception ex) { Error("Não foi possível salvar a playlist.", ex); }
        recordingTimer.Stop();
    }
}
