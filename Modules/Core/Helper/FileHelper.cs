using System.IO;

namespace NDBotUI.Modules.Core.Helper;

public static class FileHelper
{
    public static string CreateFolderIfNotExist(string path)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), path);

        // Tạo thư mục nếu chưa có
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        return folderPath;
    }
}