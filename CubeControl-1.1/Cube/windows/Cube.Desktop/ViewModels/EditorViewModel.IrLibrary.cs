using System.IO;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public IrLibraryViewModel IrLibrary { get; }
    public bool CanChooseIr => !Busy;
    private int irWorkspaceTab;
    public int IrWorkspaceTab
    {
        get => irWorkspaceTab;
        set
        {
            if (value is < 0 or > 1 || !Set(ref irWorkspaceTab, value)) return;
            irSelectionGeneration++;
            IrLibrary.SetPackPicker(value == 1);
            if (value == 1) PackEditor.SelectedIr = IrLibrary.Selected;
        }
    }
    private int irSelectionGeneration;
    internal Task PendingLibraryLoad { get; private set; } = Task.CompletedTask;
    private void SelectLibraryIr(IrLibraryEntry? entry)
    {
        int generation = ++irSelectionGeneration;
        PendingLibraryLoad = entry == null || IrWorkspaceTab == 1 ? Task.CompletedTask : LoadLibrarySelectionAsync(entry, generation);
    }
    private async Task LoadLibrarySelectionAsync(IrLibraryEntry entry, int generation)
    {
        await Task.Delay(125);
        if (generation != irSelectionGeneration || Busy || IrWorkspaceTab == 1) return;
        int loadingGeneration = generation + 1; // Execute invalidates earlier selections before awaiting queued work.
        await Execute(async () =>
        {
            if (loadingGeneration != irSelectionGeneration || IrWorkspaceTab == 1) return;
            IrAudio audio;
            try
            {
                if (new FileInfo(entry.FullPath).Length > 32 * 1024 * 1024) throw new FormatException("WAV excede 32 MB.");
                audio = IrAudio.Decode(await File.ReadAllBytesAsync(entry.FullPath));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
            {
                // Local file failures must not fault a healthy USB session or replace the active audio.
                Status = ErrorMessage = $"Não foi possível carregar o IR: {entry.Name}. {ex.Message}";
                return;
            }
            if (loadingGeneration != irSelectionGeneration || IrWorkspaceTab == 1) return;
            await LoadWavAndPreviewAsync(entry.FullPath, audio, entry.Collection,
                () => loadingGeneration == irSelectionGeneration && IrWorkspaceTab == 0);
        });
    }
}
