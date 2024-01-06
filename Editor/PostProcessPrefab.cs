using UnityEditor;
using UnityEngine;

namespace Gilzoide.RuntimePreset.Editor
{
    public class PostProcessPrefab : AssetPostprocessor
    {
        void OnPostprocessPrefab(GameObject gameObject)
        {
            foreach (RuntimePresetApplier presetApplier in gameObject.GetComponentsInChildren<RuntimePresetApplier>(true))
            {
                foreach (RuntimePreset runtimePreset in presetApplier.Presets)
                {
                    if (runtimePreset)
                    {
                        context.DependsOnCustomDependency(runtimePreset.AssetDependencyKey);
                    }
                }
                if (presetApplier.ApplyAt.HasFlag(PresetApplicationEvent.OnImport))
                {
                    presetApplier.Apply();
                }
                if (presetApplier._destroyAfterImport)
                {
                    Object.DestroyImmediate(presetApplier, true);
                }
            }
        }
    }
}
