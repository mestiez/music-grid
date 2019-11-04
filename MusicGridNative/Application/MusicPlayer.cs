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
        private bool isReadyToPlay;

        private WaveStream currentStream;
        private WaveOutEvent currentWaveOut;

        public event EventHandler<Exception> OnFailure;

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

        public void SetTrack(string path)
        {
            isReadyToPlay = false;
            currentWaveOut?.Dispose();
            currentStream?.Dispose();

            string readablePath = path.Normalize();

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
            isReadyToPlay = true;
            ConsoleEntity.Log($"Set track to {path}", ConsoleSourceIdentifier);
        }

        //Ja dit gaat niet werken :(
        //public byte[] GetData(int length)
        //{
        //    if (AssertReadyTo("get data")) return 0;
        //    length = 
        //    byte[] data = new byte[length];
        //    currentStream.Read(data, currentStream.Position, length);
        //    return ;
        //}

        public void Play()
        {
            if (AssertReadyTo("play music")) return;
            currentWaveOut.Play();
        }

        public void Stop()
        {
            if (AssertReadyTo("stop music")) return;
            currentWaveOut.Stop();
        }

        public void Pause()
        {
            if (AssertReadyTo("pause music")) return;
            currentWaveOut.Pause();
        }
    }
}
