using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace MusicGridNative
{
    public class DistrictEntity : Entity
    {
        public readonly District District = new District();

        private RectangleShape background;
        private Vertex[] resizeHandleVertices;
        private Text title;

        private readonly UiElement element = new UiElement();
        private readonly UiElement resizeHandle = new UiElement();

        private const float HandleSize = 16;

        private static readonly Vector2f[] handleShape = new Vector2f[3]
            {
                new Vector2f(0, 0),
                new Vector2f(-HandleSize, 0),
                new Vector2f(0, -HandleSize)
            };

        public override void Created()
        {
            background = new RectangleShape
            {
                Position = new Vector2f(0, 0),
                Size = new Vector2f(256, 256),
                FillColor = new Color(125, 0, 15)
            };

            title = new Text("Empty", MusicGridApplication.Main.Assets.DefaultFont)
            {
                FillColor = new Color(255, 255, 255, 125),
                Position = new Vector2f(0, 0)
            };

            resizeHandleVertices = new Vertex[3]
            {
                new Vertex(handleShape[0], title.FillColor),
                new Vertex(handleShape[1], title.FillColor),
                new Vertex(handleShape[2], title.FillColor)
            };
        }

        public override void Update()
        {
            element.InteractionStep();
            resizeHandle.InteractionStep();

            SyncElement();
            SyncResizeHandle();
        }

        private void SyncElement()
        {
            element.Color = District.Color;
            element.ActiveColor = Utilities.Lerp(District.Color, Color.Black, 0.2f);
            element.HoverColor = Utilities.Lerp(District.Color, Color.White, 0.2f);
            element.Position = District.Position;
            element.Size = District.Size;

            background.Position = element.Position;
            background.Size = element.Size;
            background.FillColor = element.ComputedColor;

            title.Position = background.Position;
            title.CharacterSize = (uint)Math.Sqrt(District.Size.X * District.Size.Y * 0.01f);
        }

        private void SyncResizeHandle()
        {
            Vector2f offset = District.Position + District.Size;

            resizeHandleVertices[0].Position = offset + handleShape[0];
            resizeHandleVertices[1].Position = offset + handleShape[1];
            resizeHandleVertices[2].Position = offset + handleShape[2];

            resizeHandle.Color = title.FillColor;
            resizeHandle.ActiveColor = Utilities.Lerp(title.FillColor, Color.Black, 0.2f);
            resizeHandle.HoverColor = Utilities.Lerp(title.FillColor, Color.White, 0.2f);
            resizeHandle.Position = offset - new Vector2f(HandleSize, HandleSize);
            resizeHandle.Size = new Vector2f(HandleSize, HandleSize);

            resizeHandleVertices[0].Color = resizeHandle.ComputedColor;
            resizeHandleVertices[1].Color = resizeHandle.ComputedColor;
            resizeHandleVertices[2].Color = resizeHandle.ComputedColor;
        }

        public override void Render()
        {
            var target = World.RenderTarget;

            target.Draw(background);
            target.Draw(title);
            target.Draw(resizeHandleVertices, PrimitiveType.Triangles);
        }
    }
}
