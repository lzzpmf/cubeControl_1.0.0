using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using Cube.Desktop.Services;
using Cube.Desktop.Localization;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public enum ProjectCloseChoice { Save, Discard, Cancel }
public sealed record BackingProjectRecovery(BackingProjectDocument Document, string? Manifest, DateTimeOffset Written,
    BackingRecoveryRecording[]? Recordings = null);
public sealed record BackingRecoveryRecording(Guid Stem, string Path, double Position, double Speed, double Pitch);

public sealed partial class BackingTracksViewModel
{
    private string projectDirectory = "", recentPath = "";
    private string deviceSettingsPath = "";
    private bool devicePreferencesLoaded;
    private sealed record DevicePreferences(int Version, string? Input, string? Output);
    private void SaveDevicePreferences()
    {
        if (restoring || string.IsNullOrEmpty(deviceSettingsPath) || disposed) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(deviceSettingsPath)!);
            string temporary = deviceSettingsPath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(new DevicePreferences(1, savedInputId, preferredOutputId)));
            File.Move(temporary, deviceSettingsPath, true); devicePreferencesLoaded = true;
        }
        catch (Exception ex) { Error("Não foi possível lembrar os dispositivos de áudio.", ex); }
    }
    private bool projectDirty, projectBusy, restoringProject;
    private string? projectPath;
    private readonly SemaphoreSlim projectWrite = new(1);
    private readonly DispatcherTimer recoveryTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly DispatcherTimer journalTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    public Func<Task<ProjectCloseChoice>>? ConfirmProjectChanges { get; set; }
    public Func<string, Task<bool>>? ConfirmRecovery { get; set; }
    public ObservableCollection<string> RecentProjects { get; } = [];
    public string ProjectName => SelectedTrack?.Title ?? LanguageService.T("Novo projeto");
    public bool ProjectDirty { get => projectDirty; private set { if (Set(ref projectDirty, value)) Notify(nameof(ProjectSaveLabel)); } }
    public string ProjectSaveLabel => ProjectDirty ? "Alterações não salvas" : projectPath == null ? "Projeto temporário" : "Projeto salvo";
    public string RecoveryStatus { get; private set; } = "Recuperação automática ativa";
    public ICommand NewProject { get; private set; } = null!;
    public ICommand OpenProject { get; private set; } = null!;
    public ICommand SaveProject { get; private set; } = null!;
    public ICommand SaveProjectAs { get; private set; } = null!;
    public ICommand ImportLegacySession { get; private set; } = null!;

    private void InitializeProjects(string dataDirectory)
    {
        projectDirectory = Path.Combine(dataDirectory, "Projects");
        recentPath = Path.Combine(dataDirectory, "backing-recent-projects.json");
        deviceSettingsPath = Path.Combine(dataDirectory, "backing-devices.json");
        try
        {
            if (File.Exists(deviceSettingsPath) && JsonSerializer.Deserialize<DevicePreferences>(File.ReadAllText(deviceSettingsPath)) is { Version: 1 } devices)
            { savedInputId = devices.Input; preferredOutputId = devices.Output; devicePreferencesLoaded = true; }
        }
        catch (Exception ex) { Log(ex); }
        try
        {
            if (File.Exists(recentPath)) foreach (var p in JsonSerializer.Deserialize<string[]>(File.ReadAllText(recentPath)) ?? [])
                if (Path.IsPathFullyQualified(p) && !RecentProjects.Contains(p)) RecentProjects.Add(p);
        }
        catch (Exception ex) { Log(ex); }
        NewProject = new UiCommand(() => Start(CreateProjectAsync()), () => CanEditSession);
        OpenProject = new UiCommand(() =>
        {
            var picker = new OpenFileDialog { Filter = "CubeControl project|*.cubeproject" };
            if (picker.ShowDialog() == true) Start(OpenProjectAsync(picker.FileName));
        }, () => CanEditSession);
        SaveProject = new UiCommand(() => Start(SaveProjectAsync()), () => CanEditSession && SelectedTrack != null);
        SaveProjectAs = new UiCommand(() => Start(SaveProjectAsync(true)), () => CanEditSession && SelectedTrack != null);
        ImportLegacySession = new UiCommand(() => Start(ImportLegacyAsync()), () => CanEditSession);
        recoveryTimer.Tick += async (_, _) => { recoveryTimer.Stop(); await SaveRecoveryAsync(); };
        journalTimer.Tick += async (_, _) => { if (IsRecording || recordingFinalizing) await SaveRecoveryAsync(); };
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedTrack)) Notify(nameof(ProjectName));
        };
    }

    private void MarkProjectChanged()
    {
        if (restoringProject || disposed || !playlistLoaded || SelectedTrack == null) return;
        ProjectDirty = true;
        recoveryTimer.Stop(); recoveryTimer.Start();
    }

    private BackingProjectDocument ProjectSnapshot()
    {
        var track = SelectedTrack ?? throw new InvalidOperationException("Nenhum projeto aberto.");
        var stems = track.Stems.Where(s => !s.IsProvisional).Select(s => new BackingProjectStem(s.Id,
            s.Clips.SelectMany(c => c.ActiveTake?.Layers ?? []).Select(l => l.Source).FirstOrDefault() ?? (File.Exists(s.Source) ? s.Source : ""),
            s.Name, s.Color, s.Volume, s.Pan, s.IsMuted, s.IsSolo, s.InputChannel, s.InputChannels, s.LegacyMono,
            s.SnapshotTakes().Where(t => t.Incomplete || s.Clips.SelectMany(c => c.Takes).SelectMany(take => take.Layers).Any(l => string.Equals(l.Source, t.Source, StringComparison.OrdinalIgnoreCase))).ToArray(), s.ActiveTake?.Id, s.Clips.ToArray())).ToArray();
        string candidate = !string.IsNullOrEmpty(track.AnalysisSource) ? track.AnalysisSource : File.Exists(track.Source) ? track.Source : "";
        string analysis = stems.SelectMany(s => s.Clips).SelectMany(c => c.Takes).SelectMany(t => t.Layers).Any(l => l.Source == candidate) ? candidate : "";
        return new(1, track.SessionId, track.Title, stems, TransportBpm, SessionMeter, FirstBeatOffset, Speed, Pitch,
            masterVolume, Volume / 100, track.Start, track.End, track.Sections.ToArray(), track.OriginalKey, track.Tuning,
            track.Preset, SnapEnabled, SnapDivision, CountInBars, MetronomeSound, LatencyCompensationMs, IsLooping, IsMuted, analysis, track.ManualKey);
    }

    public async Task<bool> ConfirmProjectCloseAsync()
    {
        if (IsExporting) { Status = "Aguarde a exportação ou cancele antes de fechar o projeto."; return false; }
        if (recordingPreparing || recordingFinalizing && recording == null) { Status = "Aguarde a preparação ou finalização da gravação."; return false; }
        if (recording != null) await StopRecordingAsync();
        await recordingTransport;
        if (projectBusy) return false;
        if (!ProjectDirty) return true;
        if (ConfirmProjectChanges == null) return false;
        var answer = await ConfirmProjectChanges();
        if (answer == ProjectCloseChoice.Cancel) return false;
        if (answer == ProjectCloseChoice.Save && !await SaveProjectAsync()) return false;
        if (answer == ProjectCloseChoice.Discard) await DeleteRecoveryAsync();
        return true;
    }

    public async Task CreateProjectAsync()
    {
        if (!await ConfirmProjectCloseAsync()) return;
        restoringProject = true;
        try
        {
            await engine.Clear();
            var track = new BackingTrack(Path.Combine(projectDirectory, Guid.NewGuid() + ".session"))
                { Name = LanguageService.T("Novo projeto"), Bpm = 120, ManualBpm = true, AccompanimentGain = 1 };
            track.Stems.Clear(); Tracks.Add(track); SelectedTrack = track; projectPath = null;
            AddEmptyStem.Execute(null); CountInBars = 1; ProjectDirty = false;
        }
        finally { restoringProject = false; }
    }

    public async Task<bool> SaveProjectAsync(bool saveAs = false)
    {
        if (projectBusy || !CanEditSession || SelectedTrack == null) return false;
        string? destination = projectPath;
        if (saveAs || destination == null)
        {
            var picker = new SaveFileDialog { Filter = "CubeControl project|*.cubeproject", DefaultExt = ".cubeproject", FileName = ProjectName };
            if (picker.ShowDialog() != true) return false;
            destination = picker.FileName;
        }
        projectBusy = true; Notify(nameof(CanEditSession)); Changed();
        try
        {
            var snapshot = ProjectSnapshot() with { Name = Path.GetFileNameWithoutExtension(destination) };
            await BackingProjectStore.Save(destination, snapshot, lifetime.Token);
            var saved = BackingProjectStore.Load(destination);
            RebindSavedMedia(snapshot, saved, destination);
            await RememberProject(destination);
            ProjectDirty = false; await DeleteRecoveryAsync(); Status = "Projeto salvo."; HasError = false;
            return true;
        }
        catch (Exception ex) { Error("Não foi possível salvar o projeto. Os arquivos anteriores foram preservados.", ex); return false; }
        finally { projectBusy = false; Notify(nameof(CanEditSession)); Changed(); }
    }

    public async Task OpenProjectAsync(string path)
    {
        if (!await ConfirmProjectCloseAsync()) return;
        projectBusy = true; Notify(nameof(CanEditSession)); Changed();
        try
        {
            var document = await Task.Run(() => BackingProjectStore.Load(path), lifetime.Token);
            await engine.Clear(); await StopLocalPreview(); StopYouTubePreview();
            ApplyProject(document, path); ProjectDirty = false;
            await RememberProject(path);
            var missing = BackingProjectStore.MediaPaths(document).Where(p => !File.Exists(p)).Distinct().Count();
            Status = missing > 0 ? $"{missing} áudio(s) ausente(s). Use Localizar áudio do clipe." : "Projeto aberto.";
            HasError = missing > 0;
        }
        catch (Exception ex) { Error("Não foi possível abrir o projeto. Verifique o arquivo e sua versão.", ex); }
        finally { projectBusy = false; Notify(nameof(CanEditSession)); Changed(); }
    }

    private void RebindSavedMedia(BackingProjectDocument previous, BackingProjectDocument saved, string manifest)
    {
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        void Pair(string a, string b) { if (!string.IsNullOrEmpty(a)) paths[a] = b; }
        Pair(previous.AnalysisSource, saved.AnalysisSource);
        for (int i = 0; i < previous.Stems.Length; i++)
        {
            var before = previous.Stems[i]; var after = saved.Stems[i]; Pair(before.Source, after.Source);
            for (int j = 0; j < before.Takes.Length; j++) Pair(before.Takes[j].Source, after.Takes[j].Source);
            var a = before.Clips.SelectMany(c => c.Takes).SelectMany(t => t.Layers).ToArray();
            var b = after.Clips.SelectMany(c => c.Takes).SelectMany(t => t.Layers).ToArray();
            for (int j = 0; j < a.Length; j++) Pair(a[j].Source, b[j].Source);
        }
        string Map(string path) => paths.GetValueOrDefault(path, path);
        BackingClip Clip(BackingClip clip) => clip with { Takes = clip.Takes.Select(t => t with {
            Layers = t.Layers.Select(l => l with { Source = Map(l.Source) }).ToArray() }).ToArray() };
        EditState Rebase(EditState state) => state with { Stems = state.Stems.Select(s => s with {
            Source = Map(s.Source), Clips = s.Clips.Select(Clip).ToArray(), Takes = s.Takes.Select(t => t with { Source = Map(t.Source) }).ToArray() }).ToArray() };
        restoringProject = true;
        try
        {
            foreach (var stem in SessionStems ?? [])
                if (saved.Stems.FirstOrDefault(s => s.Id == stem.Id) is { } media) stem.RebindMedia(media);
            foreach (var (from, to) in paths) if (ClipWaves.TryGetValue(from, out var wave)) ClipWaves[to] = wave;
            if (SelectedTrack != null) { SelectedTrack.Name = saved.Name; SelectedTrack.AnalysisSource = saved.AnalysisSource; }
            if (History is { } history)
            {
                history.Undo = new Stack<EditState>(history.Undo.Reverse().Select(Rebase));
                history.Redo = new Stack<EditState>(history.Redo.Reverse().Select(Rebase));
                history.Current = SnapshotEdit();
            }
            clipClipboard = clipClipboard.Select(c => (c.Track, Clip(c.Clip))).ToArray();
            metronomeSound = saved.MetronomeSound; Notify(nameof(MetronomeSound));
            if (Path.IsPathFullyQualified(metronomeSound) && !CustomMetronomeFiles.Contains(metronomeSound)) CustomMetronomeFiles.Add(metronomeSound);
            projectPath = manifest; Notify(nameof(ProjectName)); Notify(nameof(ProjectSaveLabel)); NotifyClips();
            Start(LoadSessionWaves());
        }
        finally { restoringProject = false; }
    }

    private void ApplyProject(BackingProjectDocument doc, string? manifest)
    {
        restoringProject = true;
        try
        {
            bool wasBusy = projectBusy; projectBusy = false;
            try
            {
                var track = new BackingTrack(Path.Combine(projectDirectory, doc.Id + ".session"))
                { SessionId = doc.Id, Name = doc.Name, Bpm = doc.Bpm, ManualBpm = true, Meter = doc.Meter,
                    Speed = doc.Speed, Pitch = doc.Pitch, MasterGain = doc.Master, AccompanimentGain = doc.Bus,
                    Start = doc.RangeStart, End = doc.RangeEnd, OriginalKey = doc.Key, ManualKey = doc.KeyIsManual,
                    Tuning = doc.Tuning, Preset = doc.Preset, AnalysisSource = doc.AnalysisSource };
                track.Stems.Clear();
                foreach (var x in doc.Stems)
                {
                    var stem = new BackingStem { Id = x.Id, Source = string.IsNullOrEmpty(x.Source) ? Path.Combine(projectDirectory, x.Id + ".wav") : x.Source,
                        Name = x.Name, Color = x.Color, Volume = x.Volume, Pan = x.Pan, IsMuted = x.Muted, IsSolo = x.Solo,
                        InputChannel = x.Input, InputChannels = x.Channels, LegacyMono = x.LegacyMono, ClipsInitialized = true };
                    foreach (var take in x.Takes) stem.Takes.Add(take);
                    stem.ActiveTake = stem.Takes.FirstOrDefault(t => t.Id == x.ActiveTake);
                    stem.Clips.AddRange(x.Clips); track.Stems.Add(stem);
                }
                foreach (var marker in doc.Sections ?? []) track.Sections.Add(marker);
                metronomeOffsets[track.Source] = doc.FirstBeat;
                if (SelectedTrack is { } old) foreach (var stem in old.Stems) DetachStem(stem);
                Tracks.Add(track); SelectedTrack = track;
                SnapEnabled = doc.Snap; SnapDivision = doc.SnapDivision; CountInBars = doc.CountIn;
                latencyCompensationMs = doc.LatencyMs; Notify(nameof(LatencyCompensationMs));
                SelectMetronomeSound(doc.MetronomeSound);
                projectPath = manifest; looping = doc.Loop && HasRange; RangeChanged(false);
                muted = doc.Muted; Notify(nameof(IsMuted)); Notify(nameof(MuteIcon)); Notify(nameof(MuteLabel));
                Notify(nameof(ProjectName)); RefreshSessionDuration();
            }
            finally { projectBusy = wasBusy; Notify(nameof(CanEditSession)); }
        }
        finally { restoringProject = false; }
    }

    private async Task ImportLegacyAsync()
    {
        if (Tracks.Count == 0) { Status = "Nenhuma sessão anterior disponível."; return; }
        if (!await ConfirmProjectCloseAsync()) return;
        var picker = new OpenFileDialog { Filter = "Playlist CubeControl|*.json", FileName = playlistPath };
        if (picker.ShowDialog() != true) return;
        string original = playlistPath;
        bool previousFailure = playlistLoadFailed;
        try
        {
            playlistLoadFailed = false;
            playlistPath = picker.FileName; playlistLoaded = false;
            LoadRecordingPlaylist();
            if (playlistLoadFailed) throw new InvalidDataException();
            var imported = Tracks.LastOrDefault(); if (imported != null) SelectedTrack = imported;
            projectPath = null; MarkProjectChanged(); Status = "Sessão importada. Use Salvar como para criar o projeto.";
        }
        finally { playlistPath = original; playlistLoaded = true; playlistLoadFailed = previousFailure; }
    }
    public async Task ImportLegacyTrackAsync(BackingTrack track)
    {
        if (!Tracks.Contains(track) || !await ConfirmProjectCloseAsync()) return;
        SelectedTrack = track; projectPath = null; MarkProjectChanged();
        Status = "Sessão importada. Use Salvar como para criar o projeto.";
    }

    private async Task RememberProject(string path)
    {
        if (RecentProjects.Contains(path)) RecentProjects.Remove(path);
        RecentProjects.Insert(0, path); while (RecentProjects.Count > 10) RecentProjects.RemoveAt(10);
        try { await BackingProjectStore.WriteAtomic(recentPath, RecentProjects.ToArray(), lifetime.Token); }
        catch (Exception ex) { Log(ex); }
    }

    private async Task<string> OwnProjectAudioAsync(string source)
    {
        if (projectPath == null) return source;
        string folder = Path.Combine(Path.GetDirectoryName(projectPath)!, "Media");
        string full = Path.GetFullPath(source);
        if (full.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return full;
        Directory.CreateDirectory(folder);
        string destination = Path.Combine(folder, Guid.NewGuid().ToString("N") + Path.GetExtension(source));
        string partial = destination + ".partial";
        try
        {
            await using (var input = File.OpenRead(source))
            await using (var output = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                await input.CopyToAsync(output, lifetime.Token);
            File.Move(partial, destination);
        }
        finally { if (File.Exists(partial)) File.Delete(partial); }
        return destination;
    }

    private string RecoveryPath => Path.Combine(projectDirectory, "Recovery", (SelectedTrack?.SessionId.ToString("N") ?? "scratch") + ".json");
    private async Task SaveRecoveryAsync()
    {
        if (disposed || restoringProject || projectBusy || SelectedTrack == null || !ProjectDirty && !IsRecording) return;
        await projectWrite.WaitAsync();
        try
        {
            var journal = recordingTargets.Select(t => new BackingRecoveryRecording(t.Stem.Id, t.Path,
                recordingAnchor.Position, Speed / 100, Pitch)).ToArray();
            await BackingProjectStore.WriteAtomic(RecoveryPath, new BackingProjectRecovery(ProjectSnapshot(), projectPath, DateTimeOffset.UtcNow, journal));
            RecoveryStatus = "Recuperação automática atualizada"; Notify(nameof(RecoveryStatus));
        }
        catch (Exception ex) { RecoveryStatus = "Falha na recuperação automática"; Notify(nameof(RecoveryStatus)); Error(RecoveryStatus, ex); }
        finally { projectWrite.Release(); }
    }

    private async Task DeleteRecoveryAsync()
    {
        recoveryTimer.Stop();
        await projectWrite.WaitAsync();
        try { if (File.Exists(RecoveryPath)) File.Delete(RecoveryPath); } catch (Exception ex) { Log(ex); }
        finally { projectWrite.Release(); }
    }

    private void RestoreLastProject()
    {
        string? last = RecentProjects.FirstOrDefault(File.Exists);
        if (last == null) return;
        try { ApplyProject(BackingProjectStore.Load(last), last); ProjectDirty = false; }
        catch (Exception ex) { Error("Não foi possível restaurar o último projeto.", ex); }
    }

    private async Task RestoreProjectRecoveryAsync()
    {
        string folder = Path.Combine(projectDirectory, "Recovery");
        if (!Directory.Exists(folder) || ConfirmRecovery == null) return;
        foreach (string file in Directory.GetFiles(folder, "*.json").OrderByDescending(File.GetLastWriteTimeUtc))
        {
            try
            {
                var recovery = JsonSerializer.Deserialize<BackingProjectRecovery>(await File.ReadAllTextAsync(file));
                if (recovery == null) continue;
                BackingProjectStore.Validate(recovery.Document);
                if (!await ConfirmRecovery(recovery.Document.Name)) { File.Delete(file); continue; }
                ApplyProject(recovery.Document, recovery.Manifest); ProjectDirty = true;
                foreach (var entry in recovery.Recordings ?? [])
                {
                    if (!File.Exists(entry.Path) || SessionStems?.FirstOrDefault(s => s.Id == entry.Stem) is not { } stem) continue;
                    stem.Takes.Add(new(Guid.NewGuid(), entry.Path, entry.Position, entry.Speed, entry.Pitch, Incomplete: true, LegacyMono: false));
                }
                SelectedStem = SessionStems?.FirstOrDefault(s => s.Takes.Any(t => t.Incomplete)) ?? SelectedStem;
                RefreshClipPresentation();
                Status = "Projeto recuperado. Revise as tomadas interrompidas antes de salvar.";
                break;
            }
            catch (Exception ex) { Log(ex); Status = "Uma recuperação inválida foi preservada para diagnóstico."; }
        }
    }
}
