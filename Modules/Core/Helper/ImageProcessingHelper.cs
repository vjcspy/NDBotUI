using System;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;
using NLog;
using SkiaSharp;

namespace NDBotUI.Modules.Core.Helper;

public static class ImageProcessingHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Tìm kiếm ảnh mẫu trong ảnh chụp màn hình (SKBitmap) và vẽ hình chữ nhật quanh vùng tìm thấy.
    /// </summary>
    /// <param name="screenshot">Ảnh screenshot hiện tại (SKBitmap).</param>
    /// <param name="templatePath">Đường dẫn đến ảnh mẫu (PNG).</param>
    /// <param name="outputPath">Đường dẫn lưu ảnh có vẽ hình chữ nhật.</param>
    /// <param name="isSaveMarkedImage">Lưu ảnh có đánh dấu hay không.</param>
    /// <returns>Trả về tọa độ tìm thấy hoặc null nếu không tìm thấy.</returns>
    public static Point? FindImageInScreenshot(SKBitmap screenshot, string templatePath, string? markedScreenshotPath)
    {
        try
        {
            Logger.Info($"FindImageInScreenshot: {templatePath}");

            // Kiểm tra xem file template có tồn tại không
            if (!File.Exists(templatePath))
            {
                Logger.Error($"Template image not found in path {templatePath}");
                return null;
            }

            using var templateMat = CvInvoke.Imread(templatePath, ImreadModes.Color);
            return FindImageInScreenshot(screenshot, templateMat, markedScreenshotPath);
        }
        catch (Exception ex)
        {
            Logger.Error($"Could not load template image from path {templatePath} with error " + ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Tìm kiếm ảnh mẫu trong ảnh chụp màn hình.
    /// </summary>
    public static Point? FindImageInScreenshot(SKBitmap screenshot, Mat templateMat, string? markedScreenshotPath)
    {
        Logger.Info("FindImageInScreenshot by templateMat");

        // Chuyển đổi SKBitmap -> Mat
        using var screenshotMat = SKBitmapToMat(screenshot);

        if (screenshotMat.IsEmpty || templateMat.IsEmpty)
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
                new Rectangle(topLeft, new Size(templateMat.Width, templateMat.Height)),
                new MCvScalar(0, 255, 0), 3);

            // 💾 Lưu ảnh kết quả
            CvInvoke.Imwrite(markedScreenshotPath, screenshotMat);
            Logger.Info($"Saved marked screenshot at path: {markedScreenshotPath}");

            return topLeft;
        }

        return null;
    }

    /// <summary>
    /// Chuyển đổi SKBitmap sang Emgu.CV Mat.
    /// </summary>
    private static Mat SKBitmapToMat(SKBitmap bitmap)
    {
        // Convert SKBitmap -> Bitmap
        using var bmp = SKBitmapToBitmap(bitmap);

        // Convert Bitmap -> Mat (Emgu.CV)
        return bmp.ToMat();
    }

    /// <summary>
    /// Chuyển SKBitmap thành Bitmap (.NET)
    /// </summary>
    private static Bitmap SKBitmapToBitmap(SKBitmap skBitmap)
    {
        using var skImage = SKImage.FromBitmap(skBitmap);
        using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);

        using var ms = new MemoryStream(skData.ToArray());
        return new Bitmap(ms);
    }
}