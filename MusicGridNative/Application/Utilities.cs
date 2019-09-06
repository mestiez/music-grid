using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public struct Utilities
    {
        public static bool IsInside(Vector2f point, Vector2f topleft, Vector2f size)
        {
            if (point.X < topleft.X) return false;
            if (point.Y < topleft.Y) return false;

            if (point.X > topleft.X + size.X) return false;
            if (point.Y > topleft.Y + size.Y) return false;

            return true;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a * (1 - t) + b * t;
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            byte red = (byte)Lerp(a.R, b.R, t);
            byte green = (byte)Lerp(a.G, b.G, t);
            byte blue = (byte)Lerp(a.B, b.B, t);
            byte alpha = (byte)Lerp(a.A, b.A, t);

            return new Color(red, green, blue, alpha);
        }
    }
}
