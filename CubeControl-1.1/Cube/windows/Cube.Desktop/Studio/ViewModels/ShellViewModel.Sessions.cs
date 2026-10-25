using System.IO;
using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.Localization;
using Cube.Studio.Audio;
using Cube.Studio.Core;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Cube.Desktop.Studio.ViewModels;

public sealed partial class ShellViewModel
{
    private readonly HashSet<string> _ownedRecordings = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _exportedPaths = new(StringComparer.OrdinalIgnoreCase);
    private Task<bool>? _discardQuestion;
    public bool ExportWithBacking { get; set; } = true;
    private async Task ClearStudyAsync()
    {
        if (!await ConfirmDiscardAsync()) return;
        if (!CanEditTimeline) return;
        await ExecuteTransportAsync(async () =>
        {
            if (IsRecording || IsBusy || _closePending) return;
            BeginBackingIntent(); ++_sessionGeneration;
            await StopTransportCoreAsync(); Editor.Clear(); _media.Clear();
            SyncPresentation(); await FlushSnapshotAsync();
            if (!_preserveRecordings) CleanupOwnedRecordings();
            _discardApproved = false; StatusMessage = "Estudo limpo";
        });
    }
    private Task<bool> ConfirmDiscardAsync()
    {
        if (!Editor.HasUnexportedAttempts) return Task.FromResult(true);
        return _discardQuestion ??= AskDiscardAsync();
    }
    private async Task<bool> AskDiscardAsync()
    {
        try
        {
            return await ShowDialogAsync("Descartar tentativas?", "Há tentativas que ainda não foram salvas em áudio. Descartá-las?", "Descartar", null, "Cancelar") == ContentDialogResult.Primary;
        }
        finally { _discardQuestion = null; }
    }
    public async Task<bool> CanCloseAsync()
    {
        _discardApproved = false;
        if (IsBusy || _transportBusy) { StatusMessage = "Conclua ou cancele a operação em andamento."; return false; }
        BeginBackingIntent();
        try { await StopAsync(); }
        catch (Exception ex) { _preserveRecordings = true; StatusMessage = ex.Message; await ShowMessageAsync("Não foi possível finalizar o áudio", ex.Message); return false; }
        _discardApproved = await ConfirmDiscardAsync();
        // Actual cleanup happens on Dispose, after the host's pedal confirmation.
        return _discardApproved;
    }
    private void CleanupOwnedRecordings()
    {
        string run = Path.GetFullPath(_runDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string parent = Path.GetFullPath(Path.Combine(_dataDirectory, "PracticeRecordings")).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!run.StartsWith(parent, StringComparison.OrdinalIgnoreCase) || run.Equals(parent, StringComparison.OrdinalIgnoreCase)) return;
        foreach (var recording in _ownedRecordings.ToArray())
        {
            string path = Path.GetFullPath(recording);
            if (!path.StartsWith(run, StringComparison.OrdinalIgnoreCase) || _exportedPaths.Contains(path)) continue;
            try
            {
                File.Delete(path); File.Delete(path + ".recording.json"); _ownedRecordings.Remove(recording);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(ex); }
        }
        try { if (Directory.Exists(_runDirectory) && !Directory.EnumerateFileSystemEntries(_runDirectory).Any()) Directory.Delete(_runDirectory); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(ex); }
    }
    private async Task RecoverRecordingsAsync()
    {
        string directory = Path.Combine(_dataDirectory, "PracticeRecordings");
        if (!Directory.Exists(directory)) { StatusMessage = "Nenhuma gravação interrompida para recuperar."; return; }
        var dialog = new OpenFileDialog { Title = T("Recuperar gravações"), InitialDirectory = directory, Multiselect = true,
            Filter = T("Registro de gravação") + "|*.recording.json" };
        if (dialog.ShowDialog(Owner) != true) return;
        _operationBusy = true; RefreshTransport();
        try
        {
            await PauseTransportAsync();
            var recovered = await Task.Run(() => dialog.FileNames.Select(StudioRecordingRecovery.Recover).ToArray(), _lifetime.Token);
            foreach (var segment in recovered)
            {
                // Recovery output is external to this run. Never erase the original journal or recovered audio.
                using var wav = new NAudio.Wave.WaveFileReader(segment.Path);
                Editor.AddAttempt(new CaptureResult(segment.Path, wav.WaveFormat.SampleRate, wav.WaveFormat.Channels,
                    wav.Length / wav.WaveFormat.BlockAlign, segment.RecordedSpeed, (int)segment.RecordedPitch, segment.Start, ClockRatio: segment.ClockRatio), T("Tentativa recuperada"));
                _media[segment.Path] = await _audio.AnalyzeAsync(segment.Path, _lifetime.Token);
            }
            _discardApproved = false; SyncPresentation(); StatusMessage = "Áudio recuperado. Use Salvar áudio para guardá-lo.";
        }
        finally { _operationBusy = false; RefreshTransport(); }
    }
    private async Task ExportAsync()
    {
        if (!CanEditTimeline || SelectedAttempt is not { } attempt) return;
        var panel = new StackPanel { MinWidth = 300 };
        var content = new System.Windows.Controls.ComboBox { ItemsSource = new[] { T("Guitarra com backing"), T("Somente guitarra") }, SelectedIndex = ExportWithBacking ? 0 : 1, Margin = new Thickness(0, 0, 0, 12) };
        var format = new System.Windows.Controls.ComboBox { ItemsSource = new[] { "WAV · PCM 24-bit", "MP3 · 320 kbps" }, SelectedIndex = 0 };
        panel.Children.Add(content); panel.Children.Add(format);
        long generation = _sessionGeneration;
        if (await ShowDialogAsync("Salvar áudio", panel, "Escolher arquivo", null, "Cancelar") != ContentDialogResult.Primary) return;
        if (generation != _sessionGeneration || !Session.Attempts.Contains(attempt) || IsRecording) return;
        ExportWithBacking = content.SelectedIndex == 0;
        bool mp3 = format.SelectedIndex == 1;
        var dialog = new SaveFileDialog { Title = T("Salvar áudio"), Filter = mp3 ? "MP3|*.mp3" : "WAV|*.wav", DefaultExt = mp3 ? ".mp3" : ".wav", AddExtension = true, FileName = SafeFileName(attempt.Name) };
        if (dialog.ShowDialog(Owner) != true) return;
        await ExportAttemptAsync(attempt, dialog.FileName, ExportWithBacking, mp3 ? StudioExportFormat.Mp3 : StudioExportFormat.Wav24);
    }
    internal async Task ExportAttemptAsync(PracticeAttempt attempt, string destination, bool withBacking, StudioExportFormat format)
    {
        if (!CanEditTimeline || !Session.Attempts.Contains(attempt)) return;
        string path = Path.GetFullPath(destination);
        if (_ownedRecordings.Contains(path) || Session.Backing?.SourcePath is { } backingPath && string.Equals(Path.GetFullPath(backingPath), path, StringComparison.OrdinalIgnoreCase)
            || Session.Attempts.Any(a => string.Equals(Path.GetFullPath(a.SourcePath!), path, StringComparison.OrdinalIgnoreCase)))
            throw new IOException(T("Escolha outro nome para preservar o áudio original."));
        Editor.SelectAttempt(attempt.Id); SyncPresentation();
        long revision = attempt.ContentRevision, generation = _sessionGeneration;
        var snapshot = BuildSnapshot(withBacking) with { LoopEnabled = false, MetronomeEnabled = false };
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _operation = operation; _operationBusy = true; OperationProgress = 0; RefreshTransport();
        try
        {
            StatusMessage = "Salvando áudio…";
            await _audio.ExportAsync(snapshot, path, new StudioExportOptions { Format = format, SampleRate = _settings.SampleRate,
                Start = attempt.AudibleStart, End = attempt.AudibleStart + OriginalAttemptClip(attempt).Duration, IncludeMetronome = false },
                new Progress<double>(value => OperationProgress = Math.Clamp(value * 100, 0, 100)), operation.Token);
            _exportedPaths.Add(path);
            if (generation == _sessionGeneration) Editor.MarkAttemptExported(attempt.Id, revision);
            StatusMessage = T("Áudio salvo: ") + path;
        }
        finally { if (_operation == operation) _operation = null; _operationBusy = false; OperationProgress = 0; RefreshTransport(); }
    }
    private async Task<ContentDialogResult> ShowDialogAsync(string title, object content, string primary, string? secondary, string close)
    {
        if (DialogHost == null) throw new InvalidOperationException(T("O diálogo do Studio não está disponível."));
        var dialog = new ContentDialog(DialogHost) { Title = T(title), Content = content is string text ? new TextBlock { Text = T(text), TextWrapping = TextWrapping.Wrap, MaxWidth = 480 } : content,
            PrimaryButtonText = T(primary), SecondaryButtonText = secondary == null ? "" : T(secondary), CloseButtonText = T(close) };
        return await dialog.ShowLocalizedAsync();
    }
    private async Task ShowMessageAsync(string title, string message) => await ShowDialogAsync(title, message, "OK", null, "Fechar");
    private static string SafeFileName(string text) { var invalid = Path.GetInvalidFileNameChars(); string result = new(text.Select(c => invalid.Contains(c) ? '_' : c).ToArray()); return string.IsNullOrWhiteSpace(result) ? "guitarra" : result.Trim(); }
}
