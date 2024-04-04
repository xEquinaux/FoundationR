#include <Windows.h>
#include <d2d1_1.h>
#pragma comment(lib, "d2d1")

// Global variables
ID2D1Factory* g_pDirect2dFactory = nullptr;
ID2D1HwndRenderTarget* g_pRenderTarget = nullptr;
ID2D1SolidColorBrush* g_pBrush = nullptr;

// Release resources
void ReleaseResources() {
    SafeRelease(&g_pRenderTarget);
    SafeRelease(&g_pBrush);
}

// Initialize Direct2D
void Initialize() {
    ReleaseResources();

    // Create Direct2D factory
    HRESULT hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &g_pDirect2dFactory);
    if (FAILED(hr)) {
        // Handle error
        return;
    }

    // Create a window and get its client area
    HWND hwnd = /* Obtain your window handle */;
    RECT rc;
    GetClientRect(hwnd, &rc);
    D2D1_SIZE_U size = D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top);

    // Create a render target
    hr = g_pDirect2dFactory->CreateHwndRenderTarget(
        D2D1::RenderTargetProperties(),
        D2D1::HwndRenderTargetProperties(hwnd, size),
        &g_pRenderTarget
    );
    if (FAILED(hr)) {
        // Handle error
        return;
    }

    // Create a red brush
    hr = g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red), &g_pBrush);
    if (FAILED(hr)) {
        // Handle error
        return;
    }
}

// Render the scene
void Render() {
    g_pRenderTarget->BeginDraw();
    g_pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::White));

    // Draw a red rectangle using ARGB byte array
    D2D1_RECT_F rect = D2D1::RectF(50.0f, 50.0f, 150.0f, 100.0f);
    g_pRenderTarget->FillRectangle(rect, g_pBrush);

    HRESULT hr = g_pRenderTarget->EndDraw();
    if (FAILED(hr)) {
        // Handle error
    }
}

// Window procedure
LRESULT CALLBACK WndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam) {
    switch (message) {
        case WM_PAINT:
            Render();
            break;
        case WM_DESTROY:
            ReleaseResources();
            PostQuitMessage(0);
            break;
        default:
            return DefWindowProc(hwnd, message, wParam, lParam);
    }
    return 0;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
    // Create and register the window class
    WNDCLASS wc = { 0 };
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = L"Direct2DExample";
    RegisterClass(&wc);

    // Create the window
    HWND hwnd = CreateWindow(wc.lpszClassName, L"Direct2D Example", WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 800, 600, nullptr, nullptr, hInstance, nullptr);

    // Initialize Direct2D
    Initialize();

    // Show the window
    ShowWindow(hwnd, nCmdShow);

    // Message loop
    MSG msg;
    while (GetMessage(&msg, nullptr, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return static_cast<int>(msg.wParam);
}
