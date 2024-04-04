#include <Windows.h>
#include <d2d1.h>
#include <wincodec.h>
#include <Direct2D.h>
#pragma comment(lib, "d2d1")
#pragma comment(lib, "windowscodecs")

// Initialize Direct2D
DLL_EXPORT void Direct2D_Init()
{
    D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &pD2DFactory);
    
    // Create WIC factory
    hr = CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER,
        IID_IWICImagingFactory, reinterpret_cast<void**>(&g_pWICFactory));
    if (FAILED(hr)) 
    {
        // Handle error
        return;
    }
    
    // Create a window
    hWnd = CreateWindow(L"WindowClass", L"Direct2D Example", WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 800, 600, nullptr, nullptr, nullptr, nullptr);

    // Create a render target
    D2D1_RENDER_TARGET_PROPERTIES props = D2D1::RenderTargetProperties(
        D2D1_RENDER_TARGET_TYPE_DEFAULT, D2D1::PixelFormat(DXGI_FORMAT_UNKNOWN, D2D1_ALPHA_MODE_PREMULTIPLIED));
    pD2DFactory->CreateHwndRenderTarget(props, D2D1::HwndRenderTargetProperties(hWnd, D2D1::SizeU(800, 600)), &pRenderTarget);
}

// Draw something
DLL_EXPORT void Direct2D_Render(const BYTE* argbBytes, UINT width, UINT height)
{
    // Assume you have an ARGB byte array named 'argbBytes'
    ID2D1Bitmap* pBitmap = CreateD2DBitmapFromARGBArray(argbBytes, width, height);
    if (pBitmap) 
    {
        // Draw the bitmap using pRenderTarget
        pRenderTarget->BeginDraw();
        pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::White));
        pRenderTarget->DrawBitmap(pBitmap, D2D1::RectF(0, 0, width, height));
        pRenderTarget->EndDraw();
        SafeRelease(&pBitmap);
    }
}

DLL_EXPORT void Direct2D_Dispose()
{
    // Cleanup
    pRenderTarget->Release();
    pD2DFactory->Release();
    SafeRelease(&g_pWICFactory);
    DestroyWindow(hWnd);
}

ID2D1Bitmap* CreateD2DBitmapFromARGBArray(const BYTE* argbBytes, UINT width, UINT height) 
{
    // Create a WIC bitmap from the ARGB byte array
    IWICBitmap* pWICBitmap = nullptr;
    HRESULT hr = g_pWICFactory->CreateBitmapFromMemory(width, height,
        GUID_WICPixelFormat32bppPBGRA, width * 4, width * height * 4,
        argbBytes, &pWICBitmap);
    if (FAILED(hr)) 
    {
        // Handle error
        return nullptr;
    }

    // Create an ID2D1Bitmap from the WIC bitmap
    ID2D1Bitmap* pD2DBitmap = nullptr;
    hr = g_pRenderTarget->CreateBitmapFromWicBitmap(pWICBitmap, &pD2DBitmap);
    if (FAILED(hr)) 
    {
        // Handle error
        return nullptr;
    }

    // Release the WIC bitmap
    SafeRelease(&pWICBitmap);

    return pD2DBitmap;
}
