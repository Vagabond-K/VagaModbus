using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Infrastructures;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum)
            {
                var name = value.ToString();
                var member = value.GetType().GetMember(name, BindingFlags.Static | BindingFlags.Public);
                DescriptionAttribute descriptionAttribute = member.FirstOrDefault()?.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;

                return descriptionAttribute?.Description ?? name;
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToDescriptionConverterExtension : MarkupExtension
    {
        protected override object ProvideValue()
        {
            return new EnumToDescriptionConverter();
        }
    }
}
