using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class AmpEqPresetTests
{
    public static Task Run()
    {
        void Check(bool value, string message) { if (!value) throw new Exception(message); }
        Check(AmpEqPresets.All.Count == 9, "Nine EQ variations");
        using var stream = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!;
        using var editor = new EditorViewModel(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cube-amp-art-" + Guid.NewGuid().ToString("N")));
        var identify = typeof(EditorViewModel).GetMethod("IdentifyAmpArtwork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        foreach (var preset in AmpEqPresets.All)
        {
            stream.Position = preset.BaseIndex * AmpImage.Size;
            var original = new byte[AmpImage.Size]; stream.ReadExactly(original);
            byte[] built = preset.Build();
            AmpImage.Validate(built);
            Check(!original.SequenceEqual(built), "Variation differs from base");
            for (int i = 0; i < built.Length; i++)
                if (!(i >= 0x5E8 && i < 0x5FC) && !(i >= 0x604 && i < 0x618))
                    Check(built[i] == original[i], $"Non-EQ byte changed: {preset.Name} / {i:X}");
            var model = new AmpViewModel { Slot = 8 };
            model.Load(built, preset.Name);
            Check(model.Slot == 8, "Loading a variation preserves destination");
            Check(model.Build().SequenceEqual(built), "Editor preserves EQ image exactly");
            model.LoadSlot(built, 0);
            Check(model.Name == preset.Name, "Uploaded variations are recognized by content");
            Check(preset.Build().SequenceEqual(built), "Builds never accumulate EQ");
            identify.Invoke(editor, [8, built, false]);
            var gear = editor.Amps[8];
            Check(gear.Name == preset.Name && gear.Value == 8, "Artwork identity follows content, not destination");
            Check(gear.Image != null && gear.DefaultImage != null, "Bundled artwork decodes for every EQ variation");
            Check(editor.Amps[0].Name == EquipmentCatalog.Amps[0].Name, "Other slots keep their identities");
            identify.Invoke(editor, [8, original, false]);
            Check(editor.Amps[8].Name == preset.BaseName && editor.Amps[8].Image != null, "Factory restore clears variation artwork");
        }
        Console.WriteLine("PASS nine EQ variants: stable filters, byte isolation, editor round trip, destination and recognition");
        Console.WriteLine("PASS nine bundled amp images, slot identity updates and factory artwork restoration");
        return Task.CompletedTask;
    }
}
