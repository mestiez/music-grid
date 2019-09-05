using SFML.Graphics;
using System;

namespace MusicGridNative
{
    public class DistrictEntity : Entity
    {
        public readonly District District = new District();

        private RectangleShape background = new RectangleShape();
        private Text title;

        public override void Created()
        {

            background = new RectangleShape();
            background.Position = new SFML.System.Vector2f(0, 0);
            background.Size = new SFML.System.Vector2f(256, 256);
            background.FillColor = new Color(125, 0, 15);

            title = new Text("Empty", MusicGridApplication.Main.Assets.DefaultFont);
            title.FillColor = new Color(255, 255, 255, 125);

            title.Position = new SFML.System.Vector2f(0, 0);
        }

        public override void PreRender()
        {
            District.Size.X += 0.1f;
            District.Size.Y += 0.1f;

            background.Position = District.Position;
            background.Size = District.Size;
            background.FillColor = District.Color;

            title.Position = background.Position;
            title.CharacterSize = (uint)Math.Sqrt(District.Size.X * District.Size.Y * 0.01f);
        }

        public override void Render()
        {
            World.RenderTarget.Draw(background);
            World.RenderTarget.Draw(title);
        }
    }
}
