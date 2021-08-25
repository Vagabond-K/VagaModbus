using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusInt64 : ModbusSerializableData<long?>
    {
        protected override long? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 8 ? BitConverter.ToInt64(bytes, 0) : (long?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
