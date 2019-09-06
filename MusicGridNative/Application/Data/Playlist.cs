using System;
using System.Collections.Generic;

namespace MusicGridNative
{
    [Serializable]
    public class Playlist
    {
        public string Name;
        public List<District> Districts = new List<District>();
        public string RootPath;

        public Playlist(string name, string rootPath)
        {
            Name = name;
            RootPath = rootPath;
        }
    }
}
