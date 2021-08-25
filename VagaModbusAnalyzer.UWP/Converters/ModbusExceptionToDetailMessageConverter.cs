using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Infrastructures;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class ModbusExceptionToDetailMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum)
            {
                var key = $"ModbusExceptionDetails/{value}";
                return StringLocalizer.GetString(key);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class ModbusExceptionToDetailMessageConverterExtension : MarkupExtension
    {
        protected override object ProvideValue()
        {
            return new ModbusExceptionToDetailMessageConverter();
        }
    }
}
