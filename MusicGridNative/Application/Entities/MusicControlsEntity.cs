using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
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
        private readonly float margin = 5;
        private float buttonGap;
        private bool requiresRecalculation = true;
        private PlaybackState stateBeforeTrackerPause;

        private DrawableElement shuffleButton;
        private DrawableElement repeatButton;
        private DrawableElement previousButton;
        private DrawableElement playButton;
        private DrawableElement nextButton;
        const int buttonCount = 5;

        private DrawableElement trackName;
        private DrawableElement trackInfo;

        private RectangleShape tracker;
        private RectangleShape trackerBackground;
        private IRenderTask trackerTask;
        private IRenderTask trackerBackgroundTask;

        public MusicControlsEntity()
        {
            MusicPlayer.OnFailure += (o, m) =>
            {
                World.Add(new DialogboxEntity(m, new Vector2f(m.Length * DialogboxEntity.CharacterSize / 2 + 50, 150)), int.MaxValue);
            };
        }

        public override void Created()
        {
            uiController = World.GetEntityByType<UiControllerEntity>();

            background = new DrawableElement(uiController, new Vector2f(250, 100), new Vector2f(50, 50));
            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);
            background.Element.IsScreenSpace = true;
            background.Element.ActiveColor = background.Element.Color;
            background.Element.HoverColor = background.Element.Color;

            trackName = new DrawableElement(uiController);
            trackName.Element.IsScreenSpace = true;
            trackName.CenterText = true;
            trackName.HideOverflow = true;
            trackName.DepthContainer = background.Element;
            trackName.Element.Color = new Color(0, 0, 0, 25);
            trackName.Element.ActiveColor = trackName.Element.Color;
            trackName.Element.HoverColor = trackName.Element.Color;

            trackInfo = new DrawableElement(uiController);
            trackInfo.Element.IsScreenSpace = true;
            trackInfo.HideOverflow = true;
            trackInfo.CenterText = true;
            trackInfo.DrawBackground = false;
            trackInfo.CharacterSize = 12;
            trackInfo.DepthContainer = background.Element;
            trackInfo.Depth = -1;
            trackInfo.Element.OnMouseDown += (o, e) => { stateBeforeTrackerPause = MusicPlayer.State; MusicPlayer.Pause(); };
            trackInfo.Element.OnMouseUp += (o, e) => { if (stateBeforeTrackerPause == PlaybackState.Playing) MusicPlayer.Play(); };

            tracker = new RectangleShape();
            trackerBackground = new RectangleShape();
            trackerTask = new ShapeRenderTask(tracker, background.Depth);
            trackerBackgroundTask = new ShapeRenderTask(trackerBackground, background.Depth);
            SetColor(new Color(200, 200, 200));

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
            World.Lua.LinkFunction("set_volume", this, (float a) => { MusicPlayer.Volume = Math.Max(Math.Min(1, a), 0); ConsoleEntity.Log($"Volume set to {Math.Round(MusicPlayer.Volume * 100)}%", "MPE"); });
            World.Lua.LinkFunction("set_track", this, (string track) => { MusicPlayer.Track = track; });

            Input.WindowClosed += (o, e) =>
            {
                MusicPlayer.Dispose();
            };
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

            //background.Size = new Vector2f(Input.WindowSize.X * 0.25f, Input.WindowSize.Y * 0.2f);
            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);
            buttonGap = Utilities.CalculateEvenSpaceGap(background.Size.X, buttonSize.X * buttonCount, buttonCount, margin);

            trackName.Position = background.Position + new Vector2f(margin, margin);
            trackName.Size = new Vector2f(background.Size.X - margin * 2, 26);

            trackInfo.Position = background.Position + new Vector2f(margin, margin + margin + trackName.Size.Y);
            trackInfo.Size = new Vector2f(background.Size.X - margin * 2, background.Size.Y - margin * 4 - trackName.Size.Y - buttonSize.Y);

            tracker.Position = trackInfo.Position;

            trackerBackground.Size = trackInfo.Size;
            trackerBackground.Position = trackInfo.Position;

            PositionButton(ref shuffleButton, 0);
            PositionButton(ref repeatButton, 1);
            PositionButton(ref previousButton, 2);
            PositionButton(ref playButton, 3);
            PositionButton(ref nextButton, 4);
        }

        public void SetColor(Color color)
        {
            trackInfo.TextColor = Utilities.IsTooBright(color) ? Color.Black : Color.White;
            tracker.FillColor = color;
            trackerBackground.FillColor = new Color(color.R, color.G, color.B, 100);

            background.Element.Color = Utilities.Lerp(Style.Background, color, 0.5f);
            background.Element.ActiveColor = background.Element.Color;
            background.Element.DisabledColor = background.Element.Color;
            background.Element.HoverColor = background.Element.Color;
            background.Element.SelectedColor = background.Element.Color;
        }

        public override void Update()
        {
            trackInfo.Text = $"{Utilities.ToHumanReadableString(MusicPlayer.Time)}/{Utilities.ToHumanReadableString(MusicPlayer.Duration)}";
            float progress;
            if (trackInfo.Element.IsBeingHeld)
            {
                progress = Utilities.Clamp((Input.ScreenMousePosition.X - background.Position.X - margin) / (background.Size.X - margin * 2), 0, 1);
                MusicPlayer.Time = TimeSpan.FromSeconds(progress * MusicPlayer.Duration.TotalSeconds);
            }
            else progress = (float)MusicPlayer.Time.TotalSeconds / (float)MusicPlayer.Duration.TotalSeconds;

            tracker.Size = new Vector2f((background.Size.X - margin * 2) * progress, background.Size.Y - margin * 4 - trackName.Size.Y - buttonSize.Y);
        }

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            RecalculateLayout();

            yield return background.RenderTask;

            yield return trackName.RenderTask;
            yield return trackerBackgroundTask;
            yield return trackerTask;
            yield return trackInfo.RenderTask;

            yield return shuffleButton.RenderTask;
            yield return repeatButton.RenderTask;
            yield return previousButton.RenderTask;
            yield return playButton.RenderTask;
            yield return nextButton.RenderTask;
        }

        public override void Destroyed()
        {
            background.Deregister();
            shuffleButton.Deregister();
            repeatButton.Deregister();
            previousButton.Deregister();
            playButton.Deregister();
            nextButton.Deregister();
            trackInfo.Deregister();
            trackName.Deregister();
        }
    }
}
