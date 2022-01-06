using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.Protocols.Modbus.Data;
using VagaModbusAnalyzer.Infrastructures;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class ModbusEndianToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ModbusEndian modbusEndian)
            {
                if (modbusEndian.OuterBigEndian)
                {
                    if (modbusEndian.InnerBigEndian) return StringLocalizer.GetString("Text_Big_endian/Text") + " (ABCD)";
                    else return StringLocalizer.GetString("Text_Mixed_endian/Text") + " (BADC)";
                }
                else
                {
                    if (modbusEndian.InnerBigEndian) return StringLocalizer.GetString("Text_Mixed_endian/Text") + " (CDAB)";
                    else return StringLocalizer.GetString("Text_Little_endian/Text") + " (DCBA)";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class ModbusEndianToTextConverterExtension : MarkupExtension
    {
        protected override object ProvideValue() => new ModbusEndianToTextConverter();
    }
}
