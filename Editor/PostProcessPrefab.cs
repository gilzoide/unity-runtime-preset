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
                if (presetApplier.ApplyAt.HasFlag(PresetApplicationEvent.OnImport))
                {
                    foreach ((_, RuntimePreset runtimePreset) in presetApplier.PresetApplications)
                    {
                        if (runtimePreset)
                        {
                            context.DependsOnCustomDependency(runtimePreset.AssetDependencyKey);
                        }
                    }
                    presetApplier.Apply();
                }
                if (presetApplier.DestroyAfterImport)
                {
                    Object.DestroyImmediate(presetApplier, true);
                }
            }
        }
    }
}
