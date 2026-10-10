using System.IO;
using Cube.Desktop.ViewModels;

internal static class DirectPackEditingTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-direct-pack-" + Guid.NewGuid().ToString("N"));
        using var model = new EditorViewModel(folder);
        var editor = model.PackEditor;
        var original = model.IrPacks.First(p => p.Group == "Predefinidos");
        var originalEntries = original.Definition!.Entries.ToArray();
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
        model.Ask = _ => Task.FromResult(true);
        await editor.SelectAsync(original);
        editor.Search = "Fender"; editor.SelectedIr = editor.PackLibrary.Entries.First(); editor.Search = "";
        Check(!editor.Dirty && !editor.IsCreating && !editor.HasDraftNotice && editor.Current!.Id == original.Id, "browsing predefined packs and library does not enter creation");
        Check(!editor.SwapSlots(0, 0) && !editor.Dirty, "ineffective reorder does not create a draft");
        editor.Slots[0].Remove.Execute(null);
        Check(editor.IsCreating && editor.Dirty && editor.HasDraftNotice && editor.Current!.Id != original.Id && editor.BasedOnName == original.Name && editor.Slots[0].Entry == null,
            "first direct edit creates a named-source independent draft and creation notice");
        Check(original.Definition.Entries.SequenceEqual(originalEntries) && model.SelectedIrPack == editor.Current, "implicit creation preserves predefined content and updates application target");
        var snapshot = editor.Draft();
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Cancel);
        Check(!await editor.SelectAsync(model.IrPacks.Last()) && editor.Draft().Entries.SequenceEqual(snapshot.Entries) && editor.Current!.Id == snapshot.Id, "canceling pack switch keeps the implicit draft");
        editor.DiscardChanges.Execute(null);
        Check(editor.Current!.Id == original.Id && !editor.Dirty && !editor.HasDraftNotice && editor.Slots.All(s => s.HasEntry), "discard returns to the exact original predefined pack");
        editor.ChooseDraftDecision = () => Task.FromResult(PackDraftDecision.Discard);
        foreach (var slot in editor.Slots) slot.Remove.Execute(null);
        Check(editor.IsCreating && !editor.HasEntries && !editor.SavePack.CanExecute(null) && !model.ApplyIrPack.CanExecute(null), "all slots can be cleared without a create mode and empty draft cannot save or apply");
        editor.AssignIr(3, editor.SelectedIr!); editor.Name = "Direct personal"; editor.Save();
        string personalId = editor.Current!.Id;
        Check(personalId != original.Id && !editor.Dirty && !editor.HasDraftNotice && model.IrPacks.Single(p => p.Id == original.Id).Definition!.Entries.SequenceEqual(originalEntries), "saving implicit creation stores a personal pack and preserves source");
        editor.Slots[3].DistanceValue = 73;
        Check(!editor.IsCreating && editor.HasDraftNotice && editor.DraftNoticeTitle == "Altera\u00e7\u00f5es n\u00e3o salvas", "personal edit uses unsaved-changes notice instead of creation");
        editor.Save();
        Check(editor.Current!.Id == personalId && editor.Current.Definition!.Entries.Single().Distance == 73, "saving a personal edit updates its identity and definition");
        editor.Slots[3].Remove.Execute(null); editor.DiscardChanges.Execute(null);
        Check(editor.Current!.Id == personalId && editor.Slots[3].Entry!.Distance == 73 && !editor.Dirty, "discard restores last saved personal contents");
        string portable = Path.Combine(folder, "direct.cubeirpack.zip"); editor.Export(portable); editor.Import(portable);
        Check(editor.IsCreating && editor.Dirty && editor.Current!.Id != personalId, "import still creates an unsaved editable draft");
        editor.DiscardChanges.Execute(null);
        Check(editor.Current!.Id == personalId && !editor.Dirty, "discarding imported draft returns to previous saved pack");
        await model.Execute(() => {
            Check(!editor.AssignIr(0, editor.SelectedIr!) && !editor.SwapSlots(3, 4) && !editor.Slots[3].Remove.CanExecute(null), "processing locks every direct editing operation");
            return Task.CompletedTask;
        });
        Check(model.WriteCommandCount == 0, "direct editing, saving and discarding send no device commands");
    }
}
