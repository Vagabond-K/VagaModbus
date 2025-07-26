using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using VagabondK.Protocols.Modbus;
using VagabondK.Protocols.Modbus.Data;

namespace VagaModbusAnalyzer
{
    public class ModbusEndianJsonConverter : JsonConverter
    {
        private static JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ModbusEndian);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = serializer.Deserialize<JObject>(reader);
            var inner = (bool)result["InnerBigEndian"];
            var outer = (bool)result["OuterBigEndian"];
            return (inner ? ModbusEndian.InnerBig : ModbusEndian.AllLittle) | (outer ? ModbusEndian.OuterBig : ModbusEndian.AllLittle);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var endian = (ModbusEndian)value;
            jsonSerializer.Serialize(writer, new
            {
                InnerBigEndian = endian.HasFlag(ModbusEndian.InnerBig),
                OuterBigEndian = endian.HasFlag(ModbusEndian.OuterBig),
            });
        }
    }
}
