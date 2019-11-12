﻿using System;
using System.IO;
using System.Linq;
using NAudio.Wave;
using SFML.Graphics;
using SFML.System;

namespace MusicGrid
{
    public partial class MusicControlsEntity : Entity
    {
        public MusicPlayer MusicPlayer { get; } = new MusicPlayer();
        public TrackQueue TrackQueue { get; } = new TrackQueue();
        private PlaybackState stateBeforeTrackerPause;

        public MusicControlsEntity()
        {
            MusicPlayer.OnFailure += (o, m) =>
            {
                World.Add(new DialogboxEntity(m, new Vector2f(m.Length * DialogboxEntity.CharacterSize / 2 + 50, 150)), int.MaxValue);
            };

            TrackQueue.OnTrackChange += OnTrackChange;
        }

        private void OnTrackChange(object sender, DistrictEntry e)
        {
            MusicPlayer.Track = e.Path;
        }

        public override void Created()
        {
            SetupLayout();

            Input.WindowClosed += (o, e) =>
            {
                MusicPlayer.Dispose();
            };

            Input.WindowResized += OnWindowResized;
            MusicPlayer.OnTrackChange += OnTrackChange;
            MusicPlayer.OnPlay += OnPlay;
            MusicPlayer.OnStop += OnStopOrPause;
            MusicPlayer.OnPause += OnStopOrPause;

            World.Lua.LinkFunction(Functions.ToggleStream, this, () => { TogglePausePlay(); });
            World.Lua.LinkFunction(Functions.Pause, this, () => { MusicPlayer.Pause(); });
            World.Lua.LinkFunction(Functions.Play, this, () => { MusicPlayer.Play(); });
            World.Lua.LinkFunction(Functions.Stop, this, () => { MusicPlayer.Stop(); });
            World.Lua.LinkFunction(Functions.SetVolume, this, (float a) => { MusicPlayer.Volume = Math.Max(Math.Min(1, a), 0); ConsoleEntity.Log($"Volume set to {Math.Round(MusicPlayer.Volume * 100)}%", "MPE"); });
            World.Lua.LinkFunction(Functions.SetTrack, this, (string track) => { MusicPlayer.Track = track; });
        }

        private void OnStopOrPause(object sender, EventArgs e) => playButton.Texture = MusicGridApplication.Assets.PlayButton;
        private void OnPlay(object sender, EventArgs e) => playButton.Texture = MusicGridApplication.Assets.PauseButton;

        private void PlayPausePressed(object sender, MouseEventArgs e)
        {
            TogglePausePlay();
        }

        public void TogglePausePlay()
        {
            if (MusicPlayer.State == PlaybackState.Paused)
                MusicPlayer.Play();
            else
                MusicPlayer.Pause();
        }

        private void OnTrackChange(object sender, string e)
        {
            trackName.Text = Path.GetFileNameWithoutExtension(e);
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
            if (trackInfo.Size.Y >= trackInfo.CharacterSize)
                trackInfo.Text = $"{Utilities.ToHumanReadableString(MusicPlayer.Time)}/{Utilities.ToHumanReadableString(MusicPlayer.Duration)}";
            else trackInfo.Text = "";

            if (trackInfo.Element.IsBeingHeld && MusicPlayer.State != PlaybackState.Stopped)
            {
                progress = Utilities.Clamp((Input.ScreenMousePosition.X - background.Position.X - margin) / (background.Size.X - margin * 2), 0, 1);
                MusicPlayer.Time = TimeSpan.FromSeconds(progress * MusicPlayer.Duration.TotalSeconds);
            }
            else progress = (float)MusicPlayer.Time.TotalSeconds / (float)MusicPlayer.Duration.TotalSeconds;

            if (resizeButton.Element.IsBeingHeld)
            {
                var rS = RelativePlayerSize;
                var min = minimumSize;
                var max = maximumSize;
                var d = (Vector2f)Input.ScreenMouseDelta;
                RelativePlayerSize = new Vector2f(
                    Utilities.Clamp(rS.X + d.X, min.X, max.X),
                    Utilities.Clamp(rS.Y - d.Y, min.Y, max.Y)
                    );
                if (Utilities.SquaredMagnitude(d) > 0)
                    requiresRecalculation = true;
            }
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
