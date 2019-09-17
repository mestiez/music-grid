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
                depthContainer = value;
                OnDepthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Depth
        {
            get => depth;

            set
            {
                depth = value;
                OnDepthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsUnderMouse { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsBeingHeld { get; private set; }
        public Color ComputedColor { get; private set; }

        public event EventHandler<MouseEventArgs> OnMouseDown;
        public event EventHandler<MouseEventArgs> OnMouseUp;

        public event EventHandler OnDepthChanged;

        public FloatRect GetLocalBounds() => new FloatRect(Position, Size);

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
                    var args = new MouseEventArgs(Mouse.Button.Left, Input.MousePosition);
                    OnMouseDown?.Invoke(this, args);
                    if (!args.IsPermeable)
                    {
                        Controller.FocusedElement = this;
                        IsBeingHeld = true;
                    }
                    else return false;
                }
            }

            if (IsBeingHeld && Input.IsButtonReleased(Mouse.Button.Left))
            {
                if (Controller.FocusedElement == this)
                    Controller.FocusedElement = null;
                IsBeingHeld = false;
                OnMouseUp?.Invoke(this, new MouseEventArgs(Mouse.Button.Left, Input.MousePosition));
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
