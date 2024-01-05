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
            get => Type.GetType(_targetType);
            set => _targetType = value?.AssemblyQualifiedName ?? "";
        }

        public bool CanBeAppliedTo(Object obj)
        {
            return obj != null
                && TargetType is Type targetType
                && targetType.IsAssignableFrom(obj.GetType());
        }

        public void ApplyTo(Object obj)
        {
            TryApplyTo(obj);
        }

        public bool TryApplyTo(Object obj)
        {
            Type targetType = TargetType;
            if (targetType != null && obj is GameObject gameObject && targetType.IsSubclassOf(typeof(Component)))
            {
                obj = gameObject.GetComponent(targetType);
            }
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
                ContractResolver = JsonSerializeFieldContractResolver.Instance,
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
