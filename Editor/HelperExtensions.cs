using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

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

        public static void FillRuntimePreset(this Preset preset, RuntimePreset runtimePreset, Object objectWithPresetApplied)
        {
            var serializedRuntimePreset = new SerializedObject(runtimePreset);
            preset.FillRuntimePreset(serializedRuntimePreset, objectWithPresetApplied);
            serializedRuntimePreset.ApplyModifiedProperties();
        }

        public static void FillRuntimePreset(this Preset preset, SerializedObject serializedRuntimePreset, Object objectWithPresetApplied)
        {
            SerializedProperty valuesJsonProperty = serializedRuntimePreset.FindProperty(nameof(RuntimePreset._valuesJson));
            SerializedProperty objectReferencesProperty = serializedRuntimePreset.FindProperty(nameof(RuntimePreset._objectReferences));

            objectReferencesProperty.ClearArray();

            using (GetPooledDictionary<string, object>(out var values))
            {
                using (var serializedObj = new SerializedObject(objectWithPresetApplied))
                foreach (PropertyModification modification in preset.PropertyModifications)
                {
                    SerializedProperty property = serializedObj.FindProperty(modification.propertyPath);
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.Boolean:
                            values.SetNested(property.propertyPath, property.boolValue);
                            break;
                        case SerializedPropertyType.Integer:
                        case SerializedPropertyType.Character:
                        case SerializedPropertyType.Enum:
                            values.SetNested(property.propertyPath, property.longValue);
                            break;
                        case SerializedPropertyType.Float:
                            values.SetNested(property.propertyPath, property.doubleValue);
                            break;
                        case SerializedPropertyType.String:
                            values.SetNested(property.propertyPath, property.stringValue);
                            break;
                        case SerializedPropertyType.ObjectReference:
                            if (property.objectReferenceValue != null)
                            {
                                int index = objectReferencesProperty.arraySize;
                                objectReferencesProperty.InsertArrayElementAtIndex(index);
                                objectReferencesProperty.GetArrayElementAtIndex(index).objectReferenceValue = property.objectReferenceValue;
                                values.SetNested(property.propertyPath, index);
                            }
                            else
                            {
                                values.SetNested(property.propertyPath, -1);
                            }
                            break;
                        default:
                            Debug.LogWarning($"[{nameof(RuntimePreset)}] Type {property.propertyType} is not supported (path: {property.propertyPath})");
                            break;
                    }
                }

                valuesJsonProperty.stringValue = JsonConvert.SerializeObject(values);
            }
        }

        #endregion

        #region Collection Pools

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
