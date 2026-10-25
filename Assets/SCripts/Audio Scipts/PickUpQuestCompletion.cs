using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PickupQuestCompletion : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("Drag the 'Pick Up Phone' QuestData ScriptableObject here.")]
    public QuestData phoneQuest;

    [Header("Scene 4 Audio Tracks (3+ Slots)")]
    [Tooltip("Add your Scene 4 audio stems/tracks here.")]
    public List<AudioSource> scene4AudioTracks = new List<AudioSource>();

    [Header("Audio Adjustments")]
    [Range(0f, 1f)] public float targetVolume = 1.0f;
    public float fadeInDuration = 2.0f;

    [Tooltip("Optional: Previous ambient audio source to fade out instantly when Scene 4 starts.")]
    public AudioSource previousSceneAmbiance;

    [Header("Stop External Sound Settings")]
    [Tooltip("Check this if you want to stop a separate, independent sound source after a delay when the phone quest triggers.")]
    public bool enableExternalSoundStop = false;
    [Tooltip("The independent AudioSource you want to stop (e.g., ambient background hum, machine noise, etc.).")]
    public AudioSource externalSoundToStop;
    [Tooltip("How many seconds after the phone quest triggers before this external sound begins to fade out.")]
    public float delayBeforeStoppingExternal = 5f;
    [Tooltip("How long the fade-out takes for this external sound.")]
    public float externalFadeOutDuration = 2.0f;

    [Header("Scene 4 Auto-Stop Settings (Optional)")]
    [Tooltip("Check this if you want the Scene 4 sounds themselves to automatically stop after a set amount of time.")]
    public bool enableScene4AutoStop = false;
    public float scene4PlayDurationSeconds = 30f;
    public float scene4AutoStopFadeOutDuration = 2.0f;

    private bool hasTriggered = false;

    private void Start()
    {
        // Subscribe to the QuestManager event
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;

            // Safety check: In case the quest was already completed before this scene/script loaded
            if (phoneQuest != null && QuestManager.Instance.IsQuestCompleted(phoneQuest))
            {
                TriggerFinalScene4Audio();
            }
        }
        else
        {
            Debug.LogWarning("[PickupQuestCompletion] QuestManager instance not found in the scene!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed to prevent memory leaks
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(QuestData completedQuest)
    {
        if (hasTriggered) return;

        // Verify if the completed quest matches our phone quest reference or ID
        if (completedQuest != null && phoneQuest != null && completedQuest.questID == phoneQuest.questID)
        {
            TriggerFinalScene4Audio();
        }
    }

    private void TriggerFinalScene4Audio()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        Debug.Log("[Quest System] Phone Quest Finished via QuestManager -> Playing Scene 4 Audio Sequence.");

        // Fade out previous ambient audio if assigned
        if (previousSceneAmbiance != null && previousSceneAmbiance.isPlaying)
        {
            StartCoroutine(FadeOutAudio(previousSceneAmbiance, fadeInDuration));
        }

        // Play all assigned Scene 4 audio tracks/stems with a smooth fade-in
        foreach (AudioSource source in scene4AudioTracks)
        {
            if (source != null)
            {
                source.volume = 0f;
                source.Play();
                StartCoroutine(FadeInAudio(source, targetVolume, fadeInDuration));
            }
        }

        // Handle independent external sound stop timer
        if (enableExternalSoundStop && externalSoundToStop != null)
        {
            StartCoroutine(ExternalSoundStopRoutine());
        }

        // Handle Scene 4 auto-stop timer (if enabled)
        if (enableScene4AutoStop)
        {
            StartCoroutine(Scene4AutoStopRoutine());
        }
    }

    private IEnumerator FadeInAudio(AudioSource source, float targetVol, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVol, currentTime / duration);
            yield return null;
        }
        source.volume = targetVol;
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, currentTime / duration);
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
    }

    private IEnumerator ExternalSoundStopRoutine()
    {
        // Wait for the specified delay after the quest triggers
        yield return new WaitForSeconds(delayBeforeStoppingExternal);

        if (externalSoundToStop != null && externalSoundToStop.isPlaying)
        {
            Debug.Log($"[PickupQuestCompletion] Stopping external sound '{externalSoundToStop.name}' after delay.");
            StartCoroutine(FadeOutAndStopAudio(externalSoundToStop, externalFadeOutDuration));
        }
    }

    private IEnumerator Scene4AutoStopRoutine()
    {
        yield return new WaitForSeconds(scene4PlayDurationSeconds);

        Debug.Log("[PickupQuestCompletion] Auto-stop timer reached for Scene 4 audio tracks.");
        foreach (AudioSource source in scene4AudioTracks)
        {
            if (source != null && source.isPlaying)
            {
                StartCoroutine(FadeOutAndStopAudio(source, scene4AutoStopFadeOutDuration));
            }
        }
    }

    private IEnumerator FadeOutAndStopAudio(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, currentTime / duration);
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
    }
}