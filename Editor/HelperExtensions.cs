using System.Collections.Generic;
using System.Linq;
using UnityEditor.Presets;

namespace Gilzoide.RuntimePreset.Editor
{
    public static class HelperExtensions
    {
        public static void SetNested(this Dictionary<string, object> dictionary, string propertyPath, object value)
        {
            Dictionary<string, object> dict = dictionary;
            foreach ((bool isLast, string subpath) in propertyPath.SplitEnumerate('.'))
            {
                if (isLast)
                {
                    dict[subpath] = value;
                }
                else
                {
                    if (!dict.TryGetValue(subpath, out object nestedDict))
                    {
                        dict[subpath] = nestedDict = new Dictionary<string, object>();
                    }
                    dict = (Dictionary<string, object>) nestedDict;
                }
            }
        }

        public static IEnumerable<(bool IsLast, string SubStr)> SplitEnumerate(this string str, char character)
        {
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOf(character, startIndex);
                if (index >= 0)
                {
                    yield return (false, str.Substring(startIndex, index - startIndex));
                    startIndex = index + 1;
                }
                else
                {
                    break;
                }
            }
            yield return (true, str.Substring(startIndex));
        }

        public static HashSet<string> GetIncludedPropertySet(this Preset preset)
        {
            var set = new HashSet<string>();
            preset.GetIncludedPropertySet(set);
            return set;
        }

        public static void GetIncludedPropertySet(this Preset preset, HashSet<string> set)
        {
            set.Clear();
            set.UnionWith(preset.PropertyModifications.Select(prop => prop.propertyPath.SplitEnumerate('.').First().SubStr));
        }

        public static void ExcludeAllPropertiesBut(this Preset preset, IEnumerable<string> includeProperties)
        {
            var properties = preset.GetIncludedPropertySet();
            properties.ExceptWith(includeProperties);
            preset.excludedProperties = properties.ToArray();
        }
    }
}
