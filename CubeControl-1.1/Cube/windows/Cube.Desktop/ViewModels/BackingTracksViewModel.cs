using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Localization;
using Microsoft.Win32;
using Windows.Devices.Enumeration;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Playback;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace Cube.Desktop.ViewModels;

public sealed record BackingTrack(string Source)
{
    public string Title => Path.GetFileNameWithoutExtension(Source);
    public double Length { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
    public float[] Peaks { get; set; } = [];

}

public sealed class BackingTracksViewModel : Observable, IDisposable
{
    private sealed record Settings(int Version, string[] Sources, string? SelectedSource, string? OutputId, double Volume, bool IsMuted);
    private readonly string settingsPath;
    private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
    private readonly DispatcherTimer positionTimer = new() { Interval = TimeSpan.FromMilliseconds(40) };
    private readonly DispatcherTimer saveTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private DeviceWatcher? watcher;
    private MediaPlayer? player;
    private BackingTrack? selectedTrack;
    private DeviceInformation? selectedOutput;
    private string? preferredOutputId;
    private string status = "Adicione uma faixa e escolha a saída de áudio.";
    private double volume = 30, position, duration;
    private bool muted, loading, playing, canSeek, disposed, restoring, refreshing, settingsDirty;
    private int playbackGeneration;
    public ObservableCollection<BackingTrack> Tracks { get; } = [];
    public ObservableCollection<DeviceInformation> Outputs { get; } = [];
    public ICommand AddFiles { get; }
    public ICommand RemoveTrack { get; }
    public ICommand PlayPause { get; }
    public ICommand Stop { get; }
    public ICommand RefreshOutputs { get; }
    public ICommand BluetoothSettings { get; }
    public ICommand MoveUp { get; }
    public ICommand MoveDown { get; }
    public ICommand Previous { get; }
    public ICommand Next { get; }
    public ICommand SkipBack { get; }
    public ICommand SkipForward { get; }
    public ICommand MarkStart { get; }
    public ICommand MarkEnd { get; }
    public ICommand ResetRange { get; }
    public ICommand ExportRange { get; }
    private CancellationTokenSource? waveformCancellation;
    private bool looping, advancing, exporting;
    private double speed = 100;
    public bool IsPlaying => playing;
    public bool HasActivePlayback => player != null && !loading;
    private bool rateChangePending, resumeAfterRateChange;
    private void ApplyPendingRate()
    {
        if (!rateChangePending || player == null || player.PlaybackSession.PlaybackState != MediaPlaybackState.Paused) return;
        rateChangePending = false;
        try { player.PlaybackSession.PlaybackRate = Speed / 100; }
        catch
        {
            speed = player.PlaybackSession.PlaybackRate * 100;
            Notify(nameof(Speed));
            throw;
        }
        finally { if (resumeAfterRateChange) player.Play(); }
    }
    public SymbolRegular PlayIcon => playing ? SymbolRegular.Pause24 : SymbolRegular.Play24;
    public bool IsLooping { get => looping; set => Set(ref looping, value); }
    public double Speed
    {
        get => speed;
        set
        {
            if (!double.IsFinite(value) || !Set(ref speed, Math.Clamp(value, 50, 150)) || player == null || loading) return;
            Run(() =>
            {
                if (playing)
                {
                    rateChangePending = true; resumeAfterRateChange = true;
                    player.Pause(); ApplyPendingRate();
                }
                else player.PlaybackSession.PlaybackRate = speed / 100;
            });
        }
    }
    public float[] Peaks => SelectedTrack?.Peaks ?? [];
    public double RangeStart
    {
        get => SelectedTrack?.Start ?? 0;
        set { if (SelectedTrack == null || !double.IsFinite(value)) return; SelectedTrack.Start = Math.Clamp(value, 0, Math.Max(0, RangeEnd - .1)); RangeChanged(); }
    }
    public double RangeEnd
    {
        get => SelectedTrack?.End ?? 0;
        set { if (SelectedTrack == null || Duration < .1 || !double.IsFinite(value)) return; SelectedTrack.End = Math.Clamp(value, RangeStart + .1, Duration); RangeChanged(); }
    }
    public string RangeLabel => $"A {TimeSpan.FromSeconds(RangeStart):mm\\:ss} — B {TimeSpan.FromSeconds(RangeEnd):mm\\:ss}";
    private void RangeChanged()
    {
        Notify(nameof(RangeStart)); Notify(nameof(RangeEnd)); Notify(nameof(RangeLabel));
        if (Position < RangeStart || Position > RangeEnd) Position = RangeStart;
        CommandManager.InvalidateRequerySuggested();
    }


    public BackingTracksViewModel(string dataDirectory)
    {
        settingsPath = Path.Combine(dataDirectory, "backing-tracks-v1.json");
        AddFiles = new UiCommand(() => Run(AddFilesCore));
        RemoveTrack = new UiCommand(RemoveSelected, () => SelectedTrack != null);
        PlayPause = new UiCommand(async () => await TogglePlayback(), () => !loading && SelectedTrack != null && SelectedOutput != null);
        Stop = new UiCommand(() => { StopPlayback(); Status = "Reprodução parada."; }, () => player != null || loading);
        RefreshOutputs = new UiCommand(async () => await RefreshDevices(), () => !refreshing);
        BluetoothSettings = new UiCommand(() => Run(() => Process.Start(new ProcessStartInfo("ms-settings:bluetooth") { UseShellExecute = true })));
        positionTimer.Tick += (_, _) => Run(UpdatePosition);
        saveTimer.Tick += (_, _) => { saveTimer.Stop(); Save(); };
        MoveUp = new UiCommand(() => MoveSelected(-1), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) > 0);
        MoveDown = new UiCommand(() => MoveSelected(1), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) < Tracks.Count - 1);
        Previous = new UiCommand(async () => await SelectAdjacent(-1), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) > 0);
        Next = new UiCommand(async () => await SelectAdjacent(1), () => SelectedTrack != null && Tracks.IndexOf(SelectedTrack) < Tracks.Count - 1);
        SkipBack = new UiCommand(() => Position -= 5, () => CanSeek);
        SkipForward = new UiCommand(() => Position += 5, () => CanSeek);
        MarkStart = new UiCommand(() => RangeStart = Position, () => CanSeek);
        MarkEnd = new UiCommand(() => RangeEnd = Position, () => CanSeek);
        ResetRange = new UiCommand(() => { if (SelectedTrack != null) { SelectedTrack.Start = 0; SelectedTrack.End = Duration; RangeChanged(); } }, () => Duration > 0);
        ExportRange = new UiCommand(async () => await ExportSelection(), () => SelectedTrack != null && RangeEnd > RangeStart && !exporting);
        Restore();
        _ = LoadWaveform();
    }

    public BackingTrack? SelectedTrack
    {
        get => selectedTrack;
        set
        {
            if (!Set(ref selectedTrack, value)) return;
            StopPlayback(); Notify(nameof(Peaks)); RangeChanged();
            Status = "Pronto para reproduzir."; Changed(); _ = LoadWaveform();
        }
    }
    public DeviceInformation? SelectedOutput
    {
        get => selectedOutput;
        set
        {
            if (!Set(ref selectedOutput, value)) return;
            StopPlayback();
            if (!restoring) { preferredOutputId = value?.Id; Changed(); }
            Notify(nameof(OutputName));
        }
    }
    public string OutputName => SelectedOutput?.Name ?? "Escolha a saída de áudio.";
    public string Status { get => status; private set => Set(ref status, value); }
    public double Volume
    {
        get => volume;
        set { if (double.IsFinite(value) && Set(ref volume, Math.Clamp(value, 0, 100))) { if (player != null) player.Volume = volume / 100; Changed(); } }
    }
    public bool IsMuted
    {
        get => muted;
        set { if (Set(ref muted, value)) { if (player != null) player.IsMuted = value; Changed(); } }
    }
    public string PlayLabel => playing ? "Pausar" : "Reproduzir";
    public double Duration { get => duration; private set => Set(ref duration, value); }
    public bool CanSeek { get => canSeek; private set => Set(ref canSeek, value); }
    public double Position
    {
        get => position;
        set
        {
            if (!CanSeek || !double.IsFinite(value)) return;
            position = Math.Clamp(value, RangeStart, RangeEnd > RangeStart ? RangeEnd : Duration);
            Run(() => { if (player != null) player.PlaybackSession.Position = TimeSpan.FromSeconds(position); });
            Notify(nameof(Position)); Notify(nameof(TimeLabel));
        }
    }
    public string TimeLabel => $"{TimeSpan.FromSeconds(position):hh\\:mm\\:ss} / {TimeSpan.FromSeconds(duration):hh\\:mm\\:ss}";

    public async Task Initialize()
    {
        try
        {
            watcher = DeviceInformation.CreateWatcher(MediaDevice.GetAudioRenderSelector());
            watcher.Added += DeviceAdded;
            watcher.Removed += DeviceRemoved;
            watcher.Updated += DeviceUpdated;
            watcher.Start();
            await RefreshDevices();
        }
        catch (Exception ex) { Status = "Não foi possível listar as saídas de áudio. " + ex.Message; }
    }

    private void Dispatch(Action action)
    {
        if (disposed || dispatcher.HasShutdownStarted) return;
        _ = dispatcher.BeginInvoke(new Action(() => { if (!disposed) Run(action); }));
    }
    private void DeviceAdded(DeviceWatcher sender, DeviceInformation args) => Dispatch(() =>
    {
        if (!args.IsEnabled || Outputs.Any(d => d.Id == args.Id)) return;
        Outputs.Add(args);
        if (args.Id == preferredOutputId && SelectedOutput == null)
        {
            restoring = true; SelectedOutput = args; restoring = false;
            Status = "Saída disponível. Pressione Reproduzir para iniciar.";
        }
    });
    private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args) => Dispatch(() => RemoveOutput(args.Id));
    private void DeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args) => Dispatch(() =>
    {
        var device = Outputs.FirstOrDefault(d => d.Id == args.Id);
        device?.Update(args);
        if (device is { IsEnabled: false }) RemoveOutput(args.Id);
        else _ = RefreshDevices();
    });
    private void RemoveOutput(string id)
    {
        if (SelectedOutput?.Id == id)
        {
            restoring = true; SelectedOutput = null; restoring = false;
            Status = "Saída desconectada. Reconecte o dispositivo e pressione Reproduzir.";
        }
        var item = Outputs.FirstOrDefault(d => d.Id == id);
        if (item != null) Outputs.Remove(item);
        CommandManager.InvalidateRequerySuggested();
    }
    private async Task RefreshDevices()
    {
        if (disposed || refreshing) return;
        refreshing = true;
        CommandManager.InvalidateRequerySuggested();
        try
        {
            var devices = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioRenderSelector()).AsTask().WaitAsync(TimeSpan.FromSeconds(8));
            if (disposed) return;
            var enabled = devices.Where(d => d.IsEnabled).ToArray();
            foreach (var old in Outputs.ToArray()) if (!enabled.Any(d => d.Id == old.Id)) RemoveOutput(old.Id);
            foreach (var device in enabled) if (!Outputs.Any(d => d.Id == device.Id)) Outputs.Add(device);
            if (SelectedOutput == null && preferredOutputId != null)
            {
                restoring = true; SelectedOutput = Outputs.FirstOrDefault(d => d.Id == preferredOutputId); restoring = false;
            }
            if (Outputs.Count == 0) Status = "Nenhuma saída disponível. Conecte o Cube Baby nas configurações Bluetooth.";
        }
        catch (Exception ex) { Status = "Não foi possível listar as saídas de áudio. " + ex.Message; }
        finally { refreshing = false; CommandManager.InvalidateRequerySuggested(); }
    }

    private void AddFilesCore()
    {
        var dialog = new OpenFileDialog { Multiselect = true, Filter = LanguageService.T("Áudio MP3 ou WAV") + "|*.mp3;*.wav" };
        if (dialog.ShowDialog() != true) return;
        AddPaths(dialog.FileNames);
    }
    public void AddPaths(IEnumerable<string> paths)
    {
        foreach (string source in paths)
        {
            if (!Path.IsPathFullyQualified(source) || !File.Exists(source) || Path.GetExtension(source).ToLowerInvariant() is not (".mp3" or ".wav")) continue;
            if (Tracks.Any(t => string.Equals(t.Source, source, StringComparison.OrdinalIgnoreCase))) continue;
            if (Tracks.Count >= 3) { Status = "A sequência aceita até três arquivos. Remova uma faixa para adicionar outra."; break; }
            Tracks.Add(new(source));
        }
        SelectedTrack ??= Tracks.FirstOrDefault(); Changed();
    }
    public void MoveTrack(BackingTrack track, int index)
    {
        int old = Tracks.IndexOf(track);
        if (old < 0 || index < 0 || index >= Tracks.Count) return;
        Tracks.Move(old, index); Changed();
    }
    private void MoveSelected(int step) { if (SelectedTrack != null) MoveTrack(SelectedTrack, Tracks.IndexOf(SelectedTrack) + step); }
    private async Task SelectAdjacent(int step)
    {
        int index = SelectedTrack == null ? -1 : Tracks.IndexOf(SelectedTrack) + step;
        if (index < 0 || index >= Tracks.Count) return;
        bool resume = playing;
        SelectedTrack = Tracks[index];
        if (resume) await TogglePlayback();
    }
    private async Task FinishTrack()
    {
        if (advancing || player == null) return;
        if (IsLooping) { Position = RangeStart; player.Play(); return; }
        advancing = true;
        try
        {
            int next = SelectedTrack == null ? Tracks.Count : Tracks.IndexOf(SelectedTrack) + 1;
            if (next < Tracks.Count) { SelectedTrack = Tracks[next]; await TogglePlayback(); }
            else { StopPlayback(); Status = "Sequência concluída."; }
        }
        finally { advancing = false; }
    }
    private async Task LoadWaveform()
    {
        waveformCancellation?.Cancel(); waveformCancellation?.Dispose();
        var cancellation = waveformCancellation = new CancellationTokenSource();
        var track = SelectedTrack;
        if (track == null) return;
        try
        {
            if (track.Peaks.Length == 0)
            {
                Status = "Desenhando a forma de onda…";
                var result = await BackingAudio.Analyze(track.Source, cancellation.Token);
                if (cancellation.IsCancellationRequested || disposed || SelectedTrack != track) return;
                track.Length = result.Duration; track.Peaks = result.Peaks;
                if (track.End <= 0) track.End = track.Length;
            }
            Duration = track.Length; CanSeek = Duration > 0;
            Notify(nameof(Peaks)); RangeChanged(); Notify(nameof(TimeLabel));
            Status = "Arraste os marcadores A/B para escolher um trecho.";
        }
        catch (OperationCanceledException) { }
        catch (Exception) { if (!disposed && SelectedTrack == track && !cancellation.IsCancellationRequested) Status = "Forma de onda indisponível para este arquivo. A reprodução continua disponível."; }
    }
    private async Task ExportSelection()
    {
        if (SelectedTrack is not { } track || exporting) return;
        var dialog = new SaveFileDialog { Filter = "WAV|*.wav", FileName = track.Title + "-trecho.wav" };
        if (dialog.ShowDialog() != true) return;
        if (Tracks.Any(t => string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(t.Source), StringComparison.OrdinalIgnoreCase))) { Status = "Escolha outro nome para preservar o áudio original."; return; }
        double start = RangeStart, end = RangeEnd;
        exporting = true; CommandManager.InvalidateRequerySuggested();
        try { await BackingAudio.Export(track.Source, dialog.FileName, start, end); Status = "Trecho exportado. O áudio original foi preservado."; }
        catch (Exception) { Status = "Não foi possível exportar o trecho."; }
        finally { exporting = false; CommandManager.InvalidateRequerySuggested(); }
    }
    private void RemoveSelected()
    {
        if (SelectedTrack is not { } track) return;
        SelectedTrack = null; Tracks.Remove(track); SelectedTrack = Tracks.FirstOrDefault(); Changed();
    }

    private async Task TogglePlayback()
    {
        if (disposed || loading || SelectedTrack == null || SelectedOutput == null) return;
        int generation = playbackGeneration;
        try
        {
            if (player != null)
            {
                if (playing) { resumeAfterRateChange = false; player.Pause(); SetPlaying(false); Status = "Reprodução pausada."; }
                else { resumeAfterRateChange = true; if (rateChangePending) ApplyPendingRate(); else player.Play(); SetPlaying(true); Status = "Reproduzindo backing track."; }
                return;
            }
            generation = ++playbackGeneration;
            loading = true; Status = "Abrindo backing track…"; CommandManager.InvalidateRequerySuggested();
            string source = SelectedTrack.Source;
            string outputId = SelectedOutput.Id;
            var outputs = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioRenderSelector()).AsTask().WaitAsync(TimeSpan.FromSeconds(8));
            if (disposed || generation != playbackGeneration) return;
            var output = outputs.FirstOrDefault(d => d.Id == outputId && d.IsEnabled);
            if (output == null) { RemoveOutput(outputId); return; }
            if (!File.Exists(source)) throw new FileNotFoundException(LanguageService.T("Arquivo de áudio não encontrado."), source);
            var file = await StorageFile.GetFileFromPathAsync(source);
            if (disposed || generation != playbackGeneration) return;
            var media = MediaSource.CreateFromStorageFile(file);
            MediaPlayer next;
            try
            {
                next = new MediaPlayer(); player = next;
                next.AutoPlay = false; next.AudioDevice = output; next.Volume = Volume / 100; next.IsMuted = IsMuted;
            }
            catch { media.Dispose(); throw; }
            next.PlaybackSession.PlaybackStateChanged += (session, _) => Dispatch(() =>
            {
                if (player != next) return;
                ApplyPendingRate();
            });
            next.MediaOpened += (sender, _) => Dispatch(() =>
            {
                if (player != sender) return;
                loading = false;
                Duration = Math.Max(0, sender.PlaybackSession.NaturalDuration.TotalSeconds);
                if (SelectedTrack != null) { SelectedTrack.Length = Duration; SelectedTrack.End = SelectedTrack.End <= 0 ? Duration : Math.Min(SelectedTrack.End, Duration); if (SelectedTrack.Start >= SelectedTrack.End) SelectedTrack.Start = 0; }
                sender.PlaybackSession.PlaybackRate = Speed / 100;
                sender.PlaybackSession.Position = TimeSpan.FromSeconds(Math.Clamp(position, RangeStart, RangeEnd));
                UpdatePosition(); RangeChanged(); sender.Play(); SetPlaying(true); Status = "Reproduzindo backing track.";
            });
            next.MediaEnded += (sender, _) => Dispatch(() => { if (player == sender) _ = FinishTrack(); });
            next.MediaFailed += (sender, _) => Dispatch(() =>
            {
                if (player != sender) return;
                StopPlayback(); Status = "Não foi possível reproduzir. Verifique a saída e o arquivo de áudio.";
            });
            try { next.Source = media; }
            catch { media.Dispose(); throw; }
            positionTimer.Start();
        }
        catch (Exception ex)
        {
            if (!disposed && generation == playbackGeneration) { StopPlayback(); Status = "Não foi possível reproduzir. " + ex.Message; }
        }
        finally { if (generation == playbackGeneration && player == null) loading = false; CommandManager.InvalidateRequerySuggested(); }
    }
    private void SetPlaying(bool value) { playing = value; Notify(nameof(PlayLabel)); Notify(nameof(PlayIcon)); Notify(nameof(IsPlaying)); Notify(nameof(HasActivePlayback)); CommandManager.InvalidateRequerySuggested(); }
    private void UpdatePosition()
    {
        if (player == null) return;
        var session = player.PlaybackSession;
        Duration = Math.Max(0, session.NaturalDuration.TotalSeconds);
        CanSeek = session.CanSeek && Duration > 0;
        position = Math.Max(0, session.Position.TotalSeconds);
        if (playing && RangeEnd > RangeStart && position >= RangeEnd) _ = FinishTrack();
        Notify(nameof(Position)); Notify(nameof(TimeLabel));
    }
    private void StopPlayback()
    {
        playbackGeneration++;
        rateChangePending = resumeAfterRateChange = false;
        positionTimer.Stop();
        var previous = player; player = null;
        if (previous != null) { var source = previous.Source as MediaSource; previous.Dispose(); source?.Dispose(); }
        loading = false; SetPlaying(false); position = RangeStart; Duration = SelectedTrack?.Length ?? 0; CanSeek = Duration > 0;
        Notify(nameof(Position)); Notify(nameof(TimeLabel));
    }
    private void Run(Action action)
    {
        try { action(); }
        catch (Exception ex) { Status = "Operação de áudio não concluída. " + ex.Message; }
    }
    private void Changed()
    {
        CommandManager.InvalidateRequerySuggested();
        if (restoring || disposed) return;
        settingsDirty = true;
        saveTimer.Stop(); saveTimer.Start();
    }
    private void Restore()
    {
        restoring = true;
        try
        {
            if (!File.Exists(settingsPath)) return;
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsPath));
            if (settings?.Version != 1 || settings.Sources == null) return;
            foreach (var source in settings.Sources.Where(s => !string.IsNullOrWhiteSpace(s) && Path.IsPathFullyQualified(s) && (Path.GetExtension(s).ToLowerInvariant() is ".mp3" or ".wav")).Distinct().Take(3))
                Tracks.Add(new(source));
            selectedTrack = Tracks.FirstOrDefault(t => t.Source == settings.SelectedSource) ?? Tracks.FirstOrDefault();
            preferredOutputId = settings.OutputId;
            volume = double.IsFinite(settings.Volume) ? Math.Clamp(settings.Volume, 0, 100) : 30;
            muted = settings.IsMuted;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException or ArgumentException)
        { Status = "Não foi possível restaurar a lista de backing tracks."; }
        finally { restoring = false; }
    }
    private void Save()
    {
        if (!settingsDirty) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(new Settings(1, Tracks.Select(t => t.Source).ToArray(), SelectedTrack?.Source, preferredOutputId, Volume, IsMuted)));
            settingsDirty = false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { Status = "Não foi possível salvar a lista de backing tracks. " + ex.Message; }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; waveformCancellation?.Cancel(); waveformCancellation?.Dispose(); saveTimer.Stop(); StopPlayback(); Save();
        if (watcher != null)
        {
            watcher.Added -= DeviceAdded; watcher.Removed -= DeviceRemoved; watcher.Updated -= DeviceUpdated;
            if (watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted) watcher.Stop();
        }
    }
}

internal static class BackingAudio
{
    public static async Task<(double Duration, float[] Peaks)> Analyze(string path, CancellationToken cancellation)
    {
        var file = await StorageFile.GetFileFromPathAsync(path).AsTask(cancellation);
        using var input = await file.OpenReadAsync().AsTask(cancellation);
        using var output = new InMemoryRandomAccessStream();
        var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
        profile.Audio = AudioEncodingProperties.CreatePcm(8000, 1, 16);
        var prepared = await new MediaTranscoder().PrepareStreamTranscodeAsync(input, output, profile).AsTask(cancellation);
        if (!prepared.CanTranscode) throw new IOException("Audio decoder unavailable.");
        await prepared.TranscodeAsync().AsTask(cancellation);
        output.Seek(0);
        using var stream = output.AsStreamForRead();
        return await Task.Run(() => ReadPeaks(stream, cancellation), cancellation);
    }
    private static (double Duration, float[] Peaks) ReadPeaks(Stream stream, CancellationToken cancellation)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, true);
        if (new string(reader.ReadChars(4)) != "RIFF") throw new IOException("Invalid waveform.");
        reader.ReadUInt32();
        if (new string(reader.ReadChars(4)) != "WAVE") throw new IOException("Invalid waveform.");
        uint rate = 0; ushort align = 0;
        while (stream.Position + 8 <= stream.Length)
        {
            string id = new(reader.ReadChars(4)); uint size = reader.ReadUInt32(); long next = Math.Min(stream.Length, stream.Position + size + (size & 1));
            if (id == "fmt " && size >= 16)
            {
                ushort format = reader.ReadUInt16(), channels = reader.ReadUInt16(); rate = reader.ReadUInt32(); reader.ReadUInt32(); align = reader.ReadUInt16(); ushort bits = reader.ReadUInt16();
                if (format == 0xFFFE && size >= 40)
                {
                    reader.ReadUInt16(); reader.ReadUInt16(); reader.ReadUInt32();
                    format = (ushort)reader.ReadUInt32();
                }
                if (format != 1 || channels != 1 || bits != 16 || align != 2) throw new IOException("Unsupported waveform format.");
            }
            else if (id == "data" && rate > 0 && align > 0)
            {
                long count = Math.Min(size, stream.Length - stream.Position) / align;
                var peaks = new float[1800];
                for (long i = 0; i < count; i++)
                {
                    if ((i & 8191) == 0) cancellation.ThrowIfCancellationRequested();
                    int bin = (int)(i * peaks.Length / Math.Max(1, count));
                    peaks[bin] = Math.Max(peaks[bin], Math.Abs(reader.ReadInt16() / 32768f));
                }
                return ((double)count / rate, peaks);
            }
            stream.Position = next;
        }
        throw new IOException("Empty waveform.");
    }
    public static async Task Export(string source, string destination, double start, double end)
    {
        var file = await StorageFile.GetFileFromPathAsync(source);
        using var input = await file.OpenReadAsync();
        using var output = new InMemoryRandomAccessStream();
        var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
        var original = await MediaEncodingProfile.CreateFromFileAsync(file);
        if (original.Audio != null)
            profile.Audio = AudioEncodingProperties.CreatePcm(original.Audio.SampleRate, original.Audio.ChannelCount, 16);
        var transcoder = new MediaTranscoder { TrimStartTime = TimeSpan.FromSeconds(start), TrimStopTime = TimeSpan.FromSeconds(end) };
        var prepared = await transcoder.PrepareStreamTranscodeAsync(input, output, profile);
        if (!prepared.CanTranscode) throw new IOException("Audio export unavailable.");
        await prepared.TranscodeAsync();
        output.Seek(0);
        using var stream = output.AsStreamForRead();
        await using var target = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await stream.CopyToAsync(target);
    }
}
