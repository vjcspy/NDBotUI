using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Emgu.CV;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Values;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Helper;

public enum MoriTemplateKey
{
    StartSettingButton,
}

public class TemplateImageData(string fileName, Mat? templateMat = null, bool isLoadError = false)
{
    public string FileName { get; } = fileName;
    public Mat? TemplateMat { get; set; } = templateMat;
    public bool IsLoadError { get; set; } = isLoadError;
}

public static class TemplateImageDataHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static bool IsLoaded = false;

    public static Dictionary<MoriTemplateKey, TemplateImageData> TemplateImageData = new()
    {
        {
            MoriTemplateKey.StartSettingButton, new TemplateImageData("start_setting_button.png")
        }
    };

    public static Unit LoadTemplateImages()
    {
        var FolderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Resources",
            "game",
            "mementomori",
            "image-detector"
        );
        FileHelper.CreateFolderIfNotExist(CoreValue.ScreenShotFolder);
        FileHelper.CreateFolderIfNotExist(FolderPath);

        if (IsLoaded) return Unit.Default;

        Logger.Info($"Loading template images for Memento Mori");
        foreach (var moriTemplateKey in TemplateImageData.Keys.ToList())
        {
            var imagePath = Path.Combine(FolderPath, TemplateImageData[moriTemplateKey].FileName);
            try
            {
                var mat = ImageProcessingHelper.GetMatByPath(imagePath);
                if (mat == null)
                {
                    TemplateImageData[moriTemplateKey].IsLoadError = true;
                }
                else
                {
                    TemplateImageData[moriTemplateKey].TemplateMat = mat;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to load template image for Memento Mori with key {moriTemplateKey}");
                TemplateImageData[moriTemplateKey].IsLoadError = true;
            }
        }

        Logger.Info($"Loaded template images for Memento Mori");

        return Unit.Default;
    }
}