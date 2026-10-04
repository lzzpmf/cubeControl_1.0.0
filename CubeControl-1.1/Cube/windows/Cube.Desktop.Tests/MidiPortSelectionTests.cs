using Cube.Usb;

internal static class MidiPortSelectionTests
{
    internal static void Run()
    {
        void Check(bool result, string name)
        {
            if (!result) throw new Exception(name);
            Console.WriteLine("PASS: " + name);
        }
        foreach (string name in new[] { "USB-Midi", "CubeBaby-Midi", "CubeBaby-Midi [2]", "USB-Midi [3]", "cubebaby-midi" })
        {
            var ports = CubeMidiPorts.Select([new("other-in", "StudioRack"), new("in", name)],
                [new("other-out", "Microsoft GS Wavetable Synth"), new("out", name)]);
            Check(ports.Input?.Id == "in" && ports.Output?.Id == "out", "select firmware port " + name);
        }
        Check(CubeMidiPorts.Select([new("in", "USB-Midi")], [new("out", "CubeBaby-Midi")]).Input == null,
            "do not mix different devices");
        Check(CubeMidiPorts.Select([new("in", "CubeBaby-Midi")], []).Input == null, "wait for both endpoints");
        Check(CubeMidiPorts.Select([new("a", "USB-Midi"), new("b", "CubeBaby-Midi")],
            [new("c", "USB-Midi"), new("d", "CubeBaby-Midi")]).Input == null, "ambiguous pedals require selection");
        Check(!CubeMidiPorts.IsCandidate(new("x", "CubeBaby Bass")) &&
            !CubeMidiPorts.IsCandidate(new("x", "Other USB-Midi")), "unrelated names are not candidates");
    }
}
