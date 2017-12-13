using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VstsGitTool.Desktop.Converters
{
    public class BooleanToVisibilityConverterEx : IValueConverter
    {
        public Visibility TrueVisibility { get; set; }
        public Visibility FalseVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return value;

            var boolValue = (bool)value;

            return boolValue ? TrueVisibility : FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
