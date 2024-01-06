using System;

namespace Gilzoide.RuntimePreset
{
    [Flags]
    public enum PresetApplicationEvent
    {
        Awake = 1 << 0,
        OnEnable = 1 << 1,
        Start = 1 << 2,
    }
}
