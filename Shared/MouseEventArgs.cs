using System;
using System.Numerics;

namespace Shared
{
    public class MouseEventArgs : EventArgs
    {
        public int Button;
        public Vector2 Point;

        public MouseEventArgs(int button, Vector2 point)
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
