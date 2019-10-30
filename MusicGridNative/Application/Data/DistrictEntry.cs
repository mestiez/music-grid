using Newtonsoft.Json;
using System;

namespace MusicGrid
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
    }
}
