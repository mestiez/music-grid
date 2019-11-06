using System;
using System.Collections.Generic;
using System.IO;
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
        private bool requiresRecalculation = true;

        private DrawableElement shuffleButton;
        private DrawableElement repeatButton;
        private DrawableElement previousButton;
        private DrawableElement playButton;
        private DrawableElement nextButton;
        const int buttonCount = 5;

        private DrawableElement trackName;

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

            trackName = new DrawableElement(uiController, default, default);
            trackName.Element.IsScreenSpace = true;
            trackName.CenterText = true;
            trackName.HideOverflow = true;

            SetupButton(ref shuffleButton, 0);
            SetupButton(ref repeatButton, 1);
            SetupButton(ref previousButton, 2);
            SetupButton(ref playButton, 3);
            SetupButton(ref nextButton, 4);

            playButton.Element.OnMouseDown += PlayPausePressed;

            var assets = MusicGridApplication.Assets;
            shuffleButton.Texture = assets.ShuffleButton;
            repeatButton.Texture = assets.RepeatButton;
            previousButton.Texture = assets.PreviousButton;
            playButton.Texture = assets.PlayButton;
            nextButton.Texture = assets.NextButton;

            Input.WindowResized += OnWindowResized;
            MusicPlayer.OnTrackChange += OnTrackChange;
            MusicPlayer.OnPlay += OnPlay;
            MusicPlayer.OnStop += OnStop;

            World.Lua.LinkFunction("pause", this, () => { MusicPlayer.Pause(); });
            World.Lua.LinkFunction("play", this, () => { MusicPlayer.Play(); });
            World.Lua.LinkFunction("stop", this, () => { MusicPlayer.Stop(); });
            World.Lua.LinkFunction("set_volume", this, (float a) => { MusicPlayer.Volume = Math.Max(Math.Min(1, a), 0); });
            World.Lua.LinkFunction("set_track", this, (string track) => { MusicPlayer.Track = track; });
        }

        private void OnStop(object sender, EventArgs e)
        {
            playButton.Texture = MusicGridApplication.Assets.PlayButton;
        }

        private void OnPlay(object sender, EventArgs e)
        {
            playButton.Texture = MusicGridApplication.Assets.PauseButton;
        }

        private void PlayPausePressed(object sender, MouseEventArgs e)
        {
            if (MusicPlayer.State == NAudio.Wave.PlaybackState.Paused)
                MusicPlayer.Play();
            else
                MusicPlayer.Pause();
        }

        private void OnTrackChange(object sender, string e)
        {
            trackName.Text = Path.GetFileNameWithoutExtension(e);
        }

        private void SetupButton(ref DrawableElement reference, int index)
        {
            reference = new DrawableElement(uiController, buttonSize, default);

            reference.Element.IsScreenSpace = true;
            reference.DepthContainer = background.Element;

            reference.Element.Color = new Color(255, 255, 255, 200);
            reference.Element.HoverColor = new Color(255, 255, 255, 255);
            reference.Element.ActiveColor = new Color(200, 200, 200, 225);
        }

        private void PositionButton(ref DrawableElement reference, int index)
        {
            reference.Position = background.Position + new Vector2f(
                (buttonSize.X + buttonGap) * index + margin,
                background.Size.Y - buttonSize.Y - margin);
        }

        private void OnWindowResized(object sender, SFML.Window.SizeEventArgs e)
        {
            requiresRecalculation = true;
        }

        private void RecalculateLayout()
        {
            if (!requiresRecalculation) return;
            requiresRecalculation = false;

            buttonGap = Utilities.CalculateEvenSpaceGap(background.Size.X, buttonSize.X * buttonCount, buttonCount, margin);
            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);

            trackName.Position = background.Position + new Vector2f(margin, margin);
            trackName.Size = new Vector2f(background.Size.X - margin * 2, 26);

            PositionButton(ref shuffleButton, 0);
            PositionButton(ref repeatButton, 1);
            PositionButton(ref previousButton, 2);
            PositionButton(ref playButton, 3);
            PositionButton(ref nextButton, 4);
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            RecalculateLayout();

            yield return background.RenderTask;

            yield return trackName.RenderTask;

            yield return shuffleButton.RenderTask;
            yield return repeatButton.RenderTask;
            yield return previousButton.RenderTask;
            yield return playButton.RenderTask;
            yield return nextButton.RenderTask;
        }
    }
}
