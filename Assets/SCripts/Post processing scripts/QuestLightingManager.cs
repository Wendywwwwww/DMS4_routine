using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Changes sun lighting and HDRP fog tint/attenuation smoothly when entering a trigger zone.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class QuestLightingManager : MonoBehaviour
{
    [Header("Target References (PLEASE DRAG MANUALLY!)")]
    [Tooltip("Drag your directional light (sun) here.")]
    public Light sunLight;
    [Tooltip("Drag your HDRP Global Volume here.")]
    public Volume globalVolume;

    [Header("Zone Lighting Settings")]
    public Color zoneSunColor = Color.white;
    public float zoneSunIntensity = 50000f; // Default to a real HDRP daylight value

    [Header("Zone HDRP Fog Settings")]
    public Color zoneFogTint = Color.yellow;
    public float zoneFogAttenuation = 10f;

    [Header("Transition Settings")]
    public float transitionDuration = 2.0f;

    private Coroutine activeTransition;

    void Awake()
    {
        BoxCollider boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        // Fallback auto-find if you forgot to drag it in
        if (sunLight == null)
        {
            sunLight = Object.FindFirstObjectByType<Light>();
        }
        if (globalVolume == null)
        {
            globalVolume = Object.FindFirstObjectByType<Volume>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[QuestLightingManager] Trigger entered by '{other.name}' on zone '{gameObject.name}'");

        // Safety check before starting
        if (sunLight == null)
        {
            Debug.LogError($"[QuestLightingManager] ERROR: No Sun Light assigned on zone '{gameObject.name}'! Drag your sun into the slot!");
            return;
        }

        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }
        activeTransition = StartCoroutine(BlendToZoneLighting());
    }

    private IEnumerator BlendToZoneLighting()
    {
        float elapsedTime = 0f;

        Color startSunColor = sunLight.color;
        float startSunIntensity = sunLight.intensity;

        Color startFogTint = Color.white;
        float startFogAttenuation = 50f;

        Fog hdrpFog = null;
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out hdrpFog);
            if (hdrpFog != null)
            {
                startFogTint = hdrpFog.tint.value;
                startFogAttenuation = hdrpFog.meanFreePath.value;
            }
        }

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            // Directly modify the sun
            sunLight.color = Color.Lerp(startSunColor, zoneSunColor, t);
            sunLight.intensity = Mathf.Lerp(startSunIntensity, zoneSunIntensity, t);

            if (hdrpFog != null)
            {
                hdrpFog.tint.value = Color.Lerp(startFogTint, zoneFogTint, t);
                hdrpFog.meanFreePath.value = Mathf.Lerp(startFogAttenuation, zoneFogAttenuation, t);
            }

            yield return null;
        }

        // Force final exact values just in case
        sunLight.intensity = zoneSunIntensity;
        sunLight.color = zoneSunColor;
    }
}