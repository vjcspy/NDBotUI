using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NDBotUI.Modules.Core.Store;

namespace NDBotUI.UI.Emulator.Converters;

public class EmulatorConnectionStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string emulatorId)
        {
            var emulatorConnection = AppStore.Instance.EmulatorStore.State.GetEmulatorConnection(emulatorId);

            if (emulatorConnection is { } g)
            {
                return emulatorConnection.State.ToString();
            }
        }

        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}