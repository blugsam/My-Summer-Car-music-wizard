namespace MySummerCarMusicManager.Infrastructure.Transcoder;

public interface IAudioTranscoder
{
    public void Transcode(string input, string output);
}
