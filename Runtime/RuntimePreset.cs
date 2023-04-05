using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.RuntimePreset
{
    public class RuntimePreset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private string _targetType;
        [SerializeField] private string _valuesJson;
        private Dictionary<string, object> _propertyValues;
        
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

        public IReadOnlyDictionary<string, object> PropertyValues => GetPropertyValues();

        public object this[string property]
        {
            get => PropertyValues.TryGetValue(property, out object value) ? value : null;
            set
            {
                LoadPropertyValuesIfNeeded();
                _propertyValues[property] = value;
            }
        }

        public bool CanBeAppliedTo(Component obj)
        {
            return TargetType.IsAssignableFrom(obj.GetType());
        }

        public bool ApplyTo(Component obj)
        {
            if (CanBeAppliedTo(obj))
            {
                JsonUtility.FromJsonOverwrite(_valuesJson, obj);
                return true;
            }
            return false;
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            if (_propertyValues != null)
            {
                _valuesJson = JsonConvert.SerializeObject(_propertyValues);
            }
        }

        public void OnAfterDeserialize()
        {
        }

        #endregion

        private Dictionary<string, object> GetPropertyValues()
        {
            LoadPropertyValuesIfNeeded();
            return _propertyValues;
        }

        private void LoadPropertyValuesIfNeeded()
        {
            if (_propertyValues != null)
            {
                return;
            }

            try
            {
                _propertyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(_valuesJson);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (_propertyValues == null)
                {
                    _propertyValues = new Dictionary<string, object>();
                }

            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _propertyValues = null;
        }
#endif
    }
}
