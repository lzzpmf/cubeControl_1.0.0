using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;

internal static class AdvancedInteractionTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static T Field<T>(object instance, string name) => (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
    private static void Invoke(object instance, string name, params object[] args) => instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, args);
    public static async Task Run()
    {
        foreach (var (kind, model, count) in new (string, object, int)[] {
            ("Chorus", new ChorusViewModel(), 7), ("Phaser", new PhaserViewModel(), 5),
            ("Delay", new DelayViewModel(), 5), ("Reverb", new ReverbViewModel(), 5) })
        {
            var editor = new EffectGraphEditor { EffectKind = kind, DataContext = model };
            var window = new Window { Content = editor, Width = 800, Height = 700, ShowActivated = false, ShowInTaskbar = false, Left = -10000 };
            window.Show();
            try
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                var picker = Field<ComboBox>(editor, "parameterPicker");
                var slider = Field<Slider>(editor, "adjustment");
                Check(picker.Items.Count == count && picker.SelectedIndex == 0, kind + " parameter discovery");
                foreach (ComboBoxItem item in picker.Items)
                {
                    picker.SelectedItem = item;
                    object node = item.Tag;
                    string key = (string)node.GetType().GetProperty("Key", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(node)!;
                    var property = model.GetType().GetProperty(key)!;
                    if (key is "Waveform" or "Sections")
                    {
                        var choices = Field<WrapPanel>(editor, "choices");
                        foreach (Wpf.Ui.Controls.Button button in choices.Children)
                        {
                            button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                            Check(Convert.ToDouble(property.GetValue(model)) == Convert.ToDouble(button.Tag), kind + " " + key + " choice");
                        }
                    }
                    else
                    {
                        foreach (double fraction in new[] { .25, .75, .5 })
                        {
                            double value = slider.Minimum + fraction * (slider.Maximum - slider.Minimum);
                            slider.Value = value;
                            Check(Math.Abs(Convert.ToDouble(property.GetValue(model)) - slider.Value) < .001, kind + " " + key + " slider binding");
                            Check(Math.Abs(slider.Value - value) <= .501, kind + " " + key + " requested value");
                        }
                        Invoke(editor, "BeginNumber");
                        var entry = Field<TextBox>(editor, "entry");
                        double target = slider.Minimum;
                        entry.Text = target.ToString(System.Globalization.CultureInfo.CurrentCulture);
                        entry.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 0, entry, picker) { RoutedEvent = Keyboard.LostKeyboardFocusEvent });
                        Check(Convert.ToDouble(property.GetValue(model)) == target, kind + " " + key + " focus-loss commit actual=" + property.GetValue(model) + " target=" + target + " editing=" + Field<bool>(editor, "editing") + " text=" + entry.Text);
                        Invoke(editor, "BeginNumber"); entry.Text = "invalid";
                        Invoke(editor, "EndNumber", true, false);
                        Check(Convert.ToDouble(property.GetValue(model)) == target && entry.IsVisible, kind + " invalid input");
                        Invoke(editor, "EndNumber", false, false);
                        if (key == "Rate")
                        {
                            slider.Value = 50;
                            var freeze = (Wpf.Ui.Controls.Button)Field<WrapPanel>(editor, "choices").Children[0];
                            freeze.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                            Check(Convert.ToDouble(property.GetValue(model)) == 0, "Delay freezes modulation");
                            freeze.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                            Check(Convert.ToDouble(property.GetValue(model)) == 50, "Delay resumes previous rate");
                        }
                    }
                    Console.WriteLine("PASS advanced interaction " + kind + "/" + key);
                }
                foreach (double width in new[] { 360d, 600d, 900d })
                {
                    window.Width = width; window.UpdateLayout();
                    Check(picker.ActualWidth > 0 && slider.ActualWidth > 0 && editor.ActualWidth <= window.ActualWidth, kind + " responsive layout");
                    var bitmap = new RenderTargetBitmap((int)editor.ActualWidth, (int)editor.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(editor);
                    var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                    string folder = "C:/CubeControl-Build/advanced-ux/reports"; Directory.CreateDirectory(folder);
                    using var output = File.Create(Path.Combine(folder, $"{kind}-{width}.png")); png.Save(output);
                }
            }
            finally { window.Close(); }
        }
    }
}
