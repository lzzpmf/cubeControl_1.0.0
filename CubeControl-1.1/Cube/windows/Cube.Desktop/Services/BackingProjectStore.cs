using System.IO;
using System.Text.Json;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Services;

public sealed record BackingProjectStem(Guid Id, string Source, string Name, string Color, double Volume,
    double Pan, bool Muted, bool Solo, int Input, int Channels, bool LegacyMono,
    BackingTake[] Takes, Guid? ActiveTake, BackingClip[] Clips);

public sealed record BackingProjectDocument(int Version, Guid Id, string Name, BackingProjectStem[] Stems,
    double Bpm = 120, string Meter = "4/4", double FirstBeat = 0, double Speed = 100, double Pitch = 0,
    double Master = 1, double Bus = 1, double? RangeStart = null, double? RangeEnd = null,
    BackingSection[]? Sections = null, string Key = "Não definido", string Tuning = "", int Preset = 0,
    bool Snap = true, string SnapDivision = "1/4", int CountIn = 1, string MetronomeSound = "Click",
    double LatencyMs = 0, bool Loop = false, bool Muted = false, string AnalysisSource = "", bool KeyIsManual = false);

/// <summary>Portable project manifests. Runtime documents use absolute media paths; disk documents use Media-relative paths.</summary>
public static class BackingProjectStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public static void Validate(BackingProjectDocument doc)
    {
        if (doc.Version != 1 || doc.Id == Guid.Empty || string.IsNullOrWhiteSpace(doc.Name) || doc.Name.Length > 256 || doc.Stems == null
            || doc.Key == null || doc.Tuning == null || doc.Preset is < 0 or > 3
            || doc.Stems.Length > 1024 || doc.Stems.Select(s => s.Id).Distinct().Count() != doc.Stems.Length)
            throw new InvalidDataException("Projeto inválido ou versão incompatível.");
        if (!double.IsFinite(doc.Bpm) || doc.Bpm is < 20 or > 300 || doc.Meter is not ("2/4" or "3/4" or "4/4" or "6/8")
            || !double.IsFinite(doc.Speed) || doc.Speed is < 50 or > 150 || !double.IsFinite(doc.Pitch) || Math.Abs(doc.Pitch) > 6
            || !double.IsFinite(doc.Master) || doc.Master < 0 || doc.Master > BackingMixMath.MaximumGain
            || !double.IsFinite(doc.Bus) || doc.Bus < 0 || doc.Bus > BackingMixMath.MaximumGain
            || !double.IsFinite(doc.FirstBeat) || doc.FirstBeat < 0 || !double.IsFinite(doc.LatencyMs) || Math.Abs(doc.LatencyMs) > 1000
            || doc.CountIn is < 0 or > 2 || doc.SnapDivision is not ("Bar" or "1/2" or "1/4" or "1/8" or "1/16")
            || doc.RangeStart is double a && (!double.IsFinite(a) || a < 0)
            || doc.RangeEnd is double b && (!double.IsFinite(b) || b < 0)
            || doc.RangeStart is double x && doc.RangeEnd is double y && y - x < .1)
            throw new InvalidDataException("Parâmetros do projeto inválidos.");
        var ids = new HashSet<Guid>();
        foreach (var stem in doc.Stems)
        {
            if (stem.Id == Guid.Empty || stem.Source == null || stem.Name == null || stem.Color == null || stem.Color.Length != 7
                || !System.Text.RegularExpressions.Regex.IsMatch(stem.Color, "^#[0-9A-Fa-f]{6}$")
                || stem.Clips == null || stem.Takes == null || !double.IsFinite(stem.Volume)
                || stem.Volume < 0 || stem.Volume > BackingMixMath.MaximumGain * 100 || !double.IsFinite(stem.Pan)
                || Math.Abs(stem.Pan) > 1 || stem.Input < 0 || stem.Channels is < 1 or > 2)
                throw new InvalidDataException("Pista inválida.");
            foreach (var clip in stem.Clips)
            {
                clip.Validate();
                if (clip.Name == null || clip.Takes.Any(t => t.Name == null)) throw new InvalidDataException("Nome de clipe ou tomada inválido.");
                if (!ids.Add(clip.Id)) throw new InvalidDataException("Identificador de clipe duplicado.");
            }
            foreach (var take in stem.Takes)
                if (!double.IsFinite(take.Offset) || !double.IsFinite(take.RecordedSpeed) || take.RecordedSpeed is < .49 or > 1.53
                    || !double.IsFinite(take.RecordedPitch) || Math.Abs(take.RecordedPitch) > 6)
                    throw new InvalidDataException("Tomada inválida.");
        }
        foreach (var section in doc.Sections ?? [])
            if (!double.IsFinite(section.Position) || section.Position < 0 || string.IsNullOrWhiteSpace(section.Name))
                throw new InvalidDataException("Marcador inválido.");
    }

    public static BackingProjectDocument Load(string manifest)
    {
        var doc = JsonSerializer.Deserialize<BackingProjectDocument>(File.ReadAllText(manifest))
            ?? throw new InvalidDataException("Projeto vazio.");
        if (doc.Version != 1 || doc.Stems == null) throw new InvalidDataException("Versão do projeto incompatível.");
        string root = Path.GetDirectoryName(Path.GetFullPath(manifest))!;
        string Resolve(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path) || path.Contains(':')) throw new InvalidDataException("Referência de mídia inválida.");
            string full = Path.GetFullPath(Path.Combine(root, path));
            string media = Path.Combine(root, "Media") + Path.DirectorySeparatorChar;
            if (!full.StartsWith(media, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Mídia fora da pasta do projeto.");
            return full;
        }
        doc = MapPaths(doc, Resolve);
        Validate(doc);
        return doc;
    }

    public static async Task Save(string manifest, BackingProjectDocument doc, CancellationToken ct = default)
    {
        Validate(doc);
        string root = Path.GetDirectoryName(Path.GetFullPath(manifest))!;
        string media = Path.Combine(root, "Media");
        Directory.CreateDirectory(media);
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string source in MediaPaths(doc).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ct.ThrowIfCancellationRequested();
            if (!File.Exists(source)) throw new FileNotFoundException("Localize o áudio ausente antes de salvar o projeto.", source);
            string full = Path.GetFullPath(source);
            if (full.StartsWith(media + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            { paths[source] = Path.GetRelativePath(root, full); continue; }
            string name = Guid.NewGuid().ToString("N") + Path.GetExtension(source);
            string destination = Path.Combine(media, name), partial = destination + ".partial";
            try
            {
                await using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 81920, true))
                await using (var output = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                { await input.CopyToAsync(output, ct); await output.FlushAsync(ct); }
                File.Move(partial, destination);
            }
            finally { if (File.Exists(partial)) File.Delete(partial); }
            paths[source] = Path.Combine("Media", name);
        }
        var disk = MapPaths(doc, p => string.IsNullOrEmpty(p) ? "" : paths[p]);
        await WriteAtomic(manifest, disk, ct);
    }

    public static IEnumerable<string> MediaPaths(BackingProjectDocument doc)
        => doc.Stems.SelectMany(s => new[] { s.Source }.Concat(s.Takes.Select(t => t.Source))
            .Concat(s.Clips.SelectMany(c => c.Takes).SelectMany(t => t.Layers).Select(l => l.Source)))
            .Append(Path.IsPathFullyQualified(doc.MetronomeSound) ? doc.MetronomeSound : "").Append(doc.AnalysisSource).Where(p => !string.IsNullOrEmpty(p));

    public static BackingProjectDocument MapPaths(BackingProjectDocument doc, Func<string, string> map)
        => doc with { Stems = doc.Stems.Select(s => s with { Source = map(s.Source),
            Takes = s.Takes.Select(t => t with { Source = map(t.Source) }).ToArray(),
            Clips = s.Clips.Select(c => c with { Takes = c.Takes.Select(t => t with {
                Layers = t.Layers.Select(l => l with { Source = map(l.Source) }).ToArray() }).ToArray() }).ToArray() }).ToArray(),
            MetronomeSound = doc.MetronomeSound.Contains('/') || doc.MetronomeSound.Contains('\\') ? map(doc.MetronomeSound) : doc.MetronomeSound,
            AnalysisSource = string.IsNullOrEmpty(doc.AnalysisSource) ? "" : map(doc.AnalysisSource) };

    public static async Task WriteAtomic<T>(string path, T value, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(value, Json), ct);
            ct.ThrowIfCancellationRequested();
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
