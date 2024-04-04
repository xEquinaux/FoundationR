#include <Windows.h>
#include <d2d1_1.h>
#include <wincodec.h>
#pragma comment(lib, "d2d1")
#pragma comment(lib, "windowscodecs")

// Initialize Direct2D and WIC
ID2D1Factory* g_pDirect2dFactory = nullptr;
IWICImagingFactory* g_pWICFactory = nullptr;

void Initialize() {
    // Create Direct2D factory
    HRESULT hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &g_pDirect2dFactory);
    if (FAILED(hr)) {
        // Handle error
        return;
    }

    // Create WIC factory
    hr = CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER,
        IID_IWICImagingFactory, reinterpret_cast<void**>(&g_pWICFactory));
    if (FAILED(hr)) {
        // Handle error
        return;
    }
}

ID2D1Bitmap* CreateD2DBitmapFromARGBArray(const BYTE* argbBytes, UINT width, UINT height) {
    // Create a WIC bitmap from the ARGB byte array
    IWICBitmap* pWICBitmap = nullptr;
    HRESULT hr = g_pWICFactory->CreateBitmapFromMemory(width, height,
        GUID_WICPixelFormat32bppPBGRA, width * 4, width * height * 4,
        argbBytes, &pWICBitmap);
    if (FAILED(hr)) {
        // Handle error
        return nullptr;
    }

    // Create an ID2D1Bitmap from the WIC bitmap
    ID2D1Bitmap* pD2DBitmap = nullptr;
    hr = g_pRenderTarget->CreateBitmapFromWicBitmap(pWICBitmap, &pD2DBitmap);
    if (FAILED(hr)) {
        // Handle error
        return nullptr;
    }

    // Release the WIC bitmap
    SafeRelease(&pWICBitmap);

    return pD2DBitmap;
}

// Usage example
void Render(ID2D1RenderTarget* pRenderTarget) {
    // Assume you have an ARGB byte array named 'argbBytes'
    ID2D1Bitmap* pBitmap = CreateD2DBitmapFromARGBArray(argbBytes, width, height);
    if (pBitmap) {
        // Draw the bitmap using pRenderTarget
        pRenderTarget->DrawBitmap(pBitmap, D2D1::RectF(0, 0, width, height));
        SafeRelease(&pBitmap);
    }
}

// Cleanup resources
void Cleanup() {
    SafeRelease(&g_pDirect2dFactory);
    SafeRelease(&g_pWICFactory);
}

// Main function and window setup...
