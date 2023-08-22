using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.RuntimePreset
{
    public class RuntimePreset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private string _targetType = "";
        [SerializeField] private string _valuesJson = "{}";
        [SerializeField] private string _objectsJson = "{}";
        [SerializeField] private List<Object> _objectReferences = new List<Object>();
        
        private JsonSerializerSettings _jsonSettings;

        public Type TargetType
        {
            get
            {
#if UNITY_EDITOR
                return Type.GetType(_targetType) ?? TargetMonoScript?.GetClass();
#else
                return Type.GetType(_targetType);
#endif
            }
            set => _targetType = value.FullName;
        }

#if UNITY_EDITOR
        public MonoScript TargetMonoScript
        {
            get
            {
                string[] guid = AssetDatabase.FindAssets($"t:MonoScript {_targetType}");
                if (guid.Length != 1)
                {
                    return null;
                }
                return AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid[0]));
            }
        }
#endif

        public bool CanBeAppliedTo(Component obj)
        {
            return TargetType.IsAssignableFrom(obj.GetType());
        }

        public bool ApplyTo(Component obj)
        {
            if (CanBeAppliedTo(obj))
            {
                JsonUtility.FromJsonOverwrite(_valuesJson, obj);
                JsonConvert.PopulateObject(_objectsJson, obj, _jsonSettings);
                return true;
            }
            return false;
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Converters = new[] { new JsonObjectConverter(_objectReferences) },
                NullValueHandling = NullValueHandling.Include,
            };
        }

        #endregion

#if UNITY_EDITOR
        void OnValidate()
        {
            try
            {
                var _ = JsonConvert.DeserializeObject<Dictionary<string, object>>(_valuesJson);
            }
            catch (Exception)
            {
                _valuesJson = "{}";
            }

            try
            {
                var _ = JsonConvert.DeserializeObject<Dictionary<string, object>>(_objectsJson);
            }
            catch (Exception)
            {
                _objectsJson = "{}";
            }
        }
#endif
    }
}
