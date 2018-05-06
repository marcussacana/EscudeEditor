using System;
using System.IO;

namespace EscudeEditor {
    public class CryptoStream : Stream {

        #region MorktCode
        //
        // Copyright (C) 2015 by morkt
        //
        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files (the "Software"), to
        // deal in the Software without restriction, including without limitation the
        // rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
        // sell copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:
        //
        // The above copyright notice and this permission notice shall be included in
        // all copies or substantial portions of the Software.
        //
        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
        // FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
        // IN THE SOFTWARE.
        //
        uint Key {
            get {
                _key ^= 0x65AC9365;
                _key ^= (((_key >> 1) ^ _key) >> 3) ^ (((_key << 1) ^ _key) << 3);
                return _key;
            }
        }
        #endregion


        uint _key = 0x0;
        uint MaxPos = 0x00;
        byte[] InBuffer = new byte[0];
        Stream Base;

        public CryptoStream(Stream Stream, bool Read, uint Key = 0) {
            Base = Stream;

            if (Read) {
                _key = Stream.PeekUInt32(0x8);
                MaxPos = ((Stream.PeekUInt32(0xC) ^ this.Key) * 12) + 0xC;
                _key = Stream.PeekUInt32(0x8);
            } else
                _key = Key;
        }
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => Base.Length;

        public override long Position { get => Base.Position; set => throw new NotImplementedException(); }


        public override void Flush() {
            if (Base.CanWrite)
                Base.Write(InBuffer, 0, InBuffer.Length);
            InBuffer = new byte[0];
        }

        public override int Read(byte[] buffer, int offset, int count) {
            uint Reaming = (MaxPos - (uint)Base.Position)+0x8;
            if (count > Reaming)
                count = (int)Reaming;

            int Readed = Base.Read(buffer, offset, count);
            for (uint i = 0; i < count / 4; i++) {
                uint Index = (uint)offset + (i * 4);
                uint DW = buffer.GetUInt32(Index);
                (DW ^ Key).WriteTo(buffer, Index);
            }

            return Readed;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            int ToBuffer = (count + InBuffer.Length) % 4;

            byte[] Buffer = new byte[(count - ToBuffer) + InBuffer.Length];
            InBuffer.CopyTo(Buffer, 0);
            for (uint i = 0; i < Buffer.Length; i++)
                Buffer[i+InBuffer.Length] = buffer[i+offset];

            InBuffer = new byte[ToBuffer];
            for (int i = 0; i < InBuffer.Length; i++)
                InBuffer[i] = buffer[i + ((offset + count) - ToBuffer)];

            for (uint i = 0; i < Buffer.Length / 4; i++) {
                uint DW = Buffer.GetUInt32(i*4);
                (DW ^ Key).WriteTo(Buffer, i*4);
            }

            Base.Write(Buffer, 0, Buffer.Length);
        }
    }
}
