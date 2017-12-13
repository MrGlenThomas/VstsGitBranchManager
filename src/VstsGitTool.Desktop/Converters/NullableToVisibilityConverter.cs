using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VstsGitTool.Desktop.Converters
{
    public class NullableToVisibilityConverter : IValueConverter
    {
        public Visibility NullVisibility { get; set; }
        public Visibility NotNullVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullVisibility : NotNullVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
