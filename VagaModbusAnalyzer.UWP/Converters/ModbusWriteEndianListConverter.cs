using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.Protocols.Modbus.Data;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class ModbusWriteEndianListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.To<bool>())
            {
                return new ModbusEndian[]
                {
                    ModbusEndian.AllBig,
                    new ModbusEndian(false, true),
                    new ModbusEndian(true, false),
                    ModbusEndian.AllLittle
                };
            }
            else
            {
                return new ModbusEndian[]
                {
                    ModbusEndian.AllBig,
                    ModbusEndian.AllLittle
                };
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class ModbusWriteEndianListConverterExtension : MarkupExtension
    {
        protected override object ProvideValue() => new ModbusWriteEndianListConverter();
    }
}
