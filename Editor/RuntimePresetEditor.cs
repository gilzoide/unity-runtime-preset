using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Gilzoide.RuntimePreset.Editor
{
    [CustomEditor(typeof(RuntimePreset))]
    public class RuntimePresetEditor : UnityEditor.Editor
    {
        private SerializedProperty targetTypeProperty;
        private SerializedProperty valuesJsonProperty;

        private Object _presetTemporaryObject;
        private GameObject _componentHolder;
        private Preset _preset;
        private HashSet<string> _includedProperties;

        void OnEnable()
        {
            targetTypeProperty = serializedObject.FindProperty(nameof(RuntimePreset._targetType));
            valuesJsonProperty = serializedObject.FindProperty(nameof(RuntimePreset._valuesJson));

            _componentHolder = new GameObject(nameof(RuntimePresetEditor))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _includedProperties = new HashSet<string>();
        }

        void OnDisable()
        {
            DestroyImmediate(_preset);
            DestroyImmediate(_componentHolder);
            DestroyImmediate(_presetTemporaryObject);
            _includedProperties = null;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            RuntimePreset runtimePreset = (RuntimePreset) serializedObject.targetObject;
            Type targetType = runtimePreset.TargetType;
            if (targetType == null)
            {
                root.Add(new HelpBox($"Could not find target type: \"{targetTypeProperty.stringValue}\"\n\n"
                    + "Please create the runtime preset from an existing object using the \"Create Runtime Preset\" context menu item.", HelpBoxMessageType.Error));
            }
            else
            {
                if (_presetTemporaryObject == null && targetType.IsSubclassOf(typeof(Component)))
                {
                    _presetTemporaryObject = _componentHolder.AddComponent(targetType);
                    Debug.Assert(runtimePreset.TryApplyTo(_presetTemporaryObject), "FIXME!!!");
                }
                if (_presetTemporaryObject == null && targetType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    _presetTemporaryObject = CreateInstance(targetType);
                    Debug.Assert(runtimePreset.TryApplyTo(_presetTemporaryObject), "FIXME!!!");
                }
                if (_preset == null)
                {
                    _preset = new Preset(_presetTemporaryObject);
                    _preset.ExcludeAllPropertiesBut(EnumerateJsonKeys());
                }

                var serializedPreset = new SerializedObject(_preset);
                var presetInspector = new InspectorElement(serializedPreset);
                presetInspector.TrackSerializedObjectValue(serializedPreset, _ =>
                {
                    _preset.GetIncludedPropertySet(_includedProperties);
                    if (!_preset.DataEquals(_presetTemporaryObject) || !_includedProperties.SetEquals(EnumerateJsonKeys()))
                    {
                        Debug.Assert(_preset.ApplyTo(_presetTemporaryObject), "FIXME!!!");
                        _preset.FillRuntimePreset(serializedObject, _presetTemporaryObject);
                    }
                    if (serializedObject.ApplyModifiedProperties())
                    {
                        runtimePreset.MarkAssetUpdated();
                    }
                });
                root.Add(presetInspector);
            }
            return root;
        }
    
        private IEnumerable<string> EnumerateJsonKeys()
        {
            return valuesJsonProperty.stringValue.EnumerateNestedJsonKeys();
        }
    }
}
