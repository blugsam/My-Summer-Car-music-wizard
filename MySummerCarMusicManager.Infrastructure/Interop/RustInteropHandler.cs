using System.Runtime.InteropServices;
using MySummerCarMusicManager.Infrastructure.Interop;

namespace MySummerCarMusicManager.NET.Interop;

internal sealed partial class RustInteropHandler : IRustInteropHandler
{
    private const string DLL_NAME = "decode_rs.dll";

    [LibraryImport(DLL_NAME, EntryPoint = "convert_audio_to_ogg", StringMarshalling = StringMarshalling.Utf8)]
    private static partial FfiMessage ConvertAudioToOgg(string input, string output);

    [LibraryImport(DLL_NAME, EntryPoint = "free_ffi_message_error")]
    private static partial void FreeFfiMessageError(IntPtr ptr);

    public void HandleConvert(string inputPath, string outputPath)
    {
        var result = ConvertAudioToOgg(inputPath, outputPath);

        if (!result.IsSuccess) HandleError(result);
    }

    private static void HandleError(FfiMessage result)
    {
        var errorMessage = "Unknown dll Error";

        try
        {
            if (result.ErrorMessage != IntPtr.Zero)
            {
                errorMessage = Marshal.PtrToStringUTF8(result.ErrorMessage);
            }
        }
        finally
        {
            if (result.ErrorMessage != IntPtr.Zero)
            {
                FreeFfiMessageError(result.ErrorMessage);
            }
        }

        throw new ApplicationException($"Transcoding error: {errorMessage}");
    }
}
