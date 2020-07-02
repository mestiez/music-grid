using System;
using System.Collections.Generic;
using System.Numerics;

namespace Shared
{
    [Serializable]
    public class District
    {
        public string Name = "Untitled";
        public Vector2 Position = new Vector2(0, 0);
        public Vector2 Size = new Vector2(256, 256);
        public Color Color = new Color(52, 168, 235);
        public bool Locked;
        public bool Muted;
        public string BackgroundPath;

        [NonSerialized]
        public Color DisplayColor = new Color(52, 168, 235);

        [NonSerialized]
        public bool Dirty = true;

        public List<DistrictEntry> Entries = new List<DistrictEntry>();

        public District(string name, Vector2 position, Vector2 size, Color color, bool locked, bool muted, string backgroundpath)
        {
            Name = name;
            Position = position;
            Size = size;
            Color = color;
            Locked = locked;
            Muted = muted;
            BackgroundPath = backgroundpath;
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
