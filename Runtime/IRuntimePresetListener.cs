namespace Gilzoide.RuntimePreset
{
    /// <summary>
    /// Implement this interface in your <see cref="UnityEngine.MonoBehaviour"/> and <see cref="UnityEngine.ScriptableObject"/> subclasses to get notified when runtime presets are applied to them.
    /// </summary>
    public interface IRuntimePresetListener
    {
        void OnPresetApplied();
    }
}
