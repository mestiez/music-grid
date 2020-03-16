namespace MusicGrid
{
    public static class ColorExtensions
    {
        public static Shared.Color ToShared(this SFML.Graphics.Color input) => new Shared.Color(input.R, input.G, input.B, input.A);

        public static SFML.Graphics.Color ToSFML(this Shared.Color input) => new SFML.Graphics.Color(input.R, input.G, input.B, input.A);
    }
}
