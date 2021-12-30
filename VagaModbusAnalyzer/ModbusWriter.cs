using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VagabondK.Protocols.Modbus;
using VagabondK.Protocols.Modbus.Data;

namespace VagaModbusAnalyzer
{
    public abstract class ModbusWriter : NotifyPropertyChangeObject
    {
        public byte SlaveAddress { get => Get((byte)1); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
        public int ResponseTimeout { get => Get(5000); set => Set(value); }

        public bool UseMultipleWriteWhenSingle { get => Get(false); set => Set(value); }

        [JsonIgnore]
        public abstract ModbusRequest Request { get; }
    }

    public abstract class ModbusWriter<T> : ModbusWriter
    {
        public ObservableCollection<T> WriteSettings { get; set; }
    }

    public class ModbusWriteSetting : NotifyPropertyChangeObject
    {
        public string Name { get => Get<string>(); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
    }

    public class ModbusHoldingRegisterWriter : ModbusWriter<ModbusWriteHoldingRegisterSetting>
    {
        public override ModbusRequest Request => new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, WriteSettings.SelectMany(setting => setting.Bytes));
    }



    public abstract class ModbusWriteHoldingRegisterSetting : ModbusWriteSetting
    {
        public bool IsFirstByte { get => Get(true); set => Set(value); }
        public abstract ushort ByteLength { get; }

        public abstract IEnumerable<byte> Bytes { get; }
    }

    public class ModbusWriteNumericSetting<T> : ModbusWriteHoldingRegisterSetting where T : struct
    {
        private static readonly ushort byteLength = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(default(T));

        public T Value { get => Get<T>(); set => Set(value); }

        public T Max { get => Get<T>(); set => Set(value); }
        public T Min { get => Get<T>(); set => Set(value); }

        public ModbusEndian ModbusEndian { get => Get(ModbusEndian.AllBig); set => Set(value); }

        public override ushort ByteLength => byteLength;

        public override IEnumerable<byte> Bytes
        {
            get
            {
                //TODO: 수치형 데이터를 바이트 배열로 변환하는 로직 작성해야 함.
                throw new NotImplementedException();
            }
        }
    }

    public class ModbusWriteBitArray : ModbusWriteHoldingRegisterSetting
    {
        public IReadOnlyList<BitValue> BitValues { get; set; }

        public override ushort ByteLength => (ushort)(BitValues?.Count / 8 + 1);

        public override IEnumerable<byte> Bytes
        {
            get
            {
                var values = BitValues.Select(bitValue => bitValue.Value).ToArray();
                var byteLength = ByteLength;
                for (int i = 0; i < byteLength; i++)
                {
                    byte byteValue = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        int index = i * 8 + j;
                        if (index < values.Length)
                            byteValue |= (byte)((values[index] ? 1 : 0) << (7 - j));
                    }
                    yield return byteValue;
                }
            }
        }

        public class BitValue : NotifyPropertyChangeObject
        {
            public bool Value { get => Get(false); set => Set(value); }
        }
    }






    public class ModbusCoilWriter : ModbusWriter<ModbusWriteCoilSetting>
    {
        public override ModbusRequest Request => new ModbusWriteCoilRequest(SlaveAddress, Address, WriteSettings.Select(setting => setting.Value));
    }

    public class ModbusWriteCoilSetting : ModbusWriteSetting
    {
        public bool Value { get => Get(false); set => Set(value); }
    }

}
