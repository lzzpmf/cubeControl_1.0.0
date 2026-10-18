using System.IO;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class IrLibraryTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-ir-library-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var impulse = new float[512]; impulse[0] = 1;
        Check(Cube.Desktop.Controls.Waveform.Spectrum(impulse).All(db => Math.Abs(db) < 1e-8), "impulse frequency response is flat at zero relative dB");
        var sine = Enumerable.Range(0, 512).Select(i => (float)Math.Sin(2 * Math.PI * 16 * i / 512)).ToArray();
        var response = Cube.Desktop.Controls.Waveform.Spectrum(sine);
        Check(Array.IndexOf(response, response.Max()) == 15 && Math.Abs(response[15]) < 1e-8, "1500 Hz reference peaks at its measured frequency bin");
        Check(Cube.Desktop.Controls.Waveform.Spectrum(new float[512]).All(db => db == -60), "silent response remains finite at display floor");
        var library = new IrLibraryViewModel(() => true);
        Check(library.Entries.Count == 958 && library.Roots.Count == 7, "958 WAVs across seven cabinet brands; NAM models excluded");
        Check(library.Roots.All(n => !n.IsExpanded && n.EntryCount > 0), "brand folders start collapsed and report their IR counts");
        Check(library.Roots.Sum(n => n.EntryCount) == library.Entries.Count, "tree contains every WAV exactly once");
        Check(library.Roots.Single(n => n.Name == "Fender").Children.Select(n => n.Name).SequenceEqual(new[] { "1x12", "2x12" }), "brand leads directly to speaker configuration without redundant format level");
        Check(library.Entries.All(e => !e.Name.Contains("Fender") && !e.Name.Contains("Marshall")), "leaf names omit redundant brand names");
        library.Search = "Mesa 4x12 SM57";
        Check(library.VisibleEntries.Count > 0 && library.VisibleEntries.All(e => e.RelativePath.Contains("Mesa") && e.RelativePath.Contains("4x12") && e.RelativePath.Contains("SM57")), "search combines brand, speaker layout and microphone");
        library.Search = "";
        var fender = library.Roots.Single(n => n.Name == "Fender");
        var marshall = library.Roots.Single(n => n.Name == "Marshall");
        fender.IsExpanded = true; fender.Children[0].IsExpanded = true;
        fender.Children[1].IsExpanded = true;
        Check(fender.IsExpanded && fender.Children[0].IsExpanded && fender.Children[1].IsExpanded, "multiple subfolders stay open within a brand");
        marshall.IsExpanded = true;
        Check(!fender.IsExpanded && fender.Children.All(n => n.IsExpanded) && marshall.IsExpanded, "opening another brand closes only the previous brand and remembers its subfolders");
        fender.IsExpanded = true;
        Check(!marshall.IsExpanded && fender.Children.All(n => n.IsExpanded), "reopening a brand restores its expanded subfolders");
        Check(library.Selected == null && library.Roots.All(n => n.IsBrand) && fender.Children.All(n => !n.IsBrand), "expansion does not select audio and only brands receive emphasis");
        library.Search = "4x12";
        Check(library.Roots.Count(n => n.IsExpanded) == 1, "search opens only one brand");
        library.Search = "";
        foreach (var entry in library.Entries)
            Check(IrAudio.Decode(await File.ReadAllBytesAsync(entry.FullPath)).Prepare().Length == 512, "decode and prepare " + entry.Name);
        if (TestCatalog.Arguments.Contains("--ir-catalog-only")) return;
        Check(new IrLibraryViewModel(() => true, Path.Combine(folder, "missing")).Entries.Count == 0, "missing library is nonfatal");
        Check(new IrLibraryViewModel(() => true, folder).Entries.Count == 0, "empty library is nonfatal");
        foreach (string name in new[] { "A", "B/nested" })
        {
            Directory.CreateDirectory(Path.Combine(folder, name));
            File.Copy(library.Entries[0].FullPath, Path.Combine(folder, name, "same.WAV"));
        }
        var duplicate = new IrLibraryViewModel(() => true, folder);
        Check(duplicate.Entries.Count == 2 && duplicate.Entries.Select(e => e.RelativePath).Distinct().Count() == 2, "recursive case-insensitive discovery preserves duplicate names");
        Check(duplicate.Roots[1].Children[0].Name == "nested" && duplicate.Roots[1].Children[0].Children.Count == 1, "tree preserves nested folder hierarchy");
        library.Search = "Orange";
        Check(library.Roots.Count == 1 && library.Roots[0].EntryCount == 20 && library.Roots[0].IsExpanded, "collection search expands matching folder");
        library.Search = "sm57";
        Check(library.VisibleEntries.Count == library.Entries.Count(e => e.SearchText.Contains("sm57", StringComparison.OrdinalIgnoreCase)) && library.Selected == null, "collection and name filters do not auto-select");
        library.Search = "no-such-ir";
        Check(library.VisibleEntries.Count == 0, "empty search results");

        using var model = new EditorViewModel(Path.Combine(folder, "data"));
        var first = model.IrLibrary.Entries[0]; var last = model.IrLibrary.Entries[^1];
        model.IrLibrary.Selected = first; Task stale = model.PendingLibraryLoad;
        model.IrLibrary.Selected = last; await Task.WhenAll(stale, model.PendingLibraryLoad);
        Check(model.Ir.File == IrViewModel.DisplayName(last.FullPath, last.Collection) && !model.Ir.PreviewActive, "rapid offline selection loads only last IR");
        using var fake = new FakeConnection(); using var session = new CubeSession(fake); await session.Identify();
        await model.Execute(() => model.AttachSession(session));
        Check(fake.Writes == 0, "connecting does not preview prepared library audio");
        model.IrLibrary.Roots[0].IsExpanded = true; model.IrLibrary.Roots[0].IsSelected = true;
        await model.PendingLibraryLoad;
        Check(fake.Writes == 0, "expanding and selecting tree folders never previews audio");
        model.IrLibrary.Search = "DELUXE"; await model.PendingLibraryLoad;
        Check(fake.Writes == 0 && model.IrLibrary.Selected == null, "filtering does not preview");
        model.IrLibrary.Search = "";
        var capture = model.IrLibrary.Entries.First(e => e.Metadata != null).Metadata!;
        model.IrLibrary.Search = capture.Brand + " " + capture.SpeakerConfiguration + " " + capture.Microphone;
        model.IrLibrary.Search = "";
        Check(fake.Writes == 0 && model.IrLibrary.Selected == null, "capture search and clearing search send no commands to connected pedal");
        model.IrLibrary.Selected = first; await model.PendingLibraryLoad;
        Check(model.Ir.PreviewConfirmed && fake.Writes > 0 && fake.FlashWrites == 0, "library selection previews in RAM only");
        var active = model.Ir.Audio; int writes = fake.Writes;
        string bad = Path.Combine(folder, "invalid.wav"); await File.WriteAllTextAsync(bad, "invalid");
        model.IrLibrary.Selected = new("invalid.wav", bad, "Test", "Invalid"); await model.PendingLibraryLoad;
        Check(ReferenceEquals(active, model.Ir.Audio) && model.CanOperate && fake.Writes == writes && model.ErrorMessage.Contains("Invalid"), "invalid WAV preserves audio and healthy connection");
        model.IrLibrary.Selected = new("missing.wav", Path.Combine(folder, "missing.wav"), "Test", "Missing"); await model.PendingLibraryLoad;
        Check(ReferenceEquals(active, model.Ir.Audio) && model.CanOperate && fake.Writes == writes, "missing WAV preserves audio and healthy connection");
        var hold = new TaskCompletionSource(); var busy = model.Execute(() => hold.Task);
        var selected = model.IrLibrary.Selected; model.IrLibrary.Selected = last;
        Check(model.IrLibrary.Selected == selected && !model.CanChooseIr, "busy transfer blocks selection");
        hold.SetResult(); await busy;
        await model.Execute(() => model.LoadWavAndPreviewAsync("external.wav", active!));
        Check(model.IrLibrary.Selected == null && model.Ir.Origin == "Arquivo externo" && model.Ir.SourceCollection == "", "external WAV clears library selection and source");
        model.IrLibrary.Selected = last; await model.PendingLibraryLoad;
        Check(model.Ir.SourceCollection == last.Collection, "return to library updates source");
        model.Ir.Slot = 2; bool confirmed = false;
        int commands = 0; fake.Observe = _ => commands++;
        int backupCount = Directory.GetFiles(Path.Combine(folder, "data"), "*", SearchOption.AllDirectories).Length;
        var confirmation = new TaskCompletionSource<bool>();
        model.Ask = _ => confirmation.Task;
        model.WriteIr.Execute(null);
        Check(!model.Busy && !model.ShowBusyPanel && commands == 0, "confirmation precedes loading and all device commands");
        confirmation.SetResult(false); await Task.Delay(50);
        Check(commands == 0 && Directory.GetFiles(Path.Combine(folder, "data"), "*", SearchOption.AllDirectories).Length == backupCount, "canceling confirmation performs no reads or backup");
        model.Ask = message => { confirmed = message.Contains(model.Ir.File) && message.Contains("slot 3", StringComparison.OrdinalIgnoreCase) && !model.Busy && commands == 0; return Task.FromResult(true); };
        var expected = model.Ir.Audio!.Sector(model.Ir.Distance);
        model.WriteIr.Execute(null); while (model.Busy) await Task.Delay(20);
        Check(confirmed && Directory.GetFiles(Path.Combine(folder, "data"), "*", SearchOption.AllDirectories).Length == backupCount && fake.FlashWrites > 0 && fake.Flash.AsSpan(8192, 4096).SequenceEqual(expected), "confirmed write preserves original in memory and verifies slot bytes");
        Check(!model.Ir.PreviewActive && model.Ir.SlotInfo.Contains(model.Ir.File), "write ends preview and retains content identity");
        model.IrLibrary.Selected = first; await model.PendingLibraryLoad;
        await model.Execute(model.EndPreviewAsync);
        Check(!model.Ir.PreviewActive && model.Ir.Audio == null && model.Ir.File == "Nenhum arquivo selecionado"
            && model.Ir.Origin == "" && model.Ir.SourceCollection == "" && model.Ir.Waveform.Length == 0
            && model.IrLibrary.Selected == null && !model.PreviewIr.CanExecute(null) && !model.WriteIr.CanExecute(null),
            "ending preview restores RAM and clears loaded file, source, waveform and selection");
        model.IrLibrary.Selected = last; var pending = model.PendingLibraryLoad;
        model.IrLibrary.Search = "cancel"; await pending;
        Check(model.Ir.Audio == null && !model.Ir.PreviewActive, "filter cancels pending preview without restoring a cleared file");
        model.IrLibrary.Search = "";
        fake.Silent = true;
        model.IrLibrary.Selected = last; await model.PendingLibraryLoad;
        Check(!model.CanOperate && model.ErrorMessage.Length > 0, "connection failure uses existing fault recovery");
    }
}
