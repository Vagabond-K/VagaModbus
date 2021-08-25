using VagabondK.Protocols;
using VagabondK.Protocols.Logging;
using VagabondK.Protocols.Modbus;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ChannelLogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ChannelOpenTemplate { get; set; }
        public DataTemplate ChannelCloseTemplate { get; set; }
        public DataTemplate ModbusExceptionTemplate { get; set; }
        public DataTemplate ChannelRequestTemplate { get; set; }
        public DataTemplate ChannelResponseTemplate { get; set; }
        public DataTemplate UnrecognizedErrorTemplate { get; set; }
        public DataTemplate ErrorCodeTemplate { get; set; }
        public DataTemplate ErrorMessageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ChannelOpenEventLog)
                return ChannelOpenTemplate ?? base.SelectTemplateCore(item, container);
            else if (item is ChannelCloseEventLog)
                return ChannelCloseTemplate ?? base.SelectTemplateCore(item, container);
            else if (item is ModbusExceptionLog)
                return ModbusExceptionTemplate ?? base.SelectTemplateCore(item, container);
            else if (item is ChannelRequestLog)
                return ChannelRequestTemplate ?? base.SelectTemplateCore(item, container);
            else if (item is ChannelResponseLog)
                return ChannelResponseTemplate ?? base.SelectTemplateCore(item, container);
            else if (item is UnrecognizedErrorLog)
                return UnrecognizedErrorTemplate ?? base.SelectTemplateCore(item, container);
            else if (item is ChannelErrorLog channelErrorLog)
                if (channelErrorLog.Exception is ErrorCodeException<ModbusCommErrorCode>)
                    return ErrorCodeTemplate ?? base.SelectTemplateCore(item, container);
                else
                    return ErrorMessageTemplate ?? base.SelectTemplateCore(item, container);

            return base.SelectTemplateCore(item, container);
        }
    }
}
