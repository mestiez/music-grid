using NAudio.Vorbis;
using NAudio.Wave;
using System;

namespace NAudioPlayer
{
    public struct WavePlayerCascade
    {
        private const string ConsoleSourceIdentifier = "WAVE CASCADE";

        public static WaveStream CreateStream(string audioFilePath)
        {
            WaveStream stream = default;

            if (Try<MediaFoundationReader>(audioFilePath, ref stream)) return stream;
            if (Try<AudioFileReader>(audioFilePath, ref stream)) return stream;
            if (Try<Mp3FileReader>(audioFilePath, ref stream)) return stream;
            if (Try<AiffFileReader>(audioFilePath, ref stream)) return stream;
            if (Try<VorbisWaveReader>(audioFilePath, ref stream)) return stream;
            if (Try<WaveFileReader>(audioFilePath, ref stream)) return stream;
            if (Try<CueWaveFileReader>(audioFilePath, ref stream)) return stream;
            return null;
        }

        private static bool Try<T>(string audioFilePath, ref WaveStream stream) where T : WaveStream
        {
            try
            {
                stream?.Dispose();
                stream = Activator.CreateInstance(typeof(T), audioFilePath) as T;
            }
            catch (Exception)
            {
                return false;
            }
            Console.WriteLine($"Using {typeof(T).Name}", ConsoleSourceIdentifier);
            return true;
        }
    }
}
