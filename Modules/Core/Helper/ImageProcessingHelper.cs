using System;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using NLog;
using SkiaSharp;
using ImreadModes = Emgu.CV.CvEnum.ImreadModes;
using Mat = Emgu.CV.Mat;
using Point = System.Drawing.Point;

namespace NDBotUI.Modules.Core.Helper;

public static class ImageProcessingHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static Point? FindImageInScreenshot(SKBitmap screenshot, string templatePath, string? markedScreenshotPath)
    {
        try
        {
            Logger.Info($"FindImageInScreenshot: {templatePath}");
            var templateMat = GetMatByPath(templatePath);

            return templateMat == null ? null : FindImageInScreenshot(screenshot, templateMat, markedScreenshotPath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Could not load template image from path {templatePath}");
        }

        return null;
    }

    public static Mat? GetMatByPath(string imagePath)
    {
        // Kiểm tra xem file template có tồn tại không
        if (!File.Exists(imagePath))
        {
            Logger.Error($"Template image not found in path {imagePath}");
            throw new FileNotFoundException($"Template image not found in path {imagePath}");
        }

        var templateMat = CvInvoke.Imread(imagePath, ImreadModes.Color);

        return templateMat;
    }

    /// <summary>
    /// Tìm kiếm ảnh mẫu trong ảnh chụp màn hình.
    /// </summary>
    public static Point? FindImageInScreenshot(SKBitmap screenshot, Mat templateMat, string? markedScreenshotPath)
    {
        Logger.Info("FindImageInScreenshot by templateMat");

        // Chuyển đổi SKBitmap -> Mat
        using var screenshotMat = ConvertSKBitmapToMat(screenshot);

        if (screenshotMat == null || screenshotMat.IsEmpty || templateMat.IsEmpty)
        {
            Logger.Error("Could not convert template mat from screenshot");
            return null;
        }

        // Chuyển ảnh về grayscale (CV_8U) để đảm bảo MatchTemplate hoạt động
        using var screenshotGray = new Mat();
        using var templateGray = new Mat();
        CvInvoke.CvtColor(screenshotMat, screenshotGray, ColorConversion.Bgr2Gray);
        CvInvoke.CvtColor(templateMat, templateGray, ColorConversion.Bgr2Gray);

        // Tạo Mat kết quả
        using var result = new Mat();
        CvInvoke.MatchTemplate(screenshotGray, templateGray, result, TemplateMatchingType.CcoeffNormed);

        // Tìm điểm có độ tương đồng cao nhất
        double minVal = 0, maxVal = 0;
        Point minLoc = default, maxLoc = default;
        CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        Logger.Info($"MatchTemplate Score: {maxVal}");

        // Nếu độ khớp > 0.8 thì coi là tìm thấy
        if (maxVal >= 0.8)
        {
            Point topLeft = maxLoc;

            if (markedScreenshotPath == null) return topLeft;

            // ✏️ Vẽ hình chữ nhật quanh vùng tìm thấy
            CvInvoke.Rectangle(screenshotMat,
                new(topLeft, new(templateMat.Width, templateMat.Height)),
                new(0, 255, 0), 3);

            // 💾 Lưu ảnh kết quả
            CvInvoke.Imwrite(markedScreenshotPath, screenshotMat);
            Logger.Info($"Saved marked screenshot at path: {markedScreenshotPath}");

            return topLeft;
        }

        return null;
    }

    // private static Mat SKBitmapToMat(SKBitmap bitmap)
    // {
    //     // Convert SKBitmap -> Bitmap
    //     using var bmp = SKBitmapToBitmap(bitmap);
    //
    //     // Convert Bitmap -> Mat (Emgu.CV)
    //     return bmp.ToMat();
    // }
    //
    // private static Bitmap SKBitmapToBitmap(SKBitmap skBitmap)
    // {
    //     using var skImage = SKImage.FromBitmap(skBitmap);
    //     using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
    //
    //     using var ms = new MemoryStream(skData.ToArray());
    //     return new(ms);
    // }

    private static Mat? ConvertSKBitmapToMat(SKBitmap skBitmap)
    {
        try
        {
            // Lấy dữ liệu pixel từ SKBitmap dưới dạng mảng SKColor[]
            int width = skBitmap.Width;
            int height = skBitmap.Height;
            SKColor[] pixels = skBitmap.Pixels;

            // Tạo mảng byte có kích thước đúng (4 bytes cho mỗi pixel)
            byte[] pixelData = new byte[width * height * 4]; // 4 byte cho mỗi pixel (RGBA)

            // Chuyển đổi dữ liệu từ SKColor[] sang byte[]
            for (int i = 0; i < pixels.Length; i++)
            {
                SKColor pixel = pixels[i];
                pixelData[i * 4 + 0] = pixel.Alpha;  // Alpha
                pixelData[i * 4 + 1] = pixel.Red;    // Red
                pixelData[i * 4 + 2] = pixel.Green;  // Green
                pixelData[i * 4 + 3] = pixel.Blue;   // Blue
            }

            // Tạo Mat từ mảng byte
            Mat mat = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 4); // 4 kênh (RGBA)
            mat.SetTo(pixelData);

            return mat;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Could not convert SKBitmap to mat");
        }

        return null;
    }
}