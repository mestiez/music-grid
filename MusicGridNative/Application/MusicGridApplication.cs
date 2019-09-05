﻿using SFML.Graphics;
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

        private readonly RenderWindow window;

        public readonly World World;
        public readonly Assets Assets;

        private float time;

        public MusicGridApplication(uint width, uint height, uint framerate, string title)
        {
            Main = this;

            window = new RenderWindow(new VideoMode(width, height), title, Styles.Resize | Styles.Close);
            window.SetFramerateLimit(framerate);

            using (var ms = new MemoryStream())
            {
                Properties.Resources.favicon_32x32.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                SFML.Graphics.Image imag = new SFML.Graphics.Image(ms);
                window.SetIcon(32, 32, imag.Pixels);
                imag.Dispose();
            }


            window.Closed += (object sender, EventArgs args) => { window.Close(); };
            window.Resized += (object sender, SizeEventArgs args) =>
            {
                window.Size = new SFML.System.Vector2u(Math.Max(args.Width, 430), Math.Max(args.Height, 430));

                var view = window.GetView();
                view.Size = new SFML.System.Vector2f(window.Size.X, window.Size.Y);
                window.SetView(view);
            };

            World = new World(window);
            Assets = new Assets();

            World.Add(new DistrictEntity());

            Input.SetWindow(window);

            MainLoop();
        }

        private void MainLoop()
        {
            Clock clock = new Clock();

            while (window.IsOpen)
            {
                window.Clear();
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
