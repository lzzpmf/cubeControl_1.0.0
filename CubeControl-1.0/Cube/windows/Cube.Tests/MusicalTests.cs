using Cube.Protocol;

static class MusicalTests
{
    public static async Task Run(Action<bool, string> check)
    {
        check(EquipmentCatalog.Amps.Count == 9 && EquipmentCatalog.Amps[4].Name == "Mesa/Boogie JP-2C" && EquipmentCatalog.Amps[4].Alias == "Cali JP A", "equipment references retain amp ordering and original aliases");
        check(EquipmentCatalog.Amps[7].Details.Contains("provável") && EquipmentCatalog.Amps[5].Category == "Overdrive", "local identification uncertainty and AC30 category are explicit");
        check(EquipmentCatalog.Cabinets.Count == 8 && EquipmentCatalog.FindFactoryCabinet("factory-cab-0")?.Name == "Line 6 Vetta 2×12" && EquipmentCatalog.FindFactoryCabinet("factory-cab-7")?.Details.Contains("Shure SM57") == true, "cabinet references exclude OFF and preserve eight factory identities");
        check(EquipmentCatalog.FindFactoryCabinet(null) == null && EquipmentCatalog.FindFactoryCabinet("factory-cab-8") == null && EquipmentCatalog.Cabinets[4].Details.Contains("Prováveis"), "unknown IRs have no equipment reference and uncertain speakers remain qualified");
        var modulation = new ModulationState();
        foreach (int raw in Enumerable.Range(0, 16))
        {
            modulation.Load(raw);
            check(modulation.Raw == raw && modulation.Enabled == (raw < 7 || raw > 8), $"modulation load preserves raw {raw}");
        }
        modulation.Load(2); modulation.SetEnabled(false);
        check(modulation.Select(ModulationKind.Phaser) == null && modulation.Adjust(6) == null && modulation.Raw == 8, "disabled modulation prepares selection without write");
        check(modulation.SetEnabled(true) == 14 && modulation.Select(ModulationKind.Chorus) == 2, "exclusive effects restore their own last position");
        check(new ModulationState().SetEnabled(true) == 3, "chorus midpoint default");
        var second = new ModulationState(); second.Select(ModulationKind.Phaser);
        check(second.SetEnabled(true) == 12 && modulation.Raw == 2, "phaser midpoint and independent preset memory");
        var reverb = new ReverbState(); reverb.Load(12); reverb.SetEnabled(false);
        check(reverb.Adjust(4) == null && !reverb.Enabled && reverb.SetEnabled(true) == 4, "reverb off preparation and restoration");
        check(new ReverbState().SetEnabled(true) == 8, "reverb midpoint default");
        var wheel = new WheelSteps();
        check(wheel.Consume(30) == 0 && wheel.Consume(30) == 0 && wheel.Consume(30) == 0 && wheel.Consume(30) == 1, "high resolution wheel accumulates fractions");
        check(wheel.Consume(-240) == -2 && wheel.Consume(60) == 0 && wheel.Consume(-60) == 0, "wheel direction and complete detents");
        var history = new EditHistory(); object gesture = new();
        history.Record(new(0, 1), 2, gesture); history.Record(new(0, 1), 3, gesture); history.Record(new(0, 1), 4, gesture);
        check(history.Pop()[new(0, 1)] == 2 && !history.CanUndo, "gesture undo preserves first value only");
        object compound = new(); history.Record(new(2, 0), 1, compound); history.Record(new(2, 9), 4, compound);
        check(history.Pop().Count == 2 && !history.CanUndo, "amp and cabinet form one undo entry");
        byte[][] sectors = Enumerable.Range(0, 8).Select(i => { var data = new byte[4096]; data[0] = (byte)(i + 1); return data; }).ToArray();
        var inventory = new CabinetInventory(); inventory.SetFactory(new(new byte[48], sectors, "test"));
        for (int i = 0; i < 7; i++) inventory.Update(i, sectors[i]);
        check(!inventory.Complete && inventory.Resolve("factory-cab-2") == null, "incomplete inventory cannot resolve a match");
        inventory.Update(7, sectors[7]);
        check(inventory.Resolve("factory-cab-2")?.Slot == 2 && new VisualPreferences().Matches.Count == 0, "factory identification retained without automatic associations");
        string hash = CabinetInventory.Hash(sectors[2]); inventory.Update(2, new byte[4096]);
        check(inventory.Resolve(hash) == null && inventory.Resolve("factory-cab-2") == null, "replaced IR suspends both factory and manual associations");
        inventory.Update(7, sectors[2]);
        check(inventory.Resolve(hash)?.Slot == 7 && inventory.Resolve("factory-cab-2")?.Slot == 7, "identical content moved to another slot is resolved");
        check(inventory.Slots[2].Name.Contains("personalizado") && inventory.Slots[2].FactoryId == null, "unknown content never inherits factory name");
        string path = Path.Combine(Path.GetTempPath(), "cube-preferences-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var preferences = new VisualPreferences(); preferences.Matches[0] = hash; preferences.Images[hash] = "image.png"; preferences.Save(path);
            var loaded = VisualPreferences.Load(path);
            check(loaded.Matches[0] == hash && loaded.Images[hash] == "image.png", "manual mapping and image metadata persist");
            preferences.Matches[1] = "factory-cab-2"; preferences.Matches[4] = "factory-cab-4"; preferences.Matches[7] = "factory-cab-3";
            preferences.Save(path); loaded = VisualPreferences.Load(path);
            check(loaded.Matches.Count == 1 && loaded.Matches[0] == hash && loaded.Images[hash] == "image.png", "legacy suggestions removed while manual associations and images survive");
            loaded.Save(path);
            check(VisualPreferences.Load(path).Matches.Count == 1, "removed suggestions stay removed after saving and reloading");
        }
        finally { File.Delete(path); }
        var sends = new List<int>(); var queue = new LiveEditQueue((_, _) => Task.CompletedTask);
        queue.EnqueueAction("amp:0", () => { sends.Add(1); return Task.CompletedTask; });
        queue.EnqueueAction("amp:0", () => { sends.Add(2); return Task.CompletedTask; });
        queue.EnqueueAction("amp:2", () => { sends.Add(3); return Task.CompletedTask; });
        await queue.Flush();
        check(sends.SequenceEqual(new[] { 2, 3 }), "pending compound selection coalesces by preset without retargeting");
        sends.Clear();
        queue.EnqueueAction("amp:0", () => { sends.Add(4); return Task.CompletedTask; });
        queue.EnqueueAction("gain:0", () => { sends.Add(5); return Task.CompletedTask; });
        await queue.Flush();
        check(sends.SequenceEqual(new[] { 4, 5 }), "queue preserves insertion order after dictionary storage is reused");
    }
}
