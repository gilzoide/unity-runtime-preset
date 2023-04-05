using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Gilzoide.RuntimePreset.Editor
{
    [CustomEditor(typeof(RuntimePreset))]
    public class RuntimePresetEditor : UnityEditor.Editor
    {
        private readonly GUIContent _targetTypeContent = new GUIContent("Target Type");

        private SerializedProperty targetTypeProperty;
        private SerializedProperty jsonProperty;

        private Component _component;
        private GameObject _componentHolder;
        private Preset _preset;
        private UnityEditor.Editor _presetEditor;
        private HashSet<string> _includedProperties;

        void OnEnable()
        {
            targetTypeProperty = serializedObject.FindProperty("_targetType");
            jsonProperty = serializedObject.FindProperty("_valuesJson");

            _componentHolder = new GameObject(nameof(RuntimePresetEditor));
            _componentHolder.hideFlags = HideFlags.HideAndDontSave;

            _includedProperties = new HashSet<string>();
        }

        void OnDisable()
        {
            DestroyImmediate(_preset);
            DestroyImmediate(_presetEditor);
            DestroyImmediate(_componentHolder);
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
            MonoScript monoScript = runtimePreset.TargetMonoScript;

            MonoScript newMonoScript = (MonoScript) EditorGUILayout.ObjectField(_targetTypeContent, monoScript, typeof(MonoScript), false);
            if (newMonoScript && !newMonoScript.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
            {
                return;
            }

            if (newMonoScript != monoScript)
            {
                targetTypeProperty.stringValue = newMonoScript != null ? newMonoScript.GetClass().FullName : "";
                if (newMonoScript == null)
                {
                    jsonProperty.stringValue = "";
                }
                DestroyImmediate(_preset);
                DestroyImmediate(_presetEditor);
                DestroyImmediate(_component);
                serializedObject.ApplyModifiedProperties();
            }

            if (newMonoScript == null)
            {
                return;
            }

            EditorGUILayout.Space();

            if (_component == null)
            {
                _component = _componentHolder.AddComponent(newMonoScript.GetClass());
                Debug.Assert(runtimePreset.ApplyTo(_component), "FIXME!!!");
            }
            if (_preset == null)
            {
                _preset = new Preset(_component);
                _preset.ExcludeAllPropertiesBut(runtimePreset.PropertyValues.Keys);
            }
            if (_presetEditor == null)
            {
                _presetEditor = CreateEditor(_preset);
            }

            EditorGUI.BeginChangeCheck();
            _presetEditor.OnInspectorGUI();
            _preset.GetIncludedPropertySet(_includedProperties);
            if (EditorGUI.EndChangeCheck() || !_includedProperties.SetEquals(runtimePreset.PropertyValues.Keys))
            {
                jsonProperty.stringValue = GetModifiedValuesJson(_preset, _component);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private string GetModifiedValuesJson(Preset preset, Component component)
        {
            var values = new Dictionary<string, object>();
            Debug.Assert(preset.ApplyTo(component), "FIXME!!!");

            var serializedComponent = new SerializedObject(component);
            foreach (PropertyModification modification in preset.PropertyModifications)
            {
                SerializedProperty property = serializedComponent.FindProperty(modification.propertyPath);
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Boolean: values.SetNested(property.propertyPath, property.boolValue); break;
                    case SerializedPropertyType.Integer: values.SetNested(property.propertyPath, property.intValue); break;
                    case SerializedPropertyType.Float: values.SetNested(property.propertyPath, property.doubleValue); break;
                    case SerializedPropertyType.String: values.SetNested(property.propertyPath, property.stringValue); break;
                }
            }

            return JsonConvert.SerializeObject(values);
        }
    }
}
