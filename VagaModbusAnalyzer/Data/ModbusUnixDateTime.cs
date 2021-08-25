using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusUnixDateTime : ModbusMultiByteData<DateTime?>
    {
        protected override DateTime? OnDeserializeValue(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        protected override byte[] OnSerializeValue()
        {
            throw new NotImplementedException();
        }
    }
}
