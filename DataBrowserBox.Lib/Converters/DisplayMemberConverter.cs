using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace DataBrowserBox.Lib.Converters
{
    internal class DisplayMemberConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object selectedItem = null;
            string displayMemberPath = null;

            if (values != null)
            {
                if (values.Length > 0 && values[0] != DependencyProperty.UnsetValue)
                    selectedItem = values[0];

                if (values.Length > 1 && values[1] != DependencyProperty.UnsetValue)
                    displayMemberPath = values[1] as string;
            }

            if (selectedItem == null)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(displayMemberPath))
                return selectedItem.ToString() ?? string.Empty;

            var prop = selectedItem.GetType().GetProperty(displayMemberPath, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return string.Empty;

            var propValue = prop.GetValue(selectedItem);
            return propValue?.ToString() ?? string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}