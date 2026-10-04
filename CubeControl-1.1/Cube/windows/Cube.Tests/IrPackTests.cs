using Cube.Protocol;

internal static class IrPackTests
{
    public static void Run(Action<bool, string> check)
    {
        check(IrPackCatalog.Identify("35E39C2D76C661C41A75CAAC13BC789A7BBEB68DF37BF91C1F580529BDA8974D") == ("bfin", 0), "captured BFIN signature recognized");
        check(IrPackCatalog.Identify("237C508C64B7A0911F156BC23A3B4E91D7D7A32CE8494E6EE4F40142F80CCF8E") == ("bfi", 0), "BFI signature recognized independently");
        check(IrPackCatalog.Identify(new string('0', 64)) == null, "unknown hash has no pack identity");
        check(EquipmentCatalog.FindCabinet("pack-bfin-cab-0") == null, "BFIN never inherits a guessed cabinet model");
        string folder = Path.Combine(Path.GetTempPath(), "cube-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            check(IrPackCatalog.LoadAvailable(folder).Count == 0, "missing pack resources disabled");
            File.WriteAllBytes(Path.Combine(folder, "BFIN.bin"), new byte[32768]);
            check(IrPackCatalog.LoadAvailable(folder).Count == 0, "incorrect pack hash rejected");
        }
        finally { Directory.Delete(folder, true); }
    }
}
