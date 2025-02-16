using System;
using System.IO;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
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
}