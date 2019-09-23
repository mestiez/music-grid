using Newtonsoft.Json;
using SFML.System;
using System;
using System.IO;

namespace MusicGridNative
{
    [Serializable]
    public class Configuration
    {
        public static Configuration CurrentConfiguration = new Configuration();
        public const string DefaultPath = "config.json";

        public static void LoadConfiguration(string path = DefaultPath)
        {
            try
            {
                string raw = File.ReadAllText(path);
                Configuration parsed = JsonConvert.DeserializeObject<Configuration>(raw);
                CurrentConfiguration = parsed;
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case FileNotFoundException _:
                        ConsoleEntity.Show("Configuration not found at " + path);
                        break;
                    case JsonException _:
                        ConsoleEntity.Show("Invalid configuration at " + path);
                        break;
                    default:
                        ConsoleEntity.Show(e.ToString());
                        break;
                }

            }
        }

        public static void SaveConfiguration(string path = DefaultPath)
        {
            try
            {
                string raw = JsonConvert.SerializeObject(CurrentConfiguration, Formatting.Indented);
                File.WriteAllText(path, raw);
            }
            catch (Exception e)
            {
                ConsoleEntity.Show(e.ToString());
            }
        }

        //State
        public uint WindowWidth = 500;
        public uint WindowHeight = 500;
        public uint FramerateCap = 300;
        public float Zoom = 1;
        public Vector2f Pan = default;

        //Settings
        public bool OpenLastOpenedAtLaunch = true;
        public float ZoomSensitivity = 1;
        public float ZoomLowerBound = .01f;
        public float ZoomUpperBound = 10f;
        public float SnappingSize = 8;
        public float TextClarity = 72;
    }
}
