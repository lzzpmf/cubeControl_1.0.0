using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Studio.Audio;

public sealed class StudioAudioEngine : IStudioAudioEngine
{
    private readonly string dataDirectory, settingsPath;
    private readonly MediaLibrary media;
    private readonly Func<StudioAudioSettings, AudioRing, IAudioBackend> createBackend;
    private readonly SemaphoreSlim snapshotUpdate = new(1, 1);
    private readonly BlockingCollection<Action> commands = new();
    private readonly Thread worker;
    private readonly CancellationTokenSource lifetime = new();
    private readonly Dictionary<string, PreparedMedia> sources = new(StringComparer.OrdinalIgnoreCase);
    private StudioAudioSettings settings = new();
    private StudioSnapshot project = new();
    private readonly MetronomeClock metronome = new();
    private volatile StudioTransportSnapshot state = new(false, false, 0, 80);
    private StudioAudioCapabilities capabilities = new([44100, 48000, 96000], [64, 128, 256, 512], 0, 2, 48000, 256);
    private IAudioBackend? backend;
    private AudioRing? ring;
    private AudioRenderer? renderer;
    private RecordingSink? recording;
    private IReadOnlyList<StudioRecordedSegment> pendingRecordings = [];
    private bool playing, outputRunning, ended, disposed;
    private double position, playStart, playClockStart;
    private float outputPeak, inputPeak;
    private string? lastError;
    private float[] renderBuffer = new float[2048];
    private float[] monitorBuffer = [];
    private AudioRing? monitorRing;
    private StereoPeakGuard? monitorGuard;
    private long lastUnderruns;
    private int underrunStreak;

    public StudioAudioEngine(string dataDirectory, string? toolsDirectory = null)
        : this(dataDirectory, toolsDirectory, (settings, ring) => settings.Driver == StudioDriver.Asio ? new AsioBackend(settings, ring) : new WasapiBackend(settings, ring)) { }

    internal StudioAudioEngine(string dataDirectory, string? toolsDirectory, Func<StudioAudioSettings, AudioRing, IAudioBackend> createBackend)
    {
        this.createBackend = createBackend;
        this.dataDirectory = Path.GetFullPath(dataDirectory);
        settingsPath = Path.Combine(this.dataDirectory, "studio-audio-v1.json");
        media = new(this.dataDirectory, toolsDirectory);
        try { if (File.Exists(settingsPath)) settings = JsonSerializer.Deserialize<StudioAudioSettings>(File.ReadAllText(settingsPath)) ?? new(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { Debug.WriteLine(ex); }
        worker = new Thread(Work) { IsBackground = true, Name = "Cube Studio Audio" };
        worker.SetApartmentState(ApartmentState.STA); worker.Start();
    }

    public StudioTransportSnapshot State => state;
    public StudioAudioSettings Settings => Volatile.Read(ref settings);
    public StudioAudioCapabilities Capabilities => Volatile.Read(ref capabilities);

    private Task<T> Invoke<T>(Func<T> action, CancellationToken cancellation = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var result = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        cancellation.ThrowIfCancellationRequested();
        commands.Add(() =>
        {
            try { cancellation.ThrowIfCancellationRequested(); result.TrySetResult(action()); }
            catch (OperationCanceledException ex) { result.TrySetCanceled(ex.CancellationToken); }
            catch (Exception ex) { lastError = ex.Message; Publish(); result.TrySetException(ex); }
        });
        return result.Task;
    }
    private Task Invoke(Action action, CancellationToken cancellation = default) => Invoke(() => { action(); return true; }, cancellation);

    public Task<IReadOnlyList<StudioDevice>> EnumerateDevicesAsync(CancellationToken cancellation = default) => Invoke<IReadOnlyList<StudioDevice>>(() =>
    {
        var devices = new List<StudioDevice>();
        using var enumerator = new MMDeviceEnumerator();
        foreach (var flow in new[] { DataFlow.Render, DataFlow.Capture })
            foreach (var device in enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active))
                using (device)
                using (var client = device.CreateAudioClient())
                    devices.Add(new(device.ID, device.FriendlyName, StudioDriver.Wasapi, flow == DataFlow.Capture, client.MixFormat.Channels));
        foreach (string driver in AsioDevice.GetDriverNames())
        { devices.Add(new(driver, driver, StudioDriver.Asio, false, 0)); devices.Add(new(driver, driver, StudioDriver.Asio, true, 0)); }
        return devices;
    }, cancellation);

    public Task<StudioAudioCapabilities> GetCapabilitiesAsync(StudioAudioSettings value, CancellationToken cancellation = default) => Invoke(() =>
    {
        ValidateSettings(value);
        if (backend != null && (value == settings || value.Driver == StudioDriver.Asio && settings.Driver == StudioDriver.Asio && value.OutputId == settings.OutputId)) return capabilities;
        if (value.Driver == StudioDriver.Asio)
        {
            if (string.IsNullOrEmpty(value.OutputId)) return new StudioAudioCapabilities([], [], 0, 0, value.SampleRate, value.BufferFrames);
            using var device = AsioDevice.Open(value.OutputId);
            var caps = device.Capabilities;
            return new StudioAudioCapabilities(new[] { 44100, 48000, 96000 }.Where(device.IsSampleRateSupported).ToArray(),
                new[] { 64, 128, 256, 512, 1024, 2048 }.Where(n => n >= caps.BufferMinSize && n <= caps.BufferMaxSize &&
                    (caps.BufferGranularity <= 0 || (n - caps.BufferMinSize) % caps.BufferGranularity == 0)).Append(caps.BufferPreferredSize).Distinct().Order().ToArray(),
                caps.NbInputChannels, caps.NbOutputChannels, device.CurrentSampleRate, caps.BufferPreferredSize);
        }
        using var enumerator = new MMDeviceEnumerator();
        using var output = string.IsNullOrEmpty(value.OutputId) ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia) : enumerator.GetDevice(value.OutputId);
        using var outputClient = output.CreateAudioClient();
        using var input = !string.IsNullOrEmpty(value.InputId) ? enumerator.GetDevice(value.InputId) : null;
        using var inputClient = input?.CreateAudioClient();
        int[] rates = value.Exclusive ? new[] { 44100, 48000, 96000 }.Where(rate => outputClient.IsFormatSupported(AudioClientShareMode.Exclusive, WaveFormat.CreateIeeeFloatWaveFormat(rate, 2))).ToArray() : [44100, 48000, 96000];
        return new StudioAudioCapabilities(rates, [64, 128, 256, 512, 1024], inputClient?.MixFormat.Channels ?? 0,
            outputClient.MixFormat.Channels, outputClient.MixFormat.SampleRate, value.BufferFrames);
    }, cancellation);

    public Task<StudioAudioCapabilities> ConfigureAsync(StudioAudioSettings value, CancellationToken cancellation = default) => Invoke(() =>
    {
        ValidateSettings(value);
        if (recording != null) throw new InvalidOperationException("Conclua a gravação antes de mudar o dispositivo.");
        UpdatePosition(); bool resume = playing;
        CloseBackend(); settings = value;
        try { EnsureBackend(); if (resume) StartPlayback(); else EnsureMetronomeOutput(); }
        catch { CloseBackend(); playing = false; throw; }
        Directory.CreateDirectory(dataDirectory);
        string temporary = settingsPath + ".tmp";
        try { File.WriteAllText(temporary, JsonSerializer.Serialize(settings)); File.Move(temporary, settingsPath, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        lastError = null; Publish(); return capabilities;
    }, cancellation);

    public async Task<StudioMediaInfo> AnalyzeAsync(string path, CancellationToken cancellation = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellation);
        return await media.Analyze(path, linked.Token).ConfigureAwait(false);
    }

    public async Task ApplySnapshotAsync(StudioSnapshot snapshot, CancellationToken cancellation = default)
    {
        snapshot = Freeze(snapshot); ValidateSnapshot(snapshot);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellation);
        await snapshotUpdate.WaitAsync(linked.Token).ConfigureAwait(false);
        try
        {
        var prepared = new Dictionary<string, PreparedMedia>(StringComparer.OrdinalIgnoreCase);
        foreach (string path in snapshot.Tracks.SelectMany(t => t.Clips).Select(c => c.Path).Distinct(StringComparer.OrdinalIgnoreCase))
            prepared[path] = await media.Prepare(path, linked.Token).ConfigureAwait(false);
        await Invoke(() =>
        {
            bool same = SameArrangement(project, snapshot);
            if (recording != null && !same) throw new InvalidOperationException("Conclua a gravação antes de editar clipes, velocidade ou transposição.");
            UpdatePosition(); project = snapshot;
            foreach (var entry in prepared) sources[entry.Key] = entry.Value;
            if (same) renderer?.UpdateMix(project);
            else if (playing) RestartAtPosition();
            else { renderer?.Dispose(); renderer = null; position = Math.Min(position, project.Duration); }
            lastError = null; Publish();
        }, linked.Token).ConfigureAwait(false);
        }
        finally { snapshotUpdate.Release(); }
    }

    public Task PlayAsync(CancellationToken cancellation = default) => Invoke(() =>
    { if (!playing) { if (position >= project.Duration && recording == null) position = project.LoopEnabled ? project.LoopStart : 0; StartPlayback(); } }, cancellation);
    public Task PauseAsync(CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) FinishRecordingCore(false);
        UpdatePosition(); playing = false; SuspendMedia(); renderer?.Dispose(); renderer = null; Publish();
    }, cancellation);
    public Task StopAsync(CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) FinishRecordingCore(false);
        metronome.Configure(metronome.Settings with { Enabled = false }, false);
        playing = false; PauseOutput(); renderer?.Dispose(); renderer = null; position = 0; lastError = null; Publish();
    }, cancellation);
    public Task SeekAsync(double value, CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) throw new InvalidOperationException("Conclua a gravação antes de mover o cursor.");
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
        position = Math.Clamp(value, 0, project.Duration);
        if (playing) RestartAtPosition(); Publish();
    }, cancellation);

    public Task ReleaseOutputAsync(CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) throw new InvalidOperationException("Conclua a gravação antes de liberar o dispositivo.");
        UpdatePosition(); metronome.Configure(metronome.Settings with { Enabled = false }, false);
        playing = false; CloseBackend(); Publish();
    }, cancellation);

    public Task ConfigureMetronomeAsync(StudioMetronomeSettings settings, bool resetPhase = false, CancellationToken cancellation = default) => Invoke(() =>
    {
        if (!double.IsFinite(settings.Bpm) || settings.Bpm is < 30 or > 300 || !double.IsFinite(settings.VolumeDb) || settings.VolumeDb is < -61 or > 6 ||
            settings.Meter is not ("4/4" or "3/4" or "6/8") || settings.Sound is not ("Click" or "Madeira" or "Bloco" or "Beep"))
            throw new ArgumentException("Configuração do metrônomo inválida.");
        metronome.Configure(settings, resetPhase);
        try
        {
            if (settings.Enabled) EnsureMetronomeOutput();
            else if (!playing && recording == null) PauseOutput();
        }
        catch { metronome.Configure(settings with { Enabled = false }, false); throw; }
        lastError = null; Publish();
    }, cancellation);

    public Task StartRecordingAsync(StudioRecordingRequest request, CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) return;
        if (request.TrackId == Guid.Empty || !project.Tracks.Any(t => t.Id == request.TrackId)) throw new InvalidOperationException("Arme uma faixa para gravar.");
        if (request.InputChannels is < 1 or > 2 || request.InputChannel < 0) throw new ArgumentOutOfRangeException(nameof(request));
        UpdatePosition(); SuspendMedia();
        if (settings.InputChannel != request.InputChannel || settings.InputChannels != request.InputChannels)
        { CloseBackend(); settings = settings with { InputChannel = request.InputChannel, InputChannels = request.InputChannels }; }
        EnsureBackend();
        backend!.StartCapture(); var queue = backend.Capture!; queue.Discard();
        pendingRecordings = [];
        recording = new(request, project, queue.SampleRate, queue.Channels, CaptureQueue.ClockSeconds, position,
            settings.RecordingOffsetMilliseconds / 1000, Path.Combine(dataDirectory, "Recordings"));
        try { StartPlayback(); }
        catch { recording.Dispose(); recording = null; backend.StopCapture(); throw; }
        Publish();
    }, cancellation);

    public Task<IReadOnlyList<StudioRecordedSegment>> FinishRecordingAsync(CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) FinishRecordingCore(false);
        var result = pendingRecordings; pendingRecordings = []; Publish(); return result;
    }, cancellation);

    private void FinishRecordingCore(bool incomplete)
    {
        if (recording == null) return;
        try { backend?.StopCapture(); DrainCapture(); }
        catch (Exception ex)
        {
            incomplete = true; lastError = ex.Message;
            try { UpdatePosition(); PauseOutput(); } catch (Exception) { }
            playing = false;
        }
        pendingRecordings = recording.Finish(incomplete); recording.Dispose(); recording = null;
        if (playing)
        {
            UpdatePosition(); project = project with { Duration = Math.Max(project.Duration, position) };
            StartPlayback();
        }
    }

    public Task ShowAsioControlPanelAsync(StudioAudioSettings value, CancellationToken cancellation = default) => Invoke(() =>
    {
        if (recording != null) throw new InvalidOperationException("Conclua a gravação antes de abrir as configurações ASIO.");
        if (value.Driver != StudioDriver.Asio || string.IsNullOrEmpty(value.OutputId)) return;
        if (backend != null && settings.Driver == StudioDriver.Asio && settings.OutputId == value.OutputId) backend.ShowControlPanel();
        else { using var driver = AsioDevice.Open(value.OutputId); driver.ShowControlPanel(); }
    }, cancellation);

    public async Task ExportAsync(StudioSnapshot snapshot, string destination, StudioExportOptions options,
        IProgress<double>? progress = null, CancellationToken cancellation = default)
    {
        snapshot = Freeze(snapshot); ValidateSnapshot(snapshot);
        if (options.SampleRate is not (44100 or 48000 or 96000)) throw new ArgumentOutOfRangeException(nameof(options));
        double start = Math.Clamp(options.Start, 0, snapshot.Duration), end = Math.Clamp(options.End ?? snapshot.Duration, 0, snapshot.Duration);
        if (end <= start) throw new InvalidOperationException("Escolha um intervalo de exportação válido.");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellation);
        var prepared = new Dictionary<string, PreparedMedia>(StringComparer.OrdinalIgnoreCase);
        foreach (string path in snapshot.Tracks.SelectMany(t => t.Clips).Select(c => c.Path).Distinct(StringComparer.OrdinalIgnoreCase))
            prepared[path] = await media.Prepare(path, linked.Token).ConfigureAwait(false);
        string fullDestination = Path.GetFullPath(destination);
        if (prepared.Keys.Any(path => string.Equals(Path.GetFullPath(path), fullDestination, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Escolha um destino diferente dos arquivos de origem.");
        await Task.Run(async () =>
        {
            string temporary = fullDestination + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using var audio = new AudioRenderer(snapshot, prepared, start, end, options.SampleRate, false, options.IncludeMetronome);
                var arguments = new List<string> { "-hide_banner", "-loglevel", "error", "-nostdin", "-f", "f32le", "-ar", options.SampleRate.ToString(CultureInfo.InvariantCulture), "-ac", "2", "-i", "pipe:0" };
                if (options.Format == StudioExportFormat.Mp3) arguments.AddRange(["-c:a", "libmp3lame", "-b:a", "320k", "-f", "mp3"]);
                else arguments.AddRange(["-c:a", "pcm_s24le", "-rf64", "auto", "-f", "wav"]);
                arguments.Add(temporary);
                using var process = new Process { StartInfo = MediaProcess.StartInfo(media.Tool("ffmpeg"), arguments) };
                process.Start();
                using var registration = linked.Token.Register(() => MediaProcess.Kill(process));
                Task<string> errors = process.StandardError.ReadToEndAsync(), output = process.StandardOutput.ReadToEndAsync();
                try
                {
                    var buffer = new float[8192]; int read;
                    while ((read = audio.Read(buffer)) > 0)
                    {
                        linked.Token.ThrowIfCancellationRequested();
                        process.StandardInput.BaseStream.Write(MemoryMarshal.AsBytes(buffer.AsSpan(0, read)));
                        progress?.Report(Math.Clamp((audio.Position - start) / (end - start), 0, 1));
                    }
                    process.StandardInput.Close();
                    await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
                    string error = await errors.ConfigureAwait(false); await output.ConfigureAwait(false);
                    if (process.ExitCode != 0) throw new IOException("Falha ao exportar o áudio. " + error[..Math.Min(error.Length, 300)]);
                }
                finally { MediaProcess.Kill(process); await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false); }
                linked.Token.ThrowIfCancellationRequested();
                File.Move(temporary, fullDestination, overwrite: true); progress?.Report(1);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }, linked.Token).ConfigureAwait(false);
    }

    private void EnsureBackend()
    {
        if (backend != null) return;
        ValidateSettings(settings);
        ring = new(settings.SampleRate, Math.Max(settings.SampleRate / 2, settings.BufferFrames * 8));
        ring.Metronome = metronome; ring.MediaEnabled = false;
        monitorRing = new(settings.SampleRate, settings.SampleRate);
        monitorGuard = new(settings.SampleRate);
        backend = createBackend(settings, ring);
        capabilities = backend.Capabilities;
        renderBuffer = new float[Math.Clamp(capabilities.EffectiveBufferFrames, 128, 1024) * 2];
    }
    private void StartPlayback()
    {
        EnsureBackend(); SuspendMedia();
        renderer?.Dispose();
        double end = project.LoopEnabled ? project.LoopEnd : recording != null ? 86400 : project.Duration;
        renderer = new(project, sources, position, end, settings.SampleRate, project.LoopEnabled, false);
        playStart = position; ended = false; playing = true;
        lastError = null; lastUnderruns = 0; underrunStreak = 0;
        FillOutput();
        if (recording != null) backend!.Capture?.Discard();
        playClockStart = backend!.PlayedSeconds;
        ring!.MediaEnabled = true;
        if (!outputRunning) { backend.Start(); outputRunning = true; }
        UpdateRecordingClock(); Publish();
    }
    private void RestartAtPosition() => StartPlayback();
    private void EnsureMetronomeOutput()
    {
        if (!metronome.Settings.Enabled) return;
        EnsureBackend();
        if (!outputRunning) { ring!.MediaEnabled = playing; backend!.Start(); outputRunning = true; }
    }
    private void SuspendMedia()
    {
        if (ring != null) ring.MediaEnabled = false;
        if (!metronome.Settings.Enabled) PauseOutput();
        else { ring?.Clear(); monitorRing?.Clear(); }
    }
    private void PauseOutput()
    {
        if (outputRunning) { backend?.Pause(); outputRunning = false; }
        if (ring != null) ring.MediaEnabled = false;
        ring?.Clear();
        monitorRing?.Clear();
    }
    private void UpdatePosition()
    {
        if (!playing || backend == null) return;
        double elapsed = Math.Max(0, backend.PlayedSeconds - playClockStart - backend.ClockCompensationSeconds);
        double linear = playStart + elapsed * project.Speed;
        position = project.LoopEnabled && linear >= project.LoopEnd && project.LoopEnd > project.LoopStart
            ? project.LoopStart + (linear - project.LoopEnd) % (project.LoopEnd - project.LoopStart) : linear;
        if (recording == null && !project.LoopEnabled) position = Math.Min(position, project.Duration);
    }
    private void FillOutput()
    {
        if (!playing || renderer == null || ring == null) return;
        int target = Math.Max(capabilities.EffectiveBufferFrames * 2, settings.SampleRate / 100) * 2;
        while (ring.Count < target && ring.Free >= renderBuffer.Length && !ended)
        {
            int count = renderer.Read(renderBuffer);
            if (count == 0) { ended = true; break; }
            if (settings.Monitoring && monitorRing != null)
            {
                if (monitorBuffer.Length < count) monitorBuffer = new float[count];
                monitorRing.Read(monitorBuffer.AsSpan(0, count));
                for (int i = 0; i < count; i += 2)
                {
                    float left = renderBuffer[i] + monitorBuffer[i], right = renderBuffer[i + 1] + monitorBuffer[i + 1];
                    monitorGuard!.Process(ref left, ref right);
                    renderBuffer[i] = left; renderBuffer[i + 1] = right;
                }
            }
            outputPeak = 0;
            for (int i = 0; i < count; i++)
            {
                outputPeak = Math.Max(outputPeak, Math.Abs(renderBuffer[i]));
            }
            ring.Write(renderBuffer.AsSpan(0, count));
        }
    }
    private void DrainCapture()
    {
        var queue = backend?.Capture;
        if (queue == null) return;
        if (queue.Overflow != 0) throw new IOException("A gravação não acompanhou a entrada. A tomada parcial foi preservada.");
        UpdateRecordingClock();
        CapturePacket? packet;
        while ((packet = queue.Peek()) != null)
        {
            try
            {
                recording?.Process(packet);
                inputPeak = recording?.InputPeak ?? 0;
                if (settings.Monitoring)
                {
                    // The monitoring copy is processed on the worker. Capture bytes stay unchanged.
                    int outputFrames = (int)Math.Round(packet.Frames * settings.SampleRate / (double)queue.SampleRate);
                    if (monitorBuffer.Length < outputFrames * 2) monitorBuffer = new float[outputFrames * 2];
                    for (int f = 0; f < outputFrames; f++)
                    {
                        int source = Math.Min(packet.Frames - 1, (int)(f * queue.SampleRate / (double)settings.SampleRate));
                        monitorBuffer[f * 2] = packet.Samples[source * queue.Channels];
                        monitorBuffer[f * 2 + 1] = packet.Samples[source * queue.Channels + queue.Channels - 1];
                    }
                    if (monitorRing != null)
                    {
                        if (monitorRing.Free < outputFrames * 2)
                            throw new IOException("O monitoramento não acompanhou a entrada. A tomada parcial foi preservada.");
                        monitorRing.Write(monitorBuffer.AsSpan(0, outputFrames * 2));
                    }
                }
            }
            finally { queue.Release(); }
        }
    }
    private void UpdateRecordingClock()
    {
        if (recording != null && backend != null)
        {
            double clock = backend.PlayedSeconds;
            recording.UpdateClock(clock, CaptureQueue.ClockSeconds, playClockStart + backend.ClockCompensationSeconds);
        }
    }
    private void Work()
    {
        try
        {
            while (!commands.IsCompleted)
            {
                if (commands.TryTake(out var action, 3)) action();
                try
                {
                    if (backend?.Error is { } error) throw new IOException(error);
                    DrainCapture(); FillOutput(); UpdatePosition();
                    if (playing && ring != null)
                    {
                        long current = Interlocked.Read(ref ring.Underruns);
                        if (!ended && current > lastUnderruns) underrunStreak++; else underrunStreak = 0;
                        lastUnderruns = current;
                        if (underrunStreak >= 3) throw new IOException("O áudio sofreu interrupções. Aumente o buffer do dispositivo.");
                        if (ended && ring.Count == 0 && recording == null)
                        { playing = false; position = project.Duration; SuspendMedia(); renderer?.Dispose(); renderer = null; }
                    }
                    Publish();
                }
                catch (Exception ex) { Fail(ex); }
            }
        }
        finally { recording?.Dispose(); renderer?.Dispose(); backend?.Dispose(); }
    }
    private void Fail(Exception error)
    {
        lastError = error.Message;
        try { UpdatePosition(); } catch (Exception) { }
        try { backend?.StopCapture(); } catch (Exception) { }
        if (recording != null)
        {
            try { pendingRecordings = recording.Finish(true); } catch (Exception) { }
            recording.Dispose(); recording = null;
        }
        playing = false; metronome.Configure(metronome.Settings with { Enabled = false }, false);
        try { CloseBackend(); } catch (Exception) { }
        Publish();
    }
    private void CloseBackend()
    {
        PauseOutput(); backend?.Dispose(); backend = null; ring = null; monitorRing = null; monitorGuard = null;
        renderer?.Dispose(); renderer = null;
    }
    private void Publish() => state = new(playing, recording != null, position, Math.Max(project.Duration, position), outputRunning ? ring?.Peak ?? outputPeak : 0, inputPeak,
        AudibleLoopPass(), lastError, outputRunning && metronome.Settings.Enabled, outputRunning && metronome.Settings.Enabled && metronome.IsPulse);
    private long AudibleLoopPass()
    {
        if (!playing || backend == null || !project.LoopEnabled) return 0;
        double linear = playStart + Math.Max(0, backend.PlayedSeconds - playClockStart - backend.ClockCompensationSeconds) * project.Speed;
        return linear < project.LoopEnd ? 0 : 1 + (long)((linear - project.LoopEnd) / (project.LoopEnd - project.LoopStart));
    }

    private static bool SameArrangement(StudioSnapshot left, StudioSnapshot right) => left.PlaybackRate == right.PlaybackRate &&
        left.Semitones == right.Semitones && left.LoopEnabled == right.LoopEnabled && left.LoopStart == right.LoopStart && left.LoopEnd == right.LoopEnd &&
        left.Tracks.Count == right.Tracks.Count && left.Tracks.Zip(right.Tracks).All(pair => pair.First.Id == pair.Second.Id && pair.First.Clips.SequenceEqual(pair.Second.Clips));
    private static StudioSnapshot Freeze(StudioSnapshot snapshot) => snapshot with { Tracks = snapshot.Tracks.Select(t => t with { Clips = t.Clips.ToArray() }).ToArray() };
    private static void ValidateSettings(StudioAudioSettings value)
    {
        if (value.SampleRate is not (44100 or 48000 or 96000) || value.BufferFrames is < 32 or > 8192 || value.InputChannel < 0 || value.InputChannels is < 1 or > 2 ||
            !double.IsFinite(value.RecordingOffsetMilliseconds) || Math.Abs(value.RecordingOffsetMilliseconds) > 1000) throw new ArgumentException("Configuração de áudio inválida.");
    }
    private static void ValidateSnapshot(StudioSnapshot snapshot)
    {
        if (!double.IsFinite(snapshot.PlaybackRate) || snapshot.PlaybackRate is < .25 or > 2 ||
            !double.IsFinite(snapshot.Semitones) || snapshot.Semitones is < -12 or > 12 || !double.IsFinite(snapshot.Duration) || snapshot.Duration is < 0 or > 86400 || snapshot.Tracks.Count > 4)
            throw new InvalidDataException("Configuração da sessão inválida.");
        if (snapshot.LoopEnabled && (!double.IsFinite(snapshot.LoopStart) || !double.IsFinite(snapshot.LoopEnd) || snapshot.LoopStart < 0 || snapshot.LoopEnd <= snapshot.LoopStart || snapshot.LoopEnd > snapshot.Duration))
            throw new InvalidDataException("Intervalo de loop inválido.");
        foreach (var track in snapshot.Tracks)
        {
            if (!double.IsFinite(track.VolumeDb) || track.VolumeDb is < -61 or > 6 || !double.IsFinite(track.Pan) || Math.Abs(track.Pan) > 1) throw new InvalidDataException("Mixagem inválida.");
            foreach (var clip in track.Clips)
                if (string.IsNullOrWhiteSpace(clip.Path) || !double.IsFinite(clip.Start) || clip.Start < 0 || !double.IsFinite(clip.Duration) || clip.Duration <= 0 ||
                    !double.IsFinite(clip.SourceOffset) || clip.SourceOffset < 0 || !double.IsFinite(clip.RecordedSpeed) || clip.RecordedSpeed is < .1 or > 10 ||
                    !double.IsFinite(clip.RecordedPitch) || Math.Abs(clip.RecordedPitch) > 12 || !double.IsFinite(clip.ClockRatio) || clip.ClockRatio is < .5 or > 2 ||
                    !double.IsFinite(clip.FadeIn) || clip.FadeIn < 0 || !double.IsFinite(clip.FadeOut) || clip.FadeOut < 0 || clip.Start + clip.Duration > 86400)
                    throw new InvalidDataException("Fonte de áudio inválida.");
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        lifetime.Cancel();
        try { await Invoke(() => { playing = false; if (recording != null) FinishRecordingCore(true); CloseBackend(); }).ConfigureAwait(false); }
        finally
        {
            disposed = true; commands.CompleteAdding();
            await Task.Run(worker.Join).ConfigureAwait(false);
            commands.Dispose(); lifetime.Dispose();
        }
    }
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}
