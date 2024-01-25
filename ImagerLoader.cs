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
        public Pixel GetPixel(short x, short y)
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
