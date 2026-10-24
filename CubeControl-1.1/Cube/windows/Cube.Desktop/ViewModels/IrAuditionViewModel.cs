using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Services;
using Cube.Protocol;
using NAudio.CoreAudioApi;

namespace Cube.Desktop.ViewModels;

public sealed record RawGuitar(string Name, string Path);

public sealed class IrAuditionViewModel : Observable, IDisposable
{
    private readonly EditorViewModel owner;
    private readonly IIrAuditionService audio;
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private Task pending = Task.CompletedTask;
    internal Task PendingOperation => pending;
    private bool disposed, suspended, looping = true;
    private int generation;
    private RawGuitar? selected;
    private BackingOutput? output;
    private double volume = 30, duration;
    private string status = "Selecione a saída USB de áudio do pedal.";
    public ObservableCollection<RawGuitar> Tracks { get; } = [];
    public ObservableCollection<BackingOutput> Outputs { get; } = [];
    public Func<Task>? BeforePlayback { get; set; }
    public Func<bool>? PlaybackAllowed { get; set; }
    public ICommand PlayPause { get; }
    public ICommand Stop { get; }
    public ICommand RefreshOutputs { get; }
    public bool IsPlaying => audio.Snapshot.State == BackingPlaybackState.Playing;
    public string PlayLabel => IsPlaying ? "Pausar" : "Reproduzir";
    public string Status { get => status; private set => Set(ref status, value); }
    public string TimeLabel => $"{TimeSpan.FromSeconds(audio.Snapshot.Position):mm\\:ss} / {TimeSpan.FromSeconds(duration):mm\\:ss}";
    public bool CanStart => !disposed && !suspended && (PlaybackAllowed?.Invoke() ?? true) && owner.CanOperate && (!owner.Ir.PreviewActive || owner.Ir.PreviewConfirmed) && selected != null && output != null;
    private bool opening, hasError, panelVisible = true;
    public bool PanelVisible { get => panelVisible; set { if (Set(ref panelVisible, value)) Notify(nameof(ShowMiniPlayer)); } }
    public bool ShowMiniPlayer => !PanelVisible && (opening || audio.Snapshot.State is BackingPlaybackState.Playing or BackingPlaybackState.Paused);
    private readonly BackingMeterEnvelope outputEnvelope = new();
    private BackingMeterReading outputMeter = BackingMeterReading.Silent;
    public BackingMeterReading OutputMeter { get => outputMeter; private set => Set(ref outputMeter, value); }
    private double outputLevel;
    public double OutputLevel { get => outputLevel; private set => Set(ref outputLevel, value); }
    public double Progress => duration > 0 ? Math.Clamp(audio.Snapshot.Position / duration * 100, 0, 100) : 0;
    public string ConnectionLabel => owner.Connected ? "Pedal conectado" : "Pedal desconectado";
    public string OutputStateLabel => hasError || audio.Snapshot.State == BackingPlaybackState.Error ? "Erro de áudio"
        : opening ? "Abrindo saída…" : output == null ? "Sem saída selecionada" : "Saída selecionada";
    public string ContextStatus => hasError || audio.Snapshot.State == BackingPlaybackState.Error ? Status
        : !owner.Connected ? "Conecte o pedal para iniciar a pré-escuta."
        : output == null ? "Selecione a saída USB de áudio do pedal."
        : suspended ? "Pré-escuta pausada durante a transferência do IR."
        : !owner.CanOperate ? "Aguarde o pedal ficar disponível."
        : owner.Ir.PreviewActive && !owner.Ir.PreviewConfirmed ? "Confirme ou restaure a prévia do IR interrompida para iniciar."
        : selected == null ? "Selecione uma gravação."
        : opening ? "Abrindo a saída de áudio…"
        : IsPlaying ? "Pré-escuta pelo pedal · efeitos atuais mantidos"
        : audio.Snapshot.State == BackingPlaybackState.Paused ? "Pré-escuta pausada."
        : "Pronto para iniciar a pré-escuta.";
    public RawGuitar? SelectedTrack
    {
        get => selected;
        set
        {
            if (!Set(ref selected, value)) return;
            hasError = false; bool resume = IsPlaying; int version = ++generation;
            _ = Queue(async () => { await audio.Clear(); duration = 0; if (resume && version == generation && CanStart) await StartCore(version); });
        }
    }
    public BackingOutput? SelectedOutput
    {
        get => output;
        set { if (Set(ref output, value)) { hasError = false; _ = StopAsync(); } }
    }
    public double Volume
    {
        get => volume;
        set { if (double.IsFinite(value) && Set(ref volume, Math.Clamp(value, 0, 100))) _ = Queue(() => audio.Volume(volume / 100, suspended)); }
    }
    public bool Loop
    {
        get => looping;
        set { if (Set(ref looping, value)) _ = Queue(() => duration > 0 ? audio.Configure(looping, duration) : Task.CompletedTask); }
    }
    public IrAuditionViewModel(EditorViewModel owner, IIrAuditionService? service = null)
    {
        this.owner = owner; audio = service ?? new IrAuditionService();
        for (int i = 1; i <= 5; i++) Tracks.Add(new($"Guitarra {i}", Path.Combine(AppContext.BaseDirectory, "Assets", "RawWaves", $"raw_guitar_{i}.wav")));
        selected = Tracks[0];
        PlayPause = new UiCommand(() => _ = ToggleAsync(), () => !opening && (IsPlaying || CanStart));
        Stop = new UiCommand(() => _ = StopAsync(), () => opening || audio.Snapshot.State is BackingPlaybackState.Playing or BackingPlaybackState.Paused);
        RefreshOutputs = new UiCommand(() => _ = RefreshDevices());
        owner.PropertyChanged += OwnerChanged;
        owner.Ir.PropertyChanged += IrChanged;
        timer.Tick += (_, _) => Refresh(); timer.Start();
    }
    private void OwnerChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(owner.Connected) && !owner.Connected) _ = StopAsync();
        Refresh();
    }
    private void IrChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => Refresh();
    private void Refresh()
    {
        Notify(nameof(ShowMiniPlayer)); Notify(nameof(IsPlaying)); Notify(nameof(PlayLabel)); Notify(nameof(TimeLabel)); Notify(nameof(CanStart));
        if (audio.Snapshot.State == BackingPlaybackState.Error) Status = "Saída de áudio indisponível. Selecione a saída e tente novamente.";
        float peak = audio.ConsumePeak();
        OutputMeter = outputEnvelope.Update(peak, IsPlaying, System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency);
        OutputLevel = IsPlaying ? Math.Clamp(peak, 0, 1) * 100 : 0;
        Notify(nameof(Progress)); Notify(nameof(ConnectionLabel)); Notify(nameof(OutputStateLabel)); Notify(nameof(ContextStatus));
        CommandManager.InvalidateRequerySuggested();
    }
    private Task Queue(Func<Task> operation)
    {
        return pending = Run(pending, operation);
    }
    private async Task Run(Task previous, Func<Task> operation)
    {
        await previous;
        if (disposed) return;
        try { await operation(); }
        catch (Exception) { hasError = true; ++generation; try { await audio.Clear(); } catch { } Status = "Não foi possível reproduzir a pré-escuta. Confira o WAV e a saída de áudio."; }
        Refresh();
    }
    public Task ToggleAsync() => Queue(async () =>
    {
        if (IsPlaying) { await audio.Pause(); return; }
        if (!CanStart) return;
        hasError = false; await StartCore(generation);
    });
    private async Task StartCore(int version)
    {
        opening = true; Refresh();
        try
        {
            var track = selected!; var endpoint = output!;
            if (BeforePlayback != null) await BeforePlayback();
            if (version != generation || !CanStart) return;
            if (audio.Snapshot.Source != track.Path || audio.Snapshot.OutputId != endpoint.Id || audio.Snapshot.State == BackingPlaybackState.Error)
            {
                duration = IrAudio.Decode(await File.ReadAllBytesAsync(track.Path)).Duration;
                await audio.Open(track.Path, endpoint.Id, duration, looping, volume / 100);
            }
            if (version != generation || !CanStart) { await audio.Clear(); return; }
            await audio.Play(); Status = "Pré-escuta pelo pedal · efeitos atuais mantidos";
        }
        finally { opening = false; Refresh(); }
    }

    public Task StopAsync()
    {
        ++generation;
        return Queue(async () => { await audio.Clear(); duration = 0; Status = "Selecione a saída USB de áudio do pedal."; });
    }
    public async Task<int?> SuspendForTransfer()
    {
        suspended = true; int version = generation; bool resume = false;
        await Queue(async () => { resume = IsPlaying; await audio.Volume(volume / 100, true); await audio.Pause(); });
        return resume ? version : null;
    }
    public Task CompleteTransfer(int? version, bool success) => Queue(async () =>
    {
        suspended = false;
        await audio.Volume(volume / 100, false);
        if (success && version == generation && owner.Connected && (!owner.Ir.PreviewActive || owner.Ir.PreviewConfirmed))
            await audio.Play();
        else if (!success) { hasError = true; Status = "Pré-escuta pausada: a transferência do IR não foi confirmada."; }
    });
    public Task RefreshDevices() => Queue(async () =>
    {
        var devices = await Task.Run(() =>
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(device =>
            { using (device) return new BackingOutput(device.ID, device.FriendlyName); }).ToArray();
        });
        string? id = output?.Id;
        Outputs.Clear(); foreach (var device in devices) Outputs.Add(device);
        var replacement = Outputs.FirstOrDefault(d => d.Id == id);
        if (replacement == null && output != null) { output = null; Notify(nameof(SelectedOutput)); ++generation; await audio.Clear(); }
        else { output = replacement; Notify(nameof(SelectedOutput)); }
    });
    public void Dispose()
    {
        disposed = true; ++generation; timer.Stop(); owner.PropertyChanged -= OwnerChanged; owner.Ir.PropertyChanged -= IrChanged; audio.Dispose();
    }
}
