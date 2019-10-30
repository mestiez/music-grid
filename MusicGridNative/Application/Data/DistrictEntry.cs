using Newtonsoft.Json;
using System;

namespace MusicGrid
{
    [Serializable]
    public class DistrictEntry
    {
        public string Name;
        public string RelativePath;

        public DistrictEntry(string name, string relativePath)
        {
            Name = name;
            RelativePath = relativePath;
        }
    }
}
