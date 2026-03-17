using System.Runtime.InteropServices;

namespace MySummerCarMusicManager.Infrastructure.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct FfiMessage
{
    public readonly byte IsSuccessValue; //rust u8 == C# byte so i use byte instead of bool for fast marshalling via 'LibraryImport'
    public readonly IntPtr ErrorMessage;

    public bool IsSuccess => IsSuccessValue != 0;
}
