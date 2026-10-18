using System.IO;
using System.Windows.Input;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private bool randomizingIr;
    private string randomIrResult = "";
    public ICommand RandomizeCabinetIr { get; }
    private int CurrentCabinetSlot => (int)Current.Parameters[9].Value - 1;
    public bool CanRandomizeCabinetIr => CanOperate && HasBank && CurrentCabinetSlot is >= 0 and < 8
        && IrLibrary.Entries.Count > 0 && !confirmingIrWrite && !randomizingIr;
    public string RandomIrDestination => HasBank && CurrentCabinetSlot is >= 0 and < 8
        ? $"Slot de destino: {CurrentCabinetSlot + 1}" : "Selecione um slot de gabinete.";
    public string RandomIrResult { get => randomIrResult; private set => Set(ref randomIrResult, value); }

    private async Task RandomizeCabinetIrAsync()
    {
        if (!CanRandomizeCabinetIr) return;
        int slot = CurrentCabinetSlot;
        var entry = IrLibrary.Entries[Random.Shared.Next(IrLibrary.Entries.Count)];
        int distance = Random.Shared.Next(66, 86);
        randomizingIr = true;
        RandomIrResult = "";
        try
        {
            await Execute(async () =>
            {
                IrAudio audio;
                try
                {
                    if (new FileInfo(entry.FullPath).Length > 32 * 1024 * 1024)
                        throw new FormatException("WAV excede 32 MB.");
                    audio = IrAudio.Decode(await File.ReadAllBytesAsync(entry.FullPath));
                    // Validate conversion before any device mutation.
                    _ = audio.Sector(distance);
                    _ = audio.LivePayload(distance);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or ArgumentException)
                {
                    RandomIrResult = Status = ErrorMessage = $"Não foi possível carregar o IR: {entry.Name}. {ex.Message}";
                    return;
                }

                await WriteIrAsync(slot, audio, distance, entry.Name, null, announceCompletion: false);
                try
                {
                    BusyDetail = "Aplicando IR gravado…";
                    // Enable the RAM payload, without changing the preset's IR on/off parameter.
                    await session!.ApplyIrRam(new(1, audio.LivePayload(distance)), Progress());
                }
                catch (Exception ex)
                {
                    RandomIrResult = $"IR gravado no slot {slot + 1}, mas a aplicação imediata não foi confirmada.";
                    throw new IOException(RandomIrResult + " " + ex.Message, ex);
                }
                RandomIrResult = Status = Ir.State = $"IR gravado e aplicado: {entry.Name} · {distance}% · slot {slot + 1}";
                Completed("IR gravado e aplicado.");
            });
        }
        finally
        {
            randomizingIr = false;
            Refresh();
        }
    }
}
