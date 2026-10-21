using System.IO;
using Cube.Desktop.Services;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingRecordingTests
{
    public static Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-recording-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            foreach (var format in new[] { new WaveFormat(44100, 16, 1), WaveFormat.CreateIeeeFloatWaveFormat(48000, 2) })
            {
                string path = Path.Combine(folder, "capture.wav");
                byte[] packet = new byte[format.AverageBytesPerSecond / 10];
                new Random(7).NextBytes(packet);
                using (var writer = new BackingRecordingWriter(path, format))
                {
                    writer.Write(packet, false); writer.Write(packet, true);
                    Check(Math.Abs(writer.Seconds - .2) < .0001, "capture duration follows written frames");
                    try { writer.Write(new byte[1], false); throw new Exception("partial frame accepted"); }
                    catch (InvalidDataException) { }
                }
                using var reader = new WaveFileReader(path);
                Check(reader.Length == packet.Length * 2 && Math.Abs(reader.TotalTime.TotalSeconds - .2) < .0001, "closed WAV has complete header and duration");
                byte[] result = new byte[packet.Length * 2]; reader.ReadExactly(result);
                Check(result.AsSpan(0, packet.Length).SequenceEqual(packet), "input samples preserved without playback gain");
                Check(result.AsSpan(packet.Length).ToArray().All(x => x == 0), "silent capture flag writes silence instead of packet contents");
            }
            using var model = new BackingTracksViewModel(folder);
            Check(!model.ToggleRecording.CanExecute(null) && !model.IsRecording, "recording disabled without selected input");
            var input = new BackingOutput("test", "Test input");
            model.Inputs.Add(input);
            model.SelectedInput = input;
            Check(!model.ToggleRecording.CanExecute(null) && model.CanChangeRecordingInput, "synchronized recording requires an output clock");
            model.SelectedOutput = new BackingOutput("test-output", "Test output");
            Check(!model.ToggleRecording.CanExecute(null), "REC requires an armed track");
            model.AddEmptyStem.Execute(null); model.SelectedInput = input; model.SelectedStem!.IsArmed = true;
            Check(model.ToggleRecording.CanExecute(null), "REC is available with an armed track, input and output");
            model.SelectedInput = null;
            Check(!model.ToggleRecording.CanExecute(null), "recording requires the selected input");
            model.SelectedInput = input;
            model.Inputs.Clear();
            Check(!model.ToggleRecording.CanExecute(null), "removed input cannot start recording");
            var language = new LanguageService(Path.Combine(AppContext.BaseDirectory, "Localization"), Path.Combine(folder, "language.json"));
            language.Preview("en-US");
            Check(language.Translate("Parar e salvar") == "Stop and save" && language.Translate("Gravação") == "Recording", "recording actions are translated by the loaded catalog");
        }
        finally { Directory.Delete(folder, true); }
        return Task.CompletedTask;
    }
    private static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }
}
