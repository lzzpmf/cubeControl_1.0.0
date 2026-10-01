using System.IO;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    public AmpViewModel Amp { get; } = new();
    private byte[]? ampOriginalRam;
    private string? ampPreviewIdentity;
    public bool CanUseAmp => CanOperate && !Ir.PreviewActive
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool AmpPreviewActive => ampOriginalRam != null;
    private Task<bool>? ampPreviewTask;
    private bool ampPreviewPending;
    public bool IsAmpPreviewSending { get; private set; }
    public bool CanAdjustAmp => (!Busy || IsAmpPreviewSending) && Amp.Loaded;
    private string ampPreviewMessage = "Ajuste seu som. As alterações são ouvidas automaticamente.";
    public string AmpPreviewMessage { get => ampPreviewMessage; private set => Set(ref ampPreviewMessage, value); }
    public bool CanEditAmpLocally => !Busy && !IsAmpPreviewSending;
    private void AmpEdited() => _ = PreviewAmpAsync();
    public Task<bool> PreviewAmpAsync()
    {
        ampPreviewPending = true;
        return ampPreviewTask ??= SendLatestAmpAsync();
    }
    private async Task<bool> SendLatestAmpAsync()
    {
        IsAmpPreviewSending = true;
        Notify(nameof(IsAmpPreviewSending)); Notify(nameof(CanAdjustAmp)); Notify(nameof(CanEditAmpLocally));
        await Task.Yield();
        bool applied = false;
        try
        {
            while (ampPreviewPending)
            {
                ampPreviewPending = false;
                if (!CanUseAmp || !Amp.Loaded)
                {
                    AmpPreviewMessage = "Seu timbre está no computador. Conecte o pedal para ouvir.";
                    return false;
                }
                AmpPreviewMessage = "Atualizando seu som…";
                applied = await ApplyAmpOnceAsync();
                if (!applied)
                {
                    ampPreviewPending = false;
                    AmpPreviewMessage = "Não foi possível ouvir no pedal. Seus ajustes continuam no computador.";
                    return false;
                }
            }
            AmpPreviewMessage = "Seu timbre está pronto para ouvir.";
            return applied;
        }
        finally
        {
            IsAmpPreviewSending = false; ampPreviewTask = null;
            Notify(nameof(IsAmpPreviewSending)); Notify(nameof(CanAdjustAmp)); Notify(nameof(CanEditAmpLocally));
        }
    }
    public bool CanOpenGearGallery => !Busy && (IsAmp || CanEdit);
    private string AmpRecoveryDirectory => Path.Combine(DataDirectory, "AmpRecovery");

    public async Task<bool> ReadAmpAsync(int? sourceSlot = null)
    {
        if (!CanUseAmp) return false;
        bool loaded = false;
        int slot = sourceSlot ?? Amp.Slot;
        await Execute(async () =>
        {
            var bytes = await session!.ReadAmpSlot(slot);
            Amp.LoadSlot(bytes, slot);
            loaded = true;
            Status = "Amp carregado para edição local.";
        });
        return loaded;
    }

    public async Task<bool?> ImportAmpAsync()
    {
        if (Busy) return null;
        var dialog = new OpenFileDialog { Filter = Localization.LanguageService.T("Preset de amp (*.bin)") + "|*.bin" };
        if (dialog.ShowDialog() != true) return null;
        bool completed = false;
        await Execute(async () =>
        {
            var info = new FileInfo(dialog.FileName);
            if (info.Length != AmpImage.Size) throw new ArgumentException("O arquivo deve ter exatamente 4096 bytes.");
            Amp.Load(await File.ReadAllBytesAsync(dialog.FileName), Path.GetFileName(dialog.FileName));
            Status = "Amp importado. O pedal não foi alterado.";
            completed = true;
        });
        return completed;
    }

    public async Task<bool?> ExportAmpAsync()
    {
        if (Busy || !Amp.Loaded) return null;
        var dialog = new SaveFileDialog { Filter = Localization.LanguageService.T("Preset de amp (*.bin)") + "|*.bin", FileName = "MeuPreset.bin" };
        if (dialog.ShowDialog() != true) return null;
        bool completed = false;
        await Execute(async () => { await File.WriteAllBytesAsync(dialog.FileName, Amp.Build()); Status = "Amp exportado."; completed = true; });
        return completed;
    }

    private async Task<bool> ApplyAmpOnceAsync()
    {
        if (!CanUseAmp || !Amp.Loaded) return false;
        bool applied = false;
        await Execute(async () =>
        {
            var data = Amp.Build();
            if (ampOriginalRam != null && ampPreviewIdentity != session!.IdentityHex) throw new ArgumentException("Reconecte o aparelho usado na prévia.");
            await session!.ApplyAmpRam(data.AsSpan(AmpImage.BodyOffset, AmpImage.BodyLength).ToArray(), ampOriginalRam == null ? async before =>
            {
                Directory.CreateDirectory(AmpRecoveryDirectory);
                var recovery = new byte[AmpImage.Size]; recovery[0] = 1; recovery[1] = 7;
                before.CopyTo(recovery, AmpImage.BodyOffset);
                await File.WriteAllBytesAsync(Path.Combine(AmpRecoveryDirectory, $"ram-{DateTime.Now:yyyyMMdd-HHmmss-fff}.bin"), recovery);
                ampOriginalRam = before; ampPreviewIdentity = session.IdentityHex;
            } : null);
            Status = "Amp aplicado em RAM. Nenhum slot foi gravado. Trocar o amp físico pode encerrar a prévia.";
            Notify(nameof(AmpPreviewActive));
            applied = true;
        });
        return applied;
    }

    public async Task<bool?> RestoreAmpAsync()
    {
        if (!CanUseAmp || ampOriginalRam == null) return null;
        bool completed = false;
        await Execute(async () => { await RestoreAmpCore(); completed = true; });
        return completed;
    }
    private async Task RestoreAmpCore()
    {
        if (ampOriginalRam == null) return;
        if (session == null || session.IdentityHex != ampPreviewIdentity) throw new ArgumentException("Reconecte o aparelho usado na prévia.");
        await session.ApplyAmpRam(ampOriginalRam);
        ampOriginalRam = null; ampPreviewIdentity = null;
        Notify(nameof(AmpPreviewActive)); Status = "RAM anterior restaurada e verificada.";
    }

    public async Task<bool?> WriteAmpAsync(Func<string, Task<bool>>? confirm = null)
    {
        if (!CanUseAmp || !Amp.Loaded) return null;
        bool? completed = false;
        await Execute(async () =>
        {
            int slot = Amp.Slot;
            byte[] target = Amp.Build();
            var current = await session!.ReadAmpSlot(slot);
            if (!await (confirm ?? Ask)($"Salvar seu amp no slot {slot + 1}?\nO som salvo nessa posição será substituído.\nO som anterior será guardado para recuperação.")) { completed = null; return; }
            await session.ReplaceAmp(slot, current, target, async bytes =>
            {
                Directory.CreateDirectory(AmpRecoveryDirectory);
                string path = Path.Combine(AmpRecoveryDirectory, $"amp-{slot + 1}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.bin");
                await File.WriteAllBytesAsync(path, bytes);
                if (!(await File.ReadAllBytesAsync(path)).SequenceEqual(bytes)) throw new IOException("Não foi possível preservar o amp original.");
            }, new Progress<TransferProgress>(p => Status = $"{p.Stage}: {p.Done}/{p.Total}"));
            completed = true;
            Status = $"Amp {slot + 1} gravado e verificado. Selecione outro amp no pedal e volte para recarregar. A prévia em RAM é independente.";
        });
        return completed;
    }
}
