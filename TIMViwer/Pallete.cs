using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DAT_Unpacker
{
    public partial class Pallete : UserControl
    {
        public Pallete()
        {
            InitializeComponent();

        }


        protected override void OnPaint(PaintEventArgs e)
        {



        }

        private void Pallete_Load(object sender, EventArgs e)
        {

            int i = 0;
            for (int y = 0; y < 16; y++)
            {

                for (int x = 0; x < 16; x++, i++)
                {



                    this.Controls.Add(new ColorSquare(Color.FromArgb(x * 16, x * 16, (x + y) * 8), i));
                    this.Controls[i].Location = new Point(x * 11, y * 11);
                    this.Controls[i].BackColorChanged += new EventHandler(Teste_Event); 

                }
            }
        }

        private void Teste_Event(object sender, EventArgs e)
        {
            
        
        }

    }
}
