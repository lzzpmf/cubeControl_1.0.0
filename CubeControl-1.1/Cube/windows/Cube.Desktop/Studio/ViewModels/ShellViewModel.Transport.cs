using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cube.Studio.Audio;
using Cube.Studio.Core;
using NAudio.Wave;

namespace Cube.Desktop.Studio.ViewModels;

public sealed partial class ShellViewModel
{
    private IReadOnlyList<StudioDevice> _devices = [];
    private StudioAudioSettings _settings = new();
    private StudioAudioCapabilities? _capabilities;
    private string _driver = "WASAPI", _device = "", _inputDevice = "", _sampleRate = "48000", _bufferSize = "256";
    private bool _monitoring, _deviceReady, _deviceDraft = true, _refreshingAudio;
    private string? _lastAudioError;
    private string _inputChannel = "Entrada 1";
    private readonly SemaphoreSlim _transportGate = new(1, 1);
    public IReadOnlyList<string> Drivers { get; } = ["WASAPI", "ASIO"];
    public IReadOnlyList<string> Devices => _devices.Where(d => d.Driver == (IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi) && !d.IsInput).Select(d => d.Name).Distinct().ToArray();
    public IReadOnlyList<string> InputDevices => _devices.Where(d => d.Driver == (IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi) && d.IsInput).Select(d => d.Name).Distinct().ToArray();
    public IReadOnlyList<string> SampleRates => (_capabilities?.SampleRates ?? []).Select(v => v.ToString()).ToArray();
    public IReadOnlyList<string> BufferSizes => (_capabilities?.BufferFrames ?? []).Select(v => v.ToString()).ToArray();
    public IReadOnlyList<string> InputChannels
    {
        get
        {
            int count = _capabilities?.InputChannels ?? 0;
            var names = new List<string> { "Nenhuma" };
            for (int i = 1; i <= count; i++) names.Add("Entrada " + i);
            for (int i = 1; i < count; i += 2) names.Add($"Estéreo {i}+{i + 1}");
            return names;
        }
    }
    public bool IsAsio => Driver == "ASIO";
    public string InputChannel { get => _inputChannel; set { if (CanChangeAudioSettings && Set(ref _inputChannel, value)) _deviceDraft = true; } }
    public string Driver { get => _driver; set { if (CanChangeAudioSettings && Set(ref _driver, value)) { _deviceDraft = true; OnPropertyChanged(nameof(IsAsio)); UpdateDeviceChoices(); Run(RefreshCapabilitiesAsync); } } }
    public string Device { get => _device; set { if (CanChangeAudioSettings && Set(ref _device, value ?? "")) { _deviceDraft = true; Run(RefreshCapabilitiesAsync); } } }
    public string AudioInputDevice { get => _inputDevice; set { if (CanChangeAudioSettings && Set(ref _inputDevice, value ?? "")) { _deviceDraft = true; Run(RefreshCapabilitiesAsync); } } }
    public string SampleRate { get => _sampleRate; set { if (CanChangeAudioSettings && Set(ref _sampleRate, value ?? "48000")) _deviceDraft = true; } }
    public string BufferSize { get => _bufferSize; set { if (CanChangeAudioSettings && Set(ref _bufferSize, value ?? "256")) _deviceDraft = true; } }
    public bool Monitoring { get => _monitoring; set { if (CanChangeAudioSettings && Set(ref _monitoring, value)) _deviceDraft = true; } }
    public string AudioSummary => _deviceReady ? $"{(_settings.Driver == StudioDriver.Asio ? "ASIO" : "WASAPI")} · {Device} · {_capabilities?.EffectiveSampleRate ?? _settings.SampleRate} Hz · {_capabilities?.EffectiveBufferFrames ?? _settings.BufferFrames} samples" : T("Escolha o dispositivo de áudio");
    public async Task InitializeAsync()
    {
        try
        {
            string path = Path.Combine(_dataDirectory, "studio-audio-v1.json");
            if (File.Exists(path)) _settings = JsonSerializer.Deserialize<StudioAudioSettings>(await File.ReadAllTextAsync(path, _lifetime.Token)) ?? new();
            _driver = _settings.Driver == StudioDriver.Asio ? "ASIO" : "WASAPI";
            _sampleRate = _settings.SampleRate.ToString(); _bufferSize = _settings.BufferFrames.ToString();
            _inputChannel = _settings.InputChannels > 1 ? $"Estéreo {_settings.InputChannel + 1}+{_settings.InputChannel + 2}" : $"Entrada {_settings.InputChannel + 1}";
            // Software monitoring requires a deliberate action in each run.
            _monitoring = false;
            await RefreshAudioDevicesAsync();
            string recordings = Path.Combine(_dataDirectory, "PracticeRecordings");
            if (Directory.Exists(recordings) && Directory.EnumerateFiles(recordings, "*.recording.json", SearchOption.AllDirectories).Any())
                StatusMessage = "Há áudio recuperável nas opções de áudio.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { StatusMessage = ex.Message; }
    }
    private void UpdateDeviceChoices()
    {
        if (!Devices.Contains(_device)) _device = _devices.FirstOrDefault(d => d.Id == _settings.OutputId && d.Driver == (IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi))?.Name ?? Devices.FirstOrDefault() ?? "";
        if (!InputDevices.Contains(_inputDevice)) _inputDevice = _devices.FirstOrDefault(d => d.Id == _settings.InputId && d.Driver == (IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi))?.Name ?? InputDevices.FirstOrDefault() ?? "";
        foreach (var name in new[] { nameof(Drivers), nameof(Driver), nameof(Devices), nameof(Device), nameof(InputDevices), nameof(AudioInputDevice), nameof(IsAsio), nameof(SampleRate), nameof(BufferSize), nameof(Monitoring), nameof(AudioSummary) }) OnPropertyChanged(name);
    }
    private async Task RefreshAudioDevicesAsync()
    {
        if (_refreshingAudio || IsRecording) return;
        _refreshingAudio = true;
        try { _devices = await _audio.EnumerateDevicesAsync(_lifetime.Token); UpdateDeviceChoices(); await RefreshCapabilitiesAsync(); }
        finally { _refreshingAudio = false; }
    }
    private StudioAudioSettings DraftSettings()
    {
        var driver = IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi;
        var output = _devices.FirstOrDefault(d => d.Driver == driver && !d.IsInput && d.Name == Device);
        var input = _devices.FirstOrDefault(d => d.Driver == driver && d.IsInput && d.Name == AudioInputDevice);
        var channels = Regex.Matches(InputChannel, @"\d+").Select(m => int.Parse(m.Value) - 1).ToArray();
        return _settings with { Driver = driver, OutputId = output?.Id ?? "", InputId = IsAsio ? output?.Id : input?.Id,
            SampleRate = int.TryParse(SampleRate, out int rate) ? rate : 48000,
            BufferFrames = int.TryParse(BufferSize, out int frames) ? frames : 256, Monitoring = Monitoring,
            InputChannel = channels.FirstOrDefault(), InputChannels = Math.Max(1, channels.Length) };
    }
    private async Task RefreshCapabilitiesAsync()
    {
        if (IsRecording || _disposed || string.IsNullOrEmpty(Device)) return;
        var draft = DraftSettings();
        var capabilities = await _audio.GetCapabilitiesAsync(draft, _lifetime.Token);
        if (_disposed || DraftSettings().OutputId != draft.OutputId || DraftSettings().InputId != draft.InputId) return;
        _capabilities = capabilities;
        if (!SampleRates.Contains(_sampleRate)) _sampleRate = SampleRates.Contains("48000") ? "48000" : SampleRates.FirstOrDefault() ?? "48000";
        if (!BufferSizes.Contains(_bufferSize)) _bufferSize = BufferSizes.Contains("256") ? "256" : BufferSizes.FirstOrDefault() ?? "256";
        if (!InputChannels.Contains(_inputChannel)) _inputChannel = InputChannels.Skip(1).FirstOrDefault() ?? "Nenhuma";
        foreach (var name in new[] { nameof(SampleRates), nameof(SampleRate), nameof(BufferSizes), nameof(BufferSize), nameof(InputChannels), nameof(InputChannel), nameof(AudioSummary) }) OnPropertyChanged(name);
    }
    private async Task ApplyAudioSettingsAsync()
    {
        if (IsRecording) return;
        await PauseTransportCoreAsync();
        var draft = DraftSettings();
        if (string.IsNullOrEmpty(draft.OutputId)) throw new InvalidOperationException(T("Escolha a saída de áudio"));
        var caps = await _audio.ConfigureAsync(draft, _lifetime.Token);
        _settings = _audio.Settings; _capabilities = caps; _deviceReady = true; _deviceDraft = false;
        if (_settings.Driver == StudioDriver.Wasapi)
            Downloader.SelectedOutput = Downloader.Outputs.FirstOrDefault(d => d.Id == _settings.OutputId) ?? Downloader.SelectedOutput;
        Directory.CreateDirectory(_dataDirectory);
        string path = Path.Combine(_dataDirectory, "studio-audio-v1.json");
        await File.WriteAllTextAsync(path + ".tmp", JsonSerializer.Serialize(_settings with { Monitoring = false }), _lifetime.Token);
        File.Move(path + ".tmp", path, true);
        OnPropertyChanged(nameof(AudioSummary)); OnPropertyChanged(nameof(InputChannels));
        StatusMessage = "Dispositivo de áudio configurado";
    }
    private async Task EnsureAudioAsync()
    {
        if (_devices.Count == 0) await RefreshAudioDevicesAsync();
        if (!_deviceReady || _deviceDraft) await ApplyAudioSettingsAsync();
    }
    private async Task OpenAsioPanelAsync() { await PauseTransportCoreAsync(); await _audio.ShowAsioControlPanelAsync(DraftSettings(), _lifetime.Token); await RefreshCapabilitiesAsync(); }
    public void Seek(double seconds)
    {
        if (!CanEditTimeline) return;
        Session.Position = Math.Clamp(seconds, 0, Project.Duration); Project.Position = Session.Position;
        Run(() => ExecuteTransportAsync(() => _audio.SeekAsync(Session.Position, _lifetime.Token)));
    }
    private async Task ExecuteTransportAsync(Func<Task> action)
    {
        await _transportGate.WaitAsync(_lifetime.Token);
        _transportBusy = true; RefreshTransport();
        try { await action(); }
        finally { _transportBusy = false; _transportGate.Release(); if (!_disposed) RefreshTransport(); }
    }
    private Task UpdateMetronomeAsync(bool resetPhase = false) => ExecuteTransportAsync(async () =>
    {
        try
        {
            if (MetronomeEnabled)
            {
                StopPreview(); if (BeforePlayback != null) await BeforePlayback();
                await Downloader.StopForIrAudition();
                await EnsureAudioAsync();
            }
            await _audio.ConfigureMetronomeAsync(new StudioMetronomeSettings(MetronomeEnabled, MetronomeBpm, Meter, MetronomeSound, MetronomeVolumeDb), resetPhase, _lifetime.Token);
            _state = _audio.State;
        }
        catch { _metronomeEnabled = false; OnPropertyChanged(nameof(MetronomeEnabled)); throw; }
    });
    private Task PlayPauseAsync() => ExecuteTransportAsync(async () =>
    {
        if (IsPlaying || IsRecording) { await PauseTransportCoreAsync(); return; }
        if (!HasAudio) return;
        await PreparePlaybackAsync();
        NormalizeTransportStart();
        await FlushSnapshotAsync(); await _audio.SeekAsync(Session.Position, _lifetime.Token); await _audio.PlayAsync(_lifetime.Token);
        _state = _audio.State;
    });
    private async Task PreparePlaybackAsync()
    {
        StopPreview(); if (BeforePlayback != null) await BeforePlayback();
        await Downloader.StopForIrAudition(); await EnsureAudioAsync();
    }
    private Task ListenAttemptAsync() => ExecuteTransportAsync(async () =>
    {
        if (SelectedAttempt is not { } attempt || IsRecording) return;
        await PreparePlaybackAsync();
        // Audition the selected performance even when A/B still refers to another section.
        Session.LoopEnabled = false; await FlushSnapshotAsync();
        Session.Position = attempt.AudibleStart;
        await _audio.SeekAsync(Session.Position, _lifetime.Token); await _audio.PlayAsync(_lifetime.Token); _state = _audio.State;
    });
    public void PauseTransport() => Run(PauseTransportAsync);
    public Task PauseTransportAsync() => ExecuteTransportAsync(PauseTransportCoreAsync);
    private async Task PauseTransportCoreAsync()
    {
        if (IsRecording) await FinishCaptureAsync();
        await _audio.PauseAsync(_lifetime.Token); _state = _audio.State; RefreshTransport();
    }
    public Task StopAsync() => ExecuteTransportAsync(StopTransportCoreAsync);
    private async Task StopTransportCoreAsync()
    {
        if (IsRecording) await FinishCaptureAsync();
        await _audio.StopAsync(_lifetime.Token);
        _metronomeEnabled = false; OnPropertyChanged(nameof(MetronomeEnabled));
        _state = _audio.State; Session.Position = 0; Project.Position = 0;
        StopPreview(); RefreshTransport();
    }
    public async Task BeforeIrAuditionAsync()
    {
        if (!CanAuditionIr) throw new InvalidOperationException(T("Finalize a gravação antes de iniciar uma prévia."));
        await ExecuteTransportAsync(async () =>
        {
            if (IsRecording) throw new InvalidOperationException(T("Finalize a gravação antes de iniciar uma prévia."));
            await PauseTransportCoreAsync(); StopPreview(); await Downloader.StopForIrAudition();
            await ReleaseStudyOutputAsync();
        });
    }
    public async Task BeforeYouTubePreviewAsync()
    {
        if (!CanPreviewPlayback) throw new InvalidOperationException(T("Finalize a gravação antes de iniciar uma prévia."));
        await ExecuteTransportAsync(async () =>
        {
            if (IsRecording) throw new InvalidOperationException(T("Finalize a gravação antes de iniciar uma prévia."));
            await PauseTransportCoreAsync(); await ReleaseStudyOutputAsync();
        });
    }
    private async Task ReleaseStudyOutputAsync()
    {
        await _audio.ReleaseOutputAsync(_lifetime.Token);
        _metronomeEnabled = false; OnPropertyChanged(nameof(MetronomeEnabled)); _state = _audio.State;
    }
    private Task ToggleRecordingAsync() => ExecuteTransportAsync(async () =>
    {
        if (IsRecording) { await PauseTransportCoreAsync(); return; }
        await PreparePlaybackAsync(); NormalizeTransportStart(forRecording: true);
        if (!InputChannels.Contains(InputChannel) || InputChannel == "Nenhuma") throw new InvalidOperationException(T("Selecione a entrada da guitarra nas opções de áudio."));
        var channels = Regex.Matches(InputChannel, @"\d+").Select(m => int.Parse(m.Value) - 1).ToArray();
        await _audio.PauseAsync(_lifetime.Token);
        _capturePending = true; _discardApproved = false; SyncPresentation();
        try
        {
            Directory.CreateDirectory(_runDirectory);
            await FlushSnapshotAsync(); await _audio.SeekAsync(Session.Position, _lifetime.Token);
            await _audio.StartRecordingAsync(new StudioRecordingRequest { TrackId = Project.Tracks[1].Id, Directory = _runDirectory, InputChannel = channels[0], InputChannels = channels.Length }, _lifetime.Token);
            _state = _audio.State; StatusMessage = "Gravando";
        }
        catch { _capturePending = false; _preserveRecordings = true; SyncPresentation(); throw; }
    });
    private async Task FinishCaptureAsync()
    {
        Exception? failure = null; int committed = 0;
        try
        {
            var segments = await _audio.FinishRecordingAsync(_lifetime.Token);
            foreach (var segment in segments)
            {
                try
                {
                    using var wav = new WaveFileReader(segment.Path);
                    long frames = wav.Length / wav.WaveFormat.BlockAlign;
                    if (frames == 0) continue;
                    _ownedRecordings.Add(Path.GetFullPath(segment.Path));
                    Editor.AddAttempt(new CaptureResult(segment.Path, wav.WaveFormat.SampleRate, wav.WaveFormat.Channels, frames,
                        segment.RecordedSpeed, (int)Math.Round(segment.RecordedPitch), segment.Start, ClockRatio: segment.ClockRatio));
                    committed++;
                    _media[segment.Path] = new(segment.Path, wav.TotalTime.TotalSeconds, wav.WaveFormat.SampleRate, wav.WaveFormat.Channels, segment.Peaks ?? []);
                    if (segment.Incomplete) _preserveRecordings = true;
                }
                catch (Exception ex) { failure ??= ex; _preserveRecordings = true; }
            }
        }
        catch { _preserveRecordings = true; throw; }
        finally { _capturePending = false; _state = _audio.State; SyncPresentation(); }
        StatusMessage = failure != null ? T("Áudio preservado para recuperação: ") + failure.Message : committed > 0 ? "Tentativas prontas para ouvir" : "Nenhum áudio foi gravado";
    }
    private void NormalizeTransportStart(bool forRecording = false)
    {
        double end = Math.Max(Session.Backing?.End ?? 0, SelectedAttempt?.End ?? 0);
        if (Session.LoopEnabled && Session.Position >= Session.LoopEnd) Session.Position = Session.LoopStart;
        else if (!forRecording && end > 0 && Session.Position >= end) Session.Position = 0;
    }
    private void Tick(object? sender, EventArgs e)
    {
        if (_disposed) return;
        var previous = _state; _state = _audio.State;
        _projecting = true;
        try
        {
            if (_state.IsRecording && _state.Duration > Project.Duration) Project.Duration = Math.Min(86400, _state.Duration);
            if (_state.IsPlaying || previous.IsPlaying)
            { Session.Position = _state.Position; Project.Position = Math.Clamp(_state.Position, 0, Project.Duration); OnPropertyChanged(nameof(TimeDisplay)); }
        }
        finally { _projecting = false; }
        OnPropertyChanged(nameof(MetronomePulse));
        if (previous.IsPlaying != _state.IsPlaying || previous.IsRecording != _state.IsRecording || previous.IsMetronomeRunning != _state.IsMetronomeRunning) RefreshTransport();
        var error = _state.Error;
        if (error != _lastAudioError)
        {
            _lastAudioError = error;
            if (!string.IsNullOrEmpty(error))
            {
                StatusMessage = error; if (_capturePending) _preserveRecordings = true;
                if (!_state.IsMetronomeRunning) { _metronomeEnabled = false; OnPropertyChanged(nameof(MetronomeEnabled)); }
            }
        }
        if (_capturePending && !_transportBusy && (!_state.IsRecording || !string.IsNullOrEmpty(error)))
            Run(() => ExecuteTransportAsync(async () => { if (!_capturePending) return; await FinishCaptureAsync(); if (!string.IsNullOrEmpty(error)) StatusMessage = error; }));
        if (_hoverResult is { } result && _tapClock.Elapsed.TotalSeconds - _hoverSince >= .45) { _hoverResult = null; Run(() => PrepareWaveformAsync(result)); }
        if (_renderDirty && _snapshotWorker.IsCompleted && !_transportBusy && !IsBusy && _tapClock.Elapsed.TotalSeconds >= _snapshotRetryAfter) Run(FlushSnapshotAsync);
    }
}
