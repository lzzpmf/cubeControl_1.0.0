namespace Cube.Desktop.Studio.Controls;

public enum TimelineEditKind { Split, Delete, Duplicate, Move, Trim, Loop }

public sealed class TimelineSeekEventArgs(double time) : EventArgs
{
    public double Time { get; } = time;
}

public sealed class TimelineSelectionEventArgs(Guid trackId, Guid? clipId) : EventArgs
{
    public Guid TrackId { get; } = trackId;
    public Guid? ClipId { get; } = clipId;
}

public sealed class TimelineEditEventArgs : EventArgs
{
    public TimelineEditKind Kind { get; init; }
    public Guid TrackId { get; init; }
    public Guid ClipId { get; init; }
    public Guid TargetTrackId { get; init; }
    public double Start { get; init; }
    public double Duration { get; init; }
    public double SourceOffset { get; init; }
    public double Time { get; init; }
}

public sealed class TimelineTrackHeightEventArgs(Guid trackId, double factor) : EventArgs
{
    public Guid TrackId { get; } = trackId;
    public double Factor { get; } = factor;
}

public sealed class TimelineFilesDroppedEventArgs(string[] paths, Guid trackId, double time) : EventArgs
{
    public string[] Paths { get; } = paths;
    public Guid TrackId { get; } = trackId;
    public double Time { get; } = time;
}
