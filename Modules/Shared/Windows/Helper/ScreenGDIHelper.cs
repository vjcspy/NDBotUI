using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NDBotUI.Modules.Shared.Windows.Helper;

public class ScreenGDIHelper
{
    public static void TakeScreenShot(string windowTitle)
    {
        var hwnd = FindWindow(null, windowTitle);

        if (hwnd == IntPtr.Zero)
        {
            Console.WriteLine("Không tìm thấy cửa sổ.");
            return;
        }

        GetWindowRect(hwnd, out var rect);
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        var hdcWindow = GetDC(hwnd);
        var hdcMemDC = CreateCompatibleDC(hdcWindow);
        var hBitmap = CreateCompatibleBitmap(hdcWindow, width, height);
        var hOld = SelectObject(hdcMemDC, hBitmap);

        // Chụp ảnh
        BitBlt(hdcMemDC, 0, 0, width, height, hdcWindow, 0, 0, 0x00CC0020);

        // Chuyển thành Bitmap C#
        using (var bitmap = Image.FromHbitmap(hBitmap))
        {
            bitmap.Save("game_gdi_screenshot.png", ImageFormat.Png);
        }

        // Dọn dẹp
        SelectObject(hdcMemDC, hOld);
        DeleteObject(hBitmap);
        DeleteDC(hdcMemDC);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr hdcDest,
        int xDest,
        int yDest,
        int w,
        int h,
        IntPtr hdcSource,
        int xSrc,
        int ySrc,
        int rop
    );

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }
}