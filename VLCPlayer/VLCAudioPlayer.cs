﻿using LibVLCSharp.Shared;
using Shared;
using System;
using System.IO;

namespace VLCPlayer
{
    public sealed class VLCAudioPlayer : IDisposable, IMusicPlayer
    {
        public const string ConsoleSourceIdentifier = "MUSIC PLAYER";

        private bool isReadyToPlay;
        private float volume;
        private DistrictEntry track;
        private bool isDisposed = false;
        private bool hasReachedEnd = false;

        private MediaPlayer player;
        private Media media;
        private LibVLC vlc;

        public event EventHandler<string> OnFailure;
        public event EventHandler<DistrictEntry> OnTrackChange;
        public event EventHandler OnPlay;
        public event EventHandler OnPause;
        public event EventHandler OnStop;
        public event EventHandler OnEndReached;

        public VLCAudioPlayer()
        {
            Core.Initialize();

            vlc = new LibVLC();
            vlc.Log += VlcLogWrap;
            player = new MediaPlayer(vlc);
            player.EndReached += (o, e) => { hasReachedEnd = true; };
        }

        private void VlcLogWrap(object sender, LogEventArgs e)
        {
            if (e.Level == LogLevel.Warning || e.Level == LogLevel.Error)
                Console.WriteLine(e.Message, this);
        }

        public DistrictEntry Track
        {
            get => track;
            set
            {
                string readablePath = value.Path.Normalize();
                isReadyToPlay = false;
                track = value;

                media?.Dispose();

                media = new Media(vlc, readablePath, FromType.FromPath);
                if (media.State == VLCState.Error)
                {
                    OnFailure?.Invoke(this, vlc.LastLibVLCError);
                    return;
                }
                player.Media = media;
                isReadyToPlay = true;
                Play();
                OnTrackChange?.Invoke(this, value);
            }
        }

        private void EndOfPlayback(object sender, EventArgs e)
        {
            OnStop?.Invoke(this, EventArgs.Empty);
            OnEndReached?.Invoke(this, EventArgs.Empty);
            hasReachedEnd = false;
            isReadyToPlay = false;
        }

        public PlayerState State
        {
            get
            {
                switch (player.State)
                {
                    case VLCState.NothingSpecial:
                        return PlayerState.Stopped;
                    case VLCState.Opening:
                        return PlayerState.Playing;
                    case VLCState.Buffering:
                        return PlayerState.Playing;
                    case VLCState.Playing:
                        return PlayerState.Playing;
                    case VLCState.Paused:
                        return PlayerState.Paused;
                    case VLCState.Stopped:
                        return PlayerState.Stopped;
                    case VLCState.Ended:
                        return PlayerState.Stopped;
                    case VLCState.Error:
                        return PlayerState.Stopped;
                    default:
                        return default;
                }
            }
        }

        public TimeSpan Time
        {
            get
            {
                if (AssertReadyTo("get position")) return TimeSpan.Zero;
                return TimeSpan.FromMilliseconds(player.Time);
            }

            set
            {
                if (AssertReadyTo("seek") || !player.IsSeekable) return;
                player.Position = (float)(value.TotalMilliseconds / player.Length);
            }
        }

        private bool AssertReadyTo(string action = "interact")
        {
            if (!isReadyToPlay || IsNotPlayingState || isDisposed)
            {
                Console.WriteLine($"Attempt to {action} before {nameof(player)} is ready", ConsoleSourceIdentifier);
                return true;
            }
            return false;
        }

        private bool IsNotPlayingState => (player == null) || (player.State != VLCState.Paused && player.State != VLCState.NothingSpecial && player.State != VLCState.Playing);

        public TimeSpan Duration => TimeSpan.FromMilliseconds(media?.Duration ?? 0);

        public float Volume
        {
            get => volume;
            set
            {
                volume = value;
                player.Volume = (int)(volume * 100);
            }
        }

        public void Play()
        {
            if (AssertReadyTo("play music")) return;
            player.SetPause(false);
            player.Play();
            OnPlay?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (AssertReadyTo("stop music")) return;
            player.Stop();
            player.Position = 0;
            OnStop?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            if (AssertReadyTo("pause music")) return;
            player.SetPause(true);
            OnPause?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            isDisposed = true;
            Stop();
            player?.Dispose();
            media?.Dispose();
            player = null;
            media = null;
        }
    }
}
