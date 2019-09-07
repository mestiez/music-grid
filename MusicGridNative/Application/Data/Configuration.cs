using System;

namespace MusicGridNative
{
    [Serializable]
    public class Configuration
    {
        public static Configuration CurrentConfiguration = new Configuration();

        //State
        public uint WindowWidth = 500;
        public uint WindowHeight = 500;
        public float Zoom = 1;
        public float PanX = 0;
        public float PanY = 0;

        //Settings
        public bool OpenLastOpenedAtLaunch = true;
        public float ZoomIntensity = 1;
        public float SnappingSize = 8;
    }
}
