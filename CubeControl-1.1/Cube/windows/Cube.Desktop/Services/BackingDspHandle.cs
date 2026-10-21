using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Cube.Desktop.Services;

internal sealed class BackingDspHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private BackingDspHandle() : base(true) { }
    protected override bool ReleaseHandle() { Native.Destroy(handle); return true; }
    internal static class Native
    {
        private const string Library = "CubeBackingDsp";
        [DllImport(Library, EntryPoint = "backing_create", CallingConvention = CallingConvention.Cdecl)]
        internal static extern BackingDspHandle Create(int channels, int sampleRate);
        [DllImport(Library, EntryPoint = "backing_create_offline", CallingConvention = CallingConvention.Cdecl)]
        internal static extern BackingDspHandle CreateOffline(int channels, int sampleRate);
        [DllImport(Library, EntryPoint = "backing_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Destroy(IntPtr handle);
        [DllImport(Library, EntryPoint = "backing_prepare", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Prepare(BackingDspHandle handle, double speed, double semitones);
        [DllImport(Library, EntryPoint = "backing_seek", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Seek(BackingDspHandle handle, float[] input, int frames);
        [DllImport(Library, EntryPoint = "backing_process", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Process(BackingDspHandle handle, float[] input, int inputFrames, [Out] float[] output, int outputFrames);
    }
}

