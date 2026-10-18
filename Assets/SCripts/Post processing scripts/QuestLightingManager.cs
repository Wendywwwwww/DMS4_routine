using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Changes sun lighting and HDRP fog settings smoothly when the player enters a trigger zone.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class QuestLightingManager : MonoBehaviour
{
    [Header("Target References (Assign once or leave empty to auto-find)")]
    [Tooltip("Drag your directional light (sun) here.")]
    public Light sunLight;
    [Tooltip("Drag your HDRP Global Volume here.")]
    public Volume globalVolume;

    [Header("Zone Lighting Settings")]
    [Tooltip("Target color/brightness for the sun in this zone.")]
    public Color zoneSunColor = Color.white;
    [Tooltip("Target intensity/brightness level for the sun in this zone.")]
    public float zoneSunIntensity = 1.0f;

    [Header("Zone HDRP Fog Settings")]
    [Tooltip("Target fog color for this zone.")]
    public Color zoneFogColor = Color.gray;
    [Tooltip("Target fog attenuation / mean free path for this zone (lower = thicker fog).")]
    public float zoneFogAttenuation = 20f;

    [Header("Transition Settings")]
    [Tooltip("How many seconds the lighting transition takes when entering the zone.")]
    public float transitionDuration = 2.0f;

    private static Coroutine activeTransition;

    void Awake()
    {
        // Ensure the collider is strictly set as a trigger
        BoxCollider boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        // Auto-find references if left empty in Inspector using modern non-deprecated Unity methods
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
        // Check if it's the player
        if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }
            activeTransition = StartCoroutine(BlendToZoneLighting());
            Debug.Log($"[QuestLightingManager] Player entered zone: {gameObject.name}. Transitioning atmosphere.");
        }
    }

    /// <summary>
    /// Smoothly blends current scene lighting/fog into this zone's settings.
    /// </summary>
    private IEnumerator BlendToZoneLighting()
    {
        float elapsedTime = 0f;

        // Capture starting values from the scene right now
        Color startSunColor = sunLight != null ? sunLight.color : Color.white;
        float startSunIntensity = sunLight != null ? sunLight.intensity : 1.0f;

        Color startFogColor = Color.white;
        float startFogAttenuation = 50f;

        Fog hdrpFog = null;
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out hdrpFog);
            if (hdrpFog != null)
            {
                startFogColor = hdrpFog.color.value;
                startFogAttenuation = hdrpFog.meanFreePath.value;
            }
        }

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            // Lerp Sun
            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(startSunColor, zoneSunColor, t);
                sunLight.intensity = Mathf.Lerp(startSunIntensity, zoneSunIntensity, t);
            }

            // Lerp HDRP Fog
            if (hdrpFog != null)
            {
                hdrpFog.color.value = Color.Lerp(startFogColor, zoneFogColor, t);
                hdrpFog.meanFreePath.value = Mathf.Lerp(startFogAttenuation, zoneFogAttenuation, t);
            }

            yield return null;
        }
    }
}