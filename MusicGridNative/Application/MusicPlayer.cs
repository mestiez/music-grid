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

        private WaveStream currentStream;
        private WaveOutEvent currentWaveOut;

        public event EventHandler<Exception> OnFailure;

        public MusicPlayer()
        {

        }

        private bool AssertNoNullSource(string action = "interact")
        {
            if (currentWaveOut == null)
            {
                ConsoleEntity.Log($"Attempt to {action} without having a track set", ConsoleSourceIdentifier);
                return true;
            }
            return false;
        }

        public void SetTrack(string path)
        {
            currentWaveOut?.Dispose();
            currentStream?.Dispose();

            string readablePath = path.Normalize();

            currentWaveOut = new WaveOutEvent
            {
                DesiredLatency = 500
            };

            try
            {
                mediaFoundationReader = new MediaFoundationReader(readablePath);
                currentStream = mediaFoundationReader;
            }
            catch (Exception)
            {
                ConsoleEntity.Log($"Unsupported audio format: {path}. Falling back to AudioFileReader", ConsoleSourceIdentifier);
                try
                {
                    mediaFoundationReader?.Dispose();
                    audioFileReader = new AudioFileReader(readablePath);
                    currentStream = audioFileReader;
                }
                catch (Exception e)
                {
                    ConsoleEntity.Log($"Error playing {path}: {e}", ConsoleSourceIdentifier);
                    OnFailure?.Invoke(this, e);
                    return;
                }
            }

            currentWaveOut.Init(currentStream);
        }

        public void Play()
        {
            if (AssertNoNullSource("play music")) return;
            currentWaveOut.Play();
        }

        public void Stop()
        {
            if (AssertNoNullSource("stop music")) return;
            currentWaveOut.Stop();
        }

        public void Pause()
        {
            if (AssertNoNullSource("pause music")) return;
            currentWaveOut.Pause();
        }
    }
}
