using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusSingle : ModbusSerializableData<float?>
    {
        protected override float? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 4 ? BitConverter.ToSingle(bytes, 0) : (float?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
