using System;
using System.IO;

namespace EscudeEditor {
    internal class BitWriter : Stream {
        Stream Base;
        int Buffer = 0;
        int BufferedBits = 0;
        internal BitWriter(Stream Output) {
            Base = Output;
        }
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => Base.Length;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() {
            Base.WriteByte((byte)(Buffer & 0xFF));
            Buffer = 0;
            BufferedBits = 0;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public void PutBits(byte Byte) {
            for (int i = 0; i < 8; i++)
                PutBit(Byte.GetBit(i));
        }
        public void PutBit(bool Bit) {
            Buffer <<= 1;
            Buffer |= (byte)(Bit ? 1 : 0);
            BufferedBits++;

            if (BufferedBits == 8) {
                Base.WriteByte((byte)(Buffer & 0xFF));
                BufferedBits -= 8;
            }
        }
    }
}


