//#define COMPRESS
using System;
using System.IO;
using System.Linq;
using AdvancedBinary;

namespace EscudeEditor {
    public static class BinPackget {
        public static Entry[] Open(string File) {
            return Open(new StreamReader(File).BaseStream);
        }
        public static void Save(Entry[] Entries, string File) {
            if (System.IO.File.Exists(File))
                System.IO.File.Delete(File);
            Save(Entries, new StreamWriter(File).BaseStream);
        }

        public static Entry[] Open(Stream Packget) {
            Stream HeaderData = new MemoryStream();

            Packget.Seek(0xC, SeekOrigin.Begin);
            Stream Decryptor = new CryptoStream(Packget, true);
            Decryptor.CopyTo(HeaderData);
            HeaderData.Position = 0;

            BinHeader Header = new BinHeader();
            StructReader Reader = new StructReader(HeaderData);
            Reader.ReadStruct(ref Header);

            return (from x in Header.Entries
                    select new Entry() {
                        FileName = Packget.PeekCString(x.NOffset + (Header.FileCount * 12) + 0x14),
                        Content = new VirtStream(Packget, x.DOffset, x.Length)
                    }).ToArray();
        }

        public static void Save(Entry[] Files, Stream Output) {
            const string Signature = "ESC-ARC2";
            Output.WriteString(Signature);
            byte[] Key = new byte[4];
            new Random().NextBytes(Key);
            Output.Write(Key, 0, Key.Length);

            MemoryStream NamesTbl = new MemoryStream();

            BinHeader Header = new BinHeader() {
                Entries = new BinEntry[Files.Length]
            };

            Stream[] Compressed = new Stream[Files.Length];
            uint DSize = 0;
            for (int i = 0; i < Files.Length; i++) {
#if COMPRESS
                Compressed[i] = new MemoryStream(FakeCompress(Files[i].Content.ToArray()));
#else
                Compressed[i] = new MemoryStream(Files[i].Content.ToArray());
#endif
                Files[i].Content.Close();

                Header.Entries[i] = new BinEntry() {
                    DOffset = DSize,
                    NOffset = (uint)NamesTbl.Length,
                    Length = (uint)Compressed[i].Length
                };

                DSize += (uint)Compressed[i].Length;
                NamesTbl.WriteCString(Files[i].FileName);
            }
            NamesTbl.Position = 0;

            Header.NameTblLen = (uint)NamesTbl.Length;

            uint HeaderLen = ((uint)Header.Entries.Length * 12) + 0x14;
            for (int i = 0; i < Files.Length; i++) {
                Header.Entries[i].DOffset += Header.NameTblLen + HeaderLen;
            }

            byte[] HeaderData = Tools.BuildStruct(ref Header);
            CryptoStream Encryptor = new CryptoStream(Output, false, Key.GetUInt32(0));
            Encryptor.Write(HeaderData, 0, HeaderData.Length);
            Encryptor.Flush();
            NamesTbl.CopyTo(Output);

            for (int i = 0; i < Files.Length; i++) {
                Compressed[i].CopyTo(Output);
                Compressed[i].Close();
            }

            NamesTbl?.Close();
            Encryptor?.Close();
            Output?.Close();
        }

        public static byte[] FakeCompress(byte[] Data) {
            using (MemoryStream Stream = new MemoryStream()) {
                byte[] Buffer = new byte[4];
                ((uint)Tools.Reverse((uint)Data.Length)).WriteTo(Buffer, 0);
                Stream.WriteCString("acp");
                Stream.Write(Buffer, 0, Buffer.Length);

                BitWriter Writer = new BitWriter(Stream);
                for (int i = 0; i < Data.Length; i++) {
                    if (i > 0 && (i % 0x4000) == 0) {
                        Writer.PutBit(true);
                        Writer.PutBits(2);
                    }


                    Writer.PutBit(false);
                    Writer.PutBits(Data[i]);
                }

                Writer.PutBit(true);
                Writer.PutBits(0);
                Writer.Flush();

                return Stream.ToArray();
            }
        }
        public static byte[] Decompress(Stream File) {
            if (File.PeekCString(0) != "acp")
                return File.ToArray();

            uint DLen = Tools.Reverse(File.PeekUInt32(0x4));

            File.Seek(0x8, 0);
            var Decoder = new GARBro.LzwDecoder(File, DLen);

            Decoder.Unpack();

            return Decoder.Output;
        }
    }

#pragma warning disable 649
    internal struct BinHeader {
        internal uint FileCount;
        internal uint NameTblLen;

        [RArray(FieldName = "FileCount"), StructField]
        internal BinEntry[] Entries;
    }

    internal struct BinEntry {
        public uint NOffset;//Name Offset
        public uint DOffset;//Data Offset
        public uint Length;
    }
    
    public struct Entry {
        public string FileName;
        public Stream Content;
    }
#pragma warning restore 649
}
