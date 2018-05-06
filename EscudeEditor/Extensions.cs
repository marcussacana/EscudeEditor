using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EscudeEditor {
    internal static class Extensions {

        public static string GetString(this byte[] Binary, uint At, uint Len) {
            byte[] Buffer = new byte[Len];
            for (int i = 0; i < Len; i++) {
                Buffer[i] = Binary[i + At];
            }

            return Encoding.GetEncoding(932).GetString(Buffer);
        }
        public static uint GetUInt32(this byte[] Binary, uint Position) {
            byte[] Buffer = new byte[4];
            for (int i = 0; i < 4; i++)
                Buffer[i] = Binary[i+Position];

            return BitConverter.ToUInt32(Buffer, 0);
        }

        public static void WriteTo(this uint Value, byte[] Binary, uint At) {
            BitConverter.GetBytes(Value).CopyTo(Binary, At);
        }     

        public static bool GetBit(this byte Byte, int Bit) {
            return (Byte & (1 << (7 - Bit))) != 0;
        }

        public static byte[] ToArray(this Stream Stream) {
            Stream.Seek(0,0);

            byte[] Data = new byte[Stream.Length];
            Stream.Read(Data, 0, Data.Length);

            return Data;
        }

        public static void WriteString(this Stream Stream, string String) {
            byte[] Buffer = Encoding.GetEncoding(932).GetBytes(String);

            Stream.Write(Buffer, 0, Buffer.Length);
        }
        public static void WriteCString(this Stream Stream, string String) {
            Stream.WriteString(String);
            Stream.WriteByte(0x00);
        }
        public static string PeekCString(this Stream Stream, uint At) {
            long Pos = Stream.Position;
            List<byte> Buffer = new List<byte>();

            Stream.Position = At;
            int b = 0;
            while ((b = Stream.ReadByte()) >= 1)
                Buffer.Add((byte)b);
            Stream.Position = Pos;

            return Encoding.GetEncoding(932).GetString(Buffer.ToArray());
        }
        public static uint PeekUInt32(this Stream Stream, uint At) {
            long Pos = Stream.Position;
            byte[] Buffer = new byte[4];
            Stream.Position = At;
            Stream.Read(Buffer, 0, Buffer.Length);
            Stream.Position = Pos;

            return Buffer.GetUInt32(0);
        }
    }
}
