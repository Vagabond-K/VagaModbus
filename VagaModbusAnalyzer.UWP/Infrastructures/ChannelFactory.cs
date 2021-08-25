using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.Protocols.Channels;
using VagaModbusAnalyzer.ChannelSetting;
using Windows.Devices.SerialCommunication;

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

        public TcpChannel CreateChannel(TcpClientChannelSetting channelSetting) => new TcpChannel(channelSetting.Host, channelSetting.Port);

        public TcpChannelProvider CreateChannel(TcpServerChannelSetting channelSetting) => new TcpChannelProvider(channelSetting.Port);

        public UdpChannel CreateChannel(UdpSocketChannelSetting channelSetting)
            => channelSetting.LocalPort == null 
            ? new UdpChannel(channelSetting.Host, channelSetting.RemotePort)
            : new UdpChannel(channelSetting.Host, channelSetting.RemotePort, channelSetting.LocalPort.Value);

        public SerialPortChannel CreateChannel(SerialPortChannelSetting channelSetting)
        {
            SerialStopBitCount stopBitCount = SerialStopBitCount.One;
            switch (channelSetting.StopBits)
            {
                case SerialPortChannelSetting.SerialStopBits.One5:
                    stopBitCount = SerialStopBitCount.OnePointFive;
                    break;
                case SerialPortChannelSetting.SerialStopBits.Two:
                    stopBitCount = SerialStopBitCount.Two;
                    break;
            }

            SerialParity parity = SerialParity.None;
            switch (channelSetting.Parity)
            {
                case SerialPortChannelSetting.SerialParity.Even:
                    parity = SerialParity.Even;
                    break;
                case SerialPortChannelSetting.SerialParity.Odd:
                    parity = SerialParity.Odd;
                    break;
                case SerialPortChannelSetting.SerialParity.Mark:
                    parity = SerialParity.Mark;
                    break;
                case SerialPortChannelSetting.SerialParity.Space:
                    parity = SerialParity.Space;
                    break;
            }

            SerialHandshake handshake = SerialHandshake.None;
            switch (channelSetting.Handshake)
            {
                case SerialPortChannelSetting.SerialHandshake.XOnXOff:
                    handshake = SerialHandshake.XOnXOff;
                    break;
                case SerialPortChannelSetting.SerialHandshake.RequestToSend:
                    handshake = SerialHandshake.RequestToSend;
                    break;
                case SerialPortChannelSetting.SerialHandshake.RequestToSendXOnXOff:
                    handshake = SerialHandshake.RequestToSendXOnXOff;
                    break;
            }

            return new SerialPortChannel(channelSetting.PortName, channelSetting.BaudRate, channelSetting.DataBits, stopBitCount, parity, handshake);
        }
    }
}
