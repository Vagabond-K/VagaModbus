using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Infrastructures;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class EnumToLocalizedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum)
            {
                var key = $"{value.GetType().ToString().Replace('.', '/')}/{value}";
                return StringLocalizer.GetString(key);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToLocalizedStringConverterExtension : MarkupExtension
    {
        protected override object ProvideValue()
        {
            return new EnumToLocalizedStringConverter();
        }
    }
}
