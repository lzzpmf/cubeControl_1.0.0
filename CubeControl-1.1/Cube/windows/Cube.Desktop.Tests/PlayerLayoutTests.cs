using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Localization;
using NAudio.Wave;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class PlayerLayoutTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private static IEnumerable<T> Find<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        { var child = VisualTreeHelper.GetChild(parent, i); if (child is T match) yield return match; foreach (var nested in Find<T>(child)) yield return nested; }
    }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-studio-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "fixture.wav");
        using (var writer = new WaveFileWriter(file, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1)))
        {
            float[] samples = Enumerable.Range(0, 48000 * 32).Select(i => (float)(Math.Sin(i * .1) * (.2 + .5 * Math.Abs(Math.Sin(i / 3200d))))).ToArray();
            writer.WriteSamples(samples, 0, samples.Length);
        }
        using var editor = new EditorViewModel(folder);
        var window = new MainWindow(editor, initializeDevices: false) { ShowInTaskbar = false, Width = 1536, Height = 1024 };
        window.Show(); Guid lastSession = Guid.Empty;
        try
        {
            ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
            var model = window.BackingTracks;
            typeof(BackingTracksViewModel).GetMethod("LoadRecordingPlaylist", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(model, null);
            model.AddEmptyStem.Execute(null);
            Check(model.SessionStems!.Count == 1, "new session starts with one empty track");
            await model.InsertLocalAudio(file); model.SelectedClips.Clear(); model.Position = 4;
            Check(model.CutAtCursor.CanExecute(null), "cut finds clip under cursor without selection"); model.CutStudioClips();
            Check(model.SelectedStem!.Clips.Count == 2, "cut keeps both halves"); model.UndoEdit();
            Check(model.SelectedStem!.Clips.Count == 1, "undo restores uncut audio");
            await model.AddLocalFolder(folder); Check(model.LocalFiles.Count == 1, "library lists real audio files");
            model.CategorizeLocalFile(model.LocalFiles[0], "Loops"); model.LocalCategory = "Vocais"; Check(model.LocalFiles.Count == 0, "categories filter assigned metadata");
            model.LocalCategory = "Loops"; Check(model.LocalFiles.Count == 1, "category assignment is available");
            model.LocalSearch = "missing"; Check(model.LocalFiles.Count == 0, "search combines with categories"); model.LocalSearch = "";
            string[] names = ["Bateria", "Baixo", "Guitarra", "Vocal", "Áudio 5"];
            string[] colors = ["#6495ED", "#40BFA0", "#AD63E8", "#E0AE45", "#93A0AA"];
            for (int i = 0; i < 5; i++)
            {
                if (i > 0) model.AddEmptyStem.Execute(null);
                model.SelectedStem!.Name = names[i]; model.SelectedStem.Color = colors[i];
                model.Position = 0;
                if (i > 0 && i < 4) await model.InsertLocalAudio(file);
                if (i < 4)
                {
                    var stem = model.SelectedStem!; var original = stem.Clips[0];
                    model.BeginClipGesture(); stem.Clips.Clear();
                    double split = i == 0 ? 19 : i == 1 ? 17.5 : i == 2 ? 14 : 16;
                    stem.Clips.Add(original.Slice(0, split) with { Name = names[i] + "_verso.wav" });
                    stem.Clips.Add(original.Slice(split,32) with { Name = names[i] + "_refrão.wav" });
                    model.EndClipGesture();
                }
            }
            model.RangeStart = 8; model.RangeEnd = 24; model.IsLooping = true;
            model.InputChoices.Add(new(0,1)); model.InputChoices.Add(new(1,1));
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
            var headerInput = Find<CheckBox>((ListBox)window.FindName("BackingStemList")).First(c => Equals(c.Content,"I"));
            var stemInput = (BackingStem)headerInput.DataContext;
            var mixerInput = Find<CheckBox>((FrameworkElement)window.FindName("SessionMixerContent")).First(c => Equals(c.Content,"I") && c.DataContext == stemInput);
            var suppression = typeof(BackingTracksViewModel).GetField("suppressMonitorChanges", BindingFlags.Instance | BindingFlags.NonPublic)!;
            suppression.SetValue(model,true);
            try
            {
                headerInput.SetCurrentValue(ToggleButton.IsCheckedProperty,true);
                await Dispatcher.Yield(DispatcherPriority.DataBind);
                Check(stemInput.IsMonitoring && mixerInput.IsChecked == true && !stemInput.IsArmed && !model.IsPlaying, "header monitoring updates mixer independently of arm and playback");
                mixerInput.SetCurrentValue(ToggleButton.IsCheckedProperty,false);
                await Dispatcher.Yield(DispatcherPriority.DataBind);
                Check(!stemInput.IsMonitoring && headerInput.IsChecked == false, "mixer monitoring updates header");
            }
            finally { suppression.SetValue(model,false); }
            string[] libraryNames = ["beat_eletronico", "loop_bateria", "baixo_funk", "guitarra_clean", "guitarra_dist", "vocal_masculino", "vocal_feminino", "sintetizador_pad", "percussao_LP", "efeito_transition"];
            foreach (string name in libraryNames)
            {
                using var writer = new WaveFileWriter(Path.Combine(folder,name+".wav"),WaveFormat.CreateIeeeFloatWaveFormat(48000,1));
                writer.WriteSamples(Enumerable.Repeat(.1f,48000*2).ToArray(),0,48000*2);
            }
            await model.AddLocalFolder(folder); model.LocalCategory = "Todos";
            await VerifyStudio(window, TestPaths.Report("C:/CubeControl-Build/reports/backing-studio"));
            stemInput.IsMonitoring = true;
            Check(!stemInput.IsMonitoring && model.HasStatus, "unavailable input clears effective monitoring with feedback");
            var other = new BackingTrack(file); model.Tracks.Add(other); model.SelectedTrack = other; lastSession = other.SessionId;
        }
        finally { window.BackingTracks.Dispose(); window.Close(); }
        using var reopened = new BackingTracksViewModel(folder);
        typeof(BackingTracksViewModel).GetMethod("LoadRecordingPlaylist", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(reopened,null);
        typeof(BackingTracksViewModel).GetMethod("RestoreStudio", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(reopened,null);
        await reopened.AddLocalFolder(folder); reopened.LocalCategory = "Loops";
        Check(reopened.SelectedTrack?.SessionId == lastSession && reopened.LocalFiles.Count == 1, "last session and local categories survive reopening");
        Check(reopened.SessionStems!.All(s => !s.IsArmed && !s.IsMonitoring), "restored tracks stay disarmed and unmonitored");
    }
    internal static async Task VerifyStudio(MainWindow window, string reports)
    {
        Directory.CreateDirectory(reports);
        var oldTheme = ThemeService.Instance.Mode; string oldLanguage = LanguageService.Instance.Language; int oldZoom = ZoomService.Instance.Percent;
        try
        {
            foreach (var scenario in new[] { (1536d,1024d,100,ThemeMode.Dark,"pt-BR"), (1536d,1024d,100,ThemeMode.Light,"en-US"), (640d,480d,100,ThemeMode.Dark,"pt-BR"), (640d,480d,200,ThemeMode.Light,"en-US") })
            {
                window.Width = scenario.Item1; window.Height = scenario.Item2;
                ZoomService.Instance.Preview(scenario.Item3); ThemeService.Instance.Preview(scenario.Item4); LanguageService.Instance.Preview(scenario.Item5);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                var page = (Grid)window.FindName("BackingPage"); var arrangement = (Grid)window.FindName("BackingArrangement"); var mixer = (Border)window.FindName("BackingVolumesPanel");
                var toolbar = (FrameworkElement)window.FindName("BackingTransportHost"); var list = (ListBox)window.FindName("BackingStemList");
                Check(mixer.IsVisible && mixer.ActualHeight > 0, "mixer remains visible at compact size and zoom");
                Console.WriteLine($"LAYOUT {scenario} page={page.RenderSize} arrangement={arrangement.ActualHeight} mixer={mixer.ActualHeight}");
                Check(Math.Abs(arrangement.ActualHeight / mixer.ActualHeight - 59d / 41) < .08, "arrangement and mixer retain reference proportions");
                Check(Math.Abs(toolbar.ActualHeight - 68) < 1, "toolbar does not shrink its controls");
                var lane = Find<BackingClipLane>(list).First(); var wave = (BackingWaveform)window.FindName("BackingMainWave");
                Check(Math.Abs(lane.TranslatePoint(new Point(), page).X - wave.TranslatePoint(new Point(), page).X) < 1, "ruler aligns with clip lanes");
                var inputs = Find<CheckBox>(list).Where(c => Equals(c.Content, "I")).ToArray();
                var mixerInputs = Find<CheckBox>((FrameworkElement)window.FindName("SessionMixerContent")).Where(c => Equals(c.Content, "I")).ToArray();
                Check(inputs.Length > 0 && mixerInputs.Length == window.BackingTracks.SessionStems!.Count, "monitor buttons exist per track and mixer channel");
                var stem = (BackingStem)inputs[0].DataContext; stem.CanEditRouting = false;
                inputs[0].SetCurrentValue(ToggleButton.IsCheckedProperty, true); window.UpdateLayout(); Check(!stem.IsMonitoring, "routing lock rejects monitoring changes");
                inputs[0].GetBindingExpression(ToggleButton.IsCheckedProperty)!.UpdateTarget(); stem.CanEditRouting = true;
                var library = (FrameworkElement)window.FindName("BackingLibraryLayout");
                Check(Grid.GetColumn(library) == (page.ActualWidth >= 1200 ? 1 : 0), "library switches between dock and overlay");
                var drawing = new DrawingVisual(); using (var dc = drawing.RenderOpen()) { dc.DrawRectangle((Brush)window.FindResource("CubeCanvas"), null, new Rect(window.RenderSize)); dc.DrawRectangle(new VisualBrush(window), null, new Rect(window.RenderSize)); }
                var image = new RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth), (int)Math.Ceiling(window.ActualHeight),96,96,PixelFormats.Pbgra32); image.Render(drawing);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image)); using var output = File.Create(Path.Combine(reports,$"studio-{scenario.Item1}-{scenario.Item3}-{scenario.Item4}.png")); encoder.Save(output);
            }
        }
        finally { ZoomService.Instance.Preview(oldZoom); ThemeService.Instance.Preview(oldTheme); LanguageService.Instance.Preview(oldLanguage); }
    }
}
