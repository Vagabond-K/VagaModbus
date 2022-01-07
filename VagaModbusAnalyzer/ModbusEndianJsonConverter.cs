using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
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
            return new ModbusEndian((bool)result["InnerBigEndian"], (bool)result["OuterBigEndian"]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            jsonSerializer.Serialize(writer, value);
        }
    }
}
