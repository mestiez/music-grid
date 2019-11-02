using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;

namespace MusicGrid
{
    [Serializable]
    public class District
    {
        public string Name = "Untitled";
        public Vector2f Position = new Vector2f(0, 0);
        public Vector2f Size = new Vector2f(256, 256);
        public Color Color = new Color(52, 168, 235);

        public List<DistrictEntry> Entries = new List<DistrictEntry>();

        public District(string name, Vector2f position, Vector2f size, Color color)
        {
            Name = name;
            Position = position;
            Size = size;
            Color = color;
        }

        public District(string name)
        {
            Name = name;
        }

        public District()
        {
        }

        public override string ToString() => Name;
    }
}
