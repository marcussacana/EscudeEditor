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
using System;
using System.IO;

public class GARBro {
    internal sealed class LzwDecoder : IDisposable {
        private MsbBitStream m_input;
        private byte[] m_output;

        public byte[] Output { get { return m_output; } }

        public LzwDecoder(Stream input, uint unpacked_size) {
            m_input = new MsbBitStream(input, true);
            m_output = new byte[unpacked_size];
        }

        public void Unpack() {
            int dst = 0;
            var lzw_dict = new int[0x8900];
            int token_width = 9;
            int dict_pos = 0;
            while (dst < m_output.Length) {
                int token = m_input.GetBits(token_width);
                if (-1 == token)
                    throw new EndOfStreamException("Invalid compressed stream");
                else if (0x100 == token) // end of input
                    break;
                else if (0x101 == token) // increase token width
                {
                    ++token_width;
                    if (token_width > 24)
                        throw new Exception("Invalid comressed stream");

                } else if (0x102 == token) // reset dictionary
                {
                    token_width = 9;
                    dict_pos = 0;

                } else {

                    if (dict_pos >= lzw_dict.Length)
                        throw new Exception("Invalid comressed stream");
                    lzw_dict[dict_pos++] = dst;

                    if (token < 0x100) {
                        m_output[dst++] = (byte)token;
                    } else {
                        token -= 0x103;
                        if (token >= dict_pos)
                            throw new Exception("Invalid comressed stream");
                        int src = lzw_dict[token];
                        int count = Math.Min(m_output.Length - dst, lzw_dict[token + 1] - src + 1);
                        if (count < 0)
                            throw new Exception("Invalid comressed stream");

                        for (int i = 0; i < count; i++)
                            m_output[dst + i] = m_output[src + i];

                        dst += count;
                    }
                }
            }
        }

        #region IDisposable Members
        bool _disposed = false;
        public void Dispose() {
            if (!_disposed) {
                m_input.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class BitStream : IDisposable {
        protected Stream m_input;
        private bool m_should_dispose;

        protected int m_bits = 0;
        protected int m_cached_bits = 0;

        public Stream Input { get { return m_input; } }
        public int CacheSize { get { return m_cached_bits; } }

        protected BitStream(Stream file, bool leave_open) {
            m_input = file;
            m_should_dispose = !leave_open;
        }

        public void Reset() {
            m_cached_bits = 0;
        }

        #region IDisposable Members
        bool m_disposed = false;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!m_disposed) {
                if (disposing && m_should_dispose && null != m_input)
                    m_input.Dispose();
                m_disposed = true;
            }
        }
        #endregion
    }

    public interface IBitStream {
        int GetBits(int count);
        int GetNextBit();
        void Reset();
    }

    public class MsbBitStream : BitStream, IBitStream {
        public MsbBitStream(Stream file, bool leave_open = false)
            : base(file, leave_open) {
        }

        public int GetBits(int count) {
            while (m_cached_bits < count) {
                int b = m_input.ReadByte();
                if (-1 == b)
                    return -1;
                m_bits = (m_bits << 8) | b;
                m_cached_bits += 8;
            }
            int mask = (1 << count) - 1;
            m_cached_bits -= count;
            return (m_bits >> m_cached_bits) & mask;
        }

        public int GetNextBit() {
            return GetBits(1);
        }
    }
}