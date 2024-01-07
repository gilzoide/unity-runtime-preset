using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.RuntimePreset.Editor
{
    [CustomEditor(typeof(RuntimePreset))]
    public class RuntimePresetEditor : UnityEditor.Editor
    {
        private readonly GUIContent _targetTypeContent = new GUIContent("Target Type");

        private SerializedProperty targetTypeProperty;
        private SerializedProperty valuesJsonProperty;

        private Object _presetTemporaryObject;
        private GameObject _componentHolder;
        private Preset _preset;
        private UnityEditor.Editor _presetEditor;
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
            DestroyImmediate(_presetEditor);
            DestroyImmediate(_componentHolder);
            DestroyImmediate(_presetTemporaryObject);
            _includedProperties = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targetTypeProperty.hasMultipleDifferentValues)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(targetTypeProperty, _targetTypeContent);
                }
                return;
            }


            RuntimePreset runtimePreset = (RuntimePreset) serializedObject.targetObject;
            Type targetType = runtimePreset.TargetType;
            if (targetType == null)
            {
                EditorGUILayout.HelpBox($"Could not find target type: '{targetTypeProperty.stringValue}'", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();

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
            if (_presetEditor == null)
            {
                _presetEditor = CreateEditor(_preset);
            }

            _presetEditor.OnInspectorGUI();
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
        }

        private IEnumerable<string> EnumerateJsonKeys()
        {
            return valuesJsonProperty.stringValue.EnumerateNestedJsonKeys();
        }
    }
}
