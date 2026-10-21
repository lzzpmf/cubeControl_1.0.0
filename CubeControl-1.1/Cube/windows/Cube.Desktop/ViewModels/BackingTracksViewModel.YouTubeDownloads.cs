using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Cube.Desktop.Localization;
#if WINUI
using Cube.WinUI.Platform;
#else
using Microsoft.Win32;
#endif

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    private sealed record DownloadSettings(int Version, string Folder, Dictionary<string, string>? Completed = null, bool PreviewOnHover = false);
    private readonly Dictionary<string, string> completedYouTubeDownloads = new(StringComparer.Ordinal);
    public bool IsYouTubeDownloaded(string id) => completedYouTubeDownloads.TryGetValue(id, out string? path) && File.Exists(path);
    private void RememberYouTubeDownload(string id, string path)
    {
        completedYouTubeDownloads[id] = path;
        Notify("YouTubeCompletedDownloads");
        try { SaveYouTubeDownloadSettings(YouTubeDownloadFolder); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Log(ex); }
    }
    private void SaveYouTubeDownloadSettings(string folder)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(downloadSettingsPath)!);
        string temporary = downloadSettingsPath + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(new DownloadSettings(1, folder, completedYouTubeDownloads, PreviewOnHover)));
            File.Move(temporary, downloadSettingsPath, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private string downloadSettingsPath = "", downloadFolder = "";
    private bool choosingDownloadFolder;
    public string YouTubeDownloadFolder { get => downloadFolder; private set { Set(ref downloadFolder, value); Notify(nameof(YouTubeDownloadFolderLabel)); } }
    public string YouTubeDownloadFolderLabel => downloadFolder.Length > 0 ? downloadFolder : "Escolha uma pasta no primeiro download";
    public ICommand ChangeYouTubeDownloadFolder => new UiCommand(() => ChooseDownloadFolder(), () => !YouTubeDownloading && !choosingDownloadFolder);
    internal Func<string, string?> SelectYouTubeDownloadFolder { get; set; } = current =>
    {
        var dialog = new OpenFolderDialog { Title = LanguageService.T("Pasta de downloads das faixas"), Multiselect = false };
        if (Directory.Exists(current)) dialog.InitialDirectory = current;
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    };
    private void InitializeYouTubeDownloads(string dataDirectory)
    {
        YouTubeResults.CollectionChanged += (_, _) => RefreshYouTubePresentation();
        downloadSettingsPath = Path.Combine(dataDirectory, "youtube-downloads-v1.json");
        try
        {
            var settings = JsonSerializer.Deserialize<DownloadSettings>(File.ReadAllText(downloadSettingsPath));
            if (settings?.Version == 1) previewOnHover = settings.PreviewOnHover;
            if (settings?.Version == 1 && settings.Completed != null)
                foreach (var entry in settings.Completed) completedYouTubeDownloads[entry.Key] = entry.Value;
            if (settings?.Version == 1 && !string.IsNullOrWhiteSpace(settings.Folder) && Path.IsPathFullyQualified(settings.Folder))
                YouTubeDownloadFolder = settings.Folder;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException) { Log(ex); }
    }
    private bool ChooseDownloadFolder()
    {
        if (YouTubeDownloading || choosingDownloadFolder || disposed) return false;
        YouTubeFolderStatus = "";
        StopYouTubePreview(); choosingDownloadFolder = true; CommandManager.InvalidateRequerySuggested();
        try
        {
            string? selected = SelectYouTubeDownloadFolder(YouTubeDownloadFolder);
            if (selected == null) return false;
            selected = Path.GetFullPath(selected);
            VerifyDownloadFolder(selected);
            SaveYouTubeDownloadSettings(selected);
            YouTubeDownloadFolder = selected;
            if (YouTubeDownloadMessageId == null) YouTubeDownloadStatus = "";
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            YouTubeFolderStatus = "Não foi possível usar ou lembrar esta pasta. Escolha outra pasta.";
            if (YouTubeDownloadMessageId == null) YouTubeDownloadStatus = YouTubeFolderStatus;
            Log(ex); return false;
        }
        finally { choosingDownloadFolder = false; CommandManager.InvalidateRequerySuggested(); }
    }
    private static void VerifyDownloadFolder(string folder)
    {
        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException();
        // Verify write access without leaving a file behind, before starting network work.
        using var probe = new FileStream(Path.Combine(folder, ".cube-write-" + Guid.NewGuid().ToString("N")), FileMode.CreateNew,
            FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose);
    }
    internal static string AvailableDownloadPath(string folder, string title, string id)
    {
        string name = string.Concat(title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim().TrimEnd('.');
        if (name.Length > 120) name = name[..120].TrimEnd(' ', '.');
        if (name.Length == 0) name = id;
        string stem = name.Split('.')[0];
        if (System.Text.RegularExpressions.Regex.IsMatch(stem, "^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) name = "_" + name;
        string path = Path.Combine(folder, name + ".mp3");
        for (int suffix = 2; File.Exists(path) || Directory.Exists(path); suffix++)
            path = Path.Combine(folder, $"{name} ({suffix}).mp3");
        return path;
    }
}
