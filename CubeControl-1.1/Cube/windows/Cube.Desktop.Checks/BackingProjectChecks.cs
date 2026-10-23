using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingProjectChecks
{
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS: " + message); }
    public static void Run()
    {
        var app = new Cube.Desktop.App(); app.InitializeComponent();
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        var task = Tests(); var frame = new DispatcherFrame();
        task.ContinueWith(_ => app.Dispatcher.BeginInvoke(new Action(() => frame.Continue = false)));
        Dispatcher.PushFrame(frame); task.GetAwaiter().GetResult(); app.Shutdown();
    }
    private static async Task Tests()
    {
        string root = Path.Combine(Path.GetTempPath(), "CubeControl-Backing-Checks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string source = Path.Combine(root, "source.wav");
        using (var writer = new WaveFileWriter(source, new WaveFormat(48000, 16, 1)))
            writer.WriteSamples(Enumerable.Repeat(.25f, 48000).ToArray(), 0, 48000);
        var take = new BackingClipTake(Guid.NewGuid(), "Take 1", [new(Guid.NewGuid(), source, 0, 0, 1)]);
        var alternate = take with { Id = Guid.NewGuid(), Name = "Take 2" };
        var clip = new BackingClip(Guid.NewGuid(), "Guitar", 0, 1, [take, alternate], take.Id, .1, .2);
        var stem = new BackingProjectStem(Guid.NewGuid(), source, "Guitar", "#6495ED", 100, .25, false, false, 0, 1, false,
            [new(take.Id, source, 0)], take.Id, [clip]);
        var document = new BackingProjectDocument(1, Guid.NewGuid(), "Project", [stem], 90, "4/4", RangeStart: .1,
            RangeEnd: .8, Sections: [new(.2, "Solo")], Key: "Am", Tuning: "Standard", Preset: 2);
        string manifest = Path.Combine(root, "project", "practice.cubeproject");
        await BackingProjectStore.Save(manifest, document);
        Assert(File.Exists(source), "saving preserves source audio");
        string json = File.ReadAllText(manifest);
        Assert(!json.Contains(source.Replace("\\", "\\\\")), "manifest does not contain external source paths");
        var loaded = BackingProjectStore.Load(manifest);
        Assert(loaded.Stems[0].Clips[0].Takes.Length == 2 && loaded.Stems[0].Clips[0].FadeOut == .2, "takes and fades survive project roundtrip");
        Assert(loaded.RangeEnd == .8 && loaded.Key == "Am" && loaded.Sections?.Length == 1, "range, musical metadata and markers survive roundtrip");
        Assert(File.Exists(loaded.Stems[0].Source), "saved media copy exists");
        string previous = File.ReadAllText(manifest);
        try { await BackingProjectStore.Save(manifest, document with { Stems = [stem with { Source = Path.Combine(root, "missing.wav") }] }); throw new Exception("missing media was accepted"); }
        catch (FileNotFoundException) { }
        Assert(File.ReadAllText(manifest) == previous, "failed save preserves existing manifest");
        string moved = Path.Combine(root, "moved"); Directory.Move(Path.GetDirectoryName(manifest)!, moved);
        manifest = Path.Combine(moved, "practice.cubeproject"); loaded = BackingProjectStore.Load(manifest);
        Assert(File.Exists(loaded.Stems[0].Clips[0].Takes[0].Layers[0].Source), "moving project folder preserves media references");
        var malicious = JsonSerializer.Deserialize<BackingProjectDocument>(File.ReadAllText(manifest))!;
        await BackingProjectStore.WriteAtomic(Path.Combine(moved, "invalid.cubeproject"), malicious with { Stems = [malicious.Stems[0] with { Source = "../outside.wav" }] });
        try { BackingProjectStore.Load(Path.Combine(moved, "invalid.cubeproject")); throw new Exception("traversal was accepted"); }
        catch (InvalidDataException) { Console.WriteLine("PASS: traversal outside Media rejected"); }
        using (var vm = new BackingTracksViewModel(Path.Combine(root, "vm")))
        {
            vm.AddPaths([source]); await vm.AddSessionFiles([]);
            var selectedStem = vm.SessionStems![0];
            vm.SelectClip(selectedStem, selectedStem.Clips[0], false);
            double zoom = vm.Zoom = 2;
            typeof(BackingTracksViewModel).GetMethod("RefreshPractice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(vm, null);
            Assert(vm.Zoom == zoom, "metadata refresh preserves timeline zoom");
            vm.ClipFadeIn = .25; Assert(vm.SelectedClip!.FadeIn == .25, "fade editor changes selected clip");
            vm.UndoEdit(); Assert(vm.SessionStems[0].Clips[0].FadeIn == 0, "undo restores fade");
            vm.UndoEdit(true); Assert(vm.SessionStems[0].Clips[0].FadeIn == .25, "redo restores fade edit");
            vm.SelectedStem.Takes.Add(new(Guid.NewGuid(), source, 0, Incomplete: true));
            vm.DiscardInterruptedTake.Execute(null); Assert(!vm.HasInterruptedTake && File.Exists(source), "ignoring interrupted take preserves audio bytes");
            vm.UndoEdit(); Assert(vm.HasInterruptedTake, "ignoring interrupted take can be undone");
            vm.UndoEdit(true);
            var busyField = typeof(BackingTracksViewModel).GetField("projectBusy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var notify = typeof(Observable).GetMethod("Notify", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            busyField.SetValue(vm, true); notify.Invoke(vm, [nameof(BackingTracksViewModel.CanEditSession)]);
            double gain = vm.SelectedStem.Volume, master = vm.MasterDb;
            vm.SelectedStem.Volume = 50; vm.MasterDb = -12;
            Assert(vm.SelectedStem.Volume == gain && vm.MasterDb == master && !vm.CanAdjustMix, "project IO locks mixing to preserve save snapshot");
            busyField.SetValue(vm, false); notify.Invoke(vm, [nameof(BackingTracksViewModel.CanEditSession)]);
            typeof(BackingTracksViewModel).GetField("playlistLoaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(vm, true);
            string vmManifest = Path.Combine(root, "vm-project", "practice.cubeproject");
            typeof(BackingTracksViewModel).GetField("projectPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(vm, vmManifest);
            vm.SelectClip(vm.SessionStems[0], vm.SessionStems[0].Clips[0], false); vm.Position = .4; vm.Zoom = 2;
            Guid selection = vm.SelectedClip!.Id;
            Assert(await vm.SaveProjectAsync(), "view model saves and rebinds portable project");
            Assert(!vm.ProjectDirty && vm.SelectedClip?.Id == selection && vm.ProjectName == "practice" && vm.Position == .4 && vm.Zoom == 2, "save preserves selection, position and zoom while clearing dirty state");
            vm.Speed = 80; Assert(vm.ProjectDirty, "speed edit marks project dirty");
            vm.ConfirmProjectChanges = () => Task.FromResult(ProjectCloseChoice.Cancel);
            await vm.OpenProjectAsync(vmManifest); Assert(vm.Speed == 80, "cancel leaves current unsaved project intact");
            vm.ConfirmProjectChanges = () => Task.FromResult(ProjectCloseChoice.Discard);
            await vm.OpenProjectAsync(vmManifest); Assert(vm.Speed == 100 && !vm.ProjectDirty, "discard and reopen restores saved project");
            Assert(!File.Exists(Path.Combine(root, "vm", "backing-playlist-v1.json")), "portable editing does not overwrite legacy playlist");
        }
        string recoveryRoot = Path.Combine(root, "recovery-vm");
        await BackingProjectStore.WriteAtomic(Path.Combine(recoveryRoot, "Projects", "Recovery", document.Id.ToString("N") + ".json"),
            new BackingProjectRecovery(document, null, DateTimeOffset.UtcNow, [new(stem.Id, source, 0, 1, 0)]));
        using (var vm = new BackingTracksViewModel(recoveryRoot))
        {
            vm.ConfirmRecovery = _ => Task.FromResult(true);
            await (Task)typeof(BackingTracksViewModel).GetMethod("RestoreProjectRecoveryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(vm, null)!;
            Assert(vm.ProjectDirty && vm.HasInterruptedTake && vm.SelectedStem?.Id == stem.Id, "recovery selects interrupted track and exposes its recovery actions");
            await (Task)typeof(BackingTracksViewModel).GetMethod("RecoverInterruptedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(vm, null)!;
            Assert(vm.SelectedClip?.Takes.Any(t => t.Name == "Tomada recuperada" || t.Name == "Recovered take") == true, "interrupted recording can be recovered in a fresh data directory");
        }
        string capturePath = Path.Combine(root, "capture.wav");
        using (var capture = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 1),
            [new(stem.Id, 0, 1, capturePath, false, 1, 0)]))
        {
            capture.SetWindow(new(.001, .003));
            var samples = Enumerable.Repeat(.5f, 240).ToArray(); var bytes = new byte[samples.Length * 4]; Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
            capture.ProcessPacket(bytes);
            var reading = capture.ConsumeMeters(stem.Id);
            Assert(reading.Input.Peak > 0 && reading.Monitor.Peak == 0, "input meter remains active with monitoring off");
            Assert(Math.Abs(capture.Snapshot(stem.Id).Seconds - .002) < 1d / 48000, "capture window gates exact sample duration");
            await capture.Stop();
        }
        using (var reader = new WaveFileReader(capturePath)) Assert(reader.Length / reader.WaveFormat.BlockAlign == 96, "only A/B window frames reach recorded WAV");
        using (var capture = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 1), [new(stem.Id, 0, 1, null, false, 1, 0)]))
        {
            capture.SetWindow(new(.0005, .0015)); var bytes = new byte[48 * sizeof(float)];
            capture.ProcessPacket(bytes, packetTime: 0); capture.ProcessPacket(bytes, packetTime: .001);
            Assert(Math.Abs(capture.Snapshot(stem.Id).Seconds - .001) < 1d / 48000, "capture boundaries remain correct across packets");
        }
        using (var capture = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 1), [new(stem.Id, 0, 1, null, false, 1, 0)]))
        {
            capture.SetWindow(new(double.PositiveInfinity, double.PositiveInfinity));
            capture.ProcessPacket(new byte[240 * sizeof(float)], packetTime: 0);
            Assert(capture.Snapshot(stem.Id).Seconds == 0, "preparation buffers audio without creating a take");
            capture.SetWindow(new(.001, .003));
            Assert(Math.Abs(capture.Snapshot(stem.Id).Seconds - .002) < 1d / 48000, "clock handoff retains initial samples even without count-in");
        }
        string interrupted = Path.Combine(root, "interrupted.wav"); File.Copy(capturePath, interrupted);
        using (var stream = new FileStream(interrupted, FileMode.Open, FileAccess.ReadWrite))
        using (var writer = new BinaryWriter(stream))
        {
            stream.Position = 4; writer.Write(0);
            byte[] contents = File.ReadAllBytes(capturePath); int data = -1;
            for (int i = 12; i < contents.Length - 8; i++) if (contents[i] == 'd' && contents[i + 1] == 'a' && contents[i + 2] == 't' && contents[i + 3] == 'a') { data = i; break; }
            stream.Position = data + 4; writer.Write(0);
        }
        byte[] originalInterrupted = File.ReadAllBytes(interrupted);
        string repaired = BackingRecordingRecovery.Recover(interrupted, Path.Combine(root, "recovered.wav"));
        using (var reader = new WaveFileReader(repaired)) Assert(reader.Length / reader.WaveFormat.BlockAlign == 96, "interrupted WAV sizes repaired in a copy");
        Assert(originalInterrupted.SequenceEqual(File.ReadAllBytes(interrupted)), "recovering interrupted audio preserves original bytes");
        var audioStem = new BackingStem { Id = stem.Id, Source = source, LegacyMono = false, Volume = 100 };
        string export = Path.Combine(root, "export.wav");
        await BackingSessionAudio.Export(clip.Sources(audioStem).ToArray(), export, 0, 1, 1, 0, 1, CancellationToken.None);
        using (var reader = new WaveFileReader(export))
        {
            var values = new float[96000]; int read = reader.ToSampleProvider().Read(values.AsSpan());
            Assert(read == 96000 && reader.WaveFormat.BitsPerSample == 16 && reader.WaveFormat.Channels == 2, "export produces full-length 48kHz stereo PCM16");
            Assert(Math.Abs(values[0]) < .0001 && Math.Abs(values[48000] - .25 * Math.Sqrt(.5)) < .002, "export includes fade and mono pan law");
        }
        await BackingSessionAudio.Export(clip.Sources(audioStem).ToArray(), Path.Combine(root, "processed.wav"), 0, 1, 1.25, 2, 1, CancellationToken.None);
        using (var reader = new WaveFileReader(Path.Combine(root, "processed.wav"))) Assert(Math.Abs(reader.TotalTime.TotalSeconds - .8) < 1d / 48000, "native DSP export applies speed and pitch with correct duration");
        Assert(BackingClipEdit.Envelope(.05, 1, .1, 0, false, false) == .5, "fade envelope is applied numerically");
        Console.WriteLine("Backing checks artifacts: " + root);
    }
}
