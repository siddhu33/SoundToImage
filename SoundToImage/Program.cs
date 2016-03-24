using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SoundToImage
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] left;
            double[] right;
            if (args.Length < 1)
            {
                Console.WriteLine("Please add a file.");
            }
            AudioToArray(args[0], out left, out right);
            Console.WriteLine("{0}[{1},{2}]",Path.GetFileName(args[0]),left.Length,right.Length);
            int seconds = left.Length/44100;
            int horizontal = 210*(int)Math.Ceiling(Math.Sqrt(seconds));
            int vertical = left.Length/horizontal + 1;
            Console.WriteLine(left.Where(c=>Math.Abs(c) < 0.00000000001).Count());
            Bitmap bitmap = new Bitmap(horizontal,vertical);
            for (int i = 0; i < vertical; i++)
            {
                for (int j = 0; j < horizontal; j++)
                {
                    int index = i*horizontal + j;
                    bitmap.SetPixel(j, i, index < left.Length ? ColorFromHsv(360.0*Math.Abs(left[index]),1.0, 0.5) : Color.FromArgb(0, 0, 0));
                }
            }
            bitmap.Save("output.bmp");
            Console.WriteLine("Written output file.");
        }


        public static Color ColorFromHsv(double hue, double saturation, double value)
        {
            //Thank you to whoever it was from StackOverflow for providing this code.
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
            short[] sampleBuffer = new short[read/2];
            Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
            left = new double[sampleBuffer.Length/2];
            right = new double[sampleBuffer.Length/2];
            for (int i = 0; i < (sampleBuffer.Length - 1); i += 2)
            {
                left[i/2] = sampleBuffer[i]/32768.0f;
                right[i/2] = sampleBuffer[i + 1]/32768.0f;
            }
        }
    }
}
