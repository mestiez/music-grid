using SFML.Graphics;
using SFML.Window;
using System;
using System.IO;
using System.Reflection;

namespace MusicGridNative
{

    public class MusicGridApplication
    {
        public static MusicGridApplication Main { get; private set; }

        private readonly RenderWindow window;

        public readonly World World;
        public readonly Assets Assets;

        public MusicGridApplication(uint width, uint height, uint framerate, string title)
        {
            Main = this;

            window = new RenderWindow(new VideoMode(width, height), title, Styles.Resize | Styles.Close);
            window.SetFramerateLimit(framerate);

            window.Closed += (object sender, EventArgs args) => { window.Close(); };

            World = new World(window);
            Assets = new Assets();

            World.Add(new DistrictEntity());

            MainLoop();
        }

        private void MainLoop()
        {
            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear();
                World.Step();
                window.Display();
            }
        }
    }
}
