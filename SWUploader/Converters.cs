using System;
using System.Globalization;
using System.Windows.Data;

namespace SWUploader
{
    /// <summary>
    /// Implements IValueConverter for XAML binding purposes.
    /// Converts a selected index (e.g. from a ListBox) to a boolean that indicates whether any item is selected.
    /// An index less than zero means no item is selected.
    /// </summary>
    public class IsItemSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && targetType == typeof(bool))
            {
                return selectedIndex >= 0;
            }

            throw new InvalidOperationException("Invalid conversion.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Converter only performs one-way conversions.");
        }
    }
}
