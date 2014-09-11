using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DAT_Unpacker
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        bool codeChange = false;


        string _FILEPATH;
        ImageList il = new ImageList();
        List<Indexes> _INDEX = new List<Indexes>();
        Dictionary<int, int> bitpp = new Dictionary<int, int>() {{4, 0}, {8,1},{16,2}};
        Dictionary<int, int> bitpp2 = new Dictionary<int, int>() { { 0, 4 }, { 1, 8 }, { 2, 16 } };
        TIM tim;

        private ColorPallet cp;

        private void Unpack()
        {
            if (_FILEPATH == null) return;

            using (FileStream fs = new FileStream(_FILEPATH, FileMode.Open))
            { 
                int fileNum = fs.extractPiece(0, 4, 4).extractInt32() + 1;
                int headerSize = fs.extractPiece(0, 4).extractInt32() * 0x800;
                int[] offsets = new int[fileNum];
                string fileName = Path.GetFileNameWithoutExtension(_FILEPATH);
                byte[] data;

                MemoryStream header = new MemoryStream(fs.extractPiece(0, headerSize, 0));
                header.Position = 8;

                for (int i = 0; i < fileNum; i++)
                {
                    offsets[i] = header.extractPiece(0, 4).extractInt32() * 0x800;
                    _INDEX.Add(new Indexes(offsets[i]));
                }

                string basePath = Path.Combine(Path.GetDirectoryName(_FILEPATH), fileName);
                for (int i = 0; i < fileNum - 1; i++)
                {
                    
                    List<TreeNode> tn = new List<TreeNode>();

                    data = fs.extractPiece(0, offsets[i + 1] - offsets[i]);
                    if (data[3] == 0x01 && data[2] < 0x10)
                    {

                        int timNum = data.extractInt32(4);
                        int[] timOffsets = new int[timNum + 1];

                        for (int x = 0; x < timNum; x++)
                        {
                            timOffsets[x] = (data.extractInt32((4 * x) + 8) * 4) + 4;
                            _INDEX[i].subOffsets.Add(timOffsets[x]);
                        }
                        _INDEX[i].subOffsets.Add(data.Length);
                        timOffsets[timOffsets.Length - 1] = data.Length;
                        

                        for (int x = 0; x < timOffsets.Length - 1; x++)
                        {
                            string timPath = Path.Combine(basePath, String.Format("{0}_{1}", fileName, i));

                            string ext = (data[timOffsets[x]] == 0x10) ? "TIM" : "BIN";
                                TreeNode tree = new TreeNode(String.Format("{0}_{1}_{2}.{3}", fileName, i, x, ext));
                                if (ext == "TIM")
                                {
                                    tree.ImageIndex = tree.SelectedImageIndex = 1;
                                    tree.Tag = x;
                                    tn.Add(tree);
                                }
                        }
                    }
                    if (tn != null && tn.Count > 0)
                    {
                        TreeNode tree = new TreeNode(String.Format("{0}_{1}.BIN", fileName, i), tn.ToArray());
                        tree.Tag = i;
                        treeView1.Nodes.Add(tree);

                    }             
                }

            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DAT Files|*.DAT";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = _FILEPATH = ofd.FileName;

                    Unpack();
                }
            }
        }


        void CreateTIM()
        {
            if (_INDEX.Count == 0) return;

            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Parent != null)
            {
                if (treeView1.SelectedNode.SelectedImageIndex == 0) return;

                using (FileStream fs = new FileStream(_FILEPATH, FileMode.Open))
                {
                    if (_INDEX.Count == 0) return;

                    int parentIndex = (int)treeView1.SelectedNode.Parent.Tag;
                    int childrenIndex = (int)treeView1.SelectedNode.Tag;

                    int size = _INDEX[parentIndex].subOffsets[childrenIndex + 1] - _INDEX[parentIndex].subOffsets[childrenIndex];
                    int offset = _INDEX[parentIndex].offset + _INDEX[parentIndex].subOffsets[childrenIndex];

                    tim = new TIM(fs.extractPiece(0, size, offset));

                    changeBPP(bitpp[tim.bpp]);

                }
            }
            ShowTIM();
        }

        void ShowTIM()
        {
            if (tim == null) return;

            tim.Transparency = checkBox2.Checked;
            tim.paletteIndex = (int)nudPallete.Value;
            tim.bpp = bitpp2[comboBox1.SelectedIndex];
            tim.clutIndex = (int)nudClut.Value;
            nudPallete.Maximum = tim.maxPalleteIndex;
            nudClut.Maximum = tim.clutNumber > 0 ? tim.clutNumber - 1 : 0;

            if(pictureBox1.IsDisposed) pictureBox1.Image.Dispose();
            if (checkBox1.Checked)
                pictureBox1.Image = tim.CreateUnsafeBitmap().ProportionallyResizeBitmap(pictureBox1.Width, pictureBox1.Height);
            else
                pictureBox1.Image = tim.CreateUnsafeBitmap();//tim.CreateBitmap();
        
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ShowTIM();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (codeChange) return;
            ShowTIM();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ResetNUDValue();
            CreateTIM();
        }

        void ResetNUDValue()
        {
            codeChange = true;
            nudPallete.Value = 0;
            nudClut.Value = 0;
            codeChange = false;
        }

        void changeBPP(int index)
        {
            codeChange = true;
            comboBox1.SelectedIndex = index;
            codeChange = false;
        }

        private void DUP_Load(object sender, EventArgs e)
        {
            treeView1.ImageList = imageList1;
            treeView1.SelectedImageIndex = -1;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            ShowTIM();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (codeChange) return;
            ResetNUDValue();
            ShowTIM();
        }

        private void nudClut_ValueChanged(object sender, EventArgs e)
        {
            if (codeChange) return;
            ShowTIM();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cp = new ColorPallet();
            cp.StartPosition = this.StartPosition;
            cp.Show(this);
        }

        public Color[] GetPallete()
        {
            return tim.GetActualPalleteInCollors();
        }
    }
}
