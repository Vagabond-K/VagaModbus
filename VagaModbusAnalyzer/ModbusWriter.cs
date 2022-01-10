using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VagabondK.Protocols.Channels;
using VagabondK.Protocols.Logging;
using VagabondK.Protocols.Modbus;
using VagabondK.Protocols.Modbus.Serialization;

namespace VagaModbusAnalyzer
{
    public class ModbusWriter : NotifyPropertyChangeObject
    {
        public ModbusWriter()
        {
            WriteValues = new ObservableCollection<ModbusWriteValue>();
        }

        public void CopyTo(ModbusWriter modbusWriter)
        {
            modbusWriter.SlaveAddress = SlaveAddress;
            modbusWriter.Address = Address;
            modbusWriter.ResponseTimeout = ResponseTimeout;
            modbusWriter.WriteValues = new ObservableCollection<ModbusWriteValue>(WriteValues.Select(writeValue =>
            {
                var result = new ModbusWriteValue();
                writeValue.CopyTo(result);
                return result;
            }));
            modbusWriter.Channel = Channel;
            modbusWriter.UseMultipleWriteWhenSingle = UseMultipleWriteWhenSingle;
        }

        public ModbusObjectType ObjectType { get => Get(ModbusObjectType.HoldingRegister); set => Set(value); }
        public byte SlaveAddress { get => Get((byte)1); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
        public ushort Count { get => Get((ushort)0); set => Set(value); }
        public int ResponseTimeout { get => Get(5000); set => Set(value); }
        public bool UseMultipleWriteWhenSingle { get => Get(false); set => Set(value); }
        public ObservableCollection<ModbusWriteValue> WriteValues { get => Get<ObservableCollection<ModbusWriteValue>>(); set => Set(value); }

        [JsonIgnore]
        public bool IsWriteSingle { get => Get(false); private set => Set(value); }

        [JsonIgnore]
        public ModbusRequest Request
        {
            get
            {
                switch (ObjectType)
                {
                    case ModbusObjectType.HoldingRegister:
                        ModbusRequest request;
                        if (Count > 1 || UseMultipleWriteWhenSingle)
                        {
                            var bytes = WriteValues.SelectMany(value => value.Bytes).ToArray();
                            if (bytes.Length % 2 == 1)
                                Array.Resize(ref bytes, bytes.Length + 1);
                            request = new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, bytes);
                        }
                        else
                        {
                            var bytes = WriteValues.SelectMany(value => value.Bytes).ToArray();
                            if (bytes.Length % 2 == 1)
                                Array.Resize(ref bytes, bytes.Length + 1);

                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(bytes);

                            request = new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, BitConverter.ToUInt16(bytes, 0));
                        }
                        return request;
                    case ModbusObjectType.Coil:
                        return Count > 1 || UseMultipleWriteWhenSingle 
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

        private static readonly ModbusRtuSerializer modbusRtuSerializer = new ModbusRtuSerializer();
        private static readonly ModbusTcpSerializer modbusTcpSerializer = new ModbusTcpSerializer();
        private static readonly ModbusAsciiSerializer modbusAsciiSerializer = new ModbusAsciiSerializer();

        internal void UpdateRequestMessage()
        {
            try
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
            catch (Exception ex)
            {
                RequestMessage = ex.Message;
            }
        }

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
                    }
                    UpdateWriteValueAddresses(WriteValues);
                    UpdateCount();
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
                    UpdateRequestMessage();
                    break;
                case nameof(Channel):
                case nameof(UseMultipleWriteWhenSingle):
                    UpdateRequestMessage();
                    break;
                case nameof(Address):
                    UpdateWriteValueAddresses(WriteValues);
                    break;
                case nameof(RequestMessage):
                    Status = null;
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
            UpdateCount();
        }

        private void UpdateCount()
        {
            if (WriteValues == null)
            {
                Count = 0;
            }
            else
            {
                switch (ObjectType)
                {
                    case ModbusObjectType.HoldingRegister:
                        Count = (ushort)Math.Ceiling((decimal)WriteValues.Sum(item => item.ByteLength) / 2);
                        break;
                    case ModbusObjectType.Coil:
                        Count = (ushort)WriteValues.Sum(item => item.ByteLength);
                        break;
                }
            }
            IsWriteSingle = Count == 1 || WriteValues.Count == 1;
        }

        internal void UpdateWriteValueAddresses(IEnumerable<ModbusWriteValue> list)
        {
            if (WriteValues == null) return;

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
}
