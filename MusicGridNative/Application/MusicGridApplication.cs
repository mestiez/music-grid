using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MusicGrid
{
    public class MusicGridApplication
    {
        public static MusicGridApplication Main { get; private set; }

        public static Globals Globals { get; private set; }
        public static Assets Assets { get; private set; }

        private readonly RenderWindow renderWindow;

        public readonly World World;

        private float time;

        public MusicGridApplication(int width, int height, int framerate, string title)
        {
            Main = this;

            renderWindow = new RenderWindow(new VideoMode((uint)width, (uint)height), title, Styles.Resize | Styles.Close, new ContextSettings(8, 8, (uint)Configuration.CurrentConfiguration.AntiAliasing));
            renderWindow.SetFramerateLimit((uint)framerate);

            using (var ms = new MemoryStream())
            {
                Properties.Resources.favicon_32x32.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                Image image = new Image(ms);
                renderWindow.SetIcon(32, 32, image.Pixels);
                image.Dispose();
            }

            renderWindow.Closed += (object sender, EventArgs args) => { renderWindow.Close(); };

            World = new World(renderWindow);
            Assets = new Assets();
            var districtManager = new DistrictManager();

            Random rand = new Random();
            World.Add(new ConsoleEntity());
            World.Add(new TaskMenu());
            World.Add(new UiControllerEntity());
            World.Add(new CameraControllerEnity());
            World.Add(new ContextMenuEntity());
            World.Add(districtManager);
            Input.SetWindow(renderWindow);

            foreach (var district in Configuration.CurrentConfiguration.Districts)
                districtManager.AddDistrict(district);

            World.Lua.LinkFunction("quit", this, () => renderWindow.Close());
            World.Lua.LinkFunction("set", this, new Action<string, dynamic>((s, d) => SetConfigKey(s, d)).Method);
            World.Lua.LinkFunction("get", this, new Func<string, dynamic>((s) => GetConfigKey(s)).Method);

            MainLoop();

            Configuration.CurrentConfiguration.Districts = new List<District>(districtManager.Districts).ToArray();
            Configuration.CurrentConfiguration.WindowHeight = (int)renderWindow.Size.Y;
            Configuration.CurrentConfiguration.WindowWidth = (int)renderWindow.Size.X;
        }

        private void ApplyConfig()
        {
            renderWindow.SetFramerateLimit((uint)Configuration.CurrentConfiguration.FramerateCap);
        }

        private void SetConfigKey(string name, dynamic value)
        {
            var field = typeof(Configuration).GetField(name);

            if (field == null)
            {
                ConsoleEntity.Log($"{name} is not a valid configuration key");
                return;
            }
            try
            {
                var takesEffectAfter = field.GetCustomAttributes(typeof(Configuration.RequiresRestartAttribute), true).Any();
                field.SetValue(Configuration.CurrentConfiguration, Convert.ChangeType(value, field.FieldType));
                ConsoleEntity.Log($"Set {name} to {value}");
                if (takesEffectAfter)
                    ConsoleEntity.Log($"This setting change will take effect after restart!");
                ApplyConfig();
            }
            catch (Exception e)
            {
                ConsoleEntity.Log($"Error while setting {name} to {value}:\n{e.Message}");
            }
        }

        private dynamic GetConfigKey(string name)
        {
            var field = typeof(Configuration).GetField(name);

            if (field == null)
                ConsoleEntity.Log($"{name} is not a valid configuration key");
            try
            {
                return field.GetValue(Configuration.CurrentConfiguration);
            }
            catch (Exception e)
            {
                ConsoleEntity.Log($"Error while retrieving {name}:\n{e.Message}");
            }
            return null;
        }

        public Vector2i WorldToScreen(Vector2f value) => renderWindow.MapCoordsToPixel(value);
        public Vector2f ScreenToWorld(Vector2i value) => renderWindow.MapPixelToCoords(value);

        public void RequestFocus() => renderWindow.RequestFocus();

        private void MainLoop()
        {
            Clock clock = new Clock();

            while (renderWindow.IsOpen)
            {
                renderWindow.Clear(World.ClearColor);

                Input.Reset();
                renderWindow.DispatchEvents();
                World.Step();

                if (Input.WindowHasFocus)
                    renderWindow.Display();
                else Thread.Sleep(100);

                float dt = clock.ElapsedTime.AsSeconds();
                time += dt;
                Globals = new Globals(time, dt);

                clock.Restart();
            }

            clock.Dispose();
        }
    }
}
