using EscudeEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EEGUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog() {
                Filter = "All Escude Packgets|*.bin"
            };

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            string OutDir = fd.FileName + "~\\";
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            var Entries = BinPackget.Open(fd.FileName);
            foreach (Entry Entry in Entries) {
                string OutPath = OutDir + Entry.FileName;

                if (!Directory.Exists(Path.GetDirectoryName(OutPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(OutPath));

                if (File.Exists(OutPath))
                    File.Delete(OutPath);

                File.WriteAllBytes(OutPath, BinPackget.Decompress(Entry.Content));
            }
            File.WriteAllLines(OutDir + "FileList.lst", (from x in Entries select x.FileName).ToArray());
            MessageBox.Show("Files Extracted");
        }

        string ld = string.Empty;
        private void repackToolStripMenuItem_Click(object sender, EventArgs e) {
#if DEBUG
            ld = @"C:\Users\Marcus\Documents\My Games\花嫁と魔王 ～王室のハーレムは下克上～\script.bin~";
#endif
            FolderBrowserDialog fbd = new FolderBrowserDialog() {
                SelectedPath = ld
            };
            if (fbd.ShowDialog() != DialogResult.OK)
                return;
            ld = fbd.SelectedPath;

            SaveFileDialog sfd = new SaveFileDialog() {
                Filter = "All Escude Packgets|*.bin"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            if (!ld.EndsWith("\\"))
                ld += '\\';

            if (!File.Exists(ld + "FileList.lst")) {
                MessageBox.Show("You need the FileList.lst to repack");
                return;
            }


            List<Entry> Entries = new List<Entry>();
            foreach (string File in File.ReadLines(ld + "FileList.lst")) {
                if (File.ToLower() == "filelist.lst")
                    continue;
                Stream Content = new StreamReader(ld + File).BaseStream;

                Entries.Add(new Entry() {
                    FileName = File,
                    Content = Content
                });
            }

            BinPackget.Save(Entries.ToArray(), sfd.FileName);
            MessageBox.Show("Packget Saved");
        }

        private void decompressFileToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog() {
                Filter = "All Files|*.*"
            };

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllBytes(fd.FileName + ".dec", BinPackget.Decompress(new MemoryStream(File.ReadAllBytes(fd.FileName))));
        }

        private void fakeCompressFileToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog() {
                Filter = "All Files|*.*"
            };

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllBytes(fd.FileName + ".comp", BinPackget.FakeCompress(File.ReadAllBytes(fd.FileName)));

        }

        BinScript Script;
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog() {
                Filter = "All Escude Scripts|*.bin"
            };

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            byte[] Script = File.ReadAllBytes(fd.FileName);
            this.Script = new BinScript(Script);
            string[] Strings = this.Script.Import();

            listBox1.Items.Clear();
            foreach (string String in Strings) {
                listBox1.Items.Add(String);
            }
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            } catch { }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog fd = new SaveFileDialog() {
                Filter = "All Escude Scripts|*.bin"
            };

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            byte[] Result = Script.Export((listBox1.Items.OfType<string>()).ToArray());
            File.WriteAllBytes(fd.FileName, Result);
            MessageBox.Show("File Saved.");
        }
    }
}
