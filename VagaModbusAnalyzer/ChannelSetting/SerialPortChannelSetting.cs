using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Channels;

namespace VagaModbusAnalyzer.ChannelSetting
{
    public class SerialPortChannelSetting : NotifyPropertyChangeObject, IChannelSetting
    {
        public string PortName { get => Get<string>(); set => Set(value); }
        public int BaudRate { get => Get(9600); set => Set(value); }
        public int DataBits { get => Get(8); set => Set(value); }
        public SerialStopBits StopBits { get => Get(SerialStopBits.One); set => Set(value); }
        public SerialParity Parity { get => Get(SerialParity.None); set => Set(value); }
        public SerialHandshake Handshake { get => Get(SerialHandshake.None); set => Set(value); }

        public bool DtrEnable { get => Get(false); set => Set(value); }
        public bool RtsEnable { get => Get(false); set => Set(value); }

        public ChannelType ChannelType => ChannelType.SerialPort;
        public IChannelSetting Copy() => new SerialPortChannelSetting
        {
            PortName = PortName,
            BaudRate = BaudRate,
            DataBits = DataBits,
            StopBits = StopBits,
            Parity = Parity,
            Handshake = Handshake,
            DtrEnable = DtrEnable,
            RtsEnable = RtsEnable,
        };

        public enum SerialStopBits
        {
            One = 0,
            One5 = 1,
            Two = 2
        }

        public enum SerialParity
        {
            None = 0,
            Odd = 1,
            Even = 2,
            Mark = 3,
            Space = 4
        }

        public enum SerialHandshake
        {
            None = 0,
            XOnXOff = 1,
            RequestToSend = 2,
            RequestToSendXOnXOff = 3
        }
    }
}
