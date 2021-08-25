using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusUInt16 : ModbusSerializableData<ushort?>
    {
        protected override ushort? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 2 ? BitConverter.ToUInt16(bytes, 0) : (ushort?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
