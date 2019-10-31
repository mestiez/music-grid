using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public struct Utilities
    {
        private static Random random = new Random();

        public static bool IsInside(Vector2f point, Vector2f topleft, Vector2f size)
        {
            if (point.X < topleft.X) return false;
            if (point.Y < topleft.Y) return false;

            if (point.X > topleft.X + size.X) return false;
            if (point.Y > topleft.Y + size.Y) return false;

            return true;
        }

        //from https://weblog.west-wind.com/posts/2010/Dec/20/Finding-a-Relative-Path-in-NET
        public static string GetRelativePath(string fullPath, string basePath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");
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

        public static bool IsTooBright(Color color)
        {
            return ((0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255) > 0.6;
        }

        public static byte RandomByte()
        {
            return (byte)random.Next(0, 256);
        }
    }
}
