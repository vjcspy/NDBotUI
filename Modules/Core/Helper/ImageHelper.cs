using System;
using System.IO;
using NDBotUI.Modules.Core.Values;

namespace NDBotUI.Modules.Core.Helper;

public class ImageHelper
{
    public static string GetImagePath(string filename, string? folder = null)
    {
        // Thay thế dấu phân tách thư mục '/' hoặc '\' bằng Path.DirectorySeparatorChar
        folder = folder
            ?.Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        // Kết hợp thư mục gốc với folder (nếu có)
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), folder ?? CoreValue.ScreenShotFolder);

        // Kiểm tra và tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Tạo tên tệp với dấu thời gian
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(folderPath, $"{filename}_{timestamp}.png");
    }
}