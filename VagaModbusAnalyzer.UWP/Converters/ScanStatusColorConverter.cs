using System;
using VagabondK.Protocols;
using VagabondK.Protocols.Modbus;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Converters
{
    public class ScanStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ErrorCodeException<ModbusExceptionCode>)
                return Color.FromArgb(0x20, 0xFF, 0x80, 0x00);
            if (value is Exception)
                return Color.FromArgb(0x20, 0xFF, 0x00, 0x00);
            else if (value is string)
                return Color.FromArgb(0x20, 0x00, 0x80, 0x00);
            return Color.FromArgb(0x10, 0x80, 0x80, 0x80);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
