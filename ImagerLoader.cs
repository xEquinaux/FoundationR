using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FoundationR.REW;

namespace FoundationR
{
    public class ImagerLoader
    {
    }
    public partial class REW
    {
        byte[] data;
        public byte[] GetBuffer => data;
        public int Count => data.Length / 4;
        public void Extract(Bitmap bitmap)
        {
            int num = 0;
            this.data = new byte[bitmap.Width * bitmap.Height * 4];
            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    num++;
                    Color c = bitmap.GetPixel(i, j);
                    Pixel pixel = new Pixel()
                    {
                        A = c.A,
                        R = c.R,
                        G = c.G,
                        B = c.B
                    };
                    data.AppendPixel(num * 4, pixel);
                    pixel = null;
                }
            }
        }
        public void Write(BinaryWriter w)
        {
            w.Write(data, 0, data.Length);
        }
        public void ReadData(BinaryReader br)
        {
            long len = br.ReadInt64();
            data = new byte[len * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                Pixel pixel = br.ReadPixel();
                data.AppendPixel(i, br.ReadPixel());
                pixel = null;
            }
        }
        public Pixel GetPixel(short x, short y, bool _lexicon = true)
        {
            int m = data.GetLength(0) + 1;
            int whoAmI = 0;
            if (y == 0)
            {
                whoAmI = x;
            }
            else if (x == 0)
            {
                whoAmI = m * y;
            }
            else whoAmI = m * y + (x - y) + 1;
            return new Pixel()
            {
                A = data[whoAmI * 4],
                R = data[whoAmI * 4 + 1],
                G = data[whoAmI * 4 + 2],
                B = data[whoAmI * 4 + 3]
            };
        }
        public void SetPixel(short x, short y, Color color)
        {
            int m = data.GetLength(0) + 1;
            int whoAmI = 0;
            if (y == 0)
            {
                whoAmI = x;
            }
            else if (x == 0)
            {
                whoAmI = m * y;
            }
            else whoAmI = m * y + (x - y) + 1;
            data[whoAmI * 4]     = color.A;
            data[whoAmI * 4 + 1] = color.R;
            data[whoAmI * 4 + 2] = color.G;
            data[whoAmI * 4 + 3] = color.B;
        }
    }
    public class Pixel
    {
        public int whoAmI;
        public byte A, R, G, B;
        public byte[] Buffer()
        {
            return new byte[] { A, R, G, B };
        }
    }
    public static class Ext
    {
        public static void Write(this BinaryWriter w, Pixel pixel)
        {
            byte[] buffer = pixel.Buffer();
            w.Write(buffer, 0, 4);
        }
        public static Pixel ReadPixel(this BinaryReader r)
        {
            Pixel i = new Pixel();
            i.A = r.ReadByte();
            i.R = r.ReadByte();
            i.G = r.ReadByte();
            i.B = r.ReadByte();
            return i;
        }
        public static byte[] AppendPixel(this byte[] array, int index, Pixel i)
        {
            array[index] = i.A;
            array[index + 1] = i.R;
            array[index + 2] = i.G;
            array[index + 3] = i.B;
            return array;
        }
    }
}
