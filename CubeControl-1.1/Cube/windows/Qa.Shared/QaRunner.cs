using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace Cube.Qa;

internal sealed record QaCase(string Id, string Name, string[] Batteries, Func<QaContext, Task> Run);
internal sealed record QaResult(string Id, string Name, string[] Batteries, string Status, double Seconds, string? Error);

internal sealed class QaContext(string directory)
{
    public string DirectoryPath { get; } = directory;
    public string FilePath(string name) => Path.Combine(DirectoryPath, name);
    public void Evidence(string name, object value) => File.WriteAllText(FilePath(name), JsonSerializer.Serialize(value, QaRunner.Json));
    public static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
    public static async Task Reject<T>(Func<Task> action) where T : Exception
    {
        try { await action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}; operation succeeded.");
    }
    public static void Reject<T>(Action action) where T : Exception => Reject<T>(() => { action(); return Task.CompletedTask; }).GetAwaiter().GetResult();
}

internal static class QaRunner
{
    internal static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public static async Task<int> Run(string host, string[] args, IReadOnlyList<QaCase> cases)
    {
        string? Value(string key) { int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        if (args.Contains("--list"))
        {
            Console.WriteLine(JsonSerializer.Serialize(cases.Select(c => new { c.Id, c.Name, c.Batteries }), Json));
            return 0;
        }
        var requested = (Value("--batteries") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.Ordinal);
        if (requested.Count == 0 || Value("--results") is not { } output)
        { Console.Error.WriteLine("Use --qa --batteries QA-XXX-001,... --results <new-directory>, or --qa --list."); return 2; }
        var selected = cases.Where(c => c.Batteries.Any(requested.Contains)).ToArray();
        if (selected.Length == 0) { Console.Error.WriteLine("No implemented cases match the selection in this host."); return 2; }
        output = Path.GetFullPath(output);
        if (Directory.Exists(output) || File.Exists(output)) { Console.Error.WriteLine("Results directory must not already exist."); return 2; }
        Directory.CreateDirectory(output);
        var results = new List<QaResult>();
        var started = DateTimeOffset.UtcNow;
        // Persist after every case. A killed worker leaves completed cases plus its current ID.
        void Save(string? running = null)
        {
            var report = new { Schema = 1, Host = host, StartedUtc = started, UpdatedUtc = DateTimeOffset.UtcNow,
                Runtime = Environment.Version.ToString(), OS = Environment.OSVersion.ToString(), Culture = CultureInfo.CurrentCulture.Name,
                HardwareAccess = false, Scope = "Automated subset only; not certification of the entire battery", Requested = requested,
                Running = running, Cases = results };
            File.WriteAllText(Path.Combine(output, "results.json.tmp"), JsonSerializer.Serialize(report, Json));
            File.Move(Path.Combine(output, "results.json.tmp"), Path.Combine(output, "results.json"), true);
            var suite = new XElement("testsuite", new XAttribute("name", host), new XAttribute("tests", results.Count),
                new XAttribute("failures", results.Count(r => r.Status == "FAIL")), results.Select(r =>
                new XElement("testcase", new XAttribute("name", r.Id + " " + r.Name), new XAttribute("classname", host),
                    new XAttribute("time", r.Seconds.ToString("R", CultureInfo.InvariantCulture)),
                    r.Error == null ? null : new XElement("failure", r.Error))));
            new XDocument(suite).Save(Path.Combine(output, "junit.xml"));
        }
        foreach (var test in selected)
        {
            Save(test.Id);
            var folder = Path.Combine(output, test.Id); Directory.CreateDirectory(folder);
            var clock = Stopwatch.StartNew(); string? error = null;
            Console.WriteLine("RUN " + test.Id + " " + test.Name);
            try { await test.Run(new QaContext(folder)); }
            catch (Exception ex) { error = ex.ToString(); File.WriteAllText(Path.Combine(folder, "error.txt"), error); }
            results.Add(new(test.Id, test.Name, test.Batteries, error == null ? "PASS" : "FAIL", clock.Elapsed.TotalSeconds, error));
            Console.WriteLine((error == null ? "PASS " : "FAIL ") + test.Id);
            Save();
        }
        return results.Any(r => r.Status == "FAIL") ? 1 : 0;
    }
}
