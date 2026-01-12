using MySummerCarMusicManager.Infrastructure.DataAccess;

namespace MySummerCarMusicManager.Application.Music.Add;

public sealed class AddMusic(IMetadataReader metadataReader)
{
    public async Task<List<Domain.Music>> Handle(IEnumerable<string> filaPaths, int startingPosition, CancellationToken cancellationToken)
    {
        var paths = filaPaths.ToList();

        if (paths.Count == 0) return [];

        return await Task.Run(() =>
        {
            return paths
                .AsParallel()
                .AsOrdered()
                .WithCancellation(cancellationToken)
                .Select((path, index) =>
                {
                    var metadata = metadataReader.ReadMetadata(path);
                    var fileName = Path.GetFileNameWithoutExtension(path);

                    int newPosition = startingPosition + 1 + index;

                    return new Domain.Music(
                        title: metadata?.Title ?? fileName,
                        authors: metadata?.Artists ?? [],
                        duration: metadata?.Duration ?? TimeSpan.Zero,
                        path: path,
                        position: newPosition);
                })
                .ToList();

        }, cancellationToken);
    }
}
