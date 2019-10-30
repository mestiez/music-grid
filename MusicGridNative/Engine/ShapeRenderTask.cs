using SFML.Graphics;

namespace MusicGrid
{
    public class ShapeRenderTask : IRenderTask
    {
        public ShapeRenderTask(Drawable drawable, int depth)
        {
            Drawable = drawable;
            Depth = depth;
        }

        public Drawable Drawable { get; set; }

        public int Depth { get; set; }

        public void Render(RenderTarget target)
        {
            target.Draw(Drawable);
        }
    }
}
