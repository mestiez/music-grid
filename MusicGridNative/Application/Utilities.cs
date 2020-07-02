using SFML.Graphics;
using SFML.System;
using System;

namespace MusicGrid
{
    public struct Utilities
    {
        private static readonly Random random = new Random();

        public static bool IsInside(System.Numerics.Vector2 point, System.Numerics.Vector2 topleft, System.Numerics.Vector2 size)
        {
            return IsInside(point.ToSFML(), topleft.ToSFML(), size.ToSFML());
        }

        public static bool IsInside(Vector2f point, Vector2f topleft, Vector2f size)
        {
            if (point.X < topleft.X) return false;
            if (point.Y < topleft.Y) return false;

            if (point.X > topleft.X + size.X) return false;
            if (point.Y > topleft.Y + size.Y) return false;

            return true;
        }

        public static bool AreTouching(System.Numerics.Vector2 aPos, System.Numerics.Vector2 aSize, System.Numerics.Vector2 bPos, System.Numerics.Vector2 bSize)
        {
            float aLeft = aPos.X;
            float bLeft = bPos.X;
            float aTop = aPos.Y;
            float bTop = bPos.Y;

            float aRight = aLeft + aSize.X;
            float bRight = bLeft + bSize.X;
            float aBottom = aTop - aSize.Y;
            float bBottom = bTop - bSize.Y;

            return (aLeft < bRight && aRight > bLeft && aTop > bBottom && aBottom < bTop);
        }

        public static Color GetMainColour(Texture texture)
        {
            const int stride = 4;
            var pixels = texture.CopyToImage().Pixels;
            float division = 0;

            float aR = 0;
            float aG = 0;
            float aB = 0;
            int index = 0;

            for (int y = 0; y < texture.Size.Y; y++)
            {
                for (int x = 0; x < texture.Size.X; x++)
                {
                    byte r = pixels[index];
                    byte g = pixels[index + 1];
                    byte b = pixels[index + 2];

                    float v = (float)Math.Pow(GetVibrance(r, g, b), 4);
                    division += v;

                    aR += r * v;
                    aG += g * v;
                    aB += b * v;

                    index += stride;
                }
            }

            return new Color(
                (byte)(int)Clamp(aR / division, 0, 255),
                (byte)(int)Clamp(aG / division, 0, 255),
                (byte)(int)Clamp(aB / division, 0, 255),
                255);
        }

        /// <summary>
        /// range 0-1
        /// </summary>
        public static float GetVibrance(Color color)
        {
            return GetVibrance(color.R, color.G, color.B);
        }

        /// <summary>
        /// range 0-1
        /// </summary>
        static float GetVibrance(byte R, byte G, byte B)
        {
            float average = (R + G + B) / 3f;
            float r = Abs(average - R);
            float g = Abs(average - G);
            float b = Abs(average - B);
            return Math.Min(r, Math.Min(g, b)) / 255f;
        }

        //from https://weblog.west-wind.com/posts/2010/Dec/20/Finding-a-Relative-Path-in-NET
        public static string GetRelativePath(string fullPath, string basePath)
        {
            if (!basePath.EndsWith("\\"))
                basePath += "\\";
            Uri relativeUri = new Uri(basePath).MakeRelativeUri(new Uri(fullPath));
            return Uri.UnescapeDataString(relativeUri.ToString().Replace("/", "\\"));
        }

        public static float SquaredMagnitude(Vector2f vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y;
        }

        public static string ToHumanReadableString(TimeSpan span)
        {
            const string s = ":";
            string final = $"{doubleOh(span.Minutes)}{s}{doubleOh(span.Seconds)}";
            if (span.Hours != 0)
                final = doubleOh(span.Hours) + s + final;
            return final;
            string doubleOh(int i)
            {
                string iss = i.ToString();
                return iss.Length == 1 ? "0" + iss : iss;
            }
        }

        public static float Abs(float i)
        {
            return i < 0 ? -i : i;
        }

        public static Vector2f Round(Vector2f i)
        {
            return new Vector2f((int)i.X, (int)i.Y);
        }

        public static float CalculateEvenSpaceGap(float totalSpace, float occupiedSpace, int elementCount, float margin = 10)
        {
            return (totalSpace - occupiedSpace - (margin * 2)) / (elementCount - 1);
        }

        public static float Clamp(float value, float lower, float upper)
        {
            return Math.Max(Math.Min(value, upper), lower);
        }

        public static int Clamp(int value, int lower, int upper)
        {
            return Math.Max(Math.Min(value, upper), lower);
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
            return GetBrightness(color.R, color.G, color.B) > 0.6f;
        }

        public static float GetBrightness(byte r, byte g, byte b)
        {
            return ((0.2126f * r + 0.7152f * g + 0.0722f * b) / 255f);
        }

        public static byte RandomByte()
        {
            return (byte)random.Next(0, 256);
        }

        public static float RandomFloat()
        {
            return (float)random.NextDouble();
        }

        public static bool IsActive => !Input.Minimised && (!Configuration.CurrentConfiguration.EcoRender || Input.WindowHasFocus);

        public static string ColourToHexString(Color color)
        {
            string r = color.R.ToString("X2");
            string g = color.G.ToString("X2");
            string b = color.B.ToString("X2");
            return "#" + r + g + b;
        }

        public static Color HexStringToColour(string color)
        {
            if (color.StartsWith("#"))
                color = color.Substring(1);
            else if (color.StartsWith("0x"))
                color = color.Substring(2);

            byte r = byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color(r, g, b);
        }

        public static bool TryParseHexColour(string hex, out Color color)
        {
            color = default;
            try
            {
                color = HexStringToColour(hex);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
