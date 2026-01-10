namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public interface IMetadataReader
{
    public AudioMetadata? ReadMetadata(string path);
}
