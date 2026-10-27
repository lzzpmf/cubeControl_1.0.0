namespace Cube.Desktop.Services;

public interface IIrAuditionService : IDisposable
{
    BackingPlaybackSnapshot Snapshot { get; }
    Task Open(string path, string output, double duration, bool loop, double volume);
    float ConsumePeak() => 0;
    Task Play();
    Task Pause();
    Task Clear();
    Task Configure(bool loop, double duration);
    Task Volume(double volume, bool muted);
}

// Raw playback only: the pedal processes the signal with its current preset and effect chain,
// including confirmed RAM previews. Loading a replacement IR is not required.
public sealed class IrAuditionService : IIrAuditionService
{
    private readonly BackingAudioEngine engine = new();
    public BackingPlaybackSnapshot Snapshot => engine.Snapshot;
    public Task Open(string path, string output, double duration, bool loop, double volume) => engine.Open(path, output, loop, volume);
    public float ConsumePeak() => engine.ConsumeOutputPeak();
    public Task Play() => engine.Play();
    public Task Pause() => engine.Pause();
    public Task Clear() => engine.Clear();
    public Task Configure(bool loop, double duration) => engine.Configure(loop);
    public Task Volume(double volume, bool muted) => engine.SetVolume(volume, muted);
    public void Dispose() => engine.Dispose();
}
