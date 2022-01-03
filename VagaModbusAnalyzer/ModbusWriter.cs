using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

    public abstract class ModbusWriter<T> : ModbusWriter where T : ModbusWriteSetting
    {
        protected ModbusWriter()
        {
            WriteSettings = new ObservableCollection<T>();
        }

        public ObservableCollection<T> WriteSettings { get => Get<ObservableCollection<T>>(); set => Set(value); }

        protected override bool OnPropertyChanging(QueryPropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(WriteSettings) && WriteSettings != null)
            {
                WriteSettings.CollectionChanged -= OnWriteSettingsCollectionChanged;
                foreach (var setting in WriteSettings)
                    setting.PropertyChanged -= OnWriteSettingPropertyChanged;
            }

            return base.OnPropertyChanging(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(WriteSettings):
                    if (WriteSettings != null)
                    {
                        WriteSettings.CollectionChanged += OnWriteSettingsCollectionChanged;
                        foreach (var setting in WriteSettings)
                            setting.PropertyChanged += OnWriteSettingPropertyChanged;

                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
                        UpdateRequestMessage();
                        UpdateWriteSettingAddresses(WriteSettings);
                    }
                    break;
                case nameof(Channel):
                case nameof(UseMultipleWriteWhenSingle):
                    UpdateRequestMessage();
                    break;
                case nameof(Address):
                    UpdateWriteSettingAddresses(WriteSettings);
                    break;
            }
        }

        private void OnWriteSettingsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems.Cast<ModbusWriteSetting>())
                    item.PropertyChanged -= OnWriteSettingPropertyChanged;
            if (e.NewItems != null)
            {
                var collection = sender as IList;
                foreach (var item in e.NewItems.Cast<ModbusWriteSetting>())
                    item.PropertyChanged += OnWriteSettingPropertyChanged;

                UpdateWriteSettingAddresses(e.NewItems);
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
            UpdateRequestMessage();
        }

        private void OnWriteSettingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Request)));
            UpdateRequestMessage();
        }

        protected abstract void UpdateWriteSettingAddresses(IEnumerable list);

    }

    public class ModbusWriteSetting : NotifyPropertyChangeObject
    {
        public string Name { get => Get<string>(); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
    }

    public class ModbusHoldingRegisterWriter : ModbusWriter<ModbusWriteHoldingRegisterSetting>
    {
        public override ModbusRequest Request => new ModbusWriteHoldingRegisterRequest(SlaveAddress, Address, WriteSettings.SelectMany(setting => setting.Bytes));

        protected override void UpdateWriteSettingAddresses(IEnumerable list)
        {
            throw new NotImplementedException();
        }
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
        public override ModbusRequest Request 
            => WriteSettings.Count > 1 || UseMultipleWriteWhenSingle ? new ModbusWriteCoilRequest(SlaveAddress, Address, WriteSettings.Select(setting => setting.Value))
            : new ModbusWriteCoilRequest(SlaveAddress, Address, WriteSettings.FirstOrDefault()?.Value ?? false);

        protected override void UpdateWriteSettingAddresses(IEnumerable list)
        {
            var collection = WriteSettings;
            foreach (var item in list.Cast<ModbusWriteCoilSetting>())
                item.Address = (ushort)(Address + collection.IndexOf(item));
        }
    }

    public class ModbusWriteCoilSetting : ModbusWriteSetting
    {
        public bool Value { get => Get(false); set => Set(value); }
    }

}
