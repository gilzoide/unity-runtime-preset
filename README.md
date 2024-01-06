# Runtime Preset
Functionality analogous to Unity's [`Preset`](https://docs.unity3d.com/ScriptReference/Presets.Preset.html) that can be used at runtime and prefab/scene import time.


## Features
[`RuntimePreset`](Runtime/RuntimePreset.cs): assets analogous to Unity's `Preset` that can be applied at runtime
- Create runtime presets from existing MonoBehaviour or ScriptableObjects by using the "Create Runtime Preset" context menu item.
  This is the recommended way to create runtime presets.
- Custom editor uses `Preset` editor, making it super easy and intuitive to edit values
- If the preset was created from a component, uses `GetComponent` when applied to a GameObject to get the target component

[`RuntimePresetApplier`](Runtime/RuntimePresetApplier.cs): component that applies presets to a target object, which by default is its own GameObject
- Supports automatic application in `Awake`, `OnEnable` and `Start`
- Runs before other scripts, so that new values from automatically applied presets should be available on your components' `Awake`, `OnEnable` and `Start` methods
- Optionally implement [IRuntimePresetListener](Runtime/IRuntimePresetListener.cs) in your MonoBehaviour or ScriptableObject subclass to get notified when a runtime preset has been applied to it
- Presets can be applied at prefab/scene import time by setting the "Apply At" flag to "On Import"
  + Optionally mark the checkbox "Destroy After Import" to destroy the runtime preset applier right after importing, to avoid any runtime processing
  + Prefabs with presets applied at import time are reimported automatically when the preset is changed


## Caveats
- Field types that don't work yet:
  + Gradient
  + Arrays/lists
- Only works with MonoBehaviour and ScriptableObject subclasses
- Runtime presets was designed to work with fields marked as public or `[SerializeField]`, just like Unity's own serialization system.
  This system does not work with properties.
