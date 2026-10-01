namespace Cube.Protocol;

public enum ModulationKind { Chorus, Phaser }

// Presentation semantics only. Raw bank values remain the authoritative device representation.
public sealed class ModulationState
{
    private int chorus = 3, phaser = 12;
    public ModulationKind Kind { get; private set; }
    public bool Enabled { get; private set; }
    public int Raw { get; private set; } = 8;
    public int Position => Kind == ModulationKind.Chorus ? 7 - chorus : phaser - 8;
    public void Load(int raw)
    {
        if (raw is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(raw));
        Raw = raw; Enabled = raw is < 7 or > 8;
        if (raw < 7) { Kind = ModulationKind.Chorus; chorus = raw; }
        else if (raw > 8) { Kind = ModulationKind.Phaser; phaser = raw; }
    }
    public int? Select(ModulationKind kind) { Kind = kind; return Enabled ? SetEnabled(true) : null; }
    public int SetEnabled(bool enabled) { Enabled = enabled; return Raw = enabled ? Kind == ModulationKind.Chorus ? chorus : phaser : 8; }
    public int? Adjust(int position)
    {
        position = Math.Clamp(position, 1, 7);
        // The hardware's Chorus range runs toward OFF as the raw value increases.
        if (Kind == ModulationKind.Chorus) chorus = 7 - position; else phaser = position + 8;
        return Enabled ? SetEnabled(true) : null;
    }
}

public sealed class ReverbState
{
    public bool Enabled { get; private set; }
    public int Amount { get; private set; } = 8;
    public void Load(int raw)
    {
        if (raw is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(raw));
        Enabled = raw != 0; if (Enabled) Amount = raw;
    }
    public int SetEnabled(bool enabled) { Enabled = enabled; return enabled ? Amount : 0; }
    public int? Adjust(int amount) { Amount = Math.Clamp(amount, 1, 15); return Enabled ? Amount : null; }
}

public sealed class WheelSteps
{
    private int remainder;
    public int Consume(int delta) { remainder += delta; int steps = remainder / 120; remainder %= 120; return steps; }
    public void Reset() => remainder = 0;
}

public sealed class EditHistory
{
    private readonly List<Dictionary<ParameterKey, int>> entries = [];
    private object? gesture;
    public bool CanUndo => entries.Count != 0;
    public void Record(ParameterKey key, int before, object? group = null)
    {
        if (entries.Count == 0 || group == null || !ReferenceEquals(gesture, group))
        { entries.Add([]); if (entries.Count > 100) entries.RemoveAt(0); }
        entries[^1].TryAdd(key, before); gesture = group;
    }
    public IReadOnlyDictionary<ParameterKey, int> Pop()
    {
        if (entries.Count == 0) return new Dictionary<ParameterKey, int>();
        var last = entries[^1]; entries.RemoveAt(entries.Count - 1); gesture = null; return last;
    }
    public void Clear() { entries.Clear(); gesture = null; }
}
