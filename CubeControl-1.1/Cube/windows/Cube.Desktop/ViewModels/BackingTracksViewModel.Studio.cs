using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
#if WINUI
using Cube.WinUI.Platform;
#else
using Microsoft.Win32;
#endif
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed record LocalAudioFile(string Path, string Name, double Seconds, long Bytes, string Category)
{
    public string Details => $"{BackingTracksViewModel.FormatTime(Seconds)}    {Bytes / 1048576d:0.0} MB";
}

public sealed partial class BackingTracksViewModel
{
    private sealed record StudioSettings(int Version, Guid? Session, string[] Folders, Dictionary<string, string> Categories);
    private string studioSettingsPath = "", localSearch = "", localCategory = "Todos";
    private bool studioSettingsInvalid;
    private readonly List<string> libraryFolders = [];
    private readonly Dictionary<string, string> libraryCategories = new(StringComparer.OrdinalIgnoreCase);
    private LocalAudioFile[] localCatalog = [];
    public ObservableCollection<LocalAudioFile> LocalFiles { get; } = [];
    public string[] LocalCategories { get; } = ["Todos", "Loops", "One Shots", "Vocais"];
    public double[] StudioSpeeds { get; } = Enumerable.Range(50, 101).Select(x => (double)x).ToArray();
    public double[] StudioPitches { get; } = Enumerable.Range(-6, 13).Select(x => (double)x).ToArray();
    public double StudioDuration => Math.Max(Duration, 32 * 60 / Math.Max(20, TransportBpm));
    public double StudioViewportDuration => Duration > 0 ? Math.Max(.1, ViewportDuration) : StudioDuration;
    public string LocalSearch { get => localSearch; set { if (Set(ref localSearch, value)) FilterLibrary(); } }
    public string LocalCategory { get => localCategory; set { if (value != null && Set(ref localCategory, value)) FilterLibrary(); } }
    public ICommand AddLibraryFolder { get; private set; } = null!;
    public ICommand CutAtCursor { get; private set; } = null!;
    private BackingAudioEngine? localPreviewEngine;
    private int localPreviewGeneration;
    private string? localPreviewPath;
    private Guid? restoredSession;

    private void InitializeStudio(string directory)
    {
        studioSettingsPath = System.IO.Path.Combine(directory, "backing-studio.json");
        try
        {
            if (File.Exists(studioSettingsPath))
            {
                var settings = JsonSerializer.Deserialize<StudioSettings>(File.ReadAllText(studioSettingsPath)) ?? throw new InvalidDataException();
                if (settings.Version != 1 || settings.Folders == null || settings.Categories == null) throw new InvalidDataException();
                libraryFolders.AddRange(settings.Folders.Distinct(StringComparer.OrdinalIgnoreCase));
                foreach (var pair in settings.Categories) libraryCategories[pair.Key] = pair.Value;
                restoredSession = settings.Session;
            }
        }
        catch (Exception ex) { studioSettingsInvalid = true; Error("Não foi possível carregar a biblioteca local.", ex); }
        AddLibraryFolder = new UiCommand(() =>
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true) Start(AddLocalFolder(dialog.FolderName));
        });
        CutAtCursor = new UiCommand(CutStudioClips, () => CanEditSession && CutTargets().Any());
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(Duration) or nameof(ViewportDuration) or nameof(TransportBpm) or nameof(Bpm))
            { Notify(nameof(StudioDuration)); Notify(nameof(StudioViewportDuration)); }
            if (e.PropertyName == nameof(SelectedTrack)) SaveStudio();
        };
    }
    private IEnumerable<(BackingStem Stem, BackingClip Clip)> CutTargets()
    {
        var selected = SelectedClipItems().ToArray();
        return (selected.Length > 0 ? selected : SelectedStem?.Clips.Select(c => (SelectedStem, c)) ?? [])
            .Where(x => Position > x.Item2.Position && Position < x.Item2.End);
    }
    public void CutStudioClips()
    {
        if (!CanEditSession) return;
        var targets = CutTargets().ToArray();
        if (targets.Length == 0) return;
        if (SelectedClips.Count == 0) foreach (var item in targets) SelectedClips.Add(item.Clip.Id);
        SplitClips();
    }
    private void RestoreStudio()
    {
        if (playlistLoadFailed) return;
        SelectedTrack = Tracks.FirstOrDefault(t => t.SessionId == restoredSession) ?? SelectedTrack ?? Tracks.FirstOrDefault();
        if (SelectedTrack == null) AddEmptyStem.Execute(null);
        TrackHeight = 96;
        TrackHeaderWidth = 335;
        Start(RefreshLocalLibrary());
    }
    private void SaveStudio()
    {
        if (studioSettingsInvalid || !playlistLoaded || disposed) return;
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(studioSettingsPath)!);
            string temporary = studioSettingsPath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(new StudioSettings(1, SelectedTrack?.SessionId, libraryFolders.ToArray(), libraryCategories)));
            File.Move(temporary, studioSettingsPath, true);
        }
        catch (Exception ex) { Error("Não foi possível salvar a biblioteca.", ex); }
    }
    public async Task AddLocalFolder(string folder)
    {
        folder = System.IO.Path.GetFullPath(folder);
        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(folder);
        if (!libraryFolders.Contains(folder, StringComparer.OrdinalIgnoreCase)) libraryFolders.Add(folder);
        SaveStudio(); await RefreshLocalLibrary();
    }
    private async Task RefreshLocalLibrary()
    {
        var folders = libraryFolders.ToArray();
        var cancellation = lifetime.Token;
        var files = await Task.Run(() =>
        {
            var result = new List<LocalAudioFile>();
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
                foreach (string path in Directory.EnumerateFiles(folder, "*", options).Where(f => System.IO.Path.GetExtension(f).ToLowerInvariant() is ".wav" or ".mp3"))
                {
                    cancellation.ThrowIfCancellationRequested();
                    try { using var decoder = new BackingDecoder(path); result.Add(new(path, System.IO.Path.GetFileName(path), decoder.Duration, new FileInfo(path).Length, "")); }
                    catch (Exception ex) when (ex is not OperationCanceledException) { Log(ex); }
                }
            }
            return result.DistinctBy(f => f.Path, StringComparer.OrdinalIgnoreCase).OrderBy(f => f.Name).ToArray();
        }, cancellation);
        if (disposed) return;
        localCatalog = files; FilterLibrary();
    }
    private void FilterLibrary()
    {
        LocalFiles.Clear();
        foreach (var file in localCatalog)
        {
            string category = libraryCategories.GetValueOrDefault(file.Path, "");
            if ((LocalCategory == "Todos" || LocalCategory == category) && file.Name.Contains(LocalSearch, StringComparison.CurrentCultureIgnoreCase))
                LocalFiles.Add(file with { Category = category });
        }
    }
    public void CategorizeLocalFile(LocalAudioFile file, string category)
    {
        if (!LocalCategories.Contains(category)) return;
        if (category == "Todos") libraryCategories.Remove(file.Path); else libraryCategories[file.Path] = category;
        SaveStudio(); FilterLibrary();
    }
    public async Task InsertLocalAudio(string file, BackingStem? target = null, double? position = null)
    {
        if (!CanEditSession || !File.Exists(file) || System.IO.Path.GetExtension(file).ToLowerInvariant() is not (".wav" or ".mp3")) return;
        target ??= SelectedStem;
        if (target == null || SessionStems?.Contains(target) != true) return;
        var session = SelectedTrack;
        double at = Math.Max(0, position ?? Position);
        var wave = await BackingAudioProcessing.Analyze(file, lifetime.Token, 32768);
        if (wave.Duration <= 0) return;
        if (disposed || !CanEditSession || session != SelectedTrack || SessionStems?.Contains(target) != true) return;
        BeginClipGesture();
        var take = new BackingClipTake(Guid.NewGuid(), System.IO.Path.GetFileNameWithoutExtension(file), [new(Guid.NewGuid(), file, 0, 0, wave.Duration)]);
        var clip = new BackingClip(Guid.NewGuid(), System.IO.Path.GetFileName(file), at, wave.Duration, [take], take.Id);
        ClipWaves[file] = wave; target.Clips.Add(clip); target.ClipsInitialized = true;
        SelectClip(target, clip, false); EndClipGesture();
    }
    public async Task PreviewLocalAudio(LocalAudioFile file)
    {
        if (!CanEditSession) return;
        if (SelectedOutput == null) { Status = "Escolha a saída de áudio"; return; }
        if (!File.Exists(file.Path)) { Status = "Arquivo ausente"; return; }
        if (localPreviewPath == file.Path) { await StopLocalPreview(); return; }
        int generation = ++localPreviewGeneration;
        StopYouTubePreview(); localPreviewEngine ??= new();
        try
        {
            await localPreviewEngine.Open(file.Path, SelectedOutput.Id, 0, 1, 0, false, 0, file.Seconds, .7, false);
            if (generation != localPreviewGeneration || disposed || !CanEditSession) return;
            if (IsPlaying) await engine.Pause();
            if (BeforePlayback != null) await BeforePlayback();
            if (generation != localPreviewGeneration || disposed || !CanEditSession) return;
            localPreviewPath = file.Path; await localPreviewEngine.Play();
        }
        catch (Exception ex) { localPreviewPath = null; Error("Não foi possível pré-ouvir o arquivo.", ex); }
    }
    public async Task StopLocalPreview()
    {
        localPreviewGeneration++; localPreviewPath = null;
        if (localPreviewEngine != null) await localPreviewEngine.Clear();
    }
}
