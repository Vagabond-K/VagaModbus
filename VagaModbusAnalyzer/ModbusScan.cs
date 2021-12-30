using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using VagabondK.Protocols.Modbus;
using VagaModbusAnalyzer.Data;

namespace VagaModbusAnalyzer
{
    public class ModbusScan : NotifyPropertyChangeObject
    {
        public ModbusObjectType ObjectType { get => Get(ModbusObjectType.InputRegister); set => Set(value); }

        public byte SlaveAddress { get => Get((byte)1); set => Set(value); }
        public ushort Address { get => Get((ushort)0); set => Set(value); }
        public ushort Length { get => Get((ushort)1); set => Set(value); }
        public int ResponseTimeout { get => Get(5000); set => Set(value); }

        public bool RunScan { get => Get(true); set => Set(value); }

        [JsonIgnore]
        public ModbusRequest Request { get => Get(() => { return new ModbusReadRequest(SlaveAddress, ObjectType, Address, Length); }); }

        [JsonIgnore]
        public IEnumerable Data { get => Get(() => { return CreateAddressItems(); }); private set => Set(value); }

        public void OnSettingChanged()
        {
            ClearProperty(nameof(Request));
            Data = CreateAddressItems();
        }

        private IEnumerable CreateAddressItems()
        {
            switch (ObjectType)
            {
                case ModbusObjectType.InputRegister:
                case ModbusObjectType.HoldingRegister:
                    return Enumerable.Range(0, Length).Select(i => new ModbusRegister { Address = (ushort)(Address + i) }).ToArray();
                case ModbusObjectType.DiscreteInput:
                case ModbusObjectType.Coil:
                    return Enumerable.Range(0, Length).Select(i => new ModbusBoolean { Address = (ushort)(Address + i) }).ToArray();
                default:
                    return null;
            }
        }


        [JsonIgnore]
        public object Status { get => Get<object>(); set => Set(value); }

        [JsonIgnore]
        public DateTime? LastUpdated { get => Get<DateTime?>(); set => Set(value); }
    }
}
