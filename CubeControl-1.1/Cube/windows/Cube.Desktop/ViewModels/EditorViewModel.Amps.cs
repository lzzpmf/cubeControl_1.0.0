using System.IO;
using Cube.Protocol;
#if WINUI
using Cube.WinUI.Platform;
#else
using Microsoft.Win32;
#endif

namespace Cube.Desktop.ViewModels;

public enum AmpOperationState { Local, PreviewSending, PreviewConfirmed, Saving, Saved, Restored, Error }

public sealed partial class EditorViewModel
{
    public AmpViewModel Amp { get; } = new();
    private byte[]? ampOriginalRam;
    private string? ampPreviewIdentity;
    public bool CanUseAmp => CanOperate
#if CUBE_SIMULATION
        && false
#endif
        ;
    public bool AmpPreviewActive => ampOriginalRam != null;
    public bool CanRestoreAmp => Connected && !Busy && AmpPreviewActive;
    public System.Windows.Input.ICommand EndAmpPreview { get; private set; } = null!;
    public bool CanSaveAmp => CanUseAmp && Amp.Loaded;
    private AmpOperationState ampOperation;
    public AmpOperationState AmpOperation { get => ampOperation; private set => Set(ref ampOperation, value); }
    private byte[]? ampSavedBytes;
    private int ampSavedSlot;
    private string? ampSavedIdentity;
    private string ampSource = "";
    public string AmpSource { get => ampSource; private set => Set(ref ampSource, value); }
    public string AmpSavedMessage => ampSavedBytes == null ? "" : $"Salvo no slot {ampSavedSlot + 1}";
    private void ReportAmpResult(bool? success, string done, string failed)
    {
        if (!success.HasValue) return;
        AmpPreviewMessage = success.Value ? done : failed;
        if (!success.Value) AmpOperation = AmpOperationState.Error;
    }
    private void InvalidateAmpSaved()
    {
        ampSavedBytes = null; ampSavedIdentity = null;
        Notify(nameof(AmpSavedMessage));
        if (AmpOperation == AmpOperationState.Saved) AmpOperation = AmpOperationState.Local;
    }
    private void RefreshAmpAvailability()
    {
        if (ampSavedBytes != null && (session?.IdentityHex != ampSavedIdentity || Amp.Slot != ampSavedSlot || blocked || session?.IsFaulted == true)) InvalidateAmpSaved();
        if (!Connected && AmpOperation == AmpOperationState.PreviewConfirmed)
        {
            AmpOperation = AmpOperationState.Local;
            AmpPreviewMessage = "Seu timbre está no computador. Conecte o pedal para ouvir.";
        }
        Notify(nameof(CanRestoreAmp)); Notify(nameof(CanSaveAmp));
        Notify(nameof(CanAdjustAmp)); Notify(nameof(CanEditAmpLocally));
    }
    private void AmpPresentationChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Amp.Loaded)) { InvalidateAmpSaved(); AmpOperation = AmpOperationState.Local; }
        if (e.PropertyName is nameof(Amp.Loaded) or nameof(Amp.Slot)) RefreshAmpAvailability();
    }
    private Task<bool>? ampPreviewTask;
    private bool ampPreviewPending, retiringAmp;
    private object? ampHistoryGroup;
    public bool IsAmpPreviewSending { get; private set; }
    public bool CanAdjustAmp => (!Busy || IsAmpPreviewSending) && Amp.Loaded;
    private string ampPreviewMessage = "Ajuste seu som. As alterações são ouvidas automaticamente.";
    public string AmpPreviewMessage { get => ampPreviewMessage; private set => Set(ref ampPreviewMessage, value); }
    public bool CanEditAmpLocally => !Busy && !IsAmpPreviewSending;
    private void AmpEdited() { InvalidateAmpSaved(); _ = PreviewAmpAsync(); }
    public Task<bool> PreviewAmpAsync()
    {
        if (retiringAmp) return Task.FromResult(false);
        ampHistoryGroup = OperationGroup("AMP");
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
                    AmpOperation = AmpOperationState.Local;
                    AmpPreviewMessage = "Seu timbre está no computador. Conecte o pedal para ouvir.";
                    return false;
                }
                AmpOperation = AmpOperationState.PreviewSending;
                AmpPreviewMessage = "Atualizando seu som…";
                applied = await ApplyAmpOnceAsync();
                if (!applied)
                {
                    ampPreviewPending = false;
                    AmpOperation = AmpOperationState.Error;
                    AmpPreviewMessage = "Não foi possível ouvir no pedal. Seus ajustes continuam no computador.";
                    return false;
                }
            }
            AmpOperation = AmpOperationState.PreviewConfirmed;
            AmpPreviewMessage = "Prévia confirmada na RAM. Nenhum slot foi gravado.";
            return applied;
        }
        finally
        {
            IsAmpPreviewSending = false; ampPreviewTask = null;
            Notify(nameof(IsAmpPreviewSending)); Notify(nameof(CanAdjustAmp)); Notify(nameof(CanEditAmpLocally));
        }
    }
    public bool CanOpenGearGallery => !Busy && (IsAmp || CanEdit);
    private byte[]? ampSlotRecovery;

    public async Task<bool> ReadAmpAsync(int? sourceSlot = null)
    {
        if (!CanUseAmp) return false;
        bool loaded = false;
        int slot = sourceSlot ?? Amp.Slot;
        await Execute(async () =>
        {
            var bytes = await session!.ReadAmpSlot(slot);
            Amp.LoadSlot(bytes, slot);
            IdentifyAmpArtwork(slot, bytes);
            AmpSource = "Modelo carregado do pedal";
            loaded = true;
            Status = "Amp carregado para edição local.";
        });
        ReportAmpResult(loaded, "Amp carregado para edição local.", "Não foi possível carregar o amp. Os ajustes anteriores foram mantidos.");
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
            AmpSource = "Arquivo importado";
            Status = "Amp importado. O pedal não foi alterado.";
            completed = true;
        });
        ReportAmpResult(completed, "Arquivo importado. A edição está no computador.", "Não foi possível importar esse amp.");
        return completed;
    }

    public async Task<bool?> ExportAmpAsync()
    {
        if (Busy || !Amp.Loaded) return null;
        var dialog = new SaveFileDialog { Filter = Localization.LanguageService.T("Preset de amp (*.bin)") + "|*.bin", FileName = "MeuPreset.bin" };
        if (dialog.ShowDialog() != true) return null;
        bool completed = false;
        await Execute(async () => { await File.WriteAllBytesAsync(dialog.FileName, Amp.Build()); Status = "Amp exportado."; completed = true; });
        ReportAmpResult(completed, "Seu amp foi exportado.", "Não foi possível exportar seu amp.");
        return completed;
    }

    private async Task<bool> ApplyAmpOnceAsync()
    {
        if (!CanUseAmp || !Amp.Loaded) return false;
        bool applied = false;
        await Execute(async () =>
        {
            var data = Amp.Build();
            var editSnapshot = Amp.CaptureEditSnapshot();
            string name = Amp.Name;
            var bankContext = confirmed?.ToArray();
            object? group = ampHistoryGroup;
            if (ampOriginalRam != null && ampPreviewIdentity != session!.IdentityHex) throw new ArgumentException("Reconecte o aparelho usado na prévia.");
            await session!.ApplyAmpRam(data.AsSpan(AmpImage.BodyOffset, AmpImage.BodyLength).ToArray(), ampOriginalRam == null ? async before =>
            {
                ampOriginalRam = before.ToArray(); ampPreviewIdentity = session.IdentityHex;
                await Task.CompletedTask;
            } : null);
            Status = "Amp aplicado em RAM. Nenhum slot foi gravado. Trocar o amp físico pode encerrar a prévia.";
            Notify(nameof(AmpPreviewActive));
            applied = true;
            RecordOperation("Prévia de AMP: " + name, async () =>
            {
                Amp.RestoreEditSnapshot(editSnapshot);
                return await PreviewAmpAsync();
            }, () => CanUseAmp && bankContext != null && confirmed?.SequenceEqual(bankContext) == true, group);
        });
        return applied;
    }

    public async Task<bool?> RestoreAmpAsync()
    {
        if (!CanRestoreAmp || ampOriginalRam == null) return null;
        bool completed = false;
        await Execute(async () => { await RestoreAmpCore(); completed = true; });
        if (completed) AmpOperation = AmpOperationState.Restored;
        ReportAmpResult(completed, "Som anterior restaurado no pedal. A edição local foi mantida.", "Não foi possível restaurar o som anterior.");
        return completed;
    }
    private async Task RestoreAmpCore()
    {
        ampPreviewPending = false;
        if (ampOriginalRam == null) return;
        retiringAmp = true;
        try
        {
        if (session == null || session.IdentityHex != ampPreviewIdentity) throw new ArgumentException("Reconecte o aparelho usado na prévia.");
        await session.ApplyAmpRam(ampOriginalRam);
        ampOriginalRam = null; ampPreviewIdentity = null;
        Notify(nameof(AmpPreviewActive)); Status = "RAM anterior restaurada e verificada.";
        }
        finally { retiringAmp = false; }
    }

    public async Task<bool?> WriteAmpAsync(Func<string, Task<bool>>? confirm = null)
    {
        if (!CanUseAmp || !Amp.Loaded) return null;
        bool? completed = false;
        await Execute(async () =>
        {
            if (!await ConfirmAdvancedPreviewChangeAsync()) { completed = null; return; }
            int slot = Amp.Slot;
            byte[] target = Amp.Build();
            var current = await session!.ReadAmpSlot(slot);
            if (!await (confirm ?? Ask)($"Salvar seu amp no slot {slot + 1}?\nO som salvo nessa posição será substituído.\nO som anterior será mantido em memória nesta sessão.")) { completed = null; return; }
            InvalidateAmpSaved();
            AmpOperation = AmpOperationState.Saving;
            AmpPreviewMessage = "Salvando no pedal…";
            await session.ReplaceAmp(slot, current, target, async bytes =>
            {
                ampSlotRecovery = bytes.ToArray();
                await Task.CompletedTask;
            }, new TransferProgressThrottle(p => Status = $"{p.Stage}: {p.Done}/{p.Total}"));
            completed = true;
            soundContentRevision++;
            IdentifyAmpArtwork(slot, target);
            ampSavedBytes = target.ToArray(); ampSavedSlot = slot; ampSavedIdentity = session.IdentityHex;
            Notify(nameof(AmpSavedMessage));
            AmpOperation = AmpOperationState.Saved;
            Status = $"Amp {slot + 1} gravado e verificado. Selecione outro amp no pedal e volte para recarregar. A prévia em RAM é independente.";
        });
        ReportAmpResult(completed, "Amp salvo. Troque de amp no pedal e volte para ouvir o som salvo.", "Não foi possível salvar seu amp no pedal.");
        return completed;
    }
}
