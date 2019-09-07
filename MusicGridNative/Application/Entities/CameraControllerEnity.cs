using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace MusicGridNative
{
    public class CameraControllerEnity : Entity
    {
        public float ZoomSensitivity { get; set; } = 1;
        public float ZoomLowerBound { get; set; } = .01f;
        public float ZoomUpperBound { get; set; } = 10f;

        public float CurrentZoom { get; private set; } = 1;
        public Vector2f CurrentPan { get; private set; }

        private View view = new View();

        public override void Update()
        {
            float zoomDelta = ZoomSensitivity * 1.2f;

            float zoomScalar = CurrentZoom;

            if (Input.MouseWheelDelta > .1f)
                CurrentZoom /= zoomDelta;
            else if (Input.MouseWheelDelta < -.1f)
                CurrentZoom *= zoomDelta;

            if (CurrentZoom > ZoomUpperBound) CurrentZoom = ZoomUpperBound;
            if (CurrentZoom < ZoomLowerBound) CurrentZoom = ZoomLowerBound;

            zoomScalar -= CurrentZoom;

            CurrentPan += ((Vector2f)Input.ScreenMousePosition - (Vector2f)World.RenderTarget.Size / 2) * zoomScalar;

            if (Input.IsButtonHeld(Mouse.Button.Middle))
                CurrentPan -= (Vector2f)Input.ScreenMouseDelta * CurrentZoom;

            view.Size = (Vector2f)World.RenderTarget.Size * CurrentZoom;
            view.Center = CurrentPan;
            World.RenderTarget.SetView(view);
        }
    }
}
