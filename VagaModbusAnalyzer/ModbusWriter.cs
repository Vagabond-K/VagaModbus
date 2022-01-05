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
    public abstract class ModbusWriter : NotifyPropertyChangeObject
    {
        public byte SlaveAddress { get => Get((byte)1); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
        public int ResponseTimeout { get => Get(5000); set => Set(value); }

        public bool UseMultipleWriteWhenSingle { get => Get(false); set => Set(value); }

        [JsonIgnore]
        public abstract ModbusRequest Request { get; }

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

        protected void UpdateRequestMessage()
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
    }

    public abstract class ModbusWriter<T> : ModbusWriter where T : ModbusWriteValue
    {
        protected ModbusWriter()
        {
            WriteValues = new ObservableCollection<T>();
        }

        public ObservableCollection<T> WriteValues { get => Get<ObservableCollection<T>>(); set => Set(value); }

        protected override bool OnPropertyChanging(QueryPropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(WriteValues) && WriteValues != null)
            {
                WriteValues.CollectionChanged -= OnWriteValuesCollectionChanged;
                foreach (var value in WriteValues)
                    value.PropertyChanged -= OnWriteValuePropertyChanged;
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
                        foreach (var value in WriteValues)
                            value.PropertyChanged += OnWriteValuePropertyChanged;

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
                foreach (var item in e.OldItems.Cast<T>())
                    item.PropertyChanged -= OnWriteValuePropertyChanged;
            if (e.NewItems != null)
            {
                var collection = sender as IList;
                foreach (var item in e.NewItems.Cast<T>())
                    item.PropertyChanged += OnWriteValuePropertyChanged;

                UpdateWriteValueAddresses(e.NewItems.Cast<T>());
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
            UpdateRequestMessage();
        }

        private void OnWriteValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
            UpdateRequestMessage();
        }

        protected abstract void UpdateWriteValueAddresses(IEnumerable<T> list);

    }

    public class ModbusWriteValue : NotifyPropertyChangeObject
    {
        public string Name { get => Get<string>(); set => Set(value); }

        [JsonIgnore]
        public ushort Address { get => Get((ushort)0); set => Set(value); }
    }

    public class ModbusHoldingRegisterWriter : ModbusWriter<ModbusWriteHoldingRegister>
    {
        public override ModbusRequest Request => new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, WriteValues.SelectMany(value => value.Bytes));

        protected override void UpdateWriteValueAddresses(IEnumerable<ModbusWriteHoldingRegister> list)
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
                        totalLength = WriteValues.Take(WriteValues.IndexOf(item)).Sum(s => s.ByteLength);

                    item.Address = (ushort)(Address + totalLength.Value / 2);
                    item.IsFirstByte = totalLength.Value % 2 == 0;

                    totalLength += item.ByteLength;
                }
            }
        }
    }



    public class ModbusWriteHoldingRegister : ModbusWriteValue
    {
        public TypeCode Type { get => Get(TypeCode.UInt16); set => Set(value); }
        public decimal Value { get => Get<decimal>(0); set => Set(value); }

        public ModbusEndian ModbusEndian { get => Get(ModbusEndian.AllBig); set => Set(value); }

        [JsonIgnore]
        public bool IsFirstByte { get => Get(true); set => Set(value); }

        [JsonIgnore]
        public ushort ByteLength => (ushort)Marshal.SizeOf(Convert.ChangeType(Value, Type));

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
    }




    public class ModbusCoilWriter : ModbusWriter<ModbusWriteCoil>
    {
        public override ModbusRequest Request 
            => WriteValues.Count > 1 || UseMultipleWriteWhenSingle ? new ModbusWriteCoilRequest(SlaveAddress, Address, WriteValues.Select(value => value.Value))
            : new ModbusWriteCoilRequest(SlaveAddress, Address, WriteValues.FirstOrDefault()?.Value ?? false);

        protected override void UpdateWriteValueAddresses(IEnumerable<ModbusWriteCoil> list)
        {
            var collection = WriteValues;
            foreach (var item in list.Cast<ModbusWriteCoil>())
                item.Address = (ushort)(Address + collection.IndexOf(item));
        }
    }

    public class ModbusWriteCoil : ModbusWriteValue
    {
        public bool Value { get => Get(false); set => Set(value); }
    }

}
