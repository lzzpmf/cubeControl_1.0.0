using System.Buffers.Binary;

namespace Cube.Protocol;

public static class AmpImage
{
    public const int Size = 4096, BodyOffset = 0x0C, BodyLength = 0x620;
    public const float MaximumGain = 100, MaximumCompensation = 100;
    public static void Validate(byte[] data)
    {
        if (data.Length != Size || data[0] != 1 || data[1] != 7 || data.AsSpan(2, 10).IndexOfAnyExcept((byte)0) >= 0)
            throw new ArgumentException("Arquivo de amp inválido: esperado setor Cube Baby de 4096 bytes.");
        foreach (var (offset, count) in new[] { (0x0C, 8), (0x2C, 8), (0x50, 50), (0x118, 1), (0x11C, 256), (0x51C, 50), (0x5E8, 5), (0x604, 5) })
            for (int i = 0; i < count; i++)
                if (!float.IsFinite(Get(data, offset + i * 4))) throw new ArgumentException("O amp contém valores não finitos.");
        for (int i = 0; i < 8; i++)
            if (Get(data, 0x0C + i * 4) is < 0 or > MaximumGain || Get(data, 0x2C + i * 4) is < 0 or > MaximumCompensation)
                throw new ArgumentException("Ganho e compensação devem estar entre 0 e 100.");
        for (int i = 0; i < 256; i++)
            if (Get(data, 0x11C + i * 4) is < 0 or > 1) throw new ArgumentException("A curva deve estar entre 0 e 1.");
        foreach (int offset in new[] { 0x5E8, 0x604 })
        {
            double a1 = Get(data, offset + 12), a2 = Get(data, offset + 16);
            if (Math.Abs(a2) >= 1 || 1 + a1 + a2 <= 0 || 1 - a1 + a2 <= 0)
                throw new ArgumentException("Filtro final instável.");
        }
    }
    public static float Get(byte[] data, int offset) => BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(offset, 4));
    public static void Set(byte[] data, int offset, float value) => BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(offset, 4), value);
    public static void SetEq(byte[] data, int offset, double hz, double db, double q)
    {
        if (!double.IsFinite(hz) || !double.IsFinite(db) || !double.IsFinite(q) || hz is < 20 or > 20000 || db is < -12 or > 12 || q is < 0.1 or > 10)
            throw new ArgumentException("EQ: 20–20000 Hz, −12 a +12 dB, Q de 0,1 a 10.");
        double w = 2 * Math.PI * hz / 48000, a = Math.Pow(10, db / 40), alpha = Math.Sin(w) / (2 * q), den = 1 + alpha / a;
        double[] values = [(1 + alpha * a) / den, -2 * Math.Cos(w) / den, (1 - alpha * a) / den, -2 * Math.Cos(w) / den, (1 - alpha / a) / den];
        for (int i = 0; i < 5; i++) Set(data, offset + i * 4, (float)values[i]);
    }
}
