using System.Diagnostics;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

// Filter before posting to the UI, preserving stage boundaries and completion.
internal sealed class TransferProgressThrottle(Action<TransferProgress> report) : IProgress<TransferProgress>
{
    private readonly IProgress<TransferProgress> target = new Progress<TransferProgress>(report);
    private readonly object sync = new();
    private long last;
    private string? stage;
    public void Report(TransferProgress value)
    {
        lock (sync)
        {
            long now = Stopwatch.GetTimestamp();
            if (stage == value.Stage && value.Done < value.Total &&
                Stopwatch.GetElapsedTime(last, now).TotalMilliseconds < 50) return;
            stage = value.Stage; last = now;
            target.Report(value);
        }
    }
}
