using System.Collections;
using UnityEngine;

/// <summary>
/// Plays an elevator open sound, waits for an exposed delay, and then starts a looping ambient sound.
/// Matches both TriggerMove() and Interact() so it integrates cleanly with your click-to-move scripts.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ElevatorDoorSoundTrigger : MonoBehaviour
{
    [Header("Sound Clips")]
    [Tooltip("Sound played immediately when the sequence starts (e.g., doors opening).")]
    public AudioClip openSound;

    [Tooltip("Ambient sound played after the delay, which will loop continuously.")]
    public AudioClip ambientLoopSound;

    [Header("Timing Settings")]
    [Tooltip("How many seconds to wait after the open sound before starting the ambient loop.")]
    public float delayBeforeAmbient = 2.0f;

    [Header("Interaction Settings")]
    [Tooltip("If checked, walking into the Box Collider (set as trigger) will start the sequence.")]
    public bool triggerOnEnter = false;

    [Tooltip("Assign your specific Trigger Collider component here if your object has multiple colliders.")]
    public Collider targetCollider;

    private AudioSource audioSource;
    private Coroutine activeSequence;

    void Awake()
    {
        // Configure the AudioSource for 3D spatial sound
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // If a specific collider wasn't dragged in, grab the component
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && other.CompareTag("Player"))
        {
            Debug.Log($"[ElevatorDoorSoundTrigger] Trigger entered by '{other.name}'. Starting sound sequence.");
            PlaySequence();
        }
    }

    /// <summary>
    /// Matches external interaction systems or standard UI clicks.
    /// </summary>
    public void Interact()
    {
        PlaySequence();
    }

    /// <summary>
    /// Matches your ClickToMoveGlobalZ script so they can both fire simultaneously from the same trigger.
    /// </summary>
    public void TriggerMove()
    {
        PlaySequence();
    }

    /// <summary>
    /// Core method that executes the open sound, delay, and ambient loop sequence.
    /// </summary>
    public void PlaySequence()
    {
        if (activeSequence != null)
        {
            StopCoroutine(activeSequence);
        }
        activeSequence = StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        // Stop any existing loops before restarting
        audioSource.Stop();
        audioSource.loop = false;

        // 1. Play Open Sound
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        else
        {
            Debug.LogWarning($"[ElevatorDoorSoundTrigger] Open sound slot is empty on '{gameObject.name}'!");
        }

        // 2. Wait for the exposed delay before starting the ambient loop
        if (delayBeforeAmbient > 0f)
        {
            yield return new WaitForSeconds(delayBeforeAmbient);
        }

        // 3. Start Playing the Ambient Loop Sound
        if (ambientLoopSound != null)
        {
            audioSource.clip = ambientLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"[ElevatorDoorSoundTrigger] Ambient loop sound slot is empty on '{gameObject.name}'!");
        }

        activeSequence = null;
    }
}