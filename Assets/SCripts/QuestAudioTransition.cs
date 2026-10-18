using System.Collections;
using UnityEngine;

/// <summary>
/// Listens to the QuestManager and transitions audio when a specific quest is completed.
/// Plays one audio instantly, and fades in another audio.
/// </summary>
public class QuestAudioTransition : MonoBehaviour
{
    [Header("Quest Reference")]
    [Tooltip("The specific Quest Data that triggers this audio transition.")]
    public QuestData targetQuestData;

    [Header("Audio to STOP (Optional)")]
    [Tooltip("The old AudioSource that should STOP immediately (e.g., old BGM). Leave empty if not needed.")]
    public AudioSource audioToStop;

    [Header("Audios to PLAY")]
    [Tooltip("This audio plays INSTANTLY at its normal volume (e.g., Quest Complete SFX).")]
    public AudioSource instantAudio;
    
    [Tooltip("This audio will FADE IN smoothly (e.g., New Background Music).")]
    public AudioSource fadingAudio;

    [Header("Fade Settings")]
    [Tooltip("Duration in seconds for the fading audio to fade in.")]
    public float fadeInDuration = 2.0f;

    private float targetVolume;

    void Start()
    {
        // Remember the target volume of the fading audio
        if (fadingAudio != null)
        {
            targetVolume = fadingAudio.volume;
        }

        // Subscribe to the global quest completion event
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(QuestData completedQuest)
    {
        if (targetQuestData == null || completedQuest == null) return;

        if (completedQuest.questID == targetQuestData.questID)
        {
            // 1. Stop the old audio
            if (audioToStop != null && audioToStop.isPlaying)
            {
                audioToStop.Stop();
            }

            // 2. Play the instant audio (no fade)
            if (instantAudio != null)
            {
                instantAudio.Play();
                Debug.Log($"[QuestAudio] Played instant audio for: {targetQuestData.questID}");
            }

            // 3. Play and fade in the fading audio
            if (fadingAudio != null)
            {
                fadingAudio.volume = 0f;
                fadingAudio.Play();
                StartCoroutine(FadeInAudio(fadingAudio, targetVolume, fadeInDuration));
                Debug.Log($"[QuestAudio] Fading in new audio for: {targetQuestData.questID}");
            }
        }
    }

    private IEnumerator FadeInAudio(AudioSource audioSource, float targetVol, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVol, currentTime / duration);
            yield return null; 
        }
        audioSource.volume = targetVol;
    }
}