namespace MySummerCarMusicManager.Infrastructure.DataAccess;

internal sealed class MetadataReader : IMetadataReader
{
    public AudioMetadata? ReadMetadata(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        try
        {
            using var file = TagLib.File.Create(path);

            var title = file.Tag.Title ?? null!;
            var artists = file.Tag.Performers ?? null!;
            var duration = file.Properties.Duration;

            return new AudioMetadata(title, artists, duration);
        }
        catch (TagLib.CorruptFileException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public sealed record AudioMetadata(string Title, string[] Artists, TimeSpan Duration);
