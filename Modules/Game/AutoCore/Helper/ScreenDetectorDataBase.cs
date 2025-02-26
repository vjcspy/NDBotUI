﻿using System;
using System.Collections.Generic;
using System.IO;
using DynamicData.Kernel;
using Emgu.CV;
using NDBotUI.Modules.Core.Helper;
using NLog;

namespace NDBotUI.Modules.Game.AutoCore.Helper;

using EmuCVMat = Mat;

public class DetectPixelColorData(int px, int py, string hex, int priority, bool isLoadError)
{
    public int PX { get; set; } = px;
    public int PY { get; set; } = py;
    public string Hex { get; set; } = hex;
    public int Priority { get; set; } = priority;
    public bool IsLoadError { get; set; } = isLoadError;
}

public class DetectTemplateImageData(
    EmuCVMat? emuCvMat = null,
    bool isLoadError = false,
    int priority = 100
)
{
    private readonly int _defaultPriority = priority;
    private readonly Dictionary<string, int> PriorityDict = new();
    public EmuCVMat? EmuCVMat { get; set; } = emuCvMat;
    public bool IsLoadError { get; set; } = isLoadError;

    public int Priority { get; set; } = priority;

    public int GetPriority(string emulatorId = "default")
    {
        return emulatorId == "default"
            ? _defaultPriority
            : PriorityDict.GetValueOrDefault(emulatorId, _defaultPriority);
    }

    public void SetPriority(string emulatorId, int priority)
    {
        PriorityDict[emulatorId] = priority;
    }

    public void Reset(string emulatorId)
    {
        PriorityDict.RemoveIfContained(emulatorId);
    }
}

public record OverrideScreenData(
    string[]? FilePath,
    int? Priority = 100
);

public class ScreenDetectorDataBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static bool IsLoaded;
    protected virtual string FolderPath { get => @"Resources\r1999\screen-detector"; }

    public virtual Dictionary<string, OverrideScreenData> OverrideScreen { get; set; } = new();

    public virtual Enum[] TemplateKeys { get; set; } = [];
    public virtual Enum[] PixelColorKeys { get; set; } = [];

    protected Dictionary<string, DetectTemplateImageData> TemplateImageData { get; set; } = new();
    protected Dictionary<string, DetectPixelColorData> PixelColorData { get; set; } = new();

    public static ScreenDetectorDataBase GetInstance()
    {
        return new ScreenDetectorDataBase();
    }

    public void LoadData()
    {
        LoadTemplateImages();
    }

    private void LoadTemplateImages()
    {
        if (IsLoaded)
        {
            return;
        }

        Logger.Info("Loading template images for screen detector");
        foreach (var templateKey in TemplateKeys)
        {
            var imagePath = Path.Combine(FolderPath, "template", $"{templateKey}.png");
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
                    TemplateImageData[templateKey.ToString()].IsLoadError = true;
                    Logger.Error($"Failed to load template image for {templateKey}");
                }
                else
                {
                    TemplateImageData[templateKey.ToString()].EmuCVMat = emuCVMat;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to load template image with key {templateKey}");
                TemplateImageData[templateKey.ToString()].IsLoadError = true;
            }
        }

        IsLoaded = true;
        Logger.Info("Loaded template images for screen detector");
    }

    private void ResetTemplateImagesPriority(string emulatorId)
    {
        foreach (var kvp in TemplateImageData)
            // Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
        {
            kvp.Value.Reset(emulatorId);
        }
    }
}