using SFML.Graphics;

namespace MusicGrid
{
    public struct PrimitiveRenderTask : IRenderTask
    {
        public PrimitiveRenderTask(Vertex[] vertices, PrimitiveType primitiveType, int depth)
        {
            Vertices = vertices;
            PrimitiveType = primitiveType;
            Depth = depth;
        }

        public Vertex[] Vertices { get; set; }
        public PrimitiveType PrimitiveType { get; set; }

        public int Depth { get; set; }

        public void Render(RenderTarget target)
        {
            target.Draw(Vertices, PrimitiveType);
        }
    }
}
