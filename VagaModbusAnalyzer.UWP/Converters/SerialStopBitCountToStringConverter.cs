using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using static VagaModbusAnalyzer.ChannelSetting.SerialPortChannelSetting;

namespace VagaModbusAnalyzer.Converters
{
    public class SerialStopBitCountToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SerialStopBits serialStopBitCount)
            {
                switch (serialStopBitCount)
                {
                    case SerialStopBits.One:
                        return "1";
                    case SerialStopBits.One5:
                        return "1.5";
                    case SerialStopBits.Two:
                        return "2";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
