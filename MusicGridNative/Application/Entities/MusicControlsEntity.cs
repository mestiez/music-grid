using System;
using SFML.Graphics;
using SFML.System;
using Shared;
using VLCPlayer;
using Color = SFML.Graphics.Color;

namespace MusicGrid
{
    public partial class MusicControlsEntity : Entity
    {
        public Repeat RepeatMode { get; set; } = Repeat.RepeatQueue;
        public IMusicPlayer MusicPlayer { get; } = new VLCAudioPlayer();
        public TrackQueue TrackQueue { get; } = new TrackQueue();
        private PlayerState stateBeforeTrackerPause;

        public override void Created()
        {
            SetupLayout();

            Input.WindowClosed += (o, e) =>
            {
                MusicPlayer.Dispose();
            };

            Input.WindowResized += OnWindowResized;
            MusicPlayer.OnFailure += (o, m) => World.Add(new DialogboxEntity(m, new Vector2f(m.Length * DialogboxEntity.CharacterSize / 2 + 50, 150)), int.MaxValue);
            MusicPlayer.OnTrackChange += OnTrackChange;
            MusicPlayer.OnPlay += OnPlay;
            MusicPlayer.OnStop += OnStopOrPause;
            MusicPlayer.OnPause += OnStopOrPause;
            TrackQueue.OnTrackChange += (o, e) => { MusicPlayer.Track = e; };

            World.Lua.LinkFunction(Functions.ToggleStream, this, () => { TogglePausePlay(); });
            World.Lua.LinkFunction(Functions.Pause, this, () => { MusicPlayer.Pause(); });
            World.Lua.LinkFunction(Functions.Play, this, () => { MusicPlayer.Play(); });
            World.Lua.LinkFunction(Functions.Stop, this, () => { MusicPlayer.Stop(); });
            World.Lua.LinkFunction(Functions.SetVolume, this, (float a) => { MusicPlayer.Volume = Math.Max(Math.Min(1, a), 0); ConsoleEntity.Log($"Volume set to {Math.Round(MusicPlayer.Volume * 100)}%", "MPE"); });
        }

        // TODO connect to custom check
        private void OnEndReached(object sender, EventArgs e)
        {
            switch (RepeatMode)
            {
                case Repeat.RepeatTrack:
                    //MusicPlayer.Time = TimeSpan.Zero;
                    //MusicPlayer.Play();
                    break;
                case Repeat.RepeatQueue:
                    TrackQueue.Next();
                    MusicPlayer.Play();
                    break;
            }
        }

        private void OnStopOrPause(object sender, EventArgs e) => playButton.Texture = MusicGridApplication.Assets.PlayButton;
        private void OnPlay(object sender, EventArgs e) => playButton.Texture = MusicGridApplication.Assets.PauseButton;

        private void PlayPausePressed(object sender, MouseEventArgs e)
        {
            TogglePausePlay();
        }

        public void TogglePausePlay()
        {
            if (MusicPlayer.State != PlayerState.Playing)
                MusicPlayer.Play();
            else
                MusicPlayer.Pause();
        }

        private void OnTrackChange(object sender, DistrictEntry e)
        {
            trackName.Text = e.Name;
            SetColor(World.GetEntityByType<DistrictManager>().GetDistrictFromEntry(e)?.Color.ToSFML() ?? Color.Magenta);
        }

        public void SetColor(SFML.Graphics.Color color)
        {
            trackInfo.TextColor = Utilities.IsTooBright(color) ? Color.Black : Color.White;
            tracker.FillColor = color;
            trackerBackground.FillColor = new Color(color.R, color.G, color.B, 100);

            background.Element.Color = Utilities.Lerp(Style.Background.ToSFML(), color, 0.5f);
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

            if (trackInfo.Element.IsBeingHeld && MusicPlayer.State != PlayerState.Stopped)
            {
                progress = Utilities.Clamp((Input.ScreenMousePosition.X - background.Position.X - margin) / (background.Size.X - margin * 2), 0, 1);
                MusicPlayer.Time = TimeSpan.FromSeconds(progress * MusicPlayer.Duration.TotalSeconds);
            }
            else progress = (float)MusicPlayer.Time.TotalSeconds / (float)MusicPlayer.Duration.TotalSeconds;

            if (resizeButton.Element.IsBeingHeld)
            {
                var rS = RelativePlayerSize;
                var min = MinimumSize;
                var max = MaximumSize;
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

        public enum Repeat
        {
            NoRepeat,
            RepeatTrack,
            RepeatQueue
        }
    }
}
