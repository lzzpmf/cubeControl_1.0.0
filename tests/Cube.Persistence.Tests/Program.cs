using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Threading;
using Cube.Desktop.ViewModels;

internal static class Program
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "CubePersistenceTests-" + Guid.NewGuid());
    private static void Call(BackingTracksViewModel model, string name, params object[] args) =>
        typeof(BackingTracksViewModel).GetMethod(name, Private)!.Invoke(model, args);
    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }
    private static string Playlist(string directory) => Path.Combine(directory, "backing-playlist-v1.json");
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start(); Dispatcher.PushFrame(frame);
    }
    private static JsonObject Read(string directory) => JsonNode.Parse(File.ReadAllText(Playlist(directory)))!.AsObject();
    [STAThread]
    private static void Main()
    {
        Directory.CreateDirectory(Root);
        // No device initialization, playback, recording or real user data is used.
        string current = Path.Combine(Root, "current");
        using (var model = new BackingTracksViewModel(current))
        {
            Call(model, "LoadRecordingPlaylist");
            var track = new BackingTrack(Path.Combine(Root, "missing.wav")) { Length = 120 };
            track.Stems.Clear(); model.Tracks.Add(track); model.SelectedTrack = track;
            Call(model, "UpdateTrackMetadata", track, 123d, "Dm", "Drop D", 2);
            model.OriginalKey = "Am"; model.LinkedPreset = 3;
            Call(model, "AddSectionAt", track, 12d, "Verso");
            track.Sections.Add(new(34, "Refrão"));
            track.Start = 10; track.End = 45;
            Call(model, "RangeChanged", true);
            Pump();
            var saved = Read(current);
            Check(saved["Version"]!.GetValue<int>() == 5, "Expected v5 autosave");
            var session = saved["Sessions"]![0]!;
            Check(session["OriginalKey"]!.GetValue<string>() == "Am", "Key was not autosaved");
            Check(session["Preset"]!.GetValue<int>() == 3, "Preset was not autosaved");
            Check(session["Sections"]!.AsArray().Count == 2, "Sections were not autosaved");
        }
        using (var model = new BackingTracksViewModel(current))
        {
            Call(model, "LoadRecordingPlaylist");
            Check(model.Tracks.Count == 1, "Session did not reload");
            var track = model.Tracks[0];
            Check(track.OriginalKey == "Am" && track.Tuning == "Drop D" && track.Preset == 3, "Metadata did not round-trip");
            Check((bool)typeof(BackingTrack).GetField("ManualKey", Private)!.GetValue(track)!, "Manual key flag lost");
            model.OriginalKey = "Em"; Pump();
            Check(Read(current)["Sessions"]![0]!["OriginalKey"]!.GetValue<string>() == "Em", "Key-only edit did not autosave");
            model.LinkedPreset = 1; Pump();
            Check(Read(current)["Sessions"]![0]!["Preset"]!.GetValue<int>() == 1, "Preset-only edit did not autosave");
            Call(model, "UpdateTrackMetadata", track, 124d, "Em", "Standard", 1); Pump();
            Check(Read(current)["Sessions"]![0]!["Tuning"]!.GetValue<string>() == "Standard", "Metadata-only edit did not autosave");
            Check(track.Start == 10 && track.End == 45, "A/B did not round-trip");
            Check(track.Sections.SequenceEqual(new[] { new BackingSection(12, "Verso"), new BackingSection(34, "Refrão") }), "Sections did not round-trip");
            track.Sections.RemoveAt(0); Pump();
            Check(Read(current)["Sessions"]![0]!["Sections"]!.AsArray().Count == 1, "Section removal was not autosaved");
            model.ClearStart.Execute(null); model.ClearEnd.Execute(null); Pump();
            Check(Read(current)["Sessions"]![0]!["Start"] == null && Read(current)["Sessions"]![0]!["End"] == null, "Cleared A/B not saved");
        }
        for (int version = 1; version <= 4; version++)
        {
            string directory = Path.Combine(Root, "v" + version); Directory.CreateDirectory(directory);
            object legacy = version == 1
                ? new { Version = version, Paths = new[] { Path.Combine(Root, "legacy.wav") } }
                : new { Version = version, Sessions = new[] { new { Id = Guid.NewGuid(), Source = Path.Combine(Root, "legacy.wav"), Stems = Array.Empty<object>() } } };
            string original = JsonSerializer.Serialize(legacy); File.WriteAllText(Playlist(directory), original);
            using var model = new BackingTracksViewModel(directory);
            Call(model, "LoadRecordingPlaylist");
            Check(model.Tracks.Count == 1, $"Version {version} did not load");
            Check(model.Tracks[0].Start == null && model.Tracks[0].Sections.Count == 0, "Invalid legacy defaults");
            Call(model, "SaveRecordingPlaylist");
            Check(Read(directory)["Version"]!.GetValue<int>() == 5, "Migration did not write v5");
            Check(File.ReadAllText(Playlist(directory) + $".v{version}.bak") == original, "Legacy backup changed");
        }
        string clipsDirectory = Path.Combine(Root, "clips-v4"); Directory.CreateDirectory(clipsDirectory);
        string clipSource = Path.Combine(Root, "clip.wav");
        var take = new BackingClipTake(Guid.NewGuid(), "Take", [new(Guid.NewGuid(), clipSource, 0, 2, 20)]);
        var clip = new BackingClip(Guid.NewGuid(), "Edited clip", 5, 20, [take], take.Id, 1, 2);
        File.WriteAllText(Playlist(clipsDirectory), JsonSerializer.Serialize(new { Version = 4, Sessions = new[] {
            new { Id = Guid.NewGuid(), Source = clipSource, Stems = new[] {
                new { Id = Guid.NewGuid(), Source = clipSource, Offset = 0, RecordedSpeed = 1, RecordedPitch = 0,
                    Volume = 1, State = "Pronta", Clips = new[] { clip } }
            } }
        } }));
        for (int pass = 0; pass < 2; pass++)
        {
            using var model = new BackingTracksViewModel(clipsDirectory);
            Call(model, "LoadRecordingPlaylist");
            Check(model.Tracks.Count == 1 && model.Tracks[0].Stems.Count == 1, "Clip session did not load");
            var restored = model.Tracks[0].Stems[0].Clips.Single();
            Check(JsonSerializer.Serialize(restored) == JsonSerializer.Serialize(clip), "Clip/take/layer changed during migration");
            Call(model, "SaveRecordingPlaylist");
        }
        foreach (bool invalidRange in new[] { true, false })
        {
            string directory = Path.Combine(Root, invalidRange ? "bad-range" : "bad-section"); Directory.CreateDirectory(directory);
            var data = Read(current);
            if (invalidRange) data["Sessions"]![0]!["Start"] = -1;
            else data["Sessions"]![0]!["Sections"]![0]!["Position"] = -1;
            string original = data.ToJsonString(); File.WriteAllText(Playlist(directory), original);
            using var model = new BackingTracksViewModel(directory);
            Call(model, "LoadRecordingPlaylist");
            Check(model.Tracks.Count == 0, "Invalid data accepted");
            try { Call(model, "SaveRecordingPlaylist"); throw new Exception("Invalid playlist overwritten"); }
            catch (TargetInvocationException ex) when (ex.InnerException is IOException) { }
            Check(File.ReadAllText(Playlist(directory)) == original, "Invalid source changed");
        }
        Console.WriteLine("PASS: metadata/A-B/sections round-trip, independent autosave edits, v1-v4 migration/backups, clips/takes preservation, invalid data preservation.");
        Console.WriteLine("Isolated test data: " + Root);
    }
}
