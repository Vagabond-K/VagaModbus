using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public static bool GetBooleanValue(object value)
        {
            if (value is bool)
                return (bool)value;
            if (value is bool?)
            {
                bool? nullable = (bool?)value;
                return nullable ?? false;
            }

            return false;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (GetBooleanValue(value))
                return Inverse ? Visibility.Collapsed : Visibility.Visible;

            return Inverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            bool booleanValue = (value is Visibility && (Visibility)value == Visibility.Visible) ^ Inverse;
            return booleanValue;
        }
    }

    public class BooleanToVisibilityConverterExtension : MarkupExtension
    {
        public bool Inverse { get; set; }

        protected override object ProvideValue() => new BooleanToVisibilityConverter() { Inverse = Inverse };
    }
}
