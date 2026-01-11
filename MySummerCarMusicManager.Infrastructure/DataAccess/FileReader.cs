namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public sealed class FileReader : IFileReader
{
    private static readonly HashSet<string> _supportedFormats = new HashSet<string> { "mp3", "wav", "flac", "m4a", "aac", "wma" };

    public IEnumerable<string> ReadAudioFiles(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("The folder path's empty.", nameof(folderPath));

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException("The folder doesn't exist.");

        return Directory.EnumerateFiles(folderPath)
            .Where(filePath =>
            {
                var extension = Path.GetExtension(filePath);

                return !string.IsNullOrEmpty(extension) && _supportedFormats.Contains(extension);
            });
    }

    public void RenameFile(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("File not found.", sourcePath);

        File.Move(sourcePath, destinationPath);
    }
}
