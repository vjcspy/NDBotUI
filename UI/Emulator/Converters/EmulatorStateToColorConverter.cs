using System;
using System.Globalization;
using AdvancedSharpAdbClient.Models;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NDBotUI.Modules.Core.Store;

namespace NDBotUI.UI.Emulator.Converters;

public class EmulatorStateToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string emulatorId)
        {
            var emulatorConnection = AppStore.Instance.EmulatorStore.State.GetEmulatorConnection(emulatorId);

            if (emulatorConnection is { } g)
            {
                return g.State switch
                {
                    DeviceState.Offline => Brushes.Red,
                    DeviceState.Online => Brushes.Green,
                    _ => Brushes.Gray,
                };
            }
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}