using System.Text.RegularExpressions;

namespace Cube.Usb;

public static class CubeMidiPorts
{
    // Firmware variants expose different USB product names. These are candidates
    // only: CubeSession.Identify still confirms the guitar pedal before access.
    public static bool IsCandidate(MidiPort port) => Regex.IsMatch(port.Name.Trim(),
        @"^(?:USB-Midi|CubeBaby-Midi|Cube Baby-Midi|CubeBaby|Cube Baby|SINCO-CubeBaby)(?: \[\d+\])?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static (MidiPort? Input, MidiPort? Output) Select(MidiPort[] inputs, MidiPort[] outputs)
    {
        var pairs = (from input in inputs.Where(IsCandidate)
                     from output in outputs.Where(IsCandidate)
                     where string.Equals(input.Name.Trim(), output.Name.Trim(), StringComparison.OrdinalIgnoreCase)
                     select (Input: input, Output: output)).ToArray();
        // Never combine ports from different devices or arbitrarily pick one of
        // several pedals. The connection dialog can resolve ambiguous selections.
        return pairs.Length == 1 ? pairs[0] : (null, null);
    }
}
