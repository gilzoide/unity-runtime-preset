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
            List<MemberInfo> members = new List<MemberInfo>();
            foreach (FieldInfo field in objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                {
                    members.Add(field);
                }
            }
            for (Type baseType = objectType.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                foreach (FieldInfo field in baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.IsPrivate && field.GetCustomAttribute<SerializeField>() != null)
                    {
                        members.Add(field);
                    }
                }
            }
            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            return base.CreateProperty(member, MemberSerialization.Fields);
        }
    }
}
