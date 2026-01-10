namespace MySummerCarMusicManager.Infrastructure.DataAccess;

public interface IFileReader
{
    IEnumerable<string> ReadAudioFiles(string folderPath);
    void RenameFile(string sourcePath, string destinationPath);
}
