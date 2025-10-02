using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace idSaveDataResignerWpf.Converters;

public class FileNameWithoutExtensionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrEmpty(s))
            return Path.GetFileNameWithoutExtension(s);
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}