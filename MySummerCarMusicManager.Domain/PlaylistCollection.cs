using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MySummerCarMusicManager.Domain;

public sealed class PlaylistCollection : ObservableCollection<Music>
{
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);

        RecalculatePositions();
    }

    private void RecalculatePositions()
    {
        for (var i = 0; i < Count; i++)
        {
            var track = this[i];
            var correctPosition = i + 1;

            if (track.Position != correctPosition)
            {
                track.Position = correctPosition;
            }
        }
    }

    public void RemoveMany(IEnumerable<Music> itemsToRemove)
    {
        var list = itemsToRemove.ToList();

        foreach (var item in list)
        {
            Remove(item);
        }
    }
}
