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
            int pixelCount = overlay.Length / 4;
            int bufferIndex = (y * sourceWidth + x);
            int fix = Math.Abs(bufferIndex - source.Length);
            int padding = sourceWidth - overlayWidth;
            int newWidth = overlayWidth;
            int n = 0;
            if (x >= sourceWidth)
            {
                return;
            }
            if (x < 0)
            {
                return;
                x = Math.Abs(x);
                bufferIndex = x;
            }
            if (y < 0)
            {
                return;
                bufferIndex -= -y * overlayWidth;
            }
            if (pixelCount > fix)
            {
                pixelCount = fix;
            }
            if (pixelCount + bufferIndex > source.Length / 4)
            {
                pixelCount -= pixelCount + bufferIndex - source.Length / 4;
            }
            if (x + overlayWidth > sourceWidth)
            {
                newWidth = Math.Abs(x - sourceWidth);
            }
            if (padding < 0)
            {
                padding = 0;
            }
            if (bufferIndex < 0 || bufferIndex >= source.Length / 4)
            {
                return;
            }
            unsafe
            {
                fixed (byte* onePtr = &source[bufferIndex * 4])
                {
                    fixed (byte* twoPtr = &overlay[0])
                    {
                        RGBA* srcARGB = (RGBA*)onePtr;
                        RGBA* ovrARGB = (RGBA*)twoPtr;

                        for (int i = 0; i < pixelCount; i++)
                        {
                            if (ovrARGB->a == 0 && srcARGB->a == 0)
                            {
                                continue;
                            }
                            byte a = 255;
                            if (ovrARGB->a != 255 && srcARGB->a != 255)
                            { 
                                a = (byte)((ovrARGB->a + srcARGB->a) / 2);
                            }
                            byte r = (byte)(ovrARGB->r * 0.15 + srcARGB->r * (1 - 0.15));
                            byte g = (byte)(ovrARGB->g * 0.15 + srcARGB->g * (1 - 0.15));
                            byte b = (byte)(ovrARGB->b * 0.15 + srcARGB->b * (1 - 0.15));
                            
                            srcARGB->a = a;
                            srcARGB->r = r;
                            srcARGB->g = g;
                            srcARGB->b = b;

                            if (n++ == newWidth)
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
