using System;
using System.Runtime.InteropServices;

namespace MySummerCarMusicManager.NET;

internal static class RustInterop
{
    private const string LIB_ROOT_PATH = "C:\\Users\\mijiy\\source\\repos\\.net\\MySummerCarMusicManager.NET\\MySummerCarMusicManagerDecodeRs\\target\\release\\decode_rs.dll";

    [DllImport(LIB_ROOT_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr get_rust_message();

    [DllImport(LIB_ROOT_PATH, CallingConvention = CallingConvention.Cdecl)]
    private static extern void free_rust_message(IntPtr ptr);

    internal static string GetMessage()
    {
        IntPtr ptr = get_rust_message();
        try
        {
            return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);

            return string.Empty;
        }
        finally
        {
            free_rust_message(ptr);
        }
    }
}
