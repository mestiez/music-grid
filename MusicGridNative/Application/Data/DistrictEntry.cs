using System;

namespace MusicGridNative
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
