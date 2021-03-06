﻿using NAudio.Wave;
using Shared;
using System;

namespace NAudioPlayer
{
    public sealed class NAudioPlayer : IDisposable, IMusicPlayer
    {
        public const string ConsoleSourceIdentifier = "MUSIC PLAYER";

        private bool isReadyToPlay;
        private float volume;
        private DistrictEntry track;
        private WaveStream waveStream;
        private IWavePlayer wavePlayer;
        private bool isDisposed = false;
        private bool hasEnded = false;

        public event EventHandler<string> OnFailure;
        public event EventHandler<DistrictEntry> OnTrackChange;
        public event EventHandler OnPlay;
        public event EventHandler OnPause;
        public event EventHandler OnStop;

        private bool AssertReadyTo(string action = "interact")
        {
            if (wavePlayer == null || !isReadyToPlay)
            {
                Console.WriteLine($"Attempt to {action} before {nameof(wavePlayer)} is ready", ConsoleSourceIdentifier);
                return true;
            }
            return false;
        }

        public DistrictEntry Track
        {
            get => track;
            set
            {
                bool wasPlaying = State == PlayerState.Playing;
                isReadyToPlay = false;
                string readablePath = value.Path.Normalize();

                wavePlayer?.Stop();

                wavePlayer?.Dispose();
                waveStream?.Dispose();

                wavePlayer = new WaveOutEvent();
                waveStream = WavePlayerCascade.CreateStream(readablePath);
                if (waveStream == null)
                {
                    var m = $"File {value} is unsupported";
                    Console.WriteLine(m, ConsoleSourceIdentifier);
                    OnFailure?.Invoke(this, m);
                    return;
                }
                try
                {
                    wavePlayer.Init(waveStream);
                    wavePlayer.Volume = Volume;
                    track = value;
                    Console.WriteLine($"Set track to {value}", ConsoleSourceIdentifier);
                    isReadyToPlay = true;
                    hasEnded = false;
                    wavePlayer.PlaybackStopped += (o, e) => { hasEnded = true; OnStop?.Invoke(this, EventArgs.Empty); isReadyToPlay = false; };
                    if (wasPlaying) Play();
                    OnTrackChange?.Invoke(this, value);

                    byte[] data = new byte[waveStream.Length];
                    waveStream.Read(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    OnFailure?.Invoke(this, e.Message);
                }
            }
        }

        public PlayerState State
        {
            get
            {
                return (PlayerState)(wavePlayer?.PlaybackState ?? 0);
            }
        }

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
            waveStream.Position = 0;
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
