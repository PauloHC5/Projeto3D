using System;
using UnityEngine;

namespace Misc
{
    public class PulsatingLightBehaviour : MonoBehaviour
    {
        [SerializeField] private Light lightSource;
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 2f;
        
        private float originalIntensity;

        private void Start()
        {
            if (lightSource == null)
            {
                lightSource = GetComponent<Light>();
            }
            else
            {
                Debug.LogWarning("No Light component found on the GameObject. Please assign a Light component to the PulsatingLightBehaviour script.");
                return;
            }

            originalIntensity = lightSource.intensity;
        }

        private void Update()
        {
            if (lightSource != null)
            {
                float intensity = Mathf.Lerp(maxIntensity, minIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1));
                lightSource.intensity = intensity;
            }
        }

        private void OnEnable()
        {
            if (lightSource != null)
            {
                originalIntensity = lightSource.intensity; // Store the original intensity when enabled
            }
        }

        private void OnDisable()
        {
            if (lightSource != null)
            {
                lightSource.intensity = originalIntensity; // Reset to original intensity when disabled
            }
        }
    }
}
