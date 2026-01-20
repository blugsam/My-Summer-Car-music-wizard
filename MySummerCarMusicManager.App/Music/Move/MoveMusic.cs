using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.App.Music.Move;

public sealed class MoveMusic
{
    public void Handle(PlaylistCollection playlist, int oldIndex, int newIndex)
    {
        playlist.Move(oldIndex, newIndex);
    }
}
