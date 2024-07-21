using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FoundationR
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ARGB
	{
		public byte a;
		public byte r;
		public byte g;
		public byte b;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct BGRA
	{
		public byte b;
		public byte g;
		public byte r;
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
		public static void Process_Pointer_Cast(int x, int y, int sourceWidth, int sourceHeight, int overlayWidth, int overlayHeight, byte[] source, byte[] overlay)
		{
			int pixelCount = overlay.Length / 4;
			int bufferIndex = (y * sourceWidth + x);

			int imageWidth = overlayWidth; // Image width
			int imageHeight = overlayHeight; // Image height
			int viewportX = 0; // Viewport X position
			int viewportY = 0; // Viewport Y position
			int viewportWidth = sourceWidth; // Viewport width
			int viewportHeight = sourceHeight; // Viewport height

			byte[] image = overlay; // Your image data here
			byte[] croppedImage = new byte[viewportWidth * viewportHeight];

			for (int j = 0; j < viewportHeight; j++)
			{
				for (int i = 0; i < viewportWidth - 4; i += 4)
				{
					// Calculate the source position
					int sourcePos = ((viewportY + j) * imageWidth) + (viewportX + i);

					// Calculate the destination position
					int destPos = (j * viewportWidth) + i;

					// Copy the pixel
					if (sourcePos >= image.Length - 4)
						continue;
					if (destPos >= croppedImage.Length - 4)
						continue;
					croppedImage[destPos] = image[sourcePos];
					croppedImage[destPos + 1] = image[sourcePos + 1];
					croppedImage[destPos + 2] = image[sourcePos + 2];
					croppedImage[destPos + 3] = image[sourcePos + 3];
				}
			}

			unsafe
			{
				fixed (byte* onePtr = &source[bufferIndex * 4])
				{
					fixed (byte* twoPtr = &overlay[0])
					{
						BGRA* srcBGRA = (BGRA*)onePtr;
						ARGB* ovrARGB = (ARGB*)twoPtr;

						for (int i = 0; i < pixelCount; i++)
						{
							if (ovrARGB->a == 0 && srcBGRA->a == 0)
							{
								continue;
							}
							byte a = 255;
							byte r = 0;
							byte g = 0;
							byte b = 0;
							if (ovrARGB->a != 255 && srcBGRA->a != 255)
								a = (byte)((ovrARGB->a + srcBGRA->a) / 2);
							if (ovrARGB->a != 255)
							{
								r = (byte)(ovrARGB->r * 0.15 + srcBGRA->r * (1 - 0.15));
								g = (byte)(ovrARGB->g * 0.15 + srcBGRA->g * (1 - 0.15));
								b = (byte)(ovrARGB->b * 0.15 + srcBGRA->b * (1 - 0.15));
							}
							else
							{
								a = 255;
								r = ovrARGB->r;
								g = ovrARGB->g;
								b = ovrARGB->b;
							}

							srcBGRA->a = a;
							srcBGRA->r = r;
							srcBGRA->g = g;
							srcBGRA->b = b;

							srcBGRA++;
							ovrARGB++;
						}
					}
				}
			}
		}
		public static byte[] Recolor(this byte[] array, ARGB color)
		{
			unsafe
			{
				fixed (byte* onePtr = &array[0])
				{
					ARGB* srcARGB = (ARGB*)onePtr;

					for (int i = 0; i < array.Length; i++)
					{
						srcARGB->r = (byte)(srcARGB->r * color.r / 255);
						srcARGB->g = (byte)(srcARGB->g * color.g / 255);
						srcARGB->b = (byte)(srcARGB->b * color.b / 255);
					}
				}
			}
			return array;
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
						ARGB* argb = (ARGB*)rgbaPtr;

						for (int i = 0; i < pixelCount; i++)
						{
							RGB* cp = (RGB*)argb;
							*cp = *rgb;
							argb->a = 255;
							rgb++;
							argb++;
						}
					}
				}
			}
		}
	}
}
