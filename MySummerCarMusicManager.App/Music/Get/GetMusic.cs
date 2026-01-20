using MySummerCarMusicManager.Infrastructure.DataAccess;

namespace MySummerCarMusicManager.App.Music.Get;

public sealed class GetMusic(IFileSystem fileReader, IMetadataReader metadataReader)
{
    private static readonly HashSet<string> _supportedFormats = new(StringComparer.OrdinalIgnoreCase) { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".wma", ".ogg" };

    public async Task<List<Domain.Music>> Handle(string folderPath, CancellationToken cancellationToken)
    {
        var files = await LoadTracksFromDisk(folderPath, cancellationToken);

        var sortedFiles = files
            .OrderBy(x => x.Position == 0 ? int.MaxValue : x.Position)
            .ToList();

        return NormalizePositions(sortedFiles);
    }

    private async Task<List<Domain.Music>> LoadTracksFromDisk(string folderPath, CancellationToken cancellationToken)
    {
        var allFiles = fileReader.GetFiles(folderPath, "*").ToList();

        var musicFiles = allFiles
            .Where(path => _supportedFormats.Contains(Path.GetExtension(path)))
            .ToList();

        if (musicFiles.Count == 0) return new List<Domain.Music>();

        return await Task.Run(() =>
        {
            var rawList = musicFiles
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(filePath =>
                {
                    var meta = metadataReader.ReadMetadata(filePath);

                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    var position = TryParseTrackNumber(fileName);

                    var title = meta?.Title ?? fileName;
                    var authors = meta?.Artists ?? Array.Empty<string>();
                    var duration = meta?.Duration ?? TimeSpan.Zero;

                    return new Domain.Music(title, authors, duration, filePath, position);
                })
                .ToList();

            return rawList;

        }, cancellationToken);
    }

    private static int TryParseTrackNumber(string fileName)
    {
        if (fileName.StartsWith("Track", StringComparison.OrdinalIgnoreCase) && int.TryParse(fileName.AsSpan(5), out var number))
            return number;

        return 0;
    }

    private static List<Domain.Music> NormalizePositions(List<Domain.Music> list)
    {
        var currentMax = list.Any(x => x.Position > 0) ? list.Max(x => x.Position) : 0;

        foreach (var item in list)
        {
            if (item.Position == 0)
            {
                currentMax++;
                item.Position = currentMax;
            }
        }

        return list;
    }
}
