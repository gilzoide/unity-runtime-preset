using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.RuntimePreset
{
    public class RuntimePresetApplier : MonoBehaviour
    {
        [Tooltip("Presets that will be applied to this GameObject when Apply is called")]
        [SerializeField] protected List<RuntimePreset> _presets;
        
        [Tooltip("Events where presets will be applied automatically")]
        [SerializeField] protected PresetApplicationEvent _applyAt = PresetApplicationEvent.Awake;
        
        [Tooltip("If true, this component will be destroyed right after the prefab or scene it belongs to gets imported")]
        [SerializeField] protected internal bool _destroyAfterImport = false;

        /// <summary>Presets that will be applied to this GameObject when Apply is called</summary>
        public List<RuntimePreset> Presets => _presets;

        /// <summary>Events where presets will be applied automatically</summary>
        public PresetApplicationEvent ApplyAt
        {
            get => _applyAt;
            set => _applyAt = value;
        }

        /// <summary>If true, this component will be destroyed right after the prefab or scene it belongs to gets imported</summary>
        public bool DestroyAfterImport
        {
            get => _destroyAfterImport;
            set => _destroyAfterImport = value;
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

        /// <summary>Apply all presets to this GameObject</summary>
        [ContextMenu("Apply Preset")]
        public void Apply()
        {
            Object target = gameObject;
            foreach (RuntimePreset runtimePreset in _presets)
            {
                if (runtimePreset)
                {
                    runtimePreset.ApplyTo(target);
                }
            }
        }
    }
}
