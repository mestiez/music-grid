using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicGrid
{
    public class CameraControllerEnity : Entity
    {
        private readonly View view = new View();

        public override void Created()
        {
            World.Lua.LinkFunction(Functions.FitView, this, () => { FitToView(World.GetEntityByType<DistrictManager>().Districts); });
            World.Lua.LinkFunction(Functions.FitViewToSelection, this, () =>
            {
                var selectedDistricts = World.GetEntitiesByType<DistrictEntity>().Where(d => d.IsSelected).Select(d => d.District).ToList();
                FitToView(selectedDistricts);
            });
        }

        public override void Update()
        {
            if (!Input.WindowHasFocus) return;
            if (ConsoleEntity.Main.ConsoleIsOpen) return;
            var config = Configuration.CurrentConfiguration;
            if (Input.IsButtonHeld(Mouse.Button.Middle))
                config.Pan -= (Vector2f)Input.ScreenMouseDelta * config.Zoom;
            else
            {
                float zoomDelta = config.ZoomSensitivity * 1.2f;
                float zoomScalar = config.Zoom;

                if (Input.MouseWheelDelta > .5f)
                    config.Zoom /= zoomDelta;
                else if (Input.MouseWheelDelta < -.5f)
                    config.Zoom *= zoomDelta;

                ClampZoom();
                zoomScalar -= config.Zoom;
                config.Pan += ((Vector2f)Input.ScreenMousePosition - (Vector2f)World.RenderTarget.Size / 2) * zoomScalar;
            }

            view.Size = (Vector2f)World.RenderTarget.Size * config.Zoom;
            view.Center = config.Pan;
            World.RenderTarget.SetView(view);
        }

        private static void ClampZoom()
        {
            var config = Configuration.CurrentConfiguration;
            Console.WriteLine(Configuration.CurrentConfiguration.Zoom);
            if (config.Zoom > config.ZoomUpperBound) config.Zoom = config.ZoomUpperBound;
            if (config.Zoom < config.ZoomLowerBound) config.Zoom = config.ZoomLowerBound;
        }

        public void FitToView(IEnumerable<District> districts, float padding = 20)
        {
            if (districts.Count() < 1) return;
            var taskMenu = World.GetEntityByType<TaskMenu>();

            float minX = districts.Min(d => d.Position.X) - padding;
            float minY = districts.Min(d => d.Position.Y) - padding - taskMenu.Height;
            float maxX = districts.Max(d => d.Position.X + d.Size.X) + padding;
            float maxY = districts.Max(d => d.Position.Y + d.Size.Y) + padding;
            var center = new Vector2f((minX + maxX) / 2, (minY + maxY) / 2);
            float width = maxX - minX;
            float height = maxY - minY;

            Configuration.CurrentConfiguration.Pan = center;
            Configuration.CurrentConfiguration.Zoom = 1f / Math.Min(World.RenderTarget.Size.X / width, (World.RenderTarget.Size.Y - taskMenu.Height) / height);
            ClampZoom();
        }
    }
}
