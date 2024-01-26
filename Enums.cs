using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationR
{
    public enum DeviceContextValues : uint
    {
        Window = 0x00000001,
        IntersectRgn = 0x00000080
    }

    public enum CompressionMethod : uint
    {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5,
        BI_ALPHABITFIELDS = 6,
        BI_CMYK = 11,
        BI_CMYKRLE8 = 12,
        BI_CMYKRLE4 = 13
    }
    public enum BitmapHeader : int
    {
        BITMAPINFOHEADER,
        BITMAPV2INFOHEADER,
        BITMAPV3INFOHEADER,
        BITMAPV4HEADER,
        BITMAPV5HEADER
    }
}
