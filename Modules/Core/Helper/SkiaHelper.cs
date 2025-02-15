using System;
using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using NDBotUI.Modules.Shared.Emulator.Models;
using NLog;
using SkiaSharp;

namespace NDBotUI.Modules.Core.Helper;

public class SkiaHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task<Unit> SaveScreenshot(EmulatorConnection emulatorConnection, string[] filePaths)
    {
        try
        {
            var screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot == null) return Unit.Default;
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
}