using System;
using System.IO;
using NDBotUI.Modules.Core.Values;

namespace NDBotUI.Modules.Core.Helper;

public class ImageHelper
{
    public static string GetImagePath(string filename)
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), CoreValue.ScreenShotFolder);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(folderPath, $"{filename}_{timestamp}.png");
    }
}