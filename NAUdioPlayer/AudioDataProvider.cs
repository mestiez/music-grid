using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Linq;

namespace NAudioPlayer
{
    public sealed class AudioDataProvider
    {
        public const string ConsoleSourceIdentifier = "AUDIO DATA PROVIDER";
        private WaveStream waveStream;
        private IWavePlayer wavePlayer;

        public float[] GetWaveData(string path)
        {
            string readablePath = path.Normalize();

            wavePlayer = new WaveOutEvent();
            waveStream = WavePlayerCascade.CreateStream(readablePath);
            if (waveStream == null)
            {
                var m = $"File {path} is unsupported";
                Console.WriteLine(m, ConsoleSourceIdentifier);
                throw new ArgumentException(m);
            }
            try
            {
                wavePlayer.Init(waveStream);
                byte[] data = new byte[waveStream.Length];
                waveStream.Read(data, 0, data.Length);
                return data.Select(b => b / 256f).ToArray();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                wavePlayer.Dispose();
                waveStream.Dispose();
            }
        }

        /*/allocation bad!
        public FrequencyDomain[] GetData(string path, int domainSize = 2048)
        {
            string readablePath = path.Normalize();

            wavePlayer = new WaveOutEvent();
            waveStream = WavePlayerCascade.CreateStream(readablePath);
            if (waveStream == null)
            {
                var m = $"File {path} is unsupported";
                Console.WriteLine(m, ConsoleSourceIdentifier);
                throw new ArgumentException(m);
            }
            try
            {
                wavePlayer.Init(waveStream);
                byte[] data = new byte[waveStream.Length];
                waveStream.Read(data, 0, data.Length);

                int fftCount = (int)Math.Floor(waveStream.Length / (float)domainSize);
                FrequencyDomain[] fft = new FrequencyDomain[fftCount];

                for (int i = 0; i < fftCount; i++)
                {
                    Complex[] complexData = data.
                        Skip(i * domainSize).
                        Take(domainSize).
                        Select(c => new Complex() { X = c / 256f }).
                        ToArray();

                    FastFourierTransform.FFT(true, (int)Math.Log(domainSize, 2), complexData);
                    fft[i].Amplitudes = complexData.Select(c => c.X).ToArray();
                }

                return fft;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                wavePlayer.Dispose();
                waveStream.Dispose();
            }
        }*/

        public struct FrequencyDomain
        {
            public float[] Amplitudes;

            public FrequencyDomain(float[] amplitudes)
            {
                Amplitudes = amplitudes;
            }
        }
    }
}
