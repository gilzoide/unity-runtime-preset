using UnityEditor;
using UnityEngine;

namespace Gilzoide.RuntimePreset.Editor
{
    public static class ContextMenuItems
    {
        [MenuItem("CONTEXT/MonoBehaviour/Create Runtime Preset", priority = 10000)]
        private static void CreateRuntimePreset(MenuCommand command)
        {
            if (command.context is MonoBehaviour component)
            {
                string path = EditorUtility.SaveFilePanelInProject("Create Runtime Preset", component.GetType().Name + "_preset", "asset", "");
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                RuntimePreset newPreset = ScriptableObject.CreateInstance<RuntimePreset>();
                newPreset.TargetType = component.GetType();
                // TODO: fill new preset with existing values
                AssetDatabase.CreateAsset(newPreset, path);
                EditorGUIUtility.PingObject(newPreset);
            }
        }
    }
}
