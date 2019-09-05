using System;

namespace MusicGridNative
{
    [Serializable]
    public struct Configuration
    {
        public uint WindowWidth;
        public uint WindowHeight;
        public bool OpenLastOpenedAtLaunch;

        public Configuration(uint windowWidth = 430, uint windowHeight = 430, bool openLastOpenedAtLaunch = true)
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            OpenLastOpenedAtLaunch = openLastOpenedAtLaunch;
        }
    }
}
