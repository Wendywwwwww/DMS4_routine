using UnityEngine;
using UnityEngine.UI; // Required for the white flash image
using UnityEngine.SceneManagement; // Required for scene transition
using System.Collections;

/// <summary>
/// A standalone trigger script that executes a timed horror sequence:
/// Delay -> Elevator Shake -> Light & Material Flicker -> White Screen Flash -> Load Scene.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ElevatorHorrorSequence : MonoBehaviour
{
    [Header("Sequence Timings")]
    [Tooltip("How long to wait after entering the trigger before the shake starts.")]
    public float initialDelay = 2f;
    [Tooltip("How long the elevator shakes before the lights start flickering.")]
    public float shakeDurationBeforeFlicker = 3f;
    [Tooltip("How long the lights flicker before the white flash starts.")]
    public float flickerDurationBeforeFlash = 4f;
    [Tooltip("How long the white flash takes to fully cover the screen.")]
    public float flashDuration = 1.5f;

    [Header("Elevator Shake Settings")]
    [Tooltip("The mesh or container of the elevator that will shake.")]
    public Transform shakeTarget;
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 25f;

    [Header("Light & Emission Flicker Settings")]
    [Tooltip("Real lights that will flicker.")]
    public Light[] targetLights;
    [Tooltip("Meshes with emissive materials that will flicker.")]
    public MeshRenderer[] emissiveRenderers;
    [Tooltip("The base emission color. It will be multiplied to create the flicker effect.")]
    [ColorUsage(true, true)] public Color baseEmissionColor = Color.white;
    public float minFlickerIntensity = 0f;
    public float maxFlickerIntensity = 2f;
    public float flickerSpeed = 0.05f;

    [Header("Scene Transition")]
    [Tooltip("The UI Image used for the white flash (must cover the whole screen).")]
    public Image whiteFlashImage;
    [Tooltip("The exact name of the scene to load at the climax.")]
    public string nextSceneName = "Scene4";

    private bool hasTriggered = false;
    private bool isShaking = false;
    private bool isFlickering = false;
    private Vector3 originalLocalPos;

    private void Start()
    {
        // Ensure the trigger is set up correctly
        GetComponent<Collider>().isTrigger = true;

        // Ensure the flash image is transparent at the start
        if (whiteFlashImage != null)
        {
            Color c = whiteFlashImage.color;
            c.a = 0f;
            whiteFlashImage.color = c;
            whiteFlashImage.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger once, and only by the Player
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("[ElevatorSequence] Player entered trigger. Starting sequence...");
            StartCoroutine(SequenceMasterRoutine());
        }
    }

    private void Update()
    {
        // Handle shaking independently in Update so it doesn't block the coroutine
        if (isShaking && shakeTarget != null)
        {
            float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeIntensity;
            float shakeZ = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeIntensity;
            float shakeY = (Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) - 0.5f) * (shakeIntensity * 0.5f);

            shakeTarget.localPosition = originalLocalPos + new Vector3(shakeX, shakeY, shakeZ);
        }
    }

    /// <summary>
    /// The master timeline that controls the sequence of events.
    /// </summary>
    private IEnumerator SequenceMasterRoutine()
    {
        // 1. Initial Delay
        yield return new WaitForSeconds(initialDelay);
        Debug.Log("[ElevatorSequence] Delay finished. Starting shake.");

        // 2. Start Shaking
        if (shakeTarget != null)
        {
            originalLocalPos = shakeTarget.localPosition;
            isShaking = true;
        }

        // Wait while shaking before turning on the flicker
        yield return new WaitForSeconds(shakeDurationBeforeFlicker);
        Debug.Log("[ElevatorSequence] Starting light and material flicker.");

        // 3. Start Flickering (runs concurrently)
        isFlickering = true;
        StartCoroutine(FlickerRoutine());

        // Wait while flickering before the final flash
        yield return new WaitForSeconds(flickerDurationBeforeFlash);
        Debug.Log("[ElevatorSequence] Starting white flash climax.");

        // 4. White Flash
        if (whiteFlashImage != null)
        {
            whiteFlashImage.gameObject.SetActive(true);
            float t = 0f;
            Color flashColor = Color.white; // Or whatever color you set in the Inspector

            while (t < flashDuration)
            {
                t += Time.deltaTime;
                flashColor.a = Mathf.Clamp01(t / flashDuration);
                whiteFlashImage.color = flashColor;
                yield return null;
            }
        }

        // Ensure shake stops exactly when the flash peaks (optional cleanup)
        isShaking = false;
        isFlickering = false;

        Debug.Log($"[ElevatorSequence] Transitioning to {nextSceneName}...");

        // 5. Transition to the next scene
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Handles the randomized flickering of Lights and Emissive Materials.
    /// </summary>
    private IEnumerator FlickerRoutine()
    {
        // Pre-enable emission keyword on all target materials
        foreach (MeshRenderer renderer in emissiveRenderers)
        {
            if (renderer != null)
            {
                renderer.material.EnableKeyword("_EMISSION");
            }
        }

        while (isFlickering)
        {
            float randomIntensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);

            // Flicker actual Lights
            foreach (Light l in targetLights)
            {
                if (l != null) l.intensity = randomIntensity;
            }

            // Flicker Emissive Materials
            foreach (MeshRenderer renderer in emissiveRenderers)
            {
                if (renderer != null)
                {
                    // Multiply the base emission color by the random intensity
                    renderer.material.SetColor("_EmissionColor", baseEmissionColor * randomIntensity);
                }
            }

            // Wait a tiny random fraction of a second before the next flicker change
            yield return new WaitForSeconds(Random.Range(flickerSpeed, flickerSpeed + 0.1f));
        }
    }
}