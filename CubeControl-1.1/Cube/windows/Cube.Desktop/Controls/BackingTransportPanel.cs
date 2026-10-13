using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// Eight cells: previous, back, play, forward, stop, next, loop and record.
// At narrow widths the four playback controls remain together on the first row.
public sealed class BackingTransportPanel : Panel
{
    private const double Gap = 8, Row = 72;
    private static double Cell(int index) => index == 2 ? 56 : 40;
    protected override Size MeasureOverride(Size availableSize)
    {
        for (int i = 0; i < InternalChildren.Count; i++) InternalChildren[i].Measure(new Size(Cell(i), Row));
        bool compact = availableSize.Width < 392;
        return new Size(compact ? 200 : 392, compact ? 2 * Row + Gap : Row);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        void ArrangeRow(int[] indices, double y)
        {
            double x = Math.Max(0, (finalSize.Width - indices.Sum(Cell) - Gap * (indices.Length - 1)) / 2);
            foreach (int i in indices)
            {
                if (i < InternalChildren.Count) InternalChildren[i].Arrange(new Rect(x, y, Cell(i), Row));
                x += Cell(i) + Gap;
            }
        }
        if (finalSize.Width >= 392) ArrangeRow([0, 1, 2, 3, 4, 5, 6, 7], 0);
        else { ArrangeRow([1, 2, 3, 4], 0); ArrangeRow([0, 5, 6, 7], Row + Gap); }
        return finalSize;
    }
}
