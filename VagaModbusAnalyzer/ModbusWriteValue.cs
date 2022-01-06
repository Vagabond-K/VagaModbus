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
            new ModbusEndian(false, true),
            new ModbusEndian(true, false),
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
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<byte>()).Reverse().ToArray());
                    case TypeCode.UInt16:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<ushort>()).Reverse().ToArray());
                    case TypeCode.UInt32:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<uint>()).Reverse().ToArray());
                    case TypeCode.UInt64:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<ulong>()).Reverse().ToArray());
                    case TypeCode.SByte:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<sbyte>()).Reverse().ToArray());
                    case TypeCode.Int16:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<short>()).Reverse().ToArray());
                    case TypeCode.Int32:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<int>()).Reverse().ToArray());
                    case TypeCode.Int64:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<long>()).Reverse().ToArray());
                    case TypeCode.Single:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<float>()).Reverse().ToArray());
                    case TypeCode.Double:
                        return ModbusEndian.Sort(BitConverter.GetBytes(Value.To<double>()).Reverse().ToArray());
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
