using Newtonsoft.Json;
using OpenTK.Input;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public HashSet<Keybind> Keybinds = new HashSet<Keybind>{
            new Keybind(new Key[]{Key.Space}, "pause_or_play()")
        };

        //State
        [RequiresRestart]
        public bool Maximised = false;
        [RequiresRestart]
        public int WindowWidth = 700;
        [RequiresRestart]
        public int WindowHeight = 500;
        public Vector2f PlayerSize = new Vector2f();
        public float Zoom = 1;
        public Vector2f Pan = default;
        [RequiresRestart]
        public District[] Districts = { };

        public class RequiresRestartAttribute : Attribute { }

        public struct Keybind
        {
            public Key[] Keys;
            public string Script;

            public Keybind(Key[] keys, string script)
            {
                Keys = keys;
                Script = script;
            }

            public bool IsSatisfied()
            {
                for (int i = 0; i < Keys.Length; i++)
                {
                    if (i + 1 == Keys.Length)
                    {
                        if (!Input.IsKeyPressed(Keys[i])) return false;
                    }
                    else if (!Input.IsKeyHeld(Keys[i]))
                        return false;
                }
                return true;
            }
        }
    }
}
