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
    public partial class ColorSquare : UserControl
    {
        public ColorSquare()
        {
            InitializeComponent();
        }

        private Color _color = SystemColors.Control;
        public int ID;

        public Color color
        {
            get { return _color; }
            set {
                _color = value;
                this.BackColor = value;
                //Invalidate();            
            }
        }

        public ColorSquare(Color color, int id)
        {
            InitializeComponent();

            //this.color = color;

            //this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.color = color;
            this.ID = id;


        }

        private void ColorSquare_Load(object sender, EventArgs e)
        {

        }

        protected override void OnClick(EventArgs e)
        {

        }


        private void ColorSquare_DoubleClick(object sender, EventArgs e)
        {

            ColorDialog cd = new ColorDialog();



            if (cd.ShowDialog() == DialogResult.OK)
            {
                color = cd.Color;
            }
        }
    }
}
