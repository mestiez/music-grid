namespace MusicGrid
{
    public class MusicControlsEntity : Entity
    {
        public MusicPlayer MusicPlayer { get; } = new MusicPlayer();

        public MusicControlsEntity()
        {
            MusicPlayer.OnFailure += (o, e) =>
            {
                World.Add(new DialogboxEntity(e.Message));
            };
        }
    }
}
