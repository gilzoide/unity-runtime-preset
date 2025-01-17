# Runtime Preset
[![openupm](https://img.shields.io/npm/v/com.gilzoide.runtime-preset?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gilzoide.runtime-preset/)

Functionality analogous to Unity's [Preset](https://docs.unity3d.com/ScriptReference/Presets.Preset.html) that can be used at runtime and prefab/scene import time.


## Features
[RuntimePreset](Runtime/RuntimePreset.cs): assets analogous to Unity's `Preset` that can be applied at runtime
- Apply presets by calling the `ApplyTo` or `TryApplyTo` methods
- Create runtime presets from existing MonoBehaviour or ScriptableObjects by using the "Create Runtime Preset" context menu item.
  This is the recommended way to create runtime presets.
- Custom editor uses `Preset` editor, making it super easy and intuitive to edit values
- Optionally implement [IRuntimePresetListener](Runtime/IRuntimePresetListener.cs) in your MonoBehaviour or ScriptableObject subclass to get notified when a runtime preset has been applied to it
- If the preset was created from a component, uses `GetComponent` when applied to a GameObject to get the correct target object

[RuntimePresetApplier](Runtime/RuntimePresetApplier.cs): component that applies presets to the corresponding target objects
- Apply presets by calling the `Apply` method
- Supports automatic application in `Awake`, `OnEnable` and `Start`
- Runs before other scripts, so that new values from automatically applied presets should be available on your components' `Awake`, `OnEnable` and `Start` methods
- Presets can be applied at prefab/scene import time by checking "On Import" in the "Apply At" flags
  + Optionally mark the checkbox "Destroy After Import" to destroy the runtime preset applier right after importing, to avoid any runtime processing
  + Prefabs with presets applied at import time are reimported automatically if preset values get changed in the inspector


## Caveats
- Only works with MonoBehaviour and ScriptableObject subclasses
- Field types that don't work yet:
  + Gradient
  + Arrays/lists


## How to install
Either:
- Use the [openupm registry](https://openupm.com/) and install this package using the [openupm-cli](https://github.com/openupm/openupm-cli):
  ```
  openupm add com.gilzoide.runtime-preset
  ```
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-runtime-preset.git#1.0.0-preview2
  ```
- Clone this repository or download a snapshot of it directly inside your project's `Assets` or `Packages` folder.
