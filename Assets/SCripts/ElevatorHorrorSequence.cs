using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections;
using Unity.Cinemachine; 

/// <summary>
/// A standalone trigger script that executes a timed horror sequence using Cinemachine Noise:
/// Delay -> Cinemachine Camera Shake -> Material Swap & Light Flicker -> White Screen Flash -> Load Scene.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ElevatorHorrorSequence : MonoBehaviour
{
    [Header("Sequence Timings")]
    public float initialDelay = 2f;
    public float shakeDurationBeforeFlicker = 3f;
    public float flickerDurationBeforeFlash = 4f;
    public float flashDuration = 1.5f;

    [Header("Cinemachine Shake Settings")]
    [Tooltip("Drag the Virtual Camera with the Cinemachine Camera component here.")]
    public CinemachineCamera vCam; 
    [Tooltip("Shake Amplitude - values in Cinemachine are typically between 1 and 5.")]
    public float shakeAmplitude = 3f;
    [Tooltip("Shake Frequency - determines the speed of the vibration.")]
    public float shakeFrequency = 5f;

    [Header("Light Component Flicker Settings")]
    [Tooltip("Real lights that will flicker.")]
    public Light[] targetLights;
    public float minFlickerIntensity = 0f;
    public float maxFlickerIntensity = 2.5f;
    [Tooltip("How fast the flicker changes.")]
    public float flickerSpeed = 0.05f;

    [Header("Material Swap Flicker Settings")]
    [Tooltip("Meshes that will swap between Lit and Unlit materials.")]
    public MeshRenderer[] targetRenderers;
    [Tooltip("The material used when the light flickers OFF.")]
    public Material unlitMaterial;
    [Tooltip("The material used when the light flickers ON.")]
    public Material litMaterial;

    [Header("Scene Transition")]
    public Image whiteFlashImage;
    public string nextSceneName = "Scene4";

    private bool hasTriggered = false;
    private bool isFlickering = false;
    
    // Caches the Cinemachine noise component
    private CinemachineBasicMultiChannelPerlin cmNoise;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (whiteFlashImage != null)
        {
            Color c = whiteFlashImage.color;
            c.a = 0f;
            whiteFlashImage.color = c;
            whiteFlashImage.gameObject.SetActive(false);
        }

        // Get and initialize the Cinemachine noise component
        if (vCam != null)
        {
            cmNoise = vCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (cmNoise != null)
            {
                // Ensure there is no camera shake when the game starts
                cmNoise.AmplitudeGain = 0f;
                cmNoise.FrequencyGain = 0f;
            }
            else
            {
                Debug.LogWarning("[ElevatorSequence] Your Cinemachine Camera is missing the Noise component!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("[ElevatorSequence] Player entered trigger. Sequence initiated.");
            StartCoroutine(SequenceMasterRoutine());
        }
    }

    private IEnumerator SequenceMasterRoutine()
    {
        yield return new WaitForSeconds(initialDelay);
        Debug.Log("[ElevatorSequence] Starting Cinemachine Camera Shake.");

        // 1. Start Cinemachine Camera Shake
        if (cmNoise != null)
        {
            cmNoise.AmplitudeGain = shakeAmplitude;
            cmNoise.FrequencyGain = shakeFrequency;
        }

        yield return new WaitForSeconds(shakeDurationBeforeFlicker);
        Debug.Log("[ElevatorSequence] Starting lights & material swap flickering.");

        // 2. Start Light & Material Flickering
        isFlickering = true;
        StartCoroutine(FlickerRoutine());

        yield return new WaitForSeconds(flickerDurationBeforeFlash);
        Debug.Log("[ElevatorSequence] Reaching peak white flash.");

        // 3. Start White Screen Flash
        if (whiteFlashImage != null)
        {
            whiteFlashImage.gameObject.SetActive(true);
            float t = 0f;
            Color flashColor = Color.white; 

            while (t < flashDuration)
            {
                t += Time.deltaTime;
                flashColor.a = Mathf.Clamp01(t / flashDuration);
                whiteFlashImage.color = flashColor;
                yield return null;
            }
        }

        isFlickering = false;

        // 4. Stop Camera Shake (Cleanup)
        if (cmNoise != null)
        {
            cmNoise.AmplitudeGain = 0f;
            cmNoise.FrequencyGain = 0f;
        }

        Debug.Log($"[ElevatorSequence] Launching transition to {nextSceneName}...");
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FlickerRoutine()
    {
        while (isFlickering)
        {
            // Flicker real lights
            float randomIntensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);
            foreach (Light l in targetLights)
            {
                if (l != null) l.intensity = randomIntensity;
            }

            // Randomly decide if the material should be ON or OFF this frame
            bool isLightOn = Random.value > 0.5f;
            Material currentMat = isLightOn ? litMaterial : unlitMaterial;

            // Apply the chosen material to all target renderers
            foreach (MeshRenderer renderer in targetRenderers)
            {
                if (renderer != null && currentMat != null)
                {
                    renderer.material = currentMat;
                }
            }

            // High frequency delay
            yield return new WaitForSeconds(Random.Range(flickerSpeed, flickerSpeed + 0.08f));
        }
    }
}