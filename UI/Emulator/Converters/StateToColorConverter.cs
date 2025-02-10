using System;
using System.Globalization;
using AdvancedSharpAdbClient.Models;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace NDBotUI.UI.Emulator.Converters;

public class StateToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DeviceState.Offline => Brushes.Red,
            DeviceState.Online => Brushes.Green,
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}