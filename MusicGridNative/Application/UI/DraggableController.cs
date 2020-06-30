using SFML.System;
using SFML.Window;
using System;

namespace MusicGrid
{
    public class DraggableController
    {
        public readonly UiElement Element;

        public event EventHandler OnDragStart;
        public bool HasMoved { protected set; get; }

        public DraggableController(UiElement element)
        {
            Element = element;
        }

        public void Update()
        {
            HasMoved = false;
            HandleDragging();
        }

        protected void HandleDragging()
        {
            if (!Element.IsBeingHeld || Input.HeldButton != Mouse.Button.Left) return;
            Element.Position += (Vector2f)Input.ScreenMouseDelta;
            OnDragStart?.Invoke(this, EventArgs.Empty);
            HasMoved = true;
        }
    }
}
