using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace Gilzoide.RuntimePreset
{
    [Serializable]
    public class JsonObjectConverter : JsonConverter<Object>
    {
        public JsonObjectConverter(List<Object> objects)
        {
            _objects = objects;
        }

        private readonly List<Object> _objects;

        public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                int index = Convert.ToInt32(reader.Value);
                return _objects[index];
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(-1);
            }
            else
            {
                int index = _objects.IndexOf(value);
                if (index < 0)
                {
                    index = _objects.Count;
                    _objects.Add(value);
                }
                writer.WriteValue(index);
            }
        }
    }
}
