using System.Numerics;

namespace Shared
{
    public class SelectionEventArgs : MouseEventArgs
    {
        public SelectionEventArgs(int button, Vector2 point) : base(button, point)
        {
        }
    }
}
