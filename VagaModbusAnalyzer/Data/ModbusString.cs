using System;
using System.Collections.Generic;
using System.Text;

namespace VagaModbusAnalyzer.Data
{
    public class ModbusString : ModbusMultiByteData<string>
    {
        private string encoding;
        private Encoding textEncoding = System.Text.Encoding.ASCII;

        public string Encoding
        {
            get => encoding;
            set
            {
                if (encoding != value)
                {
                    var bytes = OnSerializeValue();
                    encoding = value;
                    try
                    {
                        textEncoding = System.Text.Encoding.GetEncoding(encoding);
                    }
                    catch
                    {
                        textEncoding = System.Text.Encoding.ASCII;
                    }
                    SetValue(bytes);
                }
            }
        }

        protected override string OnDeserializeValue(byte[] bytes) => textEncoding.GetString(bytes);

        protected override byte[] OnSerializeValue() => textEncoding.GetBytes(Value);
    }
}
