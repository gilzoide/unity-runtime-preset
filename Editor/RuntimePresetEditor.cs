using System.Collections.Generic;
using System.Linq;
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
        private SerializedProperty valuesJsonProperty;
        private SerializedProperty objectsJsonProperty;
        private SerializedProperty objectReferencesProperty;

        private Component _component;
        private GameObject _componentHolder;
        private Preset _preset;
        private UnityEditor.Editor _presetEditor;
        private HashSet<string> _includedProperties;

        void OnEnable()
        {
            targetTypeProperty = serializedObject.FindProperty("_targetType");
            valuesJsonProperty = serializedObject.FindProperty("_valuesJson");
            objectsJsonProperty = serializedObject.FindProperty("_objectsJson");
            objectReferencesProperty = serializedObject.FindProperty("_objectReferences");

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
                    valuesJsonProperty.stringValue = "";
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
                _preset.ExcludeAllPropertiesBut(EnumerateJsonKeys());
            }
            if (_presetEditor == null)
            {
                _presetEditor = CreateEditor(_preset);
            }

            EditorGUI.BeginChangeCheck();
            _presetEditor.OnInspectorGUI();
            _preset.GetIncludedPropertySet(_includedProperties);
            if (EditorGUI.EndChangeCheck() || !_includedProperties.SetEquals(EnumerateJsonKeys()))
            {
                FillModifiedValuesJson(_preset, _component);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FillModifiedValuesJson(Preset preset, Component component)
        {
            objectReferencesProperty.ClearArray();

            using (HelperExtensions.GetPooledDictionary<string, object>(out var values))
            using (HelperExtensions.GetPooledDictionary<string, object>(out var objects))
            {
                Debug.Assert(preset.ApplyTo(component), "FIXME!!!");

                var serializedComponent = new SerializedObject(component);
                foreach (PropertyModification modification in preset.PropertyModifications)
                {
                    SerializedProperty property = serializedComponent.FindProperty(modification.propertyPath);
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.Boolean:
                            values.SetNested(property.propertyPath, property.boolValue);
                            break;
                        case SerializedPropertyType.Integer:
                            values.SetNested(property.propertyPath, property.intValue);
                            break;
                        case SerializedPropertyType.Float:
                            values.SetNested(property.propertyPath, property.doubleValue);
                            break;
                        case SerializedPropertyType.Character:
                            values.SetNested(property.propertyPath, property.intValue);
                            break;
                        case SerializedPropertyType.String:
                            values.SetNested(property.propertyPath, property.stringValue);
                            break;
                        case SerializedPropertyType.Enum:
                            values.SetNested(property.propertyPath, property.intValue);
                            break;
                        case SerializedPropertyType.ObjectReference:
                            if (property.objectReferenceValue != null)
                            {
                                int index = objectReferencesProperty.arraySize;
                                objectReferencesProperty.InsertArrayElementAtIndex(index);
                                objectReferencesProperty.GetArrayElementAtIndex(index).objectReferenceValue = property.objectReferenceValue;
                                objects.SetNested(property.propertyPath, index);
                            }
                            else
                            {
                                objects.SetNested(property.propertyPath, -1);
                            }
                            break;
                        default:
                            Debug.LogWarning($"[{nameof(RuntimePreset)}] Type {property.propertyType} is not supported (path: {property.propertyPath})");
                            break;
                    }
                }

                valuesJsonProperty.stringValue = JsonConvert.SerializeObject(values);
                objectsJsonProperty.stringValue = JsonConvert.SerializeObject(objects);
            }
        }

        private IEnumerable<string> EnumerateJsonKeys()
        {
            return valuesJsonProperty.stringValue.EnumerateNestedJsonKeys()
                .Concat(objectsJsonProperty.stringValue.EnumerateNestedJsonKeys());
        }
    }
}
