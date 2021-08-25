using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Channels;
using VagaModbusAnalyzer.ChannelSetting;

namespace VagaModbusAnalyzer
{
    public interface IChannelFactory
    {
        IChannel CreateChannel(IChannelSetting channelSetting);
    }
}
