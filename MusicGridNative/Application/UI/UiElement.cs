using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public class UiElement
    {
        private int depth;
        private float lastClickedTime = -1;
        private UiElement depthContainer;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public Color Color { get; set; }
        public Color HoverColor { get; set; }
        public Color ActiveColor { get; set; }
        public Color DisabledColor { get; set; }
        public Color SelectedColor { get; set; }

        public bool IsScreenSpace { get; set; }

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
        public bool IsSelected { get; set; }

        public bool Selectable { get; set; }
        public bool Disabled { get; set; }
        public float DoubleClickMaxDuration { get; set; } = 0.5f;

        public event EventHandler<MouseEventArgs> OnMouseDown;
        public event EventHandler<MouseEventArgs> OnMouseUp;
        public event EventHandler<MouseEventArgs> OnDoubleClick;
        public event EventHandler<SelectionEventArgs> OnSelect;
        public event EventHandler<SelectionEventArgs> OnDeselect;
        public event EventHandler OnDepthChanged;

        public FloatRect GetLocalBounds() => new FloatRect(Position, Size);

        public bool EvaluateInteraction(InteractionInfo info)
        {
            ComputedColor = Color;

            var mousePos = IsScreenSpace ? (Vector2f)Input.ScreenMousePosition : Input.MousePosition;

            if (info.FirstServed)
            {
                IsUnderMouse = false;
                IsActive = false;
            }
            else
            {
                IsUnderMouse = Utilities.IsInside(mousePos, Position, Size);
                IsActive = !Disabled && IsUnderMouse && info.Held.HasValue;

                if (IsUnderMouse && info.Pressed.HasValue && !Disabled)
                {
                    var args = new MouseEventArgs(info.Pressed.Value, mousePos);
                    OnMouseDown?.Invoke(this, args);
                    if (lastClickedTime > 0 && MusicGridApplication.Globals.Time - lastClickedTime < DoubleClickMaxDuration)
                    {
                        OnDoubleClick?.Invoke(this, args);
                        lastClickedTime = -1;
                    }
                    else
                        lastClickedTime = MusicGridApplication.Globals.Time;

                    if (!args.IsPermeable)
                    {
                        if (Selectable)
                        {
                            if (info.Pressed.Value == Mouse.Button.Right)
                            {
                                if (Controller.Selected.Contains(this))
                                    Controller.Select(this, true);
                                else
                                    Controller.Select(this, Controller.Multiselecting);
                            }
                            else
                                Controller.HandleSelection(this);

                            if (IsSelected)
                                OnSelect?.Invoke(this, new SelectionEventArgs(info.Pressed.Value, mousePos));
                            else
                                OnDeselect?.Invoke(this, new SelectionEventArgs(info.Pressed.Value, mousePos));
                        }
                        else Controller.ClearSelection();
                        Controller.FocusedElement = this;
                        IsBeingHeld = true;
                    }
                    else return false;
                }
            }

            if (!Disabled && IsBeingHeld && info.Released.HasValue)
            {
                if (Controller.FocusedElement == this)
                    Controller.FocusedElement = null;
                IsBeingHeld = false;
                OnMouseUp?.Invoke(this, new MouseEventArgs(info.Released.Value, mousePos));
            }
            ComputeColors();
            return IsUnderMouse || IsBeingHeld;
        }

        public void ComputeColors()
        {
            if (Disabled)
                ComputedColor = DisabledColor;
            else if (IsSelected)
                ComputedColor = SelectedColor;
            else if (IsActive || IsBeingHeld)
                ComputedColor = ActiveColor;
            else if (IsUnderMouse)
                ComputedColor = HoverColor;
            else
                ComputedColor = Color;
        }
    }
}
