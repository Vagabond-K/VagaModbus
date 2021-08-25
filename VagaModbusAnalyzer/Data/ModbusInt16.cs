using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusInt16 : ModbusSerializableData<short?>
    {
        protected override short? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 2 ? BitConverter.ToInt16(bytes, 0) : (short?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
