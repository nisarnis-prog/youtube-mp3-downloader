using System.Globalization;
using System.Windows.Data;

namespace YouTube_MP3_Downloader.Converters;

public sealed class RowNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is int index ? (index + 1).ToString(culture) : "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}