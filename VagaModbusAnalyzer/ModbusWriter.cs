using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
}
