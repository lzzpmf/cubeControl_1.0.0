using System.IO;
using Cube.Desktop;

internal static class FirmwareWorkerTests
{
    // Explicit opt-in: exercises UAC and the actual worker, but invalid extension
    // guarantees failure before disk enumeration or any device access.
    internal static async Task Run()
    {
        var messages = new List<FirmwareUpdater.UpdateMessage>();
        const string expected = "Selecione um arquivo de firmware .up.";
        try
        {
            await FirmwareUpdater.RunElevated(Path.Combine(Path.GetTempPath(), "firmware-ipc-test.txt"),
                "unused", messages.Add);
            throw new Exception("Invalid firmware unexpectedly accepted.");
        }
        catch (IOException ex) when (ex.Message == expected)
        {
            if (messages.Count != 1 || !messages[0].Finished || messages[0].Success || messages[0].Text != expected)
                throw new Exception("Worker did not deliver the structured validation failure.");
        }
        Console.WriteLine("PASS: elevated firmware worker reports validation failure without accessing hardware.");
    }
}
