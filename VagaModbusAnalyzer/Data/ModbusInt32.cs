using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusInt32 : ModbusSerializableData<int?>
    {
        protected override int? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 4 ? BitConverter.ToInt32(bytes, 0) : (int?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
