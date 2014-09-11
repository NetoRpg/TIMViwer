using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace DAT_Unpacker
{
    class TIM
    {

        public int _bpp;
        public int Width;
        public int Heigth;
        private int rawWidth;

        public short clutNumber = 0;
        public int clutIndex = 0;
        public int paletteIndex = 0;
        public int maxPalleteIndex = 0;
        public bool Transparency = false;

        private short colorNumber = 0;
        
        private byte[][] clut;// = new byte[10,512];

        private long imageDataPosition;

        private MemoryStream data;

        private Dictionary<int, int> palleteSize = new Dictionary<int, int>() { { 4, 32 }, { 8, 512 }, {16, 0}, {24, 0} };
        private Dictionary<int, int> bpptoInt = new Dictionary<int, int>() { { 8, 4 }, { 9, 8 }, {2, 16}, {3, 24} };

        public int bpp
        {
            set
            {  
                _bpp = value;

                if(value < 16) this.maxPalleteIndex = (int)Math.Floor(((double)(colorNumber) / (double)palleteSize[value]));
                this.Width = GetRealWidth();
            }
            get { return _bpp; }
        }


        public TIM(byte[] data)
        {
            this.data = new MemoryStream(data);
            readHeader();
        }

        private void readHeader()
        {
            int tmp = this.data.extractPiece(0, 1, 0x4)[0];
            this.data.Position = 0x10;

            if (tmp != 2 && tmp != 3)
            {
                this.colorNumber = this.data.extractPiece(0, 2, 0x10).extractInt16();
                this.clutNumber = this.data.extractPiece(0, 2).extractInt16();
                clut = new byte[clutNumber][];
                for (int i = 0; i < clutNumber; i++)
                {
                    clut[i] = new byte[512];
                    clut[i].copyFrom(this.data.extractPiece(0, colorNumber * 2), 0, colorNumber * 2);
                }

                //this.clut.copyFrom(this.data.extractPiece(0, colorNumber * clutNumber * 2), 0, colorNumber * clutNumber * 2);

                this.data.Position += 8;
            }

            this.rawWidth = this.data.extractPiece(0, 2).extractInt16();
            this.Heigth = this.data.extractPiece(0, 2).extractInt16();

            this.bpp = bpptoInt.ContainsKey(tmp) ? bpptoInt[tmp] : 4;

            this.imageDataPosition = this.data.Position;
        }


        public unsafe Bitmap CreateUnsafeBitmap()
        {
            this.data.Position = this.imageDataPosition;

            Bitmap bmp = new Bitmap(this.Width, this.Heigth, PixelFormat.Format32bppArgb);

            BitmapData _bmd = bmp.LockBits(new Rectangle(0, 0, this.Width, this.Heigth), ImageLockMode.ReadWrite, bmp.PixelFormat);
            
            int _pixelSize = 4;
            byte* _current = (byte*)(void*)_bmd.Scan0;

            for (int y = 0; y < this.Heigth; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if ((x * _pixelSize) % _pixelSize == 0 || x == 0)
                    {
                        byte t = (byte)this.data.ReadByte();
                        if (_bpp == 4)
                        {
                            Color color = CLUTColor(t & 0x0F);
                            SetPixel(_current, color);

                            _current += _pixelSize;

                            color = CLUTColor((t & 0xF0) >> 4);
                            SetPixel(_current, color);

                            x++;
                        }
                        else if (_bpp == 8)
                        {

                            Color color = CLUTColor(t);
                            SetPixel(_current, color);

                        }
                        else if (_bpp == 16)
                        {
                            ushort color = (ushort)(t + (data.ReadByte() << 8));

                            SetPixel(_current, Get16bitColor(color));

                        }
                        _current += _pixelSize;
                    }
                    
                }
            }
            bmp.UnlockBits(_bmd);
 
            return bmp;
        }

        private unsafe void SetPixel(byte* pixel, Color color)
        {
            pixel[2] = color.R;
            pixel[1] = color.G;
            pixel[0] = color.B;
            pixel[3] = color.A;
        }

        public Bitmap CreateBitmap()
        {
            this.data.Position = this.imageDataPosition;


            Bitmap bmp = new Bitmap(this.Width, this.Heigth);

            

            for (int y = 0; y < this.Heigth; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    byte t = (byte)this.data.ReadByte();
                    if (_bpp == 4)
                    {
                        bmp.SetPixel(x, y, CLUTColor((t & 0x0F)));

                        bmp.SetPixel(x + 1, y, CLUTColor(((t & 0xF0) >> 4)));
                        x += 1;
                    }
                    else if (_bpp == 8)
                    {

                        bmp.SetPixel(x, y, CLUTColor(t));

                    }
                }
            }

            return bmp;
        }

        private Color CLUTColor(int index)
        {
            if (clut == null) GenerateRandonCLUT();

            index *= 2;
            index += (int)paletteIndex * palleteSize[_bpp];
            //index = Math.Abs(index);

            //if (index > 512) index -= 512;

            ushort color = (ushort)(clut[clutIndex][index] + (clut[clutIndex][index + 1] << 8));

            return Get16bitColor(color);
        }

        private Color Get16bitColor(ushort color)
        {
            int R = (color & 0x1F) * 8;
            int G = ((color & 0x3E0) >> 5) * 8;
            int B = ((color & 0x7C00) >> 10) * 8;
            int STP = ((color & 0x8000) >> 15);
            int A = (R == 0 && G == 0 && B == 0 && STP == 0 && Transparency) ? 0 : 255;

            return Color.FromArgb(A, R, G, B);
        }

        private int GetRealWidth()
        {
            switch (_bpp)
            {
                case 4:
                    return rawWidth * 4;
                case 8:
                    return rawWidth * 2;
                case 16:
                    return rawWidth;
                case 24:
                    return rawWidth / 2;
                default:
                    return 1;
            }
        }

        private void GenerateRandonCLUT()
        {
            clut = new byte[1][];
            clut[0] = new byte[512];

            Random random = new Random();

            for (int i = 0; i < 512; i++)
            {
                clut[0][i] = (byte)random.Next(0, 255);
            }

        }


        public byte[] GetActualPallete()
        { 
            byte[] pallete = new byte[palleteSize[_bpp]];

            for (int i = 0; i < palleteSize[_bpp]; i++)
            {
                pallete[i] = clut[clutIndex][i];
            }

            return pallete;
        }

        public Color[] GetActualPalleteInCollors()
        {
            Color[] c = new Color[palleteSize[_bpp] / 2];

            for (int i = 0; i < c.Length; i++)
            {
                c[i] = CLUTColor(i);  
            }
            return c;
        }
    }
}
