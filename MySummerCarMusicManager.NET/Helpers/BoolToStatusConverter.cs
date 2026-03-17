using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MySummerCarMusicManager.NET.Helpers;

public sealed class BoolToStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isReady)
        {
            return isReady ? "Ready" : "Sync Req.";
        }
        return "Unknown";
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
