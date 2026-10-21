using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using GongSolutions.Wpf.DragDrop;
using GongDrag = GongSolutions.Wpf.DragDrop.DragDrop;

internal static class DragDropTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        var items = new ObservableCollection<object> { new TextBlock { Text = "First" }, new TextBlock { Text = "Second" }, new TextBlock { Text = "Third" } };
        var list = new ListBox { ItemsSource = items };
        var window = new Window { Content = list, Width = 600, Height = 400, ShowInTaskbar = false };
        window.Show(); window.UpdateLayout();
        try
        {
            list.SelectedItem = items[0];
            var selected = list.SelectedItem;
            var animate = DragMotion.CaptureList(list); items.Move(0, 2); animate();
            Check(list.SelectedItem == selected && list.Items[2] == selected, "list move retains selection and data identity");
            await Task.Delay(260);
            Check(DragMotion.ActiveCount == 0, "list flights release their overlays");
            var container = (FrameworkElement)list.ItemContainerGenerator.ContainerFromIndex(0);
            container.SetBinding(UIElement.OpacityProperty, new Binding("Tag") { Source = new Border { Tag = .8 } });
            var adapter = CubeDragDrop.Attach(list, info => (info.SourceItem, DragDropEffects.Move));
            var drag = Proxy<IDragInfo>(("SourceItem", items[0]), ("VisualSource", list), ("VisualSourceItem", container), ("MouseButton", MouseButton.Left));
            adapter.StartDrag(drag); adapter.DragCancelled();
            Check(container.Opacity == .8 && BindingOperations.IsDataBound(container, UIElement.OpacityProperty) && !container.HasAnimatedProperties,
                "cancel restores source opacity without removing its binding");
            adapter.StartDrag(drag); adapter.DragDropOperationFinished(DragDropEffects.None, drag);
            Check(!DragFeedback.IsVisible && container.Opacity == .8, "outside drop cleans feedback and source");
            Check(CubeDragDrop.IsInteractive(new Button(), list) && CubeDragDrop.IsInteractive(new Knob(), list) &&
                CubeDragDrop.IsInteractive(new TextBox(), list), "interactive children do not initiate a drag");
            DragMotion.Settle(container); window.Hide();
            Check(DragMotion.ActiveCount == 0, "hidden view releases settling animation");
        }
        finally { window.Close(); }

        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-drag-" + Guid.NewGuid().ToString("N")));
        var editor = model.PackEditor;
        await editor.SelectAsync(null);
        var entries = editor.PackLibrary.Entries.Take(2).ToArray();
        editor.AssignIr(0, entries[0]); editor.AssignIr(1, entries[1]);
        editor.Slots[0].DistanceValue = 19; editor.Slots[1].DistanceValue = 83;
        var view = new IrPackEditor { DataContext = editor };
        window = new Window { Content = view, Width = 1200, Height = 850, ShowInTaskbar = false };
        window.Show(); window.UpdateLayout();
        try
        {
            var cards = Descendants<Border>(view).Where(x => x.Name == "SlotCard").OrderBy(x => ((IrPackSlotViewModel)x.DataContext).Index).ToArray();
            var source = (CubeDragDrop)GongDrag.GetDragHandler(cards[0]);
            var target = GongDrag.GetDropHandler(cards[1]);
            var drag = Proxy<IDragInfo>(("VisualSource", cards[0]), ("VisualSourceItem", cards[0]), ("MouseButton", MouseButton.Left));
            source.StartDrag(drag);
            var drop = Proxy<IDropInfo>(("Data", drag.Data), ("DragInfo", drag), ("VisualTarget", cards[1]), ("DropPosition", new Point(20, 20)));
            target.DragOver(drop);
            Check(drop.Effects == DragDropEffects.Move && editor.Slots[0].DistanceValue == 19 && editor.Slots[1].DistanceValue == 83,
                "IR hover accepts a swap without changing contents");
            target.Drop(drop); source.DragDropOperationFinished(drop.Effects, drag);
            Check(editor.Slots[0].DistanceValue == 83 && editor.Slots[1].DistanceValue == 19 && editor.Slots[0].Index == 0,
                "Gong IR adapter swaps full contents and preserves slot identities");
            await Task.Delay(260);
            Check(DragMotion.ActiveCount == 0 && !DragFeedback.IsVisible && cards.All(x => !x.HasAnimatedProperties), "IR drop clears transient visuals");
            var self = Proxy<IDropInfo>(("Data", drag.Data), ("DragInfo", drag), ("VisualTarget", cards[0]), ("DropPosition", new Point(20, 20)));
            source.DragOver(self); Check(self.Effects == DragDropEffects.None, "same-slot drop is invalid");
            var foreign = Proxy<IDropInfo>(("Data", new object()), ("VisualTarget", cards[0]), ("DropPosition", new Point(20, 20)));
            source.DragOver(foreign); Check(foreign.Effects == DragDropEffects.None, "foreign data cannot replace IR content");
            source.DragLeave(foreign);
            var moveEmpty = Proxy<IDropInfo>(("Data", drag.Data), ("DragInfo", drag), ("VisualTarget", cards[2]), ("DropPosition", new Point(20, 20)));
            GongDrag.GetDropHandler(cards[2]).Drop(moveEmpty);
            Check(!editor.Slots[0].HasEntry && editor.Slots[2].DistanceValue == 83, "adapter moves contents into an empty slot");
        }
        finally { window.Close(); }
        Check(DragMotion.ActiveCount == 0 && !DragFeedback.IsVisible, "unloading editor clears pending flights and hints");
        await BackingAdapters();
    }
    private static async Task BackingAdapters()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-drag-backing-" + Guid.NewGuid().ToString("N"));
        using var model = new EditorViewModel(folder);
        var window = new Cube.Desktop.MainWindow(model, initializeDevices: false) { Width = 1400, Height = 950, ShowInTaskbar = false };
        window.Show();
        try
        {
            var tracks = window.BackingTracks;
            var first = new BackingTrack(Path.Combine(folder, "keep-first.wav"));
            var hidden = new BackingTrack(Path.Combine(folder, "hidden.wav"));
            var last = new BackingTrack(Path.Combine(folder, "keep-last.wav"));
            tracks.Tracks.Add(first); tracks.Tracks.Add(hidden); tracks.Tracks.Add(last);
            tracks.SelectedTrack = first; tracks.Search = "keep";
            ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
            var list = (ListBox)window.FindName("BackingTrackList");
            var page = (FrameworkElement)window.FindName("BackingPage");
            await Task.Delay(250); list.BringIntoView(); window.UpdateLayout();
            var target = (FrameworkElement)list.ItemContainerGenerator.ContainerFromItem(last);
            var info = Proxy<IDropInfo>(("Data", first), ("VisualTarget", page),
                ("DropPosition", target.TranslatePoint(new Point(15, target.ActualHeight - 2), page)));
            var handler = GongDrag.GetDropHandler(page);
            handler.DragOver(info);
            Check(info.Effects == DragDropEffects.Move && tracks.Tracks[0] == first, "filtered playlist hover does not mutate order");
            handler.Drop(info);
            Check(tracks.Tracks.SequenceEqual(new[] { hidden, last, first }) && tracks.SelectedTrack == first,
                "playlist adapter uses unfiltered identities and preserves the active track");
            var stems = (ListBox)window.FindName("BackingStemList");
            var invalid = Proxy<IDropInfo>(("Data", first), ("VisualTarget", stems), ("DropPosition", new Point(5, 5)));
            GongDrag.GetDropHandler(stems).DragOver(invalid);
            Check(invalid.Effects == DragDropEffects.None && !invalid.NotHandled, "stem destination rejects and consumes playlist data");
            var foreign = Proxy<IDropInfo>(("Data", first.Stems[0]), ("VisualTarget", page), ("DropPosition", new Point(5, 5)));
            handler.DragOver(foreign);
            Check(foreign.Effects == DragDropEffects.None, "playlist rejects stem payloads");

            Directory.CreateDirectory(folder);
            string audio = Path.Combine(folder, "import.wav");
            using (var writer = new NAudio.Wave.WaveFileWriter(audio, new NAudio.Wave.WaveFormat(48000, 16, 1))) writer.Write(new byte[9600], 0, 9600);
            int before = first.Stems.Count;
            var data = new DataObject(DataFormats.FileDrop, new[] { audio });
            var ctor = typeof(DragEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            var args = (DragEventArgs)ctor.Invoke(ctor.GetParameters().Select(p => p.ParameterType == typeof(IDataObject) ? (object)data
                : p.ParameterType == typeof(int) ? 0 : p.ParameterType == typeof(DragDropKeyStates) ? DragDropKeyStates.LeftMouseButton
                : p.ParameterType == typeof(Point) ? new Point(5, 5) : p.ParameterType == typeof(DragDropEffects) ? DragDropEffects.Copy : (object)stems).ToArray());
            args.RoutedEvent = System.Windows.DragDrop.DropEvent;
            stems.RaiseEvent(args);
            await Task.Delay(500);
            Check(first.Stems.Count == before + 1 && tracks.Tracks.Count == 3 && args.Handled,
                "real routed file drop adds once to stems without bubbling into playlist");
            stems.BringIntoView(); window.UpdateLayout();
            var stem = first.Stems[1];
            var original = first.Stems[0];
            var stemTarget = (FrameworkElement)stems.ItemContainerGenerator.ContainerFromItem(original);
            var move = Proxy<IDropInfo>(("Data", stem), ("VisualTarget", stems),
                ("DropPosition", stemTarget.TranslatePoint(new Point(15, 2), stems)));
            GongDrag.GetDropHandler(stems).Drop(move);
            Check(first.Stems[0] == stem && first.Stems[1] == original && tracks.SelectedTrack == first,
                "stem adapter reorders within its session without selecting another track");
        }
        finally { window.Close(); }
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T item) yield return item;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
    private static T Proxy<T>(params (string Name, object Value)[] values) where T : class
    {
        var result = DispatchProxy.Create<T, DragInfoProxy>();
        var proxy = (DragInfoProxy)(object)result;
        foreach (var (name, value) in values) proxy.Values[name] = value;
        return result;
    }
}

public class DragInfoProxy : DispatchProxy
{
    public Dictionary<string, object?> Values { get; } = [];
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method!.Name.StartsWith("set_")) { Values[method.Name[4..]] = args![0]; return null; }
        if (method.Name.StartsWith("get_") && Values.TryGetValue(method.Name[4..], out var value)) return value;
        return method.ReturnType.IsValueType ? Activator.CreateInstance(method.ReturnType) : null;
    }
}
