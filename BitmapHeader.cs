using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationR
{
    //  DWORD           4 bytes uint
    //  LONG            4 bytes int
    //  WORD            2 bytes short
    //  CIEXYZTRIPLE    ? bytes object
    public struct CIEXYZTRIBPLE
    {
        public CIEXYZ[] TRIPLE;
    }
    public struct CIEXYZ
    {
        public int R, G, B;
    }
    public struct BITMAPINFOHEADER
    {
        internal uint biSize;
        internal int biWidth;
        internal int biHeight;
        internal short biPlanes;
        internal short biBitCount;
        internal uint biCompression;
        internal uint biSizeImage;
        internal int biXPelsPerMeter;
        internal int biYPelsPerMeter;
        internal uint biClrUsed;
        internal uint biClrImportant;
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
                .Concat(BitConverter.GetBytes(header.biCompression))
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
                .Concat(BitConverter.GetBytes(header.biPlanes = (short)0))
                //Pixel format
                .Concat(BitConverter.GetBytes(header.biBitCount = image.BitsPerPixel))
                //Compression, if raw, normally 0
                .Concat(BitConverter.GetBytes(header.biCompression = (uint)CompressionMethod.BI_RGB))
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
    public struct BITMAPV3INFOHEADER
    {
        internal uint biSize;
        internal int biWidth;
        internal int biHeight;
        internal short biPlanes;
        internal short biBitCount;
        internal uint biCompression;
        internal uint biSizeImage;
        internal int biXPelsPerMeter;
        internal int biYPelsPerMeter;
        internal uint biClrUsed;
        internal uint biClrImportant;
        internal uint biRedMask;
        internal uint biGreenMask;
        internal uint biBlueMask;
        internal uint biAlphaMask;
        internal uint biCSType;
        internal CIEXYZTRIBPLE ciexyzTriple;
        internal uint biGammaRed;
        internal uint biGammaGreen;
        internal uint biGammaBlue;
        internal uint biIntent;
        internal uint biProfileData;
        internal uint biProfileSize;
        internal uint biReserved;
        public static readonly uint HeaderOffset = 100;
        public static byte[] CreateDIBHeader(BITMAPV3INFOHEADER header)
        {
            byte[] array = new byte[HeaderOffset];
            Array.Copy(BitConverter.GetBytes(header.biSize), 0, array, 0, 4);
            Array.Copy(BitConverter.GetBytes(header.biWidth), 0, array, 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biHeight), 0, array, 8, 4);
            Array.Copy(BitConverter.GetBytes(header.biPlanes), 0, array, 12, 2);
            Array.Copy(BitConverter.GetBytes(header.biBitCount), 0, array, 14, 2);
            Array.Copy(BitConverter.GetBytes(header.biCompression), 0, array, 16, 4);
            Array.Copy(BitConverter.GetBytes(header.biSizeImage), 0, array, 20, 4);
            Array.Copy(BitConverter.GetBytes(header.biXPelsPerMeter), 0, array, 24, 4);
            Array.Copy(BitConverter.GetBytes(header.biYPelsPerMeter), 0, array, 28, 4);
            Array.Copy(BitConverter.GetBytes(header.biClrUsed), 0, array, 32, 4);
            Array.Copy(BitConverter.GetBytes(header.biClrImportant), 0, array, 36, 4);
            Array.Copy(BitConverter.GetBytes(header.biRedMask), 0, array, 40, 4);
            Array.Copy(BitConverter.GetBytes(header.biGreenMask), 0, array, 44, 4);
            Array.Copy(BitConverter.GetBytes(header.biBlueMask), 0, array, 48, 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask ), 0, array, 52, 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType), 0, array, 56, 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask), 0, array, 60, 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType), 0, array, 64, 4);
            Array.Copy(BitConverter.GetBytes(/*ciexyzTriple*/24), 0, array, 68, 24);
            Array.Copy(BitConverter.GetBytes(header.biGammaRed), 0, array, 92, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaGreen), 0, array, 96, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaBlue), 0, array, 100, 4);
            Array.Copy(BitConverter.GetBytes(header.biIntent), 0, array, 104, 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileData), 0, array, 108, 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileSize), 0, array, 112, 4);
            Array.Copy(BitConverter.GetBytes(header.biReserved), 0, array, 116, 4);
            return array;
        }
        public static byte[] CreateDIBHeader(REW rew, out BITMAPV3INFOHEADER header)
        {
            header = new BITMAPV3INFOHEADER();
            byte[] array = new byte[HeaderOffset];
            Array.Copy(BitConverter.GetBytes(header.biSize = HeaderOffset), 0, array, 0, 4);
            Array.Copy(BitConverter.GetBytes(header.biWidth = (int)rew.Width), 0, array, 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biHeight = (int)rew.Height), 0, array, 8, 4);
            Array.Copy(BitConverter.GetBytes(header.biPlanes = 0), 0, array, 12, 2);
            Array.Copy(BitConverter.GetBytes(header.biBitCount = 32), 0, array, 14, 2);
            Array.Copy(BitConverter.GetBytes(header.biCompression = (uint)CompressionMethod.BI_ALPHABITFIELDS), 0, array, 16, 4);
            Array.Copy(BitConverter.GetBytes(header.biSizeImage = (uint)rew.RealLength), 0, array, 20, 4);
            Array.Copy(BitConverter.GetBytes(header.biXPelsPerMeter = 96), 0, array, 24, 4);
            Array.Copy(BitConverter.GetBytes(header.biYPelsPerMeter = 96), 0, array, 28, 4);
            Array.Copy(BitConverter.GetBytes(header.biClrUsed = 0), 0, array, 32, 4);
            Array.Copy(BitConverter.GetBytes(header.biClrImportant = 0), 0, array, 36, 4);
            Array.Copy(BitConverter.GetBytes(header.biRedMask = 0), 0, array, 40, 4);
            Array.Copy(BitConverter.GetBytes(header.biGreenMask =  0), 0, array, 44, 4);
            Array.Copy(BitConverter.GetBytes(header.biBlueMask = 0), 0, array, 48, 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask = 0), 0, array, 52, 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType = 0), 0, array, 56, 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask = 0), 0, array, 60, 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType = 0), 0, array, 64, 4);
            Array.Copy(BitConverter.GetBytes(/*ciexyzTriple*/18), 0, array, 68, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaRed = 0), 0, array, 72, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaGreen = 0), 0, array, 76, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaBlue = 0), 0, array, 80, 4);
            Array.Copy(BitConverter.GetBytes(header.biIntent = 0), 0, array, 84, 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileData = 0), 0, array, 88, 4);
            Array.Copy(BitConverter.GetBytes(header.biProfileSize = 0), 0, array, 92, 4);
            Array.Copy(BitConverter.GetBytes(header.biReserved = 0), 0, array, 96, 4);
            return array;
        }
    }
    //BITMAPV3INFOHEADER, * PBITMAPV3INFOHEADER;
}
