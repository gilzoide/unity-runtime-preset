using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.RuntimePreset
{
    public class RuntimePresetApplier : MonoBehaviour
    {
        [SerializeField] protected List<RuntimePreset> _presets;
        [SerializeField] protected PresetApplicationEvent _applyAt = PresetApplicationEvent.Awake;

        public List<RuntimePreset> Presets => _presets;
        public PresetApplicationEvent ApplyAt
        {
            get => _applyAt;
            set => _applyAt = value;
        }

        protected void Awake()
        {
            if (_applyAt.HasFlag(PresetApplicationEvent.Awake))
            {
                Apply();
            }
        }

        protected void OnEnable()
        {
            if (_applyAt.HasFlag(PresetApplicationEvent.OnEnable))
            {
                Apply();
            }
        }

        protected void Start()
        {
            if (_applyAt.HasFlag(PresetApplicationEvent.Start))
            {
                Apply();
            }
        }

        [ContextMenu("Apply Preset")]
        public void Apply()
        {
            foreach (RuntimePreset runtimePreset in _presets)
            {
                runtimePreset.ApplyTo(gameObject);
            }
        }
    }
}
