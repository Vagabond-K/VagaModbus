using VagabondK.Protocols;
using VagabondK.Protocols.Logging;
using VagabondK.Protocols.Modbus;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ChannelLogStyleSelector : StyleSelector
    {
        public Style ChannelOpenStyle { get; set; }
        public Style ChannelCloseStyle { get; set; }
        public Style ModbusExceptionStyle { get; set; }
        public Style ChannelRequestStyle { get; set; }
        public Style ChannelResponseStyle { get; set; }
        public Style UnrecognizedErrorStyle { get; set; }
        public Style RequestErrorStyle { get; set; }
        public Style ErrorCodeStyle { get; set; }
        public Style ErrorMessageStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is ChannelOpenEventLog)
                return ChannelOpenStyle ?? base.SelectStyleCore(item, container);
            else if (item is ChannelCloseEventLog)
                return ChannelCloseStyle ?? base.SelectStyleCore(item, container);
            else if (item is ModbusExceptionLog)
                return ModbusExceptionStyle ?? base.SelectStyleCore(item, container);
            else if (item is ChannelRequestLog)
                return ChannelRequestStyle ?? base.SelectStyleCore(item, container);
            else if (item is ChannelResponseLog)
                return ChannelResponseStyle ?? base.SelectStyleCore(item, container);
            else if (item is UnrecognizedErrorLog)
                return UnrecognizedErrorStyle ?? base.SelectStyleCore(item, container);
            else if (item is ChannelErrorLog channelErrorLog)
                if (channelErrorLog.Exception is RequestException<ModbusCommErrorCode> requestException && requestException.ReceivedBytes != null && requestException.ReceivedBytes.Count > 0)
                    return RequestErrorStyle ?? ErrorCodeStyle ?? base.SelectStyleCore(item, container);
                else if (channelErrorLog.Exception is ErrorCodeException<ModbusCommErrorCode>)
                    return ErrorCodeStyle ?? base.SelectStyleCore(item, container);
                else
                    return ErrorMessageStyle ?? base.SelectStyleCore(item, container);

            return base.SelectStyleCore(item, container);
        }
    }
}
