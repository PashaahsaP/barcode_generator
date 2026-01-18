using System;
using System.Globalization;
using System.Windows.Data;

namespace barcode_gen.Converter
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if ((bool)value)
                return barcode_gen.Enum.Mode.Parse(targetType, parameter.ToString());

            return Binding.DoNothing;
        }
    }
}
