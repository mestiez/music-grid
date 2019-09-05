using SFML.Graphics;
using SFML.System;
using System;

namespace MusicGridNative
{
    [Serializable]
    public class District
    {
        public string Name = "Untitled";
        public Vector2f Position = new Vector2f(0, 0);
        public Vector2f Size = new Vector2f(256, 256);
        public Color Color = new Color(52, 168, 235);
    }
}
