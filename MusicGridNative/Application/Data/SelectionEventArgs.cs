using SFML.System;
using SFML.Window;

namespace MusicGrid
{
    public class SelectionEventArgs : MouseEventArgs
    {
        public SelectionEventArgs(Mouse.Button button, Vector2f point) : base(button, point)
        {
        }
    }
}
