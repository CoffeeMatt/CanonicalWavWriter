using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CanonicalWavWriter
{
    class Program
    {
        public static string InputFile;
        public static string OutputFile;
        public static bool OverwriteInPlace;

        public static byte[] Data;
        public static Int32 ChunkID;
        public static Int32 ChunkSize;
        public static Int32 Format;
        public static Int32 Subchunk1ID;
        public static Int32 Subchunk1Size;
        public static Int16 AudioFormat;
        public static Int16 NumChannels;
        public static Int32 SampleRate;
        public static Int32 ByteRate;
        public static Int16 BlockAlign;
        public static Int16 BitsPerSample;
        public static byte[] ExtraData;
        public static Int32 Subchunk2ID;
        public static Int32 Subchunk2Size;

        public static int HeaderLength;

        public static int ExtraDataLength
        {
            get
            {
                return Subchunk1Size - 16;
            }
        }

        public static int ActualAudioDataStartPoint
        {
            get
            {
                return 44 + ExtraDataLength;
            }
        }
        
        
        static void Main(string[] args)
        {
            if(args.Length < 1 || args.Length > 2)
            {
                // Wrong number of parameters. Print usage instructions.
                Console.WriteLine("The program strips extra meta data from the header of PCM WAV audio files and rewrites them with a canonical 44-byte header that is universlly compatible with all software that reads WAVs.");
                Console.WriteLine("Usage:");
                Console.WriteLine("CanonicalWavWriter.exe input.wav output.wav");
                Console.WriteLine("CanonicalWavWriter.exe audio.wav (Overwrites existing file)");
                return; // Exit
            }
            else if(args.Length == 1)
            {
                OverwriteInPlace = true;
                InputFile = args[0];
                OutputFile = args[0];
            }
            else if(args.Length == 2)
            {
                OverwriteInPlace = false;
                InputFile = args[0];
                OutputFile = args[1];
            }
            
            // Read in the file
            Console.WriteLine("Reading file...");
            Data = File.ReadAllBytes(InputFile);

            ChunkID = BitConverter.ToInt32(Data, 0);
            ChunkSize = BitConverter.ToInt32(Data, 4);
            Format = BitConverter.ToInt32(Data, 8);
            Subchunk1ID = BitConverter.ToInt32(Data, 12);
            Subchunk1Size = BitConverter.ToInt32(Data, 16);
            AudioFormat = BitConverter.ToInt16(Data, 20);
            NumChannels = BitConverter.ToInt16(Data, 22);
            SampleRate = BitConverter.ToInt32(Data, 24);
            ByteRate = BitConverter.ToInt32(Data, 28);
            BlockAlign = BitConverter.ToInt16(Data, 32);
            BitsPerSample = BitConverter.ToInt16(Data, 34);
            if (Subchunk1Size == 16)
            {
                Console.WriteLine("The input file already has a normal 44-byte header. Nothing to do.");
                return; // Exit
            }
            else
            {
                HeaderLength = 44 + (Subchunk1Size - 16);
                Console.WriteLine("The input file's header is " + HeaderLength.ToString() + " bytes long.");
                ExtraData = new byte[ExtraDataLength];
                Array.Copy(Data, 36, ExtraData, 0, ExtraDataLength);
            }
            Subchunk2ID = BitConverter.ToInt32(Data, 36 + ExtraDataLength);
            Subchunk2Size = BitConverter.ToInt32(Data, 40 + ExtraDataLength);

            // We have everything now. Write the new file
            Console.WriteLine("Writing file...");
            BinaryWriter writer = new BinaryWriter(File.Open(OutputFile, FileMode.Create));
            writer.Write(ChunkID);
            writer.Write(ChunkSize);
            writer.Write(Format);
            writer.Write(Subchunk1ID);
            writer.Write(16); // Change to 16
            writer.Write(AudioFormat);
            writer.Write(NumChannels);
            writer.Write(SampleRate);
            writer.Write(ByteRate);
            writer.Write(BlockAlign);
            writer.Write(BitsPerSample);
            // Skip extra data
            writer.Write(Subchunk2ID);
            writer.Write(Subchunk2Size);
            writer.Write(Data, ActualAudioDataStartPoint, (Data.Length - ActualAudioDataStartPoint));
            writer.Close();

            Console.WriteLine("Complete.");
            Console.WriteLine(ExtraDataLength.ToString() + " bytes of extra data removed from WAV header.");
        }
    }
}
