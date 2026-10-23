using System.IO;

namespace Cube.Desktop.Services;

internal static class BackingRecordingRecovery
{
    // A crash can leave WAV sizes uncommitted. Repair a copy, never the interrupted original.
    internal static string Recover(string source, string destination)
    {
        File.Copy(source, destination, false);
        using var stream = new FileStream(destination, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, true);
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true);
        if (stream.Length < 44 || reader.ReadUInt32() != 0x46464952) throw new InvalidDataException("WAV inválido.");
        stream.Position = 8; if (reader.ReadUInt32() != 0x45564157) throw new InvalidDataException("WAV inválido.");
        long fact = -1; int blockAlign = 0;
        while (stream.Position + 8 <= stream.Length)
        {
            uint chunk = reader.ReadUInt32(), length = reader.ReadUInt32(); long content = stream.Position;
            if (chunk == 0x20746D66 && length >= 16) { stream.Position = content + 12; blockAlign = reader.ReadUInt16(); }
            if (chunk == 0x74636166 && length >= 4) fact = content;
            if (chunk == 0x61746164)
            {
                if (blockAlign <= 0) throw new InvalidDataException("Formato WAV indisponível.");
                long data = (stream.Length - content) / blockAlign * blockAlign;
                if (data <= 0 || data > uint.MaxValue || stream.Length - 8 > uint.MaxValue) throw new InvalidDataException("Tomada sem áudio recuperável.");
                stream.Position = content - 4; writer.Write((uint)data);
                stream.Position = 4; writer.Write((uint)(content + data - 8));
                if (fact >= 0) { stream.Position = fact; writer.Write((uint)(data / blockAlign)); }
                stream.SetLength(content + data); writer.Flush(); return destination;
            }
            if (length > stream.Length - content) break;
            stream.Position = content + length + (length & 1);
        }
        throw new InvalidDataException("Dados WAV não encontrados.");
    }
}
