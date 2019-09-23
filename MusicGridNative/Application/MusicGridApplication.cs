using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Drawing;
using System.IO;

namespace MusicGridNative
{
    public class MusicGridApplication
    {
        public static MusicGridApplication Main { get; private set; }

        public static Globals Globals { get; private set; }
        public static Assets Assets { get; private set; }

        private readonly RenderWindow window;

        public readonly World World;

        private float time;

        public MusicGridApplication(uint width, uint height, uint framerate, string title)
        {
            Main = this;

            window = new RenderWindow(new VideoMode(width, height), title, Styles.Resize | Styles.Close);
            window.SetFramerateLimit(framerate);

            using (var ms = new MemoryStream())
            {
                Properties.Resources.favicon_32x32.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                SFML.Graphics.Image image = new SFML.Graphics.Image(ms);
                window.SetIcon(32, 32, image.Pixels);
                image.Dispose();
            }


            window.Closed += (object sender, EventArgs args) => { window.Close(); };

            World = new World(window);
            Assets = new Assets();

            Random rand = new Random();
            World.Add(new ConsoleEntity(true));

            for (int i = 0; i < 54; i++)
            {
                var district = new District(
                        "District " + i,
                        new Vector2f(5 * i - 128, 5 * i - 128),
                        new Vector2f(1212, 743),
                        new SFML.Graphics.Color((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255))
                    );

                for (int o = 0; o < 25; o++)
                    district.Entries.Add(new DistrictEntry("Entry " + rand.Next(255), ""));

                World.Add(new DistrictEntity(district));
            }

            World.Add(new UiControllerEntity());
            World.Add(new CameraControllerEnity());
            Input.SetWindow(window);

            MainLoop();

            Configuration.CurrentConfiguration.WindowHeight = window.Size.Y;
            Configuration.CurrentConfiguration.WindowWidth = window.Size.X;
        }

        public Vector2i WorldToScreen(Vector2f value) => window.MapCoordsToPixel(value);
        public Vector2f ScreenToWorld(Vector2i value) => window.MapPixelToCoords(value);

        private void MainLoop()
        {
            Clock clock = new Clock();

            while (window.IsOpen)
            {
                window.Clear(World.ClearColor);

                Input.Reset();
                window.DispatchEvents();
                World.Step();

                window.Display();

                float dt = clock.ElapsedTime.AsSeconds();
                time += dt;
                Globals = new Globals(time, dt);

                clock.Restart();
            }

            clock.Dispose();
        }
    }
}
