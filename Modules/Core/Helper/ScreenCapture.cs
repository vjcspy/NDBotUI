// using System;
// using System.Drawing;
// using System.Drawing.Imaging;
// using System.Runtime.InteropServices;
//
// namespace NDBotUI.Modules.Core.Helper;
//
// public class ScreenCapture
// {
//     [DllImport("user32.dll")]
//     private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
//
//     [DllImport("user32.dll")]
//     private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
//
//     /// <summary>
//     ///     Chụp ảnh màn hình cửa sổ ứng dụng theo tên và lưu vào file.
//     /// </summary>
//     /// <param name="windowTitle">Tên cửa sổ ứng dụng cần chụp.</param>
//     /// <param name="outputPath">Đường dẫn lưu ảnh chụp màn hình.</param>
//     public static void TakeScreenshot(string windowTitle, string outputPath)
//     {
//         var hWnd = FindWindow(null, windowTitle);
//         if (hWnd == IntPtr.Zero)
//         {
//             Console.WriteLine("Không tìm thấy cửa sổ: " + windowTitle);
//             return;
//         }
//
//         // Giả định kích thước cửa sổ, cần chỉnh sửa nếu cửa sổ lớn hơn
//         var width = 1920;
//         var height = 1080;
//
//         using (var bmp = new Bitmap(width, height))
//         {
//             using (var g = Graphics.FromImage(bmp))
//             {
//                 var hdc = g.GetHdc();
//                 var success = PrintWindow(hWnd, hdc, 0);
//                 g.ReleaseHdc(hdc);
//
//                 if (!success)
//                 {
//                     Console.WriteLine("Không thể chụp ảnh cửa sổ.");
//                     return;
//                 }
//             }
//
//             bmp.Save(outputPath, ImageFormat.Png);
//             Console.WriteLine($"Ảnh chụp màn hình đã lưu tại: {outputPath}");
//         }
//     }
// }

