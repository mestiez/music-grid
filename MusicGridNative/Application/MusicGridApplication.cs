using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.IO;

namespace MusicGridNative
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

            Random rand = new Random();
            World.Add(new ConsoleEntity());
            World.Add(new TaskMenu());

           //for (int i = 0; i < 50; i++)
           //{
           //    var district = new District(
           //            "District " + i,
           //            new Vector2f(5 * i - 128, 5 * i - 128),
           //            new Vector2f(1212, 743),
           //            new SFML.Graphics.Color((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255))
           //        );

           //    for (int o = 0; o < 0; o++)
           //        district.Entries.Add(new DistrictEntry("Entry " + rand.Next(255), ""));

           //    World.Add(new DistrictEntity(district));
           //}

            World.Add(new UiControllerEntity());
            World.Add(new CameraControllerEnity());
            Input.SetWindow(renderWindow);

            MainLoop();

            Configuration.CurrentConfiguration.WindowHeight = renderWindow.Size.Y;
            Configuration.CurrentConfiguration.WindowWidth = renderWindow.Size.X;
        }

        public Vector2i WorldToScreen(Vector2f value) => renderWindow.MapCoordsToPixel(value);
        public Vector2f ScreenToWorld(Vector2i value) => renderWindow.MapPixelToCoords(value);

        private void MainLoop()
        {
            Clock clock = new Clock();

            while (renderWindow.IsOpen)
            {
                renderWindow.Clear(World.ClearColor);

                Input.Reset();
                renderWindow.DispatchEvents();
                World.Step();

                renderWindow.Display();

                float dt = clock.ElapsedTime.AsSeconds();
                time += dt;
                Globals = new Globals(time, dt);

                clock.Restart();
            }

            clock.Dispose();
        }
    }
}
