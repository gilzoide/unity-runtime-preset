using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.RuntimePreset
{
    public class RuntimePresetApplier : MonoBehaviour
    {
        [Serializable]
        public struct PresetApplication
        {
            [Tooltip("Target object that will have its values updated by Preset")]
            public Object Target;

            [Tooltip("Preset that will be applied to Target object")]
            public RuntimePreset Preset;

            public void Apply()
            {
                if (Preset)
                {
                    Preset.ApplyTo(Target);
                }
            }

            public void Deconstruct(out Object key, out RuntimePreset value)
            {
                key = Target;
                value = Preset;
            }
        }

        [Tooltip("Presets that will be applied to their Target objects when Apply is called")]
        [SerializeField] protected List<PresetApplication> _presetApplications;
        
        [Tooltip("Events where presets will be applied automatically")]
        [SerializeField] protected PresetApplicationEvent _applyAt = PresetApplicationEvent.Awake;
        
        [Tooltip("If true, this component will be destroyed right after the prefab or scene it belongs to gets imported")]
        [SerializeField] protected bool _destroyAfterImport = false;

        /// <summary>Presets that will be applied to their Target objects when Apply is called</summary>
        public List<PresetApplication> PresetApplications => _presetApplications;

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

        /// <summary>Execute all preset applications</summary>
        [ContextMenu("Apply Presets")]
        public void Apply()
        {
            foreach (PresetApplication presetApplication in _presetApplications)
            {
                presetApplication.Apply();
            }
        }
    }
}
