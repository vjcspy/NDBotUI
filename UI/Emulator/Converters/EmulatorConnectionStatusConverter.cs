using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NDBotUI.UI.Emulator.Converters;

public class EmulatorConnectionStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}