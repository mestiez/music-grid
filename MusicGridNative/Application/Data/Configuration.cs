using Newtonsoft.Json;
using SFML.System;
using System;
using System.IO;

namespace MusicGrid
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
                ConsoleEntity.Log("Configuration succesfully loaded", "CONFIG");
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case FileNotFoundException fe:
                        ConsoleEntity.Log("Configuration not found at " + path, "CONFIG");
                        ConsoleEntity.Log(fe.Message, "CONFIG");
                        break;
                    case JsonException je:
                        ConsoleEntity.Log("Invalid configuration at " + path, "CONFIG");
                        ConsoleEntity.Log(je.Message, "CONFIG");
                        break;
                    default:
                        ConsoleEntity.Log(e.Message.ToString());
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
                ConsoleEntity.Log("Configuration succesfully saved", "CONFIG");
            }
            catch (Exception e)
            {
                ConsoleEntity.Log(e.Message, "CONFIG");
                return;
            }
        }

        //Settings
        public int FramerateCap = 60;
        public bool OpenLastOpenedAtLaunch = true;
        public float ZoomSensitivity = 1;
        public float ZoomLowerBound = .01f;
        public float ZoomUpperBound = 10f;
        public float SnappingSize = 8;
        public float SelectionWaveSmoothness = 0.3f;
        public float SelectionWaveFrequency = 1f;
        public float SelectionWaveWave = .0001f;
        [RequiresRestart]
        public float AntiAliasing = 4;
        [RequiresRestart]
        public float TextClarity = 72;

        //State
        [RequiresRestart]
        public bool Maximised = false;
        [RequiresRestart]
        public int WindowWidth = 700;
        [RequiresRestart]
        public int WindowHeight = 500;
        public float Zoom = 1;
        public Vector2f Pan = default;
        [RequiresRestart]
        public District[] Districts = { };

        public class RequiresRestartAttribute : Attribute { }
    }
}
