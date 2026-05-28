using System;
using System.IO;
using System.Media;

namespace ChatbotPart2
{
    public static class AudioPlayer
    {
        // Play a WAV file synchronously (blocking)
        public static void PlaySync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return;

                if (!File.Exists(path))
                    return;

                using var player = new SoundPlayer(path);
                player.Load();
                player.PlaySync();
            }
            catch
            {
                // ignore audio errors
            }
        }

        // Play a WAV file asynchronously (non-blocking)
        public static void PlayAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return;

                if (!File.Exists(path))
                    return;

                var player = new SoundPlayer(path);
                player.LoadAsync();
                player.Play();
                // Do not dispose immediately; let GC clean up after playback.
            }
            catch
            {
                // ignore
            }
        }

        // EnsureTestGreeting: create a small test WAV file (sine beep) if it doesn't exist.
        public static bool EnsureTestGreeting(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                if (File.Exists(path))
                    return true;

                // WAV params
                int sampleRate = 22050;
                short bitsPerSample = 16;
                short channels = 1;
                double durationSeconds = 0.8;
                double frequency = 540.0;

                int samples = (int)(sampleRate * durationSeconds);
                int byteRate = sampleRate * channels * bitsPerSample / 8;
                short blockAlign = (short)(channels * bitsPerSample / 8);
                int subchunk2Size = samples * channels * bitsPerSample / 8;
                int chunkSize = 36 + subchunk2Size;

                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                using var bw = new BinaryWriter(fs);

                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(chunkSize);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);
                bw.Write((short)1);
                bw.Write(channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write(blockAlign);
                bw.Write(bitsPerSample);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(subchunk2Size);

                double amplitude = 0.25 * short.MaxValue;
                double fadeSamples = Math.Min(0.02 * sampleRate, samples / 10.0);

                for (int i = 0; i < samples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = 1.0;
                    if (i < fadeSamples) env = i / fadeSamples;
                    else if (i > samples - fadeSamples) env = (samples - i) / fadeSamples;

                    double sample = amplitude * env * Math.Sin(2.0 * Math.PI * frequency * t);
                    short s = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, (int)sample));
                    bw.Write(s);
                }

                bw.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
