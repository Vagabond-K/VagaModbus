using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusDouble : ModbusSerializableData<double?>
    {
        protected override double? OnDeserializeValue(byte[] bytes)
            => bytes != null && bytes.Length == 8 ? BitConverter.ToDouble(bytes, 0) : (double?)null;

        protected override byte[] OnSerializeValue()
            => Value != null ? BitConverter.GetBytes(Value.Value) : null;
    }
}
