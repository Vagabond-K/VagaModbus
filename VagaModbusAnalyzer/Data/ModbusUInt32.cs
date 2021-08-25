using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusUInt32 : ModbusSerializableData<uint?>
    {
        protected override uint? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 4 ? BitConverter.ToUInt32(bytes, 0) : (uint?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
