using Newtonsoft.Json;
using OpenTK.Input;
using SFML.System;
using Shared;
using System;
using System.Collections.Generic;
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
                ConsoleEntity.Log("Configuration succesfully loaded", typeof(Configuration).Name);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case FileNotFoundException fe:
                        ConsoleEntity.Log("Configuration not found at " + path, typeof(Configuration).Name);
                        ConsoleEntity.Log(fe.Message, typeof(Configuration).Name);
                        break;
                    case JsonException je:
                        ConsoleEntity.Log("Invalid configuration at " + path, typeof(Configuration).Name);
                        ConsoleEntity.Log(je.Message, typeof(Configuration).Name);
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
                ConsoleEntity.Log("Configuration succesfully saved", typeof(Configuration).Name);
            }
            catch (Exception e)
            {
                ConsoleEntity.Log(e.Message, typeof(Configuration).Name);
                return;
            }
        }

        //Settings
        public int FramerateCap = 60;
        public bool EcoRender = false;
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
        public Keybind[] Keybinds = {
            new Keybind(new Key[]{Key.Space}, $"{Functions.ToggleStream}()"),
            new Keybind(new Key[]{Key.Period}, $"{Functions.FitViewToSelection}()"),
            new Keybind(new Key[]{Key.KeypadPeriod}, $"{Functions.FitViewToSelection}()"),
            new Keybind(new Key[]{Key.Home}, $"{Functions.FitView}()"),

            new Keybind(new Key[]{Key.LAlt}, $"{Functions.EnableSnap}()", true),
            new Keybind(new Key[]{Key.LAlt}, $"{Functions.DisableSnap}()"),

            new Keybind(new Key[]{Key.A}, $"{Functions.SelectAll}()"),

            new Keybind(new Key[]{Key.LShift}, $"{Functions.EnableMultiselect}()"),
            new Keybind(new Key[]{Key.LShift}, $"{Functions.DisableMultiselect}()", true),

            new Keybind(new Key[]{Key.F3}, $"{Functions.ClearTextureCache}()"),
        };

        //State
        [RequiresRestart]
        public bool Maximised = false;
        [RequiresRestart]
        public int WindowWidth = 700;
        [RequiresRestart]
        public int WindowHeight = 500;
        public Vector2f PlayerSize = new Vector2f(0,0);
        public float Zoom = 1;
        public Vector2f Pan = default;
        [RequiresRestart]
        public District[] Districts = { };
        public bool Shuffle = true;
        public bool Repeat = true;

        public class RequiresRestartAttribute : Attribute { }

        public struct Keybind
        {
            public readonly Key[] Keys;
            public readonly string Script;
            public readonly bool ForKeyRelease;

            public Keybind(Key[] keys, string script, bool forKeyRelease = false)
            {
                ForKeyRelease = forKeyRelease;
                Keys = keys;
                Script = script;
            }

            public bool IsSatisfied()
            {
                for (int i = 0; i < Keys.Length; i++)
                {
                    if (i + 1 == Keys.Length)
                    {
                        if (ForKeyRelease)
                        {
                            if (!Input.IsKeyReleased(Keys[i]))
                                return false;
                        }
                        else if (!Input.IsKeyPressed(Keys[i]))
                            return false;
                    }
                    else if (!Input.IsKeyHeld(Keys[i]))
                        return false;
                }
                return true;
            }
        }
    }
}
