using UnityEditor.Callbacks;
using UnityEngine;

namespace Gilzoide.RuntimePreset.Editor
{
    public static class PostProcessScene
    {
        [PostProcessScene]
        public static void ApplyPresetsOnImport()
        {
            foreach (RuntimePresetApplier presetApplier in Object.FindObjectsOfType<RuntimePresetApplier>(true))
            {
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
