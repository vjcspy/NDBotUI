using System;
using System.IO;
using NLog;
using NLog.Fluent;
using Tesseract;

namespace NDBotUI.Modules.Game.Ete.Helper;

public class EteHelperData
{

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static void Test()
    {
        Logger.Info("Start test OCR");
        var folderPath = @"Resources\game\ete\screen-detector";
        var currentDirectory = Directory.GetCurrentDirectory();
        var imagePath = Path.Combine(currentDirectory, folderPath, $"test.png");
        // Khởi tạo OCR với bộ dữ liệu ngôn ngữ English
        using (var engine = new TesseractEngine(@"./Resources/tessdata", "eng", EngineMode.Default))
        {
            using (var img = Pix.LoadFromFile(imagePath))
            {
                using (var page = engine.Process(img))
                {
                    string text = page.GetText();

                    // Lọc chỉ lấy số từ kết quả OCR
                    string numbersOnly = System.Text.RegularExpressions.Regex.Replace(text, "[^0-9]", "");

                    Logger.Info("Detected number " + numbersOnly);
                }
            }
        }
    }
}