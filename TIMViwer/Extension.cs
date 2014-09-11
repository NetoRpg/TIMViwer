using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace DAT_Unpacker
{
    static class Extension
    {


        public static int extractInt32(this byte[] bytes, int index = 0)
        {
            return (bytes[index + 3] << 24) + (bytes[index + 2] << 16) + (bytes[index + 1] << 8) + bytes[index + 0];
        }


        public static byte[] extractPiece(this FileStream ms, int offset, int length, int changeOffset = -1)
        {
            if (changeOffset > -1) ms.Position = changeOffset;

            byte[] data = new byte[length];
            ms.Read(data, 0, length);

            return data;
        }

        public static byte[] extractPiece(this MemoryStream ms, int offset, int length, int changeOffset = -1)
        {
            if (changeOffset > -1) ms.Position = changeOffset;

            byte[] data = new byte[length];
            ms.Read(data, 0, length);

            return data;
        }


        public static void Save(this byte[] data, string path, int offset = -1, int length = -1)
        {
            int _offset = (offset > -1) ? offset : 0;
            int _length = (length > -1) ? length : data.Length;

            using (FileStream fs = File.Create(path))
            {
                fs.Write(data, _offset, _length);
            }
        }


        public static byte[] int32ToByteArray(this int value)
        {
            byte[] result = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return result;
        }


        public static Int16 extractInt16(this byte[] bytes, int index = 0)
        {
            return (short)((bytes[index + 1] << 8) + bytes[index + 0]);
        }


        public static byte[] int16ToByteArray(this short value)
        {
            byte[] result = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                result[i] = (byte)((value >> i * 8) & 0xFF);
            }
            return result;
        }

        public static byte[] copyFrom(this byte[] self, byte[] data, int copyOffset, int length, int destinyOffset = 0) 
        {

            for (int i = copyOffset; i < length; i++)
            {
                self[destinyOffset + (i - copyOffset)] = data[i];
            }

            return self;
        }


        public static Bitmap ProportionallyResizeBitmap(this Bitmap sourceBitmap, int maxWidth, int maxHeight)
        {

            Size size = sourceBitmap.Size;

            int height = (int)((double)(maxWidth) / ((double)sourceBitmap.Width / (double)sourceBitmap.Height));
            size = new Size((maxWidth), height);

            if (size.Width > maxWidth)
            {
                height = (int)((double)(maxWidth) / ((double)sourceBitmap.Width / (double)sourceBitmap.Height));
                size = new Size((maxWidth), height);
            }

            if (size.Height > maxHeight)
            {
                int width = (int)((double)(maxHeight) * ((double)sourceBitmap.Width / (double)sourceBitmap.Height));
                size = new Size(width, (maxHeight));
            }


            Bitmap resizedBitmap = new Bitmap((int)size.Width, (int)size.Height);
            using (Graphics g = Graphics.FromImage((System.Drawing.Image)resizedBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBitmap, 0, 0, (int)size.Width, (int)size.Height);
            }
            return resizedBitmap;
        }  

    }
}
