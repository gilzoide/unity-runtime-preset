using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Gilzoide.RuntimePreset
{
    public class JsonSerializeFieldContractResolver : DefaultContractResolver
    {
        public static JsonSerializeFieldContractResolver Instance => _instance != null ? _instance : (_instance = new JsonSerializeFieldContractResolver());
        private static JsonSerializeFieldContractResolver _instance;

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            List<MemberInfo> members = base.GetSerializableMembers(objectType);
            foreach (FieldInfo field in objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (field.GetCustomAttribute<SerializeField>() != null)
                {
                    members.Add(field);
                }
            }
            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
            if (member is FieldInfo && member.GetCustomAttribute<SerializeField>() != null)
            {
                jsonProperty.Ignored = false;
                jsonProperty.Writable = true;
                jsonProperty.Readable = true;
                jsonProperty.HasMemberAttribute = true;
            }
            return jsonProperty;
        }
    }
}
