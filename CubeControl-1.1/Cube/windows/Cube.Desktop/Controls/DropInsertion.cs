namespace Cube.Desktop.Controls;

internal static class DropInsertion
{
    // Indices are always from the source collection, never from a filtered view.
    public static int Index(int oldIndex, int targetIndex, bool after, int count)
    {
        if (oldIndex < 0 || oldIndex >= count || targetIndex < -1 || targetIndex >= count) return -1;
        int insertion = targetIndex < 0 ? count : targetIndex + (after ? 1 : 0);
        if (oldIndex < insertion) insertion--;
        return insertion == oldIndex ? -1 : insertion;
    }
}
