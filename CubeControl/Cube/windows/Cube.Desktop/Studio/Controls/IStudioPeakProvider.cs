using Cube.Studio.Core;

namespace Cube.Desktop.Studio.Controls;

/// <summary>
/// UI-only access to already decoded, cached peaks. Implementations never perform I/O
/// in GetPeak; unknown/missing media returns zero. Times are source-file seconds.
/// </summary>
public interface IStudioPeakProvider
{
    float GetPeak(ClipModel clip, double sourceSeconds, double spanSeconds);
    bool HasMedia(ClipModel clip) => true;
    event EventHandler? PeaksChanged;
}
