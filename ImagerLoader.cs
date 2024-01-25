using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using static FoundationR.REW;

namespace FoundationR
{
    public struct BitmapFile
    {
        public string Name;
        public Bitmap Value;
    }
    public static class ImagerLoader
    {
        static int count = 0;
        static bool skip = false;
        public static string WorkingDir;
        public static void Initialize(string path)
        {
            WorkingDir = path;
        }
        public static REW BitmapIngest(in BitmapFile bitmap, REW instance)
        {
            string file = Path.Combine(WorkingDir, bitmap.Name);
            BEGIN:
            if (File.Exists(file) && !skip)
            {
                if (count == 0 || count > 1)
                { 
                    var result = MessageBox.Show($"File:\n\n{file}\n\nAlready exists. Would you like to overwrite it?", "File Overwrite", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        handleFile(bitmap);
                        count++;
                    }
                    else if (result == DialogResult.No)
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Title = "Pick a save file";
                        dialog.DefaultExt = "rew";
                        dialog.CheckPathExists = true;
                        dialog.RestoreDirectory = true;
                        dialog.ShowDialog();
                        file = dialog.FileName;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return instance;
                    }
                }
                else if (count == 1)
                {
                    var result = MessageBox.Show("There are clearly more files to be processed. Would you like to skip this dialog and overwrite them all?", "Overwrite All", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        skip = true;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return instance;
                    }
                    count++;
                    goto BEGIN;
                }
            }
            else
            {
               handleFile(bitmap);
            }
            void handleFile(BitmapFile bitmap)
            {
                instance.Extract(bitmap.Value);
                using (FileStream fs = new FileStream(Path.Combine(WorkingDir, bitmap.Name), FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    instance.Write(bw);
                }
            }
            return instance;
        }
    }
    public partial class REW
    {
        byte[] data;
        int i;
        public readonly int HeaderOffSet = 8;
        public byte[] GetBuffer => data.Skip(HeaderOffSet).ToArray();
        public short Width { get; private set; } 
        public short Height { get; private set; }
        public int Count => (data.Length - HeaderOffSet) / 4;
        public void Extract(Bitmap bitmap)
        {
            int num = 0;
            this.i = bitmap.Width;
            this.Width = (short)bitmap.Width;
            this.Height = (short)bitmap.Height;
            this.data = new byte[bitmap.Width * bitmap.Height * 4 + HeaderOffSet];
            this.data.AddHeader(new Point16(this.Width, this.Height), bitmap.Width * bitmap.Height * 4 + HeaderOffSet);
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
                    data.AppendPixel(num * 4 + HeaderOffSet, pixel);
                    pixel = null;
                }
            }
        }
        public void Write(BinaryWriter w)
        {
            w.Write(new Point16(Width, Height));
            w.Write(data.Length);
            w.Write(data, 0, data.Length);
        }
        public void ReadData(BinaryReader br)
        {
            Point16 size = br.ReadPoint16();
            int len = br.ReadInt32();
            byte[] lenb = BitConverter.GetBytes(len);
            data = new byte[(len - HeaderOffSet) * 4];
            byte[] sizeb = size.Buffer();
            data[0] = sizeb[0];
            data[1] = sizeb[1];
            data[2] = sizeb[2];
            data[3] = sizeb[3];
            this.i = Width = size.X;
            Height = size.Y;
            data[4] = lenb[0];
            data[5] = lenb[1];
            data[6] = lenb[2];
            data[7] = lenb[3];
            for (int i = HeaderOffSet; i < data.Length; i += 4)
            {
                Pixel pixel = br.ReadPixel();
                data.AppendPixel(i, pixel);
                pixel = null;
            }
        }
        public Pixel GetPixel(short x, short y)
        {
            int i = this.i + 1;
            int whoAmI;
            if (y == 0)
            {
                whoAmI = x;
            }
            else if (x == 0)
            {
                whoAmI = i * y;
            }
            else whoAmI = i * y + (x - y) + 1;
            whoAmI += HeaderOffSet;
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
            int i = this.i + 1;
            int whoAmI;
            if (y == 0)
            {
                whoAmI = x;
            }
            else if (x == 0)
            {
                whoAmI = i * y;
            }
            else whoAmI = i * y + (x - y) + 1;
            whoAmI += HeaderOffSet;
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
    public struct Point16
    {
        public Point16(short x, short y)
        {
            X = x;
            Y = y;
        }
        public short X;
        public short Y;
        public byte[] Buffer()
        {
            var x = BitConverter.GetBytes(X);
            var y = BitConverter.GetBytes(Y);
            return new byte[] { x[0], x[1], y[0], y[1] };
        }
    }
    public static class Ext
    {
        public static void AddHeader(this byte[] buffer, Point16 size, int dataLength)
        {
            byte[] sizeb = size.Buffer();
            buffer[0] = sizeb[0];
            buffer[1] = sizeb[1];
            buffer[2] = sizeb[2];
            buffer[3] = sizeb[3];
            byte[] lenb = BitConverter.GetBytes(dataLength);
            buffer[4] = lenb[0];
            buffer[5] = lenb[1];
            buffer[6] = lenb[2];
            buffer[7] = lenb[3];
        }

        public static void Write(this BinaryWriter w, Pixel pixel)
        {
            byte[] buffer = pixel.Buffer();
            w.Write(buffer, 0, 4);
        }
        public static void Write(this BinaryWriter w, Point16 point)
        {
            byte[] buffer = point.Buffer();
            w.Write(buffer, 0, 4);
        }
        public static Point16 ReadPoint16(this BinaryReader r)
        {
            Point16 p = new Point16();
            byte x0 = r.ReadByte();
            byte x1 = r.ReadByte();
            byte y0 = r.ReadByte();
            byte y1 = r.ReadByte();
            p.X = BitConverter.ToInt16(new byte[] { x0, x1 }, 0);
            p.Y = BitConverter.ToInt16(new byte[] { y0, y1 }, 0);
            return p;
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
            array[index]     = i.A;
            array[index + 1] = i.R;
            array[index + 2] = i.G;
            array[index + 3] = i.B;
            return array;
        }
        public static byte[] AppendPoint16(this byte[] array, int index, Point16 i)
        {
            byte[] buffer = i.Buffer();
            array[index]     = buffer[0];   // x
            array[index + 1] = buffer[1];
            array[index + 2] = buffer[3];   // y
            array[index + 3] = buffer[4];
            return array;
        }
    }
}
