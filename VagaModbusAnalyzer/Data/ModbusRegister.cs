using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusRegister : ModbusData
    {
        public ModbusRegister()
        {
            FirstByte = new ModbusRegisterByte(this);
            SecondByte = new ModbusRegisterByte(this);
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Address))
            {
                FirstByte.Address = Address;
                SecondByte.Address = Address;
            }
        }

        [JsonIgnore]
        public ModbusRegisterByte FirstByte { get; }
        [JsonIgnore]
        public ModbusRegisterByte SecondByte { get; }

        [JsonIgnore]
        public IEnumerable<byte> ValueBytes
        {
            get
            {
                yield return FirstByte.Value ?? 0;
                yield return SecondByte.Value ?? 0;
            }
        }

        [JsonIgnore]
        public short? SignedValue
        {
            get
            {
                if (FirstByte.Value != null && SecondByte.Value != null)
                    return BitConverter.IsLittleEndian
                        ? BitConverter.ToInt16(new byte[] { SecondByte.Value.Value, FirstByte.Value.Value }, 0)
                        : BitConverter.ToInt16(new byte[] { FirstByte.Value.Value, SecondByte.Value.Value }, 0);

                return null;
            }
        }

        [JsonIgnore]
        public short? ReverseSignedValue
        {
            get
            {
                if (FirstByte.Value != null && SecondByte.Value != null)
                    return !BitConverter.IsLittleEndian
                        ? BitConverter.ToInt16(new byte[] { SecondByte.Value.Value, FirstByte.Value.Value }, 0)
                        : BitConverter.ToInt16(new byte[] { FirstByte.Value.Value, SecondByte.Value.Value }, 0);

                return null;
            }
        }

        [JsonIgnore]
        public ushort? UnsignedValue
        {
            get
            {
                if (FirstByte.Value != null && SecondByte.Value != null)
                    return BitConverter.IsLittleEndian
                        ? BitConverter.ToUInt16(new byte[] { SecondByte.Value.Value, FirstByte.Value.Value }, 0)
                        : BitConverter.ToUInt16(new byte[] { FirstByte.Value.Value, SecondByte.Value.Value }, 0);

                return null;
            }
        }

        [JsonIgnore]
        public ushort? ReverseUnsignedValue
        {
            get
            {
                if (FirstByte.Value != null && SecondByte.Value != null)
                    return !BitConverter.IsLittleEndian
                        ? BitConverter.ToUInt16(new byte[] { SecondByte.Value.Value, FirstByte.Value.Value }, 0)
                        : BitConverter.ToUInt16(new byte[] { FirstByte.Value.Value, SecondByte.Value.Value }, 0);

                return null;
            }
        }

        public void SetRegisterValue(IReadOnlyList<byte> bytes, int offset = 0)
        {
            byte? first = null;
            byte? second = null;

            byte? oldFirst;
            byte? oldSecond;

            if (bytes != null && offset >= 0 && offset + 1 < bytes.Count)
            {
                first = bytes[offset];
                second = bytes[offset + 1];
            }

            oldFirst = FirstByte.Value;
            oldSecond = SecondByte.Value;

            FirstByte.Value = first;
            SecondByte.Value = second;

            if (oldFirst != first || oldSecond != second)
            {
                RaisePropertyChanged(nameof(SignedValue));
                RaisePropertyChanged(nameof(ReverseSignedValue));
                RaisePropertyChanged(nameof(UnsignedValue));
                RaisePropertyChanged(nameof(ReverseUnsignedValue));
            }

        }
    }

    public class ModbusRegisterByte : ModbusData<byte?>
    {
        public ModbusRegisterByte(ModbusRegister register)
        {
            Register = register;
        }

        public ModbusRegister Register { get; }
    }
}
