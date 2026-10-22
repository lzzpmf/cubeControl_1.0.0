using System.IO;
using System.Security.Cryptography;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private CabinetCaptureStore? captureStore;
    private CabinetCaptureStore Captures => captureStore ??= new(Path.Combine(DataDirectory, "cabinet-captures-v1.json"));
    private readonly HashSet<string> captureLookups = [];
    private readonly SemaphoreSlim captureLookupGate = new(1);

    private static CabinetCapture? MatchCapture(IrLibraryEntry[] candidates, byte[] sector, int? volume = null)
    {
        var matches = new List<CabinetCapture>();
        foreach (var entry in candidates)
        {
            if (entry.Metadata is not { } metadata) continue;
            try
            {
                if (new FileInfo(entry.FullPath).Length > 32 * 1024 * 1024) continue;
                var wav = File.ReadAllBytes(entry.FullPath);
                if (!Convert.ToHexString(SHA256.HashData(wav)).Equals(metadata.Sha256, StringComparison.OrdinalIgnoreCase)) continue;
                var audio = IrAudio.Decode(wav);
                for (int level = volume ?? 0; level <= (volume ?? 99); level++)
                    if (audio.Sector(level).AsSpan().SequenceEqual(sector))
                    {
                        matches.Add(new(metadata, Path.GetFileName(entry.FullPath)));
                        break;
                    }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or ArgumentException) { }
        }
        var distinct = matches.DistinctBy(c => (c.Title, c.Speaker, c.Summary, c.Hint)).ToArray();
        return distinct.Length == 1 ? distinct[0] : null;
    }

    private Task<CabinetCapture?> CaptureForWriteAsync(string path, byte[] sector, int volume)
    {
        var candidates = IrLibrary.Entries.Where(e => e.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)
            || e.RelativePath.Equals(path.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase)).ToArray();
        return Task.Run(() => MatchCapture(candidates, sector, volume));
    }

    private void RememberCapture(string hash, CabinetCapture? capture)
    {
        if (capture == null) return;
        try { Captures.Set(hash, capture); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { NotifyFailure("IR identificado, mas não foi possível salvar os metadados: " + ex.Message); }
    }

    private async Task RecoverCaptureAsync(string hash, byte[] sector, string? name)
    {
        if (Captures.Get(hash) != null || string.IsNullOrWhiteSpace(name)) return;
        // Restrict old recordings to named candidates; never scan the whole audio library on connection.
        var candidates = IrLibrary.Entries.Where(e => e.Metadata != null &&
            (name.Equals(e.Name, StringComparison.OrdinalIgnoreCase) ||
             name.Equals(IrViewModel.DisplayName(e.FullPath, e.Collection), StringComparison.OrdinalIgnoreCase) ||
             name.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase))).ToArray();
        if (candidates.Length == 0 || !captureLookups.Add(hash)) return;
        await captureLookupGate.WaitAsync();
        try
        {
            var capture = await Task.Run(() => MatchCapture(candidates, sector));
            // A write may have identified this content while the recovery was running.
            if (capture == null || Captures.Get(hash) != null) return;
            RememberCapture(hash, capture);
            if (!disposed && inventory.Slots.Any(c => c.Hash == hash)) RebuildCabinets();
        }
        finally { captureLookupGate.Release(); }
    }
}
