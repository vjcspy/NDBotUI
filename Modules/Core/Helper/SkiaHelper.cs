using System;
using System.IO;
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
    //
    // public static Mat? ConvertSKBitmapToMat(SKBitmap skBitmap)
    // {
    //     try
    //     {
    //         // Lấy dữ liệu pixel từ SKBitmap dưới dạng mảng SKColor[]
    //         int width = skBitmap.Width;
    //         int height = skBitmap.Height;
    //         SKColor[] pixels = skBitmap.Pixels;
    //
    //         // Tạo mảng byte có kích thước đúng (4 bytes cho mỗi pixel)
    //         byte[] pixelData = new byte[width * height * 4]; // 4 byte cho mỗi pixel (RGBA)
    //
    //         // Chuyển đổi dữ liệu từ SKColor[] sang byte[]
    //         for (int i = 0; i < pixels.Length; i++)
    //         {
    //             SKColor pixel = pixels[i];
    //             pixelData[i * 4 + 0] = pixel.Alpha;  // Alpha
    //             pixelData[i * 4 + 1] = pixel.Red;    // Red
    //             pixelData[i * 4 + 2] = pixel.Green;  // Green
    //             pixelData[i * 4 + 3] = pixel.Blue;   // Blue
    //         }
    //
    //         // Tạo Mat từ mảng byte
    //         Mat mat = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 4); // 4 kênh (RGBA)
    //         mat.SetTo(pixelData);
    //
    //         return mat;
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.Error(e, $"Could not convert SKBitmap to mat");
    //     }
    //
    //     return null;
    // }
    //
    // public static Mat? ConvertSKBitmapToMat1(SKBitmap skBitmap)
    // {
    //     try
    //     {
    //         // Lấy kích thước ảnh
    //         int width = skBitmap.Width;
    //         int height = skBitmap.Height;
    //
    //         // Tạo mảng byte chứa dữ liệu ảnh
    //         byte[] imageData = skBitmap.Bytes;
    //
    //         // Tạo Mat có cùng kích thước và 3 kênh màu (RGB)
    //         Mat mat = new Mat(height, width, DepthType.Cv8U, 4); // Skia lưu ảnh theo BGRA (4 kênh)
    //     
    //         // Sao chép dữ liệu từ mảng byte vào Mat
    //         mat.SetTo(imageData);
    //
    //         // Chuyển từ BGRA → BGR để phù hợp với OpenCV
    //
    //         return mat;
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.Error(e, $"Could not convert SKBitmap to mat");
    //     }
    //
    //     return null;
    // }
}