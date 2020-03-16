using System;

namespace Shared
{
    [Serializable]
    public struct Color : IEquatable<Color>
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public bool Equals(Color other) =>
                other.R == R &&
                other.G == G &&
                other.B == B &&
                other.A == A;

        public override bool Equals(object obj)
        {
            if (obj is Color color)
                return Equals(color);
            else return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"R {R}, G {G}, B {B}, A {A}";

        public Color(byte red, byte green, byte blue)
        {
            this = new Color(red, green, blue, byte.MaxValue);
        }

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public Color(Color color)
        {
            this = new Color(color.R, color.G, color.B, color.A);
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }

        public static Color operator +(Color left, Color right)
        {
            return new Color((byte)Math.Min(left.R + right.R, 255), (byte)Math.Min(left.G + right.G, 255), (byte)Math.Min(left.B + right.B, 255), (byte)Math.Min(left.A + right.A, 255));
        }

        public static Color operator -(Color left, Color right)
        {
            return new Color((byte)Math.Max(left.R - right.R, 0), (byte)Math.Max(left.G - right.G, 0), (byte)Math.Max(left.B - right.B, 0), (byte)Math.Max(left.A - right.A, 0));
        }

        public static Color operator *(Color left, Color right)
        {
            return new Color((byte)(left.R * right.R / 255), (byte)(left.G * right.G / 255), (byte)(left.B * right.B / 255), (byte)(left.A * right.A / 255));
        }

        public uint ToInteger()
        {
            return (uint)((R << 24) | (G << 16) | (B << 8) | A);
        }

        public static implicit operator uint(Color c) => c.ToInteger();
    }
}
