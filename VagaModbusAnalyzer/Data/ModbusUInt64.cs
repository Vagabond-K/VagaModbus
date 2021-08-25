using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusUInt64 : ModbusSerializableData<ulong?>
    {
        protected override ulong? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 8 ? BitConverter.ToUInt64(bytes, 0) : (ulong?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
