using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VagaModbusAnalyzer
{
    public enum ModbusType
    {
        [Description("TCP")]
        TCP = 0,
        [Description("RTU")]
        RTU = 1,
        [Description("ASCII")]
        ASCII = 2,
    }

    public enum ChannelType
    {
        [Description("None")]
        None = 0,
        [Description("TCP Client")]
        TcpClient = 1,
        [Description("TCP Server")]
        TcpServer = 2,
        [Description("UDP Socket")]
        UdpSocket = 3,
        [Description("Serial Port")]
        SerialPort = 4,
    }
}
