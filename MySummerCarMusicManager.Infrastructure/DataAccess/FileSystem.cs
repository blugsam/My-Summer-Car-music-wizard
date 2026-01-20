namespace MySummerCarMusicManager.Infrastructure.DataAccess;

internal sealed class FileSystem : IFileSystem
{
    public IEnumerable<string> GetFiles(string filesPath, string pattern = "Track*.ogg")
    {
        if (string.IsNullOrWhiteSpace(filesPath))
            throw new ArgumentNullException(nameof(filesPath), "The folder path's empty.");

        return Directory.EnumerateFiles(filesPath, pattern, SearchOption.TopDirectoryOnly);
    }

    public void MoveFile(string filePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Source path is empty.");

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath), "Destination path is empty.");

        File.Move(filePath, destinationPath);
    }

    public void CopyFile(string filePath, string destinationPath, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Source path is empty.");

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath), "Destination path is empty.");

        File.Copy(filePath, destinationPath, overwrite);
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Source path is empty.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        File.Delete(filePath);
    }

    public void CreateFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentNullException(nameof(folderPath), "Folder path is empty.");

        Directory.CreateDirectory(folderPath);
    }

    public void DeleteFolder(string folderPath, bool isRecursive)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentNullException(nameof(folderPath), "Folder path is empty.");

        Directory.Delete(folderPath, isRecursive);
    }

    public void WriteText(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Folder path is empty.");

        File.WriteAllText(filePath, content);
    }

    public string ReadText(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        return File.ReadAllText(filePath);
    }

    public bool IsFileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "Folder path is empty.");

        return File.Exists(filePath);
    }

    public bool IsFolderExists(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentNullException(nameof(folderPath), "Folder path is empty.");

        return File.Exists(folderPath);
    }
}
