using System;
using System.IO;
using Emgu.CV;
using NLog;
using OpenCvSharp;
using Mat = OpenCvSharp.Mat;
using Point = System.Drawing.Point;

namespace NDBotUI.Modules.Core.Helper;

public static class ImageFinderOpenCvSharp
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static Mat? GetMatByPath(string imagePath)
    {
        try
        {
            // Kiểm tra xem file template có tồn tại không
            if (!File.Exists(imagePath))
            {
                Logger.Error($"Template image not found in path {imagePath}");
                throw new FileNotFoundException($"Template image not found in path {imagePath}");
            }

            var templateMat = Cv2.ImRead(imagePath, ImreadModes.Grayscale);

            return templateMat;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to load template image");
            return null;
        }
    }

    public static Point? FindTemplateInScreenshot(Mat screenshot, Mat template, double threshold = 0.8)
    {
        if (screenshot.Width < template.Width || screenshot.Height < template.Height)
            throw new ArgumentException("Template width and height must be smaller than template");

        // Cv2.CvtColor(screenshot, screenshot, ColorConversionCodes.BGR2GRAY);

        // Đảm bảo ảnh template ở dạng grayscale (nếu chưa)
        // Cv2.CvtColor(template, template, ColorConversionCodes.BGR2GRAY);

        using (Mat result = new Mat())
        {
            // So sánh template với ảnh lớn
            Cv2.MatchTemplate(screenshot, template, result, TemplateMatchModes.CCoeffNormed);

            OpenCvSharp.Point maxLoc;
            // Lấy giá trị khớp tốt nhất và vị trí của nó
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out maxLoc);
            Logger.Info($"MaxLoc {maxVal}");
            // Kiểm tra nếu độ tương đồng lớn hơn ngưỡng threshold
            return maxVal >= threshold ? new Point(maxLoc.X, maxLoc.Y) : null;
        }
    }
}