using System.Diagnostics;
using System.Text.Json;

internal static class CatalogTests
{
    public static async Task Run()
    {
        var listed = await Invoke("--list");
        if (listed.Code != 0) throw new Exception("Discovery failed: " + listed.Error);
        using var document = JsonDocument.Parse(listed.Output);
        var ids = document.RootElement.EnumerateArray().Select(e => e.GetProperty("Id").GetString()).ToArray();
        if (ids.Length == 0 || ids.Distinct().Count() != ids.Length) throw new Exception("Suite IDs must be nonempty and unique.");
        foreach (string[] args in new[] { new[] { "--unknown" }, new[] { "--suite" }, new[] { "--suite", "missing" }, new[] { "--theme-only", "--borderless" }, new[] { "--list", "--theme-only" } })
        {
            var result = await Invoke(args);
            if (result.Code != 2 || string.IsNullOrWhiteSpace(result.Error)) throw new Exception("Invalid CLI must fail explicitly: " + string.Join(' ', args));
        }
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
