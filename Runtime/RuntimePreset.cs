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
        [SerializeField] internal string _valuesJson = "{}";
        [SerializeField] internal List<Object> _objectReferences = new List<Object>();
        
        private JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Target type for preset.
        /// Presets can only be applied to objects of this type.
        /// </summary>
        public Type TargetType
        {
            get => Type.GetType(_targetType);
            set => _targetType = value?.AssemblyQualifiedName ?? "";
        }

        /// <returns>Whether this preset can be applied to <paramref name="obj"/></returns>
        public bool CanBeAppliedTo(Object obj)
        {
            return obj != null
                && TargetType is Type targetType
                && targetType.IsAssignableFrom(obj.GetType());
        }

        /// <summary>
        /// Alias for <see cref="TryApplyTo"/> that ignores the returned value.
        /// </summary>
        /// <seealso cref="TryApplyTo"/>
        public void ApplyTo(Object targetObject)
        {
            TryApplyTo(targetObject);
        }

        /// <summary>
        /// Try applying preset values to the target object.
        /// </summary>
        /// <remarks>
        /// If the target object is <see langword="null"/> or does not inherit from <see cref="TargetType"/>, the call is a no-op.
        /// If the target object is a <see cref="GameObject"/> and this preset's target type is a component, <see cref="GetComponent"/> is used to find the correct target component.
        /// If the target object implements <see cref="IRuntimePresetListener"/> it will be notified after the values were applied.
        /// </remarks>
        /// <returns>
        /// Whether the preset's values were successfully applied to the target object.
        /// </returns>
        /// <seealso cref="ApplyTo"/>
        public bool TryApplyTo(Object targetObject)
        {
            if (targetObject is GameObject gameObject
                && TargetType is Type targetType
                && targetType.IsSubclassOf(typeof(Component)))
            {
                targetObject = gameObject.GetComponent(targetType);
            }
            if (CanBeAppliedTo(targetObject))
            {
                JsonConvert.PopulateObject(_valuesJson, targetObject, _jsonSettings);
                if (targetObject is IRuntimePresetListener presetListener)
                {
                    presetListener.OnPresetApplied();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new JsonObjectConverter(_objectReferences), new JsonLayerMaskConverter() },
                ContractResolver = JsonSerializeFieldContractResolver.Instance,
                NullValueHandling = NullValueHandling.Include,
            };
        }

        #endregion

#if UNITY_EDITOR
        public string AssetDependencyKey => $"{typeof(RuntimePreset).FullName}.{AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this))}";

        internal void MarkAssetUpdated()
        {
            AssetDatabase.RegisterCustomDependency(AssetDependencyKey, Hash128.Compute(EditorJsonUtility.ToJson(this)));
        }

        protected void OnValidate()
        {
            try
            {
                var _ = JsonConvert.DeserializeObject<Dictionary<string, object>>(_valuesJson);
            }
            catch (Exception)
            {
                _valuesJson = "{}";
            }
        }
#endif
    }
}
