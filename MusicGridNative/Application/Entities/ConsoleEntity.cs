using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public class ConsoleEntity : Entity
    {
        public static ConsoleEntity Main { get; private set; }

        private Text display;
        private RectangleShape background;
        private static readonly Queue<string> history = new Queue<string>();

        private readonly List<string> inputHistory = new List<string>();
        private int inputHistoryIndex = 0;
        private int framesCounted = 0;
        private int framesLastSecond = 0;
        private float t = 0;

        private string input = "";

        private ShapeRenderTask backgroundTask;
        private ShapeRenderTask displayTask;

        public ConsoleEntity(bool showFramerate = false, uint maximumMessages = 64)
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

            background = new RectangleShape(new Vector2f(4000, 4000));
            background.FillColor = new Color(0, 0, 0, 200);

            backgroundTask = new ShapeRenderTask(background, int.MinValue);
            displayTask = new ShapeRenderTask(display, int.MinValue);
            World.Lua.LinkFunction<object>("print", this, AddToHistory);
            World.Lua.LinkFunction("exit", this, () => { ConsoleIsOpen = false; });
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

            if (Input.IsKeyReleased(Keyboard.Key.F12))
                ConsoleIsOpen = !ConsoleIsOpen;

            if (ConsoleIsOpen)
                HandleUserInput();
        }

        private void HandleUserInput()
        {
            HandleInputHistoryTraversing();
            if (Input.IsKeyReleased(Keyboard.Key.Enter))
                ConsumeInput();
            else
                foreach (var c in Input.TextEntered)
                    switch (c)
                    {
                        case '\b':
                            input = input.Substring(0, Math.Max(0, input.Length - 1));
                            break;
                        default:
                            input += c.ToString();
                            break;
                    }
        }

        private void HandleInputHistoryTraversing()
        {
            if (inputHistory.Any())
            {
                if (Input.IsKeyReleased(Keyboard.Key.Up))
                {
                    inputHistoryIndex--;
                    if (inputHistoryIndex >= 0)
                        input = inputHistory[inputHistoryIndex];
                    else inputHistoryIndex = -1;
                }
                else if (Input.IsKeyReleased(Keyboard.Key.Down))
                {
                    inputHistoryIndex++;
                    if (inputHistoryIndex >= inputHistory.Count)
                    {
                        inputHistoryIndex = inputHistory.Count;
                        input = "";
                    }
                    else
                        input = inputHistory[inputHistoryIndex];
                }
            }
        }

        private void ConsumeInput()
        {
            inputHistory.Add(input);
            inputHistoryIndex = inputHistory.Count;
            var results = World.Lua.Execute(input);
            if (results != null)
                foreach (var result in results.Where(e => e != null))
                    Log(result);

            input = "";
        }

        public static void Log(object message)
        {
            if (message == null) message = "null";

            if (message is ICollection messageList)
            {
                foreach (var item in messageList)
                    Log(item);
                return;
            }

            if (history.Count >= (Main?.MaximumMessages ?? 32)) history.Dequeue();
            history.Enqueue($"[{DateTime.Now.ToLongTimeString()}] " + message.ToString());
        }

        private void AddToHistory(object message)
        {
            Log(message);
        }

        public override void PreRender()
        {
            if (!ConsoleIsOpen && !ShowFramerate) return;
            string fps = "[" + framesLastSecond + " fps]";
            string separator = "";
            for (int i = 0; i < Math.Floor(World.RenderTarget.Size.X / (float)display.CharacterSize); i++)
                separator += "一";

            string userInput = input + (MusicGridApplication.Globals.Time % 2f > 1f ? "_" : " ");

            if (ConsoleIsOpen)
                display.DisplayedString = $"MUSIC GRID v{ApplicationVersion.Version} ({ApplicationVersion.Build}) by mestiez {fps}\n{separator}\n{userInput}\n\n{string.Join("\n", history.Reverse())}";
            else if (ShowFramerate)
                display.DisplayedString = fps;
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            if (!ConsoleIsOpen && !ShowFramerate) yield break;
            background.Size = (Vector2f)World.RenderTarget.Size;
            yield return backgroundTask;
            yield return displayTask;
        }
    }
}
