﻿using SFML.Graphics;
using SFML.System;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = SFML.Graphics.Color;

namespace MusicGrid
{
    public partial class MusicControlsEntity : Entity
    {
        private UiControllerEntity uiController;
        private DrawableElement background;
        private static readonly Vector2f buttonSize = new Vector2f(32, 32);
        private static readonly float margin = 5;
        private float buttonGap;
        private bool requiresRecalculation = true;
        private DrawableElement shuffleButton;
        private DrawableElement repeatButton;
        private DrawableElement previousButton;
        private DrawableElement playButton;
        private DrawableElement nextButton;
        private const int buttonCount = 5;
        private DrawableElement trackName;
        private DrawableElement trackInfo;
        private DrawableElement resizeButton;
        private RectangleShape tracker;
        private RectangleShape trackerBackground;
        private IRenderTask trackerTask;
        private IRenderTask trackerBackgroundTask;
        private float progress;

        private Vector2f MinimumSize => new Vector2f(buttonSize.X * buttonCount + margin * 2, 25 + margin * 4 + buttonSize.Y + 4);
        private Vector2f MaximumSize => new Vector2f(Input.WindowSize.X, Input.WindowSize.Y - World.GetEntityByType<TaskMenu>().Height);

        public Vector2f RelativePlayerSize
        {
            set => Configuration.CurrentConfiguration.PlayerSize = new Vector2f(value.X / Input.WindowSize.X, value.Y / Input.WindowSize.Y);
            get => new Vector2f(Input.WindowSize.X * Configuration.CurrentConfiguration.PlayerSize.X, Input.WindowSize.Y * Configuration.CurrentConfiguration.PlayerSize.Y);
        }

        private void SetupLayout()
        {
            var assets = MusicGridApplication.Assets;

            uiController = World.GetEntityByType<UiControllerEntity>();

            background = new DrawableElement(uiController, RelativePlayerSize, new Vector2f(50, 50));
            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);
            background.Element.IsScreenSpace = true;
            background.Element.ActiveColor = background.Element.Color;
            background.Element.HoverColor = background.Element.Color;

            trackName = new DrawableElement(uiController);
            trackName.Element.IsScreenSpace = true;
            trackName.TextAlignment = DrawableElement.TextAlignmentMode.Both;
            trackName.HideOverflow = true;
            trackName.DepthContainer = background.Element;
            trackName.Element.Color = new Color(0, 0, 0, 25);
            trackName.Element.HoverColor = trackName.Element.Color;
            trackName.Element.ActiveColor = trackName.Element.Color;

            resizeButton = new DrawableElement(uiController, new Vector2f(12, 12));
            resizeButton.Element.IsScreenSpace = true;
            resizeButton.Texture = assets.ResizeHandle;
            resizeButton.DepthContainer = background.Element;
            resizeButton.Element.Color = new Color(255, 255, 255, 125);
            resizeButton.Element.HoverColor = Color.White;
            resizeButton.Element.ActiveColor = new Color(255, 255, 255, 125);
            resizeButton.Depth = -1;

            trackInfo = new DrawableElement(uiController);
            trackInfo.Element.IsScreenSpace = true;
            trackInfo.HideOverflow = true;
            trackInfo.TextAlignment = DrawableElement.TextAlignmentMode.Both;
            trackInfo.DrawBackground = false;
            trackInfo.CharacterSize = 12;
            trackInfo.DepthContainer = background.Element;
            trackInfo.Depth = -1;
            trackInfo.Element.OnMouseDown += (o, e) => { stateBeforeTrackerPause = MusicPlayer.State; MusicPlayer.Pause(); };
            trackInfo.Element.OnMouseUp += (o, e) => { if (stateBeforeTrackerPause == PlayerState.Playing) MusicPlayer.Play(); };

            tracker = new RectangleShape();
            trackerBackground = new RectangleShape();
            trackerTask = new ShapeRenderTask(tracker, background.Depth);
            trackerBackgroundTask = new ShapeRenderTask(trackerBackground, background.Depth);
            SetColor(new Color(0, 0, 0));

            SetupButton(ref repeatButton);
            SetupButton(ref previousButton);
            SetupButton(ref playButton);
            SetupButton(ref nextButton);
            SetupButton(ref shuffleButton);

            playButton.Element.OnMouseDown += PlayPausePressed;
            nextButton.Element.OnMouseDown += (o, e) => TrackQueue.Next();
            previousButton.Element.OnMouseDown += (o, e) => TrackQueue.Previous();
            shuffleButton.Element.OnMouseDown += (o, e) => { TrackQueue.Shuffle = !TrackQueue.Shuffle; };

            shuffleButton.Texture = TrackQueue.Shuffle ? MusicGridApplication.Assets.ShuffleButton : MusicGridApplication.Assets.ShuffleDisabledButton;
            repeatButton.Texture = assets.RepeatButton;
            previousButton.Texture = assets.PreviousButton;
            playButton.Texture = assets.PlayButton;
            nextButton.Texture = assets.NextButton;
        }

        private void OnStopOrPause(object sender, EventArgs e) => playButton.Texture = MusicGridApplication.Assets.PlayButton;
        
        private void OnPlay(object sender, EventArgs e) => playButton.Texture = MusicGridApplication.Assets.PauseButton;

        private void OnShuffleChange(object sender, bool e)
        {
            shuffleButton.Texture = e ? MusicGridApplication.Assets.ShuffleButton : MusicGridApplication.Assets.ShuffleDisabledButton;
            Configuration.CurrentConfiguration.Shuffle = e;
        }

        private void SetupButton(ref DrawableElement reference)
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

        public override IEnumerable<IRenderTask> RenderScreen()
        {
            RecalculateLayout();

            if (EnableVisualiser && audioData != null && audioData.Length != 0)
            {
                uint progress = (uint)System.Math.Round(smoothTime / MusicPlayer.Duration.TotalSeconds * audioData.Length);
                if (progress < audioData.Length)
                    background.Element.Color = Utilities.Lerp(Color.Black, Color.Red, audioData[progress]);
            }

            yield return background.RenderTask;

            yield return trackName.RenderTask;
            yield return trackerBackgroundTask;
            yield return trackInfo.RenderTask;

            tracker.Position = trackInfo.Position;
            tracker.Size = new Vector2f((background.Size.X - margin * 2) * Utilities.Clamp(smoothTime / (float)MusicPlayer.Duration.TotalSeconds, 0, 1), background.Size.Y - margin * 4 - trackName.Size.Y - buttonSize.Y);
            yield return trackerTask;

            yield return shuffleButton.RenderTask;
            yield return repeatButton.RenderTask;
            yield return previousButton.RenderTask;
            yield return playButton.RenderTask;
            yield return nextButton.RenderTask;

            yield return resizeButton.RenderTask;
        }

        private void RecalculateLayout()
        {
            if (!requiresRecalculation) return;
            requiresRecalculation = false;

            var relSize = RelativePlayerSize;
            var min = MinimumSize;
            var max = MaximumSize;
            background.Size = new Vector2f(
                Utilities.Clamp(relSize.X, min.X, max.X),
                Utilities.Clamp(relSize.Y, min.Y, max.Y)
                );

            background.Position = new Vector2f(0, Input.WindowSize.Y - background.Size.Y);
            buttonGap = Utilities.CalculateEvenSpaceGap(background.Size.X, buttonSize.X * buttonCount, buttonCount, margin);

            trackName.Position = background.Position + new Vector2f(margin, margin);
            trackName.Size = new Vector2f(background.Size.X - margin * 2, 26);

            trackInfo.Position = background.Position + new Vector2f(margin, margin + margin + trackName.Size.Y);
            trackInfo.Size = new Vector2f(background.Size.X - margin * 2, background.Size.Y - margin * 4 - trackName.Size.Y - buttonSize.Y);


            trackerBackground.Size = trackInfo.Size;
            trackerBackground.Position = trackInfo.Position;

            resizeButton.Position = background.Position + new Vector2f(background.Size.X - resizeButton.Size.X, 0);

            PositionButton(ref repeatButton, 0);
            PositionButton(ref previousButton, 1);
            PositionButton(ref playButton, 2);
            PositionButton(ref nextButton, 3);
            PositionButton(ref shuffleButton, 4);
        }
    }
}
