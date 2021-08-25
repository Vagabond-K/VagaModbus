using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using static VagaModbusAnalyzer.ChannelSetting.SerialPortChannelSetting;

namespace VagaModbusAnalyzer.Converters
{
    public class SerialHandshakeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SerialHandshake serialHandshake)
            {
                switch (serialHandshake)
                {
                    case SerialHandshake.None:
                        return "None";
                    case SerialHandshake.RequestToSend:
                        return "RTS";
                    case SerialHandshake.XOnXOff:
                        return "XOn/XOff";
                    case SerialHandshake.RequestToSendXOnXOff:
                        return "RTS, XOn/XOff";
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
