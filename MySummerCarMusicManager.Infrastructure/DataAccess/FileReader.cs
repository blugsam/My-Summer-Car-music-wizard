namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public sealed class FileReader : IFileReader
{
    private readonly HashSet<string> _supportedFormats = new HashSet<string> { "Mp3", "Wav", "Flac", "M4a", "Aac", "Wma" };

    public IEnumerable<string> ReadAudioFiles(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return Enumerable.Empty<string>();

        try
        {
            return Directory.EnumerateFiles(folderPath)
                .Where(filePath =>
                {
                    var extension = Path.GetExtension(filePath);

                    return !string.IsNullOrEmpty(extension) && _supportedFormats.Contains(extension);
                });
        }
        catch (IOException)
        {
            return Enumerable.Empty<string>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<string>();
        }
    }

    public void RenameFile(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath)) return;

        File.Move(sourcePath, destinationPath);
    }
}
