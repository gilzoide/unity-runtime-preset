using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Presets;

namespace Gilzoide.RuntimePreset.Editor
{
    public static class HelperExtensions
    {
        #region Dictionary

        public static void SetNested(this Dictionary<string, object> dictionary, string propertyPath, object value)
        {
            Dictionary<string, object> dict = dictionary;
            string[] subpaths = propertyPath.Split('.');
            for (int i = 0; i < subpaths.Length - 1; i++)
            {
                string subpath = subpaths[i];
                if (!dict.TryGetValue(subpath, out object nestedDict))
                {
                    dict[subpath] = nestedDict = new Dictionary<string, object>();
                }
                dict = (Dictionary<string, object>) nestedDict;
            }

            string lastSubpath = subpaths[subpaths.Length - 1];
            dict[lastSubpath] = value;
        }

        #endregion

        #region Preset

        public static void GetIncludedPropertySet(this Preset preset, HashSet<string> set)
        {
            set.Clear();
            set.UnionWith(preset.PropertyModifications.Select(prop => prop.propertyPath));
            set.UnionWith(preset.PropertyModifications.Select(prop => prop.propertyPath.Split('.', 2)[0]));
        }

        public static void ExcludeAllPropertiesBut(this Preset preset, IEnumerable<string> includeProperties)
        {
            using (GetPooledHashSet(includeProperties, out HashSet<string> properties))
            {
                preset.GetIncludedPropertySet(properties);
                properties.ExceptWith(includeProperties);
                preset.excludedProperties = properties.ToArray();
            }
        }

        #endregion

        #region Collection Pools

#if UNITY_2021_1_OR_NEWER
        public static UnityEngine.Pool.PooledObject<List<T>> GetPooledList<T>(out List<T> set)
        {
            return UnityEngine.Pool.ListPool<T>.Get(out set);
        }

        public static UnityEngine.Pool.PooledObject<HashSet<T>> GetPooledHashSet<T>(IEnumerable<T> elements, out HashSet<T> set)
        {
            var disposable = UnityEngine.Pool.HashSetPool<T>.Get(out set);
            set.UnionWith(elements);
            return disposable;
        }
        
        public static UnityEngine.Pool.PooledObject<Dictionary<TKey, TValue>> GetPooledDictionary<TKey, TValue>(out Dictionary<TKey, TValue> dict)
        {
            return UnityEngine.Pool.DictionaryPool<TKey, TValue>.Get(out dict);
        }
#else
        public static System.IDisposable GetPooledList<T>(out List<T> set)
        {
            set = new List<T>();
            return null;
        }

        public static System.IDisposable GetPooledHashSet<T>(IEnumerable<T> elements, out HashSet<T> set)
        {
            set = new HashSet<T>(elements);
            return null;
        }

        public static System.IDisposable GetPooledDictionary<TKey, TValue>(out Dictionary<TKey, TValue> dict)
        {
            dict = new Dictionary<TKey, TValue>();
            return null;
        }
#endif

        #endregion

        #region String

        public static IEnumerable<string> EnumerateNestedJsonKeys(this string json)
        {
            using (GetPooledList(out List<string> keys))
            using (var textReader = new StringReader(json))
            using (var reader = new JsonTextReader(textReader))
            {
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.StartObject:
                            keys.Add(null);
                            break;
                        
                        case JsonToken.EndObject:
                            keys.RemoveAt(keys.Count - 1);
                            break;

                        case JsonToken.PropertyName:
                            keys[keys.Count - 1] = (string) reader.Value;
                            yield return string.Join('.', keys);
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
