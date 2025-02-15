using System.IO;

namespace NDBotUI.Modules.Core.Helper;

public static class FileHelper
{
    private static string? CurrentDirectory;

    public static string CreateFolderIfNotExist(string path)
    {
        CurrentDirectory ??= Directory.GetCurrentDirectory();

        var folderPath = Path.Combine(CurrentDirectory, path);

        // Tạo thư mục nếu chưa có
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        return folderPath;
    }

    public static string getFolderPath(string[] paths)
    {
        CurrentDirectory ??= Directory.GetCurrentDirectory();

        return Path.Combine(
            CurrentDirectory,
            Path.Combine(paths)
        );
    }
}