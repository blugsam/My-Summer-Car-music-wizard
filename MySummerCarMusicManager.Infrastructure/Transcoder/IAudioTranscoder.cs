namespace MySummerCarMusicManager.Infrastructure.Transcoder;

public interface IAudioTranscoder
{
    public void TranscodeToOggAsync(string input, string output);
}
