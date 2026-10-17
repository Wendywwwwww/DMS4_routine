using System.Collections;
using UnityEngine;

/// <summary>
/// This script manages ambient tick-tock audio sources placed around the scene. 
/// It integrates directly with the project's existing QuestManager and QuestData architecture 
/// (following the same event subscription pattern as QuestAudioPlayer and EnableAfterQuest).
/// 
/// How to set it up in the Inspector:
/// 1. Create an empty GameObject in your scene and attach this script.
/// 2. Drag your target QuestData asset (e.g., "quest data object thingy5") into the "Target Quest" slot.
/// 3. Set the size of the "Tick Tock Audio Sources" array and drag in your tick-tock 
///    objects / AudioSources located around different parts of the scene.
/// 4. Adjust the target volume and fade duration for how quickly you want the audio to crank up 
///    when the quest completes.
/// </summary>
public class MasterAudioController : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("The QuestData asset (e.g., your quest data object thingy5) that triggers the volume increase when completed.")]
    public QuestData targetQuest;

    [Header("Tick Tock Audio Sources")]
    [Tooltip("Drag the AudioSource components of your tick-tock objects located around different places in the scene.")]
    public AudioSource[] tickTockSources;

    [Header("Volume Transition Settings")]
    [Tooltip("The high volume level to crank up to when the quest is completed.")]
    public float targetVolume = 1.0f;

    [Tooltip("How long the volume fade/transition takes in seconds.")]
    public float fadeDuration = 2.0f;

    private bool isCrankUpTriggered = false;

    void Start()
    {
        // 1. Subscribe to the QuestManager completion event (matching project standards)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        }

        // 2. Check if the quest was already completed before this scene/object loaded
        if (targetQuest != null && QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted(targetQuest))
        {
            CrankUpVolumeInstant();
        }
    }

    void OnDestroy()
    {
        // 3. Unsubscribe from the event to prevent memory leaks when destroying the object
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    /// <summary>
    /// Event handler triggered whenever any quest is completed in the game.
    /// </summary>
    private void HandleQuestCompleted(QuestData completedQuest)
    {
        if (isCrankUpTriggered) return;

        // Check if the completed quest matches our target quest
        if (completedQuest == targetQuest && targetQuest != null)
        {
            StartCoroutine(CrankUpVolumeRoutine());
            isCrankUpTriggered = true;
            Debug.Log($"[MasterAudioController] Target quest '{targetQuest.questID}' completed. Cranking up tick-tock volumes.");
        }
    }

    /// <summary>
    /// Smoothly transitions the volume of all tick-tock audio sources up to the target volume level.
    /// </summary>
    private IEnumerator CrankUpVolumeRoutine()
    {
        if (tickTockSources == null || tickTockSources.Length == 0) yield break;

        // Store the starting volumes for each source
        float[] startVolumes = new float[tickTockSources.Length];
        for (int i = 0; i < tickTockSources.Length; i++)
        {
            if (tickTockSources[i] != null)
            {
                startVolumes[i] = tickTockSources[i].volume;
                if (!tickTockSources[i].isPlaying)
                {
                    tickTockSources[i].Play();
                }
            }
        }

        float elapsedTime = 0f;

        // Gradually fade/crank up the volume over the specified duration
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            for (int i = 0; i < tickTockSources.Length; i++)
            {
                if (tickTockSources[i] != null)
                {
                    tickTockSources[i].volume = Mathf.Lerp(startVolumes[i], targetVolume, t);
                }
            }
            yield return null;
        }

        // Ensure final volumes are precisely set to the target volume
        for (int i = 0; i < tickTockSources.Length; i++)
        {
            if (tickTockSources[i] != null)
            {
                tickTockSources[i].volume = targetVolume;
            }
        }
    }

    /// <summary>
    /// Instantly sets volumes to the target level if the quest was finished prior to loading.
    /// </summary>
    private void CrankUpVolumeInstant()
    {
        if (tickTockSources == null) return;

        isCrankUpTriggered = true;
        foreach (var source in tickTockSources)
        {
            if (source != null)
            {
                source.volume = targetVolume;
                if (!source.isPlaying)
                {
                    source.Play();
                }
            }
        }
    }
}