using SFML.Graphics;

namespace MusicGridNative
{
    public class DistrictEntity : Entity
    {
        public readonly District District;

        private RectangleShape background = new RectangleShape();
        private Text title;

        public override void Created()
        {
            base.Created();

            background = new RectangleShape();
            background.Position = new SFML.System.Vector2f(0, 0);
            background.Size = new SFML.System.Vector2f(256, 256);
            background.FillColor = new Color(125, 0, 15);

            title = new Text("Empty", MusicGridApplication.Main.Assets.DefaultFont);
            title.CharacterSize = 16;
            title.FillColor = new Color(255, 255, 255, 125);

            title.
        }

        public override void Render()
        {
            base.Render();

            World.RenderTarget.Draw(background);
            World.RenderTarget.Draw(text);
        }
    }
}
