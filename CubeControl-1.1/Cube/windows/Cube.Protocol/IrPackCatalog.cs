using System.Security.Cryptography;

namespace Cube.Protocol;

public sealed record IrPack(string Id, string Name, byte[][] Sectors)
{
    public override string ToString() => Name;
}

public static class IrPackCatalog
{
    public const string BfinHash = "1FABBBF9DD2797E4B4F9B43247BC2112C7285C6D8E73EDDEC0AE9191332E7E0B";
    private static readonly Dictionary<string, (string Pack, int Index)> Signatures = new()
    {
        ["237C508C64B7A0911F156BC23A3B4E91D7D7A32CE8494E6EE4F40142F80CCF8E"] = ("bfi", 0),
        ["52817AF6B39CC9696F519EE08F16023265586278D4CF15789C7C94E3174DE64F"] = ("bfi", 1),
        ["59204C476E885946118A98608667A5B158BB68BDA842BD2577F7B10B7CA6CCE9"] = ("bfi", 2),
        ["A7EC665C086CCB8C0FE980ED56C731FDD48EAD9C5AD91EE33E135A6BBD4A9EEA"] = ("bfi", 3),
        ["DF3815826DAAF239E7891AD8CB31CEA2C8C4F4CBD296320A5221129D54D51283"] = ("bfi", 4),
        ["68E478E4DC6B4255AC233916FE416D58EAC67AE923DD135B4BA58F7FFA5FF105"] = ("bfi", 5),
        ["3B6795FEE9CA9A5AD6CB3918A3202DEEF49E69C3D32250BB65FF48F701D4EC0F"] = ("bfi", 6),
        ["1179040017A5A58F3EB67DE2E88FD174C92CED08154D46317C83C42C8FC1C603"] = ("bfi", 7),
        ["35E39C2D76C661C41A75CAAC13BC789A7BBEB68DF37BF91C1F580529BDA8974D"] = ("bfin", 0),
        ["48A79E1EC2949019BB82F9629BB2E1C5E9665E6191FC22C9FC76ED7BCC9CA0A5"] = ("bfin", 1),
        ["3AF166BC7BE29209E2CFF583909149D7666F2D328B8784A6F484DB77B19C70FD"] = ("bfin", 2),
        ["898E307384477E32D6EABB6C86CA106973159447EFED2EC97BD20859012D4D76"] = ("bfin", 3),
        ["66EACF3AFFC93C0E56BB55F74E408B17E446451A698328C066C3B540098F9638"] = ("bfin", 4),
        ["49C25CA974CFCD26C7C7C35E3618A717AA394A4BD7A117242BB5A498ED020A51"] = ("bfin", 5),
        ["6F877BBBABCD460A46E930B605739FA1FA8F87D3B74767649C18FEE41AEF83D1"] = ("bfin", 6),
        ["5F190AAF16F3FE0B09D8FA50A1DFAE835B03E206BC4D6209EC8502AE5CE35986"] = ("bfin", 7),
    };
    public static (string Pack, int Index)? Identify(string hash)
        => Signatures.TryGetValue(hash, out var match) ? match : null;
    public static string CabinetId(string pack, int index) => $"pack-{pack}-cab-{index}";
    public static string CabinetName(string pack, int index) => pack == "bfi"
        ? EquipmentCatalog.Cabinets[index].Name : $"Pack BFIN · IR {index + 1}";
    public static IReadOnlyList<IrPack> LoadAvailable(string directory)
    {
        var packs = new List<IrPack>();
        Load("BFI.bin", FactoryResources.BfiHash, 69632, 0x9000, "bfi", "Pack BFI");
        Load("BFIN.bin", BfinHash, 32768, 0, "bfin", "Pack BFIN");
        return packs;
        void Load(string file, string hash, int length, int offset, string id, string name)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(directory, file));
                if (bytes.Length != length || Convert.ToHexString(SHA256.HashData(bytes)) != hash) return;
                packs.Add(new(id, name, Enumerable.Range(0, 8).Select(i => bytes.AsSpan(offset + i * 4096, 4096).ToArray()).ToArray()));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }
}
