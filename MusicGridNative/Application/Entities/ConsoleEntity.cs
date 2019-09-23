﻿using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGridNative
{
    public class ConsoleEntity : Entity
    {
        public static ConsoleEntity Main { get; private set; }

        private Text display;
        private static readonly Queue<string> history = new Queue<string>();
        private int framesCounted = 0;
        private int framesLastSecond = 0;
        private float t = 0;

        public ConsoleEntity(bool showFramerate = false, uint maximumMessages = 32)
        {
            ShowFramerate = showFramerate;
            MaximumMessages = maximumMessages;
        }

        public bool ConsoleIsOpen { get; set; }
        public bool ShowFramerate { get; set; }
        public uint MaximumMessages { get; set; }

        public override void Created()
        {
            Main = this;
            display = new Text("", MusicGridApplication.Assets.DefaultFont, 16);
            display.Position = new Vector2f(5, 5);
        }

        public override void Update()
        {
            framesCounted++;
            t += MusicGridApplication.Globals.DeltaTime;
            if (t >= 1)
            {
                framesLastSecond = framesCounted;
                framesCounted = 0;
                t = 0;
            }
            if (Input.IsKeyReleased(SFML.Window.Keyboard.Key.F12))
                ConsoleIsOpen = !ConsoleIsOpen;
        }

        public static void Show(object message)
        {
            if (history.Count >= (Main?.MaximumMessages ?? 32)) history.Dequeue();
            history.Enqueue($"[{DateTime.Now.ToLongTimeString()}] " + message.ToString());
        }

        public override void PreRender()
        {
            string fps = "[" + framesLastSecond + " fps]";
            string separator = "";
            for (int i = 0; i < Math.Floor(World.RenderTarget.Size.X / (float)display.CharacterSize); i++)
                separator += "一";

            if (ConsoleIsOpen)
                display.DisplayedString = $"MUSIC GRID v{ApplicationVersion.Version} ({ApplicationVersion.Build}) by mestiez {(ShowFramerate ? fps : "")}\n{separator}\n{string.Join("\n", history)}";
            else if (ShowFramerate)
                display.DisplayedString = fps;
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            yield return new ShapeRenderTask(display, int.MinValue);
        }
    }
}
