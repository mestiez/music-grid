using SFML.System;
using SFML.Window;
using System;

namespace MusicGridNative
{
    public class MouseEventArgs : EventArgs
    {
        public Mouse.Button Button;
        public Vector2f Point;

        public MouseEventArgs(Mouse.Button button, Vector2f point)
        {
            Button = button;
            Point = point;
        }

        public bool IsPermeable { get; private set; }

        public void PropagateEvent()
        {
            IsPermeable = true;
        }
    }
}
