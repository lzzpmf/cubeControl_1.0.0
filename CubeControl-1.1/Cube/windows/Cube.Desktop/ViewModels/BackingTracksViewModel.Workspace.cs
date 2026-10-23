using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public enum BackingTransportState { Stopped, Preparing, CountingIn, Playing, Recording, Finalizing, Error }

public sealed partial class BackingTracksViewModel
{
    public bool CanAdjustMix => !projectBusy && !IsExporting;
    public BackingTransportState TransportState => recordingPreparing ? BackingTransportState.Preparing : recordingFinalizing ? BackingTransportState.Finalizing
        : IsRecording ? engine.IsCountingIn ? BackingTransportState.CountingIn : BackingTransportState.Recording
        : HasError ? BackingTransportState.Error : IsLoading ? BackingTransportState.Preparing
        : IsPlaying ? BackingTransportState.Playing : BackingTransportState.Stopped;
    public string TransportLabel => TransportState switch
    {
        BackingTransportState.Preparing => "Preparando…", BackingTransportState.CountingIn => "Pré-contagem",
        BackingTransportState.Playing => "Reproduzindo", BackingTransportState.Recording => "Gravando",
        BackingTransportState.Finalizing => "Finalizando gravação…", BackingTransportState.Error => "Verifique o áudio",
        _ => "Pronto para gravar"
    };
    public BackingClip? SelectedClip => SelectedClipItems().Select(x => x.Clip).FirstOrDefault();
    public BackingClipTake[] SelectedTakes => SelectedClip?.Takes ?? [];
    public bool HasSelectedClip => SelectedClip != null;
    public BackingClipTake? SelectedTake
    {
        get => SelectedClip?.ActiveTake;
        set
        {
            if (!CanEditSession || SelectedClip is not { } clip || value == null || value.Incomplete
                || !clip.Takes.Contains(value) || clip.ActiveTakeId == value.Id || SelectedClipItems().FirstOrDefault().Stem is not { } stem) return;
            BeginClipGesture(); ReplaceClip(stem, clip with { ActiveTakeId = value.Id }); EndClipGesture();
        }
    }
    public string TakeName
    {
        get => SelectedTake?.Name ?? "";
        set
        {
            if (!CanEditSession || SelectedClip is not { } clip || SelectedTake is not { } take || string.IsNullOrWhiteSpace(value)
                || value.Trim() == take.Name || SelectedClipItems().FirstOrDefault().Stem is not { } stem) return;
            BeginClipGesture(); ReplaceClip(stem, clip with { Takes = clip.Takes.Select(t => t.Id == take.Id ? t with { Name = value.Trim() } : t).ToArray() }); EndClipGesture();
        }
    }
    public double ClipFadeIn { get => SelectedClip?.FadeIn ?? 0; set => SetClipFade(value, true); }
    public double ClipFadeOut { get => SelectedClip?.FadeOut ?? 0; set => SetClipFade(value, false); }
    private void SetClipFade(double value, bool fadeIn)
    {
        if (!CanEditSession || !double.IsFinite(value) || SelectedClip is not { } clip || SelectedClipItems().FirstOrDefault().Stem is not { } stem) return;
        value = Math.Clamp(value, 0, clip.Duration);
        if ((fadeIn ? clip.FadeIn : clip.FadeOut) == value) return;
        BeginClipGesture(); ReplaceClip(stem, fadeIn ? clip with { FadeIn = value } : clip with { FadeOut = value }); EndClipGesture();
    }
    public bool CanCrossfade
    {
        get
        {
            var items = SelectedClipItems().OrderBy(x => x.Clip.Position).ToArray();
            return CanEditSession && items.Length == 2 && items[0].Stem == items[1].Stem
                && items[1].Clip.Position > items[0].Clip.Position && items[1].Clip.Position < items[0].Clip.End
                && items[1].Clip.End >= items[0].Clip.End;
        }
    }
    public ICommand Undo { get; private set; } = null!;
    public ICommand Redo { get; private set; } = null!;
    public ICommand Crossfade { get; private set; } = null!;
    public ICommand AuditionTake { get; private set; } = null!;
    public ICommand PreviousTake { get; private set; } = null!;
    public ICommand NextTake { get; private set; } = null!;
    public ICommand AcceptSuggestedBpm { get; private set; } = null!;
    public bool HasInterruptedTake => SelectedStem?.Takes.Any(t => t.Incomplete) == true;
    public ICommand RecoverInterruptedTake { get; private set; } = null!;
    public ICommand DiscardInterruptedTake { get; private set; } = null!;
    private async Task RecoverInterruptedAsync()
    {
        if (SelectedStem is not { } stem || stem.Takes.LastOrDefault(t => t.Incomplete) is not { } interrupted) return;
        string target = Path.Combine(recordingDirectory, "Recovered-" + Guid.NewGuid().ToString("N") + ".wav");
        try
        {
            Directory.CreateDirectory(recordingDirectory);
            string recovered = await Task.Run(() => BackingRecordingRecovery.Recover(interrupted.Source, target));
            var wave = await BackingAudioProcessing.Analyze(recovered, lifetime.Token, 32768);
            if (!CanEditSession || SessionStems?.Contains(stem) != true) return;
            BeginClipGesture();
            var take = new BackingClipTake(Guid.NewGuid(), Cube.Desktop.Localization.LanguageService.T("Tomada recuperada"), [new(Guid.NewGuid(), recovered, 0, 0, wave.Duration * interrupted.RecordedSpeed, interrupted.RecordedSpeed, interrupted.RecordedPitch)]);
            var clip = new BackingClip(Guid.NewGuid(), stem.Name, interrupted.Offset, wave.Duration * interrupted.RecordedSpeed, [take], take.Id);
            var updated = BackingClipEdit.Overdub(stem.Clips, clip); stem.Clips.Clear(); stem.Clips.AddRange(updated);
            ClipWaves[recovered] = wave; SelectClip(stem, clip, false); EndClipGesture();
            Status = "Tomada recuperada em uma cópia. O arquivo interrompido foi preservado."; HasError = false;
        }
        catch (Exception ex) { Error("Não foi possível recuperar esta tomada.", ex); }
    }
    public string SuggestedBpmLabel => SelectedTrack?.SuggestedBpm > 0 ? $"BPM sugerido: {SelectedTrack.SuggestedBpm:0.#}" : "BPM não identificado";
    private string panelSettingsPath = "";
    private bool libraryOpen, mixerOpen;
    private double mixerHeight = 260, libraryWidth = 310;
    public bool LibraryOpen { get => libraryOpen; set { if (Set(ref libraryOpen, value)) SavePanelSettings(); } }
    public bool MixerOpen { get => mixerOpen; set { if (Set(ref mixerOpen, value)) SavePanelSettings(); } }
    public double MixerHeight { get => mixerHeight; set { if (double.IsFinite(value) && Set(ref mixerHeight, Math.Clamp(value, 160, 600))) SavePanelSettings(); } }
    public double LibraryWidth { get => libraryWidth; set { if (double.IsFinite(value) && Set(ref libraryWidth, Math.Clamp(value, 240, 480))) SavePanelSettings(); } }
    private sealed record PanelSettings(bool Library, bool Mixer, double Height, double Width);
    private void InitializeWorkspace()
    {
        panelSettingsPath = Path.Combine(Path.GetDirectoryName(studioSettingsPath)!, "backing-panels.json");
        try
        {
            if (File.Exists(panelSettingsPath) && JsonSerializer.Deserialize<PanelSettings>(File.ReadAllText(panelSettingsPath)) is { } p)
            { libraryOpen = p.Library; mixerOpen = p.Mixer; mixerHeight = Math.Clamp(p.Height, 160, 600); libraryWidth = Math.Clamp(p.Width, 240, 480); }
        }
        catch (Exception ex) { Log(ex); }
        Undo = new UiCommand(() => UndoEdit(), () => CanEditSession && History?.Undo.Count > 0);
        Redo = new UiCommand(() => UndoEdit(true), () => CanEditSession && History?.Redo.Count > 0);
        Crossfade = new UiCommand(CrossfadeClips, () => CanCrossfade);
        AuditionTake = new UiCommand(() => { if (SelectedClip is { } clip) { Position = Math.Max(0, clip.Position); PlaySelected(); } }, () => CanEditSession && SelectedClip != null && SelectedOutput != null);
        PreviousTake = new UiCommand(() => StepTake(-1), () => CanEditSession && SelectedTakes.Count(t => !t.Incomplete) > 1);
        NextTake = new UiCommand(() => StepTake(1), () => CanEditSession && SelectedTakes.Count(t => !t.Incomplete) > 1);
        AcceptSuggestedBpm = new UiCommand(() => { if (SelectedTrack is { SuggestedBpm: > 0 } track) Bpm = track.SuggestedBpm; }, () => CanEditSession && SelectedTrack?.SuggestedBpm > 0);
        RecoverInterruptedTake = new UiCommand(() => Start(RecoverInterruptedAsync()), () => CanEditSession && HasInterruptedTake);
        DiscardInterruptedTake = new UiCommand(() =>
        {
            if (SelectedStem is not { } stem || stem.Takes.LastOrDefault(t => t.Incomplete) is not { } take) return;
            CommitEditHistory(); stem.Takes.Remove(take); MarkProjectChanged(); CommitEditHistory(); RefreshClipPresentation();
            Status = "Referência da tomada removida. O arquivo original foi preservado.";
        }, () => CanEditSession && HasInterruptedTake);
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CanEditSession))
            {
                Notify(nameof(CanAdjustMix));
                foreach (var stem in SessionStems ?? []) { stem.CanEditRouting = CanEditSession; stem.CanAdjustMix = CanAdjustMix; }
            }
            if (e.PropertyName is nameof(IsPlaying) or nameof(IsRecording) or nameof(RecordingTime) or nameof(State) or nameof(HasError) or nameof(CanEditSession))
            { Notify(nameof(TransportState)); Notify(nameof(TransportLabel)); }
            if (e.PropertyName is nameof(SelectedStem)) RefreshClipPresentation();
            if (e.PropertyName is nameof(SelectedTrack) or nameof(BpmBadge)) Notify(nameof(SuggestedBpmLabel));
        };
    }
    private void RefreshClipPresentation()
    {
        foreach (string p in new[] { nameof(SelectedClip), nameof(SelectedTakes), nameof(SelectedTake), nameof(TakeName), nameof(HasSelectedClip), nameof(ClipFadeIn), nameof(ClipFadeOut), nameof(CanCrossfade), nameof(HasInterruptedTake) }) Notify(p);
    }
    private void StepTake(int step)
    {
        var takes = SelectedTakes.Where(t => !t.Incomplete).ToArray(); if (takes.Length == 0) return;
        int current = Array.FindIndex(takes, t => t.Id == SelectedTake?.Id);
        SelectedTake = takes[(current + step + takes.Length) % takes.Length];
    }
    private void SavePanelSettings()
    {
        if (string.IsNullOrEmpty(panelSettingsPath) || disposed) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(panelSettingsPath)!);
            string temporary = panelSettingsPath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(new PanelSettings(libraryOpen, mixerOpen, mixerHeight, libraryWidth)));
            File.Move(temporary, panelSettingsPath, true);
        }
        catch (Exception ex) { Log(ex); }
    }
}
