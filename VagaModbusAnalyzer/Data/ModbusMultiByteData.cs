using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Modbus.Data;

namespace VagaModbusAnalyzer.Data
{
    public abstract class ModbusMultiByteData<TValue> : ModbusSerializableData<TValue>
    {
        private int length;

        public int Length { get => length; set => SetProperty(ref length, value); }
    }
}
