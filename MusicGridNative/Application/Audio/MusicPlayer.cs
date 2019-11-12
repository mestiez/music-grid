using NAudio.Wave;
using System;
using System.IO;

namespace MusicGrid
{
    public sealed class MusicPlayer : IDisposable
    {
        public const string ConsoleSourceIdentifier = "MUSIC PLAYER";

        private bool isReadyToPlay;
        private float volume;
        private string track;
        private WaveStream waveStream;
        private IWavePlayer wavePlayer;
        private bool isDisposed = false;

        public event EventHandler<string> OnFailure;
        public event EventHandler<string> OnTrackChange;
        public event EventHandler OnPlay;
        public event EventHandler OnPause;
        public event EventHandler OnStop;

        private bool AssertReadyTo(string action = "interact")
        {
            if (wavePlayer == null || !isReadyToPlay)
            {
                ConsoleEntity.Log($"Attempt to {action} before {nameof(wavePlayer)} is ready", ConsoleSourceIdentifier);
                return true;
            }
            return false;
        }

        public string Track
        {
            get => track;
            set
            {
                if (track == value) return;
                isReadyToPlay = false;
                string readablePath = value.Normalize();
                wavePlayer?.Dispose();
                waveStream?.Dispose();

                wavePlayer = new WaveOut();
                wavePlayer.PlaybackStopped += EndOfPlayback;
                waveStream = WavePlayerCascade.CreateStream(readablePath);
                if (waveStream == null)
                {
                    var m = $"File {value} is unsupported";
                    ConsoleEntity.Log(m, ConsoleSourceIdentifier);
                    OnFailure?.Invoke(this, m);
                    return;
                }
                try
                {
                    wavePlayer.Init(waveStream);
                    wavePlayer.Volume = Volume;
                    track = value;
                    ConsoleEntity.Log($"Set track to {value}", ConsoleSourceIdentifier);
                    isReadyToPlay = true;
                    OnTrackChange?.Invoke(this, value);
                }
                catch (Exception e)
                {
                    OnFailure?.Invoke(this, e.Message);
                }
            }
        }

        private void EndOfPlayback(object sender, StoppedEventArgs e)
        {
            OnStop?.Invoke(this, EventArgs.Empty);
        }

        public PlaybackState State => wavePlayer?.PlaybackState ?? default;

        public TimeSpan Time
        {
            get
            {
                if ((waveStream?.CanRead ?? false) && !isDisposed)
                    return waveStream?.CurrentTime ?? TimeSpan.Zero;
                else return TimeSpan.Zero;
            }

            set
            {
                if (AssertReadyTo("seek") || !waveStream.CanSeek) return;
                waveStream.CurrentTime = value;
            }
        }
        public TimeSpan Duration => waveStream?.TotalTime ?? default;

        public float Volume
        {
            get => volume;
            set
            {
                volume = value;
                if (isReadyToPlay)
                    wavePlayer.Volume = volume;
            }
        }

        public void Play()
        {
            if (AssertReadyTo("play music")) return;
            wavePlayer.Play();
            OnPlay?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (AssertReadyTo("stop music")) return;
            wavePlayer.Stop();
            OnStop?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            if (AssertReadyTo("pause music")) return;
            wavePlayer.Pause();
            OnPause?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            isDisposed = true;
            Stop();
            waveStream?.Dispose();
            wavePlayer?.Dispose();
            waveStream = null;
            wavePlayer = null;
        }
    }
}
