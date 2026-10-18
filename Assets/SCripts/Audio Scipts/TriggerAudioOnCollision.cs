using UnityEngine;

/// <summary>
/// Plays a 3D audio clip via an AudioSource when a target object (like the player) 
/// enters this trigger collider. Can play once or repeat.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class TriggerAudioOnCollision : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The sound effect to play when triggered.")]
    public AudioClip soundToPlay;

    [Tooltip("If checked, the sound will only play the first time the player enters.")]
    public bool playOnlyOnce = true;

    [Header("Trigger Filter")]
    [Tooltip("The tag required to trigger this sound (e.g., 'Player'). Leave blank to trigger on anything.")]
    public string requiredTag = "Player";

    private AudioSource audioSource;
    private bool hasTriggered = false;

    void Start()
    {
        // 1. Ensure collider is strictly a trigger
        BoxCollider boxCol = GetComponent<BoxCollider>();
        boxCol.isTrigger = true;

        // 2. Cache the AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Ensure the AudioSource is configured for 3D spatial sound so it comes from this location
        audioSource.spatialBlend = 1.0f;
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we already played and only wanted it once
        if (playOnlyOnce && hasTriggered) return;

        // Check if the object matching our required tag entered (or if tag filter is empty)
        bool matchesTag = string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag) || other.name.Contains(requiredTag);

        if (matchesTag && soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
            hasTriggered = true;
            Debug.Log($"[TriggerAudioOnCollision] Played sound '{soundToPlay.name}' from object '{gameObject.name}'.");
        }
    }
}