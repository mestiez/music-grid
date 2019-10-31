using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.IO;
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

        public MusicGridApplication(uint width, uint height, uint framerate, string title)
        {
            Main = this;

            renderWindow = new RenderWindow(new VideoMode(width, height), title, Styles.Resize | Styles.Close);
            renderWindow.SetFramerateLimit(framerate);

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
            World.Lua.LinkFunction("set_framerate_cap", this, (int c) =>
            {
                renderWindow.SetFramerateLimit((uint)c);
                Configuration.CurrentConfiguration.FramerateCap = (uint)c;
                ConsoleEntity.Show("Set framerate cap to " + c);
            });

            MainLoop();

            Configuration.CurrentConfiguration.Districts = new List<District>(districtManager.Districts).ToArray();
            Configuration.CurrentConfiguration.WindowHeight = renderWindow.Size.Y;
            Configuration.CurrentConfiguration.WindowWidth = renderWindow.Size.X;
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
