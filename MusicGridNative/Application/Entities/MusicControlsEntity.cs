using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace MusicGrid
{
    public class MusicControlsEntity : Entity
    {
        public MusicPlayer MusicPlayer { get; } = new MusicPlayer();

        private UiControllerEntity uiController;
        private DrawableElement background;

        private readonly Vector2f buttonSize = new Vector2f(32, 32);
        private readonly float margin = 10;
        private float buttonGap;

        private DrawableElement shuffleButton;
        private DrawableElement repeatButton;
        private DrawableElement previousButton;
        private DrawableElement playButton;
        private DrawableElement nextButton;

        public MusicControlsEntity()
        {
            MusicPlayer.OnFailure += (o, e) =>
            {
                World.Add(new DialogboxEntity(e.Message, new SFML.System.Vector2f(e.Message.Length * DialogboxEntity.CharacterSize / 2 + 50, 150)));
            };
        }

        public override void Created()
        {
            uiController = World.GetEntityByType<UiControllerEntity>();

            background = new DrawableElement(uiController, new Vector2f(250, 88), new Vector2f(50, 50));
            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);
            background.Element.IsScreenSpace = true;
            background.Element.ActiveColor = background.Element.Color;
            background.Element.HoverColor = background.Element.Color;

            buttonGap = Utilities.CalculateEvenSpaceGap(background.Size.X, buttonSize.X * 5, 5, margin);
            SetupButton(ref shuffleButton, 0);
            SetupButton(ref repeatButton, 1);
            SetupButton(ref previousButton, 2);
            SetupButton(ref playButton, 3);
            SetupButton(ref nextButton, 4);

            var assets = MusicGridApplication.Assets;
            shuffleButton.Texture = assets.ShuffleButton;
            repeatButton.Texture = assets.RepeatButton;
            previousButton.Texture = assets.PreviousButton;
            playButton.Texture = assets.PlayButton;
            nextButton.Texture = assets.NextButton;

            Input.WindowResized += OnWindowResized;

            World.Lua.LinkFunction("pause", this, () => { MusicPlayer.Pause(); });
            World.Lua.LinkFunction("play", this, () => { MusicPlayer.Play(); });
            World.Lua.LinkFunction("stop", this, () => { MusicPlayer.Stop(); });
            World.Lua.LinkFunction("set_track", this, (string track) => { MusicPlayer.SetTrack(track); });
        }

        private void SetupButton(ref DrawableElement reference, int index)
        {
            reference = new DrawableElement(uiController, buttonSize, background.Position + new Vector2f(
                (buttonSize.X + buttonGap) * index + margin,
                background.Size.Y - buttonSize.Y - margin));

            reference.Element.IsScreenSpace = true;
            reference.DepthContainer = background.Element;

            reference.Element.Color = new Color(255, 255, 255, 200);
            reference.Element.HoverColor = new Color(255, 255, 255, 255);
            reference.Element.ActiveColor = new Color(200, 200, 200, 225);
        }

        private void OnWindowResized(object sender, SFML.Window.SizeEventArgs e)
        {
            background.Position = new Vector2f(0, e.Height - background.Size.Y);
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            yield return background.RenderTask;

            yield return shuffleButton.RenderTask;
            yield return repeatButton.RenderTask;
            yield return previousButton.RenderTask;
            yield return playButton.RenderTask;
            yield return nextButton.RenderTask;
        }
    }
}
