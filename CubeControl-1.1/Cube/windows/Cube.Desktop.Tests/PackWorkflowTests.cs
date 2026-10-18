using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class PackWorkflowTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        await DirectPackEditingTests.Run();
        string data = Path.Combine(Path.GetTempPath(), "cube-pack-workflow-" + Guid.NewGuid().ToString("N"));
        using var model = new EditorViewModel(data);
        var editor = model.PackEditor;
        editor.ChooseDetails = (name, description) => Task.FromResult<(string Name, string Description)?>((name, description));
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
        await editor.SelectAsync(null);
        Check(!model.ApplyIrPack.CanExecute(null) && !editor.SavePack.CanExecute(null), "empty draft has no save or apply action");
        editor.PackLibrary.Selected = editor.PackLibrary.Entries.First();
        editor.Slots[1].UseIr.Execute(null); editor.Slots[6].UseIr.Execute(null);
        editor.Slots[1].DistanceValue = 19;
        var firstEntry = editor.Slots[1].Entry!;
        var secondIr = editor.PackLibrary.Entries.First(e => e.RelativePath != editor.SelectedIr!.RelativePath);
        Check(editor.AssignIr(3, secondIr) && editor.Slots[3].Entry!.Distance == 80, "library drop assigns a fresh IR at distance 80");
        editor.Slots[3].DistanceValue = 83;
        var secondEntry = editor.Slots[3].Entry!;
        Check(editor.SwapSlots(1, 3) && editor.Slots[1].Entry!.Sha256 == secondEntry.Sha256 && editor.Slots[3].Entry!.Distance == 19 && editor.Slots[1].Entry!.Distance == 83,
            "slot drag swaps content and its distance while preserving destination numbers");
        Check(editor.SwapSlots(3, 4) && !editor.Slots[3].HasEntry && editor.Slots[4].Entry!.Slot == 4 && editor.Slots[4].Entry!.Distance == 19, "dropping on an empty slot moves content and clears origin");
        var unchanged = editor.Draft();
        Check(!editor.SwapSlots(4, 4) && !editor.SwapSlots(0, 5) && !editor.AssignIr(9, secondIr) && editor.Draft().Entries.SequenceEqual(unchanged.Entries), "invalid and canceled moves leave draft intact");
        editor.Slots[1].Remove.Execute(null); editor.Slots[4].Remove.Execute(null);
        editor.Slots[1].UseIr.Execute(null);
        editor.Name = "Metadata before"; editor.Description = "Original notes";
        int detailsCalls = 0;
        editor.ChooseDetails = (name, description) => { detailsCalls++; return Task.FromResult<(string Name, string Description)?>(null); };
        Check(!await editor.EnsureDetailsAsync(true) && editor.Name == "Metadata before" && editor.Description == "Original notes", "canceling metadata preserves named draft and fields");
        Check(await editor.EnsureDetailsAsync() && detailsCalls == 1, "exporting an already named draft does not reopen metadata");
        editor.ChooseDetails = (name, description) => Task.FromResult<(string Name, string Description)?>(("Confirmed name", "Confirmed notes"));
        Check(await editor.EnsureDetailsAsync(true) && editor.Name == "Confirmed name" && editor.Description == "Confirmed notes", "saving a named pack still requests metadata and commits only confirmed fields");
        editor.ChooseDetails = (name, description) => Task.FromResult<(string Name, string Description)?>((name, description));
        editor.Name = "Portable draft";
        string bundle = Path.Combine(data, "portable.cubeirpack.zip"); Directory.CreateDirectory(data);
        editor.Export(bundle);
        using (var zip = ZipFile.OpenRead(bundle)) Check(zip.Entries.Count == 2, "portable export deduplicates repeated WAVs by hash");
        Check(!Directory.Exists(model.PackStore.DirectoryPath), "exporting a draft does not save a pack");
        var emptyStore = new IrPackStore(Path.Combine(data, "empty-library"), Path.Combine(data, "recipient"));
        var imported = emptyStore.ImportDraft(bundle);
        Check(emptyStore.Resolve(imported).Problem == "" && imported.Entries.Select(e => e.Slot).SequenceEqual(new[] { 1, 6 }), "portable pack resolves on a computer without the original library");
        Check(!Directory.Exists(emptyStore.DirectoryPath), "portable import stages WAVs without saving a definition");
        string legacy = Path.Combine(data, "legacy.cubeirpack.json"); IrPackStore.Export(legacy, editor.Draft());
        Check(emptyStore.Resolve(emptyStore.ImportDraft(legacy)).Problem.Length > 0, "legacy reference packs remain repairable when WAVs are missing");
        string invalid = Path.Combine(data, "invalid.cubeirpack.zip");
        using (var zip = ZipFile.Open(invalid, ZipArchiveMode.Create))
        {
            using (var stream = zip.CreateEntry("manifest.json").Open()) JsonSerializer.Serialize(stream, editor.Draft());
            using var outside = zip.CreateEntry("../outside.wav").Open(); outside.WriteByte(0);
        }
        bool rejected = false;
        try { emptyStore.ImportDraft(invalid); } catch (FormatException) { rejected = true; }
        Check(rejected && !File.Exists(Path.Combine(data, "outside.wav")), "archive paths cannot escape the imported library");
        string corrupt = Path.Combine(data, "corrupt.cubeirpack.zip");
        using (var zip = ZipFile.Open(corrupt, ZipArchiveMode.Create))
        {
            using (var stream = zip.CreateEntry("manifest.json").Open()) JsonSerializer.Serialize(stream, editor.Draft());
            using var wav = zip.CreateEntry("wavs/" + editor.Draft().Entries[0].Sha256 + ".wav").Open(); wav.WriteByte(0);
        }
        rejected = false; try { emptyStore.ImportDraft(corrupt); } catch (FormatException) { rejected = true; }
        Check(rejected, "corrupt embedded WAV rejected before import commits");
        editor.Import(bundle);
        Check(editor.Dirty && model.IrLibrary.Entries.Any(e => e.RelativePath.StartsWith("Imported/")), "import stages the pack and updates the individual library");
        Check(editor.PackLibrary.Roots.Any(n => n.Name == "IRs importados"), "imported WAVs appear in the shared pack library");
        Check(editor.Current!.Definition!.Entries.All(e => e.Name == firstEntry.Name) && editor.PackLibrary.Entries.Where(e => e.RelativePath.StartsWith("Imported/")).All(e => Path.GetFileNameWithoutExtension(e.FullPath).StartsWith(firstEntry.Name.Split(' ')[0], StringComparison.Ordinal)), "imported library keeps musical cabinet names instead of losing the original collection context");
        editor.Save();
        editor.Name = "Save on switch";
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Save);
        var savedId = editor.Current!.Id;
        await editor.SelectAsync(model.IrPacks.First(p => p.Definition != null && p.Group != "Meus packs"));
        Check(model.IrPacks.Single(p => p.Id == savedId).Name == "Save on switch", "save-discard-cancel switch can save the current personal draft");
        Check(model.IrPacks.Contains(editor.Current!), "saving during a switch selects an item from the refreshed catalog");
        Check(editor.CanEdit && editor.Slots[0].Remove.CanExecute(null) && !editor.Dirty, "predefined packs offer direct editing before any draft is created");
        using var fake = new FakeConnection(); using var session = new CubeSession(fake); await session.Identify();
        await model.Execute(() => model.AttachSession(session));
        var before = fake.Flash.ToArray();
        var predefined = editor.Current!;
        model.ReviewPack = review =>
        {
            Check(review.Slots.All(s => s.Selected && s.Distance == 80), "review starts with all available entries and stored distances");
            review.SelectNone.Execute(null); Check(!review.CanContinue, "review blocks an empty selection");
            review.Slots[2].Selected = true; review.Slots[2].Distance = 99;
            Check(review.CanContinue && review.SelectedDefinition().Entries.Single().Distance == 99, "review snapshot combines selection and per-IR distance");
            return Task.FromResult(false);
        };
        async Task Apply() => await (Task)typeof(EditorViewModel).GetMethod("ApplyIrPackAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(model, null)!;
        await Apply(); Check(fake.Flash.SequenceEqual(before) && fake.Writes == 0, "canceling review sends no device write");
        Check(predefined.Definition!.Entries.All(e => e.Distance == 80) && !editor.Dirty, "temporary review distances do not modify the predefined pack");
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
        await editor.SelectAsync(null); editor.SelectedIr = editor.PackLibrary.Entries[0]; editor.Slots[4].UseIr.Execute(null);
        Check(model.ApplyIrPack.CanExecute(null), "unnamed unsaved draft can be applied");
        int writes = fake.Writes;
        model.ReviewPack = review => { review.Slots.Single().Distance = 23; return Task.FromResult(true); };
        model.Ask = _ => Task.FromResult(true);
        var expected = model.PackStore.Resolve(editor.ApplicationDraft() with { Entries = [editor.Slots[4].Entry! with { Distance = 23 }] });
        var readSlots = new HashSet<uint>();
        fake.Observe = packet =>
        {
            if (packet.Command == 0x23 && packet.Payload[0] == 0)
                readSlots.Add((System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1)) - 0x69000) / 4096);
        };
        await Apply();
        Check(readSlots.SetEquals(new uint[] { 4 }), "partial pack reads only its populated destination, including verification");
        fake.Observe = null;
        for (int i = 0; i < 8; i++) Check(fake.Flash.AsSpan(i * 4096, 4096).SequenceEqual(i == 4 ? expected.Sectors[i] : before.AsSpan(i * 4096, 4096)), "draft application preserves destination " + (i + 1));
        Check(editor.Dirty && editor.Name == "" && editor.Slots[4].Entry!.Distance == 80 && fake.Writes > writes, "application leaves the unnamed draft and its saved distance intact");
        int erases = fake.Erases; await Apply(); Check(fake.Erases == erases, "identical adjusted sector is not rewritten");
        var missing = editor.ApplicationDraft() with { Entries = [editor.Slots[4].Entry! with { RelativePath = "missing.wav" }] };
        var reviewMissing = new IrPackReviewViewModel(missing, model.PackStore, Enumerable.Range(0, 8).Select(i => before.AsSpan(i*4096,4096).ToArray()).ToArray(), editor);
        reviewMissing.SelectAll.Execute(null);
        Check(!reviewMissing.CanContinue && !reviewMissing.Slots[0].Selected && reviewMissing.Slots[0].Problem.Length > 0, "missing reference is visibly unavailable and cannot be selected");
    }
}
