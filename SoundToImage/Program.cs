using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace SoundToImage
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("SoundToImage [MODE] [FILENAME]");
                Console.WriteLine("MODE: 0 - audio to .bmp file, 1 - image to .wav file");
                Console.WriteLine("Valid audio formats - .wav, .mp3");
                return;
            }
            if (args.Length == 1)
            {
                Console.WriteLine("Please include a file.");
                return;
            }
            if (args.Length == 2)
            {
                int mode = int.Parse(args[0]);
                switch (mode)
                {
                    case 0: SoundToImageConverter.GetImageFromAudio(args[1]);
                        break;
                    case 1: SoundToImageConverter.GetAudioFromImage(args[1]);
                        break;
                    default: Console.WriteLine("Mode {0} is invalid. Please choose a correct mode from the list.",mode);
                        break;
                }
            }
        }
    }
}
