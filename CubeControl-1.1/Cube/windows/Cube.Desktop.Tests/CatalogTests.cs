using System.Diagnostics;
using System.Text.Json;

internal static class CatalogTests
{
    public static async Task Run()
    {
        const string root = "--documentation-root=C:\\Test package\\Assets\\Documents";
        foreach (string[] args in new[] { new[] { "--documentation-only", root }, new[] { "--suite", "documentation", root } })
        {
            var suite = TestCatalog.Resolve(args, out var normalized);
            if (suite?.Id != "documentation" || !normalized.Skip(1).SequenceEqual(new[] { "--documentation-only", root }))
                throw new Exception("Documentation root must reach the suite unchanged, including spaces.");
        }
        foreach (string[] args in new[] { new[] { root }, new[] { "--theme-only", root }, new[] { "--documentation-only", "--documentation-root=" }, new[] { "--documentation-only", root, root } })
            if (TestCatalog.Resolve(args, out _) != null) throw new Exception("Invalid documentation options accepted.");
        foreach (var expected in TestCatalog.Suites)
        {
            foreach (string[] args in new[] { new[] { expected.Alias }, new[] { "--suite", expected.Id } })
                if (TestCatalog.Resolve(args, out var normalized) != expected || !normalized.Skip(1).SequenceEqual(new[] { expected.Alias }))
                    throw new Exception("Suite selector lost its legacy execution mode: " + expected.Id);
        }
        if (TestCatalog.Resolve([], out var defaults)?.Id != "default" || defaults[1] != "--default")
            throw new Exception("No arguments must retain the default integration suite.");
        var listed = await Invoke("--list");
        if (listed.Code != 0) throw new Exception("Discovery failed: " + listed.Error);
        using var document = JsonDocument.Parse(listed.Output);
        var ids = document.RootElement.EnumerateArray().Select(e => e.GetProperty("Id").GetString()).ToArray();
        if (ids.Length == 0 || ids.Any(string.IsNullOrWhiteSpace) || ids.Distinct().Count() != ids.Length) throw new Exception("Suite IDs must be nonempty and unique.");
        var aliases = document.RootElement.EnumerateArray().Select(e => e.GetProperty("Alias").GetString()).ToArray();
        if (aliases.Any(a => a == null || !a.StartsWith("--") || a.Length <= 2) || aliases.Distinct().Count() != aliases.Length)
            throw new Exception("Suite aliases must be valid and unique.");
        foreach (var entry in document.RootElement.EnumerateArray())
        {
            if (string.IsNullOrWhiteSpace(entry.GetProperty("Description").GetString()) ||
                entry.GetProperty("Requirement").GetString() is not ("local" or "hardware" or "audio-device" or "online" or "elevation") ||
                entry.GetProperty("Wpf").ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw new Exception("Invalid catalog metadata: " + entry);
        }
        foreach (string[] args in new[] { new[] { "--unknown" }, new[] { "--suite" }, new[] { "--suite", "missing" }, new[] { "--suite", "" }, new[] { "--suite", "theme", "--borderless" }, new[] { "--theme-only", "--borderless" }, new[] { "--list", "--theme-only" }, new[] { "--hardware-advanced-recovery" }, new[] { "--suite", "hardware-advanced-recovery" }, new[] { "--theme-only", root }, new[] { "--documentation-only", "--documentation-root=" }, new[] { "--documentation-only", root, root } })
        {
            var result = await Invoke(args);
            if (result.Code != 2 || string.IsNullOrWhiteSpace(result.Error)) throw new Exception("Invalid CLI must fail explicitly: " + string.Join(' ', args));
        }
        var canonical = await Invoke("--suite", "midi-ports");
        var legacy = await Invoke("--midi-ports-only");
        if (canonical.Code != 0 || legacy.Code != 0 || string.IsNullOrWhiteSpace(canonical.Output) || canonical.Output != legacy.Output)
            throw new Exception("Canonical selector and legacy alias must execute the same local suite: " + canonical.Error + legacy.Error);
        Console.WriteLine("PASS catalog is discoverable; invalid and conflicting arguments are rejected before WPF startup");
    }

    private static async Task<(int Code, string Output, string Error)> Invoke(params string[] args)
    {
        var info = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string arg in args) info.ArgumentList.Add(arg);
        using var process = Process.Start(info)!;
        var output = process.StandardOutput.ReadToEndAsync(); var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch { if (!process.HasExited) process.Kill(true); throw; }
        return (process.ExitCode, await output, await error);
    }
}
