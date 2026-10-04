using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Views;
using Cube.Protocol;

internal static class AmpCapacitorTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static async Task Run()
    {
        using var resource = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!;
        var bytes = new byte[AmpImage.Size];
        int count = 0;
        while (resource.Position < resource.Length)
        {
            resource.ReadExactly(bytes);
            var amp = new AmpViewModel();
            amp.Load(bytes, "Test");
            Check(amp.MainControls[0].Minimum == 0 && amp.MainControls[0].Maximum ==
                Math.Min(100, AmpImage.MaximumGain / Math.Max(.001, amp.Gain.Max(r => r.Number()))), "Original gain range");
            Check(amp.MainControls[1].Minimum == 0 && amp.MainControls[1].Maximum ==
                Math.Min(8, AmpImage.MaximumCompensation / Math.Max(.001, amp.Compensation.Max(r => r.Number()))), "Original volume range");
            foreach (var eq in new[] { amp.EqAControls, amp.EqBControls })
                Check(eq[0].Minimum == 20 && eq[0].Maximum == 20000 &&
                    eq[1].Minimum == -12 && eq[1].Maximum == 12 &&
                    eq[2].Minimum == .1 && eq[2].Maximum == 10, "Original EQ ranges");
            for (int i = 0; i < 8; i++)
            {
                Check(amp.CoefficientAControls[i].Minimum == 90 && amp.CoefficientAControls[i].Maximum ==
                    (i is 1 or 3 ? 100 : 110), "Original body and texture ranges");
                Check(amp.CoefficientBControls[i].Minimum == 90 && amp.CoefficientBControls[i].Maximum ==
                    (i <= 2 ? 100 : 110), "Original character and definition ranges");
            }
            Check(amp.CurveControls.All(c => c.Minimum == 67 && c.Maximum == 133), "Only response segments have reduced ranges");
            Check(amp.Build().SequenceEqual(bytes), "Loading must preserve the original image");
            Check(amp.CouplingControls.SequenceEqual(amp.CurveControls.Take(8)) &&
                amp.CathodeBypassControls.SequenceEqual(amp.CurveControls.Skip(8)), "Capacitor groups must preserve segment order");
            foreach (var control in amp.MainControls.Concat(amp.EqAControls).Concat(amp.EqBControls)
                .Concat(amp.CurveControls).Concat(amp.CoefficientAControls).Concat(amp.CoefficientBControls))
            {
                double original = control.Number;
                Check(!control.SetNumber(control.Minimum - .001) && !control.SetNumber(control.Maximum + .001), "Out of range numeric edits must fail");
                control.Position = -100;
                Check(Math.Abs(control.Number - control.Minimum) < 1e-8, "Lower drag endpoint");
                control.Position = 1100;
                Check(Math.Abs(control.Number - control.Maximum) < 1e-8, "Upper drag endpoint");
                control.Restore();
                Check(Math.Abs(control.Number - original) < 1e-8, "Restore loaded reference");
            }
            Check(amp.Build().SequenceEqual(bytes), "Restoring every control must restore the original image");
            var segment = amp.CurveControls[0];
            Check(segment.Minimum == 67 && segment.Maximum == 133 && !segment.TryEdit("133.01"), "Response bounds and typed input");
            segment.SetNumber(133);
            var changed = amp.Build();
            Check(changed.AsSpan(0, 0x11C).SequenceEqual(bytes.AsSpan(0, 0x11C)) &&
                changed.AsSpan(0x15C).SequenceEqual(bytes.AsSpan(0x15C)), "Segment edits must not affect other table values");
            VerifyProfiles(bytes);
            count++;
        }
        Console.WriteLine($"PASS capacitor groups, edit limits and restoration across {count} factory amps");

        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-capacitors-" + Guid.NewGuid().ToString("N")));
        model.Amp.Load(bytes, "Capacitors");
        model.Amp.Coupling.ProfileIndex = 4;
        model.Amp.Cathode.ProfileIndex = 1;
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        var editor = new AmpEditor(model);
        ((ListBox)editor.FindName("AmpSections")).SelectedIndex = 3;
        var window = new Window { Content = editor, Width = 920, Height = 1100, ShowActivated = false, ShowInTaskbar = false, Left = -10000,
            Background = (Brush)Application.Current.FindResource("CubeCard") };
        window.Show();
        try
        {
            foreach (int width in new[] { 920, 540 })
            {
                window.Width = width;
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                window.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                Directory.CreateDirectory("C:/CubeControl-Build/reports/capacitors");
                using var output = File.Create($"C:/CubeControl-Build/reports/capacitors/response-{width}.png");
                encoder.Save(output);
            }
            await RenderCards(editor, model);
        }
        finally { window.Close(); }
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }

    private static async Task RenderCards(AmpEditor editor, EditorViewModel model)
    {
        var cards = new ItemsControl { ItemsSource = model.Amp.CapacitorBanks, ItemTemplate = (DataTemplate)editor.Resources["CapacitorPanel"] };
        cards.SetResourceReference(Control.ForegroundProperty, "CubeText");
        var scroll = new ScrollViewer { Content = cards, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        var gallery = new Window { Content = scroll, Width = 920, Height = 1100, ShowActivated = false, ShowInTaskbar = false, Left = -10000 };
        gallery.Resources.MergedDictionaries.Add(editor.Resources);
        gallery.SetResourceReference(Window.BackgroundProperty, "CubeCanvas");
        gallery.Show();
        try
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var initialSelectors = Descendants<ComboBox>(cards).ToArray();
            initialSelectors[0].SelectedIndex = 2;
            Check(model.Amp.Coupling.ProfileIndex == 2 && model.Amp.CurveControls[0].Number == 88, "Visor selects profile and applies all segments through binding");
            initialSelectors[0].SelectedIndex = 0;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(!Descendants<Cube.Desktop.Controls.Knob>(cards).Where(k => k.Maximum == 100).First().IsEnabled, "Original disables intensity in UI");
            initialSelectors[0].SelectedIndex = 4;
            Cube.Desktop.Localization.LanguageService.Instance.Preview("en-US");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(Descendants<TextBlock>(cards).Any(t => t.Text == "22 nF · Full-bodied"), "Profile display localizes");
            Cube.Desktop.Localization.LanguageService.Instance.Preview("pt-BR");
            foreach (string theme in new[] { "Dark", "Light", "HighContrast" })
            {
                if (theme == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                else Cube.Desktop.Appearance.ThemeService.Instance.Preview(Enum.Parse<Cube.Desktop.Appearance.ThemeMode>(theme));
                foreach (int width in new[] { 920, 540 })
                    foreach (double scale in new[] { .5, 1, 2 })
                    {
                        gallery.Width = width; cards.LayoutTransform = new ScaleTransform(scale, scale);
                        scroll.ScrollToTop();
                        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); gallery.UpdateLayout();
                        var selectors = Descendants<ComboBox>(cards).ToArray();
                        Check(selectors.Length == 2 && selectors.All(c => c.Items.Count == 6), "Two six-position profile selectors");
                        var knobs = Descendants<Cube.Desktop.Controls.Knob>(cards).ToArray();
                        Check(knobs.Length == 4, "Only four macro knobs are visible");
                        foreach (var control in knobs.Cast<FrameworkElement>().Concat(selectors))
                        {
                            var bounds = control.TransformToAncestor(cards).TransformBounds(new Rect(control.RenderSize));
                            Check(bounds.Left >= -1 && bounds.Right <= cards.ActualWidth + 1, "Macro controls remain within card width at all zoom levels");
                        }
                        for (int bottom = 0; bottom <= 1; bottom++)
                        {
                            if (bottom == 1) { scroll.ScrollToBottom(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); }
                            var bitmap = new RenderTargetBitmap((int)gallery.ActualWidth, (int)gallery.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                            bitmap.Render(gallery);
                            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                            using var output = File.Create($"C:/CubeControl-Build/reports/capacitors/cards-{theme}-{width}-{scale * 100:0}-{bottom}.png");
                            png.Save(output);
                        }
                    }
            }
            Console.WriteLine("PASS macro layout at 540/920 widths, 50/100/200% zoom, dark/light/high contrast");
        }
        finally { gallery.Close(); Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark); }
    }

    private static void VerifyProfiles(byte[] reference)
    {
        var amp = new AmpViewModel(); amp.Load(reference, "Profile tests");
        int events = 0; amp.Edited += () => events++;
        foreach (bool coupling in new[] { true, false })
        {
            var bank = coupling ? amp.Coupling : amp.Cathode;
            int offset = coupling ? 0 : 128;
            Check(bank.ProfileIndex == 0 && bank.Intensity == 50 && !bank.IsActive, "Initial macro state");
            for (int p = 1; p < 6; p++)
            {
                events = 0; bank.ProfileIndex = p;
                Check(events == 1, "One atomic event per profile change");
                foreach (double intensity in new[] { 0d, 50d, 100d })
                {
                    events = 0; bank.Intensity = intensity;
                    Check(events == 1, "One atomic event per intensity change");
                    byte[] built = amp.Build();
                    for (int i = 0; i < 256; i++)
                    {
                        float original = AmpImage.Get(reference, 0x11C + i * 4);
                        double percentage = i >= offset && i < offset + 128 ? bank.Percentage((i - offset) / 16) : 100;
                        Check(percentage >= 67 && percentage <= 133, "Macro percentage limits");
                        float expected = Math.Clamp((float)(original * (percentage / 100)), 0, 1);
                        Check(AmpImage.Get(built, 0x11C + i * 4) == expected, "Exact profile output and bank isolation");
                    }
                    Check(built.AsSpan(0, 0x11C).SequenceEqual(reference.AsSpan(0, 0x11C)) &&
                        built.AsSpan(0x51C).SequenceEqual(reference.AsSpan(0x51C)), "Macro preserves non-curve bytes");
                    if (intensity == 0) Check(built.SequenceEqual(reference), "Zero intensity restores exact reference");
                }
                byte[] once = amp.Build();
                bank.ProfileIndex = p == 5 ? 1 : 5; bank.ProfileIndex = p;
                Check(amp.Build().SequenceEqual(once), "Switching profiles never compounds edits");
            }
            events = 0; bank.RestoreOriginal.Execute(null);
            Check(events == 1 && bank.ProfileIndex == 0 && bank.Intensity == 50 && amp.Build().SequenceEqual(reference), "Restore original resets only bank and intensity");
        }
        amp.Coupling.ProfileIndex = 4; amp.Cathode.ProfileIndex = 5; amp.Cathode.Intensity = 73;
        amp.MainControls[0].SetNumber(.8); amp.EqAControls[1].SetNumber(3);
        amp.CoefficientAControls[0].SetNumber(94);
        var snapshot = amp.CaptureEditSnapshot(); byte[] audio = amp.Build();
        amp.Load(audio, "Imported");
        Check(amp.Build().SequenceEqual(audio) && amp.Coupling.ProfileIndex == 0 && amp.Cathode.ProfileIndex == 0, "Reimport preserves audio and establishes new reference");
        amp.RestoreEditSnapshot(snapshot);
        Check(amp.Build().SequenceEqual(audio) && amp.Coupling.ProfileIndex == 4 && amp.Cathode.ProfileIndex == 5 && amp.Cathode.Intensity == 73, "History preserves audio and macro state");
        amp.Coupling.RestoreOriginal.Execute(null);
        Check(amp.Cathode.ProfileIndex == 5 && amp.Cathode.Intensity == 73, "Restoring one bank preserves the other");
        amp.Cathode.RestoreOriginal.Execute(null);
        var restored = amp.Build();
        Check(restored.AsSpan(0x11C, 1024).SequenceEqual(reference.AsSpan(0x11C, 1024)), "History retains original curve reference");
        var knob = new Cube.Desktop.Controls.Knob { Minimum = 0, Maximum = 5, StepSize = 1 };
        knob.SetCurrentValue(Cube.Desktop.Controls.Knob.ValueProperty, 2.7);
        Check(knob.Value == 3, "Discrete values snap even through dependency-property binding");
        knob.SetCurrentValue(Cube.Desktop.Controls.Knob.ValueProperty, 20d);
        Check(knob.Value == 5, "Discrete values clamp");
        var source = new System.Windows.Interop.HwndSource(new System.Windows.Interop.HwndSourceParameters("Capacitor key test") { Width = 1, Height = 1, PositionX = -10000 });
        try
        {
            foreach (var (key, expected) in new[] { (System.Windows.Input.Key.Home, 0d), (System.Windows.Input.Key.Right, 1d), (System.Windows.Input.Key.End, 5d), (System.Windows.Input.Key.Left, 4d) })
            {
                knob.RaiseEvent(new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = System.Windows.Input.Keyboard.KeyDownEvent });
                Check(knob.Value == expected, "Discrete keyboard selection");
            }
            knob.RaiseEvent(new System.Windows.Input.MouseWheelEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, -120) { RoutedEvent = System.Windows.Input.Mouse.MouseWheelEvent });
            Check(knob.Value == 3, "Wheel advances one profile at a time");
        }
        finally { source.Dispose(); }
    }
}
