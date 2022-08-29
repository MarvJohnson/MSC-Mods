using MSCLoader;
using UnityEngine;

namespace Menthus15Mods.Voicemail
{
    [RequireComponent(typeof(Material))]
    public class NewMessagesLight : MonoBehaviour
    {
        private const string MaterialEmissionColorKey = "_EmissionColor";
        [field: SerializeField]
        private Color OnColor { get; set; }
        [field: SerializeField]
        private Color OffColor { get; set; }
        [field: SerializeField]
        private float FlashRate { get; set; }
        [field: SerializeField]
        private Light _Light { get; set; }
        private MeshRenderer _Material { get; set; }
        private bool LightState { get; set; }
        private float EmissionIntensity { get; } = 1000f;
        private Color Invisible { get; } = new Color(0f, 0f, 0f, 0f);

        public void EnableFlashing()
        {
            Disable(true);
            InvokeRepeating(nameof(ToggleLight), 0f, FlashRate);
        }

        public void Enable(bool stopFlashing)
        {
            if (stopFlashing)
                CancelInvoke(nameof(ToggleLight));

            _Material.material.color = OnColor;
            _Material.material.SetColor(MaterialEmissionColorKey, OnColor * EmissionIntensity);
            _Light.enabled = true;
        }

        public void Disable(bool stopFlashing)
        {
            if (stopFlashing)
                CancelInvoke(nameof(ToggleLight));

            _Material.material.color = OffColor;
            _Material.material.SetColor(MaterialEmissionColorKey, Invisible);
            _Light.enabled = false;
        }

        private void ToggleLight()
        {
            LightState = !LightState;

            if (LightState)
                Enable(false);
            else
                Disable(false);
        }

        private void Awake()
        {
            _Material = GetComponent<MeshRenderer>();
            _Light.color = OnColor;
        }

        private void OnDisable() => Disable(true);
    }
}