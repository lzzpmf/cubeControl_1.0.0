using System.IO;
using System.Text.Json.Serialization;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

// Seconds in the session are independent of tempo. Layers preserve composite alternatives
// without flattening, copying, or modifying source audio.
public sealed record BackingClipEnvelope(double Position, double Duration, double FadeIn, double FadeOut, bool PowerIn, bool PowerOut);
public sealed record BackingClipLayer(Guid Id, string Source, double Position, double SourceStart,
    double Duration, double RecordedSpeed = 1, double RecordedPitch = 0, bool LegacyMono = false,
    double FadeIn = 0, double FadeOut = 0, bool EqualPowerIn = false, bool EqualPowerOut = false, BackingClipEnvelope[]? Envelopes = null);
public sealed record BackingClipTake(Guid Id, string Name, BackingClipLayer[] Layers, bool Incomplete = false);
public sealed record BackingClip(Guid Id, string Name, double Position, double Duration,
    BackingClipTake[] Takes, Guid ActiveTakeId, double FadeIn = 0, double FadeOut = 0,
    bool EqualPowerIn = false, bool EqualPowerOut = false)
{
    [JsonIgnore] public double End => Position + Duration;
    [JsonIgnore] public BackingClipTake? ActiveTake => Takes.FirstOrDefault(t => t.Id == ActiveTakeId && !t.Incomplete);
    public BackingClip Slice(double start, double end, bool newId = true)
    {
        start = Math.Max(start, Position); end = Math.Min(end, End);
        if (end <= start) throw new ArgumentOutOfRangeException(nameof(end));
        double shift = start - Position;
        var slices = Takes.Select(t => t with { Layers = t.Layers.Select(l => l with { Position = l.Position - shift,
            Envelopes = [..(l.Envelopes ?? []).Select(e => e with { Position = e.Position - shift }), new(-shift, Duration, FadeIn, FadeOut, EqualPowerIn, EqualPowerOut)] }).ToArray() }).ToArray();
        return this with { Id = newId ? Guid.NewGuid() : Id, Position = start, Duration = end - start,
            Takes = slices, FadeIn = 0, FadeOut = 0 };
    }
    public BackingClip Trim(double start, double end)
    {
        var layers = ActiveTake?.Layers ?? [];
        double minimum = Position + layers.Select(l => l.Position).DefaultIfEmpty(0).Min();
        double maximum = Position + layers.Select(l => l.Position + l.Duration).DefaultIfEmpty(Duration).Max();
        if (!double.IsFinite(start + end) || start < Math.Max(0, minimum) || end > maximum || end - start < .001) throw new ArgumentOutOfRangeException(nameof(start));
        double shift = start - Position;
        return this with { Position = start, Duration = end - start,
            Takes = Takes.Select(t => t with { Layers = t.Layers.Select(l => l with { Position = l.Position - shift, Envelopes = (l.Envelopes ?? []).Select(e => e with { Position = e.Position - shift }).ToArray() }).ToArray() }).ToArray(),
            FadeIn = Math.Min(FadeIn, end - start), FadeOut = Math.Min(FadeOut, end - start) };
    }
    public void Validate()
    {
        static bool Nonnegative(double x) => double.IsFinite(x) && x >= 0;
        if (Id == Guid.Empty || !double.IsFinite(Position) || !Nonnegative(Duration) || !Nonnegative(FadeIn) || !Nonnegative(FadeOut)
            || FadeIn > Duration || FadeOut > Duration || Takes == null || Takes.Length == 0
            || Takes.Select(t => t.Id).Distinct().Count() != Takes.Length || ActiveTake == null)
            throw new InvalidDataException("Invalid clip.");
        foreach (var take in Takes)
        {
            if (take.Layers == null) throw new InvalidDataException("Invalid clip take.");
            foreach (var l in take.Layers)
                if (!Path.IsPathFullyQualified(l.Source) || !double.IsFinite(l.Position) || !Nonnegative(l.SourceStart)
                    || !Nonnegative(l.Duration) || !double.IsFinite(l.RecordedSpeed) || l.RecordedSpeed is < .49 or > 1.53
                    || !double.IsFinite(l.RecordedPitch) || Math.Abs(l.RecordedPitch) > 6 || !Nonnegative(l.FadeIn) || !Nonnegative(l.FadeOut))
                    throw new InvalidDataException("Invalid clip layer.");
            foreach (var curve in take.Layers.SelectMany(l => l.Envelopes ?? []))
                if (!double.IsFinite(curve.Position) || !Nonnegative(curve.Duration) || !Nonnegative(curve.FadeIn) || !Nonnegative(curve.FadeOut))
                    throw new InvalidDataException("Invalid clip envelope.");
        }
    }
    public IEnumerable<BackingStemSource> Sources(BackingStem stem)
    {
        foreach (var layer in ActiveTake?.Layers ?? [])
        {
            double from = Math.Max(0, layer.Position), to = Math.Min(Duration, layer.Position + layer.Duration);
            if (to <= from) continue;
            yield return new BackingStemSource(layer.Id, layer.Source, Position + from, layer.RecordedSpeed, layer.RecordedPitch,
                stem.IsMuted, stem.IsSolo, stem.Volume / 100, stem.Pan, layer.LegacyMono,
                stem.Id, layer.SourceStart + (from - layer.Position) / layer.RecordedSpeed, to - from,
                Position, Duration, FadeIn, FadeOut, EqualPowerIn, EqualPowerOut,
                Position + layer.Position, layer.Duration, layer.FadeIn, layer.FadeOut, layer.EqualPowerIn, layer.EqualPowerOut, (layer.Envelopes ?? []).Select(e => e with { Position = Position + e.Position }).ToArray());
        }
    }
}

public static class BackingClipEdit
{
    public static double Envelope(double time, double duration, double fadeIn, double fadeOut, bool powerIn, bool powerOut)
    {
        if (time < 0 || time >= duration) return 0;
        double a = fadeIn > 0 ? Math.Clamp(time / fadeIn, 0, 1) : 1;
        double b = fadeOut > 0 ? Math.Clamp((duration - time) / fadeOut, 0, 1) : 1;
        return (powerIn ? Math.Sin(a * Math.PI / 2) : a) * (powerOut ? Math.Sin(b * Math.PI / 2) : b);
    }
    public static BackingClip[] Overdub(IEnumerable<BackingClip> existing, BackingClip recorded)
    {
        var result = new List<BackingClip>();
        var overlaps = existing.Where(c => c.End > recorded.Position && c.Position < recorded.End).ToArray();
        foreach (var clip in existing)
        {
            if (clip.End <= recorded.Position || clip.Position >= recorded.End) { result.Add(clip); continue; }
            if (clip.Position < recorded.Position) result.Add(clip.Slice(clip.Position, recorded.Position));
            if (clip.End > recorded.End) result.Add(clip.Slice(recorded.End, clip.End));
        }
        BackingClipLayer[] Project(BackingClip clip, BackingClipTake? take) => (take?.Layers ?? [])
            .Where(layer => Math.Min(Math.Min(clip.End, recorded.End), clip.Position + layer.Position + layer.Duration)
                > Math.Max(Math.Max(clip.Position, recorded.Position), clip.Position + layer.Position))
            .Select(layer => layer with { Id = Guid.NewGuid(), Position = clip.Position + layer.Position - recorded.Position,
                Envelopes = [..(layer.Envelopes ?? []).Select(e => e with { Position = clip.Position + e.Position - recorded.Position }),
                    new(clip.Position - recorded.Position, clip.Duration, clip.FadeIn, clip.FadeOut, clip.EqualPowerIn, clip.EqualPowerOut)] }).ToArray();
        var previous = overlaps.SelectMany(c => Project(c, c.ActiveTake)).ToArray();
        var alternatives = new List<BackingClipTake>();
        if (previous.Length > 0) alternatives.Add(new(Guid.NewGuid(), "Áudio anterior", previous));
        foreach (var clip in overlaps)
            foreach (var take in clip.Takes.Where(t => t.Id != clip.ActiveTakeId))
                alternatives.Add(new(Guid.NewGuid(), clip.Name + " · " + take.Name,
                    overlaps.SelectMany(c => Project(c, c == clip ? take : c.ActiveTake)).ToArray(), take.Incomplete));
        recorded = recorded with { Takes = [..alternatives, ..recorded.Takes] };
        result.Add(recorded); return result.OrderBy(c => c.Position).ToArray();
    }
}
