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
        public short R, G, B;
    }
    public struct tagBITMAPV3INFOHEADER
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
        internal readonly uint BitmapHeader = 14;
        internal readonly uint HeaderOffset = 0;
        public byte[] CreateHeader(REW rew)
        {
            byte[] array = new byte[rew.RealLength + HeaderOffset + BitmapHeader];
            Array.Copy(BitConverter.GetBytes(biSize = HeaderOffset), 0, array, 0, 4);
            Array.Copy(BitConverter.GetBytes(biWidth = (int)rew.Width), 0, array, 4, 4);
            Array.Copy(BitConverter.GetBytes(biHeight = (int)rew.Height), 0, array, 8, 4);
            Array.Copy(BitConverter.GetBytes(biPlanes = 0), 0, array, 12, 2);
            Array.Copy(BitConverter.GetBytes(biBitCount = 32), 0, array, 12, 2);
            Array.Copy(BitConverter.GetBytes(biCompression = (uint)CompressionMethod.BI_ALPHABITFIELDS), 0, array, 14, 4);
            Array.Copy(BitConverter.GetBytes(biSizeImage = (uint)rew.RealLength), 0, array, 18, 4);
            Array.Copy(BitConverter.GetBytes(biXPelsPerMeter = 96), 0, array, 22, 4);
            Array.Copy(BitConverter.GetBytes(biYPelsPerMeter = 96), 0, array, 26, 4);
            Array.Copy(BitConverter.GetBytes(biClrUsed = 0), 0, array, 30, 4);
            Array.Copy(BitConverter.GetBytes(biClrImportant = 0), 0, array, 34, 4);
            Array.Copy(BitConverter.GetBytes(biRedMask = 0), 0, array, 38, 4);
            Array.Copy(BitConverter.GetBytes(biGreenMask =  0), 0, array, 42, 4);
            Array.Copy(BitConverter.GetBytes(biBlueMask = 0), 0, array, 46, 4);
            Array.Copy(BitConverter.GetBytes(biAlphaMask = 0), 0, array, 50, 4);
            Array.Copy(BitConverter.GetBytes(biCSType = 0), 0, array, 54, 4);
            Array.Copy(BitConverter.GetBytes(biAlphaMask = 0), 0, array, 58, 4);
            Array.Copy(BitConverter.GetBytes(biCSType = 0), 0, array, 62, 4);
            Array.Copy(BitConverter.GetBytes(/*ciexyzTriple*/18), 0, array, 66, 18);
            Array.Copy(BitConverter.GetBytes(biGammaRed = 0), 0, array, 84, 4);
        }
    }
    //BITMAPV3INFOHEADER, * PBITMAPV3INFOHEADER;
}
