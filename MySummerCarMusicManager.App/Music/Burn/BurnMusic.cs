using MySummerCarMusicManager.Infrastructure.DataAccess;

namespace MySummerCarMusicManager.App.Music.Burn;

public sealed class BurnMusic(IAudioTranscoder transcoder, IFileSystem fileSystem)
{
    public async Task Handle(IEnumerable<Domain.Music> playlist, string targetGameFolder, CancellationToken cancellationToken)
    {
        var tempRoot = Path.GetTempPath();
        var tempFolder = Path.Combine(tempRoot, $"MSC_Manager_{Guid.NewGuid()}");

        try
        {
            fileSystem.CreateFolder(tempFolder);

            var tracks = playlist.ToList();

            await Task.Run(() =>
            {
                var options = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.ForEach(tracks, options, (track) =>
                {
                    var targetFileName = $"Track{track.Position}.ogg";
                    var targetPath = Path.Combine(tempFolder, targetFileName);

                    if (!track.IsReady)
                    {
                        transcoder.Transcode(track.Path, targetPath);
                    }
                    else
                    {
                        fileSystem.CopyFile(track.Path, targetPath, overwrite: true);
                    }
                });

            }, cancellationToken);

            CommitChanges(tempFolder, targetGameFolder);
        }
        finally
        {
            fileSystem.DeleteFolder(tempFolder, isRecursive: true);
        }
    }

    private void CommitChanges(string tempFolder, string targetFolder)
    {
        var oldFiles = fileSystem.GetFiles(targetFolder);
        foreach (var file in oldFiles)
        {
            fileSystem.DeleteFile(file);
        }

        var newFiles = fileSystem.GetFiles(tempFolder);
        foreach (var file in newFiles)
        {
            var fileName = Path.GetFileName(file);
            var destPath = Path.Combine(targetFolder, fileName);

            fileSystem.MoveFile(file, destPath);
        }
    }
}
