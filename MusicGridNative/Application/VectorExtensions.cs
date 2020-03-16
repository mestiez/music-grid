using SFML.System;
using System.Numerics;

namespace MusicGrid
{
    public static class VectorExtensions
    {
        public static Vector2 ToNumerics(this Vector2f input) => new Vector2(input.X, input.Y);

        public static Vector2f ToSFML(this Vector2 input) => new Vector2f(input.X, input.Y);
    }
}
