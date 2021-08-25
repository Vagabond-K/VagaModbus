using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.ChannelSetting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VagaModbusAnalyzer.Infrastructures
{
    class ChannelSettingEditTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TcpClientDataTemplate { get; set; }
        public DataTemplate TcpServerDataTemplate { get; set; }
        public DataTemplate UdpSocketDataTemplate { get; set; }
        public DataTemplate SerialPortDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TcpClientChannelSetting)
                return TcpClientDataTemplate;
            else if (item is TcpServerChannelSetting)
                return TcpServerDataTemplate;
            else if (item is UdpSocketChannelSetting)
                return UdpSocketDataTemplate;
            else if (item is SerialPortChannelSetting)
                return SerialPortDataTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
