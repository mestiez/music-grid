using OpenTK;
using OpenTK.Graphics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Shared;
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

        private readonly NativeWindow nativeWindow;

        public static Globals Globals { get; private set; }
        public static Assets Assets { get; private set; }

        private readonly RenderWindow renderWindow;

        public readonly World World;

        private float time;

        public MusicGridApplication(int width, int height, int framerate, string title)
        {
            Main = this;

            nativeWindow = new NativeWindow(width, height, title, GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.GetDisplay(DisplayIndex.Default));
            if (Configuration.CurrentConfiguration.Maximised)
                nativeWindow.WindowState = WindowState.Maximized;
            renderWindow = new RenderWindow(nativeWindow.WindowInfo.Handle);
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
            World.Add(new MusicControlsEntity());
            World.Add(new KeybindExecutor());

            World.Add(districtManager);
            Input.SetWindow(renderWindow, nativeWindow);

            foreach (var district in Configuration.CurrentConfiguration.Districts)
                districtManager.AddDistrict(district);

            World.Lua.LinkFunction(Functions.Quit, this, () => renderWindow.Close());
            World.Lua.LinkFunction(Functions.Set, this, new Action<string, dynamic>((s, d) => SetConfigKey(s, d)).Method);
            World.Lua.LinkFunction(Functions.Get, this, new Func<string, dynamic>((s) => GetConfigKey(s)).Method);

            Toolkit.Init();
            GraphicsContext context = new GraphicsContext(ContextHandle.Zero, OpenTK.Platform.Utilities.CreateWindowsWindowInfo(renderWindow.SystemHandle));
            context.LoadAll();

            MainLoop();

            context.Dispose();
            Configuration.CurrentConfiguration.Maximised = nativeWindow.WindowState == WindowState.Maximized;
            Configuration.CurrentConfiguration.Districts = new List<District>(districtManager.Districts).ToArray();
            if (!Configuration.CurrentConfiguration.Maximised)
            {
                Configuration.CurrentConfiguration.WindowHeight = (int)renderWindow.Size.Y;
                Configuration.CurrentConfiguration.WindowWidth = (int)renderWindow.Size.X;
            }
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
                ConsoleEntity.Log($"{name} is not a valid configuration key", "APP");
                return;
            }
            try
            {
                var takesEffectAfter = field.GetCustomAttributes(typeof(Configuration.RequiresRestartAttribute), true).Any();
                field.SetValue(Configuration.CurrentConfiguration, Convert.ChangeType(value, field.FieldType));
                ConsoleEntity.Log($"Set {name} to {value}", "APP");
                if (takesEffectAfter)
                    ConsoleEntity.Log($"This change will take effect after restart!", "APP");
                ApplyConfig();
            }
            catch (Exception e)
            {
                ConsoleEntity.Log($"Error while setting {name} to {value}:\n{e.Message}", "APP");
            }
        }

        private dynamic GetConfigKey(string name)
        {
            var field = typeof(Configuration).GetField(name);

            if (field == null)
                ConsoleEntity.Log($"{name} is not a valid configuration key", "APP");
            try
            {
                return field.GetValue(Configuration.CurrentConfiguration);
            }
            catch (Exception e)
            {
                ConsoleEntity.Log($"Error while retrieving {name}:\n{e.Message}", "APP");
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
                nativeWindow.ProcessEvents();
                renderWindow.DispatchEvents();
                World.Step();

                if (Utilities.IsActive)
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
