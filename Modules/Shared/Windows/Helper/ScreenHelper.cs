using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

class ScreenHelper
{
    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    public static bool TakeScreenshot(string windowTitle, string savePath)
    {
        try
        {
            // 1️⃣ Find the game window by title
            IntPtr hwnd = FindWindow(null, windowTitle);
            if (hwnd == IntPtr.Zero)
            {
                Console.WriteLine("Window not found: " + windowTitle);
                return false;
            }

            // 2️⃣ Get window size
            GetWindowRect(hwnd, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
            {
                Console.WriteLine("Invalid window dimensions.");
                return false;
            }

            // 3️⃣ Capture only the game window area
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(rect.Left, rect.Top), Point.Empty, new Size(width, height));
                }

                // Save as PNG
                bitmap.Save(savePath, ImageFormat.Png);
            }

            Console.WriteLine("Screenshot saved: " + savePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error capturing screenshot: " + ex.Message);
            return false;
        }
    }
}
