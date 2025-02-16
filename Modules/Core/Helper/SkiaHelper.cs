using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;
using LanguageExt;
using NDBotUI.Modules.Shared.Emulator.Models;
using NLog;
using SkiaSharp;

namespace NDBotUI.Modules.Core.Helper;

public class SkiaHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task<Unit> SaveScreenshot(
        EmulatorConnection emulatorConnection,
        string[] filePaths,
        Framebuffer? screenshot = null
    )
    {
        try
        {
            screenshot ??= await emulatorConnection.TakeScreenshotAsync();
            if (screenshot == null)
            {
                throw new ArgumentNullException(nameof(screenshot));
            }

            var imagePath = Path.Combine(FileHelper.getFolderPath(filePaths));
            var emguMat = screenshot.ToSKBitmap();
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                emguMat?.Encode(stream, SKEncodedImageFormat.Png, 100);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while saving screenshot");
            throw;
        }


        return Unit.Default;
    }

    public static async Task<Unit> SaveScreenshot(
        EmulatorConnection emulatorConnection,
        string imagePath,
        Framebuffer? screenshot = null
    )
    {
        try
        {
            screenshot ??= await emulatorConnection.TakeScreenshotAsync();
            if (screenshot == null)
            {
                throw new ArgumentNullException(nameof(screenshot));
            }

            var emguMat = screenshot.ToSKBitmap();
            await using var stream = new FileStream(imagePath, FileMode.Create);
            emguMat?.Encode(stream, SKEncodedImageFormat.Png, 100);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while saving screenshot");
        }


        return Unit.Default;
    }
    
    public static Mat SkiaBitmapToMat(SKBitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        // Tạo Mat có 4 kênh (RGBA)
        Mat mat = new Mat(height, width, DepthType.Cv8U, 4);
        
        // Lấy mảng pixel từ SkiaBitmap
        SKColor[] pixels = bitmap.Pixels;

        // Chuyển SKColor[] thành byte[]
        byte[] byteData = new byte[width * height * 4];
        for (int i = 0; i < pixels.Length; i++)
        {
            byteData[i * 4 + 0] = pixels[i].Blue;
            byteData[i * 4 + 1] = pixels[i].Green;
            byteData[i * 4 + 2] = pixels[i].Red;
            byteData[i * 4 + 3] = pixels[i].Alpha; // Giữ lại kênh Alpha nếu cần
        }

        // Ghi dữ liệu vào Mat
        Marshal.Copy(byteData, 0, mat.DataPointer, byteData.Length);

        // Chuyển từ BGRA → BGR (nếu cần)
        CvInvoke.CvtColor(mat, mat, ColorConversion.Bgra2Bgr);

        return mat;
    }
}