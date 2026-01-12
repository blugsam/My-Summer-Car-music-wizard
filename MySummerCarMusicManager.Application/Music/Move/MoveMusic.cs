using MySummerCarMusicManager.Domain;

namespace MySummerCarMusicManager.Application.Music.Move;

public static class MoveMusic
{
    public static void Handle(PlaylistCollection playlist, int oldIndex, int newIndex)
    {
        playlist.Move(oldIndex, newIndex);
    }
}
