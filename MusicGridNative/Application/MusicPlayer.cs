using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.IO;

namespace MusicGrid
{
    public sealed class MusicPlayer
    {
        public const string ConsoleSourceIdentifier = "MUSIC PLAYER";

        private MediaFoundationReader mediaFoundationReader;
        private AudioFileReader audioFileReader;
        private VorbisWaveReader vorbisWaveReader;

        private bool isReadyToPlay;
        private string track;
        private WaveStream currentStream;
        private WaveOutEvent currentWaveOut;

        public event EventHandler<Exception> OnFailure;
        public event EventHandler<string> OnTrackChange;
        public event EventHandler OnPlay;
        public event EventHandler OnPause;
        public event EventHandler OnStop;

        public MusicPlayer()
        {

        }

        private bool AssertReadyTo(string action = "interact")
        {
            if (currentWaveOut == null || !isReadyToPlay)
            {
                ConsoleEntity.Log($"Attempt to {action} before WaveOut is ready", ConsoleSourceIdentifier);
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
                currentWaveOut?.Dispose();
                currentStream?.Dispose();

                string readablePath = value.Normalize();

                currentWaveOut = new WaveOutEvent
                {
                    DesiredLatency = 1000
                };

                try
                {
                    mediaFoundationReader = new MediaFoundationReader(readablePath);
                    currentStream = mediaFoundationReader;
                }
                catch (Exception)
                {
                    ConsoleEntity.Log($"Unsupported audio format: {value}. Falling back to AudioFileReader", ConsoleSourceIdentifier);
                    try
                    {
                        mediaFoundationReader?.Dispose();
                        audioFileReader = new AudioFileReader(readablePath);
                        currentStream = audioFileReader;
                    }
                    catch (Exception e)
                    {
                        ConsoleEntity.Log($"Unsupported audio format: {value}. Falling back to VorbisWaveReader", ConsoleSourceIdentifier);
                        try
                        {
                            vorbisWaveReader?.Dispose();
                            vorbisWaveReader = new VorbisWaveReader(readablePath);
                            currentStream = vorbisWaveReader;
                        }
                        catch (Exception)
                        {
                            ConsoleEntity.Log($"Error playing {value}: {e}", ConsoleSourceIdentifier);
                            OnFailure?.Invoke(this, e);
                            return;
                        }
                    }
                }

                currentWaveOut.Init(currentStream);
                isReadyToPlay = true;
                track = value;
                OnTrackChange?.Invoke(this, value);
                ConsoleEntity.Log($"Set track to {value}", ConsoleSourceIdentifier);
            }
        }

        public PlaybackState State => currentWaveOut.PlaybackState;

        public float Volume
        {
            get => currentWaveOut.Volume;
            set => currentWaveOut.Volume = value;
        }

        public void Play()
        {
            if (AssertReadyTo("play music")) return;
            currentWaveOut.Play();
            OnPlay?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (AssertReadyTo("stop music")) return;
            currentWaveOut.Stop();
            OnPause?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            if (AssertReadyTo("pause music")) return;
            currentWaveOut.Pause();
            OnStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
