using System.Collections.Generic;
using DynamicData.Kernel;

namespace NDBotUI.Modules.Game.MementoMori.Helper;
using OpenCVMat = OpenCvSharp.Mat;
using EmuCVMat = Emgu.CV.Mat;
public class TemplateImageData(
    string[] filePath,
    OpenCVMat? openCVMat = null,
    EmuCVMat? emuCvMat = null,
    bool isLoadError = false,
    int priority = 100
)
{
    private readonly int _defaultPriority = priority;
    private readonly Dictionary<string, int> PriorityDict = new();
    public string[] FilePath { get; } = filePath;
    public OpenCVMat? OpenCVMat { get; set; } = openCVMat;
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