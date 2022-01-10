using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Converters
{
    public class DecimalToByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return decimal.ToByte(decimalValue);
            }
            return (byte)0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is byte byteValue)
            {
                return System.Convert.ToDecimal(byteValue);
            }
            return 0M;
        }
    }
}
