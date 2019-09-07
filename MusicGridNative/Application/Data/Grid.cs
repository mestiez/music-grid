using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MusicGridNative
{
    [Serializable]
    public class Grid
    {
        public string Name;
        public List<District> Districts = new List<District>();

        [JsonIgnore]
        public string RootPath;

        public Grid(string name)
        {
            Name = name;
        }
    }
}
