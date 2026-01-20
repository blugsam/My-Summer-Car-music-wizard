namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public interface IFileSystem
{
    IEnumerable<string> GetFiles(string folderPath, string pattern = "Track*.ogg");
    void MoveFile(string sourcePath, string destinationPath);
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);
    void DeleteFile(string sourcePath);
    void CreateFolder(string folderPath);
    void DeleteFolder(string folderPath, bool isRecursive);
    void WriteText(string path, string content);
    string ReadText(string path);
    bool IsFileExists(string folderPath);
    bool IsFolderExists(string folderPath);
}
