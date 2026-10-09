using System.Windows.Input;
using System.Windows.Media;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

// An isolated application snapshot: selection and distance never mutate the editor or saved definitions.
public sealed class IrPackReviewViewModel : Observable
{
    private readonly IrPackStore store;
    public IrPackReviewViewModel(IrPackDefinition definition, IrPackStore store, byte[][] originals, IrPackEditorViewModel editor)
    {
        this.store = store; Definition = definition;
        Slots = definition.Entries.OrderBy(e => e.Slot).Select(e => new IrPackReviewSlot(this, e, originals[e.Slot], editor)).ToArray();
    }
    public IrPackDefinition Definition { get; }
    public IrPackReviewSlot[] Slots { get; }
    public bool CanContinue => Slots.Any(s => s.Selected && s.Available);
    public string Summary
    {
        get
        {
            string slots = string.Join(", ", Enumerable.Range(0, 8).Where(i => !Slots.Any(s => s.Index == i && s.Selected)).Select(i => i + 1));
            return slots.Length == 0 ? "Nenhum" : slots;
        }
    }
    public ICommand SelectAll => new UiCommand(() => { foreach (var slot in Slots) slot.Selected = slot.Available; }, () => true);
    public ICommand SelectNone => new UiCommand(() => { foreach (var slot in Slots) slot.Selected = false; }, () => true);
    internal byte[] Prepare(IrPackEntry entry, out string problem)
    {
        var pack = store.Resolve(Definition with { Entries = [entry] }); problem = pack.Problem; return pack.Sectors[entry.Slot];
    }
    internal void Changed() { Notify(nameof(CanContinue)); Notify(nameof(Summary)); }
    public IrPackDefinition SelectedDefinition() => Definition with { Entries = Slots.Where(s => s.Selected && s.Available).Select(s => s.Entry).ToArray() };
}

public sealed class IrPackReviewSlot : Observable
{
    private readonly IrPackReviewViewModel review;
    private readonly byte[] original;
    private bool selected;
    public IrPackReviewSlot(IrPackReviewViewModel review, IrPackEntry entry, byte[] original, IrPackEditorViewModel editor)
    {
        this.review = review; this.original = original; Entry = entry;
        Image = editor.SlotImage(entry.Slot, entry); Prepare(); selected = Available;
    }
    public IrPackEntry Entry { get; private set; }
    public int Index => Entry.Slot;
    public string Title => $"Slot {Index + 1}";
    public string Name => Entry.Name;
    public ImageSource? Image { get; }
    public bool Available => Problem.Length == 0;
    public string Problem { get; private set; } = "";
    public byte[] Sector { get; private set; } = [];
    public string Comparison => !Available ? Problem : original.SequenceEqual(Sector) ? "Idêntico ao pedal. Não será gravado novamente." : "Será substituído se selecionado.";
    public bool Selected { get => selected; set { if (Set(ref selected, value && Available)) review.Changed(); } }
    public double Distance
    {
        get => Entry.Distance;
        set
        {
            if (!Available || !double.IsFinite(value)) return;
            int distance = Math.Clamp((int)Math.Round(value), 0, 99);
            if (distance == Entry.Distance) return;
            Entry = Entry with { Distance = distance }; Prepare(); Notify(); review.Changed();
        }
    }
    private void Prepare()
    {
        Sector = review.Prepare(Entry, out var problem); Problem = problem;
        if (!Available) selected = false;
        Notify(nameof(Problem)); Notify(nameof(Available)); Notify(nameof(Comparison)); Notify(nameof(Selected));
    }
}
