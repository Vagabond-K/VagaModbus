using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VagabondK.Protocols.Modbus.Data;

namespace VagaModbusAnalyzer
{
    public class ModbusWriteValue : NotifyPropertyChangeObject
    {
        private static ulong[] maxValues = Enumerable.Range(1, 8).Select(length =>
        {
            var bytes = Enumerable.Repeat((byte)0xff, length).Concat(Enumerable.Repeat((byte)0, 8 - length));
            return BitConverter.ToUInt64((BitConverter.IsLittleEndian ? bytes : bytes.Reverse()).ToArray(), 0);
        }).ToArray();
        private static long[] minValues = Enumerable.Range(1, 8).Select(length =>
        {
            var bytes = Enumerable.Repeat((byte)0, length - 1).Concat(Enumerable.Repeat((byte)0x80, 1)).Concat(Enumerable.Repeat((byte)0xff, 8 - length));
            return BitConverter.ToInt64((BitConverter.IsLittleEndian ? bytes : bytes.Reverse()).ToArray(), 0);
        }).ToArray();

        internal ModbusWriter modbusWriter;

        public string Name { get => Get<string>(); set => Set(value); }

        [JsonIgnore]
        public ushort Address { get => Get((ushort)0); set => Set(value); }

        public TypeCode Type { get => Get(TypeCode.UInt64); set => Set(value); }
        public ushort ByteLength { get => Get((ushort)2); set => Set(value); }

        public ModbusEndian ModbusEndian { get => Get(() => ModbusEndian.AllBig); set => Set(value); }

        public decimal Value { get => Get(0M); set => Set(value); }

        [JsonIgnore]
        public bool IsFirstByte { get => Get(true); set => Set(value); }

        [JsonIgnore]
        public bool EditableByteLength { get => Get(true); private set => Set(value); }

        [JsonIgnore]
        public bool EditableModbusEndian { get => Get(true); private set => Set(value); }

        [JsonIgnore]
        public ModbusEndian[] ModbusEndians => IsFirstByte && ByteLength >= 4 && ByteLength % 2 == 0 ? allEndians : filteredEndians;

        private static readonly ModbusEndian[] allEndians = new ModbusEndian[]
        {
            ModbusEndian.AllBig,
            new ModbusEndian(false, true),
            new ModbusEndian(true, false),
            ModbusEndian.AllLittle
        };

        private static readonly ModbusEndian[] filteredEndians = new ModbusEndian[]
        {
            ModbusEndian.AllBig,
            ModbusEndian.AllLittle
        };


        [JsonIgnore]
        public IEnumerable<byte> Bytes
        {
            get
            {
                switch (Type)
                {
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        decimal value = Value;
                        int byteLength = ByteLength;
                        if (byteLength > 0 && byteLength <= 8)
                        {
                            var maxValue = maxValues[byteLength - 1];
                            var minValue = minValues[byteLength - 1];

                            if (value > maxValue) value = maxValue;
                            if (value < minValue) value = minValue;

                            byte[] bytes;
                            if (value > 0) bytes = BitConverter.GetBytes((ulong)value).ToArray();
                            else bytes = BitConverter.GetBytes((long)value).ToArray();

                            bytes = BitConverter.IsLittleEndian ? bytes.Take(byteLength).Reverse().ToArray() : bytes.Skip(8 - byteLength).ToArray();
                            return ModbusEndian.Sort(bytes);
                        }
                        return Array.Empty<byte>();
                    case TypeCode.Single:
                        return BitConverter.IsLittleEndian
                            ? ModbusEndian.Sort(BitConverter.GetBytes((float)Value).Reverse().ToArray())
                            : ModbusEndian.Sort(BitConverter.GetBytes((float)Value).ToArray());
                    case TypeCode.Double:
                        return BitConverter.IsLittleEndian
                            ? ModbusEndian.Sort(BitConverter.GetBytes((double)Value).Reverse().ToArray())
                            : ModbusEndian.Sort(BitConverter.GetBytes((double)Value).ToArray());
                }
                return null;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(Type):
                    switch (Type)
                    {
                        case TypeCode.Single:
                            ByteLength = 4;
                            EditableByteLength = false;
                            break;
                        case TypeCode.Double:
                            ByteLength = 8;
                            EditableByteLength = false;
                            break;
                        case TypeCode.Boolean:
                            ByteLength = 1;
                            EditableByteLength = false;
                            break;
                        default:
                            EditableByteLength = true;
                            break;
                    }
                    EditableModbusEndian = Type != TypeCode.Boolean && ByteLength > 1;
                    break;
                case nameof(Value):
                    switch (Type)
                    {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            decimal value = Value;
                            int byteLength = ByteLength;
                            if (byteLength > 0 && byteLength <= 8)
                            {
                                var maxValue = maxValues[byteLength - 1];
                                var minValue = minValues[byteLength - 1];

                                if (value > maxValue) Value = maxValue;
                                if (value < minValue) Value = minValue;
                            }
                            break;
                    }
                    modbusWriter?.UpdateRequestMessage();
                    break;
                case nameof(ByteLength):
                    if (modbusWriter != null)
                        modbusWriter.UpdateWriteValueAddresses(modbusWriter.WriteValues.Skip(modbusWriter.WriteValues.IndexOf(this)));
                    EditableModbusEndian = Type != TypeCode.Boolean && ByteLength > 1;
                    UpdateModbusEndians();
                    break;
                case nameof(IsFirstByte):
                    UpdateModbusEndians();
                    break;
                case nameof(ModbusEndians):
                    ModbusEndian = tempModbusEndian;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModbusEndian)));
                    break;
            }
        }

        private ModbusEndian tempModbusEndian = ModbusEndian.AllBig;
        private void UpdateModbusEndians()
        {
            if (IsFirstByte && ByteLength >= 4 && ByteLength % 2 == 0)
            {
                tempModbusEndian = ModbusEndian;
            }
            else
            {
                if (ModbusEndian != ModbusEndian.AllBig && ModbusEndian != ModbusEndian.AllLittle)
                    tempModbusEndian = ModbusEndian.AllBig;
                else
                    tempModbusEndian = ModbusEndian;
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModbusEndians)));
        }
    }
}
