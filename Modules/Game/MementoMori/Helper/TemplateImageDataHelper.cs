using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Values;
using NDBotUI.Modules.Game.MementoMori.Store;
using NLog;
using OpenCVMat = OpenCvSharp.Mat;
using EmuCVMat = Emgu.CV.Mat;

namespace NDBotUI.Modules.Game.MementoMori.Helper;

public class TemplateImageData(
    string[] filePath,
    OpenCVMat? openCVMat = null,
    EmuCVMat? emuCvMat = null,
    bool isLoadError = false,
    int priority = 100)
{
    public string[] FilePath { get; } = filePath;
    public OpenCVMat? OpenCVMat { get; set; } = openCVMat;
    public EmuCVMat? EmuCVMat { get; set; } = emuCvMat;
    public bool IsLoadError { get; set; } = isLoadError;

    public int Priority { get; } = priority;
}

public static class TemplateImageDataHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static bool IsLoaded;

    public static Dictionary<MoriTemplateKey, TemplateImageData> TemplateImageData = new()
    {
        {
            MoriTemplateKey.StartSettingButton,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "start_setting_button.png"
            ])
        },
        {
            MoriTemplateKey.SkipMovieButton,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "skip_movie_button.png"
            ], priority: 90)
        },
        {
            MoriTemplateKey.StartStartButton,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "start_start_button.png"
            ])
        },
        {
            MoriTemplateKey.IconSpeakBeginningFirst,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "icon_speak_beginning_first.png"
            ])
        },
        {
            MoriTemplateKey.ChallengeButton,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "challenge_button.png"
            ])
        },


        {
            MoriTemplateKey.TextSelectFirstCharToTeam,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "text_select_first_char_to_team.png"
            ], priority: 10)
        },
        {
            MoriTemplateKey.TextSelectSecondCharToTeam,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "text_select_second_char_to_team.png"
            ], priority: 10)
        },
        {
            MoriTemplateKey.TextSelectThirdCharToTeam,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "text_select_third_char_to_team.png"
            ], priority: 10)
        },
        {
            MoriTemplateKey.TextSelectFourCharToTeam,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "text_select_four_char_to_team.png"
            ], priority: 10)
        },


        {
            MoriTemplateKey.PartyInformation,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "party_information.png"
            ], priority: 90)
        },
        {
            MoriTemplateKey.TapToClose,
            new TemplateImageData([
                "Resources", "game", "mementomori", "image-detector", "reroll", "tap_to_close.png"
            ], priority: 90)
        }
    };

    public static Unit LoadTemplateImages()
    {
        FileHelper.CreateFolderIfNotExist(CoreValue.ScreenShotFolder);

        if (IsLoaded) return Unit.Default;

        Logger.Info("Loading template images for Memento Mori");
        foreach (var moriTemplateKey in TemplateImageData.Keys.ToList())
        {
            var imagePath = Path.Combine(FileHelper.getFolderPath(TemplateImageData[moriTemplateKey].FilePath));
            try
            {
                // var openCVMat = ImageFinderOpenCvSharp.GetMatByPath(imagePath);
                // if (openCVMat == null)
                // {
                //     TemplateImageData[moriTemplateKey].IsLoadError = true;
                //     Logger.Error($"Failed to load template image for {moriTemplateKey}");
                // }
                // else
                // {
                //     TemplateImageData[moriTemplateKey].OpenCVMat = openCVMat;
                // }

                var emuCVMat = ImageFinderEmguCV.GetMatByPath(imagePath);
                if (emuCVMat == null)
                {
                    TemplateImageData[moriTemplateKey].IsLoadError = true;
                    Logger.Error($"Failed to load template image for {moriTemplateKey}");
                }
                else
                {
                    TemplateImageData[moriTemplateKey].EmuCVMat = emuCVMat;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to load template image for Memento Mori with key {moriTemplateKey}");
                TemplateImageData[moriTemplateKey].IsLoadError = true;
            }
        }

        IsLoaded = true;
        Logger.Info("Loaded template images for Memento Mori");

        return Unit.Default;
    }
}