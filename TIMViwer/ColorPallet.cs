using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DAT_Unpacker
{
    public partial class ColorPallet : Form
    {
        public ColorPallet()
        {
            InitializeComponent();
        }

        private void ColorPallet_Load(object sender, EventArgs e)
        {

            Color[] c = ((Main)this.Owner).GetPallete();

            int i = 0;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++, i++)
                {
                    this.Controls.Add(new ColorSquare(c[i], i));
                    this.Controls[i].Location = new Point(x * 11, y * 11);
                    if (i == c.Length) break;
                }
            }
        }

        public void Teste()
        {
            MessageBox.Show("A");
        }
    }
}
