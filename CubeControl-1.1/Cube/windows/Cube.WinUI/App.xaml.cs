using Microsoft.UI.Xaml;
namespace Cube.WinUI;
public partial class App : Application
{
    private static readonly bool Diagnostics = Environment.GetCommandLineArgs().Contains("--diagnostics");
    internal static void Trace(string stage)
    {
        if (Diagnostics)
            System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cube-winui-trace.txt"), stage + "\n");
    }
    private Window? window;
    public App()
    {
        if (Diagnostics)
            AppDomain.CurrentDomain.FirstChanceException += (_, e) => { if (e.Exception is InvalidCastException) LogFailure(new Exception(Environment.StackTrace, e.Exception)); };
        UnhandledException += (_, e) => LogFailure(e.Exception);
        InitializeComponent();
    }
    private static void LogFailure(Exception exception)
    {
        try
        {
            string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CubeControl-WinUI", "Logs");
            System.IO.Directory.CreateDirectory(directory);
            if (Diagnostics) System.IO.File.AppendAllText(System.IO.Path.Combine(directory, "diagnostics.txt"), exception + "\n");
            System.IO.File.WriteAllText(System.IO.Path.Combine(directory, "last-error.txt"), exception.ToString());
        }
        catch (Exception) { /* Logging must not replace the original failure. */ }
    }
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var commandLine = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (commandLine.FirstOrDefault() == "--firmware-worker")
        { Environment.Exit(Cube.Desktop.FirmwareUpdater.RunWorker(commandLine)); return; }
        _ = Platform.UiApplication.Current; // Capture the UI dispatcher before a MIDI worker logs its first message.
        window = new MainWindow();
        window.Closed += (_, _) => window = null;
        window.Activate();
    }
}
