using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Studio.Audio;

internal interface IAudioBackend : IDisposable
{
    int SampleRate { get; }
    double PlayedSeconds { get; }
    double OutputLatencySeconds { get; }
    double ClockCompensationSeconds { get; }
    CaptureQueue? Capture { get; }
    StudioAudioCapabilities Capabilities { get; }
    string? Error { get; }
    void Start();
    void Pause();
    void StartCapture();
    void StopCapture();
    void ShowControlPanel();
}

internal sealed class WasapiBackend : IAudioBackend
{
    private readonly MMDevice outputDevice;
    private readonly MMDevice? inputDevice;
    private readonly WasapiPlayer player;
    private WasapiRecorder? recorder;
    private readonly StudioAudioSettings settings;
    private string? error;
    public int SampleRate => settings.SampleRate;
    public double PlayedSeconds => player.GetPosition() / (double)player.OutputWaveFormat.AverageBytesPerSecond;
    public double OutputLatencySeconds => player.AverageLatency.TotalSeconds;
    public double ClockCompensationSeconds => 0; // WASAPI GetPosition is the hardware AudioClock.
    public CaptureQueue? Capture { get; private set; }
    public StudioAudioCapabilities Capabilities { get; private set; }
    public string? Error => Volatile.Read(ref error);

    internal WasapiBackend(StudioAudioSettings settings, AudioRing ring)
    {
        this.settings = settings;
        using var enumerator = new MMDeviceEnumerator();
        outputDevice = string.IsNullOrEmpty(settings.OutputId) ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia) : enumerator.GetDevice(settings.OutputId);
        try
        {
            var builder = new WasapiPlayerBuilder().WithDevice(outputDevice).WithEventSync().WithMmcssThreadPriority("Pro Audio")
                .WithLatency(Math.Max(3, (int)Math.Ceiling(settings.BufferFrames * 1000d / settings.SampleRate)));
            player = settings.Exclusive ? builder.WithExclusiveMode().Build() : builder.WithSharedMode().WithLowLatency().Build();
            player.PlaybackStopped += (_, e) => { if (e.Exception != null) Volatile.Write(ref error, e.Exception.Message); };
            player.Init(ring.ToWaveProvider());
            if (!string.IsNullOrEmpty(settings.InputId)) inputDevice = enumerator.GetDevice(settings.InputId);
            using var inputClient = inputDevice?.CreateAudioClient();
            using var outputClient = outputDevice.CreateAudioClient();
            int inputChannels = inputClient?.MixFormat.Channels ?? 0;
            Capabilities = new([44100, 48000, 96000], [64, 128, 256, 512, 1024], inputChannels,
                outputClient.MixFormat.Channels, settings.SampleRate,
                Math.Max(1, (int)Math.Round(player.AverageLatency.TotalSeconds * settings.SampleRate)));
        }
        catch { player?.Dispose(); outputDevice.Dispose(); inputDevice?.Dispose(); throw; }
    }
    public void Start() => player.Play();
    public void Pause() => player.Pause();
    public void StartCapture()
    {
        if (recorder != null) { if (Capture != null) Capture.Enabled = true; return; }
        if (inputDevice == null) throw new InvalidOperationException("Escolha uma entrada de áudio para gravar.");
        var builder = new WasapiRecorderBuilder().WithDevice(inputDevice).WithEventSync().WithMmcssThreadPriority("Pro Audio")
            .WithBufferLength(Math.Max(5, (int)Math.Ceiling(settings.BufferFrames * 1000d / settings.SampleRate)));
        recorder = settings.Exclusive ? builder.WithExclusiveMode().Build() : builder.WithSharedMode().WithLowLatency().Build();
        var format = recorder.WaveFormat;
        if (settings.InputChannel < 0 || settings.InputChannel + settings.InputChannels > format.Channels)
        { recorder.Dispose(); recorder = null; throw new InvalidOperationException("Os canais de entrada selecionados não estão disponíveis."); }
        Capture = new(format.SampleRate, settings.InputChannel, settings.InputChannels) { Enabled = true };
        recorder.DataAvailable += (data, flags, devicePosition, qpc) => Capture.Native(data, format, flags, devicePosition, qpc);
        recorder.RecordingStopped += (_, e) => { if (e.Exception != null) Volatile.Write(ref error, e.Exception.Message); };
        recorder.StartRecording();
    }
    public void StopCapture() { recorder?.StopRecording(); if (Capture != null) Capture.Enabled = false; recorder?.Dispose(); recorder = null; }
    public void ShowControlPanel() { }
    public void Dispose() { StopCapture(); player.Dispose(); inputDevice?.Dispose(); outputDevice.Dispose(); }
}

internal sealed class AsioBackend : IAudioBackend
{
    private readonly AsioDevice device;
    private readonly AudioRing ring;
    private readonly float[] output;
    private long frames;
    private string? error;
    private volatile bool captureEnabled;
    public int SampleRate { get; }
    public double PlayedSeconds => Interlocked.Read(ref frames) / (double)SampleRate;
    public double OutputLatencySeconds => device.OutputLatencySamples / (double)SampleRate;
    public double ClockCompensationSeconds => OutputLatencySeconds;
    public CaptureQueue? Capture { get; }
    public StudioAudioCapabilities Capabilities { get; }
    public string? Error => Volatile.Read(ref error);
    internal AsioBackend(StudioAudioSettings settings, AudioRing ring)
    {
        this.ring = ring; SampleRate = settings.SampleRate;
        device = AsioDevice.Open(settings.OutputId);
        try
        {
            var caps = device.Capabilities;
            if (caps.NbOutputChannels < 2) throw new InvalidOperationException("O driver ASIO precisa oferecer uma saída estéreo.");
            bool capture = !string.IsNullOrEmpty(settings.InputId);
            if (capture && settings.InputChannel + settings.InputChannels > caps.NbInputChannels)
                throw new InvalidOperationException("Os canais ASIO selecionados não estão disponíveis.");
            if (capture) Capture = new(SampleRate, 0, settings.InputChannels);
            device.InitDuplex(new AsioDuplexOptions { SampleRate = SampleRate, BufferSize = settings.BufferFrames,
                InputChannels = capture ? Enumerable.Range(settings.InputChannel, settings.InputChannels).ToArray() : [],
                OutputChannels = [0, 1], Processor = Process });
            output = new float[device.FramesPerBuffer * 2];
            Capabilities = new(new[] { 44100, 48000, 96000 }.Where(device.IsSampleRateSupported).ToArray(),
                new[] { 64, 128, 256, 512, 1024, 2048 }.Where(n => n >= caps.BufferMinSize && n <= caps.BufferMaxSize &&
                    (caps.BufferGranularity <= 0 || (n - caps.BufferMinSize) % caps.BufferGranularity == 0)).Append(caps.BufferPreferredSize).Distinct().Order().ToArray(),
                caps.NbInputChannels, caps.NbOutputChannels, device.CurrentSampleRate, device.FramesPerBuffer);
            device.Stopped += (_, e) => { if (e.Exception != null) Volatile.Write(ref error, e.Exception.Message); };
            device.DriverResetRequest += (_, _) => Volatile.Write(ref error, "Configuração ASIO alterada. Aplique novamente o dispositivo; a gravação parcial foi preservada.");
            device.ResyncOccurred += (_, _) => Volatile.Write(ref error, "O driver ASIO informou uma interrupção do áudio.");
        }
        catch { device.Dispose(); throw; }
    }
    private void Process(in AsioProcessBuffers buffers)
    {
        ring.Read(output.AsSpan(0, buffers.Frames * 2));
        var left = buffers.GetOutput(0); var right = buffers.GetOutput(1);
        for (int f = 0; f < buffers.Frames; f++) { left[f] = output[f * 2]; right[f] = output[f * 2 + 1]; }
        if (captureEnabled) Capture?.Asio(in buffers, frames / (double)SampleRate - device.InputLatencySamples / (double)SampleRate);
        Interlocked.Add(ref frames, buffers.Frames);
    }
    public void Start() => device.Start();
    public void Pause() => device.Stop();
    public void StartCapture() { if (Capture == null) throw new InvalidOperationException("Escolha uma entrada ASIO para gravar."); captureEnabled = true; Capture.Enabled = true; }
    public void StopCapture() { captureEnabled = false; if (Capture != null) Capture.Enabled = false; }
    public void ShowControlPanel() => device.ShowControlPanel();
    public void Dispose() { StopCapture(); device.Dispose(); }
}
