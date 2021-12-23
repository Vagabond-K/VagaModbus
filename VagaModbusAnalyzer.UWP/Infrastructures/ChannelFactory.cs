using System;
using System.IO.Ports;
using VagabondK.Protocols.Channels;
using VagaModbusAnalyzer.ChannelSetting;

namespace VagaModbusAnalyzer.Infrastructures
{
    [ServiceDescription(typeof(IChannelFactory))]
    public class ChannelFactory : IChannelFactory
    {
        public IChannel CreateChannel(IChannelSetting channelSetting)
        {
            if (channelSetting != null)
            {
                switch (channelSetting.ChannelType)
                {
                    case ChannelType.TcpClient:
                        return CreateChannel(channelSetting as TcpClientChannelSetting);
                    case ChannelType.TcpServer:
                        return CreateChannel(channelSetting as TcpServerChannelSetting);
                    case ChannelType.UdpSocket:
                        return CreateChannel(channelSetting as UdpSocketChannelSetting);
                    case ChannelType.SerialPort:
                        return CreateChannel(channelSetting as SerialPortChannelSetting);
                }
            }
            return null;
        }

        public TcpChannel CreateChannel(TcpClientChannelSetting channelSetting) => new TcpChannel(channelSetting.Host, channelSetting.Port, channelSetting.ConnectTimeout);

        public TcpChannelProvider CreateChannel(TcpServerChannelSetting channelSetting) => new TcpChannelProvider(channelSetting.Port);

        public UdpChannel CreateChannel(UdpSocketChannelSetting channelSetting)
            => channelSetting.LocalPort == null 
            ? new UdpChannel(channelSetting.Host, channelSetting.RemotePort)
            : new UdpChannel(channelSetting.Host, channelSetting.RemotePort, channelSetting.LocalPort.Value);

        public SerialPortChannel CreateChannel(SerialPortChannelSetting channelSetting)
        {
            StopBits stopBitCount = StopBits.One;
            switch (channelSetting.StopBits)
            {
                case SerialPortChannelSetting.SerialStopBits.One5:
                    stopBitCount = StopBits.OnePointFive;
                    break;
                case SerialPortChannelSetting.SerialStopBits.Two:
                    stopBitCount = StopBits.Two;
                    break;
            }

            Parity parity = Parity.None;
            switch (channelSetting.Parity)
            {
                case SerialPortChannelSetting.SerialParity.Even:
                    parity = Parity.Even;
                    break;
                case SerialPortChannelSetting.SerialParity.Odd:
                    parity = Parity.Odd;
                    break;
                case SerialPortChannelSetting.SerialParity.Mark:
                    parity = Parity.Mark;
                    break;
                case SerialPortChannelSetting.SerialParity.Space:
                    parity = Parity.Space;
                    break;
            }

            Handshake handshake = Handshake.None;
            switch (channelSetting.Handshake)
            {
                case SerialPortChannelSetting.SerialHandshake.XOnXOff:
                    handshake = Handshake.XOnXOff;
                    break;
                case SerialPortChannelSetting.SerialHandshake.RequestToSend:
                    handshake = Handshake.RequestToSend;
                    break;
                case SerialPortChannelSetting.SerialHandshake.RequestToSendXOnXOff:
                    handshake = Handshake.RequestToSendXOnXOff;
                    break;
            }

            return new SerialPortChannel(channelSetting.PortName, channelSetting.BaudRate, channelSetting.DataBits, stopBitCount, parity, handshake)
            {
                DtrEnable = channelSetting.DtrEnable,
                RtsEnable = channelSetting.RtsEnable
            };
        }

    }
}
