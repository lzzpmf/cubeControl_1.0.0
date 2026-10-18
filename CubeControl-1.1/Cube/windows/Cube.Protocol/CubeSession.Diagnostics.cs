using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cube.Protocol;

public sealed partial class CubeSession
{
    private long diagnosticReads, diagnosticWrites, diagnosticTxBytes, diagnosticRxBytes;
    // Scopes start only after acquiring the session gate; no overlapping transactions.
    private IDisposable MeasureOperation([CallerMemberName] string name = "") => new OperationMeasurement(this, name);

    private void RecordPerformance(string message) => Log?.Invoke(message);

    private sealed class OperationMeasurement(CubeSession session, string name) : IDisposable
    {
        private readonly long started = Stopwatch.GetTimestamp();
        private readonly long reads = session.diagnosticReads, writes = session.diagnosticWrites,
            tx = session.diagnosticTxBytes, rx = session.diagnosticRxBytes;
        public void Dispose()
        {
            session.RecordPerformance($"PERF {name}: {Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1} ms; " +
                $"reads={session.diagnosticReads - reads}; writes={session.diagnosticWrites - writes}; " +
                $"tx={session.diagnosticTxBytes - tx}; rx={session.diagnosticRxBytes - rx}");
        }
    }
}
