namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public sealed class FileSystem : IFileSystem
{
    private static readonly HashSet<string> _supportedFormats = new HashSet<string> { "mp3", "wav", "flac", "m4a", "aac", "wma" };

    public IEnumerable<string> GetFiles(string folderPath, string pattern = "Track*.ogg")
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("The folder path's empty.", nameof(folderPath));

        return Directory.EnumerateFiles(folderPath, pattern)
            .Where(filePath =>
            {
                var extension = Path.GetExtension(filePath);

                return !string.IsNullOrEmpty(extension) && _supportedFormats.Contains(extension);
            });
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path is empty.", nameof(sourcePath));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path is empty.", nameof(destinationPath));

        File.Move(sourcePath, destinationPath);
    }

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path is empty.", nameof(sourcePath));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path is empty.", nameof(destinationPath));

        File.Copy(sourcePath, destinationPath, overwrite);
    }

    public void DeleteFile(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Source path is empty.", nameof(folderPath));

        if (!File.Exists(folderPath))
            throw new FileNotFoundException("File not found", folderPath);

        File.Delete(folderPath);
    }

    public void CreateFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path is empty.", nameof(folderPath));

        Directory.CreateDirectory(folderPath);
    }

    public void DeleteFolder(string folderPath, bool isRecursive)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path is empty.", nameof(folderPath));

        Directory.Delete(folderPath, isRecursive);
    }
}
