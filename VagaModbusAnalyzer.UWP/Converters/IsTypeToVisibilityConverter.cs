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
    public class IsTypeToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Inverse ? Visibility.Visible : Visibility.Collapsed;

            if (parameter is string typeString)
            {
                var type = Type.GetType(typeString);
                if (type != null)
                    return type.IsAssignableFrom(value.GetType()) != Inverse ? Visibility.Visible : Visibility.Collapsed;
                else throw new ArgumentException(nameof(parameter));
            }
            else throw new ArgumentException(nameof(parameter));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IsTypeToVisibilityConverterExtension : MarkupExtension
    {
        public bool Inverse { get; set; }

        protected override object ProvideValue() => new IsTypeToVisibilityConverter { Inverse = Inverse };
    }
}
