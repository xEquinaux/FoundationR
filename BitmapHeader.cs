using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FoundationR
{
    //  DWORD           4 bytes uint
    //  LONG            4 bytes int
    //  WORD            2 bytes short
    //  CIEXYZTRIPLE    ? bytes object
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfo
    {
        public BitmapInfoHeader Header;
        public RGBQuad[] Colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfoHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ClrUsed;
        public uint ClrImportant;
        //V3
        public uint RedMask;
        public uint GreenMask;
        public uint BlueMask;
        public uint AlphaMask;
        public uint CSType;
        public CIEXYZTRIPLE ciexyzTriple;
        public uint GammaRed;
        public uint GammaGreen;
        public uint GammaBlue;
        public uint Intent;
        public uint ProfileData;
        public uint ProfileSize;
        public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQuad
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZTRIPLE
    {
        public CIEXYZ ciexyzRed;
        public CIEXYZ ciexyzGreen;
        public CIEXYZ ciexyzBlue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZ
    {
        public uint ciexyzX;
        public uint ciexyzY;
        public uint ciexyzZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
        }

        public static readonly uint HeaderOffset = 40;
        public static byte[] CreateDIBHeader(BITMAPINFOHEADER header)
        {
            byte[] array =
                //Size of DIB header
                BitConverter.GetBytes(HeaderOffset)
                //Image width
                .Concat(BitConverter.GetBytes(header.biWidth))
                //Image height
                .Concat(BitConverter.GetBytes(header.biHeight))
                //# of color planes being used
                .Concat(BitConverter.GetBytes(header.biPlanes))
                //Pixel format
                .Concat(BitConverter.GetBytes(header.biBitCount))
                //Compression, if raw, normally 0
                .Concat(BitConverter.GetBytes((uint)header.biCompression))
                //Size of the pixel array (including padding)
                .Concat(BitConverter.GetBytes(header.biSizeImage))
                //Horizontal resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biXPelsPerMeter))
                //Vertical resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biYPelsPerMeter))
                //# of colors in the color palette
                .Concat(BitConverter.GetBytes(header.biClrUsed))
                //# of important colors used (0 means all)
                .Concat(BitConverter.GetBytes(header.biClrImportant))
                .ToArray();
            return array;
        }
        public static byte[] CreateDIBHeader(REW image, out BITMAPINFOHEADER header)
        {
            header = new BITMAPINFOHEADER();
            byte[] array =
                //Size of DIB header
                BitConverter.GetBytes(HeaderOffset)
                //Image width
                .Concat(BitConverter.GetBytes(header.biWidth = (int)image.Width))
                //Image height
                .Concat(BitConverter.GetBytes(header.biHeight = (int)image.Height))
                //# of color planes being used
                .Concat(BitConverter.GetBytes(header.biPlanes = (ushort)0))
                //Pixel format
                .Concat(BitConverter.GetBytes(header.biBitCount = (ushort)image.BitsPerPixel))
                //Compression, if raw, normally 0
                .Concat(BitConverter.GetBytes(header.biCompression = (uint)BitmapCompressionMode.BI_RGB))
                //Size of the pixel array (including padding)
                .Concat(BitConverter.GetBytes(header.biSizeImage = (uint)image.RealLength))
                //Horizontal resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biXPelsPerMeter = 0))
                //Vertical resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biYPelsPerMeter = 0))
                //# of colors in the color palette
                .Concat(BitConverter.GetBytes(header.biClrUsed = 0))
                //# of important colors used (0 means all)
                .Concat(BitConverter.GetBytes(header.biClrImportant = 0))
                .ToArray();
            return array;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPV3INFOHEADER    //BITMAPV3INFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
        public uint biRedMask;
        public uint biGreenMask;
        public uint biBlueMask;
        public uint biAlphaMask;
        public uint biCSType;
        public CIEXYZTRIPLE ciexyzTriple;
        public uint biGammaRed;
        public uint biGammaGreen;
        public uint biGammaBlue;
        public uint biIntent;
        public uint biProfileData;
        public uint biProfileSize;
        public uint biReserved;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
        }
        public static byte[] CreateDIBHeader(BITMAPV3INFOHEADER header)
        {
            int num = 0;
            uint size = (uint)Marshal.SizeOf(header);
            byte[] array = new byte[size];
            Array.Copy(BitConverter.GetBytes(size), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biWidth), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biHeight), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biPlanes), 0, array, num, num += 2);
            Array.Copy(BitConverter.GetBytes(header.biBitCount), 0, array, num, num += 2);
            Array.Copy(BitConverter.GetBytes(header.biCompression), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biSizeImage), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biXPelsPerMeter), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biYPelsPerMeter), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biClrUsed), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biClrImportant), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biRedMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGreenMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biBlueMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(/*ciexyzTriple*/36), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaRed), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaGreen), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaBlue), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biIntent), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileData), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileSize), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biReserved), 0, array, num, num += 4);
            return array;
        }
        public static byte[] CreateDIBHeader(REW rew, out BITMAPV3INFOHEADER header)
        {
            int num = 0;
            header = new BITMAPV3INFOHEADER();
            uint size = (uint)Marshal.SizeOf(header);
            byte[] array = new byte[size];
            Array.Copy(BitConverter.GetBytes(size), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biWidth = (int)rew.Width), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biHeight = (int)rew.Height), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biPlanes = 1), 0, array, num, num += 2);
            Array.Copy(BitConverter.GetBytes(header.biBitCount = 32), 0, array, num, num += 2);
            Array.Copy(BitConverter.GetBytes(header.biCompression = (uint)BitmapCompressionMode.BI_BITFIELDS), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biSizeImage = (uint)rew.RealLength), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biXPelsPerMeter = 96), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biYPelsPerMeter = 96), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biClrUsed = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biClrImportant = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biRedMask = 0x00FF0000), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGreenMask = 0x0000FF00), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biBlueMask = 0x000000FF), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask = 0xFF000000), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(/*ciexyzTriple*/36), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaRed = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaGreen = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaBlue = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biIntent = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileData = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileSize = 0), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biReserved = 0), 0, array, num, 4);
            return array;
        }
    }
    //BITMAPV3INFOHEADER, * PBITMAPV3INFOHEADER;
}
