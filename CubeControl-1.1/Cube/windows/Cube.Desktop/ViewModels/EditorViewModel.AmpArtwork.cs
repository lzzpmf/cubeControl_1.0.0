using System.IO;
using System.Text.Json;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private string AmpArtworkSlotsPath => Path.Combine(DataDirectory, "amp-artwork-slots-v1.json");

    // Only remember which slots need identification. Never trust a saved label without reading the device.
    private Dictionary<string, int[]> ReadAmpArtworkSlots()
    {
        try
        {
            return File.Exists(AmpArtworkSlotsPath)
                ? JsonSerializer.Deserialize<Dictionary<string, int[]>>(File.ReadAllText(AmpArtworkSlotsPath)) ?? [] : [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            NotifyFailure("Não foi possível ler as associações de imagens dos amps: " + ex.Message);
            return [];
        }
    }

    private void IdentifyAmpArtwork(int slot, byte[] bytes, bool remember = true)
    {
        var variation = AmpEqPresets.All.FirstOrDefault(p => bytes.AsSpan().SequenceEqual(p.Build()));
        if (variation != null)
        {
            Amps[slot] = WithImage(new GearItem($"amp-eq-{variation.BaseIndex}-slot-{slot}", slot, variation.Name,
                EquipmentCatalog.Amps[variation.BaseIndex].Category, "amp", "#D5AD7D", variation.Description,
                ampImage: variation.ImageName));
        }
        else
        {
            int originalIndex = -1;
            using var stream = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!;
            var original = new byte[AmpImage.Size];
            for (int i = 0; i < EquipmentCatalog.Amps.Count; i++)
            {
                stream.ReadExactly(original);
                if (bytes.AsSpan().SequenceEqual(original)) { originalIndex = i; break; }
            }
            if (originalIndex >= 0)
            {
                var reference = EquipmentCatalog.Amps[originalIndex];
                Amps[slot] = WithImage(new GearItem($"amp-{originalIndex}", slot, reference.Name,
                    reference.Category, "amp", "#D5AD7D", reference.Description, reference.Alias));
            }
            else
                Amps[slot] = WithImage(new GearItem($"amp-custom-{slot}", slot, $"Amp do slot {slot + 1}",
                    "Personalizado", "amp", "#D5AD7D"));
        }
        Notify(nameof(Amps)); RefreshEditor();
        if (!remember || session == null) return;
        try
        {
            var slots = ReadAmpArtworkSlots();
            slots.TryGetValue(session.IdentityHex, out var known);
            slots[session.IdentityHex] = (known ?? []).Append(slot).Distinct().Order().ToArray();
            Directory.CreateDirectory(DataDirectory);
            File.WriteAllText(AmpArtworkSlotsPath, JsonSerializer.Serialize(slots));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            NotifyFailure("Amp identificado, mas não foi possível salvar a associação da imagem: " + ex.Message);
        }
    }

    private async Task RefreshKnownAmpArtworkAsync()
    {
        Amps = EquipmentCatalog.Amps.Select((reference, i) => WithImage(new GearItem($"amp-{i}", i,
            reference.Name, reference.Category, "amp", "#D5AD7D", reference.Description, reference.Alias))).ToArray();
        Notify(nameof(Amps)); RefreshEditor();
        if (session == null || !ReadAmpArtworkSlots().TryGetValue(session.IdentityHex, out var slots)) return;
        foreach (int slot in (slots ?? []).Distinct().Where(i => i >= 0 && i < EquipmentCatalog.Amps.Count))
            IdentifyAmpArtwork(slot, await session.ReadAmpSlot(slot), remember: false);
    }
}
