using Cube.Desktop.ViewModels;
using Cube.WinUI.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Cube.WinUI;

// One workspace per window: navigating never recreates audio engines or unsaved drafts.
internal sealed class Workspace : IDisposable
{
    public EditorViewModel Editor { get; }
    public BackingTracksViewModel Backing { get; }
    // Presentation references survive module switches and navigation. The domain owns writes.
    private readonly Dictionary<(object Source, string Path), double> instrumentReferences = [];
    internal double Reference(object source, string path)
    {
        var key = (source, path);
        if (!instrumentReferences.TryGetValue(key, out double value))
            instrumentReferences[key] = value = Convert.ToDouble(source.GetType().GetProperty(path)!.GetValue(source));
        return value;
    }
    internal void CaptureReference(object source, string path) => instrumentReferences[(source, path)] = Convert.ToDouble(source.GetType().GetProperty(path)!.GetValue(source));
    public Workspace()
    {
        App.Trace("Workspace ctor");
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "--data-directory");
        string? directory = index >= 0 && index + 1 < args.Length ? System.IO.Path.GetFullPath(args[index + 1]) : null;
        Editor = new(directory);
        Editor.PropertyChanged += EditorReferenceChanged;
        App.Trace("Editor created");
        Backing = new(Editor.DataDirectory);
        App.Trace("Backing created");
        Editor.IrAudition.BeforePlayback = Backing.StopForIrAudition;
        Backing.BeforePlayback = Editor.IrAudition.StopAsync;
        Backing.ChooseExportMode = async () =>
        {
            var result = await Dialogs.Show(new ContentDialog { Title = "Exportar WAV", Content = "Escolha entre a mixagem com velocidade e afinação atuais ou a mixagem sem essas transformações.", PrimaryButtonText = "Com ajustes", SecondaryButtonText = "Sem ajustes", CloseButtonText = "Cancelar" });
            return result == ContentDialogResult.None ? null : result == ContentDialogResult.Primary;
        };
    }
    private void EditorReferenceChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // A read/reload/save establishes a clean bank, including parameters whose view is closed.
        if (e.PropertyName is not ("Dirty" or "HasBank") || !Editor.HasBank || Editor.Dirty) return;
        foreach (var preset in Editor.Presets)
            foreach (var parameter in preset.Parameters) CaptureReference(parameter, "Value");
    }
    public async Task Initialize()
    {
        await Editor.Initialize();
        await Backing.Initialize();
        await Editor.IrAudition.RefreshDevices();
        if (!Environment.GetCommandLineArgs().Contains("--offline")) Editor.StartAutomaticConnection();
    }
    public async Task<bool> PrepareClose()
    {
        App.Trace("PrepareClose packDirty=" + Editor.PackEditor.Dirty);
        if (Editor.Busy || !Backing.CanChangeRecordingInput || Backing.IsExporting)
        {
            await Dialogs.Show(new ContentDialog { Title = "Operação em andamento", Content = "Conclua ou interrompa a gravação, exportação ou operação do pedal antes de fechar.", CloseButtonText = "Voltar" });
            return false;
        }
        if (!await Editor.PackEditor.ConfirmDiscard()) { App.Trace("Pack close cancelled"); return false; }
        App.Trace("Pack close accepted");
        if (Editor.Dirty && !await Editor.Ask("Há ajustes aplicados que não foram salvos. Fechar o editor?")) return false;
        if (Enumerable.Range(1, 4).Any(Editor.AdvancedPreviewHasChanges) && !await Editor.Ask("Encerrar as prévias de efeitos e restaurar os parâmetros anteriores?")) return false;
        try
        {
            await Editor.IrAudition.StopAsync();
            await Backing.StopForIrAudition();
            await Editor.Flush();
            await Editor.EndDelayPreviewAsync(); await Editor.EndReverbPreviewAsync();
            await Editor.EndChorusPreviewAsync(); await Editor.EndPhaserPreviewAsync();
            if (await Editor.RestoreAmpAsync() == false) throw new System.IO.IOException(Editor.AmpPreviewMessage);
            await Editor.EndPreviewAsync();
        }
        catch (Exception ex) { if (!await Editor.Ask(ex.Message + "\nFechar mesmo assim?")) return false; }
        return true;
    }
    public void Dispose() { Editor.PropertyChanged -= EditorReferenceChanged; instrumentReferences.Clear(); Backing.Dispose(); Editor.Dispose(); }
}
