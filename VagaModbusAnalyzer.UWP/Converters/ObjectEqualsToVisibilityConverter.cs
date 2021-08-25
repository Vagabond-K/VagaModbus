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
    public class ObjectEqualsToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum)
            {
                if (parameter is string paramString)
                    parameter = Enum.Parse(value.GetType(), paramString);
                else
                    parameter = Enum.ToObject(value.GetType(), parameter);
            }

            return Equals(value, parameter) != Inverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectEqualsToVisibilityConverterExtension : MarkupExtension
    {
        public bool Inverse { get; set; }

        protected override object ProvideValue() => new ObjectEqualsToVisibilityConverter { Inverse = Inverse };
    }
}
