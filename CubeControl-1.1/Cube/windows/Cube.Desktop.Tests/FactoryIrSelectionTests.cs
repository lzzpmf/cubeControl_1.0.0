using System.IO;
using System.Reflection;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class FactoryIrSelectionTests
{
    public static async Task Run()
    {
        using var fake = new FakeConnection();
        using var session = new CubeSession(fake);
        await session.Identify();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-ir-selection-" + Guid.NewGuid().ToString("N")));
        await model.Execute(() => model.AttachSession(session));
        var packs = IrPackCatalog.LoadAvailable(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CubeSuite", "bin"));
        if (packs.Count != 2) throw new Exception("This integration test requires the local BFI and BFIN resources");
        var inventory = new CabinetInventory();
        foreach (var pack in packs)
        for (int i = 0; i < 8; i++)
        {
            int movedSlot = (i + 3) % 8;
            var item = inventory.Update(movedSlot, pack.Sectors[i]);
            if (item.CabinetId != IrPackCatalog.CabinetId(pack.Id, i)) throw new Exception("Pack recognition depends on slot");
            var gear = new GearItem(item.Hash, movedSlot + 1, item.Name, item.Category, "cab", "#C5B77F", cabinetId: item.CabinetId);
            if (gear.DefaultImage == null) throw new Exception("Pack image missing");
            var changed = pack.Sectors[i].ToArray(); changed[200] ^= 1;
            if (inventory.Update(movedSlot, changed).CabinetId != null) throw new Exception("Modified IR falsely recognized");
        }
        var factory = new FactoryResources(fake.Bank.ToArray(), Enumerable.Range(0, 8)
            .Select(i => new IrAudio(48000, 1, 32, "float", [(i + 1) / 16f]).Sector(50)).ToArray(), "test");
        typeof(EditorViewModel).GetField("factory", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(model, factory);
        model.SelectedIrPack = new IrPack("test", "Pack de teste", factory.IrSectors);
        factory.IrSectors[0].CopyTo(fake.Flash, 0);
        byte[] original = fake.Flash.ToArray(), bank = fake.Bank.ToArray();
        async Task Reset()
        {
            await (Task)typeof(EditorViewModel).GetMethod("ApplyIrPackAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(model, null)!;
        }
        model.ChoosePackIrSlots = different =>
        {
            if (different[0] || different.Skip(1).Any(value => !value)) throw new Exception("Incorrect comparison");
            return Task.FromResult<int[]?>(null);
        };
        await Reset();
        if (fake.Erases != 0) throw new Exception("Cancel wrote flash");
        model.ChoosePackIrSlots = _ => Task.FromResult<int[]?>([1, 6]);
        model.Ask = _ => Task.FromResult(false);
        await Reset();
        if (fake.Erases != 0) throw new Exception("Declining confirmation wrote flash");
        model.Ask = _ => Task.FromResult(true);
        await Reset();
        for (int i = 0; i < 8; i++)
            if (!fake.Flash.AsSpan(i * 4096, 4096).SequenceEqual(i is 1 or 6 ? factory.IrSectors[i] : original.AsSpan(i * 4096, 4096)))
                throw new Exception($"Incorrect slot {i + 1}");
        if (fake.Erases != 2 || !fake.Bank.SequenceEqual(bank)) throw new Exception("Unexpected writes");
        for (int i = 0; i < 8; i++) factory.IrSectors[i].CopyTo(fake.Flash, i * 4096);
        model.ChoosePackIrSlots = _ => throw new Exception("Identical IRs prompted for replacement");
        await Reset();
        if (fake.Erases != 2) throw new Exception("Identical IRs wrote flash");
        Console.WriteLine("PASS factory IR comparison, cancellation, selective restore, preserved slots/bank and identical IRs.");
    }
}
