using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NLog;
using Mat = Emgu.CV.Mat;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using BFMatcher = Emgu.CV.Features2D.BFMatcher;
using ORB = Emgu.CV.Features2D.ORB;
using VectorOfKeyPoint = Emgu.CV.Util.VectorOfKeyPoint;

namespace NDBotUI.Modules.Core.Helper;

public static class ImageFinderEmguCV
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static Mat? GetMatByPath(string imagePath)
    {
        // Kiểm tra xem file template có tồn tại không
        if (!File.Exists(imagePath))
        {
            Logger.Error($"Template image not found in path {imagePath}");
            throw new FileNotFoundException($"Template image not found in path {imagePath}");
        }

        var templateMat = CvInvoke.Imread(imagePath);

        return templateMat;
    }

    public static void SaveMatToFile(Mat mat, string fileName = "screenshot.png")
    {
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        CvInvoke.Imwrite(outputPath, mat); // Lưu ảnh xuống file PNG

        Logger.Info($"Saved image to {outputPath}");
    }

    public static Point? FindTemplateORB(Mat screenshotMat, Mat templateMat)
    {
        using var orb = new ORB(1000); // 🔥 Tăng số lượng keypoints
        using var screenshotGray = new Mat();
        using var templateGray = new Mat();

        CvInvoke.CvtColor(screenshotMat, screenshotGray, ColorConversion.Bgr2Gray);
        CvInvoke.CvtColor(templateMat, templateGray, ColorConversion.Bgr2Gray);


        // Tìm keypoints và descriptors
        using var screenshotKeyPoints = new VectorOfKeyPoint();
        using var templateKeyPoints = new VectorOfKeyPoint();
        using var screenshotDescriptors = new Mat();
        using var templateDescriptors = new Mat();

        orb.DetectAndCompute(screenshotGray, null, screenshotKeyPoints, screenshotDescriptors, false);
        orb.DetectAndCompute(templateGray, null, templateKeyPoints, templateDescriptors, false);

        using var screenshotKeypointImg = new Mat();
        using var templateKeypointImg = new Mat();

        Features2DToolbox.DrawKeypoints(screenshotGray, screenshotKeyPoints, screenshotKeypointImg, new Bgr(0, 255, 0));
        Features2DToolbox.DrawKeypoints(templateGray, templateKeyPoints, templateKeypointImg, new Bgr(0, 255, 0));

        CvInvoke.Imwrite("screenshot_keypoints.jpg", screenshotKeypointImg);
        CvInvoke.Imwrite("template_keypoints.jpg", templateKeypointImg);

        // So khớp descriptor giữa hai ảnh bằng KNN Match
        using var matcher = new BFMatcher(DistanceType.Hamming);
        using var matches = new VectorOfVectorOfDMatch(); // 🔥 Dùng KNN Match

        matcher.KnnMatch(templateDescriptors, screenshotDescriptors, matches, 2);

        // Lọc good matches bằng Lowe’s Ratio Test
        var goodMatches = new List<MKeyPoint>();
        for (var i = 0; i < matches.Size; i++)
            if (matches[i][0].Distance < 0.75 * matches[i][1].Distance)
                goodMatches.Add(templateKeyPoints[matches[i][0].QueryIdx]);

        if (goodMatches.Count >= 4)
        {
            var matchedPoint = goodMatches[0].Point;
            return new Point((int)matchedPoint.X, (int)matchedPoint.Y);
        }

        return null;
    }

    public static Point? FindTemplateMatPoint(Mat screenshotMat, Mat templateMat,
        string? markedScreenshotFileName = null, string? debugKey = null, double? matchValue = 0.8)
    {
        var stopwatch = Stopwatch.StartNew();
        // Chuyển ảnh về grayscale (CV_8U) để đảm bảo MatchTemplate hoạt động
        using var screenshotGray = new Mat();
        using var templateGray = new Mat();

        CvInvoke.CvtColor(templateMat, templateGray, ColorConversion.Bgr2Gray);
        CvInvoke.CvtColor(screenshotMat, screenshotGray, ColorConversion.Bgr2Gray);
        // CvInvoke.CvtColor(templateMat, templateGray, ColorConversion.Bgr2Gray);

        // Tạo Mat kết quả
        using var result = new Mat();
        CvInvoke.MatchTemplate(screenshotGray, templateGray, result, TemplateMatchingType.CcoeffNormed);

        // Tìm điểm có độ tương đồng cao nhất
        double minVal = 0, maxVal = 0;
        Point minLoc = default, maxLoc = default;
        CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        Logger.Info($"MatchTemplate Score: {debugKey} {maxVal}");
        stopwatch.Stop();
        Logger.Debug("FindTemplateMatPoint finished in {time} ms", stopwatch.ElapsedMilliseconds);

        if (maxVal >= matchValue)
        {
            var topLeft = maxLoc;

            if (markedScreenshotFileName == null) return topLeft;

            // ✏️ Vẽ hình chữ nhật quanh vùng tìm thấy
            CvInvoke.Rectangle(screenshotMat,
                new Rectangle(topLeft, new Size(templateMat.Width, templateMat.Height)),
                new MCvScalar(0, 255, 0), 3);

            // 💾 Lưu ảnh kết quả
            var imagePath = ImageHelper.GetImagePath(markedScreenshotFileName);
            CvInvoke.Imwrite(imagePath, screenshotMat);
            // SaveMatToFile(screenshotMat, markedScreenshotPath);
            Logger.Info($"Saved marked screenshot at path: {imagePath}");

            return topLeft;
        }

        return null;
    }


    public static Point? FindTemplateMatPoint(Mat screenshotMat, Mat templateMat, bool shouldResize,
        string? markedScreenshotFileName = null, string? debugKey = null, double? matchValue = 0.8)
    {
        var stopwatch = Stopwatch.StartNew();

        var processedScreenshot = screenshotMat.Clone();
        var processedTemplate = templateMat.Clone();
        var scaleFactor = 1.0;

        // Nếu cần resize
        if (shouldResize)
        {
            var targetWidth = 960;
            scaleFactor = (double)targetWidth / screenshotMat.Width;
            var targetHeight = (int)(screenshotMat.Height * scaleFactor); // Giữ nguyên tỷ lệ

            processedScreenshot = new Mat();
            CvInvoke.Resize(screenshotMat, processedScreenshot, new Size(targetWidth, targetHeight),
                (double)Inter.Linear);

            processedTemplate = new Mat();
            CvInvoke.Resize(templateMat, processedTemplate, Size.Empty, scaleFactor, scaleFactor);
        }

        using (processedScreenshot)
        using (processedTemplate)
        {
            // Chuyển về ảnh grayscale
            using var screenshotGray = new Mat();
            using var templateGray = new Mat();
            CvInvoke.CvtColor(processedScreenshot, screenshotGray, ColorConversion.Bgr2Gray);
            CvInvoke.CvtColor(processedTemplate, templateGray, ColorConversion.Bgr2Gray);

            // Tạo Mat kết quả
            using var result = new Mat();
            CvInvoke.MatchTemplate(screenshotGray, templateGray, result, TemplateMatchingType.CcoeffNormed);

            // Tìm điểm có độ tương đồng cao nhất
            double minVal = 0, maxVal = 0;
            Point minLoc = default, maxLoc = default;
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            Logger.Info($"MatchTemplate Score: {debugKey} {(float)maxVal}");
            stopwatch.Stop();
            Logger.Debug("FindTemplateMatPoint finished in {time} ms", stopwatch.ElapsedMilliseconds);

            // Nếu độ khớp > 0.8 thì coi là tìm thấy
            if (maxVal >= matchValue)
            {
                var topLeft = shouldResize
                    ? new Point((int)(maxLoc.X / scaleFactor),
                        (int)(maxLoc.Y / scaleFactor)) // Chuyển tọa độ về ảnh gốc
                    : maxLoc;

                if (markedScreenshotFileName == null) return topLeft;

                // ✏️ Vẽ hình chữ nhật quanh vùng tìm thấy trên ảnh đã resize hoặc gốc
                CvInvoke.Rectangle(processedScreenshot,
                    new Rectangle(maxLoc, new Size(processedTemplate.Width, processedTemplate.Height)),
                    new MCvScalar(0, 255, 0), 3);

                // 💾 Lưu ảnh kết quả (ảnh resize hoặc ảnh gốc)
                var imagePath = ImageHelper.GetImagePath(markedScreenshotFileName);
                CvInvoke.Imwrite(imagePath, processedScreenshot);
                Logger.Debug($"Saved marked screenshot at path: {imagePath}");

                return topLeft;
            }
        }

        return null;
    }


    // public static Point? FindImageInScreenshot(SKBitmap screenshot, string templatePath, string? markedScreenshotPath)
    // {
    //     try
    //     {
    //         Logger.Info($"FindImageInScreenshot: {templatePath}");
    //         var templateMat = GetMatByPath(templatePath);
    //
    //         return templateMat == null ? null : FindImageInScreenshot(screenshot, templateMat, markedScreenshotPath);
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger.Error(ex, $"Could not load template image from path {templatePath}");
    //     }
    //
    //     return null;
    // }


    // public static Point? FindImageInScreenshot(SKBitmap screenshot, Mat templateMat, string? markedScreenshotPath)
    // {
    //     Logger.Info("FindImageInScreenshot by templateMat");
    //
    //     // Chuyển đổi SKBitmap -> Mat
    //     using var screenshotMat = ConvertSKBitmapToMat(screenshot);
    //
    //     if (screenshotMat == null || screenshotMat.IsEmpty || templateMat.IsEmpty)
    //     {
    //         Logger.Error("Could not convert template mat from screenshot");
    //         return null;
    //     }
    //
    //     return FindTemplateMatPoint(screenshotMat, templateMat, markedScreenshotPath);
    // }

    // private static Mat SKBitmapToMat(SKBitmap bitmap)
    // {
    //     // Convert SKBitmap -> Bitmap
    //     using var bmp = SKBitmapToBitmap(bitmap);
    //
    //     // Convert Bitmap -> Mat (Emgu.CV)
    //     return bmp.ToEmguMat();
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

    // private static Mat? ConvertSKBitmapToMat(SKBitmap skBitmap)
    // {
    //     try
    //     {
    //         // Lấy dữ liệu pixel từ SKBitmap dưới dạng mảng SKColor[]
    //         var width = skBitmap.Width;
    //         var height = skBitmap.Height;
    //         var pixels = skBitmap.Pixels;
    //
    //         // Tạo mảng byte có kích thước đúng (4 bytes cho mỗi pixel)
    //         var pixelData = new byte[width * height * 4]; // 4 byte cho mỗi pixel (RGBA)
    //
    //         // Chuyển đổi dữ liệu từ SKColor[] sang byte[]
    //         for (var i = 0; i < pixels.Length; i++)
    //         {
    //             var pixel = pixels[i];
    //             pixelData[i * 4 + 0] = pixel.Alpha; // Alpha
    //             pixelData[i * 4 + 1] = pixel.Red; // Red
    //             pixelData[i * 4 + 2] = pixel.Green; // Green
    //             pixelData[i * 4 + 3] = pixel.Blue; // Blue
    //         }
    //
    //         // Tạo Mat từ mảng byte
    //         var mat = new Mat(height, width, DepthType.Cv8U, 4); // 4 kênh (RGBA)
    //         mat.SetTo(pixelData);
    //
    //         return mat;
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.Error(e, "Could not convert SKBitmap to mat");
    //     }
    //
    //     return null;
    // }
}