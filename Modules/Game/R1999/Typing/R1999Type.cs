namespace NDBotUI.Modules.Game.R1999.Typing;

public enum R1999JobType
{
    None,
    ReRoll,
}

public enum ScreenDetectedType
{
    OpenCV,
    PixelColor,
}

public record CurrentScreen(string ScreenName = "Unknown", ScreenDetectedType ScreenDetectedType = ScreenDetectedType.OpenCV)
{
}