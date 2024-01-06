using UnityEditor;
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

        private static void CreateRuntimePreset(Object obj)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Runtime Preset", obj.GetType().Name + "_preset", "asset", "");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            RuntimePreset newPreset = ScriptableObject.CreateInstance<RuntimePreset>();
            newPreset.TargetType = obj.GetType();
            // TODO: fill new preset with existing values
            AssetDatabase.CreateAsset(newPreset, path);
            EditorGUIUtility.PingObject(newPreset);
        }
    }
}
