using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Channels;

namespace VagaModbusAnalyzer.ChannelSetting
{
    public class TcpClientChannelSetting : NotifyPropertyChangeObject, IChannelSetting
    {
        public string Host { get => Get<string>(); set => Set(value); }
        public int Port { get => Get(502); set => Set(value); }
        public int ConnectTimeout { get => Get(1000); set => Set(value); }

        public ChannelType ChannelType => ChannelType.TcpClient;
        public IChannelSetting Copy() => new TcpClientChannelSetting
        {
            Host = Host,
            Port = Port,
            ConnectTimeout = ConnectTimeout,
        };
    }
}
