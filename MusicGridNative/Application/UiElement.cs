using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGridNative
{
    public class UiElement
    {
        private int depth;
        private UiElement depthContainer;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public Color Color { get; set; }
        public Color HoverColor { get; set; }
        public Color ActiveColor { get; set; }

        public UiControllerEntity Controller { get; set; }
        public UiElement DepthContainer
        {
            get => depthContainer;

            set
            {
                OnDepthChanged?.Invoke(this, EventArgs.Empty);
                depthContainer = value;
            }
        }

        public int Depth
        {
            get => depth;

            set
            {
                OnDepthChanged?.Invoke(this, EventArgs.Empty);
                depth = value;
            }
        }

        public bool IsUnderMouse { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsBeingHeld { get; private set; }
        public Color ComputedColor { get; private set; }

        public event EventHandler OnMouseDown;
        public event EventHandler OnMouseUp;

        public event EventHandler OnDepthChanged;

        public bool EvaluateInteraction(bool firstHasBeenServed)
        {
            if (firstHasBeenServed)
            {
                IsUnderMouse = false;
                IsActive = false;
            }
            else
            {
                IsUnderMouse = Utilities.IsInside(Input.MousePosition, Position, Size);
                IsActive = IsUnderMouse && Input.IsButtonHeld(Mouse.Button.Left);

                if (IsUnderMouse && Input.IsButtonPressed(Mouse.Button.Left))
                {
                    Controller.FocusedElement = this;
                    IsBeingHeld = true;
                    OnMouseDown?.Invoke(this, EventArgs.Empty);
                }
            }

            if (IsBeingHeld && Input.IsButtonReleased(Mouse.Button.Left))
            {
                if (Controller.FocusedElement == this)
                    Controller.FocusedElement = null;
                IsBeingHeld = false;
                OnMouseUp?.Invoke(this, EventArgs.Empty);
            }

            if (IsActive || IsBeingHeld)
                ComputedColor = ActiveColor;
            else if (IsUnderMouse)
                ComputedColor = HoverColor;
            else
                ComputedColor = Color;

            return IsUnderMouse || IsBeingHeld;
        }
    }
}
