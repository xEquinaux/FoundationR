using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using static FoundationR.REW;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace FoundationR
{
    public struct BitmapFile
    {
        public string Name;
        public Bitmap Value;
        static byte[] GetDIBHeader(REW image)
        {
            byte[] array = new byte[0];
            switch (image.Header)
            { 
                default:
                case BitmapHeader.BITMAPINFOHEADER:
                    array = array.Concat(BITMAPINFOHEADER.CreateDIBHeader(image, out _)).ToArray();
                    break;
                case BitmapHeader.BITMAPV3INFOHEADER:
                    array = array.Concat(BITMAPV3INFOHEADER.CreateDIBHeader(image, out _)).ToArray();
                    break;
            }
            return array;
        }
        static byte[] GetDataBuffer(REW image)
        {
            return image.GetPixels();
        }
        static byte[] BmpHeader(REW image, int arrayOffset)
        {
            byte[] fileSize = BitConverter.GetBytes(image.RealLength);
            byte[] offset = BitConverter.GetBytes(arrayOffset);
                            //  B     M   , Total file size                                   , N/a       , Index offset of where pixel array is
            return new byte[] { 0x42, 0x4D, fileSize[0], fileSize[1], fileSize[2], fileSize[3], 0, 0, 0, 0, offset[0], offset[1], offset[2], offset[3] };
        }
        public static byte[] Create(REW image)
        {
            int headerSize = 14;
            byte[] dib = GetDIBHeader(image);
            byte[] header = BmpHeader(image, dib.Length + headerSize);
            var result = header.Concat(dib);
            byte[] data = GetDataBuffer(image);
            return result.Concat(data).ToArray();
        }
    }
    public class RewBatch
    {
        [DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)]
        static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BitmapInfo pbmi, uint pila, out IntPtr ppbBits, IntPtr hSection, uint dwOffset);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private int stride => width * ((PixelFormats.Bgr24.BitsPerPixel + 7) / 8);
        private int width, height;
        private int oldWidth, oldHeight;
        private Int32Rect backBufferRect => new Int32Rect(0, 0, width, height);
        private static REW BackBuffer;
        private static Bitmap _backBuffer;
        public RewBatch(int width, int height)
        {
            Initialize(width, height);
        }
        void Initialize(int width, int height)
        {
            this.width = width;
            this.height = height;
            _backBuffer = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
        public bool Resize(int width, int height)
        {
            if (oldWidth != width || oldHeight != height)
            {
                this.width = width;
                this.oldWidth = width;
                this.height = height;
                this.oldHeight = height;
                _backBuffer = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                return true;
            }
            return false;
        }
        public void Begin()
        {
            BackBuffer = REW.CreateEmpty(width, height, PixelFormats.Bgr32);
        }
        public void Draw(REW image, int x, int y)
        {
            BackBuffer.Composite(image, x, y);
        }
        public void Render(Graphics g)
        {
            CreateBitmapFromByteArray(BackBuffer.GetPixels(), width, height);
            g.DrawImage(_backBuffer, 0, 0, width, height);
        }
        public void End()
        {
            BackBuffer = null;
        }
        Bitmap CreateBitmapFromByteArray(byte[] pixels, int width, int height)
        {
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = _backBuffer.LockBits(rect, ImageLockMode.WriteOnly, _backBuffer.PixelFormat);
            try
            {
                Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            }
            finally
            {
                _backBuffer.UnlockBits(bmpData);
            }
            return _backBuffer;
        }
    }
    public static class ImageLoader
    {
        static int count = 0;
        static bool skip = false;
        public static string WorkingDir;
        public static void Initialize(string path)
        {
            WorkingDir = path;
        }
        public static REW BitmapIngest(BitmapFile bitmap, PixelFormat format, bool skipConvert = true)
        {
            REW instance = REW.CreateEmpty(bitmap.Value.Width, bitmap.Value.Height, format);
            if (!skipConvert)
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
            }
            else
            {
                instance.Extract(bitmap.Value);
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
        public static readonly int HeaderOffset = 10;
        public short Width { get; private set; } 
        public short Height { get; private set; }
        public short BitsPerPixel { get; private set; }
        public int Count => (data.Length - HeaderOffset) / NumChannels;
        public int RealLength => data.Length - HeaderOffset;
        public int NumChannels => BitsPerPixel >= 32 ? 4 : 3;
        public BitmapHeader Header => NumChannels == 4 ? BitmapHeader.BITMAPV3INFOHEADER : BitmapHeader.BITMAPINFOHEADER;
        public static REW Create(int width, int height, Color color, PixelFormat format)
        {
            return new REW(width, height, color, format);
        }
        public static REW CreateEmpty(int width, int height, PixelFormat format)
        {
            return new REW(width, height, default, format);
        }
        private REW() { }
        private REW(int width, int height, Color color, PixelFormat format)
        {
            this.i = width;
            this.BitsPerPixel = (short)format.BitsPerPixel;
            this.Width = (short)width;
            this.Height = (short)height;
            this.data = new byte[Width * Height * NumChannels + HeaderOffset];
            WriteHeader(this);
            if (color != default)
            { 
                WriteDataChunk(this, color);
            }
        }
        public byte[] GetPixels()
        {
            if (NumChannels < 4)
            {
                int padding = (4 - (Width * (BitsPerPixel / 8)) % 4) % 4;
                if (padding > 0)
                { 
                    var list = data.Skip(HeaderOffset).ToList();
                    int num = Width;
                    { 
                        for (int i = 0; i < Height; i++)
                        {
                            list.InsertRange(num, new byte[padding]);
                            num += Width + padding;
                        }
                    }
                    return list.ToArray();
                }
            }
            return data.Skip(REW.HeaderOffset).ToArray();
        }
        static void WriteHeader(REW rew)
        {
            rew.data.AddHeader(new Point16(rew.Width, rew.Height), rew.Width * rew.Height * rew.NumChannels, rew.BitsPerPixel);
        }
        static void WriteDataChunk(REW rew, Color color)
        {
            int num = 0;
            for (int j = 0; j < rew.Height; j++)
            {
                for (int i = 0; i < rew.Width; i++)
                {
                    Pixel pixel = default;
                    if (rew.NumChannels == 4)
                    {
                        pixel = new Pixel(color.A, color.R, color.G, color.B); 
                    }
                    else 
                    { 
                        pixel = new Pixel(color.R, color.G, color.B);
                    }
                    rew.data.AppendPixel(num * rew.NumChannels + REW.HeaderOffset, pixel);
                    pixel = null;
                    num++;
                }
            }
        }
        public void Extract(Bitmap bitmap)
        {
            int num = 0;
            this.i = bitmap.Width;
            this.Width = (short)bitmap.Width;
            this.Height = (short)bitmap.Height;
            this.data = new byte[bitmap.Width * bitmap.Height * NumChannels + HeaderOffset];
            this.data.AddHeader(new Point16(this.Width, this.Height), bitmap.Width * bitmap.Height * NumChannels + HeaderOffset, BitsPerPixel);
            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    Color c = bitmap.GetPixel(i, j);
                    Pixel pixel = default;
                    if (NumChannels == 4)
                    {
                        pixel = new Pixel(c.A, c.R, c.G, c.B);
                    }
                    else
                    {
                        pixel = new Pixel(c.R, c.G, c.B);
                    }
                    data.AppendPixel(num * NumChannels + HeaderOffset, pixel);
                    pixel = null;
                    num++;
                }
            }
        }
        public static REW Extract(Bitmap bitmap, short bitsPerPixel)
        {
            PixelFormat format = default;
            if (bitsPerPixel == 32)
            {
                format = PixelFormats.Bgr32;
            }
            else format = PixelFormats.Bgr24;
            REW result = REW.CreateEmpty(bitmap.Width, bitmap.Height, format);
            int num = 0;
            result.i = bitmap.Width;
            result.Width = (short)bitmap.Width;
            result.Height = (short)bitmap.Height;
            result.data = new byte[bitmap.Width * bitmap.Height * result.NumChannels + HeaderOffset];
            result.data.AddHeader(new Point16(result.Width, result.Height), bitmap.Width * bitmap.Height * result.NumChannels + HeaderOffset, result.BitsPerPixel);
            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    Color c = bitmap.GetPixel(i, j);
                    Pixel pixel = default;
                    if (result.NumChannels == 4)
                    {
                        pixel = new Pixel(c.A, c.R, c.G, c.B);
                    }
                    else
                    {
                        pixel = new Pixel(c.R, c.G, c.B);
                    }
                    result.data.AppendPixel(num * result.NumChannels + HeaderOffset, pixel);
                    pixel = null;
                    num++;
                }
            }
            bitmap.Dispose();
            return result;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(new Point16(Width, Height));
            w.Write(data.Length);
            w.Write(BitsPerPixel);
            w.Write(data, 0, data.Length);
        }
        public void ReadData(BinaryReader br)
        {
            Point16 size = br.ReadPoint16();
            int len = br.ReadInt32();
            BitsPerPixel = br.ReadInt16();
            data = new byte[(len - HeaderOffset) * NumChannels];
            data.AddHeader(size, len, BitsPerPixel);
            this.i = Width = size.X;
            Height = size.Y;
            for (int i = HeaderOffset; i < data.Length; i += NumChannels)
            {
                Pixel pixel = br.ReadPixel();
                pixel.hasAlpha = NumChannels == 4;
                data.AppendPixel(i, pixel);
                pixel = null;
            }
        }
        public Pixel GetPixel(int x, int y)
        {
            int i = this.Width;
            int whoAmI = y * i + x;
            if (NumChannels == 4)
            {
                return new Pixel(
                    data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset)],
                    data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset + 1)],
                    data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset + 2)],
                    data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset + 3)]
                );
            }
            else
            {
                return new Pixel(
                    data[Math.Min(data.Length - 1, whoAmI * 3 + HeaderOffset)],
                    data[Math.Min(data.Length - 1, whoAmI * 3 + HeaderOffset + 1)],
                    data[Math.Min(data.Length - 1, whoAmI * 3 + HeaderOffset + 2)]
                );
            }
        }
        public void SetPixel(int x, int y, Color color)
        {
            int i = this.Width;
            int whoAmI = y * i + x;
            if (NumChannels == 4)
            { 
                data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset)]     = color.A;
                data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset + 1)] = color.R;
                data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset + 2)] = color.G;
                data[Math.Min(data.Length - 1, whoAmI * 4 + HeaderOffset + 3)] = color.B;
            }
            else
            {
                data[Math.Min(data.Length - 1, whoAmI * 3 + HeaderOffset)]     = color.R;
                data[Math.Min(data.Length - 1, whoAmI * 3 + HeaderOffset + 1)] = color.G;
                data[Math.Min(data.Length - 1, whoAmI * 3 + HeaderOffset + 2)] = color.B;
            }
        }
    }
    public class Pixel
    {
        public bool hasAlpha;
        public Pixel() { }
        public Pixel(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
            this.hasAlpha = true;
        }
        public Pixel(byte R, byte G, byte B)
        {
            //  Flipped; requires drawing 24bppBGR
            this.R = R;
            this.G = G;
            this.B = B;
            this.hasAlpha = false;
        }
        public byte A = 255, R, G, B;
        public byte[] Buffer => hasAlpha ? new byte[] { R, G, B, A } : new byte[] { R, G, B };
        public Color color   => Color.FromArgb(A, R, G, B);
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
        public static void AddHeader(this byte[] buffer, Point16 size, int dataLength, int bpp)
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
            byte[] _bpp = BitConverter.GetBytes(bpp);
            buffer[8] = _bpp[0];
            buffer[9] = _bpp[1];
        }

        public static void Write(this BinaryWriter w, Pixel pixel)
        {
            byte[] buffer = pixel.Buffer;
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
            if (i.hasAlpha)
            { 
                array[index]     = i.R;
                array[index + 1] = i.G;
                array[index + 2] = i.B;
                array[index + 3] = i.A;
            }
            else
            {
                array[index]     = i.R;
                array[index + 1] = i.G;
                array[index + 2] = i.B;
            }
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
        public static void Composite(this REW one, REW tex, int x, int y)
        {
            short width = tex.Width;
            short height = tex.Height;
            for (int n = 0; n < height; n++)
            {
                for (int m = 0; m < width; m++)
                {
                    Pixel _one = one.GetPixel(m + x, n + y);
                    Pixel _two = tex.GetPixel(m, n);
                    if (_two.A < 255)
                    {
                        PreMultiply(_one);
                        PreMultiply(_two);
                        one.SetPixel(m + x, n + y, _two.color.Blend(_one.color, 0.5d));
                    }
                    else one.SetPixel(m + x, n + y, _two.color);
                }
            }
            Pixel PreMultiply(Pixel pixel)
            {
                byte r = pixel.R;
                byte g = pixel.G;
                byte b = pixel.B;
                byte a = pixel.A;
                pixel.R = (byte)((r * a) / 255);
                pixel.G = (byte)((g * a) / 255);
                pixel.B = (byte)((b * a) / 255);
                return pixel;
            }
        }
    }
    public class Composite
    {
        public Composite(string layerOneFile, string layerTwoFile, string outputFile)
        {
            // load the files
            var layerOne = new BitmapImage(new Uri(layerOneFile, UriKind.Absolute));
            var layerTwo = new BitmapImage(new Uri(layerTwoFile, UriKind.Absolute));

            // create the destination based upon layer one
            var composite = new WriteableBitmap(layerOne);

            // premultiply the alpha values for layer one
            composite = PremultiplyAlpha(composite);

            // premultiply the alpha values for layer two
            var _layerTwo = PremultiplyAlpha(new WriteableBitmap(layerTwo));

            // copy the pixels from layer two on to the destination
            int[] pixels = new int[(int)layerTwo.Width * (int)layerTwo.Height];
            int stride = (int)(4 * layerTwo.Width);
            _layerTwo.CopyPixels(pixels, stride, 0);
            composite.WritePixels(new Int32Rect(0, 0, (int)layerTwo.Width, (int)layerTwo.Height), pixels, stride, 0);

            // encode the bitmap to the output file
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(composite));
            using (var stream = new FileStream(outputFile, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        // premultiply the alpha values for a bitmap
        private WriteableBitmap PremultiplyAlpha(WriteableBitmap bitmap)
        {
            int[] pixels = new int[(int)bitmap.Width * (int)bitmap.Height];
            int stride = (int)(4 * bitmap.Width);
            bitmap.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i++)
            {
                byte a = (byte)(pixels[i] >> 24);
                byte r = (byte)(pixels[i] >> 16);
                byte g = (byte)(pixels[i] >> 8);
                byte b = (byte)(pixels[i] >> 0);

                r = (byte)((r * a) / 255);
                g = (byte)((g * a) / 255);
                b = (byte)((b * a) / 255);

                pixels[i] = (a << 24) | (r << 16) | (g << 8) | (b << 0);
            }

            var result = new WriteableBitmap(bitmap);
            result.WritePixels(new Int32Rect(0, 0, (int)bitmap.Width, (int)bitmap.Height), pixels, stride, 0);

            return result;
        }
    }
    public static class ColorExtensions
    {
        public static Color Blend(this Color color, Color backColor, double amount)
        {
            byte a = (byte)Math.Min(color.A + backColor.A, 255); // unknown
            byte r = (byte)(color.R * amount + backColor.R * (1 - amount));
            byte g = (byte)(color.G * amount + backColor.G * (1 - amount));
            byte b = (byte)(color.B * amount + backColor.B * (1 - amount));
            return Color.FromArgb(a, r, g, b);
        }
    }
}
