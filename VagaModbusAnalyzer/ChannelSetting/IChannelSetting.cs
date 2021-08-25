using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.ChannelSetting
{
    public interface IChannelSetting
    {
        ChannelType ChannelType { get; }
        IChannelSetting Copy();
    }
}
