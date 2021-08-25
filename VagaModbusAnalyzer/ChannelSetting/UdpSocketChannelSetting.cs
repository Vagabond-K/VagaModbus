using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Channels;

namespace VagaModbusAnalyzer.ChannelSetting
{
    public class UdpSocketChannelSetting : NotifyPropertyChangeObject, IChannelSetting
    {
        public string Host { get => Get<string>(); set => Set(value); }
        public int RemotePort { get => Get(502); set => Set(value); }
        public int? LocalPort { get => Get<int?>(); set => Set(value); }

        public ChannelType ChannelType => ChannelType.UdpSocket;
        public IChannelSetting Copy() => new UdpSocketChannelSetting
        {
            Host = Host,
            RemotePort = RemotePort,
            LocalPort = LocalPort,
        };
    }
}
