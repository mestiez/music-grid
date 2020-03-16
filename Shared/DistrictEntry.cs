using System;

namespace Shared
{
    [Serializable]
    public class DistrictEntry
    {
        public string Name;
        public string Path;

        public DistrictEntry(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public override string ToString() => Name;
    }
}
