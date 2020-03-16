using System;

namespace Shared
{
    public interface IMusicPlayer
    {
        TimeSpan Duration { get; }
        PlayerState State { get; }
        TimeSpan Time { get; set; }
        DistrictEntry Track { get; set; }
        float Volume { get; set; }

        event EventHandler<string> OnFailure;
        event EventHandler OnPause;
        event EventHandler OnPlay;
        event EventHandler OnStop;
        event EventHandler<DistrictEntry> OnTrackChange;

        void Dispose();
        void Pause();
        void Play();
        void Stop();
    }

    public enum PlayerState
    {
        Stopped, Playing, Paused
    }
}