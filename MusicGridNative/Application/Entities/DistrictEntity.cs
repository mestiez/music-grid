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
        private Vertex[] resizeHandle;
        private Text title;

        private readonly UiElement element = new UiElement();

        private const float HandleSize = 16;
        private const float HandlePadding = 0;

        private static readonly Vector2f[] handleShape = new Vector2f[3]
            {
                new Vector2f(-HandlePadding, -HandlePadding),
                new Vector2f(-HandleSize -HandlePadding, -HandlePadding),
                new Vector2f(-HandlePadding, -HandleSize - HandlePadding)
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

            resizeHandle = new Vertex[3]
            {
                new Vertex(handleShape[0], title.FillColor),
                new Vertex(handleShape[1], title.FillColor),
                new Vertex(handleShape[2], title.FillColor)
            };

            element.OnClick += (sender, e) =>
            {
                Console.Write("e");
            };
        }

        public override void Update()
        {
            element.InteractionStep();
            SyncElement();
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

            Vector2f offset = District.Position + District.Size;

            resizeHandle[0].Position = offset + handleShape[0];
            resizeHandle[1].Position = offset + handleShape[1];
            resizeHandle[2].Position = offset + handleShape[2];
        }

        public override void Render()
        {
            var target = World.RenderTarget;

            target.Draw(background);
            target.Draw(title);
            target.Draw(resizeHandle, PrimitiveType.Triangles);
        }
    }
}
