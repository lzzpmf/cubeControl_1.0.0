using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

// Pixel scrolling with generation limited to visible rows and one row of overscan.
// The ListBox retains selection, automation and recycled containers as columns change.
public sealed class EquipmentImageGridPanel : VirtualizingPanel, IScrollInfo
{
    private const double TileWidth = 190, TileHeight = 230;
    private Size extent, viewport;
    private double offset;
    public int Columns { get; private set; } = 1;
    public bool CanHorizontallyScroll { get; set; }
    public bool CanVerticallyScroll { get; set; }
    public double ExtentHeight => extent.Height;
    public double ExtentWidth => extent.Width;
    public double ViewportHeight => viewport.Height;
    public double ViewportWidth => viewport.Width;
    public double HorizontalOffset => 0;
    public double VerticalOffset => offset;
    public ScrollViewer? ScrollOwner { get; set; }

    protected override Size MeasureOverride(Size available)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner == null) return new Size();
        double width = double.IsFinite(available.Width) ? Math.Max(0, available.Width) : TileWidth * 4;
        double height = double.IsFinite(available.Height) ? Math.Max(0, available.Height) : TileHeight * 2;
        int previousColumns = Columns;
        Columns = Math.Max(1, (int)(width / TileWidth));
        // Keep the previously visible first item near the top after a resize.
        if (previousColumns != Columns) offset = Math.Floor(Math.Floor(offset / TileHeight) * previousColumns / Columns) * TileHeight;
        extent = new Size(width, Math.Ceiling((double)owner.Items.Count / Columns) * TileHeight);
        viewport = new Size(width, height);
        offset = Math.Clamp(offset, 0, Math.Max(0, extent.Height - height));
        ScrollOwner?.InvalidateScrollInfo();

        int first = Math.Max(0, ((int)(offset / TileHeight) - 1) * Columns);
        int last = Math.Min(owner.Items.Count - 1, ((int)Math.Ceiling((offset + height) / TileHeight) + 1) * Columns - 1);
        // Accessing the child collection connects an items host to its generator.
        var children = InternalChildren;
        var generator = ItemContainerGenerator;
        // Remove off-screen containers before inserting the newly visible range.
        for (int child = InternalChildren.Count - 1; child >= 0; child--)
        {
            int index = generator.IndexFromGeneratorPosition(new GeneratorPosition(child, 0));
            if (index < first || index > last)
            {
                var position = new GeneratorPosition(child, 0);
                if (generator is IRecyclingItemContainerGenerator recycling) recycling.Recycle(position, 1);
                else generator.Remove(position, 1);
                RemoveInternalChildRange(child, 1);
            }
        }
        if (last >= first)
        {
            var start = generator.GeneratorPositionFromIndex(first);
            int childIndex = start.Offset == 0 ? start.Index : start.Index + 1;
            using (generator.StartAt(start, GeneratorDirection.Forward, true))
                for (int index = first; index <= last; index++, childIndex++)
                {
                    var child = (UIElement)generator.GenerateNext(out bool newContainer);
                    // A recycled container can be known to the generator but detached
                    // from this panel after a filter/scroll. Attach it in either case.
                    if (newContainer || !InternalChildren.Contains(child))
                    {
                        if (childIndex >= InternalChildren.Count) AddInternalChild(child);
                        else InsertInternalChild(childIndex, child);
                        generator.PrepareItemContainer(child);
                    }
                    child.Measure(new Size(width / Columns, TileHeight));
                }
        }
        return viewport;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (int child = 0; child < InternalChildren.Count; child++)
        {
            int index = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(child, 0));
            double cell = finalSize.Width / Columns;
            InternalChildren[child].Arrange(new Rect(index % Columns * cell, index / Columns * TileHeight - offset, cell, TileHeight));
        }
        return finalSize;
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        // A filter replaces the source; discard containers and let the generator rebuild.
        if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        { RemoveInternalChildRange(0, InternalChildren.Count); offset = 0; }
        else if (args.Action is System.Collections.Specialized.NotifyCollectionChangedAction.Remove or System.Collections.Specialized.NotifyCollectionChangedAction.Replace or System.Collections.Specialized.NotifyCollectionChangedAction.Move)
        {
            if (args.ItemUICount > 0) RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
        }
        InvalidateMeasure();
    }

    protected override void BringIndexIntoView(int index)
    {
        Reveal(index); UpdateLayout();
    }
    private void Reveal(int index)
    {
        double top = index / Columns * TileHeight;
        if (top < offset) SetVerticalOffset(top);
        else if (top + TileHeight > offset + viewport.Height) SetVerticalOffset(top + TileHeight - viewport.Height);
    }
    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        var container = owner == null ? null : ItemsControl.ContainerFromElement(owner, visual);
        int index = container == null ? -1 : owner!.ItemContainerGenerator.IndexFromContainer(container);
        if (index >= 0) Reveal(index);
        return rectangle;
    }
    public void SetVerticalOffset(double value)
    {
        if (!double.IsFinite(value)) return;
        double next = Math.Clamp(value, 0, Math.Max(0, extent.Height - viewport.Height));
        if (Math.Abs(next - offset) < .01) return;
        offset = next; InvalidateMeasure(); ScrollOwner?.InvalidateScrollInfo();
    }
    public void SetHorizontalOffset(double value) { }
    public void LineUp() => SetVerticalOffset(offset - 32);
    public void LineDown() => SetVerticalOffset(offset + 32);
    public void MouseWheelUp() => SetVerticalOffset(offset - 96);
    public void MouseWheelDown() => SetVerticalOffset(offset + 96);
    public void PageUp() => SetVerticalOffset(offset - viewport.Height);
    public void PageDown() => SetVerticalOffset(offset + viewport.Height);
    public void LineLeft() { }
    public void LineRight() { }
    public void MouseWheelLeft() { }
    public void MouseWheelRight() { }
    public void PageLeft() { }
    public void PageRight() { }
}
