using MySummerCarMusicManager.Infrastructure.Interop;

namespace MySummerCarMusicManager.Infrastructure.DataAccess;

internal sealed class AudioTranscoder(IRustInteropHandler rustInteropHandler) : IAudioTranscoder
{
    public void Transcode(string input, string output)
    {
        rustInteropHandler.HandleConvert(input, output);
    }
}
