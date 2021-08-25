using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Channels;

namespace VagaModbusAnalyzer.ChannelSetting
{
    public class TcpServerChannelSetting : NotifyPropertyChangeObject, IChannelSetting
    {
        public int Port { get => Get(502); set => Set(value); }

        public ChannelType ChannelType => ChannelType.TcpServer;
        public IChannelSetting Copy() => new TcpServerChannelSetting { Port = Port };
    }
}
