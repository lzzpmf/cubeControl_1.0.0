using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public enum BackingRecordingMode { Guitar, BackingAndGuitar, Input, Output }
public sealed record BackingRecordingSource(BackingRecordingMode Mode, string Label, string? Endpoint = null);

public sealed partial class BackingTracksViewModel
{
    private BackingRecording? recording;
    private string? recordingDestination, lastRecording;
    private bool recordingFinalizing;
    private bool recordingStartedMonitor;
    private double? previousMonitorVolume;
    private BackingRecordingMode? activeRecordingMode;
    private BackingRecordingSource? selectedRecordingSource;
    public ObservableCollection<BackingRecordingSource> RecordingSources { get; } = [];
    public BackingRecordingSource? SelectedRecordingSource
    {
        get => selectedRecordingSource;
        set
        {
            if (!CanChangeRecordingInput || !Set(ref selectedRecordingSource, value)) return;
            Notify(nameof(RecordingHelp)); Changed();
        }
    }
    public string RecordingHelp => SelectedRecordingSource?.Mode switch
    {
        BackingRecordingMode.BackingAndGuitar => "Grava a saída selecionada com a backing track e a guitarra. O monitor é ativado automaticamente (50% se estiver em 0%). Outros sons nessa saída também entram no WAV. Use fones.",
        BackingRecordingMode.Output => "Grava tudo que toca no dispositivo de saída escolhido, incluindo sons de outros aplicativos.",
        _ => "Grava somente o dispositivo de entrada escolhido, sem incluir a backing track."
    };
    public bool IsRecordingMix => IsRecording && activeRecordingMode == BackingRecordingMode.BackingAndGuitar;
    public bool CanChangeRecordingOutput => !recordingFinalizing && (!IsRecording || activeRecordingMode is BackingRecordingMode.Guitar or BackingRecordingMode.Input);
    private bool HasRecordingSource => SelectedRecordingSource?.Mode switch
    {
        BackingRecordingMode.Guitar => SelectedInput != null,
        BackingRecordingMode.BackingAndGuitar => SelectedInput != null && SelectedOutput != null && SelectedTrack != null,
        BackingRecordingMode.Input => Inputs.Any(x => x.Id == SelectedRecordingSource.Endpoint),
        BackingRecordingMode.Output => Outputs.Any(x => x.Id == SelectedRecordingSource.Endpoint),
        _ => false
    };
    private readonly DispatcherTimer recordingTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    public ICommand ToggleRecording { get; private set; } = null!;
    public ICommand OpenRecording { get; private set; } = null!;
    public ICommand AddRecording { get; private set; } = null!;
    public bool IsRecording => recording != null;
    public bool CanChangeRecordingInput => !IsRecording && !recordingFinalizing;
    public string RecordingLabel => IsRecording ? "Parar e salvar" : "Iniciar gravação";
    public string RecordingTime => FormatTime(recording?.Seconds ?? 0, true);
    public string? LastRecording => lastRecording;

    private void InitializeRecording()
    {
        ToggleRecording = new UiCommand(() => Start(ToggleRecordingSession()), () => !recordingFinalizing && (IsRecording || HasRecordingSource));
        OpenRecording = new UiCommand(() => Run(() => Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + lastRecording + "\"") { UseShellExecute = true })), () => File.Exists(lastRecording));
        AddRecording = new UiCommand(() => Run(() => { if (File.Exists(lastRecording)) AddPaths([lastRecording!]); }), () => File.Exists(lastRecording));
        recordingTimer.Tick += (_, _) => Notify(nameof(RecordingTime));
        PropertyChanged += (_, e) => { if (e.PropertyName is nameof(SelectedInput) or nameof(SelectedOutput) or nameof(SelectedTrack)) Changed(); };
        Inputs.CollectionChanged += (_, _) => RefreshRecordingSources();
        Outputs.CollectionChanged += (_, _) => RefreshRecordingSources();
        RefreshRecordingSources();
    }

    private void RefreshRecordingSources()
    {
        if (IsRecording || recordingFinalizing) return;
        var previous = SelectedRecordingSource;
        RecordingSources.Clear();
        RecordingSources.Add(new(BackingRecordingMode.Guitar, "Somente guitarra"));
        RecordingSources.Add(new(BackingRecordingMode.BackingAndGuitar, "Backing track + guitarra"));
        foreach (var input in Inputs) RecordingSources.Add(new(BackingRecordingMode.Input, "Entrada: " + input.Name, input.Id));
        foreach (var output in Outputs) RecordingSources.Add(new(BackingRecordingMode.Output, "Saída: " + output.Name, output.Id));
        SelectedRecordingSource = RecordingSources.FirstOrDefault(x => x.Mode == previous?.Mode && x.Endpoint == previous.Endpoint) ?? RecordingSources[0];
        Changed();
    }

    private async Task ToggleRecordingSession()
    {
        if (recording is { } active) { recordingFinalizing = true; Changed(); await active.Stop(); return; }
        if (!HasRecordingSource || recordingFinalizing || SelectedRecordingSource == null) return;
        var dialog = new SaveFileDialog { Filter = "WAV|*.wav", DefaultExt = ".wav", AddExtension = true, FileName = "Recording-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".wav", Title = LanguageService.T("Salvar gravação") };
        if (dialog.ShowDialog() != true) return;
        // A recording may never overwrite a source currently in the playlist.
        if (Tracks.Any(x => string.Equals(Path.GetFullPath(x.Source), Path.GetFullPath(dialog.FileName), StringComparison.OrdinalIgnoreCase)))
        { Status = "Escolha outro arquivo para preservar a backing track."; return; }
        try
        {
            var source = SelectedRecordingSource;
            if (!HasRecordingSource) return;
            recordingFinalizing = true; RefreshRecording();
            bool loopback = source.Mode is BackingRecordingMode.BackingAndGuitar or BackingRecordingMode.Output;
            string endpoint = source.Mode switch
            {
                BackingRecordingMode.Guitar => SelectedInput!.Id,
                BackingRecordingMode.BackingAndGuitar => SelectedOutput!.Id,
                _ => source.Endpoint!
            };
            if (source.Mode == BackingRecordingMode.BackingAndGuitar)
            {
                if (BeforePlayback != null) await BeforePlayback();
                if (disposed) { recordingFinalizing = false; return; }
                if (MonitorVolume <= 0) { previousMonitorVolume = MonitorVolume; MonitorVolume = 50; }
                recordingStartedMonitor = !IsMonitoring;
                if (recordingStartedMonitor) StartMonitor();
                if (!IsMonitoring) throw new InvalidOperationException("Guitar monitor unavailable.");
            }
            recordingDestination = dialog.FileName;
            recording = new BackingRecording(endpoint, dialog.FileName, loopback);
            recordingFinalizing = false;
            activeRecordingMode = source.Mode;
            HasError = false; Status = "Gravação em andamento.";
            recordingTimer.Start(); RefreshRecording();
            _ = ObserveRecording(recording);
        }
        catch (Exception ex)
        {
            recordingFinalizing = false; RestoreRecordingMonitor(); RefreshRecording();
            Error("Não foi possível iniciar a gravação. Verifique a entrada e o destino.", ex);
        }
    }

    private async Task ObserveRecording(BackingRecording session)
    {
        Exception? error = await session.Completion;
        Dispatch(() =>
        {
            if (recording != session || disposed) return;
            CompleteRecording(session, error);
        });
    }

    private void CompleteRecording(BackingRecording session, Exception? error)
    {
        recordingTimer.Stop();
        session.Dispose(); recording = null; recordingFinalizing = false;
        activeRecordingMode = null; RestoreRecordingMonitor();
        try
        {
            if (error != null) throw error;
            File.Move(session.TemporaryPath, recordingDestination!, true);
            lastRecording = recordingDestination; Notify(nameof(LastRecording));
            Status = "Gravação salva em WAV."; HasError = false;
        }
        catch (Exception ex)
        {
            lastRecording = session.TemporaryPath; Notify(nameof(LastRecording));
            Error("Gravação interrompida ou não salva no destino. O WAV recuperável está em: " + session.TemporaryPath, ex);
        }
        RefreshRecording();
        RefreshRecordingSources();
    }

    private void RestoreRecordingMonitor()
    {
        bool stop = recordingStartedMonitor; recordingStartedMonitor = false;
        if (stop) StopMonitor();
        if (previousMonitorVolume is double volume) { MonitorVolume = volume; previousMonitorVolume = null; }
    }

    private void RefreshRecording()
    {
        Notify(nameof(IsRecording)); Notify(nameof(IsRecordingMix)); Notify(nameof(CanChangeRecordingInput)); Notify(nameof(CanChangeRecordingOutput)); Notify(nameof(RecordingLabel)); Notify(nameof(RecordingTime)); Changed();
    }

    private void DisposeRecording()
    {
        if (recording is not { } session) return;
        // Dispose joins the capture worker, then closes the WAV header before app shutdown.
        session.Dispose(); CompleteRecording(session, session.Completion.GetAwaiter().GetResult());
    }
}
