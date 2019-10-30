using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace MusicGrid
{
    public class CameraControllerEnity : Entity
    {
        private readonly View view = new View();

        public override void Update()
        {
            if (!Input.WindowHasFocus) return;

            var config = Configuration.CurrentConfiguration;

            if (Input.IsButtonHeld(Mouse.Button.Middle))
                config.Pan -= (Vector2f)Input.ScreenMouseDelta * config.Zoom;
            else
            {
                float zoomDelta = config.ZoomSensitivity * 1.2f;

                float zoomScalar = config.Zoom;

                if (Input.MouseWheelDelta > .1f)
                    config.Zoom /= zoomDelta;
                else if (Input.MouseWheelDelta < -.1f)
                    config.Zoom *= zoomDelta;

                if (config.Zoom > config.ZoomUpperBound) config.Zoom = config.ZoomUpperBound;
                if (config.Zoom < config.ZoomLowerBound) config.Zoom = config.ZoomLowerBound;

                zoomScalar -= config.Zoom;

                config.Pan += ((Vector2f)Input.ScreenMousePosition - (Vector2f)World.RenderTarget.Size / 2) * zoomScalar;
            }

            view.Size = (Vector2f)World.RenderTarget.Size * config.Zoom;
            view.Center = config.Pan;
            World.RenderTarget.SetView(view);
        }
    }
}
