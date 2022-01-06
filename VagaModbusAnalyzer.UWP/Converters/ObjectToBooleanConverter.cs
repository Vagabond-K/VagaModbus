using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class ObjectToBooleanConverter : MarkupExtension, IValueConverter
    {
        public bool Inverse { get; set; }
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

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
            if (Equals(value, TrueValue)) return !Inverse;
            else if (Equals(value, FalseValue)) return Inverse;
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Inverse ? TrueValue : FalseValue;

            if (GetBooleanValue(value))
                return Inverse ? FalseValue : TrueValue;

            return Inverse ? TrueValue : FalseValue;
        }

        protected override object ProvideValue()
        {
            return this;
        }
    }

    public class ObjectToBooleanConverterExtension : MarkupExtension
    {
        public bool Inverse { get; set; }
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        protected override object ProvideValue() => new ObjectToBooleanConverter()
        {
            Inverse = Inverse,
            TrueValue = TrueValue,
            FalseValue = FalseValue
        };
    }

}
