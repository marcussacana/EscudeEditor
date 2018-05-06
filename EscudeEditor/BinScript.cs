using AdvancedBinary;
using System;
using System.Text;

namespace EscudeEditor {
    public class BinScript {
        const string Signature = "ESCR1_00";

        public Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Script;
        BinFormat Bin;
        public BinScript(byte[] Script) => this.Script = Script;

        public string[] Import() {
            if (Script.GetString(0, 0x8) != Signature)
                throw new Exception("Bad Escude Script");

            Bin = new BinFormat();
            Tools.ReadStruct(Script, ref Bin, Encoding: Encoding);

            return Bin.Strings;
        }

        public byte[] Export(string[] Strings) {
            Bin.Strings = Strings;
            uint Len = 0;
            for (int i = 0; i < Strings.Length; i++) {
                Bin.Offsets[i] = Len;
                Len += (uint)Encoding.GetByteCount(Strings[i]) + 1;//+1 = \x0
            }
            return Tools.BuildStruct(ref Bin, Encoding: Encoding);
        }
    }
    
#pragma warning disable 649, 169
    internal struct BinFormat {
        [FString(Length = 0x8)]
        public string Signature;

        public uint StringCount;

        [RArray(FieldName = "StringCount")]
        public uint[] Offsets;

        [PArray(PrefixType = Const.UINT32)]
        public byte[] VM;

        uint Unk1;

        [RArray(FieldName = "StringCount"), CString()]
        public string[] Strings;
    }
#pragma warning restore 649, 169
}
