using Newtonsoft.Json;
using System;

namespace MusicGridNative
{
    [Serializable]
    public class DistrictEntry
    {
        public string Name;
        public string RelativePath;

        [JsonIgnore]
        public bool IsDirty;

        public DistrictEntry(string name, string relativePath)
        {
            Name = name;
            RelativePath = relativePath;
        }
    }
}
