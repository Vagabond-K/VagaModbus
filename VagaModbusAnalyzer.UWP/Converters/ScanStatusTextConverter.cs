using System;
using VagabondK.Protocols;
using VagabondK.Protocols.Modbus;
using VagaModbusAnalyzer.Infrastructures;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Converters
{
    public class ScanStatusTextConverter : IValueConverter
    {
        private static EnumToLocalizedStringConverter enumToLocalizedStringConverter = new EnumToLocalizedStringConverter();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ErrorCodeException<ModbusCommErrorCode> errorCodeException)
                return $"{enumToLocalizedStringConverter.Convert(errorCodeException.Code, typeof(string), null, null)}";
            else if (value is Exception exception)
                return $"{exception.Message}";
            else if (value is string text)
                return StringLocalizer.GetString(text);
            else
                return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
