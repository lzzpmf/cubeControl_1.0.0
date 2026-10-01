using System.Windows.Input;
using Cube.Desktop.Controls;

internal static class KnobWheelTests
{
    public static async Task Run()
    {
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); }
        void Wheel(Knob knob, int delta) => knob.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta) { RoutedEvent = Mouse.MouseWheelEvent });
        var normal = new Knob { Value = 60 };
        var output = new Knob { Value = 60, WheelStepDelta = 100 };
        for (int i = 0; i < 5; i++) { Wheel(normal, 120); Wheel(output, 120); }
        Check(normal.Value == 65 && output.Value == 66, "output wheel is 20 percent faster; other knobs unchanged");
        for (int i = 0; i < 5; i++) Wheel(output, -120);
        Check(output.Value == 60, "reverse wheel preserves exact rate");
        for (int i = 0; i < 20; i++) Wheel(output, 30);
        Check(output.Value == 66, "high resolution wheel preserves fractional input");
        output.Value = 126; Wheel(output, 150); Wheel(output, -120);
        Check(output.Value == 126, "upper limit reverses immediately without stale remainder");
        output.Value = 1; Wheel(output, -150); Wheel(output, 120);
        Check(output.Value == 1, "lower limit reverses immediately");
        output.IsEnabled = false; Wheel(output, 120);
        Check(output.Value == 1, "disabled output cannot change");
        foreach (int threshold in new[] { 100, 120 })
        {
            var faster = new Knob { Value = 30, WheelStepDelta = threshold, WheelSpeedMultiplier = 1.3 };
            for (int i = 0; i < 10; i++) Wheel(faster, threshold);
            Check(faster.Value == 43, "exact 30 percent increase over existing speed at threshold " + threshold);
            for (int i = 0; i < 40; i++) Wheel(faster, -threshold / 4);
            Check(faster.Value == 30, "fractional reverse input preserves exact rate at threshold " + threshold);
            faster.Value = 127; Wheel(faster, 120); Wheel(faster, -120);
            Check(faster.Value == 126, "accelerated wheel reverses at upper limit");
            faster.Value = 0; Wheel(faster, -120); Wheel(faster, 120);
            Check(faster.Value == 1, "accelerated wheel reverses at lower limit");
            faster.IsEnabled = false; Wheel(faster, 120);
            Check(faster.Value == 1, "disabled accelerated knob does not change");
        }
        await Task.Delay(500);
    }
}
