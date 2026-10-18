using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Cube.Qa;
using static Cube.Qa.QaContext;

internal static class QaUsability
{
    private static readonly List<string> unhandled = [];
    private static string? currentDirectory;
    internal static Task RunAll()
    {
        string batteries = string.Join(",", Cases().SelectMany(c => c.Batteries).Distinct());
        string output = Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "qa-results");
        int result = RunMain(["--qa", "--batteries", batteries, "--results", output]);
        if (result != 0) throw new InvalidOperationException("QA cases failed; see " + output);
        return Task.CompletedTask;
    }
    public static int RunMain(string[] args)
    {
        if (args.Contains("--list")) return QaRunner.Run("usability", args, Cases()).GetAwaiter().GetResult();
        var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown }; app.InitializeComponent();
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        app.DispatcherUnhandledException += (_, e) =>
        {
            RecordUnhandled("Dispatcher", e.Exception);
            // Keep the harness alive to capture evidence and FAIL this case, never silently pass it.
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) => RecordUnhandled("Fatal", e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, e) => RecordUnhandled("UnobservedTask", e.Exception);
        const string probeId = "exception-probe";
        var cases = args.Contains("--exception-probe")
            ? new[] { Test(probeId, "Deliberate dispatcher exception must fail", "QA-RES-001", async j =>
                {
                    _ = j.Window.Dispatcher.BeginInvoke(new Action(() => throw new InvalidOperationException("QA_EXPECTED_DISPATCHER_PROBE")));
                    await Task.Delay(100);
                }) }
            : Cases();
        Task<int> run = QaRunner.Run("usability", args, cases);
        _ = run.ContinueWith(_ => app.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background), TaskScheduler.Default);
        Dispatcher.Run();
        try { return run.GetAwaiter().GetResult(); }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static void RecordUnhandled(string kind, object error)
    {
        lock (unhandled)
        {
            string message = kind + ": " + error; unhandled.Add(message);
            Console.Error.WriteLine(message);
            if (currentDirectory != null) File.AppendAllText(Path.Combine(currentDirectory, "unhandled.log"), message + Environment.NewLine);
        }
    }
    private static QaCase Test(string id, string name, string batteries, Func<Journey, Task> action) =>
        new(id, name, batteries.Split(','), async c =>
        {
            currentDirectory = c.DirectoryPath; int errors = unhandled.Count;
            var trace = new BindingTrace(); PresentationTraceSources.DataBindingSource.Listeners.Add(trace);
            using var model = new EditorViewModel(c.DirectoryPath);
            var shell = new MainWindow(model, initializeDevices: false) { Width = 1280, Height = 800, ShowInTaskbar = false };
            Application.Current.MainWindow = shell;
            var journey = new Journey(c, shell, model);
            try
            {
                LanguageService.Instance.Preview("pt-BR");
                shell.Show(); await Settle(shell); journey.Step("Janela real aberta; dispositivos desativados");
                await action(journey); await Settle(shell);
                Check(unhandled.Count == errors, "Unhandled application exception; see unhandled.log");
                journey.Capture("final");
            }
            catch { if (shell.IsVisible) journey.Capture("failure"); throw; }
            finally
            {
                // Dismiss test-owned UI only; never persist user preferences.
                foreach (var dialog in Find<Wpf.Ui.Controls.ContentDialog>(shell).ToArray()) dialog.Hide();
                foreach (Window child in shell.OwnedWindows.Cast<Window>().ToArray()) child.Close();
                model.Ask = _ => Task.FromResult(true);
                if (shell.IsVisible) shell.Close();
                await Settle(shell);
                c.Evidence("steps.json", journey.Steps);
                c.Evidence("bindings.json", trace.Messages);
                c.Evidence("exceptions.json", unhandled.Skip(errors).ToArray());
                PresentationTraceSources.DataBindingSource.Listeners.Remove(trace);
                currentDirectory = null;
            }
            Check(!shell.IsVisible, "Window remained open after cleanup");
            Check(unhandled.Count == errors, "Unhandled exception during closing; see exceptions.json");
        });

    private static QaCase[] Cases() =>
    [
        Test("ux-navigation", "Every page pair through real navigation controls", "QA-NAV-001,QA-NAV-002,QA-NAV-003", async j =>
        {
            await j.Connect(); j.Model.Current.Volume.Value = 35; await j.Model.Flush();
            for (int from = 0; from < 5; from++) for (int to = 0; to < 5; to++)
            {
                await j.Navigate(from); await j.Navigate(to);
                Check(j.Model.Dirty && j.Model.Current.Volume.Value == 35, "Navigation lost pending edits");
                Check(j.Window.OwnedWindows.Count == 0, "Navigation spawned an unexpected window");
            }
            j.Capture("navigation");
        }),
        Test("ux-responsive", "Page navigation stays reachable at supported window sizes", "QA-UI-001,QA-UI-002,QA-NAV-001", async j =>
        {
            var geometry = new List<object>();
            foreach (var size in new[] { (640d, 480d), (920d, 740d), (1280d, 800d) })
            {
                j.Window.Width = size.Item1; j.Window.Height = size.Item2;
                for (int page = 0; page < 5; page++)
                {
                    await j.Navigate(page);
                    foreach (ListBoxItem item in j.Navigation.Items)
                    {
                        var origin = item.TranslatePoint(new Point(), j.Window);
                        bool visible = item.IsVisible && item.ActualWidth > 0 && item.ActualHeight > 0 && origin.X >= -1 && origin.Y >= -1
                            && origin.X + item.ActualWidth <= j.Window.ActualWidth + 1 && origin.Y + item.ActualHeight <= j.Window.ActualHeight + 1;
                        geometry.Add(new { size, page, item.ActualWidth, item.ActualHeight, origin.X, origin.Y, visible });
                        j.Context.Evidence("geometry.json", geometry);
                        Check(visible, $"Navigation target clipped at {size}, page {page}");
                    }
                    j.Capture($"size-{size.Item1}-{size.Item2}-page-{page}");
                }
            }
            j.Context.Evidence("geometry.json", geometry);
        }),
        Test("ux-gallery", "Repeated gallery opening, cancel and return preserve edits", "QA-NAV-002,QA-CONC-001,QA-STA-003", async j =>
        {
            await j.Connect(); j.Model.SelectedStage = 0;
            byte[] bank = j.Fake!.Bank.ToArray();
            var trigger = (ButtonBase)j.Window.FindName("EquipmentPickerCard");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                trigger.BringIntoView(); await Settle(j.Window); Invoke(trigger); Invoke(trigger);
                await Until(() => Find<Wpf.Ui.Controls.ContentDialog>(j.Window).Any(d => d.IsVisible));
                var dialogs = Find<Wpf.Ui.Controls.ContentDialog>(j.Window).Where(d => d.IsVisible).ToArray();
                Check(dialogs.Length == 1, "Repeated click created multiple dialogs");
                j.Capture("gallery-" + cycle); j.Step("Galeria aberta duas vezes; somente um diálogo");
                var cancel = Find<ButtonBase>(dialogs[0]).First(b => b.IsVisible && b.Content?.ToString() == "Cancelar");
                Invoke(cancel); await Until(() => !dialogs[0].IsVisible);
                Check(j.Fake.Bank.SequenceEqual(bank), "Cancel modified bank");
                await j.Navigate(4); await j.Navigate(0);
            }
        }),
        Test("ux-dirty-close", "Cancel real close confirmation then keep editing", "QA-MSG-001,QA-STA-003,QA-NAV-002", async j =>
        {
            await j.Connect(); j.Model.Current.Volume.Value = 41; await j.Model.Flush();
            _ = j.Window.Dispatcher.BeginInvoke(new Action(j.Window.Close));
            await Until(() => j.Window.OwnedWindows.Cast<Window>().Any(w => w.IsVisible));
            var dialog = j.Window.OwnedWindows.Cast<Window>().Single(w => w.IsVisible);
            Check(ReferenceEquals(dialog.Owner, j.Window), "Confirmation has no correct owner");
            j.Capture("close-confirmation");
            var cancel = Find<ButtonBase>(dialog).First(b => b.IsVisible && b.Content?.ToString() == "Cancelar");
            Invoke(cancel); await Until(() => !dialog.IsVisible);
            Check(j.Window.IsVisible && j.Model.Dirty, "Cancel closed window or discarded edit");
            await j.Navigate(1); await j.Navigate(0);
            Check(j.Model.Current.Volume.Value == 41, "Close cancellation lost value");
            j.Step("Fechamento cancelado; edição e navegação preservadas");
        }),
        Test("ux-busy-close", "Close during operation shows notice and permits recovery", "QA-CONC-001,QA-MSG-001,QA-RES-001", async j =>
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var operation = j.Model.Execute(() => release.Task);
            try
            {
                _ = j.Window.Dispatcher.BeginInvoke(new Action(j.Window.Close));
                await Until(() => j.Window.OwnedWindows.Cast<Window>().Any(w => w.IsVisible));
                Check(j.Window.IsVisible && j.Model.Busy, "Closed during pending operation");
                var notice = j.Window.OwnedWindows.Cast<Window>().Single(w => w.IsVisible);
                Invoke(Find<ButtonBase>(notice).First(b => b.IsVisible && b.Content?.ToString() == "Fechar"));
                await Until(() => !notice.IsVisible); j.Step("Fechar durante operação informa bloqueio");
            }
            finally { release.TrySetResult(); await operation; }
            Check(!j.Model.Busy && j.Window.IsVisible, "UI stuck after operation");
            await j.Navigate(4);
        }),
        Test("ux-error-recovery", "Visible error feedback and navigation after invalid operation", "QA-MSG-002,QA-RES-001,QA-STA-002", async j =>
        {
            await j.Model.Execute(() => throw new IOException("QA: arquivo indisponível")); await Settle(j.Window);
            Check(!j.Model.Busy && j.Model.HasError, "Operation failed without recoverable error state");
            var error = Find<Wpf.Ui.Controls.InfoBar>(j.Window).FirstOrDefault(b => b.IsOpen && b.IsVisible && b.Severity == Wpf.Ui.Controls.InfoBarSeverity.Error);
            Check(error != null && error.Message?.ToString()?.Contains("arquivo indisponível") == true, "Error is not visible to user");
            j.Capture("error-visible");
            await j.Navigate(4); await j.Navigate(0);
            bool ran = false; await j.Model.Execute(() => { ran = true; return Task.CompletedTask; });
            Check(ran && !j.Model.Busy, "Error blocked future actions"); j.Step("Erro apresentado; próxima ação executada");
        }),
        Test("ux-keyboard", "Keyboard edits real knob, preserves focus and preset target", "QA-INP-002,QA-ACC-002,QA-UX-001", async j =>
        {
            await j.Connect();
            var knob = (Knob)j.Window.FindName("PresetVolumeKnob"); knob.BringIntoView(); await Settle(j.Window);
            knob.Focus();
            Check(knob.IsKeyboardFocused, "Editable knob cannot receive keyboard focus");
            j.Model.Current.Volume.Value = 40;
            KeyPress(knob, Key.Right); await j.Model.Flush();
            Check(j.Model.Current.Volume.Value > 40, "Right arrow did not edit focused knob");
            double value = j.Model.Current.Volume.Value;
            await j.Navigate(2); await j.Navigate(0);
            Check(j.Model.Current.Volume.Value == value, "Keyboard edit disappeared on return");
            var buttons = Find<ButtonBase>(j.Window).Where(b => b.IsVisible && b.Command == j.Model.Save).ToArray();
            Check(buttons.Length > 0 && buttons.Any(b => b.IsEnabled), "Edited preset offers no Save action");
            Invoke(buttons.First(b => b.IsEnabled)); await Until(() => !j.Model.Busy && !j.Model.Dirty);
            j.Step("Teclado alterou controle; salvar pela UI concluiu");
        }),
        Test("ux-effect-cards", "Advanced and simple faces share a card without child windows", "QA-NAV-002,QA-UI-001,QA-CONC-001", async j =>
        {
            await j.Connect();
            for (int stage = 1; stage <= 4; stage++)
            {
                j.Model.SelectedStage = stage; await Settle(j.Window);
                var trigger = (ButtonBase)j.Window.FindName("EffectCardToggle");
                for (int cycle = 0; cycle < 2; cycle++)
                {
                    Invoke(trigger); await Until(() => j.Model.IsAdvancedCard && !j.Model.Busy);
                    Check(j.Window.OwnedWindows.Count == 0, "Card flip opened a child window");
                    j.Capture($"effect-{stage}-{cycle}");
                    Invoke(trigger); await Until(() => j.Model.IsSimpleCard);
                    j.Step($"Effect {stage}: advanced and back to simple ({cycle})");
                }
            }
        }),
        Test("ux-seeded-actions", "Reproducible unusual action ordering across pages", "QA-NAV-003,QA-CONC-001,QA-REG-001", async j =>
        {
            const int seed = 20261003; var random = new Random(seed); await j.Connect();
            j.Context.Evidence("seed.json", new { seed, actions = 80 });
            for (int i = 0; i < 80; i++)
            {
                int action = random.Next(4);
                j.Step($"Iniciar ação {i}, tipo {action}, seed {seed}");
                if (action == 0) await j.Navigate(random.Next(5));
                else if (action == 1)
                {
                    await j.Navigate(0);
                    var presets = (ListBox)j.Window.FindName("PresetSelector");
                    presets.SelectedIndex = random.Next(3); j.Step("Selecionar preset " + presets.SelectedIndex);
                }
                else if (action == 2)
                {
                    await j.Navigate(0); var knob = (Knob)j.Window.FindName("PresetVolumeKnob");
                    knob.BringIntoView(); await Settle(j.Window); KeyPress(knob, random.Next(2) == 0 ? Key.Left : Key.Right);
                    j.Step("Seta no volume");
                }
                else
                {
                    var button = (ButtonBase)j.Window.FindName("MoreOptionsButton");
                    await Task.Delay(350); Invoke(button); await Until(() => button.ContextMenu?.IsOpen == true);
                    Check(button.ContextMenu?.IsOpen == true, "Options did not open");
                    button.ContextMenu!.IsOpen = false; await Task.Delay(350); j.Step("Abrir e fechar opções");
                }
                await j.Model.Flush();
                Check(!j.Model.Busy && j.Navigation.SelectedIndex == j.Tabs.SelectedIndex && j.Tabs.SelectedIndex >= 0, "Invalid navigation or stuck Busy");
                Check(j.Window.OwnedWindows.Count == 0, "Unexplained owned window after action");
            }
        })
    ];

    private sealed class Journey(QaContext context, MainWindow window, EditorViewModel model)
    {
        public QaContext Context => context;
        public MainWindow Window => window;
        public EditorViewModel Model => model;
        public QaConnection? Fake { get; private set; }
        public List<object> Steps { get; } = [];
        public ListBox Navigation => (ListBox)window.FindName("Navigation");
        public TabControl Tabs => (TabControl)window.FindName("Tabs");
        public void Step(string action) { Steps.Add(new { index = Steps.Count, action, page = Tabs.SelectedIndex, model.Busy, model.Dirty }); context.Evidence("steps.json", Steps); }
        public async Task Connect()
        {
            Fake = new QaConnection(); var session = new CubeSession(Fake) { Timeout = TimeSpan.FromMilliseconds(150) };
            await session.Identify(); await model.AttachSession(session); await Settle(window); Step("Conexão simulada pronta");
        }
        public async Task Navigate(int page)
        {
            var item = (ListBoxItem)Navigation.Items[page];
            var peer = UIElementAutomationPeer.CreatePeerForElement(Navigation)!.GetChildren()[page];
            ((ISelectionItemProvider)peer.GetPattern(PatternInterface.SelectionItem)!).Select();
            await Settle(window);
            Check(Tabs.SelectedIndex == page && Navigation.SelectedIndex == page, "Navigation disagrees with visible page");
            Check(Tabs.SelectedContent is FrameworkElement { IsVisible: true }, "Selected page is empty/invisible");
            Step("Navegar para " + page);
        }
        public void Capture(string name)
        {
            window.UpdateLayout();
            var bitmap = new RenderTargetBitmap(Math.Max(1, (int)window.ActualWidth), Math.Max(1, (int)window.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window); var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var output = File.Create(context.FilePath(name + ".png")); png.Save(output);
            foreach (Window child in window.OwnedWindows) if (child.IsVisible)
            {
                var image = new RenderTargetBitmap(Math.Max(1, (int)child.ActualWidth), Math.Max(1, (int)child.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                image.Render(child); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
                using var file = File.Create(context.FilePath(name + "-dialog.png")); encoder.Save(file);
            }
        }
    }
    private static async Task Settle(Window window)
    {
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); await Task.Delay(40);
    }
    private static async Task Until(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition()) { if (DateTime.UtcNow >= deadline) throw new TimeoutException("UI did not reach expected state"); await Task.Delay(25); }
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
    }
    private static void Invoke(ButtonBase button)
    {
        Check(button.IsVisible && button.IsEnabled, "User action target is hidden/disabled: " + button.Name);
        var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
        ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)!).Invoke();
    }
    private static void KeyPress(UIElement element, Key key) => element.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
        PresentationSource.FromVisual(element), 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }
    private sealed class BindingTrace : TraceListener
    {
        public List<string> Messages { get; } = [];
        public override void Write(string? message) { if (message != null) Messages.Add(message); }
        public override void WriteLine(string? message) => Write(message);
    }
}



