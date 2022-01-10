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
    public class NegationConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value == DependencyProperty.UnsetValue) return value;
            if (value == DBNull.Value) return null;
            if (!IsBooleanConvertable(value)) return DependencyProperty.UnsetValue;

            return !value.To<bool>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value == DependencyProperty.UnsetValue) return value;
            if (value == DBNull.Value) return null;
            if (!IsBooleanConvertable(value)) return DependencyProperty.UnsetValue;

            return !value.To<bool>();
        }

        protected override object ProvideValue()
        {
            return this;
        }

        private static bool IsBooleanConvertable(object value)
            => value is bool
            || value is sbyte
            || value is short
            || value is int
            || value is long
            || value is float
            || value is double
            || value is byte
            || value is ushort
            || value is uint
            || value is ulong
            || value is string
            || value is decimal;

    }
}
