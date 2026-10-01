namespace Cube.Protocol;

public static class MidiMessage
{
    public static byte[] ControlChange(int channel, int controller, int value)
        => [(byte)(0xB0 | Channel(channel)), Data(controller), Data(value)];
    public static byte[] ProgramChange(int channel, int program)
        => [(byte)(0xC0 | Channel(channel)), Data(program)];
    private static int Channel(int value) => value is >= 1 and <= 16
        ? value - 1 : throw new ArgumentOutOfRangeException(nameof(value), "Canal deve estar entre 1 e 16.");
    private static byte Data(int value) => value is >= 0 and <= 127
        ? (byte)value : throw new ArgumentOutOfRangeException(nameof(value), "Dado MIDI deve estar entre 0 e 127.");
}
