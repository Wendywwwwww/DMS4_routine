using UnityEngine;

/// <summary>
/// Triggers and plays all AudioSource components found on this object 
/// when any object enters the trigger collider.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TriggerAudioOnCollision : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("If checked, the sounds will only play the first time something enters.")]
    public bool playOnlyOnce = true;

    [Header("Options")]
    [Tooltip("If checked, will also search child objects for AudioSources.")]
    public bool includeChildren = true;

    private AudioSource[] audioSources;
    private bool hasTriggered = false;

    void Start()
    {
        // 1. Ensure collider is strictly a trigger
        BoxCollider boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        // 2. Gather all AudioSource components automatically
        if (includeChildren)
        {
            audioSources = GetComponentsInChildren<AudioSource>();
        }
        else
        {
            audioSources = GetComponents<AudioSource>();
        }

        // Configure them for 3D spatial playback and ensure they don't loop/play on awake
        foreach (var source in audioSources)
        {
            source.spatialBlend = 1.0f;
            source.playOnAwake = false;
        }

        Debug.Log($"[TriggerAudioOnCollision] Initialized on '{gameObject.name}' with {audioSources.Length} AudioSource(s) found.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we already played and only wanted it once
        if (playOnlyOnce && hasTriggered) return;

        if (audioSources != null && audioSources.Length > 0)
        {
            foreach (var source in audioSources)
            {
                if (source != null && source.clip != null)
                {
                    source.Play();
                    Debug.Log($"--> Playing audio source clip: '{source.clip.name}' on object '{source.gameObject.name}'.");
                }
            }
            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning($"--> WARNING: Trigger entered by '{other.name}', but no AudioSource components were found on '{gameObject.name}'!");
        }
    }
}