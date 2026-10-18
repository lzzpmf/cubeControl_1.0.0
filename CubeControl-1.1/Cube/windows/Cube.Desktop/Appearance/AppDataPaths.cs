using System.IO;
namespace Cube.Desktop.Appearance;

// Explicit test/simulation override; normal installations retain the Windows profile.
internal static class AppDataPaths
{
    public static string LocalApplicationData
    {
        get
        {
            string? isolated = Environment.GetEnvironmentVariable("CUBECONTROL_TEST_DATA");
            if (string.IsNullOrWhiteSpace(isolated)) return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!Path.IsPathFullyQualified(isolated)) throw new InvalidOperationException("CUBECONTROL_TEST_DATA must be an absolute path.");
            return Path.GetFullPath(isolated);
        }
    }
}
