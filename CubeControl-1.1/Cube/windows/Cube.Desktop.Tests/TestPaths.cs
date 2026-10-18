using System.IO;

internal static class TestPaths
{
    // Preserve standalone legacy destinations; the runner isolates every generated artifact.
    internal static string Report(string legacyPath)
    {
        string? root = Environment.GetEnvironmentVariable("CUBECONTROL_TEST_DATA");
        string path = string.IsNullOrEmpty(root) ? legacyPath
            : Path.Combine(root, "reports", legacyPath["C:/CubeControl-Build/".Length..]);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return path;
    }
}
