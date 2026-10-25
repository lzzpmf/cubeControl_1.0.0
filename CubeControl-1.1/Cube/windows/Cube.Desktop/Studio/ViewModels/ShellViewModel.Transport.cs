using System.Collections.ObjectModel;
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
    private string _outputPair = "1 – 2";
    private readonly SemaphoreSlim _transportGate = new(1, 1);
    public IReadOnlyList<string> Drivers { get; } = ["WASAPI", "ASIO"];
    private IReadOnlyList<string> _outputChoices = new ObservableCollection<string>(), _inputChoices = new ObservableCollection<string>(),
        _rateChoices = new ObservableCollection<string>(), _bufferChoices = new ObservableCollection<string>(),
        _channelChoices = new ObservableCollection<string> { "Nenhuma" }, _outputPairs = new ObservableCollection<string> { "1 – 2" };
    private bool _updatingAudioChoices, _loadingAudioCapabilities;
    private int _capabilityRequest;
    private StudioAudioCapabilities? _activeCapabilities;
    public IReadOnlyList<string> Devices => _outputChoices;
    public IReadOnlyList<string> InputDevices => _inputChoices;
    public IReadOnlyList<string> SampleRates => _rateChoices;
    public IReadOnlyList<string> BufferSizes => _bufferChoices;
    public IReadOnlyList<string> InputChannels => _channelChoices;
    public bool CanApplyAudioSettings => CanChangeAudioSettings && !_loadingAudioCapabilities && Devices.Contains(Device)
        && SampleRates.Contains(SampleRate) && BufferSizes.Contains(BufferSize);
    public bool IsAsio => Driver == "ASIO";
    public string AudioDeviceFieldLabel => T(IsAsio ? "Dispositivo ASIO" : "Saída WASAPI");
    public IReadOnlyList<string> OutputPairs => _outputPairs;
    private bool CanUpdateAudioChoice => CanChangeAudioSettings && !_updatingAudioChoices;
    public string OutputPair { get => _outputPair; set { if (CanUpdateAudioChoice && OutputPairs.Contains(value) && Set(ref _outputPair, value)) _deviceDraft = true; } }
    public string InputChannel { get => _inputChannel; set { if (CanUpdateAudioChoice && InputChannels.Contains(value) && Set(ref _inputChannel, value)) _deviceDraft = true; } }
    public string Driver { get => _driver; set { if (CanUpdateAudioChoice && Drivers.Contains(value) && Set(ref _driver, value)) { _deviceDraft = true; UpdateDeviceChoices(); Run(RefreshCapabilitiesAsync); } } }
    public string Device { get => _device; set { if (CanUpdateAudioChoice && Devices.Contains(value) && Set(ref _device, value)) { _deviceDraft = true; Run(RefreshCapabilitiesAsync); } } }
    public string AudioInputDevice { get => _inputDevice; set { if (CanUpdateAudioChoice && InputDevices.Contains(value) && Set(ref _inputDevice, value)) { _deviceDraft = true; Run(RefreshCapabilitiesAsync); } } }
    public string SampleRate { get => _sampleRate; set { if (CanUpdateAudioChoice && SampleRates.Contains(value) && Set(ref _sampleRate, value)) _deviceDraft = true; } }
    public string BufferSize { get => _bufferSize; set { if (CanUpdateAudioChoice && BufferSizes.Contains(value) && Set(ref _bufferSize, value)) _deviceDraft = true; } }
    public bool Monitoring { get => _monitoring; set { if (CanUpdateAudioChoice && Set(ref _monitoring, value)) _deviceDraft = true; } }
    private string AppliedDeviceName => _devices.FirstOrDefault(d => d.Driver == _settings.Driver && !d.IsInput && d.Id == _settings.OutputId)?.Name ?? _settings.OutputId;
    public string AudioDeviceLabel => _deviceReady ? $"{(_settings.Driver == StudioDriver.Asio ? "ASIO" : "WASAPI")} · {AppliedDeviceName}"
        : string.IsNullOrWhiteSpace(Device) ? T("Escolha o dispositivo de áudio") : Driver + " · " + Device;
    public string AudioSummary => _deviceReady ? $"{AudioDeviceLabel} · {_activeCapabilities?.EffectiveSampleRate ?? _settings.SampleRate} Hz · {_activeCapabilities?.EffectiveBufferFrames ?? _settings.BufferFrames} samples" : T("Escolha o dispositivo de áudio");
    public async Task InitializeAsync()
    {
        try
        {
            string path = Path.Combine(_dataDirectory, "studio-audio-v1.json");
            if (File.Exists(path)) _settings = JsonSerializer.Deserialize<StudioAudioSettings>(await File.ReadAllTextAsync(path, _lifetime.Token)) ?? new();
            _driver = _settings.Driver == StudioDriver.Asio ? "ASIO" : "WASAPI";
            _sampleRate = _settings.SampleRate.ToString(); _bufferSize = _settings.BufferFrames.ToString();
            _inputChannel = string.IsNullOrEmpty(_settings.InputId) ? "Nenhuma" : _settings.InputChannels > 1 ? $"Estéreo {_settings.InputChannel + 1}+{_settings.InputChannel + 2}" : $"Entrada {_settings.InputChannel + 1}";
            _outputPair = $"{_settings.OutputChannel + 1} – {_settings.OutputChannel + 2}";
            // Software monitoring requires a deliberate action in each run.
            _monitoring = false;
            await RefreshAudioDevicesAsync();
            string recordings = Path.Combine(_dataDirectory, "PracticeRecordings");
            if (Directory.Exists(recordings) && Directory.EnumerateFiles(recordings, "*.recording.json", SearchOption.AllDirectories).Any())
                StatusMessage = "Há áudio recuperável nas opções de áudio.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { StatusMessage = ex.Message; }
    }
    private IReadOnlyList<string> UpdateChoices(IReadOnlyList<string> target, IEnumerable<string> values)
    {
        var items = values.ToArray();
        if (target.SequenceEqual(items)) return target;
        bool wasUpdating = _updatingAudioChoices; _updatingAudioChoices = true;
        try
        {
            var collection = (ObservableCollection<string>)target;
            for (int i = collection.Count - 1; i >= 0; i--)
                if (!items.Contains(collection[i])) collection.RemoveAt(i);
            for (int i = 0; i < items.Length; i++)
            {
                int previous = collection.IndexOf(items[i]);
                if (previous < 0) collection.Insert(i, items[i]);
                else if (previous != i) collection.Move(previous, i);
            }
        }
        finally { _updatingAudioChoices = wasUpdating; }
        return target;
    }
    private void PublishAudioChoices()
    {
        _updatingAudioChoices = true;
        try
        {
            foreach (var name in new[] { nameof(Driver), nameof(Devices), nameof(Device), nameof(InputDevices), nameof(AudioInputDevice), nameof(IsAsio), nameof(AudioDeviceFieldLabel), nameof(SampleRates), nameof(SampleRate), nameof(BufferSizes), nameof(BufferSize), nameof(InputChannels), nameof(InputChannel), nameof(OutputPairs), nameof(OutputPair), nameof(Monitoring), nameof(AudioSummary), nameof(AudioDeviceLabel), nameof(CanApplyAudioSettings) }) OnPropertyChanged(name);
        }
        finally { _updatingAudioChoices = false; }
        ApplyAudioSettingsCommand.RaiseCanExecuteChanged();
    }
    private void UpdateDeviceChoices()
    {
        var driver = IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi;
        _outputChoices = UpdateChoices(_outputChoices, _devices.Where(d => d.Driver == driver && !d.IsInput).Select(d => d.Name).Distinct().ToArray());
        _inputChoices = UpdateChoices(_inputChoices, _devices.Where(d => d.Driver == driver && d.IsInput).Select(d => d.Name).Distinct().ToArray());
        if (!Devices.Contains(_device)) _device = _devices.FirstOrDefault(d => d.Id == _settings.OutputId && !d.IsInput && d.Driver == driver)?.Name ?? Devices.FirstOrDefault() ?? "";
        if (!InputDevices.Contains(_inputDevice)) _inputDevice = _devices.FirstOrDefault(d => d.Id == _settings.InputId && d.IsInput && d.Driver == driver)?.Name ?? InputDevices.FirstOrDefault() ?? "";
        PublishAudioChoices();
    }
    private async Task RefreshAudioDevicesAsync()
    {
        if (_refreshingAudio || IsRecording) return;
        _refreshingAudio = true;
        try { _devices = await _audio.EnumerateDevicesAsync(_lifetime.Token); UpdateDeviceChoices(); await RefreshCapabilitiesAsync(); }
        finally { _refreshingAudio = false; }
    }
    private StudioAudioSettings DraftSettings(bool forCapabilities = false)
    {
        var driver = IsAsio ? StudioDriver.Asio : StudioDriver.Wasapi;
        var output = _devices.FirstOrDefault(d => d.Driver == driver && !d.IsInput && d.Name == Device);
        var input = _devices.FirstOrDefault(d => d.Driver == driver && d.IsInput && d.Name == AudioInputDevice);
        var channels = Regex.Matches(InputChannel, @"\d+").Select(m => int.Parse(m.Value) - 1).ToArray();
        int outputChannel = IsAsio && int.TryParse(Regex.Match(OutputPair, @"\d+").Value, out int firstOutput) ? firstOutput - 1 : 0;
        return _settings with { Driver = driver, OutputId = output?.Id ?? "", InputId = !forCapabilities && InputChannel == "Nenhuma" ? null : IsAsio ? output?.Id : input?.Id,
            SampleRate = int.TryParse(SampleRate, out int rate) ? rate : 48000,
            BufferFrames = int.TryParse(BufferSize, out int frames) ? frames : 256, Monitoring = Monitoring,
            InputChannel = channels.FirstOrDefault(), InputChannels = Math.Max(1, channels.Length), OutputChannel = outputChannel };
    }
    private async Task RefreshCapabilitiesAsync()
    {
        if (IsRecording || _disposed) return;
        int request = ++_capabilityRequest;
        _loadingAudioCapabilities = true;
        _rateChoices = UpdateChoices(_rateChoices, []); _bufferChoices = UpdateChoices(_bufferChoices, []); _channelChoices = UpdateChoices(_channelChoices, ["Nenhuma"]); _outputPairs = UpdateChoices(_outputPairs, []);
        PublishAudioChoices();
        try
        {
            if (string.IsNullOrEmpty(Device)) return;
            // Probe the selected input even when recording is disabled, so choosing
            // "Nenhuma" cannot erase the channels needed to enable it again.
            var draft = DraftSettings(forCapabilities: true);
            var capabilities = await _audio.GetCapabilitiesAsync(draft, _lifetime.Token);
            if (_disposed || request != _capabilityRequest) return;
            _capabilities = capabilities;
            _rateChoices = UpdateChoices(_rateChoices, capabilities.SampleRates.Select(v => v.ToString()).ToArray());
            _bufferChoices = UpdateChoices(_bufferChoices, capabilities.BufferFrames.Select(v => v.ToString()).ToArray());
            var channels = new List<string> { "Nenhuma" };
            for (int i = 1; i <= capabilities.InputChannels; i++) channels.Add("Entrada " + i);
            for (int i = 1; i < capabilities.InputChannels; i += 2) channels.Add($"Estéreo {i}+{i + 1}");
            _channelChoices = UpdateChoices(_channelChoices, channels);
            _outputPairs = UpdateChoices(_outputPairs, Enumerable.Range(0, Math.Max(1, (IsAsio ? capabilities.OutputChannels : 2) / 2)).Select(i => $"{i * 2 + 1} – {i * 2 + 2}").ToArray());
            if (!SampleRates.Contains(_sampleRate)) _sampleRate = SampleRates.Contains("48000") ? "48000" : SampleRates.FirstOrDefault() ?? "";
            if (!BufferSizes.Contains(_bufferSize)) _bufferSize = BufferSizes.Contains("256") ? "256" : BufferSizes.FirstOrDefault() ?? "";
            if (!InputChannels.Contains(_inputChannel)) _inputChannel = InputChannels.Skip(1).FirstOrDefault() ?? "Nenhuma";
            if (!OutputPairs.Contains(_outputPair)) _outputPair = OutputPairs.FirstOrDefault() ?? "";
        }
        catch (Exception) when (_disposed || request != _capabilityRequest) { }
        finally
        {
            if (!_disposed && request == _capabilityRequest) { _loadingAudioCapabilities = false; PublishAudioChoices(); }
        }
    }
    private async Task ApplyAudioSettingsAsync()
    {
        if (IsRecording) return;
        if (_loadingAudioCapabilities) throw new InvalidOperationException(T("Aguarde a atualização dos dispositivos de áudio."));
        if (!SampleRates.Contains(SampleRate) || !BufferSizes.Contains(BufferSize))
            throw new InvalidOperationException(T("Escolha uma taxa e um buffer disponíveis para o dispositivo."));
        var draft = DraftSettings();
        await PauseTransportCoreAsync();
        _deviceReady = false;
        if (string.IsNullOrEmpty(draft.OutputId)) throw new InvalidOperationException(T("Escolha a saída de áudio"));
        var caps = await _audio.ConfigureAsync(draft, _lifetime.Token);
        _settings = _audio.Settings; _capabilities = caps; _activeCapabilities = caps; _deviceReady = true; _deviceDraft = false;
        Directory.CreateDirectory(_dataDirectory);
        string path = Path.Combine(_dataDirectory, "studio-audio-v1.json");
        await File.WriteAllTextAsync(path + ".tmp", JsonSerializer.Serialize(_settings with { Monitoring = false }), _lifetime.Token);
        File.Move(path + ".tmp", path, true);
        OnPropertyChanged(nameof(AudioSummary)); OnPropertyChanged(nameof(AudioDeviceLabel)); OnPropertyChanged(nameof(InputChannels));
        StatusMessage = "Dispositivo de áudio configurado";
    }
    private async Task EnsureAudioAsync()
    {
        if (_devices.Count == 0) await RefreshAudioDevicesAsync();
        else if (_loadingAudioCapabilities || SampleRates.Count == 0) await RefreshCapabilitiesAsync();
        if (!_deviceReady || _deviceDraft) await ApplyAudioSettingsAsync();
    }
    private async Task OpenAsioPanelAsync()
    {
        await PauseTransportCoreAsync();
        await ReleaseStudyOutputAsync();
        _deviceReady = false; _deviceDraft = true;
        await _audio.ShowAsioControlPanelAsync(DraftSettings(), _lifetime.Token);
        await RefreshCapabilitiesAsync();
    }
    public void Seek(double seconds)
    {
        if (!CanEditTimeline) return;
        if (!double.IsFinite(seconds)) return;
        double target = Math.Clamp(seconds, 0, Project.Duration);
        if (Session.LoopEnabled && IsPlaying && (target < Session.LoopStart || target >= Session.LoopEnd))
            target = Session.LoopStart;
        Session.Position = target; Project.Position = target;
        // Capture this seek intent; a transport tick must not replace it while queued.
        Run(() => ExecuteTransportAsync(() => _audio.SeekAsync(target, _lifetime.Token)));
    }
    private async Task ExecuteTransportAsync(Func<Task> action)
    {
        await _transportGate.WaitAsync(_lifetime.Token);
        _transportBusy = true; RefreshTransport();
        try { await action(); }
        finally { _transportBusy = false; _transportGate.Release(); if (!_disposed) RefreshTransport(); }
    }
    private int _metronomeRevision;
    private bool _resetMetronomePhase;
    private async void RunMetronomeUpdate(bool resetPhase = false)
    {
        int revision = ++_metronomeRevision;
        _resetMetronomePhase |= resetPhase;
        bool entered = false, starting = false;
        try
        {
            await _transportGate.WaitAsync(_lifetime.Token); entered = true;
            if (revision != _metronomeRevision || _disposed) return;
            starting = MetronomeEnabled && !_audio.State.IsMetronomeRunning;
            if (starting)
            {
                _transportBusy = true; RefreshTransport();
                StopPreview(); if (BeforePlayback != null) await BeforePlayback();
                await Downloader.StopForIrAudition();
                await EnsureAudioAsync();
            }
            bool reset = _resetMetronomePhase; _resetMetronomePhase = false;
            await _audio.ConfigureMetronomeAsync(new StudioMetronomeSettings(MetronomeEnabled, MetronomeBpm, Meter, MetronomeSound, MetronomeVolumeDb), reset, _lifetime.Token);
            _state = _audio.State;
        }
        catch (OperationCanceledException) when (_disposed || _lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _metronomeEnabled = false; OnPropertyChanged(nameof(MetronomeEnabled));
            StatusMessage = ex.Message;
        }
        finally
        {
            if (entered) _transportGate.Release();
            if (starting) _transportBusy = false;
            if (!_disposed && starting) RefreshTransport();
        }
    }
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
    public Task PauseTransportAsync() => ExecuteTransportAsync(async () => { StopPreview(); await PauseTransportCoreAsync(); });
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
        if (Session.LoopEnabled && (Session.Position < Session.LoopStart || Session.Position >= Session.LoopEnd)) Session.Position = Session.LoopStart;
        else if (!forRecording && end > 0 && Session.Position >= end) Session.Position = 0;
    }
    private void Tick(object? sender, EventArgs e)
    {
        if (_disposed) return;
        var previous = _state; _state = _audio.State;
        _projecting = true;
        try
        {
            bool visible = PageVisible && _visualUpdatesEnabled;
            if (visible && _state.IsRecording && _state.Duration > Project.Duration) Project.Duration = Math.Min(86400, _state.Duration);
            if (_state.IsPlaying || previous.IsPlaying)
            {
                double oldPosition = Session.Position;
                Session.Position = _state.Position;
                if (visible) Project.Position = Math.Clamp(_state.Position, 0, Project.Duration);
                if ((int)(oldPosition * 10) != (int)(Session.Position * 10)) OnPropertyChanged(nameof(TimeDisplay));
                if ((int)oldPosition != (int)Session.Position) OnPropertyChanged(nameof(CompactTimeDisplay));
            }
        }
        finally { _projecting = false; }
        if (PageVisible && _visualUpdatesEnabled) UpdateSessionPreview();
        if (PageVisible && _visualUpdatesEnabled && previous.IsMetronomePulse != _state.IsMetronomePulse) OnPropertyChanged(nameof(MetronomePulse));
        if (previous.IsPlaying != _state.IsPlaying || previous.IsRecording != _state.IsRecording || previous.IsMetronomeRunning != _state.IsMetronomeRunning || previous.IsPreviewPlaying != _state.IsPreviewPlaying) RefreshTransport();
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
        if (_hoverResult is { } result && _tapClock.Elapsed.TotalSeconds - _hoverSince >= .45) { _hoverResult = null; StartPreview(result); }
        if (_previewResult == null && _renderDirty && _snapshotWorker.IsCompleted && !_transportBusy && !IsBusy && _tapClock.Elapsed.TotalSeconds >= _snapshotRetryAfter) Run(FlushSnapshotAsync);
        UpdateUiCadence();
    }
}
