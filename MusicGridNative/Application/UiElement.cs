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
        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public Color Color { get; set; }
        public Color HoverColor { get; set; }
        public Color ActiveColor { get; set; }

        public bool IsUnderMouse { get; private set; }
        public bool IsActive { get; private set; }
        public Color ComputedColor { get; private set; }

        public event EventHandler OnClick;

        public void InteractionStep()
        {
            IsUnderMouse = Utilities.IsInside(Input.MousePosition, Position, Size);
            IsActive = IsUnderMouse && Input.IsButtonHeld(Mouse.Button.Left);

            if (IsUnderMouse && Input.IsButtonReleased(Mouse.Button.Left))
                OnClick?.Invoke(this, EventArgs.Empty);

            if (IsActive)
                ComputedColor = ActiveColor;
            else if (IsUnderMouse)
                ComputedColor = HoverColor;
            else
                ComputedColor = Color;
        }
    }
}
