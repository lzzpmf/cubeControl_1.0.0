using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.ViewModels;

internal static class ClosingChecks
{
    internal static void Run(bool busy)
    {
        var app = new App();
        app.InitializeComponent();
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        var model = new EditorViewModel(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CubeClosingChecks", Guid.NewGuid().ToString("N")));
        model.PackEditor.Name = "Unsaved pack";
        if (!model.PackEditor.Dirty) throw new Exception("Expected unsaved pack");
        model.PackEditor.ChooseDraftDecision = () => throw new Exception("Closing must not prompt separately for the pack");
        if (busy) typeof(EditorViewModel).GetProperty("Busy")!.SetValue(model, true);
        var window = (Window)Activator.CreateInstance(typeof(MainWindow), BindingFlags.Instance | BindingFlags.NonPublic,
            null, new object[] { model, false }, null)!;
        app.MainWindow = window;
        window.ShowInTaskbar = false;
        window.ShowActivated = false;
        window.Opacity = 0;
        int confirmations = 0;
        bool closed = false;
        model.Ask = _ => Task.FromResult(++confirmations > 1);
        window.Closed += (_, _) => closed = true;
        var timeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        timeout.Tick += (_, _) => { timeout.Stop(); throw new Exception("Confirmed exit did not complete"); };
        window.Loaded += (_, _) => window.Dispatcher.BeginInvoke(new Action(() =>
        {
            window.Close();
            if (closed || confirmations != 1) throw new Exception("Cancel must preserve the window after one confirmation");
            Console.WriteLine("PASS: cancel keeps the unsaved pack and window open");
            window.Close();
        }));
        timeout.Start();
        app.Run(window);
        timeout.Stop();
        if (!closed || confirmations != 2) throw new Exception("Exit requires exactly one confirmation per attempt");
        Console.WriteLine("PASS: confirmed exit closes the whole app with an unsaved pack" + (busy ? " while busy" : ""));
    }
}
