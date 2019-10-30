namespace MusicGrid
{
    public struct Globals
    {
        public readonly float Time;
        public readonly float DeltaTime;

        public Globals(float time, float deltaTime)
        {
            Time = time;
            DeltaTime = deltaTime;
        }
    }
}
