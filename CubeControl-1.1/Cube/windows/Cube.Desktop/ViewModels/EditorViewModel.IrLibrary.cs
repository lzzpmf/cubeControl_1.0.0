using System.IO;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public IrLibraryViewModel IrLibrary { get; }
    public bool CanChooseIr => !Busy;
    private int irSelectionGeneration;
    internal Task PendingLibraryLoad { get; private set; } = Task.CompletedTask;
    private void SelectLibraryIr(IrLibraryEntry? entry)
    {
        int generation = ++irSelectionGeneration;
        PendingLibraryLoad = entry == null ? Task.CompletedTask : LoadLibrarySelectionAsync(entry, generation);
    }
    private async Task LoadLibrarySelectionAsync(IrLibraryEntry entry, int generation)
    {
        await Task.Delay(125);
        if (generation != irSelectionGeneration || Busy) return;
        await Execute(async () =>
        {
            int loadingGeneration = irSelectionGeneration;
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
            if (loadingGeneration != irSelectionGeneration) return;
            await LoadWavAndPreviewAsync(entry.FullPath, audio, entry.Collection);
        });
    }
}
