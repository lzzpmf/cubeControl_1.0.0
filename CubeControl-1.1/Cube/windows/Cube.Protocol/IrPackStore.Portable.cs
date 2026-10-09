using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Cube.Protocol;

public sealed partial class IrPackStore
{
    private const int MaxWavBytes = 32 * 1024 * 1024;
    private const long MaxBundleBytes = 256L * 1024 * 1024 + 128 * 1024;

    public void ExportPortable(string path, IrPackDefinition pack)
    {
        Validate(pack);
        var wavs = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in pack.Entries)
        {
            var file = ResolvePath(entry.RelativePath);
            if (new FileInfo(file).Length > MaxWavBytes) throw new FormatException("WAV excede 32 MB.");
            var bytes = File.ReadAllBytes(file);
            if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new FormatException("IR modificado na biblioteca.");
            _ = IrAudio.Decode(bytes);
            wavs.TryAdd(entry.Sha256.ToUpperInvariant(), bytes);
        }
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var zip = ZipFile.Open(temp, ZipArchiveMode.Create))
            {
                using (var manifest = zip.CreateEntry("manifest.json").Open()) JsonSerializer.Serialize(manifest, pack, Json);
                foreach (var (hash, bytes) in wavs)
                {
                    using var stream = zip.CreateEntry("wavs/" + hash + ".wav", CompressionLevel.Optimal).Open();
                    stream.Write(bytes);
                }
            }
            File.Move(temp, path, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    // Import validates the entire archive before committing WAVs. Saving the pack is separate.
    public IrPackDefinition ImportDraft(string path)
    {
        if (!path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return Read(path) with { Id = Guid.NewGuid().ToString("N"), Group = "Meus packs" };
        try { return ImportPortable(path); }
        catch (InvalidDataException ex) { throw new FormatException("Conteúdo do pack inválido.", ex); }
    }
    private IrPackDefinition ImportPortable(string path)
    {
        if (new FileInfo(path).Length > MaxBundleBytes) throw new FormatException("Pack excede o limite de tamanho.");
        using var zip = ZipFile.OpenRead(path);
        if (zip.Entries.Count is < 2 or > 9 || zip.Entries.Sum(e => e.Length) > MaxBundleBytes
            || zip.Entries.Select(e => e.FullName).Distinct(StringComparer.OrdinalIgnoreCase).Count() != zip.Entries.Count)
            throw new FormatException("Conteúdo do pack inválido.");
        var manifest = zip.GetEntry("manifest.json") ?? throw new FormatException("Manifesto do pack ausente.");
        if (manifest.Length > 128 * 1024) throw new FormatException("Arquivo de pack excede 128 KB.");
        IrPackDefinition pack;
        using (var stream = manifest.Open()) pack = JsonSerializer.Deserialize<IrPackDefinition>(ReadBounded(stream, 128 * 1024)) ?? throw new FormatException("Pack inválido.");
        Validate(pack);
        var hashes = pack.Entries.Select(e => e.Sha256.ToUpperInvariant()).Distinct().ToArray();
        var expected = hashes.Select(h => "wavs/" + h + ".wav").Append("manifest.json").ToHashSet(StringComparer.Ordinal);
        if (zip.Entries.Any(e => !expected.Contains(e.FullName)) || zip.Entries.Count != expected.Count)
            throw new FormatException("Conteúdo do pack inválido.");
        var validated = new Dictionary<string, byte[]>();
        foreach (var hash in hashes)
        {
            var entry = zip.GetEntry("wavs/" + hash + ".wav")!;
            if (entry.Length > MaxWavBytes) throw new FormatException("WAV excede 32 MB.");
            using var stream = entry.Open(); var bytes = ReadBounded(stream, MaxWavBytes);
            if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(hash, StringComparison.Ordinal))
                throw new FormatException("IR modificado no arquivo de pack.");
            _ = IrAudio.Decode(bytes); validated.Add(hash, bytes);
        }
        var paths = new Dictionary<string, string>();
        foreach (var (hash, bytes) in validated)
        {
            var original = pack.Entries.First(e => e.Sha256.Equals(hash, StringComparison.OrdinalIgnoreCase));
            // Preserve the musical identity in the shared library, even when the original
            // filename was only "Big", "Clean" or a microphone position inside brand folders.
            var invalid = Path.GetInvalidFileNameChars().ToHashSet();
            string basename = string.Concat(original.Name.Select(c => invalid.Contains(c) ? '_' : c));
            basename = basename[..Math.Min(basename.Length, 160)].Trim().TrimEnd('.', ' ');
            if (System.Text.RegularExpressions.Regex.IsMatch(basename, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                basename = "IR - " + basename;
            string filename = (basename.Length == 0 ? hash : basename) + ".wav";
            string relative = "Imported/" + hash + "/" + filename;
            string target = ResolvePath(relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            string temp = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { File.WriteAllBytes(temp, bytes); File.Move(temp, target, true); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
            paths.Add(hash, relative);
        }
        return pack with { Id = Guid.NewGuid().ToString("N"), Group = "Meus packs",
            Entries = pack.Entries.Select(e => e with { RelativePath = paths[e.Sha256.ToUpperInvariant()] }).ToArray() };
    }
    private static byte[] ReadBounded(Stream stream, int limit)
    {
        using var output = new MemoryStream(); var buffer = new byte[8192]; int read;
        while ((read = stream.Read(buffer)) > 0)
        {
            if (output.Length + read > limit) throw new FormatException("Pack excede o limite de tamanho.");
            output.Write(buffer, 0, read);
        }
        return output.ToArray();
    }
}
