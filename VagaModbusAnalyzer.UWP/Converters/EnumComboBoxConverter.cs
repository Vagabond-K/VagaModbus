using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class EnumComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum)
            {
                var type = Enum.GetUnderlyingType(value.GetType());
                
                return System.Convert.ChangeType(value, type);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class EnumComboBoxConverterExtension : MarkupExtension
    {
        protected override object ProvideValue()
        {
            return new EnumComboBoxConverter();
        }
    }

    public class EnumComboBoxItem
    {
        public string Display { get; set; }
        public object Value { get; set; }
    }
}
