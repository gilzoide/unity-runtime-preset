using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Gilzoide.RuntimePreset.Editor
{
    public static class ContextMenuItems
    {
        [MenuItem("CONTEXT/MonoBehaviour/Create Runtime Preset", priority = 10000)]
        private static void CreateRuntimePresetFromMonoBehaviour(MenuCommand command)
        {
            if (command.context is MonoBehaviour component)
            {
                CreateRuntimePreset(component);
            }
        }

        [MenuItem("CONTEXT/ScriptableObject/Create Runtime Preset", priority = 10000)]
        private static void CreateRuntimePresetFromScriptableObject(MenuCommand command)
        {
            if (command.context is ScriptableObject obj)
            {
                CreateRuntimePreset(obj);
            }
        }

        [MenuItem("CONTEXT/ScriptableObject/Create Runtime Preset", isValidateFunction: true)]
        private static bool CanCreateRuntimePreset(MenuCommand command)
        {
            return command.context is not RuntimePreset;
        }

        private static void CreateRuntimePreset(Object obj)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Runtime Preset", obj.GetType().Name + "_preset", "asset", "");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            RuntimePreset runtimePreset = ScriptableObject.CreateInstance<RuntimePreset>();
            runtimePreset.TargetType = obj.GetType();

            var tempPreset = new Preset(obj);
            tempPreset.FillRuntimePreset(runtimePreset, obj);
            Object.DestroyImmediate(tempPreset);
            
            AssetDatabase.CreateAsset(runtimePreset, path);
            EditorGUIUtility.PingObject(runtimePreset);
        }
    }
}
