using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MySummerCarMusicManager.NET.Helpers;

public sealed class AssetBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconName && parameter is string size)
        {
            try
            {
                var path = $"avares://MySummerCarMusicManager.NET/Icons/{iconName}-{size}.png";
                var uri = new Uri(path);

                if (AssetLoader.Exists(uri))
                {
                    return new Bitmap(AssetLoader.Open(uri));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
