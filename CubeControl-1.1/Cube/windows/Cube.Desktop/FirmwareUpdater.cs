using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Microsoft.Win32.SafeHandles;

namespace Cube.Desktop;

internal static class FirmwareUpdater
{
    internal sealed record UpdateMessage(string Text, int Percent, bool Finished = false, bool Success = false);
    internal static string Folder => Path.Combine(AppContext.BaseDirectory, "Assets", "Firmwares", "CubeBaby");
    private const string ResetHash = "C02D7D528891ED695BD77F2167F9F9C7915BCA73F2D2B40DF84CB58E7E13C6F1";

    internal static byte[] Validate(string path)
    {
        if (!Path.GetExtension(path).Equals(".up", StringComparison.OrdinalIgnoreCase))
            throw new IOException("Selecione um arquivo de firmware .up.");
        var length = new FileInfo(path).Length;
        if (length is < 65792 or > 524544) throw new IOException("Tamanho de firmware não compatível com o perfil Cube Baby.");
        byte[] data = File.ReadAllBytes(path);
        if (!data.AsSpan(0, 16).SequenceEqual(Convert.FromHexString("434F4E46494700010000000009000000")) ||
            data[254] != 0x55 || data[255] != 0xAA ||
            BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(16)) != data.Length - 256 ||
            BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(20)) != Crc(data.AsSpan(256)))
            throw new IOException("Firmware inválido: cabeçalho, tamanho ou CRC não corresponde ao formato Cube Baby. Nenhum dado foi enviado.");
        return data;
    }

    private static ushort Crc(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (byte value in data)
        {
            crc ^= (ushort)(value << 8);
            for (int bit = 0; bit < 8; bit++) crc = (ushort)((crc << 1) ^ ((crc & 0x8000) != 0 ? 0x1021 : 0));
        }
        return crc;
    }

    internal static async Task RunElevated(string path, string hash, Action<UpdateMessage> progress)
    {
        string pipeName = "CubeControl-Firmware-" + Guid.NewGuid().ToString("N");
        using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        var start = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "CubeControl.exe"))
        { UseShellExecute = true, Verb = "runas", WindowStyle = ProcessWindowStyle.Hidden };
        foreach (string arg in new[] { "--firmware-worker", Path.GetFullPath(path), hash, pipeName }) start.ArgumentList.Add(arg);
        using var worker = Process.Start(start) ?? throw new IOException("Não foi possível iniciar a atualização.");
        using var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        Task connect = pipe.WaitForConnectionAsync(connectTimeout.Token);
        Task exit = worker.WaitForExitAsync();
        if (await Task.WhenAny(connect, exit) == exit && !pipe.IsConnected)
            throw new IOException("O processo de atualização encerrou antes de conectar.");
        await connect;
        using var reader = new StreamReader(pipe);
        UpdateMessage? final = null;
        while (await reader.ReadLineAsync() is { } line)
        {
            var message = JsonSerializer.Deserialize<UpdateMessage>(line) ?? throw new IOException("Resposta de atualização inválida.");
            progress(message);
            if (message.Finished) { final = message; break; }
        }
        await exit;
        if (final?.Success != true) throw new IOException(final?.Text ??
            $"O auxiliar de atualização encerrou sem informar o resultado (código {worker.ExitCode}). Não foi possível confirmar a etapa da atualização.");
    }

    internal static int RunWorker(string[] args)
    {
        if (args.Length != 4) return 1;
        // CurrentUserOnly on the elevated client compares the server owner with the
        // elevated token owner (Administrators), rejecting the unelevated app's pipe.
        // The server still restricts access to the current user. Identification also
        // prevents the server from impersonating this elevated worker.
        using var pipe = new NamedPipeClientStream(".", args[3], PipeDirection.Out,
            PipeOptions.None, TokenImpersonationLevel.Identification);
        try { pipe.Connect(30000); } catch { return 1; }
        using var writer = new StreamWriter(pipe) { AutoFlush = true };
        void Report(UpdateMessage message) => writer.WriteLine(JsonSerializer.Serialize(message));
        try
        {
            byte[] file = Validate(args[1]);
            if (!Convert.ToHexString(SHA256.HashData(file)).Equals(args[2], StringComparison.OrdinalIgnoreCase))
                throw new IOException("O firmware foi alterado após a confirmação. Selecione-o novamente.");
            byte[] reset = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "Firmwares", "reset.bin"));
            if (reset.Length != 12288 || Convert.ToHexString(SHA256.HashData(reset)) != ResetHash)
                throw new IOException("Auxiliar de reinicialização ausente ou inválido.");
            if (Process.GetProcessesByName("flash_loader").Length != 0)
                throw new IOException("Feche o flash_loader antes de atualizar pelo CubeControl.");
            var targets = FindDisks();
            if (targets.Count != 1) throw new IOException(targets.Count == 0
                ? "Cube Baby em modo flash não encontrado. Ligue-o conectado ao PC segurando o terceiro footswitch."
                : "Há mais de um Cube Baby em modo flash. Conecte somente o pedal que será atualizado.");
            var target = targets[0];
            using (var device = new FlashDevice(target.Path))
            {
                Report(new("Preparando o pedal…", 0));
                device.Send(Convert.FromHexString("C0810900000000000000000000000000"));
                var id = device.Send(Command(0, 4, 0), readLength: 4);
                if (!id.SequenceEqual(Convert.FromHexString("85128512")))
                    throw new IOException("Identificação da flash diferente da unidade validada. Gravação bloqueada.");
                byte[] payload = file[256..];
                int written = 0;
                var addresses = Enumerable.Range(1, (payload.Length - 1) / 65536).Select(i => i * 65536).Append(0);
                foreach (int address in addresses)
                {
                    int size = Math.Min(65536, payload.Length - address);
                    byte[] block = payload.AsSpan(address, size).ToArray();
                    Report(new($"Gravando firmware: {written * 100 / payload.Length}% — não desconecte o pedal.", written * 100 / payload.Length));
                    device.Send(Command(5, 0, address));
                    device.Send(Command(2, size, address), block);
                    byte[] check = device.Send(Command(0x82, size, address), readLength: 2);
                    if (BinaryPrimitives.ReadUInt16LittleEndian(check) != Crc(block))
                        throw new IOException($"CRC divergente no bloco 0x{address:X}. Atualização interrompida; entre novamente em modo flash.");
                    written += size;
                }
                Report(new("Firmware gravado e verificado. Reiniciando o pedal…", 100));
                device.Send(Convert.FromHexString("C0810900000000000000000000000000"));
                device.Send(Command(9, reset.Length - 256, 0x820000), reset[256..]);
                try { device.Send(Command(10, 0, 0x820001)); }
                catch (Win32Exception ex) when (ex.NativeErrorCode is 121 or 1167 or 995 or 1117) { }
            }
            var deadline = DateTime.UtcNow.AddSeconds(25);
            while (DateTime.UtcNow < deadline)
            {
                if (NormalDeviceReturned(target.Location))
                {
                    Report(new("Firmware instalado, CRCs conferidos e pedal reconectado em modo normal.", 100, true, true));
                    return 0;
                }
                Thread.Sleep(500);
            }
            throw new IOException("Firmware gravado e verificado, mas o retorno ao modo normal não foi confirmado. Desligue e ligue o pedal para concluir.");
        }
        catch (Exception ex)
        {
            try { Report(new(ex.Message, 0, true)); } catch (IOException) { }
            return 1;
        }
    }

    private static byte[] Command(byte opcode, int length, int address)
    {
        byte[] cdb = new byte[16]; cdb[0] = 0xC0; cdb[1] = opcode;
        BinaryPrimitives.WriteInt32LittleEndian(cdb.AsSpan(2), length);
        BinaryPrimitives.WriteInt32LittleEndian(cdb.AsSpan(6), address);
        return cdb;
    }

    private sealed record Target(string Path, string Location);
    private static List<Target> FindDisks()
    {
        Guid guid = new("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
        IntPtr set = SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, 0x12);
        if (set == new IntPtr(-1)) throw new Win32Exception();
        var targets = new List<Target>();
        try
        {
            for (uint i = 0; ; i++)
            {
                var item = new InterfaceData { Size = Marshal.SizeOf<InterfaceData>() };
                if (!SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref guid, i, ref item))
                { if (Marshal.GetLastWin32Error() != 259) throw new Win32Exception(); break; }
                SetupDiGetDeviceInterfaceDetail(set, ref item, IntPtr.Zero, 0, out uint size, IntPtr.Zero);
                IntPtr detail = Marshal.AllocHGlobal((int)size);
                IntPtr info = Marshal.AllocHGlobal(Marshal.SizeOf<DeviceInfo>());
                try
                {
                    Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6);
                    Marshal.StructureToPtr(new DeviceInfo { Size = Marshal.SizeOf<DeviceInfo>() }, info, false);
                    if (!SetupDiGetDeviceInterfaceDetail(set, ref item, detail, size, out _, info)) throw new Win32Exception();
                    uint node = Marshal.PtrToStructure<DeviceInfo>(info).Node;
                    for (int depth = 0; depth < 8; depth++)
                    {
                        string id = DeviceId(node);
                        if (id.StartsWith("USB\\VID_301A&PID_2800\\", StringComparison.OrdinalIgnoreCase))
                        {
                            string location = Location(node);
                            if (location.Length == 0) throw new IOException("Não foi possível identificar a porta física do pedal.");
                            targets.Add(new(Marshal.PtrToStringUni(detail + 4)!, location)); break;
                        }
                        if (CM_Get_Parent(out uint parent, node, 0) != 0) break;
                        node = parent;
                    }
                }
                finally { Marshal.FreeHGlobal(detail); Marshal.FreeHGlobal(info); }
            }
        }
        finally { SetupDiDestroyDeviceInfoList(set); }
        return targets;
    }

    private static string DeviceId(uint node)
    {
        var id = new StringBuilder(512);
        return CM_Get_Device_ID(node, id, id.Capacity, 0) == 0 ? id.ToString() : "";
    }
    private static string Location(uint node)
    {
        byte[] data = new byte[2048]; uint size = (uint)data.Length;
        if (CM_Get_DevNode_Registry_Property(node, 14, out _, data, ref size, 0) != 0 || CM_Get_Parent(out uint parent, node, 0) != 0) return "";
        return DeviceId(parent) + "|" + Encoding.Unicode.GetString(data, 0, (int)size).TrimEnd('\0');
    }
    private static bool NormalDeviceReturned(string location)
    {
        if (CM_Get_Device_ID_List_Size(out uint size, null, 0x100) != 0) return false;
        char[] buffer = new char[size];
        if (CM_Get_Device_ID_List(null, buffer, size, 0x100) != 0) return false;
        foreach (string id in new string(buffer).Split('\0', StringSplitOptions.RemoveEmptyEntries))
            if (id.StartsWith("USB\\VID_301A&PID_5555\\", StringComparison.OrdinalIgnoreCase) &&
                CM_Locate_DevNode(out uint node, id, 0) == 0 && Location(node) == location) return true;
        return false;
    }

    private sealed class FlashDevice : IDisposable
    {
        private readonly SafeFileHandle handle;
        private readonly uint alignment;
        internal FlashDevice(string path)
        {
            handle = CreateFile(path, 0xC0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
            if (handle.IsInvalid) throw new Win32Exception();
            try
            {
                byte[] descriptor = Query(0);
                string Field(int offset)
                {
                    int start = BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(offset));
                    if (start <= 0 || start >= descriptor.Length) return "";
                    return Encoding.ASCII.GetString(descriptor, start, Array.IndexOf(descriptor, (byte)0, start) - start).Trim();
                }
                if (BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(28)) != 7 || Field(12) != "SMTLINK" || Field(16) != "DEVICE")
                    throw new IOException("O destino não corresponde ao bootloader Cube Baby.");
                alignment = BinaryPrimitives.ReadUInt32LittleEndian(Query(1).AsSpan(16));
            }
            catch { handle.Dispose(); throw; }
        }
        private byte[] Query(int property)
        {
            byte[] query = new byte[12], result = new byte[4096];
            BinaryPrimitives.WriteInt32LittleEndian(query, property);
            if (!DeviceIoControlQuery(handle, 0x2D1400, query, query.Length, result, result.Length, out _, IntPtr.Zero)) throw new Win32Exception();
            return result;
        }
        internal byte[] Send(byte[] cdb, byte[]? output = null, int readLength = 0)
        {
            int length = output?.Length ?? readLength;
            IntPtr data = length == 0 ? IntPtr.Zero : VirtualAlloc(IntPtr.Zero, (nuint)length, 0x3000, 4);
            try
            {
                if (length > 0 && (data == IntPtr.Zero || ((ulong)data.ToInt64() & alignment) != 0)) throw new IOException("Não foi possível alocar o buffer de transferência.");
                if (output != null) Marshal.Copy(output, 0, data, length);
                var packet = new ScsiPacket { Sense = new byte[32], Spt = new ScsiPassThrough
                {
                    Length = (ushort)Marshal.SizeOf<ScsiPassThrough>(), CdbLength = 16, SenseInfoLength = 32,
                    DataIn = (byte)(readLength > 0 ? 1 : 0), DataTransferLength = (uint)length, TimeOutValue = 10,
                    DataBuffer = data, SenseInfoOffset = (uint)Marshal.OffsetOf<ScsiPacket>(nameof(ScsiPacket.Sense)).ToInt32(), Cdb = cdb
                }};
                int size = Marshal.SizeOf<ScsiPacket>();
                IntPtr packetBuffer = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(packet, packetBuffer, false);
                    if (!DeviceIoControl(handle, 0x4D014, packetBuffer, size, packetBuffer, size, out _, IntPtr.Zero)) throw new Win32Exception();
                    packet = Marshal.PtrToStructure<ScsiPacket>(packetBuffer);
                }
                finally { Marshal.FreeHGlobal(packetBuffer); }
                if (packet.Spt.ScsiStatus != 0 || packet.Spt.DataTransferLength != length)
                    throw new IOException($"Resposta SCSI inválida ({packet.Spt.ScsiStatus:X2}), sense {Convert.ToHexString(packet.Sense)}. Operação interrompida.");
                byte[] result = new byte[readLength];
                if (readLength > 0) Marshal.Copy(data, result, 0, readLength);
                return result;
            }
            finally { if (data != IntPtr.Zero) VirtualFree(data, 0, 0x8000); }
        }
        public void Dispose() => handle.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)] private struct InterfaceData { public int Size; public Guid Class; public uint Flags; public IntPtr Reserved; }
    [StructLayout(LayoutKind.Sequential)] private struct DeviceInfo { public int Size; public Guid Class; public uint Node; public IntPtr Reserved; }
    [StructLayout(LayoutKind.Sequential)] private struct ScsiPassThrough
    {
        public ushort Length;
        public byte ScsiStatus, PathId, TargetId, Lun, CdbLength, SenseInfoLength, DataIn;
        public uint DataTransferLength, TimeOutValue;
        public IntPtr DataBuffer;
        public uint SenseInfoOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] Cdb;
    }
    [StructLayout(LayoutKind.Sequential)] private struct ScsiPacket
    {
        public ScsiPassThrough Spt; public uint Filler;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] Sense;
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateFileW")] private static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool DeviceIoControl(SafeFileHandle handle, uint code, IntPtr input, int inputSize, IntPtr output, int outputSize, out uint returned, IntPtr overlapped);
    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl")] private static extern bool DeviceIoControlQuery(SafeFileHandle handle, uint code, byte[] input, int inputSize, byte[] output, int outputSize, out uint returned, IntPtr overlapped);
    [DllImport("kernel32.dll")] private static extern IntPtr VirtualAlloc(IntPtr address, nuint size, uint allocation, uint protection);
    [DllImport("kernel32.dll")] private static extern bool VirtualFree(IntPtr address, nuint size, uint type);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "SetupDiGetClassDevsW")] private static extern IntPtr SetupDiGetClassDevs(ref Guid guid, string? enumerator, IntPtr parent, uint flags);
    [DllImport("setupapi.dll", SetLastError = true)] private static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr info, ref Guid guid, uint index, ref InterfaceData data);
    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "SetupDiGetDeviceInterfaceDetailW")] private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr set, ref InterfaceData data, IntPtr detail, uint size, out uint required, IntPtr info);
    [DllImport("setupapi.dll")] private static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
    [DllImport("cfgmgr32.dll")] private static extern uint CM_Get_Parent(out uint parent, uint node, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_Device_IDW")] private static extern uint CM_Get_Device_ID(uint node, StringBuilder id, int length, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_DevNode_Registry_PropertyW")] private static extern uint CM_Get_DevNode_Registry_Property(uint node, uint property, out uint type, byte[] buffer, ref uint size, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_Device_ID_List_SizeW")] private static extern uint CM_Get_Device_ID_List_Size(out uint size, string? filter, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Get_Device_ID_ListW")] private static extern uint CM_Get_Device_ID_List(string? filter, [Out] char[] buffer, uint length, uint flags);
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, EntryPoint = "CM_Locate_DevNodeW")] private static extern uint CM_Locate_DevNode(out uint node, string id, uint flags);
}
