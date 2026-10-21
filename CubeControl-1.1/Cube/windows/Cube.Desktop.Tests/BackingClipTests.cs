using System.IO;
using System.Reflection;
using System.Text.Json;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingClipTests
{
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private static void Invoke(object target, string method) => target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(target, null);
    private static BackingClip Clip(string source, double position, double duration, double sourceStart = 0)
    {
        var take = new BackingClipTake(Guid.NewGuid(), "Take", [new(Guid.NewGuid(), source, 0, sourceStart, duration, LegacyMono: true)]);
        return new(Guid.NewGuid(), "Audio", position, duration, [take], take.Id);
    }
    private static float[] Render(BackingStem stem, double start, double end, double speed = 1)
    {
        using var audio = new BackingSessionAudio(stem.Clips.SelectMany(c => c.Sources(stem)), start, end, speed, 0, offline: true);
        var result = new List<float>(); var buffer = new float[2048]; int read;
        while ((read = audio.Read(buffer, 0, buffer.Length)) > 0) result.AddRange(buffer.Take(read));
        return result.ToArray();
    }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-clips-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "source.wav");
        using (var writer = new WaveFileWriter(file, WaveFormat.CreateIeeeFloatWaveFormat(48000, 2))) writer.WriteSamples(Enumerable.Repeat(.2f, 48000 * 2 * 4).ToArray(), 0, 48000 * 2 * 4);
        byte[] original = File.ReadAllBytes(file);
        var stem = new BackingStem { Source = file, LegacyMono = true, ClipsInitialized = true };
        var clip = Clip(file, 0, 4) with { FadeIn = 1, FadeOut = 1 };
        stem.Clips.Add(clip);
        var full = Render(stem, 0, 4);
        Check(Math.Abs(full[48000] - .1) < .001, "linear fade uses the session frame clock");
        var seek = Render(stem, .5, 1);
        Check(Math.Abs(seek[4800] - full[48000 + 4800]) < .001, "seek preserves envelope position");
        stem.Clips.Clear(); stem.Clips.AddRange([clip.Slice(0, .5), clip.Slice(.5, 4)]);
        var split = Render(stem, 0, 4);
        Check(split.Length == full.Length && split.Zip(full).All(pair => Math.Abs(pair.First - pair.Second) < .001), "split preserves fades sample for sample");
        var trimmed = Clip(file, 0, 4).Trim(1, 3); var restored = trimmed.Trim(0, 4);
        Check(restored.Duration == 4 && restored.ActiveTake!.Layers[0].Position == 0, "non-destructive trim can restore both edges");
        var a = Clip(file, 0, 2) with { FadeOut = 1, EqualPowerOut = true };
        var b = Clip(file, 1, 2) with { FadeIn = 1, EqualPowerIn = true };
        stem.Clips.Clear(); stem.Clips.AddRange([a, b]); var cross = Render(stem, 0, 3);
        Check(Math.Abs(cross[144000] - .2 * Math.Sqrt(2)) < .002, "equal-power crossfade sums both clips before track gain");
        var overdub = BackingClipEdit.Overdub([Clip(file, 0, 4), Clip(file, 1, 2)], Clip(file, 1.5, 1));
        Check(overdub.Any(c => c.Position == 0 && c.End == 1.5) && overdub.Any(c => c.Position == 2.5 && c.End == 4), "overdub preserves both outer sections");
        var recorded = overdub.Single(c => c.Position == 1.5 && c.Duration == 1);
        Check(recorded.Takes.Length == 2 && recorded.Takes[0].Layers.Length == 2, "overdub preserves overlapping audio as a composite alternative");
        stem.Clips.Clear(); stem.Clips.Add(recorded with { ActiveTakeId = recorded.Takes[0].Id });
        Check(Math.Abs(Render(stem, 1.5, 2.5)[4800] - .4) < .002, "composite take restores the previous audible sum");
        Check(original.SequenceEqual(File.ReadAllBytes(file)), "editing never changes source bytes");
        using (var model = new BackingTracksViewModel(folder))
        {
            Invoke(model, "LoadRecordingPlaylist");
            var track = new BackingTrack(file); model.Tracks.Add(track); model.SelectedTrack = track;
            await model.AddSessionFiles([file]); await Task.Delay(100);
            var edited = model.SessionStems![0]; var initial = edited.Clips[0];
            model.SelectClip(edited, initial, false); model.Position = 1; model.SplitClips();
            Check(edited.Clips.Count == 2, "split creates two clips");
            model.UndoEdit(); Check(model.SessionStems![0].Clips.Count == 1, "undo restores the prior arrangement");
            model.UndoEdit(true); Check(model.SessionStems![0].Clips.Count == 2, "redo restores the split");
            edited = model.SessionStems[0]; model.SelectClip(edited, edited.Clips[0], false); model.CopyClips(); model.Position = 2; model.PasteClips();
            Check(edited.Clips.Count == 3 && edited.Clips.Any(c => c.Position == 2), "paste uses the cursor without overwriting audio");
            model.AddEmptyStem.Execute(null); var emptySolo = model.SelectedStem!; emptySolo.IsSolo = true;
            var sources = (BackingStemSource[])model.GetType().GetMethod("SessionSources", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(model, null)!;
            using (var solo = new BackingSessionAudio(sources, 0, 1, 1, 0, offline: true)) { var silent = new float[4800]; solo.Read(silent, 0, silent.Length); Check(silent.All(x => x == 0), "solo on an empty track silences other tracks"); }
            emptySolo.IsSolo = false;
            var movedId = edited.Clips[0].Id; int originalCount = edited.Clips.Count;
            model.SelectClip(edited, edited.Clips[0], false); var destination = model.SessionStems[1];
            model.BeginClipGesture(); model.MoveClips(.25, destination); model.EndClipGesture();
            Check(edited.Clips.Count == originalCount - 1 && destination.Clips.Any(c => c.Id == movedId && c.Position == .25), "moving a clip preserves identity across tracks");
            model.UndoEdit(); edited = model.SessionStems[0];
            Check(edited.Clips.Count == originalCount && edited.Clips.Any(c => c.Id == movedId && c.Position == 0), "undo restores clip position and routing together");
            var beforeGesture = edited.Clips.ToArray(); model.BeginClipGesture(); model.ReplaceClip(edited, edited.Clips[0] with { FadeIn = .1 }); model.CancelClipGesture();
            Check(edited.Clips.SequenceEqual(beforeGesture), "cancelled trim or fade restores the original clips");
            model.Bpm = 120; model.FirstBeatOffset = .1; model.SessionMeter = "6/8"; model.SnapDivision = "Bar";
            Check(Math.Abs(model.SnapTime(1.7) - 1.6) < .0001, "6/8 snap uses three quarter notes and first-beat offset");
            var positions = edited.Clips.Select(c => c.Position).ToArray(); model.Bpm = 90;
            Check(positions.SequenceEqual(edited.Clips.Select(c => c.Position)), "BPM changes do not move or stretch audio");
            Invoke(model, "SaveRecordingPlaylist");
        }
        using (var saved = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "backing-playlist-v1.json")))) Check(saved.RootElement.GetProperty("Version").GetInt32() == 4, "clip sessions save as version 4");
        using (var reloaded = new BackingTracksViewModel(folder)) { Invoke(reloaded, "LoadRecordingPlaylist"); Check(reloaded.SessionStems![0].Clips.Count == 3 && reloaded.SessionMeter == "6/8", "clips and musical grid survive reopening"); }
        string future = Path.Combine(folder, "future"); Directory.CreateDirectory(future);
        string futurePath = Path.Combine(future, "backing-playlist-v1.json"); File.WriteAllText(futurePath, "{\"Version\":99}");
        using (var model = new BackingTracksViewModel(future)) Invoke(model, "LoadRecordingPlaylist");
        Check(File.ReadAllText(futurePath) == "{\"Version\":99}", "future versions remain untouched on close");
        Console.WriteLine("CLIP FIXTURES " + folder);
    }
}
