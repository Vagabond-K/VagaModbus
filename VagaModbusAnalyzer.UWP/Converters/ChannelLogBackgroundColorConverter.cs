using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.Protocols.Logging;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VagaModbusAnalyzer.Converters
{
    public class ChannelLogBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;

            if (value is ChannelErrorLog || value is UnrecognizedErrorLog)
                return Color.FromArgb(0x20, 0xFF, 0x00, 0x00);
            else if (value is ModbusExceptionLog)
                return Color.FromArgb(0x20, 0xFF, 0x80, 0x00);
            else if (value is ChannelResponseLog)
                return Color.FromArgb(0x20, 0x00, 0x80, 0x00);
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ChannelLogBackgroundColorConverterExtension : MarkupExtension
    {
        public bool Inverse { get; set; }

        protected override object ProvideValue() => new ChannelLogBackgroundColorConverter();
    }
}
