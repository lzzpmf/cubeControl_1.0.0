using System.IO;
using System.Security.Cryptography;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    internal IrPackSlotViewModel? PackPreviewSlot { get; private set; }
    internal bool PackPreviewSending { get; private set; }
    public bool HasPackPreview => PackPreviewSlot != null;
    private int packPreviewGeneration;
    private int? pendingPackDistance;
    private Task? packPreviewPump;

    internal void MovePackPreview(IrPackSlotViewModel from, IrPackSlotViewModel to)
    {
        if (PackPreviewSlot != from) return;
        PackPreviewSlot = to;
        from.Refresh(); to.Refresh();
    }

    private void RefreshPackPreview()
    {
        if (packEditor != null) foreach (var slot in packEditor.Slots) slot.Refresh();
        Notify(nameof(PackPreviewSending));
        Notify(nameof(HasPackPreview));
    }

    internal async Task TogglePackPreviewAsync(IrPackSlotViewModel slot)
    {
        if (!CanOperate || slot.Entry == null || slot.Problem.Length != 0) return;
        ++packPreviewGeneration; pendingPackDistance = null;
        await Execute(async () =>
        {
            if (PackPreviewSlot == slot) { await EndPreviewAsync(); return; }
            var entry = slot.Entry;
            string path = PackStore.ResolvePath(entry.RelativePath);
            var audio = await Task.Run(() =>
            {
                if (new FileInfo(path).Length > 32 * 1024 * 1024) throw new FormatException("WAV excede 32 MB.");
                byte[] bytes = File.ReadAllBytes(path);
                if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new FormatException("IR modificado na biblioteca.");
                return IrAudio.Decode(bytes);
            });
            Ir.Load(path, audio);
            Ir.Distance = entry.Distance;
            PackPreviewSlot = slot;
            RefreshPackPreview();
            await PreviewCoreAsync(null);
            RefreshPackPreview();
        }, liveEdit: true, packPreviewEdit: true);
        RefreshPackPreview();
    }

    internal async Task<bool> StopPackPreviewAsync()
    {
        if (PackPreviewSlot == null) return true;
        if (Busy) return false;
        bool restored = false;
        ++packPreviewGeneration; pendingPackDistance = null;
        await Execute(async () => { await EndPreviewAsync(); restored = true; }, liveEdit: true, packPreviewEdit: true);
        return restored;
    }

    internal void QueuePackDistance(IrPackSlotViewModel slot)
    {
        if (PackPreviewSlot != slot || !Connected || (!Ir.PreviewConfirmed && !PackPreviewSending)) return;
        pendingPackDistance = slot.Entry!.Distance;
        if (packPreviewPump == null || packPreviewPump.IsCompleted)
            packPreviewPump = PumpPackDistanceAsync(packPreviewGeneration);
    }

    private async Task PumpPackDistanceAsync(int generation)
    {
        try
        {
        while (pendingPackDistance != null && generation == packPreviewGeneration)
        {
            await Task.Delay(150);
            if (generation != packPreviewGeneration || PackPreviewSlot == null || !Connected) return;
            if (Busy) continue;
            int distance = pendingPackDistance!.Value;
            pendingPackDistance = null;
            PackPreviewSending = true; RefreshPackPreview();
            bool applied = false;
            await Execute(async () =>
            {
                if (generation != packPreviewGeneration || Ir.Audio == null) return;
                Ir.Distance = distance;
                Ir.PreviewConfirmed = false;
                await session!.ApplyIrRam(new(1, Ir.Audio.LivePayload(distance)));
                Ir.PreviewConfirmed = true;
                applied = true;
            }, liveEdit: true, packPreviewEdit: true);
            PackPreviewSending = false; RefreshPackPreview();
            if (!applied) { pendingPackDistance = null; return; }
        }
        }
        finally
        {
            packPreviewPump = null;
            if (pendingPackDistance != null && PackPreviewSlot != null && Connected && Ir.PreviewConfirmed)
                packPreviewPump = PumpPackDistanceAsync(packPreviewGeneration);
        }
    }
}
