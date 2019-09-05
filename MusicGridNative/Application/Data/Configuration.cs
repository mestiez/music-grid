using System;

namespace MusicGridNative
{
    [Serializable]
    public struct Configuration
    {
        public uint WindowWidth;
        public uint WindowHeight;
        public bool OpenLastOpenedAtLaunch;

        public Configuration(uint windowWidth, uint windowHeight, bool openLastOpenedAtLaunch)
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            OpenLastOpenedAtLaunch = openLastOpenedAtLaunch;
        }
    }
}
