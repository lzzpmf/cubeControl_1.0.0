using NAudio.CoreAudioApi;

internal static class BackingHardwareChecks
{
    public static void Run()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            foreach (var flow in new[] { DataFlow.Capture, DataFlow.Render })
            {
                var endpoints = enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);
                Console.WriteLine($"{flow}: {endpoints.Count} active endpoints");
                foreach (var device in endpoints)
                    using (device) Console.WriteLine($"  {device.FriendlyName}; {device.State}");
            }
            Console.WriteLine("Read-only enumeration; no playback, capture, latency or drift measurement performed.");
        }
        catch (Exception ex) { Console.WriteLine("Hardware enumeration unavailable: " + ex.Message); }
    }
}
