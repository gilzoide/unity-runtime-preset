using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Gilzoide.RuntimePreset
{
    public class JsonLayerMaskConverter : JsonConverter<LayerMask>
    {
        public override LayerMask ReadJson(JsonReader reader, Type objectType, LayerMask existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    JObject obj = JObject.Load(reader);
                    return obj.GetValue("m_Bits")?.Value<int>() ?? default;
                
                default:
                    return reader.ReadAsInt32() ?? default;
            }
        }

        public override void WriteJson(JsonWriter writer, LayerMask value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("m_Bits");
            writer.WriteValue((int) value);
            writer.WriteEndObject();
        }
    }
}
