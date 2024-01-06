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
        [SerializeField] internal string _targetType = "";
        [SerializeField] internal string _unityJson = "{}";
        [SerializeField] internal string _newtonsoftJson = "{}";
        [SerializeField] internal List<Object> _objectReferences = new List<Object>();
        
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
                JsonUtility.FromJsonOverwrite(_unityJson, obj);
                JsonConvert.PopulateObject(_newtonsoftJson, obj, _jsonSettings);
                if (obj is IRuntimePresetListener presetListener)
                {
                    presetListener.OnPresetApplied();
                }
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
        public string AssetDependencyKey => $"{typeof(RuntimePreset).FullName}.{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this))}";

        internal void AssetUpdated()
        {
            AssetDatabase.RegisterCustomDependency(AssetDependencyKey, Hash128.Compute(EditorJsonUtility.ToJson(this)));
        }

        void OnValidate()
        {
            try
            {
                var _ = JsonConvert.DeserializeObject<Dictionary<string, object>>(_unityJson);
            }
            catch (Exception)
            {
                _unityJson = "{}";
            }

            try
            {
                var _ = JsonConvert.DeserializeObject<Dictionary<string, object>>(_newtonsoftJson);
            }
            catch (Exception)
            {
                _newtonsoftJson = "{}";
            }
        }
#endif
    }
}
