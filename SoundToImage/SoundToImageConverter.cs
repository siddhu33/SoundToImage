using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SoundToImage
{
    class SoundToImageConverter
    {
        public static void GetImageFromAudio(string filename)
        {
            double[] left;
            double[] right;
            AudioToArray(filename, out left, out right);
            Console.WriteLine("Getting Image from {0}",Path.GetFileName(filename));
            int seconds = left.Length / 44100;
            Console.WriteLine("Channel Information:[{1},{2}],length:{3}m{4}s", Path.GetFileName(filename), left.Length, right.Length,seconds / 60, seconds % 60);
            int horizontal = 210 * (int)Math.Ceiling(Math.Sqrt(seconds));
            int vertical = left.Length / horizontal + 1;
            Bitmap bitmap = new Bitmap(horizontal, vertical);
            for (int y = 0; y < vertical; y++)
            {
                for (int x = 0; x < horizontal; x++)
                {
                    int index = y * horizontal + x;
                    bitmap.SetPixel(x, y, index < left.Length ? ColorFromHsv(180 * (left[index] + 1.0), right[index] / 2.0f + 0.5f, 0.5) : Color.FromArgb(0, 0, 0));
                }
            }
            var outputPath = $"{Path.GetFileNameWithoutExtension(filename)}.bmp";
            bitmap.Save(outputPath, ImageFormat.Bmp);
            Console.WriteLine("Written output image.");
        }

        public static void GetAudioFromImage(string filename)
        {
            Console.WriteLine("Getting Audio from {0}", Path.GetFileName(filename));
            WaveFormat waveFormat = new WaveFormat(44100, 16, 2);
            using (WaveFileWriter wfw = new WaveFileWriter($"{Path.GetFileNameWithoutExtension(filename)}.wav", waveFormat))
            {
                Image image = Image.FromFile(filename);
                Bitmap bitmap = new Bitmap(image);
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        float hue, saturation, value;
                        ColorToHsv(color, out hue, out saturation, out value);
                        wfw.WriteSample((hue - 180) / 180.0f);
                        wfw.WriteSample((saturation - 0.5f) * 2.0f);
                    }
                }
            }
        }

        public static void ColorToHsv(Color color, out float hue, out float saturation, out float value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (float)(max == 0 ? 0 : 1d - (1d * min / max));
            value = (float)(max / 255d);
        }

        public static Color ColorFromHsv(double hue, double saturation, double value)
        {
            //http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        // Returns left and right double arrays. 'right' will be null if sound is mono.
        public static void AudioToArray(string filename, out double[] left, out double[] right)
        {
            string ext = Path.GetExtension(filename);
            switch (ext)
            {
                case ".wav":
                    using (WaveFileReader reader = new WaveFileReader(filename))
                    {
                        ObtainSamples(out left, out right, reader);
                    }
                    break;
                case ".mp3":
                    using (Mp3FileReader reader = new Mp3FileReader(filename))
                    {
                        ObtainSamples(out left, out right, reader);
                    }
                    break;
                default:
                    using (AudioFileReader reader = new AudioFileReader(filename))
                    {
                        ObtainSamples(out left, out right, reader);
                    }
                    break;
            }
        }

        private static void ObtainSamples(out double[] left, out double[] right, WaveStream reader)
        {
            if (reader.WaveFormat.BitsPerSample != 16)
            {
                throw new Exception("Only works with 16 bit audio");
            }
            byte[] buffer = new byte[reader.Length];
            int read = reader.Read(buffer, 0, buffer.Length);
            short[] sampleBuffer = new short[read / 2];
            Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
            left = new double[sampleBuffer.Length / 2];
            right = new double[sampleBuffer.Length / 2];
            for (int i = 0; i < (sampleBuffer.Length - 1); i += 2)
            {
                left[i / 2] = sampleBuffer[i] / 32768.0f;
                right[i / 2] = sampleBuffer[i + 1] / 32768.0f;
            }
        }
    }
}
