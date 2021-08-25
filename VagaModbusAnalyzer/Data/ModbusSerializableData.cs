using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Modbus.Data;

namespace VagaModbusAnalyzer.Data
{
    public abstract class ModbusSerializableData<TValue> : ModbusRegisterData<TValue>
    {
        private ModbusEndian modbusEndian;

        public ModbusEndian ModbusEndian
        {
            get => modbusEndian;
            set
            {
                if (modbusEndian != value)
                {
                    var bytes = OnSerializeValue();
                    SetProperty(ref modbusEndian, value);
                    SetValue(bytes);
                }
            }
        }

        public void SetValue(byte[] bytes)
        {
            Value = OnDeserializeValue(modbusEndian.Sort(bytes));
        }

        protected abstract TValue OnDeserializeValue(byte[] bytes);
        protected abstract byte[] OnSerializeValue();
    }
}
