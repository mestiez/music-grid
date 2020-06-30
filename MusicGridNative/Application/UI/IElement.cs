using SFML.Graphics;
using SFML.System;
using Shared;
using System;

namespace MusicGrid
{
    public interface IElement
    {
        SFML.Graphics.Color ActiveColor { get; set; }
        SFML.Graphics.Color Color { get; set; }
        SFML.Graphics.Color ComputedColor { get; }
        UiControllerEntity Controller { get; set; }
        int Depth { get; set; }
        UiElement DepthContainer { get; set; }
        bool Disabled { get; set; }
        SFML.Graphics.Color DisabledColor { get; set; }
        float DoubleClickMaxDuration { get; set; }
        SFML.Graphics.Color HoverColor { get; set; }
        bool IsActive { get; }
        bool IsBeingHeld { get; }
        bool IsScreenSpace { get; set; }
        bool IsSelected { get; set; }
        bool IsUnderMouse { get; }
        bool MouseOverlap { get; }
        Vector2f Position { get; set; }
        bool Selectable { get; set; }
        SFML.Graphics.Color SelectedColor { get; set; }
        Vector2f Size { get; set; }

        event EventHandler OnDepthChanged;
        event EventHandler<SelectionEventArgs> OnDeselect;
        event EventHandler<MouseEventArgs> OnDoubleClick;
        event EventHandler<MouseEventArgs> OnMouseDown;
        event EventHandler<MouseEventArgs> OnMouseUp;
        event EventHandler<SelectionEventArgs> OnSelect;

        FloatRect GetLocalBounds();
    }
}