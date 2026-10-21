namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private EffectEditorCommands? chorusCommands;
    public EffectEditorCommands ChorusCommands => chorusCommands ??= new(
        ReadChorusAsync, RestoreChorusAsync, ImportChorusAsync, ExportChorusAsync,
        () => CanOperate, () => CanRestoreChorus, () => CanUseChorusFiles, () => CanUseChorusFiles && Chorus.Loaded,
        ex => { ErrorMessage = ex.Message; SetAdvancedError(1, ex.Message); }, () => ListenAdvancedModulationAsync(1), () => CanListenChorus);

    private EffectEditorCommands? phaserCommands;
    public EffectEditorCommands PhaserCommands => phaserCommands ??= new(
        ReadPhaserAsync, RestorePhaserAsync, ImportPhaserAsync, ExportPhaserAsync,
        () => CanOperate, () => CanRestorePhaser, () => CanUsePhaserFiles, () => CanUsePhaserFiles && Phaser.Loaded,
        ex => { ErrorMessage = ex.Message; SetAdvancedError(2, ex.Message); }, () => ListenAdvancedModulationAsync(2), () => CanListenPhaser);

    private EffectEditorCommands? delayCommands;
    public EffectEditorCommands DelayCommands => delayCommands ??= new(
        ReadDelayAsync, RestoreDelayAsync, ImportDelayAsync, ExportDelayAsync,
        () => CanOperate, () => CanRestoreDelay, () => CanUseDelayFiles, () => CanUseDelayFiles && Delay.Loaded,
        ex => { ErrorMessage = ex.Message; SetAdvancedError(3, ex.Message); });

    private EffectEditorCommands? reverbCommands;
    public EffectEditorCommands ReverbCommands => reverbCommands ??= new(
        ReadReverbAsync, RestoreReverbAsync, ImportReverbAsync, ExportReverbAsync,
        () => CanOperate, () => CanRestoreReverb, () => CanUseReverbFiles, () => CanUseReverbFiles && Reverb.Loaded,
        ex => { ErrorMessage = ex.Message; SetAdvancedError(4, ex.Message); });

}
