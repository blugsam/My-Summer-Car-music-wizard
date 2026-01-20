namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public interface IAudioTranscoder
{
    public void Transcode(string input, string output);
}
