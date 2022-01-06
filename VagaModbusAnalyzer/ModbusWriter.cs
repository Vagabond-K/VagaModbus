using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VagabondK.Protocols.Modbus;
using VagabondK.Protocols.Modbus.Data;
using VagabondK.Protocols.Modbus.Serialization;

namespace VagaModbusAnalyzer
{
    public class ModbusWriter : NotifyPropertyChangeObject
    {
        public ModbusWriter()
        {
            WriteValues = new ObservableCollection<ModbusWriteValue>();
        }

        public byte SlaveAddress { get => Get((byte)1); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
        public int ResponseTimeout { get => Get(5000); set => Set(value); }

        public bool UseMultipleWriteWhenSingle { get => Get(false); set => Set(value); }

        public ModbusObjectType ObjectType { get => Get(ModbusObjectType.HoldingRegister); set => Set(value); }

        [JsonIgnore]
        public ModbusRequest Request
        {
            get
            {
                switch (ObjectType)
                {
                    case ModbusObjectType.HoldingRegister:
                        return WriteValues.Count > 1 || UseMultipleWriteWhenSingle
                            ? new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, WriteValues.SelectMany(value => value.Bytes))
                            : new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, BitConverter.IsLittleEndian
                                ? BitConverter.ToUInt16(WriteValues.FirstOrDefault()?.Bytes?.Reverse()?.ToArray() ?? new byte[] { 0, 0 }, 0)
                                : BitConverter.ToUInt16(WriteValues.FirstOrDefault()?.Bytes?.ToArray() ?? new byte[] { 0, 0 }, 0));
                    case ModbusObjectType.Coil:
                        return WriteValues.Count > 1 || UseMultipleWriteWhenSingle 
                            ? new ModbusWriteCoilRequest(SlaveAddress, Address, WriteValues.Select(value => value.Value.To<bool>()))
                            : new ModbusWriteCoilRequest(SlaveAddress, Address, (WriteValues.FirstOrDefault()?.Value ?? 0) == 1);
                    default:
                        return null;
                }
            }
        }

        [JsonIgnore]
        public bool IsBusy { get => Get(false); set => Set(value); }

        [JsonIgnore]
        public object Status { get => Get<object>(); set => Set(value); }

        [JsonIgnore]
        public DateTime? LastUpdated { get => Get<DateTime?>(); set => Set(value); }

        [JsonIgnore]
        public ModbusChannel Channel { get => Get<ModbusChannel>(); set => Set(value); }

        [JsonIgnore]
        public string RequestMessage { get => Get<string>(); private set => Set(value); }

        public void Write(ModbusMaster modbusMaster, ICrossThreadDispatcher dispatcher)
        {
            try
            {
                dispatcher.Invoke(() => { IsBusy = true; });
                var response = modbusMaster.Request(Request, ResponseTimeout);
                dispatcher.Invoke(() => { IsBusy = false; });
            }
            catch (Exception ex)
            {
                dispatcher.Invoke(() => { IsBusy = false; });

                throw ex;
            }
        }

        private static readonly ModbusRtuSerializer modbusRtuSerializer = new ModbusRtuSerializer();
        private static readonly ModbusTcpSerializer modbusTcpSerializer = new ModbusTcpSerializer();
        private static readonly ModbusAsciiSerializer modbusAsciiSerializer = new ModbusAsciiSerializer();

        internal void UpdateRequestMessage()
        {
            if (Channel != null)
            {
                switch (Channel.ModbusType)
                {
                    case ModbusType.RTU:
                        RequestMessage = BitConverter.ToString(modbusRtuSerializer.Serialize(Request).ToArray()).Replace('-', ' ');
                        break;
                    case ModbusType.TCP:
                        RequestMessage = "?? ??" + BitConverter.ToString(modbusTcpSerializer.Serialize(Request).ToArray()).Replace('-', ' ').Remove(0, 5);
                        break;
                    case ModbusType.ASCII:
                        RequestMessage = BitConverter.ToString(modbusAsciiSerializer.Serialize(Request).ToArray()).Replace('-', ' ');
                        //StringBuilder stringBuilder = new StringBuilder();
                        //foreach (var b in modbusAsciiSerializer.Serialize(Request))
                        //{
                        //    switch (b)
                        //    {
                        //        case 0x0D:
                        //            stringBuilder.Append("\\r");
                        //            break;
                        //        case 0x0A:
                        //            stringBuilder.Append("\\n");
                        //            break;
                        //        default:
                        //            if (b >= 33 && b <= 126)
                        //                stringBuilder.Append((char)b);
                        //            else
                        //            {
                        //                stringBuilder.Append("{0x");
                        //                stringBuilder.Append(b.ToString("X2"));
                        //                stringBuilder.Append("}");
                        //            }
                        //            break;
                        //    }
                        //}
                        //RequestMessage = stringBuilder.ToString();
                        break;
                }
            }
            else
                RequestMessage = string.Empty;
        }



        public ObservableCollection<ModbusWriteValue> WriteValues { get => Get<ObservableCollection<ModbusWriteValue>>(); set => Set(value); }

        protected override bool OnPropertyChanging(QueryPropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(WriteValues) && WriteValues != null)
            {
                WriteValues.CollectionChanged -= OnWriteValuesCollectionChanged;
                foreach (var item in WriteValues)
                    item.modbusWriter = null;
            }

            return base.OnPropertyChanging(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(WriteValues):
                    if (WriteValues != null)
                    {
                        WriteValues.CollectionChanged += OnWriteValuesCollectionChanged;
                        foreach (var item in WriteValues)
                            item.modbusWriter = this;

                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
                        UpdateRequestMessage();
                        UpdateWriteValueAddresses(WriteValues);
                    }
                    break;
                case nameof(Channel):
                case nameof(UseMultipleWriteWhenSingle):
                    UpdateRequestMessage();
                    break;
                case nameof(Address):
                    UpdateWriteValueAddresses(WriteValues);
                    break;
            }
        }

        private void OnWriteValuesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<ModbusWriteValue>())
                    item.modbusWriter = null;

                if (e.OldStartingIndex >= 0)
                    UpdateWriteValueAddresses(WriteValues.Skip(e.OldStartingIndex));
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.Cast<ModbusWriteValue>())
                    item.modbusWriter = this;

                UpdateWriteValueAddresses(e.NewItems.Cast<ModbusWriteValue>());
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
            UpdateRequestMessage();
        }

        internal void UpdateWriteValueAddresses(IEnumerable<ModbusWriteValue> list)
        {
            int? totalLength = null;
            foreach (var item in list)
            {
                if (WriteValues.Count == 0)
                {
                    item.Address = Address;
                    totalLength = item.ByteLength;
                }
                else
                {
                    int index = WriteValues.IndexOf(item);
                    if (totalLength == null)
                        totalLength = WriteValues.Take(index).Sum(s => s.ByteLength);

                    switch (ObjectType)
                    {
                        case ModbusObjectType.HoldingRegister:
                            item.Address = (ushort)(Address + totalLength.Value / 2);
                            item.IsFirstByte = totalLength.Value % 2 == 0;
                            break;
                        case ModbusObjectType.Coil:
                            item.Address = (ushort)(Address + totalLength.Value);
                            break;
                    }

                    totalLength += item.ByteLength;
                }
            }
        }

    }

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
