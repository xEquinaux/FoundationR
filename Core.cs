using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FoundationR
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBA
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGB
    {
        public byte r;
        public byte g;
        public byte b;
    }
    public static class Core
    { 
        public static void Process_Pointer_Cast(int x, int y, int sourceWidth, int overlayWidth, byte[] source, byte[] overlay)
        {
            int pixelCount = overlay.Length;
            int bufferIndex = (y * sourceWidth + x) * 4;
            int fix = Math.Abs(bufferIndex - source.Length);
            int padding = sourceWidth - overlayWidth;
            int newWidth = overlayWidth;
            int compensate = 0;
            int n = 0;
            if (pixelCount > fix)
            {
                pixelCount = fix;
            }
            if (pixelCount + bufferIndex > source.Length)
            {
                pixelCount -= pixelCount + bufferIndex - source.Length;
            }
            if (x + overlayWidth > sourceWidth)
            {
                newWidth = sourceWidth - overlayWidth;
            }
            unsafe
            {
                fixed (byte* onePtr = &source[0])
                {
                    fixed (byte* twoPtr = &overlay[0])
                    {
                        RGBA* srcARGB = (RGBA*)onePtr;
                        RGBA* ovrARGB = (RGBA*)twoPtr;

                        for (int m = 0; m < bufferIndex; m++)
                        {
                            srcARGB++;
                        }
                        for (int i = 0; i < pixelCount; i++)
                        {
                            byte a = (byte)((ovrARGB->a + srcARGB->a) / 2);
                            byte r = (byte)(ovrARGB->r * 0.15 + srcARGB->r * (1 - 0.15));
                            byte g = (byte)(ovrARGB->g * 0.15 + srcARGB->g * (1 - 0.15));
                            byte b = (byte)(ovrARGB->b * 0.15 + srcARGB->b * (1 - 0.15));
                            
                            srcARGB->a = a;
                            srcARGB->r = r;
                            srcARGB->g = g;
                            srcARGB->b = b;

                            if (++n == newWidth)
                            {
                                for (int j = 0; j < padding; j++)
                                { 
                                    srcARGB++;
                                }
                                n = 0;
                                continue;
                            }
                            srcARGB++;
                            ovrARGB++;
                        }
                    }
                }
            }
        }
        static void Process_Pointer_Cast(int pixelCount, byte[] rgbData, byte[] rgbaData)
        {
            unsafe
            { 
                fixed (byte* rgbPtr = &rgbData[0])
                {
                    fixed (byte* rgbaPtr = &rgbaData[0])
                    {
                        RGB* rgb = (RGB*)rgbPtr;
                        RGBA* rgba = (RGBA*)rgbaPtr;

                        for (int i = 0; i < pixelCount; i++)
                        {
                            RGB* cp = (RGB*)rgba;
                            *cp = *rgb;
                            rgba->a = 255;
                            rgb++;
                            rgba++;
                        }
                    }
                }
            }
        }
    }
}
