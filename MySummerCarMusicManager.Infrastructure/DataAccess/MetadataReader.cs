namespace MySummerCarMusicManager.Infrastructure.DataAccess;

internal sealed class MetadataReader : IMetadataReader
{
    public AudioMetadata? ReadMetadata(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentNullException(nameof(sourcePath), "The folder path's empty.");

        using var file = TagLib.File.Create(sourcePath);

        var title = file.Tag.Title;
        var artists = file.Tag.Performers ?? Array.Empty<string>();
        var duration = file.Properties.Duration;

        return new AudioMetadata(title, artists, duration);
    }
}

public sealed record AudioMetadata(string Title, string[] Artists, TimeSpan Duration);
