using System.IO;
using System.Reflection;
using System.Text.Json;
using Cube.Desktop.Controls;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingSessionTests
{
    public static void Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-session-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string first = Path.Combine(folder, "first.wav"), second = Path.Combine(folder, "second.wav");
            WriteConstant(first, .25f); WriteConstant(second, .125f);
            var a = new BackingStemSource(Guid.NewGuid(), first);
            var b = new BackingStemSource(Guid.NewGuid(), second, .1);
            using (var mix = new BackingSessionAudio([a, b], 0, double.PositiveInfinity, 1, 0))
            {
                var samples = new float[48000 * 2];
                Check(mix.Read(samples, 0, samples.Length) == samples.Length, "session renders a common stereo clock");
                Near(samples[4800], .25, "future stem contributes silence before its offset");
                Near(samples[24000], .375, "aligned stems sum after their offset");
                mix.SetMix([a with { Solo = true }, b]);
                var tail = new float[960]; mix.Read(tail, 0, tail.Length);
                Near(tail[^1], 0, "non-solo stem is silent when the solo stem has ended");
            }
            using (var mix = new BackingSessionAudio([a with { Muted = true, Solo = true }, b], .2, .4, 1, 0))
            {
                var samples = new float[960]; mix.Read(samples, 0, samples.Length);
                Check(samples.All(x => x == 0), "mute takes priority over solo and excludes non-solo stems");
            }
            using (var mix = new BackingSessionAudio([a with { Solo = true }, b with { Solo = true }], .2, .4, 1, 0))
            {
                var samples = new float[960]; mix.Read(samples, 0, samples.Length);
                Near(samples[^1], .375, "multiple solo tracks remain audible");
            }
            using (var mix = new BackingSessionAudio([a with { RecordedSpeed = .5, RecordedPitch = -6 }], 0, double.PositiveInfinity, 1.5, 6))
            {
                var samples = new float[960]; int count = 0, got;
                while ((got = mix.Read(samples, 0, samples.Length)) > 0) count += got;
                Check(Math.Abs(count / 2d - 16000) <= 1, "relative 3x speed and twelve semitones preserve timeline duration");
            }
            string exported = Path.Combine(folder, "mix.wav");
            BackingSessionAudio.Export([a, b], exported, .2, .4, 1, 0, 1, CancellationToken.None).GetAwaiter().GetResult();
            using (var reader = new WaveFileReader(exported))
            {
                Check(reader.WaveFormat.BitsPerSample == 16 && reader.WaveFormat.Channels == 2 && reader.WaveFormat.SampleRate == 48000, "mix export format");
                Check(Math.Abs(reader.TotalTime.TotalSeconds - .2) < .001, "mix export obeys the selected range");
            }
            byte[] originalExport = File.ReadAllBytes(exported);
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                try { BackingSessionAudio.Export([a, b], exported, 0, 1, 1, 0, 1, cancelled.Token).GetAwaiter().GetResult(); throw new Exception("Cancelled export succeeded"); }
                catch (OperationCanceledException) { }
            }
            Check(File.ReadAllBytes(exported).SequenceEqual(originalExport), "cancelled session export preserves existing destination");
            try { BackingSessionAudio.Export([a with { Path = Path.Combine(folder, "missing-source.wav") }], exported, 0, 1, 1, 0, 1, CancellationToken.None).GetAwaiter().GetResult(); throw new Exception("Missing source accepted"); }
            catch (FileNotFoundException) { }
            Check(File.ReadAllBytes(exported).SequenceEqual(originalExport) && !Directory.EnumerateFiles(folder, "*.tmp").Any(), "failed session export preserves destination and leaves no temporary files");
            var format = WaveFormat.CreateIeeeFloatWaveFormat(48000, 1);
            var envelope = new RecordingEnvelope(format);
            var block = new float[48000]; Array.Fill(block, .75f);
            var bytes = new byte[block.Length * 4]; Buffer.BlockCopy(block, 0, bytes, 0, bytes.Length);
            envelope.Write(bytes, false); envelope.Write(bytes, true);
            var peaks = envelope.Snapshot();
            Check(peaks.Length == 200 && peaks.Take(100).All(x => x == .75f) && peaks.Skip(100).All(x => x == 0), "incremental envelope includes silent packets");
            for (int i = 0; i < 350; i++) envelope.Write(bytes, true);
            peaks = envelope.Snapshot();
            Check(peaks.Length <= 32768 && peaks[0] == .75f, "long recording compaction bounds memory and retains peaks");
            Check(DropInsertion.Index(0, 2, true, 4) == 2, "move downward accounts for removal");
            Check(DropInsertion.Index(3, 0, false, 4) == 0, "move upward inserts before target");
            Check(DropInsertion.Index(0, -1, true, 4) == 3, "drop in empty list space appends");
            Check(DropInsertion.Index(2, 2, true, 4) == -1, "self-drop is a no-op");
            // Visible filtered targets retain their underlying indices (0 and 3).
            Check(DropInsertion.Index(0, 3, false, 5) == 2, "filtered reorder uses source identities");
            Persistence(folder);
        }
        finally { Directory.Delete(folder, true); }
    }
    private static void Persistence(string folder)
    {
        string data = Path.Combine(folder, "data"); Directory.CreateDirectory(data);
        string path = Path.Combine(data, "backing-playlist-v1.json");
        string missing = Path.Combine(folder, "missing.wav");
        File.WriteAllText(path, JsonSerializer.Serialize(new { Version = 1, Paths = new[] { missing } }));
        Guid id;
        using (var model = new BackingTracksViewModel(data))
        {
            Invoke(model, "LoadRecordingPlaylist");
            Check(model.Tracks.Count == 1 && model.SessionStems?.Count == 1, "v1 migration creates one session per file");
            var stem = model.SessionStems![0]; id = stem.Id;
            Check(stem.State == "Arquivo ausente", "missing source remains repairable");
            stem.IsSolo = true; stem.IsMuted = true; stem.Volume = 42; stem.Offset = .125;
            stem.RecordedSpeed = .75; stem.RecordedPitch = -2;
            stem.Name = "Lead"; stem.Pan = .4; stem.Color = "#40BFA0"; stem.IsArmed = true; stem.InputChannel = 2; stem.InputChannels = 2;
            Invoke(model, "SaveRecordingPlaylist");
        }
        Check(File.Exists(path + ".v1.bak"), "migration preserves the original playlist");
        using (var model = new BackingTracksViewModel(data))
        {
            Invoke(model, "LoadRecordingPlaylist"); var stem = model.SessionStems![0];
            Check(stem.Id == id && stem.IsSolo && stem.IsMuted && stem.Volume == 42, "track identity and mix survive reload");
            Check(stem.Name == "Lead" && stem.Pan == .4 && stem.InputChannel == 2 && stem.InputChannels == 2 && !stem.IsArmed && !stem.IsMonitoring, "v3 restores routing and identity without arming or monitoring");
            Check(stem.Offset == .125 && stem.RecordedSpeed == .75 && stem.RecordedPitch == -2, "take timing survives reload");
        }
        File.WriteAllText(path, "unreadable playlist");
        using (var model = new BackingTracksViewModel(data)) Invoke(model, "LoadRecordingPlaylist");
        Check(File.ReadAllText(path) == "unreadable playlist", "unreadable playlist is not overwritten on disposal");
        string v2Folder = Path.Combine(folder, "version2"); Directory.CreateDirectory(v2Folder);
        string v2Path = Path.Combine(v2Folder, "backing-playlist-v1.json");
        var sessionId = Guid.NewGuid(); var stemId = Guid.NewGuid();
        File.WriteAllText(v2Path, JsonSerializer.Serialize(new
        {
            Version = 2,
            Sessions = new[] { new { Id = sessionId, Source = missing, Speed = 80, Pitch = -1,
                Stems = new[] { new { Id = stemId, Source = missing, Offset = .3, RecordedSpeed = .8, RecordedPitch = -1, Muted = true, Solo = false, Volume = 0, State = "Arquivo ausente" } } } }
        }));
        using (var model = new BackingTracksViewModel(v2Folder))
        {
            Invoke(model, "LoadRecordingPlaylist"); var stem = model.SessionStems![0];
            Check(model.SelectedTrack!.SessionId == sessionId && stem.Id == stemId && stem.Volume == 0 && stem.GainDb == -61 && stem.LegacyMono && stem.Pan == 0,
                "v2 migration preserves silence, identity and legacy center amplitude");
            Check(stem.Takes.Count == 1 && stem.ActiveTake!.Offset == .3 && model.MasterDb == 0, "v2 source becomes one aligned take with unity Master");
            Invoke(model, "SaveRecordingPlaylist");
        }
        Check(File.Exists(v2Path + ".v2.bak") && JsonDocument.Parse(File.ReadAllText(v2Path)).RootElement.GetProperty("Version").GetInt32() == 4,
            "v2 backup precedes version 4 persistence");
    }
    private static void Invoke(object instance, string method) => instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, null);
    private static void WriteConstant(string path, float sample)
    {
        using var writer = new WaveFileWriter(path, WaveFormat.CreateIeeeFloatWaveFormat(48000, 2));
        var samples = Enumerable.Repeat(sample, 96000).ToArray(); writer.WriteSamples(samples, 0, samples.Length);
    }
    private static void Near(float actual, double expected, string message) => Check(Math.Abs(actual - expected) < .002, message);
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
}
