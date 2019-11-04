using SFML.Graphics;
using System;

namespace MusicGrid
{
    public struct ActionRenderTask : IRenderTask
    {
        public ActionRenderTask(Action<RenderTarget> action, int depth)
        {
            Action = action;
            Depth = depth;
        }

        public Action<RenderTarget> Action { get; set; }

        public int Depth { get; set; }

        public void Render(RenderTarget target)
        {
            Action(target);
        }
    }
}
